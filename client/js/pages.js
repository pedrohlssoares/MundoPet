/* Páginas do aplicativo. As funções "...Html" são puras (recebem dados e devolvem HTML); as "render..." falam com a API. */

const app = document.getElementById('app');

function showError(e) {
    const detalhes = e.errors?.length ? ` ${e.errors.join(' ')}` : '';
    toast(`${e.message ?? 'Erro inesperado.'}${detalhes}`, 'danger');
}

const notFoundHtml = () => `<div class="alert alert-secondary"><i class="bi bi-compass me-2"></i>
    Página não encontrada. <a href="#/">Voltar ao painel</a>.</div>`;

/* ======================================================================== Formulário ===== */

function selectOptions(field, lookups) {
    if (field.options) return field.options.map((o) => (typeof o === 'string' ? { value: o, label: o } : o));
    const map = lookups[field.ref] ?? new Map();
    return Array.from(map.values()).map((item) => ({ value: item[idFieldOf(field.ref)], label: field.refLabel(item) }));
}

function fieldHtml(f, value, lookups) {
    const id = `f_${f.name.replace(/\./g, '_')}`;
    const req = f.required ? 'required' : '';
    const nome = `id="${id}" name="${esc(f.name)}"`;
    const maxlen = f.max ? `maxlength="${f.max}"` : '';
    let input;

    if (f.type === 'textarea') {
        input = `<textarea class="form-control custom-input" ${nome} rows="3" ${maxlen} ${req}>${esc(value ?? '')}</textarea>`;
    } else if (f.type === 'select') {
        const vazio = f.required ? '— selecione —' : '— nenhum —';
        const opcoes = selectOptions(f, lookups)
            .map((o) => `<option value="${esc(o.value)}" ${String(o.value) === String(value ?? '') ? 'selected' : ''}>${esc(o.label)}</option>`)
            .join('');
        input = `<select class="form-select custom-input" ${nome} ${req}><option value="">${vazio}</option>${opcoes}</select>`;
    } else {
        const tipos = { text: 'text', email: 'email', number: 'number', decimal: 'number', date: 'date', time: 'time', datetime: 'datetime-local', password: 'password' };
        let v = value ?? '';
        if (f.type === 'date') v = String(v).slice(0, 10);   // a API devolve "2026-06-04T00:00:00"
        if (f.type === 'time') v = String(v).slice(0, 5);
        if (f.type === 'datetime') v = String(v).slice(0, 16);

        const extras = [];
        if (f.type === 'decimal') extras.push('step="0.01"');
        if (f.type === 'number') extras.push('step="1"');
        if ((f.type === 'number' || f.type === 'decimal') && f.min !== undefined) extras.push(`min="${f.min}"`);
        if (f.type === 'password' && f.min) extras.push(`minlength="${f.min}"`);
        if (f.pattern) extras.push(`pattern="${esc(f.pattern)}"`);
        if (f.type === 'password') extras.push('autocomplete="new-password"');

        input = `<input class="form-control custom-input" type="${tipos[f.type] ?? 'text'}" ${nome} value="${esc(v)}" ${maxlen} ${extras.join(' ')} ${req}>`;
    }

    return `<div class="col-md-${f.col ?? 6}">
        <label class="form-label fw-semibold" for="${id}">${esc(f.label)}${f.required ? ' *' : ''}</label>
        ${input}
        ${f.hint ? `<div class="form-text">${esc(f.hint)}</div>` : ''}
    </div>`;
}

// Converte os campos do formulário para o JSON esperado pela API.
function readForm(res, form) {
    const payload = {};
    for (const f of res.fields) {
        const el = form.elements.namedItem(f.name);
        if (!el) continue;

        let v = el.value;
        if (f.type === 'number' || f.type === 'decimal') {
            v = v === '' ? null : Number(v);
        } else if (f.type === 'select') {
            v = v === '' ? null : (Number.isNaN(Number(v)) ? v : Number(v));
        } else if (f.type === 'time') {
            v = v === '' ? null : (v.length === 5 ? `${v}:00` : v);           // a API espera hh:mm:ss
        } else if (f.type === 'datetime') {
            v = v === '' ? null : (v.length === 16 ? `${v}:00` : v);
        } else if (f.type !== 'password') {
            v = v.trim();
        }
        setPath(payload, f.name, v);
    }
    return payload;
}

