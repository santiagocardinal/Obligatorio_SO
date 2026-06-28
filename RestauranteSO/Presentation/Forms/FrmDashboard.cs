using System.Drawing.Drawing2D;
using RestauranteSO.Configuration;
using RestauranteSO.Constants;
using RestauranteSO.Domain.Interfaces;
using RestauranteSO.Presentation.Controls;
using RestauranteSO.Presentation.Themes;
using RestauranteSO.Services.Ataques;
using RestauranteSO.Services.LectoresEscritores;
using RestauranteSO.Services.ProductorConsumidor;

namespace RestauranteSO.Presentation.Forms
{
    public sealed class FrmDashboard : Form
    {
        // ─── CONTROLES ────────────────────────────────────────────────────────

        private Panel            _panelHeader  = null!;
        private Panel            _panelCentral = null!;
        private TableLayoutPanel _tableCards   = null!;

        private SimulationCard _card1 = null!;
        private SimulationCard _card2 = null!;
        private SimulationCard _card3 = null!;
        private SimulationCard _card4 = null!;

        private Label _lblHora          = null!;
        private Label _lblFecha         = null!;

        private StatusStrip          _footer        = null!;
        private ToolStripStatusLabel _footerHilos   = null!;
        private ToolStripStatusLabel _footerVersion = null!;
        private ToolStripStatusLabel _footerAutor   = null!;
        private ToolStripStatusLabel _footerHora    = null!;

        private System.Windows.Forms.Timer _timerReloj = null!;
        private System.Windows.Forms.Timer _timerStats = null!;

        private readonly ISimulationLogger _logger;

        private FrmProductorConsumidor?      _frmPC  = null;
        private FrmLectoresEscritores?        _frmLE  = null;
        private FrmAtaqueProductorConsumidor? _frmAPC = null;
        private FrmAtaqueLectoresEscritores?  _frmALE = null;

        // ─── CONSTRUCTOR ─────────────────────────────────────────────────────

        public FrmDashboard()
        {
            _logger = AppSettings.Resolver<ISimulationLogger>();
            InitializeComponent();
            IniciarTimers();
        }

        // ─── INIT ─────────────────────────────────────────────────────────────

        private void InitializeComponent()
        {
            SuspendLayout();

            Text          = $"{AppConstants.NombreRestaurante} — Simulador SO";
            MinimumSize   = new Size(1400, 860);
            WindowState   = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor     = ColorConstants.FondoPrincipal;
            ForeColor     = ColorConstants.TextoPrincipal;
            Font          = AppTheme.FuenteLabel;

            ConstruirHeader();
            ConstruirCentral();
            ConstruirFooter();

            FormClosing += (_, _) =>
            {
                _timerReloj?.Stop();
                _timerStats?.Stop();
                _frmPC?.Close();
                _frmLE?.Close();
                _frmAPC?.Close();
                _frmALE?.Close();
            };

            ResumeLayout(true);
        }

        // ─── HEADER ───────────────────────────────────────────────────────────

        private void ConstruirHeader()
        {
            _panelHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 130,
                BackColor = ColorConstants.FondoSuperior
            };

            _panelHeader.Paint += (_, e) =>
            {
                int y = _panelHeader.Height - 3;
                if (_panelHeader.Width <= 0) return;
                using var lgb = new LinearGradientBrush(
                    new Rectangle(0, y, _panelHeader.Width, 3),
                    ColorConstants.AcentoPrincipal,
                    ColorConstants.TarjetaLectores,
                    LinearGradientMode.Horizontal);
                e.Graphics.FillRectangle(lgb, 0, y, _panelHeader.Width, 3);
            };

