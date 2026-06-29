// =============================================================================
// RestaurantOS — Restaurant Operating Environment
// Archivo  : Presentation/Forms/FrmSplash.cs
// Propósito: Splash Screen de inicio del sistema.
//            Muestra logo, nombre, versión y barra de carga animada.
//            Dura aproximadamente 3 segundos y luego cierra automáticamente.
// SOLID    : SRP — solo gestiona la experiencia de inicio.
// =============================================================================

using System.Drawing.Drawing2D;
using System.Drawing.Text;
using RestauranteSO.Constants;
using RestauranteSO.Presentation.Themes;

namespace RestauranteSO.Presentation.Forms
{
    public sealed class FrmSplash : Form
    {
        // ─── CONTROLES ────────────────────────────────────────────────────────

        private Panel  _panelContenido = null!;
        private Label  _lblLogo        = null!;
        private Label  _lblNombre      = null!;
        private Label  _lblTagline     = null!;
        private Label  _lblVersion     = null!;
        private Panel  _panelBarra     = null!;
        private Panel  _barraRelleno   = null!;
        private Label  _lblMensaje     = null!;
        private Label  _lblCopyright   = null!;

        // ─── TIMERS ───────────────────────────────────────────────────────────

        private System.Windows.Forms.Timer _timerCarga   = null!;
        private System.Windows.Forms.Timer _timerFade    = null!;

        // ─── ESTADO ───────────────────────────────────────────────────────────

        private int   _progreso      = 0;
        private int   _pasoMensaje   = 0;
        private float _opacidad      = 0f;
        private bool  _cerrando      = false;

        private static readonly string[] _mensajes =
        {
            "Inicializando entorno de simulación...",
            "Cargando módulo Productor-Consumidor...",
            "Cargando módulo Lectores-Escritores...",
            "Inicializando primitivas de concurrencia...",
            "Configurando SemaphoreSlim y ReaderWriterLockSlim...",
            "Registrando servicios de ataque...",
            "Verificando integridad de recursos compartidos...",
            "Preparando simulaciones educativas...",
            "Sincronizando hilos del sistema...",
            "RestaurantOS listo."
        };

        // ─── CONSTRUCTOR ─────────────────────────────────────────────────────

        public FrmSplash()
        {
            InitializeComponent();
        }

        // ─── INIT ─────────────────────────────────────────────────────────────

        private void InitializeComponent()
        {
            SuspendLayout();

            // Form sin bordes, centrado, tamaño fijo
            FormBorderStyle = FormBorderStyle.None;
            StartPosition   = FormStartPosition.CenterScreen;
            Size            = new Size(640, 400);
            BackColor       = ColorConstants.FondoSplash;
            Opacity         = 0;
            ShowInTaskbar   = false;

            ConstruirContenido();
            ConstruirTimers();

            ResumeLayout(true);
        }

        // ─── CONTENIDO ────────────────────────────────────────────────────────

        private void ConstruirContenido()
        {
            // Panel principal con padding
            _panelContenido = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = ColorConstants.FondoSplash,
                Padding   = new Padding(0)
            };
            _panelContenido.Paint += PanelContenido_Paint;

