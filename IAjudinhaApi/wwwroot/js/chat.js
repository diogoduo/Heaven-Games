import { api } from './api.js';
import { ehCliente, ehFuncionario, aoMudarSessao } from './sessao.js';
import { $, mostrar, registrarAcoes, escapeHtml } from './ui.js';
import { criarChamado } from './chamados.js';
import { pedirLogin } from './auth.js';

const SAUDACAO = 'Olá! Eu sou a IAjudinha 🎮 Posso te ajudar a encontrar jogos, tirar dúvidas sobre a loja ou abrir um chamado com a nossa equipe.';
const SUGESTOES = ['Quais jogos de terror vocês têm?', 'Qual o jogo mais barato?', 'Abrir chamado'];
const GATILHO_CHAMADO = /\b(abrir|abre|criar)\s+(um\s+)?chamado\b|\bfalar com (um |uma )?(atendente|humano|pessoa)\b/i;
const CANCELAR = /^(cancelar|cancela|sair|parar)$/i;

// Conversa enviada para a IA (apenas perguntas e respostas, sem o fluxo de chamado).
const historico = [];
// Etapas da abertura de chamado pelo chat: null -> 'assunto' -> 'descricao'
const fluxo = { etapa: null, assunto: '' };
let aguardando = false;

export function iniciarChat() {
    $('#chat-form').addEventListener('submit', (evento) => {
        evento.preventDefault();
        enviar($('#chat-input').value);
    });

    registrarAcoes({
        'abrir-chat': () => abrirChat(),
        'fechar-chat': () => fecharChat(),
        'sugestao-chat': (botao) => enviar(botao.dataset.texto),
        'perguntar-jogo': (botao) => {
            abrirChat();
            enviar(`Me fale sobre o jogo ${botao.dataset.titulo}.`);
        },
        'chat-entrar': () => pedirLogin('Cliente', () => {
            abrirChat();
            iniciarFluxoChamado();
        }),
    });

    aoMudarSessao(() => {
        if (!ehCliente()) fluxo.etapa = null;
    });

    adicionarMensagem('ia', SAUDACAO, { sugestoes: SUGESTOES });
}

export function abrirChat() {
    mostrar($('#chat-janela'), true);
    $('#chat-botao').setAttribute('aria-expanded', 'true');
    $('#chat-input').focus();
}

function fecharChat() {
    const janela = $('#chat-janela');
    if (janela.classList.contains('hidden')) return;
    mostrar(janela, false);
    $('#chat-botao').setAttribute('aria-expanded', 'false');
    $('#chat-botao').focus();
}

async function enviar(textoDigitado) {
    const texto = textoDigitado.trim();
    if (!texto || aguardando) return;

    $('#chat-input').value = '';
    adicionarMensagem('usuario', texto);

    if (await tratarFluxoChamado(texto)) return;
    await perguntarIa(texto);
}

// ---------------------------------------------------------------
// Abertura de chamado pelo chat
// ---------------------------------------------------------------
async function tratarFluxoChamado(texto) {
    if (fluxo.etapa && CANCELAR.test(texto)) {
        fluxo.etapa = null;
        adicionarMensagem('ia', 'Tudo bem, cancelei a abertura do chamado. Posso ajudar em mais alguma coisa?');
        return true;
    }

    // A etapa atual é verificada antes dos gatilhos: assim a descrição pode conter
    // palavras como "chamado" ou "problema" sem reiniciar o fluxo.
    if (fluxo.etapa === 'assunto') {
        if (texto.length < 5) {
            adicionarMensagem('ia', 'Pode detalhar um pouco mais o assunto? (mínimo de 5 caracteres)');
            return true;
        }
        fluxo.assunto = texto.slice(0, 200);
        fluxo.etapa = 'descricao';
        adicionarMensagem('ia', 'Entendi! Agora descreva o problema com o máximo de detalhes que puder.');
        return true;
    }

    if (fluxo.etapa === 'descricao') {
        if (texto.length < 10) {
            adicionarMensagem('ia', 'Conte um pouco mais para a equipe entender o problema (mínimo de 10 caracteres).');
            return true;
        }
        fluxo.etapa = null;
        await comIndicadorDigitando(async () => {
            try {
                const chamado = await criarChamado(fluxo.assunto, texto.slice(0, 2000));
                adicionarMensagem('ia',
                    `Pronto! Abri o chamado **#${chamado.idChamado}**. Nossa equipe vai responder em breve e você acompanha tudo em "Meus chamados".`,
                    { botoes: [{ rotulo: 'Ver chamado', acao: 'abrir-chamado', dados: { id: chamado.idChamado } }] });
            } catch (erro) {
                adicionarMensagem('ia', `Não consegui abrir o chamado: ${erro.message}`);
            }
        });
        return true;
    }

    if (GATILHO_CHAMADO.test(texto)) {
        iniciarFluxoChamado();
        return true;
    }

    return false;
}

