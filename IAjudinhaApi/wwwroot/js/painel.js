import { api } from './api.js';
import { $, paginaAtual, escapeHtml, formatarData, tempoRelativo, badgeStatus, htmlCarregando, htmlErro } from './ui.js';

// Fila de atendimento: chamados abertos primeiro, depois os mais recentes.
const PRIORIDADE = { Aberto: 0, EmAndamento: 1, Resolvido: 2 };
let chamados = [];

export function iniciarPainel() {
    $('#filtro-status').addEventListener('change', renderizarTabela);
    $('#busca-chamado').addEventListener('input', renderizarTabela);
    document.addEventListener('chamados:alterados', () => {
        if (paginaAtual().nome === 'painel') carregarPainel();
    });
}

export async function carregarPainel() {
    const tabela = $('#tabela-chamados');
    if (chamados.length === 0) tabela.innerHTML = htmlCarregando();

    try {
        chamados = (await api('/chamados')).sort((a, b) =>
            PRIORIDADE[a.status] - PRIORIDADE[b.status] || new Date(b.dataAtualizacao) - new Date(a.dataAtualizacao));
        renderizarResumo();
        renderizarTabela();
    } catch (erro) {
        tabela.innerHTML = htmlErro(erro.message);
    }
}

function renderizarResumo() {
    const contar = (status) => chamados.filter((c) => c.status === status).length;
    const cartoes = [
        ['Abertos', contar('Aberto'), 'text-amber-300'],
        ['Em andamento', contar('EmAndamento'), 'text-sky-300'],
        ['Resolvidos', contar('Resolvido'), 'text-emerald-300'],
        ['Total', chamados.length, 'text-white'],
    ];
    $('#painel-resumo').innerHTML = cartoes.map(([rotulo, valor, cor]) => `
        <div class="cartao p-4">
            <p class="text-sm text-gray-400">${rotulo}</p>
            <p class="mt-1 text-3xl font-bold ${cor}">${valor}</p>
        </div>`).join('');
}

function renderizarTabela() {
    const status = $('#filtro-status').value;
    const termo = normalizar($('#busca-chamado').value);
    const filtrados = chamados.filter((c) =>
        (!status || c.status === status) &&
        (!termo || normalizar(`${c.idChamado} ${c.assunto} ${c.nomeCliente} ${c.emailCliente}`).includes(termo)));

    if (filtrados.length === 0) {
        $('#tabela-chamados').innerHTML = '<p class="p-10 text-center text-gray-400">Nenhum chamado encontrado.</p>';
        return;
    }

    $('#tabela-chamados').innerHTML = `
        <table class="w-full min-w-[720px] text-left text-sm">
            <thead class="border-b border-white/10 text-xs uppercase tracking-wide text-gray-500">
                <tr>
                    <th scope="col" class="px-4 py-3">#</th>
                    <th scope="col" class="px-4 py-3">Assunto</th>
                    <th scope="col" class="px-4 py-3">Cliente</th>
                    <th scope="col" class="px-4 py-3">Status</th>
                    <th scope="col" class="px-4 py-3">Responsável</th>
                    <th scope="col" class="px-4 py-3">Atualizado</th>
                </tr>
            </thead>
            <tbody class="divide-y divide-white/5">
                ${filtrados.map(linhaChamado).join('')}
            </tbody>
        </table>`;
}

function linhaChamado(c) {
    return `
        <tr class="cursor-pointer transition hover:bg-white/5" data-acao="abrir-chamado" data-id="${c.idChamado}">
            <td class="px-4 py-3 font-mono text-secondary/70">${c.idChamado}</td>
            <td class="px-4 py-3">
                <button type="button" class="text-left font-medium text-white hover:underline" data-acao="abrir-chamado" data-id="${c.idChamado}">
                    ${escapeHtml(c.assunto)}
                </button>
            </td>
            <td class="px-4 py-3 text-gray-300">${escapeHtml(c.nomeCliente)}</td>
            <td class="px-4 py-3">${badgeStatus(c.status)}</td>
            <td class="px-4 py-3 text-gray-400">${escapeHtml(c.nomeResponsavel ?? '—')}</td>
            <td class="px-4 py-3 text-gray-400" title="${formatarData(c.dataAtualizacao)}">${tempoRelativo(c.dataAtualizacao)}</td>
        </tr>`;
}

const normalizar = (texto) => texto.normalize('NFD').replace(/[̀-ͯ]/g, '').toLowerCase().trim();
