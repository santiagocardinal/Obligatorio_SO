using RestauranteSO.Constants;
using RestauranteSO.Domain.Enums;
using RestauranteSO.Domain.Interfaces;
using RestauranteSO.Domain.Models;
using RestauranteSO.Presentation.Components;
using RestauranteSO.Presentation.Controls;
using RestauranteSO.Presentation.Themes;
using RestauranteSO.Services.Ataques;
using RestauranteSO.Services.ProductorConsumidor;
using System.Drawing.Drawing2D;

namespace RestauranteSO.Presentation.Forms
{
    /// <summary>
    /// Simulación Productor-Consumidor con paleta clara, identidad visual propia,
    /// buffer animado y flujo visual de pedidos.
    /// </summary>
    public sealed class FrmProductorConsumidor : Form
    {
        private readonly ProductorConsumidorService _service;
        private readonly AtaqueProductorConsumidorService _attackService;
        private readonly ISimulationLogger _logger;

        // ─── BARRA DE TÍTULO ──────────────────────────────────────────────────
        private Panel _titleBar = null!;
        private Label _lblTitulo = null!;
        private Button _btnMinimize = null!;
        private Button _btnMaximize = null!;
        private Button _btnClose = null!;
        private StatusBadge _badgeEstado = null!;

        // ─── TOOLBAR ─────────────────────────────────────────────────────────
        private Panel _toolbar = null!;
        private Button _btnIniciar = null!;
        private Button _btnPausar = null!;
        private Button _btnDetener = null!;
        private Button _btnAgregarCliente = null!;
        private Button _btnAgregarCocinero = null!;
        private Button _btnVaciarCola = null!;
        private Button _btnAtaque = null!;
        private TrackBar _trackVelProd = null!;
        private TrackBar _trackVelCons = null!;
        private Label _lblVelProd = null!;
        private Label _lblVelCons = null!;

        // ─── CUERPO ──────────────────────────────────────────────────────────
        private Panel _panelClientes = null!;
        private FlowLayoutPanel _flowProductores = null!;
        private Panel _panelBuffer = null!;
        private BufferBlocks _bufferBlocks = null!;
        private Panel _panelCocineros = null!;
        private FlowLayoutPanel _flowConsumidores = null!;
        private Panel _panelStats = null!;
        private Label _lblGen = null!;
        private Label _lblComp = null!;
        private Label _lblAlt = null!;
        private Label _lblThroughput = null!;
        private Label _lblTiempo = null!;
        private LogViewer _logViewer = null!;
        private SemaphoreVisualizer _vizSemEspacios = null!;
        private SemaphoreVisualizer _vizSemItems = null!;

        // ─── PANEL DE ATAQUE ─────────────────────────────────────────────────
        private Panel _panelAtaque = null!;
        private Button _btnVerPoliticas = null!;

        // ─── STATUS ──────────────────────────────────────────────────────────
        private StatusStrip _status = null!;
        private ToolStripStatusLabel _lblStatus = null!;
        private ToolStripStatusLabel _lblCola = null!;

        // ─── TIMERS ──────────────────────────────────────────────────────────
        private System.Windows.Forms.Timer _timerUI = null!;

        private const int TITLE_BAR_HEIGHT = 38;
        private const int PADDING_GLOBAL = 24;
        private const int GAP_BETWEEN_PANELS = 16;

        public FrmProductorConsumidor(
            ProductorConsumidorService service,
            AtaqueProductorConsumidorService attackService,
            ISimulationLogger logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _attackService = attackService ?? throw new ArgumentNullException(nameof(attackService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            InitializeComponent();
            ConfigurarEventos();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1280, 800);
            BackColor = ColorConstants.FondoPrincipal;
            ForeColor = ColorConstants.TextoPrincipal;
            Font = AppTheme.FuenteLabel;

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);

            ConstruirTitleBar();
            ConstruirToolbar();
            ConstruirCuerpo();
            ConstruirPanelAtaque();
            ConstruirStatus();

            _timerUI = new System.Windows.Forms.Timer { Interval = AppConstants.IntervalActualizacionUIMs };
            _timerUI.Tick += TimerUI_Tick;

            ResumeLayout(true);
        }

        // ─── BARRA DE TÍTULO ─────────────────────────────────────────────────