function iniciarFluxoChamado() {
    if (ehFuncionario()) {
        adicionarMensagem('ia', 'Você está conectado(a) como equipe. Os chamados dos clientes ficam no painel de atendimento.',
            { botoes: [{ rotulo: 'Ir para o painel', href: '#/painel' }] });
        return;
    }

    if (!ehCliente()) {
        adicionarMensagem('ia', 'Para abrir um chamado, preciso que você entre na sua conta. Depois do login eu continuo daqui!',
            { botoes: [{ rotulo: 'Entrar ou criar conta', acao: 'chat-entrar' }] });
        return;
    }

    fluxo.etapa = 'assunto';
    adicionarMensagem('ia', 'Vamos abrir um chamado! Qual é o assunto? (digite "cancelar" a qualquer momento)');
}

// ---------------------------------------------------------------
// Perguntas para a IA (via API, a chave do Gemini fica no servidor)
// ---------------------------------------------------------------
async function perguntarIa(texto) {
    historico.push({ papel: 'usuario', texto: texto.slice(0, 1000) });

    await comIndicadorDigitando(async () => {
        try {
            const { resposta, fontes } = await api('/chat', { metodo: 'POST', corpo: { mensagens: historico.slice(-20) } });
            historico.push({ papel: 'assistente', texto: resposta.slice(0, 1000) });
            adicionarMensagem('ia', resposta, { fontes });
        } catch (erro) {
            historico.pop(); // descarta a pergunta sem resposta para manter a alternância usuário/assistente
            adicionarMensagem('ia', erro.message, { sugestoes: erro.status === 503 ? ['Abrir chamado'] : [] });
        }
    });
}

async function comIndicadorDigitando(tarefa) {
    aguardando = true;
    $('#chat-input').disabled = true;
    $('#chat-enviar').disabled = true;

    const indicador = document.createElement('div');
    indicador.className = 'flex justify-start';
    indicador.innerHTML = '<div class="rounded-xl bg-white/10 px-3 py-2 text-sm text-gray-300">IAjudinha está digitando<span class="animate-pulse">...</span></div>';
    $('#chat-mensagens').append(indicador);
    rolarParaFim();

    try {
        await tarefa();
    } finally {
        indicador.remove();
        aguardando = false;
        $('#chat-input').disabled = false;
        $('#chat-enviar').disabled = false;
        $('#chat-input').focus();
    }
}

// ---------------------------------------------------------------
// Renderização das mensagens
// ---------------------------------------------------------------
function adicionarMensagem(autor, texto, { sugestoes = [], botoes = [], fontes = [] } = {}) {
    const doUsuario = autor === 'usuario';
    const linha = document.createElement('div');
    linha.className = `flex animate-surgir ${doUsuario ? 'justify-end' : 'justify-start'}`;

    const bolha = document.createElement('div');
    bolha.className = `max-w-[85%] break-words rounded-xl px-3 py-2 text-sm shadow ${doUsuario ? 'bg-primary text-white' : 'bg-white/10 text-gray-100'}`;
    bolha.innerHTML = doUsuario ? escapeHtml(texto) : markdownSimples(texto);

    const links = fontes.filter((f) => /^https?:\/\//.test(f.url));
    if (links.length) {
        bolha.insertAdjacentHTML('beforeend', `
            <p class="mt-2 border-t border-white/10 pt-1 text-xs text-gray-400">Fontes:
                ${links.map((f, i) => `<a href="${escapeHtml(f.url)}" target="_blank" rel="noopener noreferrer" class="ml-1 underline hover:text-white">${i + 1}. ${escapeHtml(f.titulo)}</a>`).join('')}
            </p>`);
    }

    if (botoes.length) {
        bolha.insertAdjacentHTML('beforeend', `<div class="mt-2 flex flex-wrap gap-2">${botoes.map(htmlBotao).join('')}</div>`);
    }

    linha.append(bolha);
    $('#chat-mensagens').append(linha);

    if (sugestoes.length) {
        const chips = document.createElement('div');
        chips.className = 'flex flex-wrap gap-2';
        chips.innerHTML = sugestoes.map((s) => `
            <button type="button" class="rounded-full border border-secondary/40 px-3 py-1 text-xs text-secondary transition hover:bg-secondary hover:text-primary"
                data-acao="sugestao-chat" data-texto="${escapeHtml(s)}">${escapeHtml(s)}</button>`).join('');
        $('#chat-mensagens').append(chips);
    }

    rolarParaFim();
}

function htmlBotao({ rotulo, acao, href, dados = {} }) {
    const classes = 'rounded-lg bg-secondary px-3 py-1 text-xs font-semibold text-primary transition hover:bg-white';
    if (href) return `<a href="${escapeHtml(href)}" class="${classes}">${escapeHtml(rotulo)}</a>`;
    const atributos = Object.entries(dados).map(([chave, valor]) => `data-${chave}="${escapeHtml(valor)}"`).join(' ');
    return `<button type="button" class="${classes}" data-acao="${acao}" ${atributos}>${escapeHtml(rotulo)}</button>`;
}

// Converte o básico de Markdown das respostas da IA. O texto é escapado antes, então nenhuma tag vinda da IA é interpretada.
function markdownSimples(texto) {
    return escapeHtml(texto)
        .replace(/^\s*[-*]\s+/gm, '• ')
        .replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>')
        .replace(/(^|[^*])\*([^*\n]+)\*/g, '$1<em>$2</em>')
        .replace(/\n/g, '<br>');
}

function rolarParaFim() {
    const mensagens = $('#chat-mensagens');
    mensagens.scrollTop = mensagens.scrollHeight;
}
