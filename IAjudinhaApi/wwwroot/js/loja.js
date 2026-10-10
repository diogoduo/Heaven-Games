import { api } from './api.js';
import { $, escapeHtml, formatarPreco, htmlErro } from './ui.js';

const CATEGORIAS = ['Ação', 'Aventura', 'Terror', 'Simulação', 'Estratégia', 'RPG', 'Esporte', 'Indie', 'VR'];

let jogos = [];
let carregamento = null;

// O catálogo é buscado uma única vez e reaproveitado na página inicial e na loja.
function carregarJogos() {
    carregamento ??= api('/jogos')
        .then((lista) => { jogos = lista; })
        .catch((erro) => { carregamento = null; throw erro; });
    return carregamento;
}

export async function iniciarLoja() {
    const grid = $('#grid-destaques');
    try {
        await carregarJogos();
        grid.innerHTML = jogos.filter((j) => j.emDestaque).map(cartaoJogo).join('');
    } catch (erro) {
        grid.innerHTML = `<div class="col-span-full">${htmlErro(erro.message)}</div>`;
    }
}

export async function renderizarLoja(categoria) {
    $('#titulo-loja').textContent = categoria ? `Jogos de ${categoria}` : 'Todos os jogos';
    $('#filtros-loja').innerHTML = [null, ...CATEGORIAS].map((c) => `
        <a href="${c ? `#/loja/${encodeURIComponent(c)}` : '#/loja'}" class="chip" ${c === categoria ? 'aria-current="true"' : ''}>
            ${escapeHtml(c ?? 'Todos')}
        </a>`).join('');

    const grid = $('#grid-loja');
    try {
        await carregarJogos();
    } catch (erro) {
        grid.innerHTML = `<div class="col-span-full">${htmlErro(erro.message)}</div>`;
        return;
    }

    const lista = categoria ? jogos.filter((j) => j.genero.split(' / ').includes(categoria)) : jogos;
    grid.innerHTML = lista.length
        ? lista.map(cartaoJogo).join('')
        : '<p class="col-span-full py-10 text-center text-gray-400">Nenhum jogo nesta categoria por enquanto.</p>';
}

function cartaoJogo(jogo) {
    const titulo = escapeHtml(jogo.titulo);
    return `
        <article class="group flex flex-col overflow-hidden rounded-xl bg-card-bg shadow-lg ring-1 ring-white/5 transition duration-300 hover:-translate-y-1 hover:ring-secondary/40">
            <img src="${escapeHtml(jogo.imagem)}" alt="Capa do jogo ${titulo}" loading="lazy"
                class="h-40 w-full border-b border-primary object-cover">
            <div class="flex flex-1 flex-col p-3">
                <h3 class="text-sm font-semibold text-secondary">${titulo}</h3>
                <p class="text-xs text-gray-400">${escapeHtml(jogo.genero)}</p>
                <p class="mt-1 text-sm font-bold text-white">${formatarPreco(jogo.preco)}</p>
                <button type="button" data-acao="perguntar-jogo" data-titulo="${titulo}"
                    class="mt-auto pt-3 text-left text-xs text-secondary/80 underline-offset-2 hover:text-white hover:underline">
                    Perguntar à IAjudinha
                </button>
            </div>
        </article>`;
}
