using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using IAjudinhaApi.Data;
using IAjudinhaApi.Seguranca;
using IAjudinhaApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// =========================================
// Banco de dados (SQL Server)
// =========================================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Configure a connection string 'DefaultConnection' (veja o README).");

builder.Services.AddDbContext<IAjudinhaDbContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));

// =========================================
// Autenticação JWT
// =========================================
// Em desenvolvimento, sem Jwt:Key configurada, usa uma chave temporária (os logins expiram ao reiniciar a API).
var chaveTemporaria = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));

builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.Secao))
    .PostConfigure(options =>
    {
        if (builder.Environment.IsDevelopment() && string.IsNullOrWhiteSpace(options.Key))
            options.Key = chaveTemporaria;
    })
    .Validate(options => Encoding.UTF8.GetByteCount(options.Key) >= 32, "Jwt:Key precisa ter pelo menos 32 caracteres.")
    .ValidateOnStart();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
    .Configure<IOptions<JwtOptions>>((options, jwtOptions) =>
    {
        var jwt = jwtOptions.Value;
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            NameClaimType = JwtRegisteredClaimNames.Name,
            RoleClaimType = "role"
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddSingleton<TokenService>();

// =========================================
// Limite de requisições por IP
// =========================================
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = (context, cancellationToken) => new ValueTask(
        context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status429TooManyRequests,
            Detail = "Muitas tentativas em pouco tempo. Aguarde um minuto e tente novamente."
        }, cancellationToken));

    options.AddPolicy(PoliticasRateLimit.Autenticacao, http => LimitePorIp(http, 10));
    options.AddPolicy(PoliticasRateLimit.Chat, http => LimitePorIp(http, 15));
});

// Atrás de um proxy reverso (ex.: hospedagem em nuvem), usa o IP real do visitante.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// =========================================
// IAjudinha (Gemini)
// =========================================
builder.Services.Configure<GeminiOptions>(builder.Configuration.GetSection(GeminiOptions.Secao));
builder.Services.AddHttpClient<GeminiChatService>(client =>
{
    client.BaseAddress = new Uri("https://generativelanguage.googleapis.com/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// =========================================
// Serviços da API
// =========================================
builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

var app = builder.Build();

// Cria/atualiza o banco e insere os dados de demonstração.
await using (var scope = app.Services.CreateAsyncScope())
{
    var context = scope.ServiceProvider.GetRequiredService<IAjudinhaDbContext>();
    await context.Database.MigrateAsync();

    if (app.Configuration.GetValue("DadosDemo:Habilitar", true))
        await DbSeeder.SeedAsync(context);
}

// =========================================
// Pipeline
// =========================================
app.UseForwardedHeaders();
app.UseExceptionHandler();
app.UseStatusCodePages();

// O front-end (wwwroot) é servido pela própria API, então não há necessidade de CORS.
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();

app.MapOpenApi();
app.MapScalarApiReference(options => options.WithTitle("IAjudinha API"));
app.MapControllers();

app.Run();

static RateLimitPartition<string> LimitePorIp(HttpContext http, int requisicoesPorMinuto) =>
    RateLimitPartition.GetFixedWindowLimiter(
        http.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
        _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = requisicoesPorMinuto,
            Window = TimeSpan.FromMinutes(1)
        });
