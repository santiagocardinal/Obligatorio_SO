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
    public sealed class FrmDesktop : Form
    {
        private readonly ISimulationLogger _logger;

        // ─── CONTROLES ──────────────────────────────────────────────────────
        private Panel _panelTopBar = null!;
        private Panel _panelCentral = null!;
        private Panel _panelDock = null!;

        private DockItem _dockPC = null!;
        private DockItem _dockLE = null!;
        private DockItem _dockAPC = null!;
        private DockItem _dockALE = null!;
        private DockItem _dockGallery = null!;

        private ClockControl _clockControl = null!;

        private Label _lblTopEstado = null!;
        private Label _lblTopHilos = null!;

        private System.Windows.Forms.Timer _timerStats = null!;

        private FrmProductorConsumidor? _frmPC = null;
        private FrmLectoresEscritores? _frmLE = null;
        private FrmAtaqueProductorConsumidor? _frmAPC = null;
        private FrmAtaqueLectoresEscritores? _frmALE = null;
        private FrmGallery? _frmGallery = null;

        public FrmDesktop()
        {
            _logger = AppSettings.Resolver<ISimulationLogger>();
            InitializeComponent();
            IniciarTimers();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            Text = "RestaurantOS";
            MinimumSize = new Size(1200, 800);
            WindowState = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = ColorConstants.FondoPrincipal;
            ForeColor = ColorConstants.TextoPrincipal;
            Font = AppTheme.FuenteLabel;

            // Habilitar doble buffer para evitar parpadeos
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;

            ConstruirTopBar();
            ConstruirDock();
            ConstruirCentral();

            FormClosing += (_, _) =>
            {
                _timerStats?.Stop();
                _frmPC?.Close();
                _frmLE?.Close();
                _frmAPC?.Close();
                _frmALE?.Close();
                _frmGallery?.Close();
                _clockControl?.Dispose();
            };

            ResumeLayout(true);
        }

        // ─── TOP BAR ──────────────────────────────────────────────────────────

        private void ConstruirTopBar()
        {
            _panelTopBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 48,
                BackColor = ColorConstants.FondoSuperior,
                Padding = new Padding(0, 4, 0, 4)
            };
            _panelTopBar.Paint += (_, e) =>
            {
                using var pen = new Pen(ColorConstants.Separador, 1);
                e.Graphics.DrawLine(pen, 0, _panelTopBar.Height - 1, _panelTopBar.Width, _panelTopBar.Height - 1);
            };

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                BackColor = ColorConstants.FondoSuperior,
                Padding = new Padding(24, 0, 24, 0)
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var lblNombre = new Label
            {
                Text = "🍽  RestaurantOS  •  v" + AppConstants.Version,
                Font = AppTheme.FuenteSmallBold,
                ForeColor = ColorConstants.AcentoPrincipal,
                BackColor = ColorConstants.FondoSuperior,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize = false
            };

            var lblMateria = new Label
            {
                Text = "Sistemas Operativos — Universidad",
                Font = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.TextoHint,
                BackColor = ColorConstants.FondoSuperior,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize = false
            };

            var panelDer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = ColorConstants.FondoSuperior
            };
            panelDer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55));
            panelDer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45));
            panelDer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            _lblTopEstado = new Label
            {
                Text = "✅  Sistema activo",
                Font = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.AcentoExito,
                BackColor = ColorConstants.FondoSuperior,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                AutoSize = false
            };

            _lblTopHilos = new Label
            {
                Text = "Hilos: —",
                Font = AppTheme.FuenteStatus,
                ForeColor = ColorConstants.TextoHint,
                BackColor = ColorConstants.FondoSuperior,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                AutoSize = false
            };

            panelDer.Controls.Add(_lblTopEstado, 0, 0);
            panelDer.Controls.Add(_lblTopHilos, 1, 0);
            tbl.Controls.Add(lblNombre, 0, 0);
            tbl.Controls.Add(lblMateria, 1, 0);
            tbl.Controls.Add(panelDer, 2, 0);
            _panelTopBar.Controls.Add(tbl);
            Controls.Add(_panelTopBar);
        }

        // ─── DOCK ─────────────────────────────────────────────────────────────

        private void ConstruirDock()
        {
            _dockPC = CrearDockItem("🔄", "Productor\nConsumidor", "Detenido", ColorConstants.TarjetaProductor);
            _dockLE = CrearDockItem("📖", "Lectores\nEscritores", "Detenido", ColorConstants.TarjetaLectores);
            _dockAPC = CrearDockItem("⚡", "Ataque\nInyección", "Educativo", ColorConstants.TarjetaAtaque1);
            _dockALE = CrearDockItem("🎣", "Ataque\nPhishing", "Educativo", ColorConstants.TarjetaAtaque2);
            _dockGallery = CrearDockItem("🖼️", "Galería", "Listo", ColorConstants.AcentoSecundario);

            _dockPC.ItemClicked += (_, _) => AbrirPC();
            _dockLE.ItemClicked += (_, _) => AbrirLE();
            _dockAPC.ItemClicked += (_, _) => AbrirAPC();
            _dockALE.ItemClicked += (_, _) => AbrirALE();
            _dockGallery.ItemClicked += (_, _) => AbrirGallery();

            var tt = new ToolTip { ShowAlways = true, InitialDelay = 400, AutoPopDelay = 5000 };
            tt.SetToolTip(_dockPC, "Productor-Consumidor\nSemaphoreSlim + ConcurrentQueue");
            tt.SetToolTip(_dockLE, "Lectores-Escritores\nReaderWriterLockSlim");
            tt.SetToolTip(_dockAPC, "Ataque: Ingeniería Social\n⚠ Solo simulación educativa");
            tt.SetToolTip(_dockALE, "Ataque: Phishing al Gerente\n⚠ Solo simulación educativa");
            tt.SetToolTip(_dockGallery, "Galería de imágenes\nMuestra recursos visuales");

            int alturaItem = Math.Max(_dockPC.Height, Math.Max(_dockLE.Height, Math.Max(_dockAPC.Height, Math.Max(_dockALE.Height, _dockGallery.Height))));

            _panelDock = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = alturaItem + 28,
                BackColor = ColorConstants.FondoDock,
                Padding = new Padding(0, 8, 0, 8)
            };
            _panelDock.Paint += (_, e) =>
            {
                using var pen = new Pen(ColorConstants.BordeDock, 1);
                e.Graphics.DrawLine(pen, 0, 0, _panelDock.Width, 0);
            };

            foreach (var item in new[] { _dockPC, _dockLE, _dockAPC, _dockALE, _dockGallery })
                item.Margin = new Padding(10, 6, 10, 6);

            var dockFlow = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = ColorConstants.FondoDock,
                Padding = new Padding(0, 0, 0, 0)
            };

            _panelDock.Resize += (_, _) =>
            {
                int totalW = 0;
                foreach (var d in new[] { _dockPC, _dockLE, _dockAPC, _dockALE, _dockGallery })
                    totalW += d.Width + d.Margin.Horizontal;
                int margen = Math.Max(0, (_panelDock.Width - totalW) / 2);
                dockFlow.Padding = new Padding(margen, 4, margen, 4);
            };

            dockFlow.Controls.AddRange(new Control[] { _dockPC, _dockLE, _dockAPC, _dockALE, _dockGallery });
            _panelDock.Controls.Add(dockFlow);
            Controls.Add(_panelDock);
        }

        private DockItem CrearDockItem(string icono, string nombre, string estado, Color acento)
        {
            return new DockItem
            {
                Icono = icono,
                Nombre = nombre,
                Estado = estado,
                ColorAcento = acento,
                BackColor = ColorConstants.FondoDock
            };
        }

        // ─── PANEL CENTRAL ─────────────────────────────────────────────────

        private void ConstruirCentral()
        {
            _panelCentral = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorConstants.FondoPrincipal
            };
            _panelCentral.Paint += PanelCentral_Paint;

            // Control personalizado para el reloj (sin artefactos)
            _clockControl = new ClockControl
            {
                Dock = DockStyle.Fill,
                BackColor = ColorConstants.FondoPrincipal
            };
            _panelCentral.Controls.Add(_clockControl);

            Controls.Add(_panelCentral);
        }

        private void PanelCentral_Paint(object? sender, PaintEventArgs e)
        {
            // Solo pintar el fondo y el glow, el reloj lo hace el ClockControl
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int w = _panelCentral.Width;
            int h = _panelCentral.Height;
            if (w < 40 || h < 40) return;

            // Fondo degradado claro
            using var fondo = new LinearGradientBrush(
                new Rectangle(0, 0, w, h),
                ColorConstants.FondoPrincipal,
                Color.FromArgb(240, 245, 252),
                LinearGradientMode.Vertical);
            g.FillRectangle(fondo, 0, 0, w, h);

            // Glow central en azul claro (solo decorativo)
            float cx = w / 2f;
            float cy = h / 2f;
            float gr = Math.Min(w, h) * 0.42f;
            using var glowBrush = new PathGradientBrush(
                new PointF[] { new(cx - gr, cy - gr), new(cx + gr, cy - gr),
                               new(cx + gr, cy + gr), new(cx - gr, cy + gr) })
            {
                CenterPoint = new PointF(cx, cy),
                CenterColor = Color.FromArgb(20, ColorConstants.AcentoPrincipal),
                SurroundColors = new[] { Color.FromArgb(0, ColorConstants.AcentoPrincipal) }
            };
            g.FillEllipse(glowBrush, cx - gr, cy - gr, gr * 2, gr * 2);

            // Separador y hint (se pintan aquí, pero el reloj los dibuja también, así que los eliminamos para evitar duplicados)
            // El ClockControl se encargará de todo el contenido central.
        }

        // ─── TIMERS ──────────────────────────────────────────────────────────

        private void IniciarTimers()
        {
            // El ClockControl tiene su propio timer, no necesitamos uno para el reloj.

            _timerStats = new System.Windows.Forms.Timer { Interval = 2500 };
            _timerStats.Tick += (_, _) =>
            {
                try
                {
                    var proc = System.Diagnostics.Process.GetCurrentProcess();
                    if (_lblTopHilos != null)
                        _lblTopHilos.Text = $"Hilos: {proc.Threads.Count}  |  {proc.WorkingSet64 / 1024 / 1024} MB";
                }
                catch { }
            };
            _timerStats.Start();
        }

        // ─── NAVEGACIÓN ─────────────────────────────────────────────────────

        private void AbrirPC()
        {
            if (_frmPC == null || _frmPC.IsDisposed)
            {
                var svc = AppSettings.Resolver<ProductorConsumidorService>();
                var ataque = AppSettings.Resolver<AtaqueProductorConsumidorService>();
                _frmPC = new FrmProductorConsumidor(svc, ataque, _logger);
                _frmPC.FormClosed += (_, _) =>
                {
                    _frmPC = null;
                    _dockPC.Abierto = false;
                    _dockPC.Estado = "Detenido";
                    _dockPC.Activo = false;
                };
            }
            _dockPC.Abierto = true;
            _dockPC.Estado = "Abierto";
            _dockPC.Activo = true;
            AbrirVentanaHija(_frmPC);
        }

        private void AbrirLE()
        {
            if (_frmLE == null || _frmLE.IsDisposed)
            {
                var svc = AppSettings.Resolver<LectoresEscritoresService>();
                var ataque = AppSettings.Resolver<AtaqueLectoresEscritoresService>();
                _frmLE = new FrmLectoresEscritores(svc, ataque, _logger);
                _frmLE.FormClosed += (_, _) =>
                {
                    _frmLE = null;
                    _dockLE.Abierto = false;
                    _dockLE.Estado = "Detenido";
                    _dockLE.Activo = false;
                };
            }
            _dockLE.Abierto = true;
            _dockLE.Estado = "Abierto";
            _dockLE.Activo = true;
            AbrirVentanaHija(_frmLE);
        }

        private void AbrirAPC()
        {
            if (_frmAPC == null || _frmAPC.IsDisposed)
            {
                var svc = AppSettings.Resolver<ProductorConsumidorService>();
                var ataque = AppSettings.Resolver<AtaqueProductorConsumidorService>();
                _frmAPC = new FrmAtaqueProductorConsumidor(svc, ataque, _logger);
                _frmAPC.FormClosed += (_, _) =>
                {
                    _frmAPC = null;
                    _dockAPC.Abierto = false;
                    _dockAPC.Estado = "Educativo";
                    _dockAPC.Activo = false;
                };
            }
            _dockAPC.Abierto = true;
            _dockAPC.Estado = "Activo";
            _dockAPC.Activo = true;
            AbrirVentanaHija(_frmAPC);
        }

        private void AbrirALE()
        {
            if (_frmALE == null || _frmALE.IsDisposed)
            {
                var svc = AppSettings.Resolver<LectoresEscritoresService>();
                var ataque = AppSettings.Resolver<AtaqueLectoresEscritoresService>();
                _frmALE = new FrmAtaqueLectoresEscritores(svc, ataque, _logger);
                _frmALE.FormClosed += (_, _) =>
                {
                    _frmALE = null;
                    _dockALE.Abierto = false;
                    _dockALE.Estado = "Educativo";
                    _dockALE.Activo = false;
                };
            }
            _dockALE.Abierto = true;
            _dockALE.Estado = "Activo";
            _dockALE.Activo = true;
            AbrirVentanaHija(_frmALE);
        }

        private void AbrirGallery()
        {
            if (_frmGallery == null || _frmGallery.IsDisposed)
            {
                _frmGallery = new FrmGallery();
                _frmGallery.FormClosed += (_, _) =>
                {
                    _frmGallery = null;
                    _dockGallery.Abierto = false;
                    _dockGallery.Estado = "Listo";
                    _dockGallery.Activo = false;
                };
            }
            _dockGallery.Abierto = true;
            _dockGallery.Estado = "Abierto";
            _dockGallery.Activo = true;
            AbrirVentanaHija(_frmGallery);
        }

        private void AbrirVentanaHija(Form ventana)
        {
            ventana.Opacity = 0;
            ventana.Show();
            var timerFade = new System.Windows.Forms.Timer { Interval = 16 };
            timerFade.Tick += (s, e) =>
            {
                if (ventana.IsDisposed) { timerFade.Stop(); timerFade.Dispose(); return; }
                ventana.Opacity = Math.Min(1, ventana.Opacity + 0.08);
                if (ventana.Opacity >= 1) { timerFade.Stop(); timerFade.Dispose(); }
            };
            timerFade.Start();

            Hide();
            ventana.FormClosed += (_, _) =>
            {
                if (!IsDisposed) Show();
            };
        }
    }

    // ─── CONTROL PERSONALIZADO PARA EL RELOJ ──────────────────────────────

    /// <summary>
    /// Control que muestra la hora y fecha sin artefactos de renderizado.
    /// Utiliza DoubleBuffered y dibuja directamente con GDI+.
    /// </summary>
    internal sealed class ClockControl : Control
    {
        private readonly System.Windows.Forms.Timer _timer;
        private string _hora = "";
        private string _fecha = "";

        public ClockControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.DoubleBuffer |
                     ControlStyles.ResizeRedraw, true);
            DoubleBuffered = true;
            BackColor = ColorConstants.FondoPrincipal;

            _timer = new System.Windows.Forms.Timer { Interval = 1000 };
            _timer.Tick += (_, _) => ActualizarTexto();
            _timer.Start();

            ActualizarTexto();
        }

        private void ActualizarTexto()
        {
            var ahora = DateTime.Now;
            _hora = ahora.ToString("HH:mm:ss");
            string fecha = ahora.ToString("dddd, d 'de' MMMM 'de' yyyy", new System.Globalization.CultureInfo("es-ES"));
            _fecha = char.ToUpper(fecha[0]) + fecha[1..];
            Invalidate(); // Solo invalida este control, no todo el formulario
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int w = Width;
            int h = Height;
            if (w < 40 || h < 40) return;

            // Dibujar fondo transparente (se pinta el fondo del padre)
            // En realidad, el fondo ya lo pinta el panel central, así que no necesitamos pintar nada más.
            // Solo dibujamos el texto.

            // Glow decorativo (opcional)
            float cx = w / 2f;
            float cy = h / 2f;
            float gr = Math.Min(w, h) * 0.42f;
            using var glowBrush = new PathGradientBrush(
                new PointF[] { new(cx - gr, cy - gr), new(cx + gr, cy - gr),
                               new(cx + gr, cy + gr), new(cx - gr, cy + gr) })
            {
                CenterPoint = new PointF(cx, cy),
                CenterColor = Color.FromArgb(20, ColorConstants.AcentoPrincipal),
                SurroundColors = new[] { Color.FromArgb(0, ColorConstants.AcentoPrincipal) }
            };
            g.FillEllipse(glowBrush, cx - gr, cy - gr, gr * 2, gr * 2);

            // ── Hora ──────────────────────────────────────────────────────────
            float horaSize = Math.Clamp(w * 0.10f, 48f, 80f);
            using var horaFont = new Font("Segoe UI", horaSize, FontStyle.Bold);
            var horaMedida = TextRenderer.MeasureText(_hora, horaFont, new Size(w, int.MaxValue), TextFormatFlags.NoPadding);
            int horaH = (int)(horaMedida.Height * 1.25f);
            int horaY = (int)(cy - horaH * 0.65f);
            var horaRect = new Rectangle(0, horaY, w, horaH);

            // Sombra
            TextRenderer.DrawText(g, _hora, horaFont,
                new Rectangle(2, horaY + 3, w, horaH),
                Color.FromArgb(35, ColorConstants.AcentoPrincipal),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
            // Texto principal
            TextRenderer.DrawText(g, _hora, horaFont, horaRect,
                ColorConstants.TextoPrincipal,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            // ── Fecha ─────────────────────────────────────────────────────────
            float fechaSize = Math.Clamp(w * 0.022f, 16f, 28f);
            using var fechaFont = new Font("Segoe UI", fechaSize, FontStyle.Regular);
            var fechaMedida = TextRenderer.MeasureText(_fecha, fechaFont, new Size(w, int.MaxValue), TextFormatFlags.NoPadding);
            int fechaH = (int)(fechaMedida.Height * 1.5f);
            int fechaY = horaY + horaH + 8;
            var fechaRect = new Rectangle(0, fechaY, w, fechaH);
            TextRenderer.DrawText(g, _fecha, fechaFont, fechaRect,
                ColorConstants.TextoSecundario,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            // ── Separador ────────────────────────────────────────────────────
            int sepY = fechaY + fechaH + 18;
            int sepW = (int)(w * 0.14f);
            int sepX = (w - sepW) / 2;
            if (sepW > 0)
            {
                var blend = new ColorBlend(3)
                {
                    Colors = new[] { Color.FromArgb(0, ColorConstants.AcentoPrincipal),
                                     ColorConstants.AcentoPrincipal,
                                     Color.FromArgb(0, ColorConstants.AcentoPrincipal) },
                    Positions = new[] { 0f, 0.5f, 1f }
                };
                using var sepBrush = new LinearGradientBrush(
                    new Rectangle(sepX, sepY, sepW, 2),
                    ColorConstants.AcentoPrincipal,
                    ColorConstants.AcentoPrincipal,
                    LinearGradientMode.Horizontal)
                { InterpolationColors = blend };
                g.FillRectangle(sepBrush, sepX, sepY, sepW, 2);
            }

            // ── Hint ─────────────────────────────────────────────────────────
            int hintY = sepY + 22;
            using var hintFont = new Font("Segoe UI", 12f, FontStyle.Regular);
            var hintMedida = TextRenderer.MeasureText("Seleccioná un módulo desde el Dock inferior",
                hintFont, new Size(w, int.MaxValue), TextFormatFlags.NoPadding);
            int hintH = (int)(hintMedida.Height * 1.4f);
            TextRenderer.DrawText(g, "Seleccioná un módulo desde el Dock inferior",
                hintFont, new Rectangle(0, hintY, w, hintH),
                Color.FromArgb(80, ColorConstants.TextoSecundario),
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer?.Stop();
                _timer?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}