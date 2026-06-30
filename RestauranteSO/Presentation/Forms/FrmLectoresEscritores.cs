using RestauranteSO.Constants;
using RestauranteSO.Domain.Enums;
using RestauranteSO.Domain.Interfaces;
using RestauranteSO.Domain.Models;
using RestauranteSO.Presentation.Components;
using RestauranteSO.Presentation.Controls;
using RestauranteSO.Presentation.Themes;
using RestauranteSO.Services.Ataques;
using RestauranteSO.Services.LectoresEscritores;
using System.Drawing.Drawing2D;

namespace RestauranteSO.Presentation.Forms
{
    /// <summary>
    /// Simulación Lectores-Escritores con paleta clara, identidad visual propia,
    /// visualización del ReaderWriterLockSlim y flujo de lectura/escritura.
    /// </summary>
    public sealed class FrmLectoresEscritores : Form
    {
        private readonly LectoresEscritoresService _service;
        private readonly AtaqueLectoresEscritoresService _attackService;
        private readonly ISimulationLogger _logger;

        // ─── BARRA DE TÍTULO ─────────────────────────────────────────────────
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
        private Button _btnAgregarLector = null!;
        private Button _btnAtaque = null!;
        private TrackBar _trackVel = null!;
        private Label _lblVel = null!;

        // ─── CUERPO ──────────────────────────────────────────────────────────
        private Panel _panelIzq = null!;
        private FlowLayoutPanel _flowLectores = null!;
        private Panel _panelGerente = null!;
        private Label _lblGerenteEstado = null!;
        private Label _lblGerenteAccion = null!;
        private SemaphoreVisualizer _vizRWLock = null!;

        private Panel _panelMenu = null!;
        private DataGridView _gridMenu = null!;
        private Label _lblMenuVersion = null!;

        private Panel _panelDer = null!;
        private Label _lblStatLecturas = null!;
        private Label _lblStatEscrituras = null!;
        private Label _lblStatComprometidos = null!;
        private Label _lblStatTiempo = null!;
        private LogViewer _logViewer = null!;

        // ─── PANEL DE ATAQUE ─────────────────────────────────────────────────
        private Panel _panelAtaque = null!;
        private Button _btnVerPoliticas = null!;

        // ─── STATUS ──────────────────────────────────────────────────────────
        private StatusStrip _status = null!;
        private ToolStripStatusLabel _lblStatus = null!;
        private ToolStripStatusLabel _lblLockStatus = null!;

        // ─── TIMERS ──────────────────────────────────────────────────────────
        private System.Windows.Forms.Timer _timerUI = null!;

        private const int TITLE_BAR_HEIGHT = 38;
        private const int PADDING_GLOBAL = 24;

        public FrmLectoresEscritores(
            LectoresEscritoresService service,
            AtaqueLectoresEscritoresService attackService,
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
                using var pen = new Pen(ColorConstants.TarjetaLectores, 3);
                e.Graphics.DrawLine(pen, 0, _titleBar.Height - 2, _titleBar.Width, _titleBar.Height - 2);
            };

