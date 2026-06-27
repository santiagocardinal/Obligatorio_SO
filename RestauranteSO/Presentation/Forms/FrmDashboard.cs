using System.Drawing.Drawing2D;
using System.Drawing.Text;
using RestauranteSO.Configuration;
using RestauranteSO.Constants;
using RestauranteSO.Domain.Interfaces;
using RestauranteSO.Presentation.Components;
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

        private Panel           _panelHeader  = null!;
        private Panel           _panelCentral = null!;
        private Panel           _panelFooter  = null!;
        private TableLayoutPanel _tableCards  = null!;

        private SimulationCard _card1 = null!;
        private SimulationCard _card2 = null!;
        private SimulationCard _card3 = null!;
        private SimulationCard _card4 = null!;

        // Header labels
        private Label _lblLogo         = null!;
        private Label _lblNombre        = null!;
        private Label _lblSubtitulo     = null!;
        private Label _lblMateria       = null!;
        private Label _lblIntegrantes   = null!;
        private Label _lblFecha         = null!;
        private Label _lblHora          = null!;
        private Label _lblEstadoSistema = null!;

        // Footer
        private StatusStrip             _footer       = null!;
        private ToolStripStatusLabel    _footerEstado = null!;
        private ToolStripStatusLabel    _footerHilos  = null!;
        private ToolStripStatusLabel    _footerHora   = null!;
        private ToolStripStatusLabel    _footerVersion = null!;
        private ToolStripStatusLabel    _footerAutor  = null!;

        // Timers
        private System.Windows.Forms.Timer _timerReloj  = null!;
        private System.Windows.Forms.Timer _timerStats  = null!;

        // Servicios
        private readonly ISimulationLogger _logger;

        // Ventanas hijas
        private FrmProductorConsumidor?       _frmPC  = null;
        private FrmLectoresEscritores?         _frmLE  = null;
        private FrmAtaqueProductorConsumidor?  _frmAPC = null;
        private FrmAtaqueLectoresEscritores?   _frmALE = null;

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

            Text             = $"{AppConstants.NombreRestaurante} — Simulador SO";
            MinimumSize      = new Size(1400, 900);
            StartPosition    = FormStartPosition.CenterScreen;
            BackColor        = ColorConstants.FondoPrincipal;
            ForeColor        = ColorConstants.TextoPrincipal;
            Font             = AppTheme.FuenteLabel;
            WindowState      = FormWindowState.Maximized;

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
                Height    = 140,
                BackColor = ColorConstants.FondoSuperior,
                Padding   = new Padding(32, 0, 32, 0)
            };

            // Línea inferior de acento
            _panelHeader.Paint += (_, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                // Gradiente de línea inferior
                var lineRect = new Rectangle(
                    0, _panelHeader.Height - 3,
                    _panelHeader.Width, 3);
                using var lgb = new LinearGradientBrush(
                    lineRect,
                    ColorConstants.AcentoPrincipal,
                    ColorConstants.TarjetaLectores,
                    LinearGradientMode.Horizontal);
                e.Graphics.FillRectangle(lgb, lineRect);
            };

            // TableLayout para el header: izquierda (logo+nombre) | derecha (info)
            var tbl = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 1,
                BackColor   = Color.Transparent
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60f));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40f));

            // ── Lado izquierdo: Logo + Nombre ─────────────────────────────
            var panelIzq = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding   = new Padding(0, 20, 0, 12)
            };

            _lblLogo = new Label
            {
                Text      = "🍽",
                Font      = new Font("Segoe UI Emoji", 44f),
                ForeColor = ColorConstants.AcentoSecundario,
                AutoSize  = false,
                Size      = new Size(80, 80),
                Location  = new Point(0, 16),
                TextAlign = ContentAlignment.MiddleCenter
            };

            _lblNombre = new Label
            {
                Text      = AppConstants.NombreRestaurante,
                Font      = AppTheme.FuenteHero,
                ForeColor = ColorConstants.TextoPrincipal,
                AutoSize  = true,
                Location  = new Point(90, 18)
            };

            _lblSubtitulo = new Label
            {
                Text      = AppConstants.Subtitulo,
                Font      = AppTheme.FuenteSubtitulo,
                ForeColor = ColorConstants.AcentoPrincipal,
                AutoSize  = true,
                Location  = new Point(92, 78)
            };

            _lblMateria = new Label
            {
                Text      = "Sistemas Operativos — Universidad",
                Font      = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.TextoHint,
                AutoSize  = true,
                Location  = new Point(94, 108)
            };

            panelIzq.Controls.AddRange(new Control[]
            {
                _lblLogo, _lblNombre, _lblSubtitulo, _lblMateria
            });

            // ── Lado derecho: Info del sistema ────────────────────────────
            var panelDer = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding   = new Padding(16, 20, 0, 12)
            };

            _lblIntegrantes = new Label
            {
                Text      = "👥 Integrantes: Lucía Rodriguez y Santiago Cardinal",
                Font      = AppTheme.FuenteLabel,
                ForeColor = ColorConstants.TextoSecundario,
                AutoSize  = true,
                Location  = new Point(16, 20)
            };

            _lblFecha = new Label
            {
                Text      = DateTime.Now.ToString(
                    "dddd, dd 'de' MMMM 'de' yyyy",
                    new System.Globalization.CultureInfo("es-ES")),
                Font      = AppTheme.FuenteLabel,
                ForeColor = ColorConstants.TextoSecundario,
                AutoSize  = true,
                Location  = new Point(16, 48)
            };

            _lblHora = new Label
            {
                Text      = DateTime.Now.ToString("HH:mm:ss"),
                Font      = new Font("Consolas", 22f, FontStyle.Bold),
                ForeColor = ColorConstants.AcentoPrincipal,
                AutoSize  = true,
                Location  = new Point(16, 72)
            };

            _lblEstadoSistema = new Label
            {
                Text      = "✅ Sistema operativo — Listo",
                Font      = AppTheme.FuenteSmallBold,
                ForeColor = ColorConstants.AcentoExito,
                AutoSize  = true,
                Location  = new Point(16, 112)
            };

            panelDer.Controls.AddRange(new Control[]
            {
                _lblIntegrantes, _lblFecha,
                _lblHora, _lblEstadoSistema
            });

            tbl.Controls.Add(panelIzq, 0, 0);
            tbl.Controls.Add(panelDer, 1, 0);
            _panelHeader.Controls.Add(tbl);
            Controls.Add(_panelHeader);
        }

        // ─── PANEL CENTRAL ────────────────────────────────────────────────────

        private void ConstruirCentral()
        {
            _panelCentral = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = ColorConstants.FondoPrincipal,
                Padding   = new Padding(48, 32, 48, 24)
            };

            // Título de sección
            var lblSeccion = new Label
            {
                Text      = "Módulos de Simulación",
                Font      = AppTheme.FuenteTitulo,
                ForeColor = ColorConstants.TextoPrincipal,
                AutoSize  = true,
                Dock      = DockStyle.Top,
                Height    = 40,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblSeccionSub = new Label
            {
                Text      = "Seleccione un módulo para iniciar la simulación",
                Font      = AppTheme.FuenteSubtitulo,
                ForeColor = ColorConstants.TextoSecundario,
                AutoSize  = false,
                Dock      = DockStyle.Top,
                Height    = 36,
                TextAlign = ContentAlignment.MiddleLeft
            };

            // Separador
            var sep = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 2,
                BackColor = ColorConstants.Separador,
                Margin    = new Padding(0, 8, 0, 8)
            };

            // Espacio
            var espacio = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 16,
                BackColor = Color.Transparent
            };

            // TableLayoutPanel 2x2 para las cards
            _tableCards = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 2,
                BackColor   = Color.Transparent,
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };
            _tableCards.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 50f));
            _tableCards.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 50f));
            _tableCards.RowStyles.Add(
                new RowStyle(SizeType.Percent, 50f));
            _tableCards.RowStyles.Add(
                new RowStyle(SizeType.Percent, 50f));
            _tableCards.Padding = new Padding(8);

            // Crear cards
            _card1 = CrearCard(
                "01", "🍽",
                "Productor — Consumidor",
                "Clientes generan pedidos.\nCocineros los consumen.\nSemaphoreSlim + ConcurrentQueue.",
                "● Lista para iniciar",
                ColorConstants.TarjetaProductor);

            _card2 = CrearCard(
                "02", "📖",
                "Lectores — Escritores",
                "Meseros leen el menú.\nEl Gerente lo actualiza.\nReaderWriterLockSlim.",
                "● Lista para iniciar",
                ColorConstants.TarjetaLectores);

            _card3 = CrearCard(
                "03", "⚡",
                "Ataque: Inyección de Pedidos",
                "Ingeniería social.\nFalso técnico compromete\nla cola de pedidos.",
                "⚠ Simulación Educativa",
                ColorConstants.TarjetaAtaque1);

            _card4 = CrearCard(
                "04", "🎣",
                "Ataque: Phishing al Gerente",
                "Correo falso compromete\ncredenciales del Gerente.\nMenú alterado.",
                "⚠ Simulación Educativa",
                ColorConstants.TarjetaAtaque2);

            _card3.EstadoColor = ColorConstants.AcentoSecundario;
            _card4.EstadoColor = ColorConstants.AlertaAtaque;

            // Agregar cards al TableLayout con Dock.Fill + Margin
            foreach (var card in new[] { _card1, _card2, _card3, _card4 })
            {
                card.Dock   = DockStyle.Fill;
                card.Margin = new Padding(16);
            }

            _tableCards.Controls.Add(_card1, 0, 0);
            _tableCards.Controls.Add(_card2, 1, 0);
            _tableCards.Controls.Add(_card3, 0, 1);
            _tableCards.Controls.Add(_card4, 1, 1);

            // Eventos clic
            _card1.Click += (_, _) => AbrirSimulacionPC();
            _card2.Click += (_, _) => AbrirSimulacionLE();
            _card3.Click += (_, _) => AbrirAtaquePC();
            _card4.Click += (_, _) => AbrirAtaqueLE();

            // Tooltips
            ConfigurarTooltips();

            _panelCentral.Controls.Add(_tableCards);
            _panelCentral.Controls.Add(espacio);
            _panelCentral.Controls.Add(sep);
            _panelCentral.Controls.Add(lblSeccionSub);
            _panelCentral.Controls.Add(lblSeccion);
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
                Cursor      = Cursors.Hand
            };
        }

        private void ConfigurarTooltips()
        {
            var tt = new ToolTip
            {
                IsBalloon    = false,
                ShowAlways   = true,
                InitialDelay = 600,
                AutoPopDelay = 6000
            };

            tt.SetToolTip(_card1,
                "Simulación Productor-Consumidor\n" +
                "Hilos reales con SemaphoreSlim\n" +
                "Haga clic para abrir");
            tt.SetToolTip(_card2,
                "Simulación Lectores-Escritores\n" +
                "ReaderWriterLockSlim\n" +
                "Haga clic para abrir");
            tt.SetToolTip(_card3,
                "Ataque Educativo: Ingeniería Social\n" +
                "Falso técnico compromete la cola\n" +
                "⚠ Solo simulación educativa");
            tt.SetToolTip(_card4,
                "Ataque Educativo: Phishing\n" +
                "Credenciales robadas → menú alterado\n" +
                "⚠ Solo simulación educativa");
        }

        // ─── FOOTER ───────────────────────────────────────────────────────────

        private void ConstruirFooter()
        {
            _footer = new StatusStrip();
            AppTheme.AplicarAStatusStrip(_footer);
            _footer.Height = 32;

            _footerEstado = new ToolStripStatusLabel
            {
                Text      = "✅ Sistema listo",
                ForeColor = ColorConstants.AcentoExito,
                Font      = AppTheme.FuenteSmallBold
            };

            var sep1 = new ToolStripSeparator();

            _footerHilos = new ToolStripStatusLabel
            {
                Text      = "Hilos: 0",
                ForeColor = ColorConstants.TextoSecundario,
                Font      = AppTheme.FuenteStatus
            };

            var sep2 = new ToolStripSeparator();

            _footerVersion = new ToolStripStatusLabel
            {
                Text      = $"v{AppConstants.Version}  |  .NET {Environment.Version.ToString(2)}  |  WinForms",
                ForeColor = ColorConstants.TextoHint,
                Font      = AppTheme.FuenteSmall
            };

            var spacer = new ToolStripStatusLabel { Spring = true };

            _footerAutor = new ToolStripStatusLabel
            {
                Text      = $"© {AppConstants.Anio} {AppConstants.NombreApp} — Simulación Educativa SO",
                ForeColor = ColorConstants.TextoHint,
                Font      = AppTheme.FuenteSmall
            };

            var sep3 = new ToolStripSeparator();

            _footerHora = new ToolStripStatusLabel
            {
                Text      = DateTime.Now.ToString("HH:mm:ss"),
                ForeColor = ColorConstants.AcentoPrincipal,
                Font      = new Font("Consolas", 10f, FontStyle.Bold)
            };

            _footer.Items.AddRange(new ToolStripItem[]
            {
                _footerEstado, sep1, _footerHilos, sep2,
                _footerVersion, spacer, _footerAutor, sep3, _footerHora
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
                _lblHora.Text    = hora;
                _footerHora.Text = hora;
                _lblFecha.Text   = DateTime.Now.ToString(
                    "dddd, dd 'de' MMMM 'de' yyyy",
                    new System.Globalization.CultureInfo("es-ES"));
            };
            _timerReloj.Start();

            _timerStats = new System.Windows.Forms.Timer { Interval = 2000 };
            _timerStats.Tick += (_, _) =>
            {
                var proc = System.Diagnostics.Process.GetCurrentProcess();
                _footerHilos.Text =
                    $"Hilos: {proc.Threads.Count}  |  " +
                    $"RAM: {proc.WorkingSet64 / 1024 / 1024} MB";
            };
            _timerStats.Start();
        }

        // ─── NAVEGACIÓN ───────────────────────────────────────────────────────

        private void AbrirSimulacionPC()
        {
            if (_frmPC == null || _frmPC.IsDisposed)
            {
                var svc    = AppSettings.Resolver<ProductorConsumidorService>();
                var ataque = AppSettings.Resolver<AtaqueProductorConsumidorService>();
                _frmPC = new FrmProductorConsumidor(svc, ataque, _logger);
                _frmPC.FormClosed += (_, _) =>
                {
                    _frmPC = null;
                    ActualizarCardEstado(_card1,
                        "● Lista para iniciar", ColorConstants.AcentoExito);
                };
            }
            ActualizarCardEstado(_card1,
                "▶ Ventana abierta", ColorConstants.TarjetaProductor);
            AbrirVentanaHija(_frmPC);
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
                    ActualizarCardEstado(_card2,
                        "● Lista para iniciar", ColorConstants.AcentoExito);
                };
            }
            ActualizarCardEstado(_card2,
                "▶ Ventana abierta", ColorConstants.TarjetaLectores);
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
                    ActualizarCardEstado(_card3,
                        "⚠ Simulación Educativa", ColorConstants.AcentoSecundario);
                };
            }
            ActualizarCardEstado(_card3,
                "⚡ Ataque en curso", ColorConstants.AlertaAtaque);
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
                    ActualizarCardEstado(_card4,
                        "⚠ Simulación Educativa", ColorConstants.AlertaAtaque);
                };
            }
            ActualizarCardEstado(_card4,
                "🎣 Phishing activo", ColorConstants.AlertaAtaque);
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

        private void ActualizarCardEstado(
            SimulationCard card, string texto, Color color)
        {
            card.EstadoTexto = texto;
            card.EstadoColor = color;
        }
    }
}