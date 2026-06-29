using RestauranteSO.Constants;
using RestauranteSO.Domain.Enums;
using RestauranteSO.Domain.Interfaces;
using RestauranteSO.Domain.Models;
using RestauranteSO.Presentation.Components;
using RestauranteSO.Presentation.Controls;
using RestauranteSO.Presentation.Themes;
using RestauranteSO.Services.Ataques;
using RestauranteSO.Services.ProductorConsumidor;

namespace RestauranteSO.Presentation.Forms
{
    public sealed class FrmProductorConsumidor : Form
    {
        private readonly ProductorConsumidorService       _service;
        private readonly AtaqueProductorConsumidorService _attackService;
        private readonly ISimulationLogger                _logger;

        private Panel       _panelHeader  = null!;
        private Label       _lblTitulo    = null!;
        private StatusBadge _badgeEstado  = null!;
        private Button      _btnVolver    = null!;

        private Panel    _panelToolbar       = null!;
        private Button   _btnIniciar         = null!;
        private Button   _btnPausar          = null!;
        private Button   _btnDetener         = null!;
        private Button   _btnAgregarCliente  = null!;
        private Button   _btnAgregarCocinero = null!;
        private Button   _btnVaciarCola      = null!;
        private Button   _btnActivarAtaque   = null!;
        private TrackBar _trackVelProd       = null!;
        private TrackBar _trackVelCons       = null!;
        private Label    _lblVelProd         = null!;
        private Label    _lblVelCons         = null!;

        private DataGridView    _gridEsperando  = null!;
        private DataGridView    _gridPreparando = null!;
        private DataGridView    _gridListos     = null!;
        private DataGridView    _gridEntregados = null!;

        private SemaphoreVisualizer _vizSemEspacios  = null!;
        private SemaphoreVisualizer _vizSemItems     = null!;
        private SemaphoreVisualizer _vizCola         = null!;
        private FlowLayoutPanel     _flowProductores = null!;
        private FlowLayoutPanel     _flowConsumidores= null!;
        private LogViewer           _logViewer       = null!;

        private Panel _panelStats        = null!;
        private Label _lblStatGen        = null!;
        private Label _lblStatComp       = null!;
        private Label _lblStatAlt        = null!;
        private Label _lblStatThroughput = null!;
        private Label _lblStatTiempo     = null!;

        private Panel  _panelAtaque     = null!;
        private Button _btnVerPoliticas = null!;

        private StatusStrip          _status    = null!;
        private ToolStripStatusLabel _lblStatus = null!;
        private ToolStripStatusLabel _lblCola   = null!;

        private System.Windows.Forms.Timer _timerUI = null!;

        // ─── FUENTES LOCALES — evita Font.ToLogFont crash ─────────────────────
        private static Font F(float size, FontStyle style = FontStyle.Regular)
            => new("Segoe UI", size, style);
        private static Font FMono(float size, FontStyle style = FontStyle.Regular)
            => new("Consolas", size, style);

        public event EventHandler<bool>? SimulacionCambiada;

        public FrmProductorConsumidor(
            ProductorConsumidorService service,
            AtaqueProductorConsumidorService attackService,
            ISimulationLogger logger)
        {
            _service       = service       ?? throw new ArgumentNullException(nameof(service));
            _attackService = attackService ?? throw new ArgumentNullException(nameof(attackService));
            _logger        = logger        ?? throw new ArgumentNullException(nameof(logger));
            InitializeComponent();
            ConfigurarEventos();
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            Text          = "🔄 Productor — Consumidor | RestaurantOS";
            WindowState   = FormWindowState.Maximized;
            MinimumSize   = new Size(1200, 750);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor     = ColorConstants.FondoPrincipal;
            ForeColor     = ColorConstants.TextoPrincipal;
            // Sin Font = AppTheme.X — se asigna por control

            ConstruirHeader();
            ConstruirToolbar();
            ConstruirStatsBar();
            ConstruirPanelAtaque();
            ConstruirStatus();
            ConstruirCuerpo();

            _timerUI = new System.Windows.Forms.Timer
                { Interval = AppConstants.IntervalActualizacionUIMs };
            _timerUI.Tick += TimerUI_Tick;
            ResumeLayout(true);
        }

        // ─── HEADER ──────────────────────────────────────────────────────────

        private void ConstruirHeader()
        {
            _panelHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 68,
                BackColor = ColorConstants.FondoSuperior
            };
            _panelHeader.Paint += (_, e) =>
            {
                using var pen = new Pen(ColorConstants.TarjetaProductor, 2);
                e.Graphics.DrawLine(pen, 0, _panelHeader.Height - 2,
                    _panelHeader.Width, _panelHeader.Height - 2);
            };

