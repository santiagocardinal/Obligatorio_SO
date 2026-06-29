using RestauranteSO.Constants;
using RestauranteSO.Domain.Enums;
using RestauranteSO.Domain.Interfaces;
using RestauranteSO.Presentation.Components;
using RestauranteSO.Presentation.Controls;
using RestauranteSO.Presentation.Themes;
using RestauranteSO.Services.Ataques;
using RestauranteSO.Services.LectoresEscritores;

namespace RestauranteSO.Presentation.Forms
{
    public sealed class FrmLectoresEscritores : Form
    {
        private readonly LectoresEscritoresService       _service;
        private readonly AtaqueLectoresEscritoresService _attackService;
        private readonly ISimulationLogger               _logger;

        // ─── HEADER ───────────────────────────────────────────────────────────
        private Panel       _panelHeader  = null!;
        private Label       _lblTitulo    = null!;
        private StatusBadge _badgeEstado  = null!;
        private Button      _btnVolver    = null!;

        // ─── TOOLBAR ──────────────────────────────────────────────────────────
        private Panel    _panelToolbar     = null!;
        private Button   _btnIniciar       = null!;
        private Button   _btnPausar        = null!;
        private Button   _btnDetener       = null!;
        private Button   _btnAgregarLector = null!;
        private Button   _btnActivarAtaque = null!;
        private TrackBar _trackVelLector   = null!;
        private Label    _lblVelL          = null!;

        // ─── PANEL CONTROL EXTENDIDO ──────────────────────────────────────────
        private Panel  _panelControl      = null!;
        private Button _btnForzarLectura  = null!;
        private Button _btnForzarEscritura= null!;
        private Button _btnVerPoliticasCtrl= null!;

        // ─── CUERPO ───────────────────────────────────────────────────────────
        private Panel               _panelIzq          = null!;
        private SemaphoreVisualizer _vizRWLock          = null!;
        private Panel               _panelGerenteVisual = null!;
        private Label               _lblGerenteEstado   = null!;
        private Label               _lblGerenteAccion   = null!;
        private FlowLayoutPanel     _flowMeseros        = null!;

        private Panel        _panelMenu      = null!;
        private DataGridView _gridMenu       = null!;
        private Label        _lblMenuTitulo  = null!;
        private Label        _lblMenuVersion = null!;

        private Panel     _panelDer            = null!;
        private Label     _lblStatLecturas      = null!;
        private Label     _lblStatEscrituras    = null!;
        private Label     _lblStatComprometidos = null!;
        private Label     _lblStatTiempo        = null!;
        private LogViewer _logViewer            = null!;

        private Panel  _panelAtaque     = null!;
        private Button _btnVerPoliticas = null!;

        private StatusStrip          _statusStrip   = null!;
        private ToolStripStatusLabel _lblStatus     = null!;
        private ToolStripStatusLabel _lblLockStatus = null!;

        private System.Windows.Forms.Timer _timerUI = null!;

        public event EventHandler<bool>? SimulacionCambiada;