function openForm(res, item, lookups, onSaved) {
    const editing = item !== null;
    const modalEl = document.getElementById('formModal');
    const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
    const form = document.getElementById('modalForm');

    document.getElementById('modalTitle').textContent = `${editing ? 'Editar' : 'Novo(a)'} ${res.singular}`;
    document.getElementById('modalBody').innerHTML =
        `<div id="formErrors"></div><div class="row g-3">${res.fields
            .map((f) => fieldHtml(f, editing ? getPath(item, f.name) : f.default, lookups)).join('')}</div>`;
    form.classList.remove('was-validated');

    form.onsubmit = async (ev) => {
        ev.preventDefault();
        if (!form.checkValidity()) { form.classList.add('was-validated'); return; }

        const botao = form.querySelector('button[type=submit]');
        botao.disabled = true;
        try {
            const payload = readForm(res, form);
            if (editing) {
                await Api.put(`${res.path}/${item[idFieldOf(res.key)]}`, payload);
            } else {
                await Api.post(res.path, payload);
            }
            modal.hide();
            toast(editing ? 'Registro atualizado com sucesso.' : 'Registro criado com sucesso.');
            await onSaved();
        } catch (e) {
            document.getElementById('formErrors').innerHTML = errorHtml(e);   // mostra o ResultWrapper de erro
        } finally {
            botao.disabled = false;
        }
    };

    modal.show();
}

/* ======================================================================== Tabela ========= */

function cellParts(col, item, lookups) {
    const v = getPath(item, col.key);

    if (col.ref) {
        const alvo = lookups[col.ref]?.get(v);
        const text = alvo ? col.refLabel(alvo) : (isEmpty(v) ? '—' : `#${v}`);
        return { text, html: esc(text) };
    }
    if (col.html) return { text: String(v ?? ''), html: col.html(v, item, lookups) };

    const out = col.fmt ? col.fmt(v, item, lookups) : v;
    const text = isEmpty(out) ? '—' : String(out);
    return { text, html: esc(text) };
}

function rowText(res, item, lookups) {
    return res.columns.map((c) => cellParts(c, item, lookups).text).join(' ').toLowerCase();
}

function actionsHtml(res, item) {
    const id = item[idFieldOf(res.key)];
    let html = '';

    for (const a of res.rowActions ?? []) {
        if (a.when && !a.when(item)) continue;
        html += `<button class="btn btn-sm ${a.cls} me-1" data-action="${esc(a.action)}" data-id="${esc(id)}" title="${esc(a.label)}"><i class="bi ${a.icon}"></i></button>`;
    }
    if (!res.noEdit) {
        html += `<button class="btn btn-sm btn-outline-primary me-1" data-action="edit" data-id="${esc(id)}" title="Editar"><i class="bi bi-pencil-square"></i></button>`;
    }
    if (!res.noDelete) {
        html += `<button class="btn btn-sm btn-outline-danger" data-action="delete" data-id="${esc(id)}" title="Excluir"><i class="bi bi-trash"></i></button>`;
    }
    return html;
}

function tableHtml(res, items, lookups) {
    if (!items.length) {
        return '<div class="alert alert-info"><i class="bi bi-info-circle me-2"></i>Nenhum registro encontrado.</div>';
    }

    const cabecalho = res.columns.map((c) => `<th>${esc(c.label)}</th>`).join('');
    const linhas = items.map((item) => {
        const celulas = res.columns.map((c) => `<td>${cellParts(c, item, lookups).html}</td>`).join('');
        return `<tr>${celulas}<td class="text-end text-nowrap">${actionsHtml(res, item)}</td></tr>`;
    }).join('');

    return `<div class="table-responsive"><table class="table table-hover align-middle mb-0">
        <thead class="custom-thead text-white"><tr>${cabecalho}<th class="text-end">Ações</th></tr></thead>
        <tbody>${linhas}</tbody></table></div>`;
}

/* ======================================================================== CRUD genérico == */

