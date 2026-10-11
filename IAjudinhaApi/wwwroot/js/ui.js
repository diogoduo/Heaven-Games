// Utilitários de interface: ações por data-acao, modais, toasts e formatação.

export const $ = (seletor, raiz = document) => raiz.querySelector(seletor);

export function mostrar(elemento, visivel) {
    elemento.classList.toggle('hidden', !visivel);
}

// ---------------------------------------------------------------
// Ações: elementos com data-acao="nome" chamam acoes[nome](elemento)
// ---------------------------------------------------------------
const acoes = {};

export function registrarAcoes(novas) {
    Object.assign(acoes, novas);
}

export function iniciarUi() {
    document.addEventListener('click', (evento) => {
        const modal = evento.target.classList?.contains('modal') ? evento.target : null;
        if (modal) {
            fecharModal(modal.id);
            return;
        }

        const alvo = evento.target.closest('[data-acao]');
        const acao = alvo && acoes[alvo.dataset.acao];
        if (acao) {
            evento.preventDefault();
            acao(alvo, evento);
        }
    });

    document.addEventListener('keydown', (evento) => {
        if (evento.key === 'Escape' && !fecharModalDoTopo()) acoes['fechar-chat']?.();
    });
}

// Lê a rota atual do hash: "#/loja/Terror" -> { nome: 'loja', parametro: 'Terror' }
export function paginaAtual() {
    const [, nome, parametro] = location.hash.split('/');
    return { nome: nome || 'inicio', parametro: parametro ? decodeURIComponent(parametro) : null };
}

// ---------------------------------------------------------------
// Modais
// ---------------------------------------------------------------
const modaisAbertos = [];

export function abrirModal(id) {
    const modal = document.getElementById(id);
    if (!modal || !modal.classList.contains('hidden')) return;

    modaisAbertos.push({ id, focoAnterior: document.activeElement });
    modal.classList.replace('hidden', 'flex');
    document.body.classList.add('overflow-hidden');
    (modal.querySelector('[data-foco-inicial]') ?? modal.querySelector('input, textarea, select'))?.focus();
}

export function fecharModal(id) {
    const modal = document.getElementById(id);
    if (!modal || modal.classList.contains('hidden')) return;

    modal.classList.replace('flex', 'hidden');
    const indice = modaisAbertos.findIndex((m) => m.id === id);
    const [registro] = indice >= 0 ? modaisAbertos.splice(indice, 1) : [];
    if (modaisAbertos.length === 0) document.body.classList.remove('overflow-hidden');
    registro?.focoAnterior?.focus?.();
}

function fecharModalDoTopo() {
    const topo = modaisAbertos.at(-1);
    if (!topo) return false;
    fecharModal(topo.id);
    return true;
}

// ---------------------------------------------------------------
// Formulários: desabilita o botão durante o envio e exibe o erro no próprio formulário
// ---------------------------------------------------------------
export function configurarFormulario(seletor, aoEnviar) {
    const form = $(seletor);
    form.addEventListener('submit', async (evento) => {
        evento.preventDefault();
        const botao = form.querySelector('[type="submit"]');
        const erro = form.querySelector('[data-erro]');
        const textoOriginal = botao.textContent;

        if (erro) mostrar(erro, false);
        botao.disabled = true;
        botao.textContent = botao.dataset.carregando ?? textoOriginal;

        try {
            await aoEnviar(Object.fromEntries(new FormData(form)), form);
        } catch (e) {
            if (erro) {
                erro.textContent = e.message;
                mostrar(erro, true);
            } else {
                toast(e.message, 'erro');
            }
        } finally {
            botao.disabled = false;
            botao.textContent = textoOriginal;
        }
    });
}