            // TableLayout principal: 7 filas
            var tbl = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 1,
                RowCount    = 7,
                BackColor   = ColorConstants.FondoSplash,
                Padding     = new Padding(60, 40, 60, 32)
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute,  80f)); // logo
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute,  56f)); // nombre
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute,  32f)); // tagline
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute,  20f)); // version
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent,  100f)); // espacio
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute,  32f)); // barra + msg
            tbl.RowStyles.Add(new RowStyle(SizeType.Absolute,  24f)); // copyright

            // ── Logo ──────────────────────────────────────────────────────────
            _lblLogo = new Label
            {
                Text      = "🍽",
                Font      = new Font("Segoe UI Emoji", 44f),
                ForeColor = ColorConstants.AcentoPrincipal,
                BackColor = ColorConstants.FondoSplash,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize  = false
            };

            // ── Nombre ────────────────────────────────────────────────────────
            _lblNombre = new Label
            {
                Text      = "RestaurantOS",
                Font      = AppTheme.FuenteSplashNombre,
                ForeColor = ColorConstants.TextoPrincipal,
                BackColor = ColorConstants.FondoSplash,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize  = false
            };

            // ── Tagline ───────────────────────────────────────────────────────
            _lblTagline = new Label
            {
                Text      = "Restaurant Operating Environment",
                Font      = AppTheme.FuenteSplashTag,
                ForeColor = ColorConstants.AcentoPrincipal,
                BackColor = ColorConstants.FondoSplash,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter,
                AutoSize  = false
            };

            // ── Versión ───────────────────────────────────────────────────────
            _lblVersion = new Label
            {
                Text      = $"v{AppConstants.Version}  •  Sistemas Operativos",
                Font      = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.SplashTextoVersion,
                BackColor = ColorConstants.FondoSplash,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.TopCenter,
                AutoSize  = false
            };

            // ── Panel de barra + mensaje ──────────────────────────────────────
            var panelBarraContenedor = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = ColorConstants.FondoSplash,
                Padding   = new Padding(0)
            };

            _lblMensaje = new Label
            {
                Text      = _mensajes[0],
                Font      = AppTheme.FuenteSplashSub,
                ForeColor = ColorConstants.TextoHint,
                BackColor = ColorConstants.FondoSplash,
                Dock      = DockStyle.Top,
                Height    = 16,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize  = false
            };

            _panelBarra = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 6,
                BackColor = ColorConstants.SplashBarraFondo
            };
            _panelBarra.Paint += PanelBarra_Paint;

            _barraRelleno = new Panel
            {
                Dock      = DockStyle.Left,
                Width     = 0,
                BackColor = ColorConstants.SplashBarraRelleno
            };
            _panelBarra.Controls.Add(_barraRelleno);

            panelBarraContenedor.Controls.Add(_panelBarra);
            panelBarraContenedor.Controls.Add(_lblMensaje);

            // ── Copyright ─────────────────────────────────────────────────────
            _lblCopyright = new Label
            {
                Text      =
                    $"© {AppConstants.Anio} RestaurantOS  •  " +
                    "Simulación Educativa  •  No ejecuta ataques reales",
                Font      = AppTheme.FuenteSplashSub,
                ForeColor = ColorConstants.TextoHint,
                BackColor = ColorConstants.FondoSplash,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize  = false
            };

            // Ensamblar
            tbl.Controls.Add(_lblLogo,              0, 0);
            tbl.Controls.Add(_lblNombre,            0, 1);
            tbl.Controls.Add(_lblTagline,           0, 2);
            tbl.Controls.Add(_lblVersion,           0, 3);
            tbl.Controls.Add(new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = ColorConstants.FondoSplash
            },                                      0, 4);
            tbl.Controls.Add(panelBarraContenedor,  0, 5);
            tbl.Controls.Add(_lblCopyright,         0, 6);

            _panelContenido.Controls.Add(tbl);
            Controls.Add(_panelContenido);
        }

        // ─── TIMERS ───────────────────────────────────────────────────────────

        private void ConstruirTimers()
        {
            // Timer de fade in
            _timerFade = new System.Windows.Forms.Timer { Interval = 16 };
            _timerFade.Tick += TimerFade_Tick;

            // Timer de carga (cada 250ms avanza la barra)
            _timerCarga = new System.Windows.Forms.Timer { Interval = 250 };
            _timerCarga.Tick += TimerCarga_Tick;
        }

        // ─── EVENTOS ─────────────────────────────────────────────────────────

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            _timerFade.Start();
        }

        private void TimerFade_Tick(object? sender, EventArgs e)
        {
            if (_cerrando)
            {
                // Fade out
                _opacidad -= 0.06f;
                if (_opacidad <= 0f)
                {
                    _opacidad = 0f;
                    Opacity   = 0;
                    _timerFade.Stop();
                    DialogResult = DialogResult.OK;
                    Close();
                    return;
                }
            }
            else
            {
                // Fade in
                _opacidad += 0.06f;
                if (_opacidad >= 1f)
                {
                    _opacidad = 1f;
                    _timerFade.Stop();
                    _timerCarga.Start();
                }
            }
            Opacity = _opacidad;
        }

        private void TimerCarga_Tick(object? sender, EventArgs e)
        {
            // Avanzar progreso
            int paso = 100 / _mensajes.Length;
            _progreso = Math.Min(100, _progreso + paso + Random.Shared.Next(0, 4));

            // Actualizar barra
            int anchoRelleno = (int)(_panelBarra.Width * (_progreso / 100.0));
            _barraRelleno.Width = anchoRelleno;
            _panelBarra.Invalidate();

            // Actualizar mensaje
            if (_pasoMensaje < _mensajes.Length - 1)
            {
                _pasoMensaje++;
                _lblMensaje.Text = _mensajes[_pasoMensaje];
            }

            // Al llegar al 100%, iniciar fade out
            if (_progreso >= 100)
            {
                _timerCarga.Stop();
                _barraRelleno.Width = _panelBarra.Width;
                _lblMensaje.Text    = _mensajes[^1];

                // Esperar 400ms antes de fade out
                var timerFin = new System.Windows.Forms.Timer { Interval = 400 };
                timerFin.Tick += (_, _) =>
                {
                    timerFin.Stop();
                    timerFin.Dispose();
                    _cerrando = true;
                    _timerFade.Start();
                };
                timerFin.Start();
            }
        }

        // ─── PINTADO ─────────────────────────────────────────────────────────

        private void PanelContenido_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Borde exterior redondeado con glow sutil
            var rect = new Rectangle(1, 1, Width - 3, Height - 3);
            using var borderPen = new Pen(
                Color.FromArgb(60, ColorConstants.AcentoPrincipal), 1.5f);
            using var path = AppTheme.CrearPathRedondeado(rect, 12);
            g.DrawPath(borderPen, path);

            // Línea de acento en la parte superior
            using var accentBrush = new System.Drawing.Drawing2D.LinearGradientBrush(
                new Rectangle(0, 0, Width, 3),
                ColorConstants.AcentoPrincipal,
                ColorConstants.TarjetaLectores,
                System.Drawing.Drawing2D.LinearGradientMode.Horizontal);
            g.FillRectangle(accentBrush, 0, 0, Width, 3);
        }

        private void PanelBarra_Paint(object? sender, PaintEventArgs e)
        {
            var g    = e.Graphics;
            var rect = new Rectangle(0, 0, _panelBarra.Width, _panelBarra.Height);

            // Fondo de la barra
            using var fondoBrush = new SolidBrush(ColorConstants.SplashBarraFondo);
            g.FillRectangle(fondoBrush, rect);

            // Relleno con gradiente
            if (_barraRelleno.Width > 0)
            {
                var rellenoRect = new Rectangle(
                    0, 0, _barraRelleno.Width, _panelBarra.Height);
                using var rellenoBrush =
                    new System.Drawing.Drawing2D.LinearGradientBrush(
                        rellenoRect,
                        ColorConstants.AcentoPrincipal,
                        ColorConstants.TarjetaLectores,
                        System.Drawing.Drawing2D.LinearGradientMode.Horizontal);
                g.FillRectangle(rellenoBrush, rellenoRect);
            }
        }

        // ─── DISPOSE ─────────────────────────────────────────────────────────

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timerCarga?.Stop();
                _timerCarga?.Dispose();
                _timerFade?.Stop();
                _timerFade?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}