// Guarda o token JWT e os dados do usuário logado no navegador.
const CHAVE = 'heaven-games:sessao';
const ouvintes = new Set();
let sessao = carregar();

function carregar() {
    try {
        const salva = JSON.parse(localStorage.getItem(CHAVE));
        if (salva?.token && new Date(salva.expiraEm) > new Date()) return salva;
    } catch {
        // localStorage indisponível (modo privado, por exemplo): segue sem sessão salva.
    }
    return null;
}

export function obterSessao() {
    if (sessao && new Date(sessao.expiraEm) <= new Date()) encerrarSessao();
    return sessao;
}

export const usuarioAtual = () => obterSessao()?.usuario ?? null;
export const ehCliente = () => usuarioAtual()?.perfil === 'Cliente';
export const ehFuncionario = () => usuarioAtual()?.perfil === 'Funcionario';

export function iniciarSessao(resposta) {
    sessao = resposta;
    try {
        localStorage.setItem(CHAVE, JSON.stringify(resposta));
    } catch { /* sessão vale só para esta aba */ }
    notificar();
}

export function encerrarSessao() {
    if (!sessao) return;
    sessao = null;
    try {
        localStorage.removeItem(CHAVE);
    } catch { /* nada a remover */ }
    notificar();
}

export function aoMudarSessao(callback) {
    ouvintes.add(callback);
}

function notificar() {
    const usuario = sessao?.usuario ?? null;
    ouvintes.forEach((callback) => callback(usuario));
}