// ---------------------------------------------------------------
// Toasts
// ---------------------------------------------------------------
export function toast(mensagem, tipo = 'sucesso') {
    const elemento = document.createElement('div');
    elemento.setAttribute('role', tipo === 'erro' ? 'alert' : 'status');
    elemento.className = `toast pointer-events-auto rounded-xl px-4 py-3 text-sm shadow-xl ring-1 ${tipo === 'erro'
        ? 'bg-red-950 text-red-100 ring-red-500/40'
        : 'bg-card-bg text-white ring-secondary/40'}`;
    elemento.textContent = mensagem;

    const container = $('#toasts');
    container.append(elemento);
    while (container.children.length > 3) container.firstElementChild.remove();

    setTimeout(() => {
        elemento.classList.add('opacity-0');
        setTimeout(() => elemento.remove(), 300);
    }, 4000);
}

// ---------------------------------------------------------------
// Formatação
// ---------------------------------------------------------------
const ESCAPES = { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' };
export const escapeHtml = (valor) => String(valor ?? '').replace(/[&<>"']/g, (c) => ESCAPES[c]);

const formatoData = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'short', timeStyle: 'short' });
export const formatarData = (iso) => (iso ? formatoData.format(new Date(iso)) : '');

const formatoRelativo = new Intl.RelativeTimeFormat('pt-BR', { numeric: 'auto' });
const UNIDADES = [['year', 31536000], ['month', 2592000], ['day', 86400], ['hour', 3600], ['minute', 60]];
export function tempoRelativo(iso) {
    const segundos = (new Date(iso) - Date.now()) / 1000;
    for (const [unidade, duracao] of UNIDADES) {
        if (Math.abs(segundos) >= duracao) return formatoRelativo.format(Math.round(segundos / duracao), unidade);
    }
    return 'agora mesmo';
}

const formatoMoeda = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' });
export const formatarPreco = (preco) => (preco == null ? 'Grátis para jogar' : formatoMoeda.format(preco));

export const primeiroNome = (nome) => String(nome ?? '').trim().split(/\s+/)[0];

// Remove acentos e caixa para buscas: "Simulação" -> "simulacao"
export const normalizar = (texto) => texto.normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase().trim();

// ---------------------------------------------------------------
// Ícones (traços do Lucide) usados nos trechos renderizados pelo JS
// ---------------------------------------------------------------
const ICONES = {
    brilho: '<path d="M9.937 15.5A2 2 0 0 0 8.5 14.063l-6.135-1.582a.5.5 0 0 1 0-.962L8.5 9.936A2 2 0 0 0 9.937 8.5l1.582-6.135a.5.5 0 0 1 .963 0L14.063 8.5A2 2 0 0 0 15.5 9.937l6.135 1.581a.5.5 0 0 1 0 .964L15.5 14.063a2 2 0 0 0-1.437 1.437l-1.582 6.135a.5.5 0 0 1-.963 0z"/>',
    seta: '<path d="m9 18 6-6-6-6"/>',
};

export const icone = (nome, classes = 'h-5 w-5') =>
    `<svg class="${classes}" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">${ICONES[nome]}</svg>`;

export const STATUS = {
    Aberto: { rotulo: 'Aberto', classes: 'bg-amber-400/15 text-amber-300 ring-amber-400/30' },
    EmAndamento: { rotulo: 'Em andamento', classes: 'bg-sky-400/15 text-sky-300 ring-sky-400/30' },
    Resolvido: { rotulo: 'Resolvido', classes: 'bg-emerald-400/15 text-emerald-300 ring-emerald-400/30' },
};

export function badgeStatus(status) {
    const info = STATUS[status] ?? { rotulo: status, classes: 'bg-white/10 text-gray-300 ring-white/20' };
    return `<span class="inline-flex items-center whitespace-nowrap rounded-full px-2.5 py-0.5 text-xs font-semibold ring-1 ring-inset ${info.classes}">${escapeHtml(info.rotulo)}</span>`;
}

export const htmlCarregando = (texto = 'Carregando…') =>
    `<p class="animate-pulse p-6 text-center text-sm text-gray-400">${escapeHtml(texto)}</p>`;

export const htmlErro = (mensagem) =>
    `<p class="p-6 text-center text-sm text-red-300">${escapeHtml(mensagem)}</p>`;
