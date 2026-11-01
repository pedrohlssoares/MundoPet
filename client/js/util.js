/* Funções utilitárias (formatação, escape de HTML, mensagens). */

// Escapa texto antes de colocar em innerHTML (evita XSS com dados vindos da API).
const esc = (v) => String(v ?? '').replace(/[&<>"']/g, (c) =>
    ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));

// Acesso a propriedades aninhadas: getPath(cliente, 'endereco.rua')
const getPath = (obj, path) => path.split('.').reduce((o, k) => (o == null ? undefined : o[k]), obj);

function setPath(obj, path, value) {
    const keys = path.split('.');
    let o = obj;
    keys.slice(0, -1).forEach((k) => { o[k] = o[k] ?? {}; o = o[k]; });
    o[keys[keys.length - 1]] = value;
    return obj;
}

const isEmpty = (v) => v === null || v === undefined || v === '';

const fmtMoney = (n) => (isEmpty(n) ? '—' : Number(n).toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' }));

const fmtDate = (s) => {
    const m = /^(\d{4})-(\d{2})-(\d{2})/.exec(s ?? '');
    return m ? `${m[3]}/${m[2]}/${m[1]}` : (isEmpty(s) ? '—' : s);
};

const fmtTime = (s) => (isEmpty(s) ? '—' : String(s).slice(0, 5));

const fmtDateTime = (s) => {
    const m = /^(\d{4})-(\d{2})-(\d{2})T(\d{2}):(\d{2})/.exec(s ?? '');
    return m ? `${m[3]}/${m[2]}/${m[1]} ${m[4]}:${m[5]}` : (isEmpty(s) ? '—' : s);
};

const fmtCpf = (s) => (/^\d{11}$/.test(s ?? '')
    ? s.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4')
    : (isEmpty(s) ? '—' : s));

const fmtInt = (n) => (isEmpty(n) ? '—' : Number(n).toLocaleString('pt-BR'));

function statusBadge(status) {
    const cores = { Agendada: 'primary', Realizada: 'success', Cancelada: 'secondary' };
    return `<span class="badge bg-${cores[status] ?? 'light text-dark'}">${esc(status)}</span>`;
}

function cacheBadge(cache) {
    if (cache === 'HIT') {
        return '<span class="badge bg-success badge-cache" title="Dados devolvidos do cache do servidor (sem consultar o banco)"><i class="bi bi-lightning-charge-fill"></i> cache HIT</span>';
    }
    if (cache === 'MISS') {
        return '<span class="badge bg-secondary badge-cache" title="Dados buscados no banco e guardados no cache"><i class="bi bi-database"></i> cache MISS</span>';
    }
    return '';
}

// Mensagens temporárias no canto da tela.
function toast(message, type = 'success') {
    const container = document.getElementById('toasts');
    const el = document.createElement('div');
    el.className = `toast align-items-center text-bg-${type} border-0`;
    el.setAttribute('role', 'alert');
    el.innerHTML = `<div class="d-flex"><div class="toast-body">${esc(message)}</div>
        <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Fechar"></button></div>`;
    container.appendChild(el);
    const t = new bootstrap.Toast(el, { delay: 4500 });
    el.addEventListener('hidden.bs.toast', () => el.remove());
    t.show();
}

// Junta a mensagem e a lista de erros do ResultWrapper em um bloco de alerta.
function errorHtml(err) {
    const lista = (err.errors ?? []).map((e) => `<li>${esc(e)}</li>`).join('');
    return `<div class="alert alert-danger"><strong>${esc(err.message ?? 'Erro inesperado.')}</strong>${lista ? `<ul class="mb-0 mt-2">${lista}</ul>` : ''}</div>`;
}

const spinnerHtml = '<div class="text-center py-5"><div class="spinner-border text-success" role="status"></div><div class="mt-2 text-muted">Carregando...</div></div>';
