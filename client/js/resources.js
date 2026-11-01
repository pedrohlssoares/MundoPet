/*
 * Cada recurso da API é descrito aqui de forma declarativa (rota, colunas da tabela, campos do formulário).
 * O motor genérico de pages.js usa estas definições para montar a listagem, o formulário e as chamadas HTTP,
 * sem repetir código para as 15 telas de cadastro.
 *
 * Opções do recurso:  path, title, singular, icon, group, idField (padrão "id"),
 *   columns[], fields[], noEdit, noDelete,
 *   dateFilter (filtro por data), rowActions[], note (aviso exibido no topo).
 * Opções de coluna:   label, key (aceita "a.b"), fmt(valor, item), html(valor, item), ref + refLabel (mostra o texto do registro ligado).
 * Opções de campo:    name (aceita "a.b"), label, type (text|email|number|decimal|date|time|datetime|textarea|select|password),
 *                     required, max, min, hint, pattern, col (largura 1-12), default, options (select fixo), ref + refLabel (select dinâmico).
 */

const label = {
    cliente: (c) => c.nome,
    pet: (p) => p.nome,
    funcionario: (f) => f.nome,
    veterinario: (v) => v.nome ?? `Veterinário #${v.id}`,
    sala: (s) => `Sala ${s.numero} - ${s.tipo}`,
    produto: (p) => p.nome,
    remedio: (r) => `${r.ativo} (lote ${r.lote})`,
    vacina: (v) => `${v.ativo} (lote ${v.lote})`,
    consulta: (c) => `#${c.id} - ${fmtDate(c.dataConsulta)} ${fmtTime(c.horario)}`,
    venda: (v) => `Venda #${v.id}`,
    forma: (f) => f.tipo
};