            var tbl = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 3,
                RowCount    = 1,
                BackColor   = ColorConstants.FondoSuperior,
                Padding     = new Padding(8, 0, 8, 0)
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 190f));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,  100f));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200f));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            _btnVolver = new Button
            {
                Text     = "⬅ Volver al Inicio",
                Width    = 168,
                Height   = 44,
                Location = new Point(8, 12),
                Font     = F(11f, FontStyle.Bold)
            };
            AppTheme.AplicarABotonVolver(_btnVolver);
            _btnVolver.Click += (_, _) => VolverAlDashboard();

            var panelVolver = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = ColorConstants.FondoSuperior
            };
            panelVolver.Controls.Add(_btnVolver);

            _lblTitulo = new Label
            {
                Text      = "🔄  Simulación Productor — Consumidor",
                Font      = F(18f, FontStyle.Bold),
                ForeColor = ColorConstants.TextoPrincipal,
                BackColor = ColorConstants.FondoSuperior,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize  = false
            };

            _badgeEstado = new StatusBadge
            {
                Text        = "● Detenida",
                ColorAcento = ColorConstants.TextoHint,
                Size        = new Size(170, 36),
                Location    = new Point(8, 16)
            };
            var panelBadge = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = ColorConstants.FondoSuperior
            };
            panelBadge.Controls.Add(_badgeEstado);
            panelBadge.Resize += (_, _) =>
                _badgeEstado.Location =
                    new Point(panelBadge.Width - 178, 16);

            tbl.Controls.Add(panelVolver, 0, 0);
            tbl.Controls.Add(_lblTitulo,  1, 0);
            tbl.Controls.Add(panelBadge,  2, 0);
            _panelHeader.Controls.Add(tbl);
            Controls.Add(_panelHeader);
        }

        // ─── TOOLBAR ─────────────────────────────────────────────────────────

        private void ConstruirToolbar()
        {
            _panelToolbar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 64,
                BackColor = ColorConstants.FondoPanel
            };
            _panelToolbar.Paint += (_, e) =>
            {
                using var pen = new Pen(ColorConstants.Separador, 1);
                e.Graphics.DrawLine(pen, 0, _panelToolbar.Height - 1,
                    _panelToolbar.Width, _panelToolbar.Height - 1);
            };

            var flow = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false,
                BackColor     = ColorConstants.FondoPanel,
                Padding       = new Padding(8, 10, 8, 10)
            };

            _btnIniciar         = CrearBtn("▶  Iniciar",   150, true,  false);
            _btnPausar          = CrearBtn("⏸  Pausar",    140, false, false);
            _btnDetener         = CrearBtn("⏹  Detener",   140, false, false);

            var sep1 = new Panel
            {
                Size      = new Size(1, 44),
                BackColor = ColorConstants.Separador,
                Margin    = new Padding(8, 0, 8, 0)
            };

            _btnAgregarCliente  = CrearBtn("+ Cliente",    130, false, false);
            _btnAgregarCocinero = CrearBtn("+ Cocinero",   140, false, false);
            _btnVaciarCola      = CrearBtn("🗑 Cola",       120, false, false);

            var sep2 = new Panel
            {
                Size      = new Size(1, 44),
                BackColor = ColorConstants.Separador,
                Margin    = new Padding(8, 0, 8, 0)
            };

            var lblVelTit = new Label
            {
                Text      = "Velocidad:",
                Font      = F(10f, FontStyle.Bold),
                ForeColor = ColorConstants.TextoHint,
                BackColor = ColorConstants.FondoPanel,
                AutoSize  = false,
                Size      = new Size(80, 44),
                TextAlign = ContentAlignment.MiddleRight,
                Margin    = new Padding(0)
            };

            _lblVelProd = new Label
            {
                Text      = "Prod 5x",
                Font      = F(10f),
                ForeColor = ColorConstants.TarjetaProductor,
                BackColor = ColorConstants.FondoPanel,
                AutoSize  = false,
                Size      = new Size(56, 22),
                TextAlign = ContentAlignment.MiddleCenter,
                Margin    = new Padding(4, 0, 0, 0)
            };
            _trackVelProd = new TrackBar
            {
                Minimum   = 1, Maximum = 10, Value = 5,
                TickStyle = TickStyle.None,
                Size      = new Size(110, 22),
                BackColor = ColorConstants.FondoPanel,
                Margin    = new Padding(0, 0, 8, 0)
            };

            _lblVelCons = new Label
            {
                Text      = "Coc 5x",
                Font      = F(10f),
                ForeColor = ColorConstants.AcentoSecundario,
                BackColor = ColorConstants.FondoPanel,
                AutoSize  = false,
                Size      = new Size(56, 22),
                TextAlign = ContentAlignment.MiddleCenter,
                Margin    = new Padding(4, 0, 0, 0)
            };
            _trackVelCons = new TrackBar
            {
                Minimum   = 1, Maximum = 10, Value = 5,
                TickStyle = TickStyle.None,
                Size      = new Size(110, 22),
                BackColor = ColorConstants.FondoPanel,
                Margin    = new Padding(0)
            };

            var panelVel = new Panel
            {
                Size      = new Size(290, 44),
                BackColor = ColorConstants.FondoPanel,
                Margin    = new Padding(0)
            };
            _lblVelProd.Location   = new Point(0,  2);
            _trackVelProd.Location = new Point(60, 0);
            _lblVelCons.Location   = new Point(0,  24);
            _trackVelCons.Location = new Point(60, 22);
            panelVel.Controls.AddRange(new Control[]
                { _lblVelProd, _trackVelProd, _lblVelCons, _trackVelCons });

            _btnActivarAtaque = new Button
            {
                Text      = "⚡ Activar Ataque",
                Size      = new Size(170, 44),
                Font      = F(11f, FontStyle.Bold),
                BackColor = ColorConstants.AlertaAtaque,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                Margin    = new Padding(12, 0, 0, 0)
            };
            _btnActivarAtaque.FlatAppearance.BorderSize = 0;

            flow.Controls.AddRange(new Control[]
            {
                _btnIniciar, _btnPausar, _btnDetener,
                sep1,
                _btnAgregarCliente, _btnAgregarCocinero, _btnVaciarCola,
                sep2, lblVelTit, panelVel, _btnActivarAtaque
            });

            _panelToolbar.Controls.Add(flow);
            Controls.Add(_panelToolbar);
        }

        // ─── STATS BAR ────────────────────────────────────────────────────────

        private void ConstruirStatsBar()
        {
            _panelStats = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 56,
                BackColor = Color.FromArgb(22, 22, 34)
            };

            var flow = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false,
                BackColor     = Color.FromArgb(22, 22, 34),
                Padding       = new Padding(8, 0, 8, 0)
            };

            _lblStatGen        = CrearLabelStat("📥 Generados: 0",     ColorConstants.TarjetaProductor);
            _lblStatComp       = CrearLabelStat("✅ Completados: 0",    ColorConstants.AcentoExito);
            _lblStatAlt        = CrearLabelStat("⚡ Alterados: 0",      ColorConstants.AlertaAtaque);
            _lblStatThroughput = CrearLabelStat("📊 Throughput: 0.0/s", ColorConstants.AcentoPrincipal);
            _lblStatTiempo     = CrearLabelStat("⏱ Tiempo: 00:00",     ColorConstants.TextoSecundario);

            flow.Controls.AddRange(new Control[]
            {
                _lblStatGen, _lblStatComp,
                _lblStatAlt, _lblStatThroughput, _lblStatTiempo
            });
            _panelStats.Controls.Add(flow);
            Controls.Add(_panelStats);
        }

        // ─── PANEL ATAQUE ─────────────────────────────────────────────────────

        private void ConstruirPanelAtaque()
        {
            _panelAtaque = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 68,
                BackColor = ColorConstants.FondoAtaque,
                Visible   = false
            };
            _panelAtaque.Paint += (_, e) =>
            {
                using var pen = new Pen(ColorConstants.AlertaAtaque, 2);
                e.Graphics.DrawLine(pen, 0, 0, _panelAtaque.Width, 0);
            };

            var tbl = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 1,
                BackColor   = ColorConstants.FondoAtaque,
                Padding     = new Padding(12, 0, 12, 0)
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75f));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));

            var panelTexto = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = ColorConstants.FondoAtaque
            };

            var lblTit = new Label
            {
                Text      = "⚡ ATAQUE ACTIVO: Inyección de Pedidos en cola",
                Font      = F(11f, FontStyle.Bold),
                ForeColor = ColorConstants.AlertaAtaque,
                BackColor = ColorConstants.FondoAtaque,
                AutoSize  = true,
                Location  = new Point(0, 8)
            };
            var lblDesc = new Label
            {
                Text      = "Un falso técnico instaló un agente malicioso. " +
                             "Pedidos duplicados, eliminados y alterados.",
                Font      = F(10f),
                ForeColor = ColorConstants.TextoSecundario,
                BackColor = ColorConstants.FondoAtaque,
                AutoSize  = true,
                Location  = new Point(0, 34)
            };
            panelTexto.Controls.AddRange(new Control[] { lblTit, lblDesc });

            _btnVerPoliticas = new Button
            {
                Text   = "🛡 Ver Políticas de Prevención",
                Dock   = DockStyle.Fill,
                Font   = F(11f, FontStyle.Bold),
                Margin = new Padding(8, 10, 0, 10)
            };
            AppTheme.AplicarABotonSeguridad(_btnVerPoliticas);
            _btnVerPoliticas.Click += (_, _) => AbrirPoliticas();

            tbl.Controls.Add(panelTexto,       0, 0);
            tbl.Controls.Add(_btnVerPoliticas, 1, 0);
            _panelAtaque.Controls.Add(tbl);
            Controls.Add(_panelAtaque);
        }

        // ─── STATUS STRIP ─────────────────────────────────────────────────────

        private void ConstruirStatus()
        {
            _status = new StatusStrip
            {
                BackColor  = ColorConstants.FondoSuperior,
                ForeColor  = ColorConstants.TextoSecundario,
                SizingGrip = false,
                Padding    = new Padding(10, 0, 10, 0),
                RenderMode = ToolStripRenderMode.Professional
            };

            _lblStatus = new ToolStripStatusLabel
            {
                Text      = "Listo — presione Iniciar",
                Spring    = true,
                ForeColor = ColorConstants.TextoSecundario,
                Font      = FMono(10f)
            };
            _lblCola = new ToolStripStatusLabel
            {
                Text      = "Cola: 0/15",
                ForeColor = ColorConstants.AcentoPrincipal,
                Font      = FMono(10f, FontStyle.Bold)
            };

            _status.Items.AddRange(
                new ToolStripItem[] { _lblStatus, _lblCola });
            Controls.Add(_status);
        }

        // ─── CUERPO ───────────────────────────────────────────────────────────

        private void ConstruirCuerpo()
        {
            var panelCuerpo = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = ColorConstants.FondoPrincipal
            };

            var panelIzq = new Panel
            {
                Dock      = DockStyle.Left,
                BackColor = ColorConstants.FondoPrincipal,
                Padding   = new Padding(8)
            };

            var panelDerSup = new Panel
            {
                Dock       = DockStyle.Left,
                BackColor  = ColorConstants.FondoLateral,
                Padding    = new Padding(8),
                AutoScroll = true
            };

            var panelDerInf = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = ColorConstants.FondoLateral
            };

            void redistribuir()
            {
                if (panelCuerpo.Width < 100) return;
                panelIzq.Width    = (int)(panelCuerpo.Width * 0.60);
                panelDerSup.Width = (int)(panelCuerpo.Width * 0.20);
            }

            panelCuerpo.Resize += (_, _) => redistribuir();
            Shown              += (_, _) => redistribuir();
            Resize             += (_, _) => redistribuir();

            ConstruirKanban(panelIzq);
            ConstruirPanelSemaforos(panelDerSup);

            _logViewer = new LogViewer { Dock = DockStyle.Fill };
            _logViewer.SetTitulo("📋 Log de Concurrencia");
            panelDerInf.Controls.Add(_logViewer);

            panelCuerpo.Controls.Add(panelDerInf);
            panelCuerpo.Controls.Add(panelDerSup);
            panelCuerpo.Controls.Add(panelIzq);
            Controls.Add(panelCuerpo);
        }

        private void ConstruirKanban(Panel panel)
        {
            var tbl = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 4,
                RowCount    = 1,
                BackColor   = ColorConstants.FondoPrincipal,
                Padding     = new Padding(8)
            };
            for (int i = 0; i < 4; i++)
                tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            _gridEsperando  = CrearGridKanban();
            _gridPreparando = CrearGridKanban();
            _gridListos     = CrearGridKanban();
            _gridEntregados = CrearGridKanban();

            tbl.Controls.Add(CrearColumnaKanban("⏳ Esperando",
                ColorConstants.EstadoEsperando,     _gridEsperando),  0, 0);
            tbl.Controls.Add(CrearColumnaKanban("🔵 Preparando",
                ColorConstants.EstadoEnPreparacion, _gridPreparando), 1, 0);
            tbl.Controls.Add(CrearColumnaKanban("✅ Listos",
                ColorConstants.EstadoListo,         _gridListos),     2, 0);
            tbl.Controls.Add(CrearColumnaKanban("📦 Entregados",
                ColorConstants.EstadoEntregado,     _gridEntregados), 3, 0);

            panel.Controls.Add(tbl);
        }

        private Panel CrearColumnaKanban(
            string titulo, Color color, DataGridView grid)
        {
            var panel = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = ColorConstants.FondoCard,
                Margin    = new Padding(4)
            };
            panel.Paint += (_, e) =>
            {
                using var pen = new Pen(color, 2);
                e.Graphics.DrawRectangle(pen, 0, 0,
                    panel.Width - 1, panel.Height - 1);
            };

            var header = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 44,
                BackColor = Color.FromArgb(40, color.R, color.G, color.B)
            };
            header.Paint += (_, e) =>
            {
                using var pen = new Pen(color, 2);
                e.Graphics.DrawLine(pen, 0, header.Height - 2,
                    header.Width, header.Height - 2);
            };

            var lbl = new Label
            {
                Text      = titulo,
                Font      = F(11f, FontStyle.Bold),
                ForeColor = color,
                BackColor = Color.FromArgb(40, color.R, color.G, color.B),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            header.Controls.Add(lbl);

            grid.Dock = DockStyle.Fill;
            panel.Controls.Add(grid);
            panel.Controls.Add(header);
            return panel;
        }

        private DataGridView CrearGridKanban()
        {
            var grid = new DataGridView();
            AppTheme.AplicarADataGrid(grid);
            grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "Num",  HeaderText = "#",       Width = 44, FillWeight = 12 });
            grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "Desc", HeaderText = "Plato",   FillWeight = 55 });
            grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "Mesa", HeaderText = "Mesa",    Width = 48, FillWeight = 13 });
            grid.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "Cli",  HeaderText = "Cliente", FillWeight = 20 });
            return grid;
        }

        private void ConstruirPanelSemaforos(Panel panel)
        {
            var scroll = new Panel
            {
                Dock       = DockStyle.Fill,
                AutoScroll = true,
                BackColor  = ColorConstants.FondoLateral,
                Padding    = new Padding(8)
            };

            var lblSem = new Label
            {
                Text      = "🔒 Primitivas de Sincronización",
                Font      = F(11f, FontStyle.Bold),
                ForeColor = ColorConstants.TarjetaProductor,
                BackColor = ColorConstants.FondoLateral,
                Dock      = DockStyle.Top,
                Height    = 32,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(4, 0, 0, 0)
            };

            var panelViz = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 230,
                BackColor = ColorConstants.FondoPanel
            };

            _vizSemEspacios = new SemaphoreVisualizer
                { Location = new Point(4,   4), Height = 72 };
            _vizSemItems = new SemaphoreVisualizer
                { Location = new Point(4,  80), Height = 72 };
            _vizCola = new SemaphoreVisualizer
                { Location = new Point(4, 156), Height = 72 };

            panelViz.Resize += (_, _) =>
            {
                int w = panelViz.Width - 8;
                _vizSemEspacios.Width = w;
                _vizSemItems.Width    = w;
                _vizCola.Width        = w;
            };

            panelViz.Controls.AddRange(new Control[]
                { _vizSemEspacios, _vizSemItems, _vizCola });

            _vizSemEspacios.ActualizarSemaforo("Sem: Espacios Libres",
                AppConstants.CapacidadMaximaCola, AppConstants.CapacidadMaximaCola);
            _vizSemItems.ActualizarSemaforo("Sem: Items Disponibles",
                0, AppConstants.CapacidadMaximaCola);
            _vizCola.ActualizarCola(0, AppConstants.CapacidadMaximaCola);

            var lblProd = new Label
            {
                Text      = "👥 Clientes (Productores)",
                Font      = F(11f, FontStyle.Bold),
                ForeColor = ColorConstants.TarjetaProductor,
                BackColor = ColorConstants.FondoLateral,
                Dock      = DockStyle.Top,
                Height    = 32,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(4, 0, 0, 0)
            };

            _flowProductores = new FlowLayoutPanel
            {
                Dock          = DockStyle.Top,
                Height        = 120,
                FlowDirection = FlowDirection.TopDown,
                WrapContents  = false,
                AutoScroll    = true,
                BackColor     = ColorConstants.FondoPanel,
                Padding       = new Padding(4)
            };

            var lblCoc = new Label
            {
                Text      = "👨‍🍳 Cocineros (Consumidores)",
                Font      = F(11f, FontStyle.Bold),
                ForeColor = ColorConstants.AcentoSecundario,
                BackColor = ColorConstants.FondoLateral,
                Dock      = DockStyle.Top,
                Height    = 32,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(4, 0, 0, 0)
            };

            _flowConsumidores = new FlowLayoutPanel
            {
                Dock          = DockStyle.Top,
                Height        = 170,
                FlowDirection = FlowDirection.TopDown,
                WrapContents  = false,
                AutoScroll    = true,
                BackColor     = ColorConstants.FondoPanel,
                Padding       = new Padding(4)
            };

            scroll.Controls.Add(_flowConsumidores);
            scroll.Controls.Add(lblCoc);
            scroll.Controls.Add(_flowProductores);
            scroll.Controls.Add(lblProd);
            scroll.Controls.Add(panelViz);
            scroll.Controls.Add(lblSem);
            panel.Controls.Add(scroll);
        }

        // ─── EVENTOS ─────────────────────────────────────────────────────────

        private void ConfigurarEventos()
        {
            _btnIniciar.Click         += (_, _) => Iniciar();
            _btnPausar.Click          += (_, _) => PausarReanudar();
            _btnDetener.Click         += (_, _) => Detener();
            _btnAgregarCliente.Click  += (_, _) => _service.AgregarProductor();
            _btnAgregarCocinero.Click += (_, _) => _service.AgregarConsumidor();
            _btnVaciarCola.Click      += (_, _) => _service.VaciarCola();
            _btnActivarAtaque.Click   += (_, _) => ToggleAtaque();

            _trackVelProd.ValueChanged += (_, _) =>
            {
                _service.AjustarVelocidad(VelAMs(_trackVelProd.Value));
                _lblVelProd.Text = $"Prod {_trackVelProd.Value}x";
            };
            _trackVelCons.ValueChanged += (_, _) =>
            {
                _service.AjustarVelocidadConsumidor(VelAMs(_trackVelCons.Value));
                _lblVelCons.Text = $"Coc {_trackVelCons.Value}x";
            };

            _service.EstadoCambiado += (_, estado) =>
            {
                if (InvokeRequired)
                { BeginInvoke(() => OnEstadoCambiado(estado)); return; }
                OnEstadoCambiado(estado);
            };

            _attackService.AtaqueActivado += (_, _) =>
            {
                if (InvokeRequired) { BeginInvoke(MostrarAtaque); return; }
                MostrarAtaque();
            };
            _attackService.AtaqueDesactivado += (_, _) =>
            {
                if (InvokeRequired) { BeginInvoke(OcultarAtaque); return; }
                OcultarAtaque();
            };

            _logger.NuevoLogAgregado += (_, entry) =>
            {
                if (!IsDisposed && _logViewer != null)
                    _logViewer.AgregarLog(entry);
            };

            FormClosing += (_, _) =>
            {
                _timerUI.Stop();
                if (_service.EstaCorreindo)        _service.Detener();
                if (_attackService.IsAttackActive) _attackService.DesactivarAtaque();
            };
        }

        // ─── ACCIONES ─────────────────────────────────────────────────────────

        private void VolverAlDashboard()
        {
            _timerUI.Stop();
            if (_service.EstaCorreindo)        _service.Detener();
            if (_attackService.IsAttackActive) _attackService.DesactivarAtaque();
            Close();
        }

        private void Iniciar()
        {
            try
            {
                _service.Iniciar();
                _timerUI.Start();
                ActualizarBotones(SimulationStatus.Corriendo);
                SimulacionCambiada?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar:\n{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PausarReanudar()
        {
            if (_service.Estado == SimulationStatus.Corriendo ||
                _service.Estado == SimulationStatus.BajoAtaque)
            {
                _service.Pausar();
                _btnPausar.Text = "▶  Reanudar";
            }
            else if (_service.Estado == SimulationStatus.Pausada)
            {
                _service.Reanudar();
                _btnPausar.Text = "⏸  Pausar";
            }
        }

        private void Detener()
        {
            _timerUI.Stop();
            _service.Detener();
            if (_attackService.IsAttackActive) _attackService.DesactivarAtaque();
            ActualizarBotones(SimulationStatus.Detenida);
            SimulacionCambiada?.Invoke(this, false);
            LimpiarGrids();
        }

        private void ToggleAtaque()
        {
            if (!_service.EstaCorreindo)
            {
                MessageBox.Show("Inicie la simulación primero.",
                    "Simulación no activa",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_attackService.IsAttackActive)
            {
                _attackService.DesactivarAtaque();
                _btnActivarAtaque.Text      = "⚡ Activar Ataque";
                _btnActivarAtaque.BackColor = ColorConstants.AlertaAtaque;
            }
            else
            {
                using var dlg = new FrmIngenieriaSocial(
                    "🔧 Soporte Técnico — Actualización",
                    "Estimado Encargado:\n\n" +
                    "Soy Carlos Martínez, técnico de SistemaResto S.A.\n" +
                    "Necesito instalar una actualización crítica.\n\n" +
                    "⚠ Es URGENTE para evitar pérdida de datos.\n\n" +
                    "¿Me permite acceder al sistema?",
                    "Aceptar actualización",
                    "Rechazar");

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _attackService.ActivarAtaque(AttackType.InyeccionDePedidos);
                    _btnActivarAtaque.Text      = "🛡 Desactivar Ataque";
                    _btnActivarAtaque.BackColor = ColorConstants.AcentoExito;
                }
            }
        }

        private void AbrirPoliticas()
        {
            using var frm = new FrmPoliticas(
                "Políticas — Inyección de Pedidos",
                AtaqueProductorConsumidorService.ObtenerPoliticas(),
                _attackService.HistorialAtaques);
            frm.ShowDialog(this);
        }

        // ─── ACTUALIZACIÓN UI ─────────────────────────────────────────────────

        private void TimerUI_Tick(object? sender, EventArgs e)
        {
            if (_service.Estado == SimulationStatus.Detenida) return;
            var stats = _service.ObtenerEstadisticas();
            ActualizarStats(stats);
            ActualizarGrids();
            ActualizarViz(stats);
            ActualizarHilos();
        }

        private void ActualizarStats(SimulationStatistics stats)
        {
            _lblStatGen.Text        = $"📥 Generados: {stats.TotalPedidosGenerados}";
            _lblStatComp.Text       = $"✅ Completados: {stats.TotalPedidosCompletados}";
            _lblStatAlt.Text        = $"⚡ Alterados: {stats.TotalPedidosAlterados}";
            _lblStatThroughput.Text = $"📊 Throughput: {stats.PedidosPorSegundo:F1}/s";
            _lblStatTiempo.Text     = $"⏱ Tiempo: {stats.TiempoEjecucion:mm\\:ss}";
            _lblCola.Text           = $"Cola: {stats.PedidosEnCola}/{stats.CapacidadMaximaCola}";
            _lblStatus.Text         =
                $"{stats.EstadoActual}  |  " +
                $"Prod: {stats.ProductoresActivos}  |  " +
                $"Cons: {stats.ConsumidoresActivos}";
        }

        private void ActualizarGrids()
        {
            var pedidos = _service.Cola.ToArray();

            RellenarGrid(_gridEsperando, pedidos.Where(p =>
                p.Estado == PedidoEstado.Esperando  ||
                p.Estado == PedidoEstado.Duplicado  ||
                p.Estado == PedidoEstado.Alterado).Take(30));

            RellenarGrid(_gridPreparando, pedidos.Where(p =>
                p.Estado == PedidoEstado.EnPreparacion).Take(20));
        }

        private void RellenarGrid(
            DataGridView grid,
            IEnumerable<Domain.Entities.Pedido> pedidos)
        {
            grid.SuspendLayout();
            grid.Rows.Clear();
            foreach (var p in pedidos)
            {
                int idx = grid.Rows.Add(
                    $"#{p.NumeroPedido:D3}",
                    p.Descripcion.Length > 24
                        ? p.Descripcion[..24] + "…" : p.Descripcion,
                    p.NumeroMesa,
                    p.ClienteId.Replace("Cliente-", "C"));

                if (p.FueAlterado || p.FueDuplicado)
                {
                    grid.Rows[idx].DefaultCellStyle.BackColor =
                        Color.FromArgb(60, 220, 50, 50);
                    grid.Rows[idx].DefaultCellStyle.ForeColor =
                        ColorConstants.EstadoAlterado;
                }
            }
            grid.ResumeLayout();
        }

        private void ActualizarViz(SimulationStatistics stats)
        {
            _vizSemEspacios.ActualizarSemaforo("Sem: Espacios Libres",
                _service.EspaciosLibres, AppConstants.CapacidadMaximaCola);
            _vizSemItems.ActualizarSemaforo("Sem: Items Disponibles",
                _service.ItemsDisponibles, AppConstants.CapacidadMaximaCola);
            _vizCola.ActualizarCola(
                stats.PedidosEnCola, AppConstants.CapacidadMaximaCola);
        }

        private void ActualizarHilos()
        {
            var clientes  = _service.Clientes;
            var cocineros = _service.Cocineros;

            SincronizarFlow(_flowProductores, clientes.Count, i =>
            {
                if (i >= clientes.Count) return;
                var c   = clientes[i];
                var ind = (ThreadStatusIndicator)_flowProductores.Controls[i];
                ind.ActualizarEstado(c.Id,
                    c.EstaActivo
                        ? ThreadStatusIndicator.EstadoHilo.Activo
                        : ThreadStatusIndicator.EstadoHilo.Detenido,
                    c.UltimoPedido != null
                        ? $"Último: #{c.UltimoPedido.NumeroPedido:D3}"
                        : "Esperando...");
            });

            SincronizarFlow(_flowConsumidores, cocineros.Count, i =>
            {
                if (i >= cocineros.Count) return;
                var c   = cocineros[i];
                var ind = (ThreadStatusIndicator)_flowConsumidores.Controls[i];
                ind.ActualizarEstado(
                    $"{c.Id} [{c.Especialidad}]",
                    c.PedidoActual != null
                        ? ThreadStatusIndicator.EstadoHilo.Activo
                        : c.EstaActivo
                            ? ThreadStatusIndicator.EstadoHilo.Esperando
                            : ThreadStatusIndicator.EstadoHilo.Detenido,
                    c.PedidoActual != null
                        ? $"Preparando #{c.PedidoActual.NumeroPedido:D3}"
                        : "Esperando pedido...",
                    c.ProgresoActual);
            });
        }

        private void SincronizarFlow(
            FlowLayoutPanel flow, int cant, Action<int> update)
        {
            while (flow.Controls.Count < cant)
                flow.Controls.Add(new ThreadStatusIndicator
                {
                    Width  = flow.Width - 12,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right
                });
            for (int i = 0; i < cant; i++) update(i);
        }

        // ─── HELPERS ─────────────────────────────────────────────────────────

        private void OnEstadoCambiado(SimulationStatus estado)
        {
            ActualizarBotones(estado);
            (_badgeEstado.Text, _badgeEstado.ColorAcento) = estado switch
            {
                SimulationStatus.Corriendo  => ("▶ Corriendo",     ColorConstants.AcentoExito),
                SimulationStatus.Pausada    => ("⏸ Pausada",       ColorConstants.EstadoEsperando),
                SimulationStatus.Deteniendo => ("⏹ Deteniendo...", ColorConstants.TextoHint),
                SimulationStatus.BajoAtaque => ("⚡ Bajo Ataque",  ColorConstants.AlertaAtaque),
                _                           => ("● Detenida",      ColorConstants.TextoHint)
            };
        }

        private void ActualizarBotones(SimulationStatus estado)
        {
            bool corriendo = estado == SimulationStatus.Corriendo ||
                             estado == SimulationStatus.BajoAtaque;
            bool pausada   = estado == SimulationStatus.Pausada;
            bool detenida  = estado == SimulationStatus.Detenida;

            _btnIniciar.Enabled         = detenida;
            _btnPausar.Enabled          = corriendo || pausada;
            _btnDetener.Enabled         = !detenida;
            _btnAgregarCliente.Enabled  = corriendo || pausada;
            _btnAgregarCocinero.Enabled = corriendo || pausada;
            _btnVaciarCola.Enabled      = corriendo || pausada;
            _btnActivarAtaque.Enabled   = corriendo;

            AppTheme.AplicarABotonPrimario(_btnIniciar);
            if (!_btnIniciar.Enabled)
            {
                _btnIniciar.BackColor = ColorConstants.Separador;
                _btnIniciar.ForeColor = ColorConstants.TextoHint;
            }
        }

        private void MostrarAtaque() => _panelAtaque.Visible = true;
        private void OcultarAtaque() => _panelAtaque.Visible = false;

        private void LimpiarGrids()
        {
            _gridEsperando.Rows.Clear();
            _gridPreparando.Rows.Clear();
            _gridListos.Rows.Clear();
            _gridEntregados.Rows.Clear();
            _flowProductores.Controls.Clear();
            _flowConsumidores.Controls.Clear();
        }

        private static int VelAMs(int v) =>
            (int)(3000 / Math.Pow(v, 0.8));

        private static Button CrearBtn(
            string texto, int ancho, bool primario, bool ataque)
        {
            var btn = new Button
            {
                Text   = texto,
                Width  = ancho,
                Height = 44,
                Font   = new Font("Segoe UI", 11f, FontStyle.Bold),
                Margin = new Padding(4, 0, 4, 0)
            };
            if (ataque)        AppTheme.AplicarABotonAtaque(btn);
            else if (primario) AppTheme.AplicarABotonPrimario(btn);
            else               AppTheme.AplicarABotonSecundario(btn);
            return btn;
        }

        private static Label CrearLabelStat(string texto, Color color)
        {
            return new Label
            {
                Text      = texto,
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = color,
                BackColor = Color.FromArgb(22, 22, 34),
                AutoSize  = false,
                Height    = 56,
                Width     = 210,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin    = new Padding(16, 0, 16, 0)
            };
        }
    }
}