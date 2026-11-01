/*
 * Guarda e consulta o token JWT no navegador (localStorage).
 * O token é enviado pelo api.js no header "Authorization: Bearer <token>" em toda requisição.
 * Observação de segurança: localStorage fica exposto a ataques XSS. Por isso todo conteúdo vindo
 * da API é escapado (esc) antes de ir para a tela. Em produção, uma alternativa seria cookie HttpOnly.
 */
const Auth = {
    KEY: 'mundopet.auth',

    save(resposta) {
        localStorage.setItem(this.KEY, JSON.stringify({
            token: resposta.token,
            expiraEm: resposta.expiraEm,
            usuario: resposta.usuario
        }));
    },

    // Devolve null se não houver login ou se o token já expirou.
    get() {
        try {
            const auth = JSON.parse(localStorage.getItem(this.KEY));
            if (!auth || !auth.token) return null;
            if (new Date(auth.expiraEm) <= new Date()) { this.clear(); return null; }
            return auth;
        } catch {
            return null;
        }
    },

    clear() { localStorage.removeItem(this.KEY); },

    usuario() { return this.get()?.usuario ?? null; }
};