async function loadLookups(res) {
    const refs = new Set();
    [...res.columns, ...res.fields].forEach((x) => { if (x.ref) refs.add(x.ref); });

    const lookups = {};
    await Promise.all([...refs].map(async (ref) => {
        try {
            const r = await Api.get(RESOURCES[ref].path);
            lookups[ref] = new Map((r.data ?? []).map((i) => [i[idFieldOf(ref)], i]));
        } catch {
            lookups[ref] = new Map();   // ex.: a lista do recurso não pôde ser carregada
        }
    }));
    return lookups;
}

async function renderCrud(key) {
    const res = RESOURCES[key];
    if (!res) { app.innerHTML = notFoundHtml(); return; }
    res.key = key;

    const state = { items: [], lookups: {}, cache: null, date: '', busca: '' };

    app.innerHTML = `
        <div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-3">
            <h2 class="fw-bold mb-0"><i class="bi ${res.icon} text-success me-2"></i>${esc(res.title)}
                <span id="cacheInfo" class="ms-2"></span></h2>
            <div class="d-flex gap-2">
                <button class="btn btn-outline-primary" id="btnAtualizar"><i class="bi bi-arrow-clockwise me-1"></i>Atualizar</button>
                <button class="btn btn-primary" id="btnNovo"><i class="bi bi-plus-lg me-1"></i>Novo(a) ${esc(res.singular)}</button>
            </div>
        </div>
        ${res.note ? `<div class="alert alert-light border"><i class="bi bi-info-circle me-2"></i>${esc(res.note)}</div>` : ''}
        <div class="row g-2 mb-3">
            <div class="col-md-6"><input type="search" class="form-control custom-input" id="busca" placeholder="Buscar na lista..."></div>
            ${res.dateFilter ? `<div class="col-md-6 d-flex gap-2">
                <input type="date" class="form-control custom-input" id="filtroData" title="Filtrar por data">
                <button class="btn btn-light" id="limparData">Limpar</button></div>` : ''}
        </div>
        <div id="tableArea">${spinnerHtml}</div>`;

    const area = document.getElementById('tableArea');

    const draw = () => {
        const termo = state.busca.trim().toLowerCase();
        const itens = termo ? state.items.filter((i) => rowText(res, i, state.lookups).includes(termo)) : state.items;
        area.innerHTML = tableHtml(res, itens, state.lookups);
        document.getElementById('cacheInfo').innerHTML = cacheBadge(state.cache);
    };

    const reload = async () => {
        area.innerHTML = spinnerHtml;
        try {
            const caminho = res.dateFilter && state.date ? `${res.path}/por-data?data=${state.date}` : res.path;
            const r = await Api.get(caminho);
            state.items = r.data ?? [];
            state.cache = r.cache;
            draw();
        } catch (e) {
            area.innerHTML = errorHtml(e);
        }
    };

    state.lookups = await loadLookups(res);
    await reload();

    document.getElementById('btnAtualizar').onclick = async () => { state.lookups = await loadLookups(res); await reload(); };
    document.getElementById('btnNovo')?.addEventListener('click', () => openForm(res, null, state.lookups, reload));
    document.getElementById('busca').oninput = (ev) => { state.busca = ev.target.value; draw(); };
    if (res.dateFilter) {
        document.getElementById('filtroData').onchange = (ev) => { state.date = ev.target.value; reload(); };
        document.getElementById('limparData').onclick = () => { document.getElementById('filtroData').value = ''; state.date = ''; reload(); };
    }

    area.onclick = async (ev) => {
        const botao = ev.target.closest('[data-action]');
        if (!botao) return;

        const { action, id } = botao.dataset;
        const item = state.items.find((i) => String(i[idFieldOf(key)]) === id);
        if (!item) return;

        try {
            if (action === 'edit') {
                openForm(res, item, state.lookups, reload);
            } else if (action === 'delete') {
                if (!confirm(`Excluir este registro de ${res.singular}?`)) return;
                const r = await Api.delete(`${res.path}/${id}`);
                toast(r.message);
                await reload();
            } else if (action.startsWith('status:')) {
                const r = await Api.patch(`${res.path}/${id}/status`, { status: action.split(':')[1] });
                toast(r.message);
                await reload();
            }
        } catch (e) {
            showError(e);   // ex.: 409 (registro em uso), 405 (operação proibida), 403 (sem permissão)
        }
    };
}

