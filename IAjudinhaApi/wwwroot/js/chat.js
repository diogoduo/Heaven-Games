import { api } from './api.js';
import { ehCliente, ehFuncionario, aoMudarSessao } from './sessao.js';
import { $, mostrar, registrarAcoes, escapeHtml, icone } from './ui.js';
import { criarChamado } from './chamados.js';
import { pedirLogin } from './auth.js';

const SAUDACAO = 'Olá! Eu sou a **IAjudinha** 👋 Posso te ajudar a encontrar jogos, tirar dúvidas sobre a loja ou abrir um chamado com a nossa equipe.';
const SUGESTOES = ['Quais jogos de terror vocês têm?', 'Qual o jogo mais barato?', 'Abrir chamado'];
const GATILHO_CHAMADO = /\b(abrir|abre|criar)\s+(um\s+)?chamado\b|\bfalar com (um |uma )?(atendente|humano|pessoa)\b/i;
const CANCELAR = /^(cancelar|cancela|sair|parar)$/i;
const ALTURA_MAXIMA_INPUT = 128;

// Conversa enviada para a IA (apenas perguntas e respostas, sem o fluxo de chamado).
const historico = [];
// Etapas da abertura de chamado pelo chat: null -> 'assunto' -> 'descricao'
const fluxo = { etapa: null, assunto: '' };
let aguardando = false;

export function iniciarChat() {
    const input = $('#chat-input');

    $('#chat-form').addEventListener('submit', (evento) => {
        evento.preventDefault();
        enviar(input.value);
    });

    // Enter envia; Shift+Enter quebra a linha.
    input.addEventListener('keydown', (evento) => {
        if (evento.key === 'Enter' && !evento.shiftKey && !evento.isComposing) {
            evento.preventDefault();
            $('#chat-form').requestSubmit();
        }
    });
    input.addEventListener('input', ajustarAlturaInput);

    registrarAcoes({
        'abrir-chat': () => abrirChat(),
        'fechar-chat': () => fecharChat(),
        'alternar-chat': () => (chatAberto() ? fecharChat() : abrirChat()),
        'nova-conversa': () => novaConversa(),
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

const chatAberto = () => !$('#chat-janela').classList.contains('hidden');

export function abrirChat() {
    mostrar($('#chat-janela'), true);
    $('#chat-botao').setAttribute('aria-expanded', 'true');
    $('#chat-botao').setAttribute('aria-label', 'Fechar chat com a IAjudinha');
    rolarParaFim();
    $('#chat-input').focus();
}

function fecharChat() {
    if (!chatAberto()) return;
    mostrar($('#chat-janela'), false);
    $('#chat-botao').setAttribute('aria-expanded', 'false');
    $('#chat-botao').setAttribute('aria-label', 'Abrir chat com a IAjudinha');
    $('#chat-botao').focus();
}

function novaConversa() {
    if (aguardando) return;
    historico.length = 0;
    fluxo.etapa = null;
    $('#chat-mensagens').innerHTML = '';
    adicionarMensagem('ia', SAUDACAO, { sugestoes: SUGESTOES });
    $('#chat-input').focus();
}

async function enviar(textoDigitado) {
    const texto = textoDigitado.trim();
    if (!texto || aguardando) return;

    $('#chat-input').value = '';
    ajustarAlturaInput();
    removerSugestoes();
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
            adicionarMensagem('ia', erro.message, {
                erro: true,
                botoes: erro.status === 503
                    ? [{ rotulo: 'Abrir chamado', acao: 'sugestao-chat', dados: { texto: 'Abrir chamado' } }]
                    : [{ rotulo: 'Tentar de novo', acao: 'sugestao-chat', dados: { texto } }],
            });
        }
    });
}

// O campo continua editável enquanto a resposta não chega; só o envio fica bloqueado.
async function comIndicadorDigitando(tarefa) {
    aguardando = true;
    $('#chat-enviar').disabled = true;

    const indicador = document.createElement('div');
    indicador.className = 'flex items-end gap-2';
    indicador.innerHTML = `
        ${avatarIa()}
        <div class="flex items-center gap-1 rounded-2xl rounded-bl-md bg-white/[0.06] px-4 py-3.5" role="status" aria-label="IAjudinha está digitando">
            <span class="h-1.5 w-1.5 animate-bounce rounded-full bg-secondary [animation-delay:-0.3s]"></span>
            <span class="h-1.5 w-1.5 animate-bounce rounded-full bg-secondary [animation-delay:-0.15s]"></span>
            <span class="h-1.5 w-1.5 animate-bounce rounded-full bg-secondary"></span>
        </div>`;
    $('#chat-mensagens').append(indicador);
    rolarParaFim();

    try {
        await tarefa();
    } finally {
        indicador.remove();
        aguardando = false;
        $('#chat-enviar').disabled = false;
        if (chatAberto()) $('#chat-input').focus();
    }
}

// ---------------------------------------------------------------
// Renderização das mensagens
// ---------------------------------------------------------------
const avatarIa = () => `
    <span class="grid h-7 w-7 shrink-0 place-items-center rounded-full bg-gradient-to-br from-ia to-primary text-white ring-1 ring-white/10">
        ${icone('brilho', 'h-3.5 w-3.5')}
    </span>`;