        public FrmLectoresEscritores(
            LectoresEscritoresService service,
            AtaqueLectoresEscritoresService attackService,
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
            Text          = "📖 Simulación Lectores — Escritores | RestaurantOS";
            WindowState   = FormWindowState.Maximized;
            MinimumSize   = new Size(1280, 800);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor     = ColorConstants.FondoPrincipal;
            ForeColor     = ColorConstants.TextoPrincipal;

            ConstruirHeader();
            ConstruirToolbar();
            ConstruirCuerpo();
            ConstruirPanelAtaque();
            ConstruirStatusStrip();

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
                using var pen = new Pen(ColorConstants.TarjetaLectores, 2);
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
                Font     = new Font("Segoe UI", 11f, FontStyle.Bold)
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
                Text      = "📖  Simulación Lectores — Escritores",
                Font      = new Font("Segoe UI", 18f, FontStyle.Bold),
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

            _btnIniciar       = CrearBtn("▶ Iniciar",      150, true,  false);
            _btnPausar        = CrearBtn("⏸ Pausar",       140, false, false);
            _btnDetener       = CrearBtn("⏹ Detener",      140, false, false);

            var sep1 = CrearSep();

            _btnAgregarLector = CrearBtn("+ Mesero",       120, false, false);

            var sep2 = CrearSep();

            var lblVel = new Label
            {
                Text      = "Velocidad Lect:",
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = ColorConstants.TextoHint,
                BackColor = ColorConstants.FondoPanel,
                AutoSize  = false,
                Size      = new Size(115, 44),
                TextAlign = ContentAlignment.MiddleRight,
                Margin    = new Padding(0)
            };

            _lblVelL = new Label
            {
                Text      = "5x",
                Font      = new Font("Segoe UI", 10f),
                ForeColor = ColorConstants.TarjetaLectores,
                BackColor = ColorConstants.FondoPanel,
                AutoSize  = false,
                Size      = new Size(32, 44),
                TextAlign = ContentAlignment.MiddleCenter,
                Margin    = new Padding(4, 0, 0, 0)
            };

            _trackVelLector = new TrackBar
            {
                Minimum   = 1, Maximum = 10, Value = 5,
                TickStyle = TickStyle.None,
                Size      = new Size(110, 44),
                Font      = new Font("Segoe UI", 9f),
                BackColor = ColorConstants.FondoPanel,
                Margin    = new Padding(0, 0, 8, 0)
            };

            _btnActivarAtaque = new Button
            {
                Text      = "🎣 Simular Phishing",
                Size      = new Size(180, 44),
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                BackColor = ColorConstants.AlertaAtaque,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                Margin    = new Padding(16, 0, 0, 0)
            };
            _btnActivarAtaque.FlatAppearance.BorderSize = 0;

            var tt = new ToolTip { ShowAlways = true, InitialDelay = 400 };
            tt.SetToolTip(_btnIniciar,       "Iniciar simulación");
            tt.SetToolTip(_btnPausar,        "Pausar / Reanudar");
            tt.SetToolTip(_btnDetener,       "Detener simulación");
            tt.SetToolTip(_btnAgregarLector, "Agregar un mesero lector");
            tt.SetToolTip(_btnActivarAtaque, "Simular ataque de phishing al gerente");

            flow.Controls.AddRange(new Control[]
            {
                _btnIniciar, _btnPausar, _btnDetener,
                sep1, _btnAgregarLector,
                sep2, lblVel, _lblVelL, _trackVelLector,
                _btnActivarAtaque
            });

            _panelToolbar.Controls.Add(flow);
            Controls.Add(_panelToolbar);
        }

        // ─── CUERPO ───────────────────────────────────────────────────────────

        private void ConstruirCuerpo()
        {
            var panelCuerpo = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = ColorConstants.FondoPrincipal
            };

            _panelIzq = new Panel
            {
                Dock      = DockStyle.Left,
                BackColor = ColorConstants.FondoLateral,
                Padding   = new Padding(8)
            };

            _panelMenu = new Panel
            {
                Dock      = DockStyle.Left,
                BackColor = ColorConstants.FondoPrincipal,
                Padding   = new Padding(8)
            };

            // Panel control extendido
            _panelControl = new Panel
            {
                Dock      = DockStyle.Left,
                BackColor = Color.FromArgb(18, 18, 30),
                Padding   = new Padding(8)
            };
            ConstruirPanelControlExtendido(_panelControl);

            _panelDer = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = ColorConstants.FondoLateral,
                Padding   = new Padding(8)
            };

            void redistribuir()
            {
                if (panelCuerpo.Width < 100) return;
                _panelIzq.Width     = (int)(panelCuerpo.Width * 0.24);
                _panelMenu.Width    = (int)(panelCuerpo.Width * 0.36);
                _panelControl.Width = (int)(panelCuerpo.Width * 0.14);
            }

            panelCuerpo.Resize += (_, _) => redistribuir();
            Shown              += (_, _) => redistribuir();
            Resize             += (_, _) => redistribuir();

