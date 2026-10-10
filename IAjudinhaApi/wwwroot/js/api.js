import { obterSessao, encerrarSessao } from './sessao.js';

export class ErroApi extends Error {
    constructor(mensagem, status) {
        super(mensagem);
        this.status = status;
    }
}

const ROTAS_DE_LOGIN = /^\/auth\/(login|cadastro|funcionarios\/login)$/;

// Chama a API (servida pela mesma origem) e converte erros em mensagens amigáveis.
export async function api(caminho, { metodo = 'GET', corpo } = {}) {
    const sessao = obterSessao();
    const headers = { Accept: 'application/json' };
    if (sessao) headers.Authorization = `Bearer ${sessao.token}`;
    if (corpo !== undefined) headers['Content-Type'] = 'application/json';

    let resposta;
    try {
        resposta = await fetch(`/api${caminho}`, {
            method: metodo,
            headers,
            body: corpo === undefined ? undefined : JSON.stringify(corpo),
        });
    } catch {
        throw new ErroApi('Não foi possível conectar ao servidor. Verifique sua conexão.', 0);
    }

    // Token expirado ou inválido: encerra a sessão (exceto quando a própria tentativa de login falhou).
    if (resposta.status === 401 && sessao && !ROTAS_DE_LOGIN.test(caminho)) {
        encerrarSessao();
        throw new ErroApi('Sua sessão expirou. Entre novamente.', 401);
    }

    const dados = await lerJson(resposta);
    if (!resposta.ok) throw new ErroApi(extrairMensagem(dados, resposta.status), resposta.status);
    return dados;
}

async function lerJson(resposta) {
    const texto = await resposta.text();
    if (!texto) return null;
    try {
        return JSON.parse(texto);
    } catch {
        return null;
    }
}

function extrairMensagem(dados, status) {
    if (dados?.errors) return Object.values(dados.errors).flat().join(' ');
    if (dados?.detail) return dados.detail;
    if (status === 403) return 'Você não tem permissão para esta ação.';
    if (status === 404) return 'Não encontrado.';
    if (status === 429) return 'Muitas tentativas em pouco tempo. Aguarde um minuto.';
    return 'Algo deu errado. Tente novamente.';
}