/* ======================================================================== Dashboard ====== */

function dashboardLinks(group) {
    return group.items.map((k) => {
        if (k.startsWith('@')) {
            const p = SPECIAL_PAGES[k.slice(1)];
            return { href: p.href, title: p.title, icon: p.icon };
        }
        const r = RESOURCES[k];
        return { href: `#/r/${k}`, title: r.title, icon: r.icon };
    }).filter(Boolean);
}

function dashboardHtml(usuario) {
    const blocos = DASHBOARD_GROUPS.map((g) => {
        const links = dashboardLinks(g);
        if (!links.length) return '';
        const botoes = links.map((l) => `<a href="${l.href}" class="btn btn-light dashboard-btn border text-start">
            <i class="bi ${l.icon} me-2 text-${g.color === 'warning' ? 'success' : g.color}"></i>${esc(l.title)}</a>`).join('');
        return `<div class="col-md-6 col-lg-4"><div class="card dashboard-card h-100"><div class="card-body">
            <h5 class="fw-bold mb-3"><i class="bi ${g.icon} text-${g.color} me-2"></i>${esc(g.title)}</h5>
            <div class="d-grid gap-2">${botoes}</div></div></div></div>`;
    }).join('');

    return `<div class="hero p-4 p-md-5 mb-4">
            <h1 class="fw-bold">Painel Administrativo</h1>
            <p class="mb-0 fs-5">Olá, ${esc(usuario?.nome ?? '')}! Selecione uma das opções abaixo para gerir os dados da clínica.</p>
        </div><div class="row g-4">${blocos}</div>`;
}

/* ======================================================================== Estatísticas === */

function statCard(titulo, valor, icone, cor) {
    return `<div class="col-6 col-lg-3"><div class="card stat-card h-100"><div class="card-body d-flex justify-content-between align-items-center">
        <div><div class="text-muted small">${esc(titulo)}</div><div class="stat-value">${esc(valor)}</div></div>
        <i class="bi ${icone} stat-icon text-${cor}"></i></div></div></div>`;
}

function estatisticasHtml(d) {
    const saldo = d.balancoLiquido >= 0 ? 'success' : 'danger';
    const secao = (titulo, cards) => `<h5 class="fw-bold mt-4 mb-3">${esc(titulo)}</h5><div class="row g-3">${cards.join('')}</div>`;

    return [
        secao('Financeiro', [
            statCard('Total faturado', fmtMoney(d.totalFaturado), 'bi-cash-coin', 'success'),
            statCard('Total de despesas', fmtMoney(d.totalDespesas), 'bi-receipt', 'danger'),
            statCard('Balanço líquido', fmtMoney(d.balancoLiquido), 'bi-graph-up-arrow', saldo),
            statCard('Faturamento em vendas', fmtMoney(d.faturamentoVendas), 'bi-cart-fill', 'primary')
        ]),
        secao('Clínica', [
            statCard('Clientes', fmtInt(d.totalClientes), 'bi-people-fill', 'primary'),
            statCard('Pets', fmtInt(d.totalPets), 'bi-heart-fill', 'danger'),
            statCard('Consultas', fmtInt(d.totalConsultas), 'bi-calendar-event', 'success'),
            statCard('Aplicações de vacina', fmtInt(d.totalAplicacoesVacina), 'bi-journal-medical', 'info')
        ]),
        secao('Estoque', [
            statCard('Produtos cadastrados', fmtInt(d.totalProdutos), 'bi-box-seam-fill', 'secondary'),
            statCard('Itens em estoque', fmtInt(d.estoqueTotalProdutos), 'bi-boxes', 'secondary'),
            statCard('Itens vendidos', fmtInt(d.qtdItensVendidos), 'bi-bag-check-fill', 'success'),
            statCard('Remédios / Vacinas', `${fmtInt(d.totalRemedios)} / ${fmtInt(d.totalVacinas)}`, 'bi-capsule', 'info')
        ])
    ].join('');
}