const RESOURCES = {
    clientes: {
        path: '/api/clientes', title: 'Clientes', singular: 'Cliente', icon: 'bi-people-fill', group: 'Cadastros',
        columns: [
            { label: 'Nome', key: 'nome' },
            { label: 'CPF', key: 'cpf', fmt: fmtCpf },
            { label: 'Celular', key: 'celular' },
            { label: 'E-mail', key: 'email' },
            { label: 'Cidade', key: 'endereco.cidade' }
        ],
        fields: [
            { name: 'nome', label: 'Nome completo', type: 'text', required: true, max: 90 },
            { name: 'cpf', label: 'CPF', type: 'text', required: true, max: 11, pattern: '\\d{11}', hint: '11 números, sem pontos', col: 3 },
            { name: 'celular', label: 'Celular', type: 'text', required: true, max: 11, pattern: '\\d{10,11}', hint: 'DDD + número, só números', col: 3 },
            { name: 'email', label: 'E-mail', type: 'email', required: true, max: 60 },
            { name: 'endereco.rua', label: 'Rua', type: 'text', required: true, max: 90, col: 8 },
            { name: 'endereco.numero', label: 'Número', type: 'text', required: true, max: 45, col: 4 },
            { name: 'endereco.bairro', label: 'Bairro', type: 'text', required: true, max: 45, col: 4 },
            { name: 'endereco.cidade', label: 'Cidade', type: 'text', required: true, max: 45, col: 4 },
            { name: 'endereco.complemento', label: 'Complemento', type: 'text', max: 45, col: 4 }
        ]
    },

    pets: {
        path: '/api/pets', title: 'Pets', singular: 'Pet', icon: 'bi-heart-fill', group: 'Cadastros',
        columns: [
            { label: 'Nome', key: 'nome' },
            { label: 'Espécie', key: 'especie' },
            { label: 'Raça', key: 'raca' },
            { label: 'Idade', key: 'idade' },
            { label: 'Tutor', key: 'clienteId', ref: 'clientes', refLabel: label.cliente }
        ],
        fields: [
            { name: 'nome', label: 'Nome do pet', type: 'text', required: true, max: 45 },
            { name: 'clienteId', label: 'Tutor (cliente)', type: 'select', required: true, ref: 'clientes', refLabel: label.cliente },
            { name: 'especie', label: 'Espécie', type: 'text', required: true, max: 45, col: 4 },
            { name: 'raca', label: 'Raça', type: 'text', required: true, max: 45, col: 4 },
            { name: 'idade', label: 'Idade', type: 'text', required: true, max: 45, hint: 'Ex.: 3 anos', col: 4 }
        ]
    },

    funcionarios: {
        path: '/api/funcionarios', title: 'Funcionários', singular: 'Funcionário', icon: 'bi-person-badge-fill',
        group: 'Cadastros',
        columns: [
            { label: 'Nome', key: 'nome' },
            { label: 'CPF', key: 'cpf', fmt: fmtCpf },
            { label: 'Celular', key: 'celular' },
            { label: 'Salário', key: 'salario', fmt: fmtMoney },
            { label: 'Cidade', key: 'endereco.cidade' }
        ],
        fields: [
            { name: 'nome', label: 'Nome completo', type: 'text', required: true, max: 90 },
            { name: 'salario', label: 'Salário (R$)', type: 'decimal', required: true, min: 0, col: 3 },
            { name: 'cpf', label: 'CPF', type: 'text', required: true, max: 11, pattern: '\\d{11}', hint: '11 números', col: 3 },
            { name: 'celular', label: 'Celular', type: 'text', required: true, max: 11, pattern: '\\d{10,11}', hint: 'DDD + número', col: 6 },
            { name: 'endereco.rua', label: 'Rua', type: 'text', required: true, max: 90, col: 8 },
            { name: 'endereco.numero', label: 'Número', type: 'text', required: true, max: 45, col: 4 },
            { name: 'endereco.bairro', label: 'Bairro', type: 'text', required: true, max: 45, col: 4 },
            { name: 'endereco.cidade', label: 'Cidade', type: 'text', required: true, max: 45, col: 4 },
            { name: 'endereco.complemento', label: 'Complemento', type: 'text', max: 45, col: 4 }
        ]
    },

    veterinarios: {
        path: '/api/veterinarios', title: 'Veterinários', singular: 'Veterinário', icon: 'bi-clipboard2-pulse-fill',
        group: 'Cadastros',
        note: 'Veterinários são funcionários já cadastrados.',
        columns: [
            { label: 'Nome', key: 'nome' },
            { label: 'CRMV', key: 'crmv' },
            { label: 'Descrição', key: 'descricao' }
        ],
        fields: [
            { name: 'funcionarioId', label: 'Funcionário', type: 'select', required: true, ref: 'funcionarios', refLabel: label.funcionario },
            { name: 'crmv', label: 'CRMV', type: 'text', required: true, max: 11 },
            { name: 'descricao', label: 'Descrição / especialidade', type: 'textarea', max: 400, col: 12 }
        ]
    },

    salas: {
        path: '/api/salas', title: 'Salas', singular: 'Sala', icon: 'bi-door-open-fill', group: 'Cadastros', idField: 'numero',
        columns: [
            { label: 'Número', key: 'numero' },
            { label: 'Tipo', key: 'tipo' },
            { label: 'Descrição', key: 'descricao' }
        ],
        fields: [
            { name: 'tipo', label: 'Tipo', type: 'text', required: true, max: 45, hint: 'Ex.: Consultório, Cirurgia' },
            { name: 'descricao', label: 'Descrição', type: 'textarea', required: true, max: 400, col: 12 }
        ]
    },

    produtos: {
        path: '/api/produtos', title: 'Produtos', singular: 'Produto', icon: 'bi-box-seam-fill', group: 'Cadastros',
        columns: [
            { label: 'Nome', key: 'nome' },
            { label: 'Estoque', key: 'quantidade' },
            { label: 'Preço', key: 'preco', fmt: fmtMoney },
            { label: 'Descrição', key: 'descricao' }
        ],
        fields: [
            { name: 'nome', label: 'Nome', type: 'text', required: true, max: 45 },
            { name: 'quantidade', label: 'Quantidade em estoque', type: 'number', required: true, min: 0, col: 3 },
            { name: 'preco', label: 'Preço (R$)', type: 'decimal', required: true, min: 0, col: 3 },
            { name: 'descricao', label: 'Descrição', type: 'textarea', required: true, max: 400, col: 12 }
        ]
    },

    remedios: {
        path: '/api/remedios', title: 'Remédios', singular: 'Remédio', icon: 'bi-capsule', group: 'Cadastros',
        columns: [
            { label: 'Produto', key: 'produtoId', ref: 'produtos', refLabel: label.produto },
            { label: 'Princípio ativo', key: 'ativo' },
            { label: 'Lote', key: 'lote' }
        ],
        fields: [
            { name: 'produtoId', label: 'Produto', type: 'select', required: true, ref: 'produtos', refLabel: label.produto },
            { name: 'ativo', label: 'Princípio ativo', type: 'text', required: true, max: 40, col: 4 },
            { name: 'lote', label: 'Lote', type: 'text', required: true, max: 45, col: 2 }
        ]
    },

    vacinas: {
        path: '/api/vacinas', title: 'Vacinas', singular: 'Vacina', icon: 'bi-shield-plus', group: 'Cadastros',
        columns: [
            { label: 'Produto', key: 'produtoId', ref: 'produtos', refLabel: label.produto },
            { label: 'Princípio ativo', key: 'ativo' },
            { label: 'Lote', key: 'lote' }
        ],
        fields: [
            { name: 'produtoId', label: 'Produto', type: 'select', required: true, ref: 'produtos', refLabel: label.produto },
            { name: 'ativo', label: 'Princípio ativo', type: 'text', required: true, max: 45, col: 4 },
            { name: 'lote', label: 'Lote', type: 'text', required: true, max: 10, col: 2 }
        ]
    },

    consultas: {
        path: '/api/consultas', title: 'Agenda de Consultas', singular: 'Consulta', icon: 'bi-calendar-event', group: 'Atendimento',
        noDelete: true, dateFilter: true,
        note: 'Consultas não podem ser excluídas (auditoria clínica): para desistir, mude o status para Cancelada. ' +
              'Um veterinário ou uma sala não podem ter duas consultas no mesmo dia e horário.',
        columns: [
            { label: 'Data', key: 'dataConsulta', fmt: fmtDate },
            { label: 'Horário', key: 'horario', fmt: fmtTime },
            { label: 'Pet', key: 'petId', ref: 'pets', refLabel: label.pet },
            { label: 'Veterinário', key: 'veterinarioId', ref: 'veterinarios', refLabel: label.veterinario },
            { label: 'Sala', key: 'salaId', ref: 'salas', refLabel: label.sala },
            { label: 'Status', key: 'status', html: (v) => statusBadge(v) },
            { label: 'Processos feitos', key: 'processosFeitos' }
        ],
        fields: [
            { name: 'petId', label: 'Pet', type: 'select', required: true, ref: 'pets', refLabel: label.pet },
            { name: 'veterinarioId', label: 'Veterinário', type: 'select', required: true, ref: 'veterinarios', refLabel: label.veterinario },
            { name: 'salaId', label: 'Sala', type: 'select', required: true, ref: 'salas', refLabel: label.sala },
            { name: 'dataConsulta', label: 'Data', type: 'date', required: true, col: 3 },
            { name: 'horario', label: 'Horário', type: 'time', required: true, col: 3 },
            { name: 'status', label: 'Status', type: 'select', required: true, default: 'Agendada',
              options: ['Agendada', 'Realizada', 'Cancelada'], col: 6 },
            { name: 'processosFeitos', label: 'Processos feitos / observações', type: 'textarea', max: 400, col: 12 }
        ],
        rowActions: [
            { action: 'status:Realizada', label: 'Realizada', icon: 'bi-check-circle', cls: 'btn-outline-success', when: (i) => i.status !== 'Realizada' },
            { action: 'status:Cancelada', label: 'Cancelar', icon: 'bi-x-circle', cls: 'btn-outline-danger', when: (i) => i.status !== 'Cancelada' },
            { action: 'status:Agendada', label: 'Reagendar', icon: 'bi-arrow-counterclockwise', cls: 'btn-outline-secondary', when: (i) => i.status !== 'Agendada' }
        ]
    },

    prescricoes: {
        path: '/api/prescricoes', title: 'Prescrições', singular: 'Prescrição', icon: 'bi-file-earmark-medical-fill',
        group: 'Atendimento', noDelete: true,
        note: 'Prescrições médicas não podem ser excluídas (auditoria clínica).',
        columns: [
            { label: 'Consulta', key: 'consultaId', ref: 'consultas', refLabel: label.consulta },
            { label: 'Remédio', key: 'remedioId', ref: 'remedios', refLabel: label.remedio },
            { label: 'Vacina', key: 'vacinaId', ref: 'vacinas', refLabel: label.vacina },
            { label: 'Dosagem', key: 'dosagem' }
        ],
        fields: [
            { name: 'consultaId', label: 'Consulta', type: 'select', required: true, ref: 'consultas', refLabel: label.consulta, col: 12 },
            { name: 'remedioId', label: 'Remédio (opcional)', type: 'select', ref: 'remedios', refLabel: label.remedio },
            { name: 'vacinaId', label: 'Vacina (opcional)', type: 'select', ref: 'vacinas', refLabel: label.vacina },
            { name: 'dosagem', label: 'Dosagem / instruções', type: 'textarea', required: true, max: 400, col: 12 }
        ]
    },

    historicoVacinacao: {
        path: '/api/historico-vacinacao', title: 'Histórico de Vacinação', singular: 'Aplicação de vacina',
        icon: 'bi-journal-medical', group: 'Atendimento',
        columns: [
            { label: 'Data', key: 'dataAplicacao', fmt: fmtDate },
            { label: 'Pet', key: 'petId', ref: 'pets', refLabel: label.pet },
            { label: 'Vacina', key: 'vacinaId', ref: 'vacinas', refLabel: label.vacina },
            { label: 'Dosagem', key: 'dosagem' }
        ],
        fields: [
            { name: 'petId', label: 'Pet', type: 'select', required: true, ref: 'pets', refLabel: label.pet },
            { name: 'vacinaId', label: 'Vacina', type: 'select', required: true, ref: 'vacinas', refLabel: label.vacina },
            { name: 'dataAplicacao', label: 'Data de aplicação', type: 'date', required: true, col: 4 },
            { name: 'dosagem', label: 'Dosagem', type: 'text', required: true, max: 45, col: 8 }
        ]
    },

    vendas: {
        path: '/api/vendas', title: 'Vendas', singular: 'Venda', icon: 'bi-cart-fill', group: 'Financeiro',
        columns: [
            { label: 'Nº', key: 'id' },
            { label: 'Produto', key: 'produtoId', ref: 'produtos', refLabel: label.produto },
            { label: 'Qtd.', key: 'quantidade' },
            { label: 'Valor unit.', key: 'valorUnitario', fmt: fmtMoney },
            { label: 'Total', key: 'id', fmt: (v, i) => fmtMoney(i.quantidade * i.valorUnitario) }
        ],
        fields: [
            { name: 'produtoId', label: 'Produto', type: 'select', required: true, ref: 'produtos', refLabel: label.produto },
            { name: 'quantidade', label: 'Quantidade', type: 'number', required: true, min: 1, col: 3 },
            { name: 'valorUnitario', label: 'Valor unitário (R$)', type: 'decimal', required: true, min: 0.01, col: 3 }
        ]
    },

    pagamentos: {
        path: '/api/pagamentos', title: 'Pagamentos', singular: 'Pagamento', icon: 'bi-cash-coin', group: 'Financeiro',
        columns: [
            { label: 'Data', key: 'dataPagamento', fmt: fmtDateTime },
            { label: 'Cliente', key: 'clienteId', ref: 'clientes', refLabel: label.cliente },
            { label: 'Valor', key: 'valor', fmt: fmtMoney },
            { label: 'Parcelas', key: 'prestacoes' },
            { label: 'Forma', key: 'formaPagamentoId', ref: 'formasPagamento', refLabel: label.forma }
        ],
        fields: [
            { name: 'clienteId', label: 'Cliente', type: 'select', required: true, ref: 'clientes', refLabel: label.cliente },
            { name: 'formaPagamentoId', label: 'Forma de pagamento', type: 'select', required: true, ref: 'formasPagamento', refLabel: label.forma },
            { name: 'valor', label: 'Valor (R$)', type: 'decimal', required: true, min: 0.01, col: 4 },
            { name: 'prestacoes', label: 'Parcelas', type: 'number', required: true, min: 1, default: 1, col: 2 },
            { name: 'dataPagamento', label: 'Data/hora (opcional)', type: 'datetime', hint: 'Em branco = agora', col: 6 },
            { name: 'consultaId', label: 'Consulta (opcional)', type: 'select', ref: 'consultas', refLabel: label.consulta, col: 4 },
            { name: 'vendaId', label: 'Venda (opcional)', type: 'select', ref: 'vendas', refLabel: label.venda, col: 4 }
        ]
    },

    formasPagamento: {
        path: '/api/formas-pagamento', title: 'Formas de Pagamento', singular: 'Forma de pagamento', icon: 'bi-credit-card-fill',
        group: 'Financeiro',
        columns: [
            { label: 'Tipo', key: 'tipo' },
            { label: 'Descrição', key: 'descricao' }
        ],
        fields: [
            { name: 'tipo', label: 'Tipo', type: 'text', required: true, max: 45, hint: 'Ex.: PIX, Cartão de crédito' },
            { name: 'descricao', label: 'Descrição', type: 'textarea', required: true, max: 200, col: 12 }
        ]
    },

    despesas: {
        path: '/api/despesas', title: 'Despesas', singular: 'Despesa', icon: 'bi-receipt', group: 'Financeiro',
        noEdit: true,
        note: 'Uma despesa lançada não pode ser alterada. Se errar, exclua e lance novamente.',
        columns: [
            { label: 'Data', key: 'data', fmt: fmtDate },
            { label: 'Descrição', key: 'descricao' },
            { label: 'Valor', key: 'preco', fmt: fmtMoney }
        ],
        fields: [
            { name: 'descricao', label: 'Descrição', type: 'text', required: true, max: 400, col: 12 },
            { name: 'preco', label: 'Valor (R$)', type: 'decimal', required: true, min: 0.01, col: 6 },
            { name: 'data', label: 'Data', type: 'date', required: true, col: 6 }
        ]
    }
};

const idFieldOf = (key) => RESOURCES[key]?.idField ?? 'id';

// Blocos do dashboard (mesma organização do painel PHP original)
const DASHBOARD_GROUPS = [
    { title: 'Cadastros', icon: 'bi-plus-circle-fill', color: 'success', items: ['clientes', 'pets', 'funcionarios', 'veterinarios', 'salas', 'produtos', 'remedios', 'vacinas'] },
    { title: 'Atendimento', icon: 'bi-heart-pulse-fill', color: 'primary', items: ['consultas', 'prescricoes', 'historicoVacinacao', '@prontuario'] },
    { title: 'Financeiro', icon: 'bi-cash-stack', color: 'warning', items: ['vendas', 'pagamentos', 'formasPagamento', 'despesas', '@estatisticas'] }
];

// Páginas especiais (não são CRUD genérico)
const SPECIAL_PAGES = {
    prontuario: { title: 'Prontuário do Pet', icon: 'bi-search-heart', href: '#/prontuario' },
    estatisticas: { title: 'Estatísticas', icon: 'bi-graph-up-arrow', href: '#/estatisticas' }
};
