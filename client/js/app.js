/* Roteador do aplicativo (SPA): cada "#/rota" da URL mostra uma página. */

const PUBLIC_ROUTES = ['login', 'registrar', 'sobre'];

// Navega para uma rota; se já estiver nela, apenas recarrega a página.
function go(hash) {
    if (location.hash === hash) route();
    else location.hash = hash;
}

function renderNav() {
    const auth = Auth.get();
    const link = (href, icone, texto) =>
        `<li class="nav-item"><a class="nav-link text-secondary px-3" href="${href}"><i class="bi ${icone} me-1"></i> ${texto}</a></li>`;

    let html = '';
    if (auth) {
        html += link('#/', 'bi-grid-1x2-fill', 'Dashboard');
        html += link('#/r/consultas', 'bi-calendar-event', 'Agenda');
        html += link('#/prontuario', 'bi-search-heart', 'Prontuário');
        html += link('#/estatisticas', 'bi-graph-up-arrow', 'Estatísticas');
    }
    html += link('#/sobre', 'bi-info-circle', 'Sobre a Clínica');

    if (auth) {
        html += `<li class="nav-item dropdown"><a class="nav-link text-secondary px-3 dropdown-toggle" href="#" data-bs-toggle="dropdown">
            <i class="bi bi-person-circle me-1"></i> ${esc(auth.usuario.nome)}</a>
            <ul class="dropdown-menu dropdown-menu-end"><li><a class="dropdown-item" href="#/sair"><i class="bi bi-box-arrow-right me-2"></i>Sair</a></li></ul></li>`;
    } else {
        html += link('#/login', 'bi-box-arrow-in-right', 'Entrar');
    }

    document.getElementById('navLinks').innerHTML = html;
}

async function route() {
    const [nome = '', arg] = location.hash.replace(/^#\/?/, '').split('/');
    const auth = Auth.get();

    if (nome === 'sair') {
        Auth.clear();
        toast('Você saiu do sistema.', 'secondary');
        go('#/login');
        return;
    }

    if (!auth && !PUBLIC_ROUTES.includes(nome)) { go('#/login'); return; }
    if (auth && (nome === 'login' || nome === 'registrar')) { go('#/'); return; }

    renderNav();
    window.scrollTo(0, 0);

    try {
        switch (nome) {
            case '': app.innerHTML = dashboardHtml(Auth.usuario()); break;
            case 'r': await renderCrud(arg); break;
            case 'prontuario': await renderProntuario(arg); break;
            case 'estatisticas': await renderEstatisticas(); break;
            case 'sobre': app.innerHTML = sobreHtml(); break;
            case 'login': renderLogin(); break;
            case 'registrar': renderRegistro(); break;
            default: app.innerHTML = notFoundHtml();
        }
    } catch (e) {
        app.innerHTML = errorHtml(e);
    }
}

window.addEventListener('hashchange', route);
window.addEventListener('DOMContentLoaded', route);