async function renderEstatisticas() {
    app.innerHTML = `<div class="d-flex flex-wrap justify-content-between align-items-center gap-2 mb-2">
            <h2 class="fw-bold mb-0"><i class="bi bi-graph-up-arrow text-success me-2"></i>Estatísticas <span id="cacheInfo" class="ms-2"></span></h2>
            <button class="btn btn-outline-primary" id="btnAtualizar"><i class="bi bi-arrow-clockwise me-1"></i>Atualizar</button>
        </div>
        <p class="text-muted">Totais calculados no banco a cada vez que a tela é aberta ou atualizada.</p>
        <div id="statsArea">${spinnerHtml}</div>`;

    const carregar = async () => {
        try {
            const r = await Api.get('/api/estatisticas');
            document.getElementById('statsArea').innerHTML = estatisticasHtml(r.data);
            document.getElementById('cacheInfo').innerHTML = cacheBadge(r.cache);
        } catch (e) {
            document.getElementById('statsArea').innerHTML = errorHtml(e);
        }
    };

    document.getElementById('btnAtualizar').onclick = carregar;
    await carregar();
}

/* ======================================================================== Prontuário ===== */

function historicoHtml(h) {
    const p = h.pet;
    const tabela = (cabecalho, linhas) => linhas.length
        ? `<div class="table-responsive"><table class="table table-sm align-middle mb-0"><thead class="custom-thead text-white"><tr>${cabecalho.map((c) => `<th>${c}</th>`).join('')}</tr></thead><tbody>${linhas.join('')}</tbody></table></div>`
        : '<div class="text-muted">Nenhum registro.</div>';

    const consultas = h.consultas.map((c) => `<tr><td>${fmtDate(c.dataConsulta)}</td><td>${fmtTime(c.horario)}</td>
        <td>${statusBadge(c.status)}</td><td>${esc(c.salaNumero ? `Sala ${c.salaNumero} - ${c.salaTipo ?? ''}` : '—')}</td>
        <td>${esc(c.processosFeitos || '—')}</td></tr>`);
    const vacinas = h.vacinas.map((v) => `<tr><td>${fmtDate(v.dataAplicacao)}</td><td>${esc(v.produto)}</td>
        <td>${esc(v.ativo)}</td><td>${esc(v.dosagem)}</td></tr>`);
    const vendas = h.vendas.map((v) => `<tr><td>${fmtDateTime(v.dataPagamento)}</td><td>${esc(v.produto)}</td>
        <td>${fmtInt(v.quantidade)}</td><td>${fmtMoney(v.valorUnitario)}</td><td>${fmtMoney(v.subtotal)}</td></tr>`);

    const bloco = (titulo, icone, conteudo) => `<div class="card dashboard-card mb-4"><div class="card-body">
        <h5 class="fw-bold mb-3"><i class="bi ${icone} text-success me-2"></i>${titulo}</h5>${conteudo}</div></div>`;

    return `<a href="#/prontuario" class="btn btn-light mb-3"><i class="bi bi-arrow-left me-1"></i>Nova busca</a>
        <div class="row g-3 mb-4">
            <div class="col-lg-8"><div class="card dashboard-card h-100"><div class="card-body">
                <h3 class="fw-bold">🐾 ${esc(p.nome)} <small class="text-muted fs-6">#${p.id}</small></h3>
                <div class="text-muted mb-2">${esc(p.especie)} · ${esc(p.raca)} · ${esc(p.idade)}</div>
                <div><i class="bi bi-person-fill me-1"></i>Tutor: <strong>${esc(p.tutorNome)}</strong>
                    · CPF ${fmtCpf(p.tutorCpf)} · <i class="bi bi-telephone me-1"></i>${esc(p.tutorCelular)}</div>
            </div></div></div>
            <div class="col-lg-4"><div class="card stat-card h-100"><div class="card-body text-center">
                <div class="text-muted small">Total gasto na clínica</div>
                <div class="stat-value text-success">${fmtMoney(h.totalGasto)}</div></div></div></div>
        </div>
        ${bloco('Consultas', 'bi-calendar-event', tabela(['Data', 'Horário', 'Status', 'Sala', 'Processos feitos'], consultas))}
        ${bloco('Vacinas aplicadas', 'bi-shield-plus', tabela(['Data', 'Produto', 'Princípio ativo', 'Dosagem'], vacinas))}
        ${bloco('Compras vinculadas às consultas', 'bi-cart-fill', tabela(['Data', 'Produto', 'Qtd.', 'Valor unit.', 'Subtotal'], vendas))}`;
}