        private void ConstruirTitleBar()
        {
            _titleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = TITLE_BAR_HEIGHT,
                BackColor = ColorConstants.FondoSuperior
            };
            _titleBar.Paint += (s, e) =>
            {
                using var pen = new Pen(ColorConstants.TarjetaProductor, 3);
                e.Graphics.DrawLine(pen, 0, _titleBar.Height - 2, _titleBar.Width, _titleBar.Height - 2);
            };

            _lblTitulo = new Label
            {
                Text = "🔄  Productor — Consumidor",
                Font = AppTheme.FuenteLabelBold,
                ForeColor = ColorConstants.TextoPrincipal,
                BackColor = ColorConstants.FondoSuperior,
                AutoSize = false,
                Dock = DockStyle.Left,
                Width = 340,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 0, 0, 0)
            };

            _btnMinimize = CrearBotonVentana("─", ColorConstants.TextoHint);
            _btnMaximize = CrearBotonVentana("☐", ColorConstants.TextoHint);
            _btnClose = CrearBotonVentana("✕", ColorConstants.AlertaAtaque);
            _btnMinimize.Click += (_, _) => WindowState = FormWindowState.Minimized;
            _btnMaximize.Click += (_, _) =>
            {
                WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
                _btnMaximize.Text = WindowState == FormWindowState.Maximized ? "☒" : "☐";
            };
            _btnClose.Click += (_, _) => CerrarVentana();

            _badgeEstado = new StatusBadge
            {
                Text = "● Detenida",
                ColorAcento = ColorConstants.TextoHint,
                Size = new Size(160, 30),
                Anchor = AnchorStyles.Right,
                Font = AppTheme.FuenteSmallBold
            };

