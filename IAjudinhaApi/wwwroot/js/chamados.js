import { api } from './api.js';
import { ehCliente, ehFuncionario, usuarioAtual } from './sessao.js';
import {
    $, mostrar, abrirModal, configurarFormulario, registrarAcoes, toast, paginaAtual,
    escapeHtml, formatarData, tempoRelativo, badgeStatus, primeiroNome, htmlCarregando, htmlErro,
} from './ui.js';

// Chamado exibido no modal de detalhes.
let idChamadoAberto = null;

export function iniciarChamados() {
    configurarFormulario('#form-chamado', async ({ assunto, descricao }, form) => {
        const chamado = await criarChamado(assunto, descricao);
        form.reset();
        toast(`Chamado #${chamado.idChamado} aberto! Nossa equipe vai responder em breve.`);
        location.hash = '#/meus-chamados';
        abrirDetalhe(chamado.idChamado);
    });

    configurarFormulario('#form-resposta', async ({ mensagem }, form) => {
        await api(`/chamados/${idChamadoAberto}/interacoes`, { metodo: 'POST', corpo: { mensagem } });
        form.reset();
        await abrirDetalhe(idChamadoAberto);
        avisarAlteracao();
    });

    configurarFormulario('#form-status', async ({ status }) => {
        await api(`/chamados/${idChamadoAberto}/status`, { metodo: 'PATCH', corpo: { status } });
        toast('Status atualizado.');
        await abrirDetalhe(idChamadoAberto);
        avisarAlteracao();
    });

    registrarAcoes({
        'abrir-chamado': (elemento) => abrirDetalhe(Number(elemento.dataset.id)),
    });

    document.addEventListener('chamados:alterados', () => {
        if (paginaAtual().nome === 'meus-chamados') carregarMeusChamados();
    });
}

export async function criarChamado(assunto, descricao) {
    const chamado = await api('/chamados', { metodo: 'POST', corpo: { assunto, descricao } });
    avisarAlteracao();
    return chamado;
}

function avisarAlteracao() {
    document.dispatchEvent(new CustomEvent('chamados:alterados'));
}

// ---------------------------------------------------------------
// Página de suporte: conteúdo muda conforme quem está logado
// ---------------------------------------------------------------
export function renderizarSuporte() {
    mostrar($('#suporte-visitante'), !usuarioAtual());
    mostrar($('#suporte-cliente'), ehCliente());
    mostrar($('#suporte-funcionario'), ehFuncionario());
    if (ehCliente()) $('#nome-cliente-suporte').textContent = primeiroNome(usuarioAtual().nome);
}

// ---------------------------------------------------------------
// Meus chamados (cliente)
// ---------------------------------------------------------------
export async function carregarMeusChamados() {
    const lista = $('#lista-meus-chamados');
    lista.innerHTML = htmlCarregando();

    try {
        const chamados = await api('/chamados/meus');
        lista.innerHTML = chamados.length
            ? chamados.map(cartaoChamado).join('')
            : `<div class="cartao p-10 text-center">
                   <p class="text-gray-300">Você ainda não abriu nenhum chamado.</p>
                   <p class="mt-1 text-sm text-gray-500">Use o botão "Novo chamado" ou peça ajuda à IAjudinha.</p>
               </div>`;
    } catch (erro) {
        lista.innerHTML = htmlErro(erro.message);
    }
}

function cartaoChamado(c) {
    return `
        <button type="button" data-acao="abrir-chamado" data-id="${c.idChamado}"
            class="cartao flex w-full flex-col gap-2 p-4 text-left transition hover:ring-secondary/40 sm:flex-row sm:items-center sm:justify-between">
            <div>
                <p class="text-xs font-mono text-secondary/70">#${c.idChamado}</p>
                <p class="font-semibold text-white">${escapeHtml(c.assunto)}</p>
                <p class="text-xs text-gray-400">Atualizado ${tempoRelativo(c.dataAtualizacao)}${c.nomeResponsavel ? ` · Atendido por ${escapeHtml(c.nomeResponsavel)}` : ''}</p>
            </div>
            ${badgeStatus(c.status)}
        </button>`;
}

// ---------------------------------------------------------------
// Detalhes do chamado (cliente e equipe)
// ---------------------------------------------------------------
export async function abrirDetalhe(id) {
    idChamadoAberto = id;
    try {
        const chamado = await api(`/chamados/${id}`);
        renderizarDetalhe(chamado);
        abrirModal('modal-chamado');
    } catch (erro) {
        toast(erro.message, 'erro');
    }
}

function renderizarDetalhe(c) {
    const equipe = ehFuncionario();
    const item = (rotulo, valor) => `
        <div>
            <dt class="text-gray-500">${rotulo}</dt>
            <dd class="text-gray-200">${escapeHtml(valor)}</dd>
        </div>`;

    $('#detalhe-cabecalho').innerHTML = `
        <div class="flex flex-wrap items-center gap-3 pr-10">
            <span class="font-mono text-sm text-secondary/70">#${c.idChamado}</span>
            ${badgeStatus(c.status)}
        </div>
        <h2 id="titulo-chamado" class="mt-2 pr-10 text-xl font-bold text-white">${escapeHtml(c.assunto)}</h2>
        <dl class="mt-4 grid grid-cols-2 gap-3 text-xs sm:grid-cols-3">
            ${equipe ? item('Cliente', `${c.nomeCliente} (${c.emailCliente})`) : ''}
            ${item('Aberto em', formatarData(c.dataInicio))}
            ${item('Responsável', c.nomeResponsavel ?? 'Aguardando equipe')}
            ${c.dataFim ? item('Resolvido em', formatarData(c.dataFim)) : ''}
        </dl>`;

    // A descrição inicial aparece como a primeira mensagem da conversa.
    const mensagens = [
        { tipoAutor: 'Cliente', nomeAutor: c.nomeCliente, mensagem: c.descricao, dataHora: c.dataInicio },
        ...c.interacoes,
    ];
    const historico = $('#detalhe-historico');
    historico.innerHTML = mensagens.map((m) => bolhaMensagem(m, equipe)).join('');
    historico.scrollTop = historico.scrollHeight;

    const resolvido = c.status === 'Resolvido';
    mostrar($('#form-resposta'), !resolvido);
    mostrar($('#detalhe-resolvido'), resolvido);
    mostrar($('#form-status'), equipe);
    $('#detalhe-status').value = c.status;
    mostrar($('#form-resposta [data-erro]'), false);
}

function bolhaMensagem(m, visaoEquipe) {
    const daEquipe = m.tipoAutor === 'Funcionario';
    const minha = daEquipe === visaoEquipe;
    return `
        <div class="flex ${minha ? 'justify-end' : 'justify-start'}">
            <div class="max-w-[85%] rounded-xl px-4 py-3 text-sm ${minha ? 'bg-primary text-white' : 'bg-white/5 text-gray-200'}">
                <p class="mb-1 text-xs font-semibold ${daEquipe ? 'text-sky-300' : 'text-secondary'}">
                    ${escapeHtml(m.nomeAutor)}${daEquipe ? ' · Equipe Heaven Games' : ''}
                </p>
                <p class="whitespace-pre-line break-words">${escapeHtml(m.mensagem)}</p>
                <p class="mt-1 text-[11px] text-gray-400">${formatarData(m.dataHora)}</p>
            </div>
        </div>`;
}
