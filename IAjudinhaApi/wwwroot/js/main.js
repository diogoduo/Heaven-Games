import { api } from './api.js';
import { aoMudarSessao, usuarioAtual } from './sessao.js';
import { $, iniciarUi, registrarAcoes, abrirModal, fecharModal, paginaAtual, toast } from './ui.js';
import { iniciarAuth, pedirLogin } from './auth.js';
import { iniciarLoja, renderizarLoja } from './loja.js';
import { iniciarChamados, carregarMeusChamados, renderizarSuporte } from './chamados.js';
import { iniciarPainel, carregarPainel } from './painel.js';
import { iniciarChat } from './chat.js';

// Rotas da aplicação (#/nome/parametro). "perfil" restringe o acesso.
const PAGINAS = {
    inicio: {},
    loja: { aoEntrar: renderizarLoja },
    suporte: { aoEntrar: renderizarSuporte },
    'meus-chamados': { perfil: 'Cliente', aoEntrar: carregarMeusChamados },
    painel: { perfil: 'Funcionario', aoEntrar: carregarPainel },
};

function rotear() {
    const { nome, parametro } = paginaAtual();
    const pagina = PAGINAS[nome];
    if (!pagina) {
        location.replace('#/');
        return;
    }

    const usuario = usuarioAtual();
    if (pagina.perfil && usuario?.perfil !== pagina.perfil) {
        const destino = location.hash;
        location.replace('#/suporte');
        if (usuario) toast('Esta área não está disponível para o seu tipo de conta.', 'erro');
        else pedirLogin(pagina.perfil, () => { location.hash = destino; });
        return;
    }

    document.querySelectorAll('[data-pagina]').forEach((secao) => {
        secao.hidden = secao.dataset.pagina !== nome;
    });
    document.querySelectorAll('[data-nav]').forEach((link) => {
        if (link.dataset.nav === nome) link.setAttribute('aria-current', 'page');
        else link.removeAttribute('aria-current');
    });
    alternarMenuMobile(false);
    pagina.aoEntrar?.(parametro);
}

function alternarMenuMobile(abrir) {
    const menu = $('#menu-mobile');
    const deveAbrir = typeof abrir === 'boolean' ? abrir : menu.classList.contains('hidden');
    menu.classList.toggle('hidden', !deveAbrir);
    $('[data-acao="menu-mobile"]').setAttribute('aria-expanded', String(deveAbrir));
}

function rolarParaFaq() {
    setTimeout(() => $('#faq').scrollIntoView({ behavior: 'smooth' }));
}

iniciarUi();
iniciarAuth();
iniciarChamados();
iniciarPainel();
iniciarChat();
iniciarLoja();

registrarAcoes({
    'abrir-modal': (botao) => abrirModal(botao.dataset.modal),
    'fechar-modal': (botao) => fecharModal(botao.closest('.modal').id),
    'trocar-modal': (botao) => {
        fecharModal(botao.closest('.modal').id);
        abrirModal(botao.dataset.modal);
    },
    'menu-mobile': () => alternarMenuMobile(),
    'ir-faq': () => {
        if (paginaAtual().nome === 'suporte') {
            rolarParaFaq();
        } else {
            window.addEventListener('hashchange', rolarParaFaq, { once: true });
            location.hash = '#/suporte';
        }
    },
});

// Ao sair de uma área restrita, volta para o início em vez de pedir login de novo.
aoMudarSessao((usuario) => {
    if (!usuario && PAGINAS[paginaAtual().nome]?.perfil) location.hash = '#/';
    else rotear();
});

window.addEventListener('hashchange', () => {
    rotear();
    window.scrollTo({ top: 0 });
});

$('#ano').textContent = new Date().getFullYear();
rotear();

// Confere se o token salvo ainda é aceito pela API (se não for, a sessão é encerrada).
if (usuarioAtual()) api('/auth/me').catch(() => {});
