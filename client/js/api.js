/*
 * Cliente HTTP da API.
 * Toda resposta da API segue o ResultWrapper: { success, message, data, errors }.
 * Aqui ele é "desembrulhado": em caso de sucesso devolve { data, message, cache };
 * em caso de falha lança ApiError com message e errors, para as telas mostrarem ao usuário.
 */
class ApiError extends Error {
    constructor(message, errors = [], status = 0) {
        super(message);
        this.errors = errors;
        this.status = status;
    }
}

const Api = {
    base: (window.APP_CONFIG?.apiUrl ?? '').replace(/\/$/, ''),

    async request(method, path, body) {
        const headers = { Accept: 'application/json' };

        const auth = Auth.get();
        if (auth) headers.Authorization = `Bearer ${auth.token}`;   // token JWT nas requisições protegidas

        const options = { method, headers };
        if (body !== undefined) {
            headers['Content-Type'] = 'application/json';
            options.body = JSON.stringify(body);
        }

        let resp;
        try {
            resp = await fetch(this.base + path, options);
        } catch {
            throw new ApiError('Não foi possível conectar à API.',
                [`Verifique se ela está em execução em ${this.base || 'este endereço'}.`], 0);
        }

        let result = null;
        try { result = await resp.json(); } catch { /* resposta sem corpo JSON */ }

        // Sessão expirada/inválida: volta para o login (exceto nas próprias telas de login/cadastro)
        if (resp.status === 401 && !path.startsWith('/api/auth/')) {
            Auth.clear();
            location.hash = '#/login';
        }

        if (!resp.ok || !result || result.success === false) {
            throw new ApiError(result?.message ?? `Erro ${resp.status} ao acessar a API.`, result?.errors ?? [], resp.status);
        }

        return { data: result.data, message: result.message, cache: resp.headers.get('X-Cache') };
    },

    get(path) { return this.request('GET', path); },
    post(path, body) { return this.request('POST', path, body ?? {}); },
    put(path, body) { return this.request('PUT', path, body ?? {}); },
    patch(path, body) { return this.request('PATCH', path, body ?? {}); },
    delete(path) { return this.request('DELETE', path); }
};