            ConstruirPanelIzquierdo();
            ConstruirPanelMenu();
            ConstruirPanelDerecho();

            panelCuerpo.Controls.Add(_panelDer);
            panelCuerpo.Controls.Add(_panelControl);
            panelCuerpo.Controls.Add(_panelMenu);
            panelCuerpo.Controls.Add(_panelIzq);
            Controls.Add(panelCuerpo);
        }

        // ─── PANEL CONTROL EXTENDIDO ─────────────────────────────────────────

        private void ConstruirPanelControlExtendido(Panel panel)
        {
            var scroll = new Panel
            {
                Dock       = DockStyle.Fill,
                AutoScroll = true,
                BackColor  = Color.FromArgb(18, 18, 30),
                Padding    = new Padding(6)
            };

            var lblTit = new Label
            {
                Text      = "🎛 Centro de Control",
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = ColorConstants.TarjetaLectores,
                BackColor = Color.FromArgb(18, 18, 30),
                Dock      = DockStyle.Top,
                Height    = 30,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var sep = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 1,
                BackColor = ColorConstants.Separador
            };

            var lblSecForzar = CrearSeccion("⚙ Forzar Operación");

            _btnForzarLectura = CrearBtnCtrl("📖 Forzar Lectura",
                ColorConstants.TarjetaLectores);
            _btnForzarLectura.Click += (_, _) =>
            {
                if (_service.EstaCorreindo)
                    _service.AgregarLector();
            };

            _btnForzarEscritura = CrearBtnCtrl("✏ Forzar Escritura",
                ColorConstants.AcentoSecundario);
            _btnForzarEscritura.Click += (_, _) =>
            {
                if (!_service.EstaCorreindo)
                    return;

                MessageBox.Show(
                    "La escritura del gerente ya se realiza automáticamente durante la simulación.",
                    "Información",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            };

            var lblSecEsc = CrearSeccion("🧪 Escenarios");

            var btnNormal = CrearBtnCtrl("🟢 Normal", ColorConstants.AcentoExito);
            btnNormal.Click += (_, _) =>
            {
                _trackVelLector.Value = 5;
                if (_service.EstaCorreindo)
                    _service.AjustarVelocidad(1000);
            };

            var btnLect = CrearBtnCtrl("📖 Lectores++", ColorConstants.TarjetaLectores);
            btnLect.Click += (_, _) =>
            {
                if (_service.EstaCorreindo)
                    for (int i = 0; i < 4; i++) _service.AgregarLector();
            };

            var btnEscr = CrearBtnCtrl("✏ Escritor perm.", ColorConstants.AcentoSecundario);
            btnEscr.Click += (_, _) =>
            {
                _trackVelLector.Value = 8;
                if (_service.EstaCorreindo)
                    _service.AjustarVelocidad(300);
            };

            var lblSecAcc = CrearSeccion("⚡ Acciones");

            _btnVerPoliticasCtrl = CrearBtnCtrl("🛡 Ver Políticas",
                ColorConstants.AcentoExito);
            _btnVerPoliticasCtrl.Click += (_, _) => AbrirPoliticas();

            var btnLimpiarLog = CrearBtnCtrl("🗑 Limpiar Log", ColorConstants.TextoHint);
            btnLimpiarLog.Click += (_, _) => _logViewer?.Limpiar();

            scroll.Controls.AddRange(new Control[]
            {
                lblTit, sep,
                lblSecForzar,
                _btnForzarLectura, _btnForzarEscritura,
                lblSecEsc,
                btnNormal, btnLect, btnEscr,
                lblSecAcc,
                _btnVerPoliticasCtrl, btnLimpiarLog
            });

            panel.Controls.Add(scroll);
        }

        private static Label CrearSeccion(string texto) => new Label
        {
            Text      = texto,
            Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
            ForeColor = ColorConstants.TextoHint,
            BackColor = Color.FromArgb(18, 18, 30),
            Dock      = DockStyle.Top,
            Height    = 24,
            TextAlign = ContentAlignment.BottomLeft,
            Margin    = new Padding(0, 8, 0, 2)
        };

        private static Button CrearBtnCtrl(string texto, Color color)
        {
            var btn = new Button
            {
                Text      = texto,
                Dock      = DockStyle.Top,
                Height    = 34,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                BackColor = Color.FromArgb(28, 28, 44),
                ForeColor = color,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(6, 0, 0, 0),
                Margin    = new Padding(0, 2, 0, 2)
            };
            btn.FlatAppearance.BorderColor = Color.FromArgb(40, color);
            btn.FlatAppearance.BorderSize  = 1;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, color);
            return btn;
        }

        // ─── PANELES INTERNOS ────────────────────────────────────────────────

        private void ConstruirPanelIzquierdo()
        {
            var lblRW = new Label
            {
                Text      = "🔐 ReaderWriterLockSlim",
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = ColorConstants.TarjetaLectores,
                BackColor = ColorConstants.FondoLateral,
                Dock      = DockStyle.Top,
                Height    = 32,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(2, 0, 0, 0)
            };

            _vizRWLock = new SemaphoreVisualizer
            {
                Dock   = DockStyle.Top,
                Height = 112
            };
            _vizRWLock.ActualizarReaderWriterLock(0, 0, false, false);

            _panelGerenteVisual = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 96,
                BackColor = ColorConstants.FondoCard,
                Padding   = new Padding(8)
            };
            _panelGerenteVisual.Paint += (_, e) =>
            {
                var color = _service.GerenteEscribiendo
                    ? ColorConstants.AlertaAtaque
                    : ColorConstants.Separador;
                using var pen = new Pen(color, 2);
                e.Graphics.DrawRectangle(pen, 1, 1,
                    _panelGerenteVisual.Width - 3,
                    _panelGerenteVisual.Height - 3);
            };

            var lblGerenteTit = new Label
            {
                Text      = "👔 Gerente (Escritor)",
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = ColorConstants.AcentoSecundario,
                BackColor = ColorConstants.FondoCard,
                AutoSize  = false,
                Dock      = DockStyle.Top,
                Height    = 28,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _lblGerenteEstado = new Label
            {
                Text      = "⏳ En espera",
                Font      = new Font("Segoe UI", 11f),
                ForeColor = ColorConstants.TextoHint,
                BackColor = ColorConstants.FondoCard,
                AutoSize  = false,
                Dock      = DockStyle.Top,
                Height    = 26,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _lblGerenteAccion = new Label
            {
                Text      = "—",
                Font      = new Font("Segoe UI", 10f),
                ForeColor = ColorConstants.TextoHint,
                BackColor = ColorConstants.FondoCard,
                AutoSize  = false,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _panelGerenteVisual.Controls.Add(_lblGerenteAccion);
            _panelGerenteVisual.Controls.Add(_lblGerenteEstado);
            _panelGerenteVisual.Controls.Add(lblGerenteTit);

            var sep = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 1,
                BackColor = ColorConstants.Separador
            };

            var lblMeseros = new Label
            {
                Text      = "🧑\u200d🍽 Meseros (Lectores)",
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = ColorConstants.TarjetaLectores,
                BackColor = ColorConstants.FondoLateral,
                Dock      = DockStyle.Top,
                Height    = 32,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(2, 0, 0, 0)
            };

            _flowMeseros = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents  = false,
                AutoScroll    = true,
                BackColor     = ColorConstants.FondoPanel,
                Padding       = new Padding(4)
            };

            _panelIzq.Controls.Add(_flowMeseros);
            _panelIzq.Controls.Add(lblMeseros);
            _panelIzq.Controls.Add(sep);
            _panelIzq.Controls.Add(_panelGerenteVisual);
            _panelIzq.Controls.Add(_vizRWLock);
            _panelIzq.Controls.Add(lblRW);
        }

        private void ConstruirPanelMenu()
        {
            _lblMenuTitulo = new Label
            {
                Text      = "📋 Menú del Restaurante (Recurso Compartido)",
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = ColorConstants.TextoSecundario,
                BackColor = ColorConstants.FondoPrincipal,
                Dock      = DockStyle.Top,
                Height    = 34,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(4, 0, 0, 0)
            };

            _lblMenuVersion = new Label
            {
                Text      = "Versión del menú: —",
                Font      = new Font("Segoe UI", 10f),
                ForeColor = ColorConstants.TextoHint,
                BackColor = ColorConstants.FondoPrincipal,
                Dock      = DockStyle.Top,
                Height    = 26,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(4, 0, 0, 0)
            };

            _gridMenu = new DataGridView { Dock = DockStyle.Fill };
            AppTheme.AplicarADataGrid(_gridMenu);

            _gridMenu.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "Id",      HeaderText = "#",              Width = 34, FillWeight = 5  });
            _gridMenu.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "Nombre",  HeaderText = "Plato",          FillWeight = 35 });
            _gridMenu.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "Precio",  HeaderText = "Precio",         Width = 76, FillWeight = 12 });
            _gridMenu.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "Cat",     HeaderText = "Categoría",      FillWeight = 18 });
            _gridMenu.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "Version", HeaderText = "Ver.",           Width = 44, FillWeight = 6  });
            _gridMenu.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "ModPor",  HeaderText = "Modificado por", FillWeight = 24 });

            _panelMenu.Controls.Add(_gridMenu);
            _panelMenu.Controls.Add(_lblMenuVersion);
            _panelMenu.Controls.Add(_lblMenuTitulo);
        }

        private void ConstruirPanelDerecho()
        {
            var lblStats = new Label
            {
                Text      = "📊 Estadísticas",
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = ColorConstants.TextoSecundario,
                BackColor = ColorConstants.FondoLateral,
                Dock      = DockStyle.Top,
                Height    = 32,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(4, 0, 0, 0)
            };

            var panelStats = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 108,
                BackColor = ColorConstants.FondoPanel,
                Padding   = new Padding(8)
            };

            var flowStats = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                BackColor     = ColorConstants.FondoPanel,
                Padding       = new Padding(0)
            };

            _lblStatLecturas      = CrearLabelStat("📖 Lecturas: 0");
            _lblStatEscrituras    = CrearLabelStat("✏ Escrituras: 0");
            _lblStatComprometidos = CrearLabelStat("⚡ Comprometidos: 0");
            _lblStatTiempo        = CrearLabelStat("⏱ Tiempo: 00:00");

            flowStats.Controls.AddRange(new Control[]
            {
                _lblStatLecturas, _lblStatEscrituras,
                _lblStatComprometidos, _lblStatTiempo
            });
            panelStats.Controls.Add(flowStats);

            var sep = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 1,
                BackColor = ColorConstants.Separador
            };

            _logViewer = new LogViewer { Dock = DockStyle.Fill };
            _logViewer.SetTitulo("📋 Log de ReaderWriterLockSlim");

            _panelDer.Controls.Add(_logViewer);
            _panelDer.Controls.Add(sep);
            _panelDer.Controls.Add(panelStats);
            _panelDer.Controls.Add(lblStats);
        }

        private void ConstruirPanelAtaque()
        {
            _panelAtaque = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 72,
                BackColor = ColorConstants.FondoAtaque,
                Visible   = false,
                Padding   = new Padding(12, 0, 12, 0)
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
                Padding     = new Padding(0, 8, 0, 8)
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 72f));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 28f));

            var panelTexto = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = ColorConstants.FondoAtaque
            };
            var lblTit = new Label
            {
                Text      = "🎣 ATAQUE ACTIVO: Phishing — Menú Comprometido",
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = ColorConstants.AlertaAtaque,
                BackColor = ColorConstants.FondoAtaque,
                AutoSize  = false,
                Dock      = DockStyle.Top,
                Height    = 26,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var lblDesc = new Label
            {
                Text      = "Credenciales del Gerente comprometidas. " +
                             "Menú alterado. Meseros leen información incorrecta.",
                Font      = new Font("Segoe UI", 10f),
                ForeColor = ColorConstants.TextoSecundario,
                BackColor = ColorConstants.FondoAtaque,
                AutoSize  = false,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            panelTexto.Controls.Add(lblDesc);
            panelTexto.Controls.Add(lblTit);

            _btnVerPoliticas = new Button
            {
                Text   = "🛡 Ver Políticas",
                Dock   = DockStyle.Fill,
                Font   = new Font("Segoe UI", 11f, FontStyle.Bold),
                Margin = new Padding(8, 4, 0, 4)
            };
            AppTheme.AplicarABotonSeguridad(_btnVerPoliticas);
            _btnVerPoliticas.Click += (_, _) => AbrirPoliticas();

            tbl.Controls.Add(panelTexto,       0, 0);
            tbl.Controls.Add(_btnVerPoliticas, 1, 0);
            _panelAtaque.Controls.Add(tbl);
            Controls.Add(_panelAtaque);
        }

        private void ConstruirStatusStrip()
        {
            _statusStrip = new StatusStrip
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
                Font      = new Font("Consolas", 10f)
            };
            _lblLockStatus = new ToolStripStatusLabel
            {
                Text      = "Lock: Libre",
                ForeColor = ColorConstants.AcentoExito,
                Font      = new Font("Consolas", 10f, FontStyle.Bold)
            };
            _statusStrip.Items.AddRange(
                new ToolStripItem[] { _lblStatus, _lblLockStatus });
            Controls.Add(_statusStrip);
        }

        // ─── EVENTOS ─────────────────────────────────────────────────────────

        private void ConfigurarEventos()
        {
            _btnIniciar.Click       += (_, _) => Iniciar();
            _btnPausar.Click        += (_, _) => PausarReanudar();
            _btnDetener.Click       += (_, _) => Detener();
            _btnAgregarLector.Click += (_, _) => _service.AgregarLector();
            _btnActivarAtaque.Click += (_, _) => ToggleAtaque();

            _trackVelLector.ValueChanged += (_, _) =>
            {
                _service.AjustarVelocidad(VelAMs(_trackVelLector.Value));
                _lblVelL.Text = $"{_trackVelLector.Value}x";
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
                MessageBox.Show($"Error al iniciar: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PausarReanudar()
        {
            if (_service.Estado == SimulationStatus.Corriendo ||
                _service.Estado == SimulationStatus.BajoAtaque)
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
            SimulacionCambiada?.Invoke(this, false);
            _gridMenu.Rows.Clear();
            _flowMeseros.Controls.Clear();
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
                _btnActivarAtaque.Text      = "🎣 Simular Phishing";
                _btnActivarAtaque.BackColor = ColorConstants.AlertaAtaque;
            }
            else
            {
                using var dlg = new FrmIngenieriaSocial(
                    "📧 Correo — Sistema de Gestión Restaurante",
                    "De: sistema@gestionresto-soporte.com\n" +
                    "Para: gerente@restaurantedoncod.com\n" +
                    "Asunto: ⚠ URGENTE: Su acceso expirará en 24 horas\n\n" +
                    "Estimado Gerente:\n\n" +
                    "Su sesión en el Sistema de Gestión expirará\n" +
                    "mañana si no renueva sus credenciales.\n\n" +
                    "Haga clic abajo para verificar su identidad.",
                    "✉ Verificar mis credenciales",
                    "Ignorar correo");

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _attackService.ActivarAtaque(AttackType.PhishingMenuAlterado);
                    _btnActivarAtaque.Text      = "🛡 Desactivar Ataque";
                    _btnActivarAtaque.BackColor = ColorConstants.AcentoExito;
                }
            }
        }

        private void AbrirPoliticas()
        {
            using var frm = new FrmPoliticas(
                "Políticas — Phishing al Gerente",
                AtaqueLectoresEscritoresService.ObtenerPoliticas(),
                _attackService.HistorialAtaques);
            frm.ShowDialog(this);
        }

        // ─── ACTUALIZACIÓN UI ─────────────────────────────────────────────────

        private void TimerUI_Tick(object? sender, EventArgs e)
        {
            if (_service.Estado == SimulationStatus.Detenida) return;
            var stats = _service.ObtenerEstadisticas();
            ActualizarEstadisticas(stats);
            ActualizarGridMenu();
            ActualizarGerenteVisual();
            ActualizarMeseros();
            ActualizarVisualizador(stats);
        }

        private void ActualizarEstadisticas(Domain.Models.SimulationStatistics stats)
        {
            _lblStatLecturas.Text      = $"📖 Lecturas: {stats.TotalLecturas}";
            _lblStatEscrituras.Text    = $"✏ Escrituras: {stats.TotalEscrituras}";
            _lblStatComprometidos.Text = $"⚡ Comprometidos: {stats.LectoresComprometidos}";
            _lblStatTiempo.Text        = $"⏱ Tiempo: {stats.TiempoEjecucion:mm\\:ss}";

            _lblLockStatus.Text = stats.EscritorActivo
                ? "🔴 ESCRITURA EXCLUSIVA"
                : stats.LectoresActivos > 0
                    ? $"🟢 {stats.LectoresActivos} leyendo"
                    : "⚪ Lock: Libre";

            _lblLockStatus.ForeColor = stats.EscritorActivo
                ? ColorConstants.AlertaAtaque
                : stats.LectoresActivos > 0
                    ? ColorConstants.AcentoExito
                    : ColorConstants.TextoHint;

            _lblStatus.Text =
                $"{stats.EstadoActual}  |  " +
                $"Lectores: {stats.LectoresActivos}  |  " +
                $"Tiempo: {stats.TiempoEjecucion:mm\\:ss}";
        }

        private void ActualizarGridMenu()
        {
            _gridMenu.SuspendLayout();
            _gridMenu.Rows.Clear();
            try
            {
                var menuRepo = Configuration.AppSettings
                    .Resolver<Domain.Interfaces.IMenuRepository>();
                var items = menuRepo.ObtenerCompleto();

                int versionMax = items.Count > 0
                    ? items.Max(i => i.Version) : 0;
                _lblMenuVersion.Text =
                    $"Versión actual: {versionMax}  " +
                    (_attackService.IsAttackActive
                        ? "⚠ COMPROMETIDA" : "✅ Íntegra");

                foreach (var item in items)
                {
                    int idx = _gridMenu.Rows.Add(
                        item.Id,
                        item.Nombre.Length > 28
                            ? item.Nombre[..28] + "…" : item.Nombre,
                        $"${item.Precio:N2}",
                        item.Categoria,
                        $"v{item.Version}",
                        item.ModificadoPor);

                    if (item.FueAlterado)
                    {
                        _gridMenu.Rows[idx].DefaultCellStyle.BackColor =
                            Color.FromArgb(60, 220, 50, 50);
                        _gridMenu.Rows[idx].DefaultCellStyle.ForeColor =
                            ColorConstants.EstadoAlterado;
                    }
                    else if (item.ModificadoPor != "Sistema")
                    {
                        _gridMenu.Rows[idx].DefaultCellStyle.ForeColor =
                            ColorConstants.AcentoSecundario;
                    }
                }
            }
            catch { }
            _gridMenu.ResumeLayout();
        }

        private void ActualizarGerenteVisual()
        {
            bool escribiendo = _service.GerenteEscribiendo;
            bool esperando   = _service.EscritorEsperando;

            _lblGerenteEstado.Text = escribiendo
                ? "✏ ESCRIBIENDO (WriteLock exclusivo)"
                : esperando ? "⏳ Esperando WriteLock..." : "💤 En espera";

            _lblGerenteEstado.ForeColor = escribiendo
                ? ColorConstants.AlertaAtaque
                : esperando
                    ? ColorConstants.EstadoEsperando
                    : ColorConstants.TextoHint;

            string ultima = _service.UltimaModificacion ?? "—";
            _lblGerenteAccion.Text = ultima.Length > 40
                ? ultima[..40] + "…" : ultima;

            _panelGerenteVisual.Invalidate();
        }

        private void ActualizarMeseros()
        {
            var meseros = _service.Meseros;

            while (_flowMeseros.Controls.Count < meseros.Count)
                _flowMeseros.Controls.Add(new ThreadStatusIndicator
                {
                    Width  = _flowMeseros.Width - 12,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right
                });

            for (int i = 0; i < meseros.Count; i++)
            {
                var m   = meseros[i];
                var ind = (ThreadStatusIndicator)_flowMeseros.Controls[i];

                var estado = m.LeyoMenuComprometido
                    ? ThreadStatusIndicator.EstadoHilo.BajoAtaque
                    : m.EstaLeyendo
                        ? ThreadStatusIndicator.EstadoHilo.Activo
                        : m.EstaEsperando
                            ? ThreadStatusIndicator.EstadoHilo.Esperando
                            : ThreadStatusIndicator.EstadoHilo.Detenido;

                string accion = m.LeyoMenuComprometido
                    ? $"⚠ Info comprometida v{m.VersionMenuLeida}"
                    : m.EstaLeyendo
                        ? $"📖 {m.UltimoItemLeido ?? "—"}"
                        : m.EstaEsperando
                            ? "⏳ Esperando ReadLock..."
                            : $"✓ {m.LecturasCompletadas} lecturas";

                ind.ActualizarEstado(m.Id, estado, accion);
            }
        }

        private void ActualizarVisualizador(Domain.Models.SimulationStatistics stats)
        {
            _vizRWLock.ActualizarReaderWriterLock(
                stats.LectoresActivos,
                stats.LectoresEsperando,
                stats.EscritorActivo,
                stats.EscritorEsperando);
        }

        private void OnEstadoCambiado(SimulationStatus estado)
        {
            ActualizarBotones(estado);
            (_badgeEstado.Text, _badgeEstado.ColorAcento) = estado switch
            {
                SimulationStatus.Corriendo  => ("▶ Corriendo",    ColorConstants.AcentoExito),
                SimulationStatus.Pausada    => ("⏸ Pausada",      ColorConstants.EstadoEsperando),
                SimulationStatus.BajoAtaque => ("🎣 Bajo Ataque", ColorConstants.AlertaAtaque),
                _                           => ("● Detenida",     ColorConstants.TextoHint)
            };
        }

        private void ActualizarBotones(SimulationStatus estado)
        {
            bool corriendo = estado == SimulationStatus.Corriendo ||
                             estado == SimulationStatus.BajoAtaque;
            bool pausada   = estado == SimulationStatus.Pausada;
            bool detenida  = estado == SimulationStatus.Detenida;

            _btnIniciar.Enabled       = detenida;
            _btnPausar.Enabled        = corriendo || pausada;
            _btnDetener.Enabled       = !detenida;
            _btnAgregarLector.Enabled = corriendo || pausada;
            _btnActivarAtaque.Enabled = corriendo;

            _btnForzarLectura.Enabled   = corriendo || pausada;
            _btnForzarEscritura.Enabled = corriendo || pausada;

            AppTheme.AplicarABotonPrimario(_btnIniciar);
            if (!_btnIniciar.Enabled)
            {
                _btnIniciar.BackColor = ColorConstants.Separador;
                _btnIniciar.ForeColor = ColorConstants.TextoHint;
            }
        }

        private void MostrarAtaque() => _panelAtaque.Visible = true;
        private void OcultarAtaque() => _panelAtaque.Visible = false;

        private Label CrearLabelStat(string texto) => new Label
        {
            Text      = texto,
            Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
            ForeColor = ColorConstants.TextoSecundario,
            BackColor = ColorConstants.FondoPanel,
            AutoSize  = true,
            Margin    = new Padding(2)
        };

        private static Panel CrearSep() => new Panel
        {
            Size      = new Size(1, 44),
            BackColor = ColorConstants.Separador,
            Margin    = new Padding(8, 0, 8, 0)
        };

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

        private static int VelAMs(int v) =>
            (int)(3000 / Math.Pow(v, 0.8));
    }
}