            var panelDerecho = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Height = TITLE_BAR_HEIGHT,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = ColorConstants.FondoSuperior,
                Padding = new Padding(0, 0, 12, 0)
            };
            panelDerecho.Controls.AddRange(new Control[] { _btnClose, _btnMaximize, _btnMinimize, _badgeEstado });
            panelDerecho.Resize += (_, _) => _badgeEstado.Location = new Point(panelDerecho.Width - 170, 4);

            _titleBar.Controls.Add(_lblTitulo);
            _titleBar.Controls.Add(panelDerecho);

            _titleBar.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    NativeMethods.ReleaseCapture();
                    NativeMethods.SendMessage(Handle, 0xA1, 0x2, 0);
                }
            };

            Controls.Add(_titleBar);
        }

        private Button CrearBotonVentana(string texto, Color color)
        {
            var btn = new Button
            {
                Text = texto,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = color,
                BackColor = ColorConstants.FondoSuperior,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(38, TITLE_BAR_HEIGHT),
                Cursor = Cursors.Hand,
                Margin = new Padding(0)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 220, 240);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(200, 200, 220);
            return btn;
        }

        // ─── TOOLBAR ─────────────────────────────────────────────────────────

        private void ConstruirToolbar()
        {
            _toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 68,
                BackColor = ColorConstants.FondoPanel,
                Padding = new Padding(8, 6, 8, 6)
            };
            _toolbar.Paint += (_, e) =>
            {
                using var pen = new Pen(ColorConstants.Separador, 1);
                e.Graphics.DrawLine(pen, 0, _toolbar.Height - 1, _toolbar.Width, _toolbar.Height - 1);
            };

            var flow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = ColorConstants.FondoPanel,
                Padding = new Padding(8, 8, 8, 8)
            };

            _btnIniciar = CrearBotonToolbar("▶ Iniciar", true);
            _btnPausar = CrearBotonToolbar("⏸ Pausar", false);
            _btnDetener = CrearBotonToolbar("⏹ Detener", false);
            _btnAgregarCliente = CrearBotonToolbar("+ Cliente", false);
            _btnAgregarCocinero = CrearBotonToolbar("+ Cocinero", false);
            _btnVaciarCola = CrearBotonToolbar("🗑 Cola", false);
            _btnAtaque = CrearBotonToolbar("⚡ Ataque", false, true);

            var sep1 = CrearSeparador();
            var sep2 = CrearSeparador();

            var lblVel = new Label
            {
                Text = "Velocidad:",
                Font = AppTheme.FuenteSmallBold,
                ForeColor = ColorConstants.TextoHint,
                BackColor = ColorConstants.FondoPanel,
                AutoSize = false,
                Size = new Size(80, 44),
                TextAlign = ContentAlignment.MiddleRight,
                Margin = new Padding(0)
            };

            _lblVelProd = new Label
            {
                Text = "Prod 5x",
                Font = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.TarjetaProductor,
                BackColor = ColorConstants.FondoPanel,
                AutoSize = false,
                Size = new Size(58, 22),
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(4, 0, 0, 0)
            };
            _trackVelProd = new TrackBar
            {
                Minimum = 1,
                Maximum = 10,
                Value = 5,
                TickStyle = TickStyle.None,
                Size = new Size(110, 22),
                BackColor = ColorConstants.FondoPanel,
                Margin = new Padding(0, 0, 8, 0)
            };

            _lblVelCons = new Label
            {
                Text = "Coc 5x",
                Font = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.AcentoSecundario,
                BackColor = ColorConstants.FondoPanel,
                AutoSize = false,
                Size = new Size(58, 22),
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(4, 0, 0, 0)
            };
            _trackVelCons = new TrackBar
            {
                Minimum = 1,
                Maximum = 10,
                Value = 5,
                TickStyle = TickStyle.None,
                Size = new Size(110, 22),
                BackColor = ColorConstants.FondoPanel,
                Margin = new Padding(0)
            };

            var panelVel = new Panel
            {
                Size = new Size(300, 44),
                BackColor = ColorConstants.FondoPanel,
                Margin = new Padding(0)
            };
            _lblVelProd.Location = new Point(0, 2);
            _trackVelProd.Location = new Point(64, 0);
            _lblVelCons.Location = new Point(0, 24);
            _trackVelCons.Location = new Point(64, 22);
            panelVel.Controls.AddRange(new Control[] { _lblVelProd, _trackVelProd, _lblVelCons, _trackVelCons });

            flow.Controls.AddRange(new Control[]
            {
                _btnIniciar, _btnPausar, _btnDetener,
                sep1, _btnAgregarCliente, _btnAgregarCocinero, _btnVaciarCola,
                sep2, lblVel, panelVel, _btnAtaque
            });

            _toolbar.Controls.Add(flow);
            Controls.Add(_toolbar);
        }

        private Button CrearBotonToolbar(string texto, bool primario, bool ataque = false)
        {
            var btn = new Button
            {
                Text = texto,
                Font = AppTheme.FuenteBoton,
                Size = new Size(140, 44),
                Margin = new Padding(6, 0, 6, 0),
                Cursor = Cursors.Hand,
                FlatStyle = FlatStyle.Flat,
                Padding = new Padding(6, 0, 6, 0)
            };
            if (ataque) AppTheme.AplicarABotonAtaque(btn);
            else if (primario) AppTheme.AplicarABotonPrimario(btn);
            else AppTheme.AplicarABotonSecundario(btn);
            return btn;
        }

        private Panel CrearSeparador() => new Panel
        {
            Size = new Size(1, 44),
            BackColor = ColorConstants.Separador,
            Margin = new Padding(10, 0, 10, 0)
        };

        // ─── CUERPO ──────────────────────────────────────────────────────────

        private void ConstruirCuerpo()
        {
            var panelCuerpo = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorConstants.FondoPrincipal,
                Padding = new Padding(PADDING_GLOBAL)
            };

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 2,
                BackColor = ColorConstants.FondoPrincipal,
                Padding = new Padding(4)
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 22));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 46));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 45));

            // ── Clientes ──────────────────────────────────────────────────────
            _panelClientes = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorConstants.FondoLateral,
                Padding = new Padding(8)
            };
            var lblClientes = new Label
            {
                Text = "👥 Clientes (Productores)",
                Font = AppTheme.FuenteLabelBold,
                ForeColor = ColorConstants.TarjetaProductor,
                Dock = DockStyle.Top,
                Height = 34,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0)
            };
            _flowProductores = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = ColorConstants.FondoPanel,
                Padding = new Padding(6)
            };
            _panelClientes.Controls.Add(_flowProductores);
            _panelClientes.Controls.Add(lblClientes);

            // ── Buffer ────────────────────────────────────────────────────────
            _panelBuffer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorConstants.FondoPanel,
                Padding = new Padding(12)
            };
            var lblBuffer = new Label
            {
                Text = "📦 Buffer (Cola de Pedidos)",
                Font = AppTheme.FuenteLabelBold,
                ForeColor = ColorConstants.AcentoPrincipal,
                Dock = DockStyle.Top,
                Height = 34,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0)
            };
            _bufferBlocks = new BufferBlocks
            {
                Dock = DockStyle.Fill,
                BackColor = ColorConstants.FondoCard,
                CapacidadMaxima = AppConstants.CapacidadMaximaCola,
                Margin = new Padding(4)
            };
            _panelBuffer.Controls.Add(_bufferBlocks);
            _panelBuffer.Controls.Add(lblBuffer);

            // ── Cocineros ─────────────────────────────────────────────────────
            _panelCocineros = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorConstants.FondoLateral,
                Padding = new Padding(8)
            };
            var lblCocineros = new Label
            {
                Text = "👨‍🍳 Cocineros (Consumidores)",
                Font = AppTheme.FuenteLabelBold,
                ForeColor = ColorConstants.AcentoSecundario,
                Dock = DockStyle.Top,
                Height = 34,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0)
            };
            _flowConsumidores = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = ColorConstants.FondoPanel,
                Padding = new Padding(6)
            };
            _panelCocineros.Controls.Add(_flowConsumidores);
            _panelCocineros.Controls.Add(lblCocineros);

            // ── Fila 2 ───────────────────────────────────────────────────────
            _panelStats = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorConstants.FondoPanel,
                Padding = new Padding(12)
            };
            var flowStats = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                BackColor = ColorConstants.FondoPanel,
                Padding = new Padding(8)
            };
            _lblGen = CrearLabelStat("📥 Generados: 0", ColorConstants.TarjetaProductor);
            _lblComp = CrearLabelStat("✅ Completados: 0", ColorConstants.AcentoExito);
            _lblAlt = CrearLabelStat("⚡ Alterados: 0", ColorConstants.AlertaAtaque);
            _lblThroughput = CrearLabelStat("📊 Throughput: 0.0/s", ColorConstants.AcentoPrincipal);
            _lblTiempo = CrearLabelStat("⏱ Tiempo: 00:00", ColorConstants.TextoSecundario);
            flowStats.Controls.AddRange(new Control[] { _lblGen, _lblComp, _lblAlt, _lblThroughput, _lblTiempo });
            _panelStats.Controls.Add(flowStats);

            // Semáforos
            var panelSem = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorConstants.FondoPanel,
                Padding = new Padding(12)
            };
            _vizSemEspacios = new SemaphoreVisualizer { Dock = DockStyle.Top, Height = 72 };
            _vizSemItems = new SemaphoreVisualizer { Dock = DockStyle.Top, Height = 72 };
            _vizSemEspacios.ActualizarSemaforo("Sem: Espacios Libres", AppConstants.CapacidadMaximaCola, AppConstants.CapacidadMaximaCola);
            _vizSemItems.ActualizarSemaforo("Sem: Items Disponibles", 0, AppConstants.CapacidadMaximaCola);
            panelSem.Controls.Add(_vizSemItems);
            panelSem.Controls.Add(_vizSemEspacios);

            // LogViewer
            _logViewer = new LogViewer { Dock = DockStyle.Fill };
            _logViewer.SetTitulo("📋 Log de Concurrencia");

            // Contenedor de la fila 2
            var panelFila2 = new Panel { Dock = DockStyle.Fill, BackColor = ColorConstants.FondoPrincipal };
            var tblFila2 = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = ColorConstants.FondoPrincipal,
                Padding = new Padding(4)
            };
            tblFila2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tblFila2.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tblFila2.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var panelIzqFila2 = new Panel { Dock = DockStyle.Fill, BackColor = ColorConstants.FondoPrincipal };
            panelIzqFila2.Controls.Add(_panelStats);
            panelIzqFila2.Controls.Add(panelSem);
            panelSem.Dock = DockStyle.Fill;
            _panelStats.Dock = DockStyle.Top;
            _panelStats.Height = 80;

            tblFila2.Controls.Add(panelIzqFila2, 0, 0);
            tblFila2.Controls.Add(_logViewer, 1, 0);
            panelFila2.Controls.Add(tblFila2);

            // Ensamblar tabla principal
            tbl.Controls.Add(_panelClientes, 0, 0);
            tbl.Controls.Add(_panelBuffer, 1, 0);
            tbl.Controls.Add(_panelCocineros, 2, 0);
            // Fila 2: usar panelFila2
            tbl.SetRowSpan(panelFila2, 1);
            tbl.SetColumnSpan(panelFila2, 3);
            tbl.Controls.Add(panelFila2, 0, 1);

            panelCuerpo.Controls.Add(tbl);
            Controls.Add(panelCuerpo);
        }

        private Label CrearLabelStat(string texto, Color color)
        {
            return new Label
            {
                Text = texto,
                Font = AppTheme.FuenteLabelBold,
                ForeColor = color,
                BackColor = ColorConstants.FondoPanel,
                AutoSize = false,
                Height = 44,
                Width = 210,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(8, 4, 8, 4)
            };
        }

        // ─── PANEL DE ATAQUE ─────────────────────────────────────────────────

        private void ConstruirPanelAtaque()
        {
            _panelAtaque = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 64,
                BackColor = ColorConstants.FondoAtaque,
                Visible = false,
                Padding = new Padding(16, 0, 16, 0)
            };
            _panelAtaque.Paint += (_, e) =>
            {
                using var pen = new Pen(ColorConstants.AlertaAtaque, 2);
                e.Graphics.DrawLine(pen, 0, 0, _panelAtaque.Width, 0);
            };

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = ColorConstants.FondoAtaque,
                Padding = new Padding(0)
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var panelTexto = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorConstants.FondoAtaque,
                Padding = new Padding(8, 0, 0, 0)
            };
            var lblTit = new Label
            {
                Text = "⚡ ATAQUE ACTIVO: Inyección de Pedidos en cola",
                Font = AppTheme.FuenteLabelBold,
                ForeColor = ColorConstants.AlertaAtaque,
                BackColor = ColorConstants.FondoAtaque,
                Dock = DockStyle.Top,
                Height = 28,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var lblDesc = new Label
            {
                Text = "Un falso técnico instaló un agente malicioso. Pedidos duplicados, eliminados y alterados.",
                Font = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.TextoSecundario,
                BackColor = ColorConstants.FondoAtaque,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            panelTexto.Controls.Add(lblDesc);
            panelTexto.Controls.Add(lblTit);

            _btnVerPoliticas = new Button
            {
                Text = "🛡 Ver Políticas",
                Dock = DockStyle.Fill,
                Font = AppTheme.FuenteBoton,
                Margin = new Padding(12, 8, 0, 8)
            };
            AppTheme.AplicarABotonSeguridad(_btnVerPoliticas);
            _btnVerPoliticas.Click += (_, _) => AbrirPoliticas();

            tbl.Controls.Add(panelTexto, 0, 0);
            tbl.Controls.Add(_btnVerPoliticas, 1, 0);
            _panelAtaque.Controls.Add(tbl);
            Controls.Add(_panelAtaque);
        }

        // ─── STATUS ──────────────────────────────────────────────────────────

        private void ConstruirStatus()
        {
            _status = new StatusStrip
            {
                BackColor = ColorConstants.FondoSuperior,
                ForeColor = ColorConstants.TextoSecundario,
                SizingGrip = false,
                Padding = new Padding(12, 0, 12, 0),
                RenderMode = ToolStripRenderMode.Professional
            };
            _lblStatus = new ToolStripStatusLabel
            {
                Text = "Listo — presione Iniciar",
                Spring = true,
                ForeColor = ColorConstants.TextoSecundario,
                Font = new Font("Consolas", 10f)
            };
            _lblCola = new ToolStripStatusLabel
            {
                Text = "Cola: 0/15",
                ForeColor = ColorConstants.AcentoPrincipal,
                Font = new Font("Consolas", 10f, FontStyle.Bold)
            };
            _status.Items.AddRange(new ToolStripItem[] { _lblStatus, _lblCola });
            Controls.Add(_status);
        }

        // ─── EVENTOS ─────────────────────────────────────────────────────────

        private void ConfigurarEventos()
        {
            _btnIniciar.Click += (_, _) => Iniciar();
            _btnPausar.Click += (_, _) => PausarReanudar();
            _btnDetener.Click += (_, _) => Detener();
            _btnAgregarCliente.Click += (_, _) => _service.AgregarProductor();
            _btnAgregarCocinero.Click += (_, _) => _service.AgregarConsumidor();
            _btnVaciarCola.Click += (_, _) => _service.VaciarCola();
            _btnAtaque.Click += (_, _) => ToggleAtaque();

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
                if (InvokeRequired) { BeginInvoke(() => OnEstadoCambiado(estado)); return; }
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
                if (_service.EstaCorreindo) _service.Detener();
                if (_attackService.IsAttackActive) _attackService.DesactivarAtaque();
            };
        }

        // ─── ACCIONES ────────────────────────────────────────────────────────

        private void CerrarVentana()
        {
            _timerUI.Stop();
            if (_service.EstaCorreindo) _service.Detener();
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
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PausarReanudar()
        {
            if (_service.Estado == SimulationStatus.Corriendo || _service.Estado == SimulationStatus.BajoAtaque)
            {
                _service.Pausar();
                _btnPausar.Text = "▶ Reanudar";
            }
            else if (_service.Estado == SimulationStatus.Pausada)
            {
                _service.Reanudar();
                _btnPausar.Text = "⏸ Pausar";
            }
        }

        private void Detener()
        {
            _timerUI.Stop();
            _service.Detener();
            if (_attackService.IsAttackActive) _attackService.DesactivarAtaque();
            ActualizarBotones(SimulationStatus.Detenida);
            LimpiarFlows();
            _bufferBlocks.Actualizar(0);
        }

        private void ToggleAtaque()
        {
            if (!_service.EstaCorreindo)
            {
                MessageBox.Show("Inicie la simulación primero.", "Simulación no activa",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_attackService.IsAttackActive)
            {
                _attackService.DesactivarAtaque();
                _btnAtaque.Text = "⚡ Ataque";
                _btnAtaque.BackColor = ColorConstants.AlertaAtaque;
            }
            else
            {
                using var dlg = new FrmIngenieriaSocial(
                    "🔧 Soporte Técnico — Actualización",
                    "Estimado Encargado:\n\nSoy Carlos Martínez, técnico de SistemaResto S.A.\n" +
                    "Necesito instalar una actualización crítica.\n\n⚠ Es URGENTE para evitar pérdida de datos.\n\n" +
                    "¿Me permite acceder al sistema?",
                    "Aceptar actualización",
                    "Rechazar");

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _attackService.ActivarAtaque(AttackType.InyeccionDePedidos);
                    _btnAtaque.Text = "🛡 Desactivar Ataque";
                    _btnAtaque.BackColor = ColorConstants.AcentoExito;
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

        // ─── ACTUALIZACIÓN UI ────────────────────────────────────────────────

        private void TimerUI_Tick(object? sender, EventArgs e)
        {
            if (_service.Estado == SimulationStatus.Detenida) return;
            var stats = _service.ObtenerEstadisticas();
            ActualizarStats(stats);
            ActualizarBuffer(stats);
            ActualizarHilos();
        }

        private void ActualizarStats(SimulationStatistics stats)
        {
            _lblGen.Text = $"📥 Generados: {stats.TotalPedidosGenerados}";
            _lblComp.Text = $"✅ Completados: {stats.TotalPedidosCompletados}";
            _lblAlt.Text = $"⚡ Alterados: {stats.TotalPedidosAlterados}";
            _lblThroughput.Text = $"📊 Throughput: {stats.PedidosPorSegundo:F1}/s";
            _lblTiempo.Text = $"⏱ Tiempo: {stats.TiempoEjecucion:mm\\:ss}";
            _lblCola.Text = $"Cola: {stats.PedidosEnCola}/{stats.CapacidadMaximaCola}";
            _lblStatus.Text = $"{stats.EstadoActual}  |  Prod: {stats.ProductoresActivos}  |  Cons: {stats.ConsumidoresActivos}";

            _vizSemEspacios.ActualizarSemaforo("Sem: Espacios Libres", _service.EspaciosLibres, AppConstants.CapacidadMaximaCola);
            _vizSemItems.ActualizarSemaforo("Sem: Items Disponibles", _service.ItemsDisponibles, AppConstants.CapacidadMaximaCola);
        }

        private void ActualizarBuffer(SimulationStatistics stats)
        {
            _bufferBlocks.Actualizar(stats.PedidosEnCola);
        }

        private void ActualizarHilos()
        {
            var clientes = _service.Clientes;
            var cocineros = _service.Cocineros;

            SincronizarFlow(_flowProductores, clientes.Count, i =>
            {
                if (i >= clientes.Count) return;
                var c = clientes[i];
                var ind = (ThreadStatusIndicator)_flowProductores.Controls[i];
                ind.ActualizarEstado(
                    c.Id,
                    c.EstaActivo ? ThreadStatusIndicator.EstadoHilo.Activo : ThreadStatusIndicator.EstadoHilo.Detenido,
                    c.UltimoPedido != null ? $"Último: #{c.UltimoPedido.NumeroPedido:D3}" : "Esperando...");
            });

            SincronizarFlow(_flowConsumidores, cocineros.Count, i =>
            {
                if (i >= cocineros.Count) return;
                var c = cocineros[i];
                var ind = (ThreadStatusIndicator)_flowConsumidores.Controls[i];
                ind.ActualizarEstado(
                    $"{c.Id} [{c.Especialidad}]",
                    c.PedidoActual != null ? ThreadStatusIndicator.EstadoHilo.Activo :
                    c.EstaActivo ? ThreadStatusIndicator.EstadoHilo.Esperando : ThreadStatusIndicator.EstadoHilo.Detenido,
                    c.PedidoActual != null ? $"Preparando #{c.PedidoActual.NumeroPedido:D3}" : "Esperando pedido...",
                    c.ProgresoActual);
            });
        }

        private void SincronizarFlow(FlowLayoutPanel flow, int cant, Action<int> update)
        {
            while (flow.Controls.Count < cant)
                flow.Controls.Add(new ThreadStatusIndicator
                {
                    Width = flow.Width - 16,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right,
                    Margin = new Padding(4)
                });
            for (int i = 0; i < cant; i++) update(i);
        }

        private void LimpiarFlows()
        {
            _flowProductores.Controls.Clear();
            _flowConsumidores.Controls.Clear();
        }

        // ─── ESTADO ──────────────────────────────────────────────────────────

        private void OnEstadoCambiado(SimulationStatus estado)
        {
            ActualizarBotones(estado);
            (_badgeEstado.Text, _badgeEstado.ColorAcento) = estado switch
            {
                SimulationStatus.Corriendo => ("▶ Corriendo", ColorConstants.AcentoExito),
                SimulationStatus.Pausada => ("⏸ Pausada", ColorConstants.EstadoEsperando),
                SimulationStatus.Deteniendo => ("⏹ Deteniendo...", ColorConstants.TextoHint),
                SimulationStatus.BajoAtaque => ("⚡ Bajo Ataque", ColorConstants.AlertaAtaque),
                _ => ("● Detenida", ColorConstants.TextoHint)
            };
        }

        private void ActualizarBotones(SimulationStatus estado)
        {
            bool corriendo = estado == SimulationStatus.Corriendo || estado == SimulationStatus.BajoAtaque;
            bool pausada = estado == SimulationStatus.Pausada;
            bool detenida = estado == SimulationStatus.Detenida;

            _btnIniciar.Enabled = detenida;
            _btnPausar.Enabled = corriendo || pausada;
            _btnDetener.Enabled = !detenida;
            _btnAgregarCliente.Enabled = corriendo || pausada;
            _btnAgregarCocinero.Enabled = corriendo || pausada;
            _btnVaciarCola.Enabled = corriendo || pausada;
            _btnAtaque.Enabled = corriendo;

            if (detenida)
            {
                _btnIniciar.BackColor = ColorConstants.AcentoPrincipal;
                _btnIniciar.ForeColor = Color.White;
            }
            else
            {
                _btnIniciar.BackColor = ColorConstants.Separador;
                _btnIniciar.ForeColor = ColorConstants.TextoHint;
            }
        }

        private void MostrarAtaque() => _panelAtaque.Visible = true;
        private void OcultarAtaque() => _panelAtaque.Visible = false;

        private static int VelAMs(int v) => (int)(3000 / Math.Pow(v, 0.8));

        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x20000;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }
    }

    // ─── CONTROL DE BUFFER VISUAL ────────────────────────────────────────────

    public sealed class BufferBlocks : UserControl
    {
        private int _capacidadMaxima = 15;
        private int _ocupacion = 0;
        private readonly System.Windows.Forms.Timer _animTimer;
        private float _pulse = 0f;

        public int CapacidadMaxima
        {
            get => _capacidadMaxima;
            set { _capacidadMaxima = Math.Max(1, value); Invalidate(); }
        }

        public BufferBlocks()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
            BackColor = ColorConstants.FondoCard;

            _animTimer = new System.Windows.Forms.Timer { Interval = 50 };
            _animTimer.Tick += (_, _) =>
            {
                _pulse += 0.05f;
                if (_pulse > MathF.Tau) _pulse -= MathF.Tau;
                Invalidate();
            };
            _animTimer.Start();
        }

        public void Actualizar(int ocupacion)
        {
            _ocupacion = Math.Clamp(ocupacion, 0, _capacidadMaxima);
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(BackColor);

            if (Width < 20 || Height < 20) return;

            int padding = 10;
            int espaciado = 8;
            int anchoDisponible = Width - padding * 2;
            int altoBloque = Height - padding * 2;
            int anchoBloque = (anchoDisponible - espaciado * (_capacidadMaxima - 1)) / _capacidadMaxima;
            anchoBloque = Math.Max(anchoBloque, 10);

            float ocupacionPct = _capacidadMaxima > 0 ? (float)_ocupacion / _capacidadMaxima : 0;
            Color colorBase = ocupacionPct > 0.8f ? ColorConstants.AlertaAtaque :
                              ocupacionPct > 0.5f ? ColorConstants.EstadoEsperando :
                              ColorConstants.AcentoPrincipal;

            float glow = (MathF.Sin(_pulse) + 1f) / 2f;

            for (int i = 0; i < _capacidadMaxima; i++)
            {
                int x = padding + i * (anchoBloque + espaciado);
                var rect = new Rectangle(x, padding, anchoBloque, altoBloque);

                bool lleno = i < _ocupacion;
                Color color = lleno ? colorBase : ColorConstants.Separador;

                using var shadowBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0));
                g.FillRectangle(shadowBrush, rect.X + 2, rect.Y + 2, rect.Width, rect.Height);

                using var brush = new SolidBrush(lleno ? color : Color.FromArgb(40, ColorConstants.Separador));
                g.FillRectangle(brush, rect);

                using var pen = new Pen(lleno ? Color.FromArgb(200, color) : ColorConstants.Separador, 1);
                g.DrawRectangle(pen, rect);

                if (lleno)
                {
                    int brillo = (int)(40 + 30 * glow);
                    using var glowBrush = new SolidBrush(Color.FromArgb(brillo, Color.White));
                    g.FillRectangle(glowBrush, rect.X + 2, rect.Y + 2, rect.Width - 4, 4);
                }

                if (anchoBloque > 20 && altoBloque > 16)
                {
                    string texto = (i + 1).ToString();
                    using var font = new Font("Consolas", Math.Min(anchoBloque * 0.4f, 10f), FontStyle.Bold);
                    var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString(texto, font, new SolidBrush(lleno ? Color.White : ColorConstants.TextoHint), rect, sf);
                }
            }

            string label = $"{_ocupacion} / {_capacidadMaxima}";
            using var fLabel = AppTheme.FuenteSmallBold;
            var size = TextRenderer.MeasureText(label, fLabel);
            var rectLabel = new Rectangle(Width - size.Width - 12, Height - size.Height - 6, size.Width, size.Height);
            TextRenderer.DrawText(g, label, fLabel, rectLabel,
                ocupacionPct > 0.8f ? ColorConstants.AlertaAtaque :
                ocupacionPct > 0.5f ? ColorConstants.EstadoEsperando :
                ColorConstants.AcentoPrincipal,
                TextFormatFlags.Right | TextFormatFlags.Bottom);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animTimer?.Stop();
                _animTimer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}