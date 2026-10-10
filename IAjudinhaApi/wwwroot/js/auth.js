import { api } from './api.js';
import { iniciarSessao, encerrarSessao, aoMudarSessao, usuarioAtual } from './sessao.js';
import { $, abrirModal, fecharModal, configurarFormulario, registrarAcoes, toast, escapeHtml, primeiroNome } from './ui.js';

// Ação executada depois de um login bem-sucedido (ex.: continuar a abertura de chamado no chat).
let aposLogin = null;

export function pedirLogin(perfil, depois = null) {
    aposLogin = depois;
    abrirModal(perfil === 'Funcionario' ? 'modal-funcionario' : 'modal-login');
}

export function iniciarAuth() {
    configurarFormulario('#form-login', async ({ email, senha }) => {
        const resposta = await api('/auth/login', { metodo: 'POST', corpo: { email, senha } });
        concluirLogin(resposta, 'modal-login');
    });

    configurarFormulario('#form-funcionario', async ({ email, senha }) => {
        const resposta = await api('/auth/funcionarios/login', { metodo: 'POST', corpo: { email, senha } });
        concluirLogin(resposta, 'modal-funcionario');
    });

    configurarFormulario('#form-cadastro', async ({ nome, email, telefone, dataNascimento, senha }) => {
        const resposta = await api('/auth/cadastro', {
            metodo: 'POST',
            corpo: { nome, email, telefone, dataNascimento: dataNascimento || null, senha },
        });
        concluirLogin(resposta, 'modal-cadastro', 'Conta criada! ');
    });

    const telefone = $('#cadastro-telefone');
    telefone.addEventListener('input', () => { telefone.value = mascararTelefone(telefone.value); });
    $('#cadastro-nascimento').max = new Date().toLocaleDateString('sv-SE'); // AAAA-MM-DD no fuso local

    registrarAcoes({
        sair: () => {
            encerrarSessao();
            toast('Você saiu da sua conta.');
        },
        'preencher-demo': (botao) => {
            const form = botao.closest('form');
            form.elements.email.value = botao.dataset.email;
            form.elements.senha.value = botao.dataset.senha;
        },
    });

    aoMudarSessao(renderizarAreaUsuario);
    renderizarAreaUsuario(usuarioAtual());
}

function concluirLogin(resposta, idModal, prefixo = '') {
    iniciarSessao(resposta);
    fecharModal(idModal);
    $(`#${idModal} form`).reset();
    toast(`${prefixo}Bem-vindo(a), ${primeiroNome(resposta.usuario.nome)}!`);

    const depois = aposLogin;
    aposLogin = null;
    if (depois) depois();
    else if (resposta.usuario.perfil === 'Funcionario') location.hash = '#/painel';
}

function renderizarAreaUsuario(usuario) {
    const desktop = $('#area-usuario');
    const mobile = $('#area-usuario-mobile');

    if (!usuario) {
        const botoes = (extra) => `
            <button type="button" class="btn-fantasma ${extra}" data-acao="abrir-modal" data-modal="modal-funcionario">Funcionários</button>
            <button type="button" class="btn-secundario py-1.5 ${extra}" data-acao="abrir-modal" data-modal="modal-login">Entrar</button>
            <button type="button" class="btn-primario py-1.5 ${extra}" data-acao="abrir-modal" data-modal="modal-cadastro">Criar conta</button>`;
        desktop.innerHTML = botoes('');
        mobile.innerHTML = botoes('w-full');
        return;
    }

    const equipe = usuario.perfil === 'Funcionario';
    const link = equipe
        ? '<a href="#/painel" class="link-nav" data-nav="painel">Painel</a>'
        : '<a href="#/meus-chamados" class="link-nav" data-nav="meus-chamados">Meus chamados</a>';
    const saudacao = `
        <span class="px-2 text-sm text-secondary">
            Olá, <strong class="text-white">${escapeHtml(primeiroNome(usuario.nome))}</strong>
            ${equipe ? '<span class="ml-1 rounded bg-sky-400/15 px-1.5 py-0.5 text-xs font-semibold text-sky-300">Equipe</span>' : ''}
        </span>`;
    const sair = '<button type="button" class="btn-fantasma" data-acao="sair">Sair</button>';

    desktop.innerHTML = link + saudacao + sair;
    mobile.innerHTML = saudacao + link.replace('class="link-nav"', 'class="link-nav block"') + sair;
}

function mascararTelefone(valor) {
    const d = valor.replace(/\D/g, '').slice(0, 11);
    if (d.length === 0) return '';
    if (d.length <= 2) return `(${d}`;
    if (d.length <= 6) return `(${d.slice(0, 2)}) ${d.slice(2)}`;
    if (d.length <= 10) return `(${d.slice(0, 2)}) ${d.slice(2, 6)}-${d.slice(6)}`;
    return `(${d.slice(0, 2)}) ${d.slice(2, 7)}-${d.slice(7)}`;
}