function adicionarMensagem(autor, texto, { sugestoes = [], botoes = [], fontes = [], erro = false } = {}) {
    const mensagens = $('#chat-mensagens');
    const linha = document.createElement('div');

    if (autor === 'usuario') {
        linha.className = 'flex animate-surgir justify-end';
        linha.innerHTML = `
            <div class="max-w-[82%] whitespace-pre-line break-words rounded-2xl rounded-br-md bg-primary px-3.5 py-2.5 text-sm text-white shadow ring-1 ring-white/10">${escapeHtml(texto)}</div>`;
    } else {
        const links = fontes.filter((f) => /^https?:\/\//.test(f.url));
        linha.className = 'flex animate-surgir items-end gap-2';
        linha.innerHTML = `
            ${avatarIa()}
            <div class="max-w-[82%] break-words rounded-2xl rounded-bl-md px-3.5 py-2.5 text-sm leading-relaxed shadow ring-1 ${erro
                ? 'bg-red-500/10 text-red-200 ring-red-500/30'
                : 'bg-white/[0.06] text-gray-100 ring-white/[0.04]'}">
                <div class="space-y-2">${markdownSimples(texto)}</div>
                ${links.length ? `
                    <p class="mt-2 border-t border-white/10 pt-1.5 text-xs text-gray-400">Fontes:
                        ${links.map((f, i) => `<a href="${escapeHtml(f.url)}" target="_blank" rel="noopener noreferrer" class="ml-1 underline hover:text-white">${i + 1}. ${escapeHtml(f.titulo)}</a>`).join('')}
                    </p>` : ''}
                ${botoes.length ? `<div class="mt-2.5 flex flex-wrap gap-2">${botoes.map(htmlBotao).join('')}</div>` : ''}
            </div>`;
    }
    mensagens.append(linha);

    if (sugestoes.length) {
        const chips = document.createElement('div');
        chips.className = 'flex animate-surgir flex-wrap gap-2 pl-9';
        chips.dataset.sugestoes = '';
        chips.innerHTML = sugestoes.map((s) => `
            <button type="button" class="rounded-full border border-white/15 bg-white/[0.03] px-3 py-1.5 text-xs text-secondary transition hover:border-secondary hover:bg-secondary hover:text-primary"
                data-acao="sugestao-chat" data-texto="${escapeHtml(s)}">${escapeHtml(s)}</button>`).join('');
        mensagens.append(chips);
    }

    rolarParaFim();
}

// Sugestões antigas saem de cena assim que a pessoa manda uma nova mensagem.
function removerSugestoes() {
    document.querySelectorAll('#chat-mensagens [data-sugestoes]').forEach((chips) => chips.remove());
}

function htmlBotao({ rotulo, acao, href, dados = {} }) {
    const classes = 'inline-flex items-center rounded-lg bg-secondary px-3 py-1.5 text-xs font-semibold text-primary transition hover:bg-white';
    if (href) return `<a href="${escapeHtml(href)}" class="${classes}">${escapeHtml(rotulo)}</a>`;
    const atributos = Object.entries(dados).map(([chave, valor]) => `data-${chave}="${escapeHtml(valor)}"`).join(' ');
    return `<button type="button" class="${classes}" data-acao="${acao}" ${atributos}>${escapeHtml(rotulo)}</button>`;
}

// Converte o básico de Markdown das respostas da IA (parágrafos, listas, negrito e itálico).
// O texto é escapado antes, então nenhuma tag vinda da IA é interpretada.
function markdownSimples(texto) {
    const blocos = [];
    let lista = null;

    for (const linha of escapeHtml(texto).split('\n')) {
        const item = linha.match(/^\s*(?:[-*•]|(\d+)[.)])\s+(.*)$/);
        if (item) {
            const tipo = item[1] ? 'ol' : 'ul';
            if (lista?.tipo !== tipo) blocos.push(lista = { tipo, itens: [] });
            lista.itens.push(item[2]);
            continue;
        }
        lista = null;
        if (linha.trim()) blocos.push({ tipo: 'p', texto: linha.replace(/^\s*#{1,6}\s+/, '') });
    }

    const inline = (trecho) => trecho
        .replace(/\*\*(.+?)\*\*/g, '<strong class="font-semibold text-white">$1</strong>')
        .replace(/(^|[^*])\*([^*\n]+)\*/g, '$1<em>$2</em>');

    return blocos.map((bloco) => {
        if (bloco.tipo === 'p') return `<p>${inline(bloco.texto)}</p>`;
        const classes = bloco.tipo === 'ol' ? 'list-decimal' : 'list-disc';
        return `<${bloco.tipo} class="${classes} space-y-1 pl-5 marker:text-secondary/60">${bloco.itens.map((i) => `<li>${inline(i)}</li>`).join('')}</${bloco.tipo}>`;
    }).join('');
}

function ajustarAlturaInput() {
    const input = $('#chat-input');
    input.style.height = 'auto';
    input.style.height = `${Math.min(input.scrollHeight, ALTURA_MAXIMA_INPUT)}px`;
}

function rolarParaFim() {
    const mensagens = $('#chat-mensagens');
    mensagens.scrollTop = mensagens.scrollHeight;
}