async function renderProntuario(idArg) {
    if (idArg) {
        app.innerHTML = spinnerHtml;
        const r = await Api.get(`/api/pets/${encodeURIComponent(idArg)}/historico`);
        app.innerHTML = historicoHtml(r.data);
        return;
    }

    app.innerHTML = `<h2 class="fw-bold mb-3"><i class="bi bi-search-heart text-success me-2"></i>Prontuário do Pet</h2>
        <p class="text-muted">Busque pelo nome ou pelo número (ID) do pet para ver consultas, vacinas e compras.</p>
        <form id="buscaPet" class="row g-2 mb-4"><div class="col-md-8"><input class="form-control custom-input" id="termo" placeholder="Nome ou ID do pet" required></div>
        <div class="col-md-4"><button class="btn btn-primary w-100"><i class="bi bi-search me-1"></i>Buscar</button></div></form>
        <div id="resultados"></div>`;

    document.getElementById('buscaPet').onsubmit = async (ev) => {
        ev.preventDefault();
        const alvo = document.getElementById('resultados');
        alvo.innerHTML = spinnerHtml;
        try {
            const r = await Api.get(`/api/pets/buscar?termo=${encodeURIComponent(document.getElementById('termo').value)}`);
            const pets = r.data ?? [];
            alvo.innerHTML = pets.length
                ? `<div class="list-group">${pets.map((p) => `<a class="list-group-item list-group-item-action" href="#/prontuario/${p.id}">
                    <strong>🐾 ${esc(p.nome)}</strong> <span class="text-muted">#${p.id} · ${esc(p.especie)} · ${esc(p.raca)}</span></a>`).join('')}</div>`
                : '<div class="alert alert-info">Nenhum pet encontrado.</div>';
        } catch (e) {
            alvo.innerHTML = errorHtml(e);
        }
    };
}

/* ======================================================================== Login / cadastro / sobre */

function authCardHtml(titulo, subtitulo, formHtml, rodape) {
    return `<div class="card auth-card"><div class="card-body p-4 p-md-5">
        <div class="text-center mb-4"><div class="fs-1">🐾</div><h3 class="fw-bold mb-1">${titulo}</h3><div class="text-muted">${subtitulo}</div></div>
        <div id="authErrors"></div>${formHtml}
        <div class="text-center mt-3 small">${rodape}</div></div></div>`;
}

function renderLogin() {
    app.innerHTML = authCardHtml('Entrar', 'Acesse o sistema da clínica', `
        <form id="loginForm" novalidate>
            <div class="mb-3"><label class="form-label fw-semibold" for="email">E-mail</label>
                <input class="form-control custom-input" type="email" id="email" required autocomplete="username"></div>
            <div class="mb-3"><label class="form-label fw-semibold" for="senha">Senha</label>
                <input class="form-control custom-input" type="password" id="senha" required autocomplete="current-password"></div>
            <button class="btn btn-primary w-100" type="submit"><i class="bi bi-box-arrow-in-right me-1"></i>Entrar</button>
        </form>`, 'Não tem conta? <a href="#/registrar">Cadastre-se</a>');

    const form = document.getElementById('loginForm');
    form.onsubmit = async (ev) => {
        ev.preventDefault();
        if (!form.checkValidity()) { form.classList.add('was-validated'); return; }

        const botao = form.querySelector('button');
        botao.disabled = true;
        try {
            const r = await Api.post('/api/auth/login', {
                email: document.getElementById('email').value,
                senha: document.getElementById('senha').value
            });
            Auth.save(r.data);                         // guarda o token JWT
            toast(`Bem-vindo(a), ${r.data.usuario.nome}!`);
            go('#/');
        } catch (e) {
            document.getElementById('authErrors').innerHTML = errorHtml(e);
        } finally {
            botao.disabled = false;
        }
    };
}