            _lblTitulo = new Label
            {
                Text = "📖  Lectores — Escritores",
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
            _btnAgregarLector = CrearBotonToolbar("+ Mesero", false);
            _btnAtaque = CrearBotonToolbar("🎣 Phishing", false, true);

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

            _lblVel = new Label
            {
                Text = "5x",
                Font = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.TarjetaLectores,
                BackColor = ColorConstants.FondoPanel,
                AutoSize = false,
                Size = new Size(44, 44),
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(4, 0, 0, 0)
            };
            _trackVel = new TrackBar
            {
                Minimum = 1,
                Maximum = 10,
                Value = 5,
                TickStyle = TickStyle.None,
                Size = new Size(120, 44),
                BackColor = ColorConstants.FondoPanel,
                Margin = new Padding(0)
            };

            flow.Controls.AddRange(new Control[] { _btnIniciar, _btnPausar, _btnDetener, sep1, _btnAgregarLector, sep2, lblVel, _lblVel, _trackVel, _btnAtaque });

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
                RowCount = 1,
                BackColor = ColorConstants.FondoPrincipal,
                Padding = new Padding(8)
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // ── Columna 1: Lectores ─────────────────────────────────────────
            _panelIzq = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorConstants.FondoLateral,
                Padding = new Padding(8)
            };
            var lblLectores = new Label
            {
                Text = "🧑‍🍽 Meseros (Lectores)",
                Font = AppTheme.FuenteLabelBold,
                ForeColor = ColorConstants.TarjetaLectores,
                Dock = DockStyle.Top,
                Height = 34,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0)
            };
            _flowLectores = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = ColorConstants.FondoPanel,
                Padding = new Padding(6)
            };

            // Gerente
            _panelGerente = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 110,
                BackColor = ColorConstants.FondoCard,
                Padding = new Padding(12)
            };
            _panelGerente.Paint += (_, e) =>
            {
                var color = _service.GerenteEscribiendo ? ColorConstants.AlertaAtaque : ColorConstants.Separador;
                using var pen = new Pen(color, 2);
                e.Graphics.DrawRectangle(pen, 1, 1, _panelGerente.Width - 3, _panelGerente.Height - 3);
            };
            var lblGerente = new Label
            {
                Text = "👔 Gerente (Escritor)",
                Font = AppTheme.FuenteLabelBold,
                ForeColor = ColorConstants.AcentoSecundario,
                BackColor = ColorConstants.FondoCard,
                Dock = DockStyle.Top,
                Height = 30,
                TextAlign = ContentAlignment.MiddleLeft
            };
            _lblGerenteEstado = new Label
            {
                Text = "⏳ En espera",
                Font = AppTheme.FuenteLabel,
                ForeColor = ColorConstants.TextoHint,
                BackColor = ColorConstants.FondoCard,
                Dock = DockStyle.Top,
                Height = 28,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 0)
            };
            _lblGerenteAccion = new Label
            {
                Text = "—",
                Font = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.TextoHint,
                BackColor = ColorConstants.FondoCard,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 0)
            };
            _panelGerente.Controls.Add(_lblGerenteAccion);
            _panelGerente.Controls.Add(_lblGerenteEstado);
            _panelGerente.Controls.Add(lblGerente);

            _vizRWLock = new SemaphoreVisualizer
            {
                Dock = DockStyle.Bottom,
                Height = 120,
                Margin = new Padding(0, 4, 0, 0)
            };
            _vizRWLock.ActualizarReaderWriterLock(0, 0, false, false);

            _panelIzq.Controls.Add(_flowLectores);
            _panelIzq.Controls.Add(_panelGerente);
            _panelIzq.Controls.Add(_vizRWLock);
            _panelIzq.Controls.Add(lblLectores);

            // ── Columna 2: Menú ─────────────────────────────────────────────
            _panelMenu = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorConstants.FondoPanel,
                Padding = new Padding(10)
            };
            var lblMenu = new Label
            {
                Text = "📋 Menú del Restaurante (Recurso Compartido)",
                Font = AppTheme.FuenteLabelBold,
                ForeColor = ColorConstants.TextoSecundario,
                Dock = DockStyle.Top,
                Height = 36,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0)
            };
            _lblMenuVersion = new Label
            {
                Text = "Versión: —",
                Font = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.TextoHint,
                Dock = DockStyle.Top,
                Height = 26,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0)
            };
            _gridMenu = new DataGridView { Dock = DockStyle.Fill };
            AppTheme.AplicarADataGrid(_gridMenu);
            _gridMenu.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "#", Width = 34, FillWeight = 5 });
            _gridMenu.Columns.Add(new DataGridViewTextBoxColumn { Name = "Nombre", HeaderText = "Plato", FillWeight = 35 });
            _gridMenu.Columns.Add(new DataGridViewTextBoxColumn { Name = "Precio", HeaderText = "Precio", Width = 80, FillWeight = 12 });
            _gridMenu.Columns.Add(new DataGridViewTextBoxColumn { Name = "Cat", HeaderText = "Categoría", FillWeight = 18 });
            _gridMenu.Columns.Add(new DataGridViewTextBoxColumn { Name = "Version", HeaderText = "Ver.", Width = 48, FillWeight = 6 });
            _gridMenu.Columns.Add(new DataGridViewTextBoxColumn { Name = "ModPor", HeaderText = "Modificado por", FillWeight = 24 });

            _panelMenu.Controls.Add(_gridMenu);
            _panelMenu.Controls.Add(_lblMenuVersion);
            _panelMenu.Controls.Add(lblMenu);

            // ── Columna 3: Estadísticas y Log ──────────────────────────────
            _panelDer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorConstants.FondoLateral,
                Padding = new Padding(8)
            };
            var lblStats = new Label
            {
                Text = "📊 Estadísticas",
                Font = AppTheme.FuenteLabelBold,
                ForeColor = ColorConstants.TextoSecundario,
                Dock = DockStyle.Top,
                Height = 34,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(8, 0, 0, 0)
            };
            var panelStats = new Panel
            {
                Dock = DockStyle.Top,
                Height = 130,
                BackColor = ColorConstants.FondoPanel,
                Padding = new Padding(8)
            };
            var flowStats = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                BackColor = ColorConstants.FondoPanel,
                Padding = new Padding(4)
            };
            _lblStatLecturas = CrearLabelStat("📖 Lecturas: 0");
            _lblStatEscrituras = CrearLabelStat("✏ Escrituras: 0");
            _lblStatComprometidos = CrearLabelStat("⚡ Comprometidos: 0");
            _lblStatTiempo = CrearLabelStat("⏱ Tiempo: 00:00");
            flowStats.Controls.AddRange(new Control[] { _lblStatLecturas, _lblStatEscrituras, _lblStatComprometidos, _lblStatTiempo });
            panelStats.Controls.Add(flowStats);

            _logViewer = new LogViewer { Dock = DockStyle.Fill };
            _logViewer.SetTitulo("📋 Log de ReaderWriterLockSlim");

            _panelDer.Controls.Add(_logViewer);
            _panelDer.Controls.Add(panelStats);
            _panelDer.Controls.Add(lblStats);

            tbl.Controls.Add(_panelIzq, 0, 0);
            tbl.Controls.Add(_panelMenu, 1, 0);
            tbl.Controls.Add(_panelDer, 2, 0);
            panelCuerpo.Controls.Add(tbl);
            Controls.Add(panelCuerpo);
        }

        private Label CrearLabelStat(string texto)
        {
            return new Label
            {
                Text = texto,
                Font = AppTheme.FuenteLabelBold,
                ForeColor = ColorConstants.TextoSecundario,
                BackColor = ColorConstants.FondoPanel,
                AutoSize = false,
                Height = 26,
                Width = 210,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(4)
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
                Text = "🎣 ATAQUE ACTIVO: Phishing — Menú Comprometido",
                Font = AppTheme.FuenteLabelBold,
                ForeColor = ColorConstants.AlertaAtaque,
                BackColor = ColorConstants.FondoAtaque,
                Dock = DockStyle.Top,
                Height = 28,
                TextAlign = ContentAlignment.MiddleLeft
            };
            var lblDesc = new Label
            {
                Text = "Credenciales del Gerente comprometidas. Menú alterado. Meseros leen información incorrecta.",
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
            _lblLockStatus = new ToolStripStatusLabel
            {
                Text = "Lock: Libre",
                ForeColor = ColorConstants.AcentoExito,
                Font = new Font("Consolas", 10f, FontStyle.Bold)
            };
            _status.Items.AddRange(new ToolStripItem[] { _lblStatus, _lblLockStatus });
            Controls.Add(_status);
        }

        // ─── EVENTOS ─────────────────────────────────────────────────────────

        private void ConfigurarEventos()
        {
            _btnIniciar.Click += (_, _) => Iniciar();
            _btnPausar.Click += (_, _) => PausarReanudar();
            _btnDetener.Click += (_, _) => Detener();
            _btnAgregarLector.Click += (_, _) => _service.AgregarLector();
            _btnAtaque.Click += (_, _) => ToggleAtaque();

            _trackVel.ValueChanged += (_, _) =>
            {
                _service.AjustarVelocidad(VelAMs(_trackVel.Value));
                _lblVel.Text = $"{_trackVel.Value}x";
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
            _flowLectores.Controls.Clear();
            _gridMenu.Rows.Clear();
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
                _btnAtaque.Text = "🎣 Phishing";
                _btnAtaque.BackColor = ColorConstants.AlertaAtaque;
            }
            else
            {
                using var dlg = new FrmIngenieriaSocial(
                    "📧 Correo — Sistema de Gestión Restaurante",
                    "De: sistema@gestionresto-soporte.com\nPara: gerente@restaurantedoncod.com\n" +
                    "Asunto: ⚠ URGENTE: Su acceso expirará en 24 horas\n\nEstimado Gerente:\n\n" +
                    "Su sesión en el Sistema de Gestión expirará mañana si no renueva sus credenciales.\n\n" +
                    "Haga clic abajo para verificar su identidad.",
                    "✉ Verificar mis credenciales",
                    "Ignorar correo");

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _attackService.ActivarAtaque(AttackType.PhishingMenuAlterado);
                    _btnAtaque.Text = "🛡 Desactivar Ataque";
                    _btnAtaque.BackColor = ColorConstants.AcentoExito;
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

        // ─── ACTUALIZACIÓN UI ────────────────────────────────────────────────

        private void TimerUI_Tick(object? sender, EventArgs e)
        {
            if (_service.Estado == SimulationStatus.Detenida) return;
            var stats = _service.ObtenerEstadisticas();
            ActualizarStats(stats);
            ActualizarGridMenu();
            ActualizarGerente();
            ActualizarLectores();
            ActualizarViz(stats);
        }

        private void ActualizarStats(SimulationStatistics stats)
        {
            _lblStatLecturas.Text = $"📖 Lecturas: {stats.TotalLecturas}";
            _lblStatEscrituras.Text = $"✏ Escrituras: {stats.TotalEscrituras}";
            _lblStatComprometidos.Text = $"⚡ Comprometidos: {stats.LectoresComprometidos}";
            _lblStatTiempo.Text = $"⏱ Tiempo: {stats.TiempoEjecucion:mm\\:ss}";
            _lblStatus.Text = $"{stats.EstadoActual}  |  Lectores: {stats.LectoresActivos}";

            _lblLockStatus.Text = stats.EscritorActivo ? "🔴 ESCRITURA EXCLUSIVA" :
                                  stats.LectoresActivos > 0 ? $"🟢 {stats.LectoresActivos} leyendo" : "⚪ Lock: Libre";
            _lblLockStatus.ForeColor = stats.EscritorActivo ? ColorConstants.AlertaAtaque :
                                       stats.LectoresActivos > 0 ? ColorConstants.AcentoExito : ColorConstants.TextoHint;
        }

        private void ActualizarGridMenu()
        {
            _gridMenu.SuspendLayout();
            _gridMenu.Rows.Clear();
            try
            {
                var menuRepo = Configuration.AppSettings.Resolver<IMenuRepository>();
                var items = menuRepo.ObtenerCompleto();
                int maxVer = items.Count > 0 ? items.Max(i => i.Version) : 0;
                _lblMenuVersion.Text = $"Versión: {maxVer}  {(_attackService.IsAttackActive ? "⚠ COMPROMETIDA" : "✅ Íntegra")}";

                foreach (var item in items)
                {
                    int idx = _gridMenu.Rows.Add(
                        item.Id,
                        item.Nombre.Length > 28 ? item.Nombre[..28] + "…" : item.Nombre,
                        $"${item.Precio:N2}",
                        item.Categoria,
                        $"v{item.Version}",
                        item.ModificadoPor);
                    if (item.FueAlterado)
                    {
                        _gridMenu.Rows[idx].DefaultCellStyle.BackColor = Color.FromArgb(50, 220, 50, 50);
                        _gridMenu.Rows[idx].DefaultCellStyle.ForeColor = ColorConstants.EstadoAlterado;
                    }
                }
            }
            catch { }
            _gridMenu.ResumeLayout();
        }

        private void ActualizarGerente()
        {
            bool escribiendo = _service.GerenteEscribiendo;
            bool esperando = _service.EscritorEsperando;

            _lblGerenteEstado.Text = escribiendo ? "✏ ESCRIBIENDO (WriteLock exclusivo)" :
                                     esperando ? "⏳ Esperando WriteLock..." : "💤 En espera";
            _lblGerenteEstado.ForeColor = escribiendo ? ColorConstants.AlertaAtaque :
                                          esperando ? ColorConstants.EstadoEsperando : ColorConstants.TextoHint;

            string ultima = _service.UltimaModificacion ?? "—";
            _lblGerenteAccion.Text = ultima.Length > 40 ? ultima[..40] + "…" : ultima;
            _panelGerente.Invalidate();
        }

        private void ActualizarLectores()
{
    var meseros = _service.Meseros;
    while (_flowLectores.Controls.Count < meseros.Count)
        _flowLectores.Controls.Add(new ThreadStatusIndicator
        {
            Width = _flowLectores.Width - 16,
            Anchor = AnchorStyles.Left | AnchorStyles.Right,
            Margin = new Padding(4)
        });

    for (int i = 0; i < meseros.Count; i++)
    {
        var m = meseros[i];
        var ind = (ThreadStatusIndicator)_flowLectores.Controls[i];

        // 👇 NUEVO: estado de bloqueo por escritor
        if (m.EstaBloqueadoPorEscritor)
        {
            ind.ActualizarEstado(
                m.Id,
                ThreadStatusIndicator.EstadoHilo.Esperando,  // amarillo
                "⛔ Bloqueado por escritor (WriteLock)"
            );
            continue;
        }

        // Resto de estados (prioridad: ataque > leyendo > esperando > detenido)
        if (m.LeyoMenuComprometido)
        {
            ind.ActualizarEstado(
                m.Id,
                ThreadStatusIndicator.EstadoHilo.BajoAtaque,
                $"⚠ Info comprometida v{m.VersionMenuLeida}"
            );
        }
        else if (m.EstaLeyendo)
        {
            ind.ActualizarEstado(
                m.Id,
                ThreadStatusIndicator.EstadoHilo.Activo,
                $"📖 {m.UltimoItemLeido ?? "—"}"
            );
        }
        else if (m.EstaEsperando)
        {
            ind.ActualizarEstado(
                m.Id,
                ThreadStatusIndicator.EstadoHilo.Esperando,
                "⏳ Esperando ReadLock..."
            );
        }
        else
        {
            ind.ActualizarEstado(
                m.Id,
                ThreadStatusIndicator.EstadoHilo.Detenido,
                $"✓ {m.LecturasCompletadas} lecturas"
            );
        }
    }
}

        private void ActualizarViz(SimulationStatistics stats)
        {
            _vizRWLock.ActualizarReaderWriterLock(
                stats.LectoresActivos,
                stats.LectoresEsperando,
                stats.EscritorActivo,
                stats.EscritorEsperando);
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
                SimulationStatus.BajoAtaque => ("🎣 Bajo Ataque", ColorConstants.AlertaAtaque),
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
            _btnAgregarLector.Enabled = corriendo || pausada;
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
}