            // TableLayout principal del header: 2 columnas
            var headerLayout = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 1,
                BackColor   = ColorConstants.FondoSuperior,
                Padding     = new Padding(24, 0, 24, 0)
            };
            headerLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 58f));
            headerLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 42f));
            headerLayout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100f));

            // ── IZQUIERDA ─────────────────────────────────────────────────
            var izqLayout = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 1,
                BackColor   = ColorConstants.FondoSuperior,
                Padding     = new Padding(0, 16, 0, 12)
            };
            izqLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Absolute, 88f));
            izqLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100f));
            izqLayout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100f));

            var lblLogo = new Label
            {
                Text      = "🍽",
                Font      = new Font("Segoe UI Emoji", 48f),
                ForeColor = ColorConstants.AcentoSecundario,
                BackColor = ColorConstants.FondoSuperior,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize  = false
            };

            var textosLayout = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 1,
                RowCount    = 3,
                BackColor   = ColorConstants.FondoSuperior,
                Padding     = new Padding(8, 0, 0, 0)
            };
            textosLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100f));
            textosLayout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 50f));
            textosLayout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 30f));
            textosLayout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 20f));

            var lblNombre = new Label
            {
                Text      = AppConstants.NombreRestaurante,
                Font      = new Font("Segoe UI", 30f, FontStyle.Bold),
                ForeColor = ColorConstants.TextoPrincipal,
                BackColor = ColorConstants.FondoSuperior,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                AutoSize  = false
            };

            var lblSubtitulo = new Label
            {
                Text      = AppConstants.Subtitulo,
                Font      = new Font("Segoe UI", 14f),
                ForeColor = ColorConstants.AcentoPrincipal,
                BackColor = ColorConstants.FondoSuperior,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize  = false
            };

            var lblMateria = new Label
            {
                Text      = "Sistemas Operativos — Universidad",
                Font      = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.TextoHint,
                BackColor = ColorConstants.FondoSuperior,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft,
                AutoSize  = false
            };

            textosLayout.Controls.Add(lblNombre,    0, 0);
            textosLayout.Controls.Add(lblSubtitulo, 0, 1);
            textosLayout.Controls.Add(lblMateria,   0, 2);

            izqLayout.Controls.Add(lblLogo,      0, 0);
            izqLayout.Controls.Add(textosLayout, 1, 0);

            // ── DERECHA ───────────────────────────────────────────────────
            var derLayout = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 1,
                RowCount    = 4,
                BackColor   = ColorConstants.FondoSuperior,
                Padding     = new Padding(24, 16, 0, 12)
            };
            derLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100f));
            derLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
            derLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 25f));
            derLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 30f));
            derLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));

            var lblIntegrantes = new Label
            {
                Text      = "👥 Integrantes: Equipo RestauranteSO",
                Font      = AppTheme.FuenteLabel,
                ForeColor = ColorConstants.TextoSecundario,
                BackColor = ColorConstants.FondoSuperior,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                AutoSize  = false
            };

            _lblFecha = new Label
            {
                Text      = DateTime.Now.ToString(
                    "dddd, dd 'de' MMMM 'de' yyyy",
                    new System.Globalization.CultureInfo("es-ES")),
                Font      = AppTheme.FuenteLabel,
                ForeColor = ColorConstants.TextoSecundario,
                BackColor = ColorConstants.FondoSuperior,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize  = false
            };

            _lblHora = new Label
            {
                Text      = DateTime.Now.ToString("HH:mm:ss"),
                Font      = new Font("Consolas", 24f, FontStyle.Bold),
                ForeColor = ColorConstants.AcentoPrincipal,
                BackColor = ColorConstants.FondoSuperior,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize  = false
            };

            var lblEstadoSistema = new Label
            {
                Text      = "✅ Sistema operativo — Listo",
                Font      = AppTheme.FuenteSmallBold,
                ForeColor = ColorConstants.AcentoExito,
                BackColor = ColorConstants.FondoSuperior,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft,
                AutoSize  = false
            };

            derLayout.Controls.Add(lblIntegrantes,  0, 0);
            derLayout.Controls.Add(_lblFecha,       0, 1);
            derLayout.Controls.Add(_lblHora,        0, 2);
            derLayout.Controls.Add(lblEstadoSistema,0, 3);

            headerLayout.Controls.Add(izqLayout,  0, 0);
            headerLayout.Controls.Add(derLayout,  1, 0);
            _panelHeader.Controls.Add(headerLayout);
            Controls.Add(_panelHeader);
        }

        // ─── PANEL CENTRAL ────────────────────────────────────────────────────

        private void ConstruirCentral()
        {
            _panelCentral = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = ColorConstants.FondoPrincipal,
                Padding   = new Padding(40, 24, 40, 16)
            };

            // Encabezado de sección
            var seccionLayout = new TableLayoutPanel
            {
                Dock        = DockStyle.Top,
                ColumnCount = 1,
                RowCount    = 2,
                BackColor   = ColorConstants.FondoPrincipal,
                Height      = 72
            };
            seccionLayout.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 100f));
            seccionLayout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 55f));
            seccionLayout.RowStyles.Add(
                new RowStyle(SizeType.Percent, 45f));

            var lblSeccion = new Label
            {
                Text      = "Módulos de Simulación",
                Font      = new Font("Segoe UI", 20f, FontStyle.Bold),
                ForeColor = ColorConstants.TextoPrincipal,
                BackColor = ColorConstants.FondoPrincipal,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.BottomLeft,
                AutoSize  = false
            };

            var lblSeccionSub = new Label
            {
                Text      = "Seleccione un módulo para iniciar la simulación correspondiente",
                Font      = new Font("Segoe UI", 12f),
                ForeColor = ColorConstants.TextoSecundario,
                BackColor = ColorConstants.FondoPrincipal,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft,
                AutoSize  = false
            };

            seccionLayout.Controls.Add(lblSeccion,    0, 0);
            seccionLayout.Controls.Add(lblSeccionSub, 0, 1);

            var sep = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 2,
                BackColor = ColorConstants.Separador
            };

            var espacio = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 14,
                BackColor = ColorConstants.FondoPrincipal
            };

            // TableLayoutPanel 2x2 para las cards
            _tableCards = new TableLayoutPanel
            {
                Dock            = DockStyle.Fill,
                ColumnCount     = 2,
                RowCount        = 2,
                BackColor       = ColorConstants.FondoPrincipal,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                Padding         = new Padding(0)
            };
            _tableCards.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 50f));
            _tableCards.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 50f));
            _tableCards.RowStyles.Add(
                new RowStyle(SizeType.Percent, 50f));
            _tableCards.RowStyles.Add(
                new RowStyle(SizeType.Percent, 50f));

            _card1 = CrearCard("01", "🍽",
                "Productor — Consumidor",
                "Clientes generan pedidos.\nCocineros los consumen.\nSemaphoreSlim + ConcurrentQueue.",
                "● Lista para iniciar",
                ColorConstants.TarjetaProductor);

            _card2 = CrearCard("02", "📖",
                "Lectores — Escritores",
                "Meseros leen el menú.\nEl Gerente lo actualiza.\nReaderWriterLockSlim.",
                "● Lista para iniciar",
                ColorConstants.TarjetaLectores);

            _card3 = CrearCard("03", "⚡",
                "Ataque: Inyección de Pedidos",
                "Ingeniería social.\nFalso técnico compromete\nla cola de pedidos.",
                "⚠ Simulación Educativa",
                ColorConstants.TarjetaAtaque1);

            _card4 = CrearCard("04", "🎣",
                "Ataque: Phishing al Gerente",
                "Correo falso compromete\ncredenciales del Gerente.\nMenú comprometido.",
                "⚠ Simulación Educativa",
                ColorConstants.TarjetaAtaque2);

            _card3.EstadoColor = ColorConstants.AcentoSecundario;
            _card4.EstadoColor = ColorConstants.AlertaAtaque;

            foreach (var c in new[] { _card1, _card2, _card3, _card4 })
            {
                c.Dock      = DockStyle.Fill;
                c.Margin    = new Padding(14);
                c.BackColor = ColorConstants.FondoCard;
            }

            _tableCards.Controls.Add(_card1, 0, 0);
            _tableCards.Controls.Add(_card2, 1, 0);
            _tableCards.Controls.Add(_card3, 0, 1);
            _tableCards.Controls.Add(_card4, 1, 1);

            _card1.CardClicked += (_, _) => AbrirSimulacionPC();
            _card2.CardClicked += (_, _) => AbrirSimulacionLE();
            _card3.CardClicked += (_, _) => AbrirAtaquePC();
            _card4.CardClicked += (_, _) => AbrirAtaqueLE();

            var tt = new ToolTip
            {
                ShowAlways = true, InitialDelay = 500, AutoPopDelay = 5000
            };
            tt.SetToolTip(_card1,
                "Simulación Productor-Consumidor\nClic para abrir");
            tt.SetToolTip(_card2,
                "Simulación Lectores-Escritores\nClic para abrir");
            tt.SetToolTip(_card3,
                "Ataque Educativo: Ingeniería Social\n⚠ Solo simulación");
            tt.SetToolTip(_card4,
                "Ataque Educativo: Phishing\n⚠ Solo simulación");

            _panelCentral.Controls.Add(_tableCards);
            _panelCentral.Controls.Add(espacio);
            _panelCentral.Controls.Add(sep);
            _panelCentral.Controls.Add(seccionLayout);
            Controls.Add(_panelCentral);
        }

        private SimulationCard CrearCard(
            string num, string icono, string titulo,
            string desc, string estado, Color acento)
        {
            return new SimulationCard
            {
                Numeracion  = num,
                Icono       = icono,
                Titulo      = titulo,
                Descripcion = desc,
                EstadoTexto = estado,
                ColorAcento = acento,
                EstadoColor = ColorConstants.AcentoExito,
                BackColor   = ColorConstants.FondoCard,
                Cursor      = Cursors.Hand
            };
        }

        // ─── FOOTER ───────────────────────────────────────────────────────────

        private void ConstruirFooter()
        {
            _footer = new StatusStrip();
            AppTheme.AplicarAStatusStrip(_footer);
            _footer.Height = 30;

            var footerEstado = new ToolStripStatusLabel
            {
                Text      = "✅ Sistema listo",
                ForeColor = ColorConstants.AcentoExito,
                Font      = AppTheme.FuenteSmallBold
            };

            _footerHilos = new ToolStripStatusLabel
            {
                Text      = "Hilos: —  |  RAM: — MB",
                ForeColor = ColorConstants.TextoSecundario,
                Font      = AppTheme.FuenteStatus
            };

            _footerVersion = new ToolStripStatusLabel
            {
                Text      =
                    $"v{AppConstants.Version}  |  " +
                    $".NET {Environment.Version.ToString(2)}  |  WinForms",
                ForeColor = ColorConstants.TextoHint,
                Font      = AppTheme.FuenteSmall
            };

            var spacer = new ToolStripStatusLabel { Spring = true };

            _footerAutor = new ToolStripStatusLabel
            {
                Text      =
                    $"© {AppConstants.Anio} {AppConstants.NombreApp}" +
                    " — Simulación Educativa SO",
                ForeColor = ColorConstants.TextoHint,
                Font      = AppTheme.FuenteSmall
            };

            _footerHora = new ToolStripStatusLabel
            {
                Text      = DateTime.Now.ToString("HH:mm:ss"),
                ForeColor = ColorConstants.AcentoPrincipal,
                Font      = new Font("Consolas", 10f, FontStyle.Bold)
            };

            _footer.Items.AddRange(new ToolStripItem[]
            {
                footerEstado,
                new ToolStripSeparator(),
                _footerHilos,
                new ToolStripSeparator(),
                _footerVersion,
                spacer,
                _footerAutor,
                new ToolStripSeparator(),
                _footerHora
            });

            Controls.Add(_footer);
        }

        // ─── TIMERS ───────────────────────────────────────────────────────────

        private void IniciarTimers()
        {
            _timerReloj = new System.Windows.Forms.Timer { Interval = 1000 };
            _timerReloj.Tick += (_, _) =>
            {
                string hora = DateTime.Now.ToString("HH:mm:ss");
                if (_lblHora    != null) _lblHora.Text    = hora;
                if (_footerHora != null) _footerHora.Text = hora;
                if (_lblFecha   != null)
                    _lblFecha.Text = DateTime.Now.ToString(
                        "dddd, dd 'de' MMMM 'de' yyyy",
                        new System.Globalization.CultureInfo("es-ES"));
            };
            _timerReloj.Start();

            _timerStats = new System.Windows.Forms.Timer { Interval = 2500 };
            _timerStats.Tick += (_, _) =>
            {
                try
                {
                    var proc = System.Diagnostics.Process.GetCurrentProcess();
                    if (_footerHilos != null)
                        _footerHilos.Text =
                            $"Hilos: {proc.Threads.Count}  |  " +
                            $"RAM: {proc.WorkingSet64 / 1024 / 1024} MB";
                }
                catch { }
            };
            _timerStats.Start();
        }

        // ─── NAVEGACIÓN ───────────────────────────────────────────────────────

        private void AbrirSimulacionPC()
        {
            try
            {
                if (_frmPC == null || _frmPC.IsDisposed)
                {
                    var svc    = AppSettings.Resolver<ProductorConsumidorService>();
                    var ataque = AppSettings.Resolver<AtaqueProductorConsumidorService>();
                    _frmPC = new FrmProductorConsumidor(svc, ataque, _logger);
                    _frmPC.FormClosed += (_, _) =>
                    {
                        _frmPC = null;
                        ActualizarCard(_card1,
                            "● Lista para iniciar", ColorConstants.AcentoExito);
                    };
                }
                ActualizarCard(_card1, "▶ Ventana abierta",
                    ColorConstants.TarjetaProductor);
                AbrirVentanaHija(_frmPC);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al abrir Productor-Consumidor:\n\n{ex.Message}\n\n{ex.StackTrace}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AbrirSimulacionLE()
        {
            if (_frmLE == null || _frmLE.IsDisposed)
            {
                var svc    = AppSettings.Resolver<LectoresEscritoresService>();
                var ataque = AppSettings.Resolver<AtaqueLectoresEscritoresService>();
                _frmLE = new FrmLectoresEscritores(svc, ataque, _logger);
                _frmLE.FormClosed += (_, _) =>
                {
                    _frmLE = null;
                    ActualizarCard(_card2,
                        "● Lista para iniciar", ColorConstants.AcentoExito);
                };
            }
            ActualizarCard(_card2, "▶ Ventana abierta",
                ColorConstants.TarjetaLectores);
            AbrirVentanaHija(_frmLE);
        }

        private void AbrirAtaquePC()
        {
            if (_frmAPC == null || _frmAPC.IsDisposed)
            {
                var svc    = AppSettings.Resolver<ProductorConsumidorService>();
                var ataque = AppSettings.Resolver<AtaqueProductorConsumidorService>();
                _frmAPC = new FrmAtaqueProductorConsumidor(svc, ataque, _logger);
                _frmAPC.FormClosed += (_, _) =>
                {
                    _frmAPC = null;
                    ActualizarCard(_card3,
                        "⚠ Simulación Educativa",
                        ColorConstants.AcentoSecundario);
                };
            }
            ActualizarCard(_card3, "⚡ Ataque en curso",
                ColorConstants.AlertaAtaque);
            AbrirVentanaHija(_frmAPC);
        }

        private void AbrirAtaqueLE()
        {
            if (_frmALE == null || _frmALE.IsDisposed)
            {
                var svc    = AppSettings.Resolver<LectoresEscritoresService>();
                var ataque = AppSettings.Resolver<AtaqueLectoresEscritoresService>();
                _frmALE = new FrmAtaqueLectoresEscritores(svc, ataque, _logger);
                _frmALE.FormClosed += (_, _) =>
                {
                    _frmALE = null;
                    ActualizarCard(_card4,
                        "⚠ Simulación Educativa",
                        ColorConstants.AlertaAtaque);
                };
            }
            ActualizarCard(_card4, "🎣 Phishing activo",
                ColorConstants.AlertaAtaque);
            AbrirVentanaHija(_frmALE);
        }

        private void AbrirVentanaHija(Form ventana)
        {
            Hide();
            ventana.FormClosed += (_, _) =>
            {
                if (!IsDisposed) Show();
            };
            ventana.Show();
        }

        private void ActualizarCard(
            SimulationCard card, string texto, Color color)
        {
            card.EstadoTexto = texto;
            card.EstadoColor = color;
        }
    }
}