function renderRegistro() {
    app.innerHTML = authCardHtml('Criar conta', 'Cadastro de novo usuário', `
        <form id="registroForm" novalidate>
            <div class="mb-3"><label class="form-label fw-semibold" for="nome">Nome</label>
                <input class="form-control custom-input" id="nome" maxlength="90" required></div>
            <div class="mb-3"><label class="form-label fw-semibold" for="email">E-mail</label>
                <input class="form-control custom-input" type="email" id="email" maxlength="60" required autocomplete="username"></div>
            <div class="mb-3"><label class="form-label fw-semibold" for="senha">Senha</label>
                <input class="form-control custom-input" type="password" id="senha" minlength="6" maxlength="100" required autocomplete="new-password">
                <div class="form-text">Mínimo de 6 caracteres.</div></div>
            <div class="mb-3"><label class="form-label fw-semibold" for="senha2">Confirmar senha</label>
                <input class="form-control custom-input" type="password" id="senha2" required autocomplete="new-password"></div>
            <button class="btn btn-primary w-100" type="submit"><i class="bi bi-person-plus me-1"></i>Cadastrar</button>
        </form>`, 'Já tem conta? <a href="#/login">Entrar</a>');

    const form = document.getElementById('registroForm');
    form.onsubmit = async (ev) => {
        ev.preventDefault();
        if (!form.checkValidity()) { form.classList.add('was-validated'); return; }
        if (document.getElementById('senha').value !== document.getElementById('senha2').value) {
            document.getElementById('authErrors').innerHTML = errorHtml({ message: 'As senhas não conferem.' });
            return;
        }

        const botao = form.querySelector('button');
        botao.disabled = true;
        try {
            await Api.post('/api/auth/registrar', {
                nome: document.getElementById('nome').value,
                email: document.getElementById('email').value,
                senha: document.getElementById('senha').value
            });
            toast('Conta criada com sucesso! Faça login para continuar.');
            go('#/login');
        } catch (e) {
            document.getElementById('authErrors').innerHTML = errorHtml(e);
        } finally {
            botao.disabled = false;
        }
    };
}

const sobreHtml = () => `
    <div class="hero p-4 p-md-5 mb-4 text-center"><h1 class="fw-bold">Sobre a MUNDO PET</h1>
        <p class="mb-0 fs-5">Cuidando do seu melhor amigo com o respeito, dedicação e o profissionalismo que ele merece.</p></div>
    <div class="row g-4 mb-4">
        <div class="col-lg-7"><div class="card dashboard-card h-100"><div class="card-body">
            <h4 class="fw-bold">Nossa História</h4>
            <p>Fundada com o objetivo de centralizar todos os cuidados necessários para a saúde e bem-estar animal, a Clínica Veterinária Mundo Pet nasceu da paixão pelos animais. Oferecemos um atendimento humanizado, contando com profissionais altamente qualificados e uma infraestrutura moderna.</p>
            <p class="mb-0">Mais do que uma clínica, somos um espaço de suporte para tutores, garantindo desde consultas de rotina até procedimentos complexos, sempre priorizando a ética e o amor em cada atendimento.</p>
        </div></div></div>
        <div class="col-lg-5"><div class="card dashboard-card h-100"><div class="card-body">
            <h4 class="fw-bold">Nosso Compromisso</h4>
            <p>Oferecer soluções completas em saúde animal através de um ecossistema integrado que une ótimos veterinários, atendimento ágil e medicamentos de confiança.</p>
            <div><i class="bi bi-award-fill text-success me-2"></i><strong>Excelência</strong> — corpo clínico especializado.</div>
            <div><i class="bi bi-heart-fill text-danger me-2"></i><strong>Carinho &amp; Afeto</strong> — cada pet é da família.</div>
            <div><i class="bi bi-lightning-charge-fill text-warning me-2"></i><strong>Praticidade</strong> — painel integrado de consultas, remédios e histórico.</div>
        </div></div></div>
    </div>
    <div class="card dashboard-card"><div class="card-body"><h5 class="fw-bold">Contato &amp; Localização</h5>
        <div><i class="bi bi-geo-alt-fill me-2"></i>Muriaé, Minas Gerais - Brasil</div>
        <div><i class="bi bi-telephone-fill me-2"></i>(32) 99999-0000</div>
        <div><i class="bi bi-envelope-fill me-2"></i>contato@mundopet.com.br</div></div></div>`;
