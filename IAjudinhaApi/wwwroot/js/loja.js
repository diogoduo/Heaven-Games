import { api } from './api.js';
import { $, escapeHtml, formatarPreco, htmlErro, icone, normalizar } from './ui.js';

const CATEGORIAS = ['Ação', 'Aventura', 'Terror', 'Simulação', 'Estratégia', 'RPG', 'Esporte', 'Indie', 'VR'];

// Jogos gratuitos (preço nulo) contam como R$ 0 na ordenação.
const ORDENACOES = {
    destaques: (a, b) => Number(b.emDestaque) - Number(a.emDestaque),
    'menor-preco': (a, b) => (a.preco ?? 0) - (b.preco ?? 0),
    'maior-preco': (a, b) => (b.preco ?? 0) - (a.preco ?? 0),
    nome: (a, b) => a.titulo.localeCompare(b.titulo, 'pt-BR'),
};

let jogos = [];
let carregamento = null;
let categoriaAtual = null;

// O catálogo é buscado uma única vez e reaproveitado na página inicial e na loja.
function carregarJogos() {
    carregamento ??= api('/jogos')
        .then((lista) => { jogos = lista; })
        .catch((erro) => { carregamento = null; throw erro; });
    return carregamento;
}

export async function iniciarLoja() {
    $('#busca-loja').addEventListener('input', renderizarGrid);
    $('#ordem-loja').addEventListener('change', renderizarGrid);

    const grid = $('#grid-destaques');
    try {
        await carregarJogos();
        grid.innerHTML = jogos.filter((j) => j.emDestaque).map(cartaoJogo).join('');
        $('#total-jogos').textContent = jogos.length;
    } catch (erro) {
        grid.innerHTML = `<div class="col-span-full">${htmlErro(erro.message)}</div>`;
    }
}

export async function renderizarLoja(categoria) {
    categoriaAtual = categoria;
    $('#titulo-loja').textContent = categoria ? `Jogos de ${categoria}` : 'Todos os jogos';
    $('#filtros-loja').innerHTML = [null, ...CATEGORIAS].map((c) => `
        <a href="${c ? `#/loja/${encodeURIComponent(c)}` : '#/loja'}" class="chip" ${c === categoria ? 'aria-current="true"' : ''}>
            ${escapeHtml(c ?? 'Todos')}
        </a>`).join('');

    try {
        await carregarJogos();
    } catch (erro) {
        $('#grid-loja').innerHTML = `<div class="col-span-full">${htmlErro(erro.message)}</div>`;
        return;
    }
    renderizarGrid();
}

function renderizarGrid() {
    const termo = normalizar($('#busca-loja').value);
    const lista = jogos
        .filter((j) => !categoriaAtual || j.genero.split(' / ').includes(categoriaAtual))
        .filter((j) => !termo || normalizar(`${j.titulo} ${j.genero}`).includes(termo))
        .sort(ORDENACOES[$('#ordem-loja').value]);

    $('#contador-loja').textContent = `${lista.length} ${lista.length === 1 ? 'jogo encontrado' : 'jogos encontrados'}`;
    $('#grid-loja').innerHTML = lista.length
        ? lista.map(cartaoJogo).join('')
        : `<div class="cartao col-span-full p-10 text-center">
               <p class="text-gray-300">Nenhum jogo encontrado${termo ? ' para essa busca' : ' nesta categoria por enquanto'}.</p>
               <p class="mt-1 text-sm text-gray-500">Tente outra categoria ou pergunte à IAjudinha por uma recomendação.</p>
           </div>`;
}

function cartaoJogo(jogo) {
    const titulo = escapeHtml(jogo.titulo);
    const gratis = jogo.preco == null;
    return `
        <article class="group flex flex-col overflow-hidden rounded-2xl bg-card-bg shadow-lg shadow-black/30 ring-1 ring-white/[0.07] transition duration-300 hover:-translate-y-1 hover:shadow-2xl hover:shadow-black/50 hover:ring-secondary/40">
            <div class="relative aspect-[3/4] overflow-hidden bg-white/5">
                <img src="${escapeHtml(jogo.imagem)}" alt="Capa do jogo ${titulo}" loading="lazy"
                    class="h-full w-full object-cover transition duration-500 group-hover:scale-105">
                <div class="pointer-events-none absolute inset-x-0 bottom-0 h-1/3 bg-gradient-to-t from-card-bg to-transparent"></div>
            </div>
            <div class="flex flex-1 flex-col p-3 pt-1">
                <h3 class="line-clamp-1 text-sm font-semibold text-white" title="${titulo}">${titulo}</h3>
                <p class="line-clamp-1 text-xs text-gray-400">${escapeHtml(jogo.genero)}</p>
                <p class="mt-2 text-base font-bold ${gratis ? 'text-emerald-300' : 'text-white'}">${formatarPreco(jogo.preco)}</p>
                <button type="button" data-acao="perguntar-jogo" data-titulo="${titulo}"
                    aria-label="Perguntar à IAjudinha sobre ${titulo}" title="Perguntar à IAjudinha"
                    class="mt-3 inline-flex w-full items-center justify-center gap-1.5 rounded-lg bg-white/[0.04] py-1.5 text-xs font-medium text-secondary ring-1 ring-white/10 transition hover:bg-secondary hover:text-primary">
                    ${icone('brilho', 'h-3.5 w-3.5 shrink-0')} Perguntar à IA
                </button>
            </div>
        </article>`;
}
