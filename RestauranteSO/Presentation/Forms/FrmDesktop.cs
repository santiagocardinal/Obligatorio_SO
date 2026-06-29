using System.Drawing.Drawing2D;
using System.Drawing.Text;
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

        private Panel _panelTopBar  = null!;
        private Panel _panelCuerpo  = null!;
        private Panel _panelIzq     = null!;
        private Panel _panelCentral = null!;
        private Panel _panelDock    = null!;

        private DockItem _dockPC  = null!;
        private DockItem _dockLE  = null!;
        private DockItem _dockAPC = null!;
        private DockItem _dockALE = null!;

        private string _horaTexto  = "";
        private string _fechaTexto = "";

        private System.Windows.Forms.Timer _timerReloj = null!;
        private System.Windows.Forms.Timer _timerStats = null!;

        private Label _lblTopEstado = null!;
        private Label _lblTopHilos  = null!;

        private FrmProductorConsumidor?       _frmPC  = null;
        private FrmLectoresEscritores?        _frmLE  = null;
        private FrmAtaqueProductorConsumidor? _frmAPC = null;
        private FrmAtaqueLectoresEscritores?  _frmALE = null;

        public FrmDesktop()
        {
            _logger = AppSettings.Resolver<ISimulationLogger>();
            InitializeComponent();
            IniciarTimers();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            Text          = "RestaurantOS";
            MinimumSize   = new Size(1200, 800);
            WindowState   = FormWindowState.Maximized;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor     = Color.FromArgb(8, 8, 20);
            ForeColor     = ColorConstants.TextoPrincipal;
            Font          = AppTheme.FuenteLabel;

            ConstruirTopBar();
            ConstruirDock();
            ConstruirCuerpo();

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

        // ─── TOP BAR ─────────────────────────────────────────────────────────

        private void ConstruirTopBar()
        {
            // Altura calculada para que la fuente respire
            int alturaTop = AppTheme.AlturaSegura(AppTheme.FuenteSmallBold, 1, 10);

            _panelTopBar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = Math.Max(alturaTop, 44),
                BackColor = Color.FromArgb(12, 12, 24)
            };
            _panelTopBar.Paint += (_, e) =>
            {
                using var pen = new Pen(ColorConstants.Separador, 1);
                e.Graphics.DrawLine(pen,
                    0, _panelTopBar.Height - 1,
                    _panelTopBar.Width, _panelTopBar.Height - 1);
            };

            var tbl = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 3,
                RowCount    = 1,
                BackColor   = Color.FromArgb(12, 12, 24),
                Padding     = new Padding(20, 0, 20, 0)
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34f));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var lblNombre = new Label
            {
                Text      = "🍽  RestaurantOS  •  v" + AppConstants.Version,
                Font      = AppTheme.FuenteSmallBold,
                ForeColor = ColorConstants.AcentoPrincipal,
                BackColor = Color.FromArgb(12, 12, 24),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize  = false,
                Padding   = new Padding(0, 4, 0, 4)
            };

            var lblMateria = new Label
            {
                Text      = "Sistemas Operativos",
                Font      = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.TextoHint,
                BackColor = Color.FromArgb(12, 12, 24),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                AutoSize  = false,
                Padding   = new Padding(0, 4, 0, 4)
            };

            var panelDer = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 1,
                BackColor   = Color.FromArgb(12, 12, 24)
            };
            panelDer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55f));
            panelDer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45f));
            panelDer.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            _lblTopEstado = new Label
            {
                Text      = "✅  Sistema activo",
                Font      = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.AcentoExito,
                BackColor = Color.FromArgb(12, 12, 24),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                AutoSize  = false,
                Padding   = new Padding(0, 4, 0, 4)
            };

            _lblTopHilos = new Label
            {
                Text      = "Hilos: —",
                Font      = AppTheme.FuenteStatus,
                ForeColor = ColorConstants.TextoHint,
                BackColor = Color.FromArgb(12, 12, 24),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                AutoSize  = false,
                Padding   = new Padding(0, 4, 8, 4)
            };

            panelDer.Controls.Add(_lblTopEstado, 0, 0);
            panelDer.Controls.Add(_lblTopHilos,  1, 0);
            tbl.Controls.Add(lblNombre,  0, 0);
            tbl.Controls.Add(lblMateria, 1, 0);
            tbl.Controls.Add(panelDer,   2, 0);
            _panelTopBar.Controls.Add(tbl);
            Controls.Add(_panelTopBar);
        }

        // ─── DOCK ─────────────────────────────────────────────────────────────

        private void ConstruirDock()
        {
            _dockPC = CrearDockItem("🔄", "Productor\nConsumidor",
                "Detenido", ColorConstants.TarjetaProductor);
            _dockLE = CrearDockItem("📖", "Lectores\nEscritores",
                "Detenido", ColorConstants.TarjetaLectores);
            _dockAPC = CrearDockItem("⚡", "Ataque\nInyección",
                "Educativo", ColorConstants.TarjetaAtaque1);
            _dockALE = CrearDockItem("🎣", "Ataque\nPhishing",
                "Educativo", ColorConstants.TarjetaAtaque2);

            _dockPC.ItemClicked  += (_, _) => AbrirPC();
            _dockLE.ItemClicked  += (_, _) => AbrirLE();
            _dockAPC.ItemClicked += (_, _) => AbrirAPC();
            _dockALE.ItemClicked += (_, _) => AbrirALE();

            var tt = new ToolTip
            {
                ShowAlways   = true,
                InitialDelay = 400,
                AutoPopDelay = 5000
            };
            tt.SetToolTip(_dockPC,
                "Productor-Consumidor\nSemaphoreSlim + ConcurrentQueue");
            tt.SetToolTip(_dockLE,
                "Lectores-Escritores\nReaderWriterLockSlim");
            tt.SetToolTip(_dockAPC,
                "Ataque: Ingeniería Social\n⚠ Solo simulación educativa");
            tt.SetToolTip(_dockALE,
                "Ataque: Phishing al Gerente\n⚠ Solo simulación educativa");

            // Altura del dock = el item más alto + padding
            int alturaItem = _dockPC.Height;
            foreach (var d in new[] { _dockLE, _dockAPC, _dockALE })
                alturaItem = Math.Max(alturaItem, d.Height);

            _panelDock = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = alturaItem + 20,   // 10px arriba + 10px abajo
                BackColor = Color.FromArgb(10, 10, 20)
            };
            _panelDock.Paint += (_, e) =>
            {
                using var pen = new Pen(
                    Color.FromArgb(50, ColorConstants.AcentoPrincipal), 1);
                e.Graphics.DrawLine(pen, 0, 0,
                    _panelDock.Width, 0);
            };

            // Márgenes uniformes
            foreach (var item in new[] { _dockPC, _dockLE, _dockAPC, _dockALE })
                item.Margin = new Padding(8, 8, 8, 8);

            var dockFlow = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false,
                BackColor     = Color.FromArgb(10, 10, 20),
                Padding       = new Padding(0, 0, 0, 0)
            };

            // Centrar dinámicamente
            _panelDock.Resize += (_, _) =>
            {
                int totalW = 0;
                foreach (var d in new[] { _dockPC, _dockLE, _dockAPC, _dockALE })
                    totalW += d.Width + d.Margin.Horizontal;
                int margen = Math.Max(0, (_panelDock.Width - totalW) / 2);
                dockFlow.Padding = new Padding(margen, 8, margen, 8);
            };

            dockFlow.Controls.AddRange(
                new Control[] { _dockPC, _dockLE, _dockAPC, _dockALE });
            _panelDock.Controls.Add(dockFlow);
            Controls.Add(_panelDock);
        }
        private static DockItem CrearDockItem(
            string icono, string nombre, string estado, Color acento)
        {
            return new DockItem
            {
                Icono       = icono,
                Nombre      = nombre,
                Estado      = estado,
                ColorAcento = acento,
                BackColor   = Color.FromArgb(10, 10, 20)
            };
        }

        // ─── CUERPO ───────────────────────────────────────────────────────────

        private void ConstruirCuerpo()
        {
            _panelCuerpo = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.FromArgb(8, 8, 20)
            };
            _panelCuerpo.Paint += Cuerpo_Paint;

            _panelIzq = new Panel
            {
                Dock      = DockStyle.Left,
                BackColor = Color.FromArgb(8, 8, 20)
            };
            _panelIzq.Paint += PanelIzq_Paint;

            _panelCentral = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.FromArgb(8, 8, 20)
            };
            _panelCentral.Paint += PanelCentral_Paint;

            // 32% izquierda, el resto central
            _panelCuerpo.Resize += (_, _) =>
            {
                if (_panelCuerpo.Width < 100) return;
                _panelIzq.Width = (int)(_panelCuerpo.Width * 0.32f);
                _panelIzq.Invalidate();
                _panelCentral.Invalidate();
            };

            _panelCuerpo.Controls.Add(_panelCentral);
            _panelCuerpo.Controls.Add(_panelIzq);
            Controls.Add(_panelCuerpo);
        }

        // ─── FONDO ───────────────────────────────────────────────────────────

        private void Cuerpo_Paint(object? sender, PaintEventArgs e)
        {
            var g    = e.Graphics;
            var rect = new Rectangle(0, 0,
                _panelCuerpo.Width, _panelCuerpo.Height);
            if (rect.Width <= 0 || rect.Height <= 0) return;

            using var lgb = new LinearGradientBrush(
                rect,
                Color.FromArgb(14, 16, 40),
                Color.FromArgb( 4,  4, 12),
                LinearGradientMode.Vertical);
            g.FillRectangle(lgb, rect);
        }

        // ─── PANEL IZQUIERDO ─────────────────────────────────────────────────

        private void PanelIzq_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            int w = _panelIzq.Width;
            int h = _panelIzq.Height;
            if (w < 60 || h < 60) return;

            // Fondo panel
            using var fondo = new LinearGradientBrush(
                new Rectangle(0, 0, w, h),
                Color.FromArgb(10, 10, 26),
                Color.FromArgb( 6,  6, 16),
                LinearGradientMode.Vertical);
            g.FillRectangle(fondo, 0, 0, w, h);

            // Card interior
            int cardM = 18;
            var cardR = new Rectangle(cardM, cardM,
                w - cardM * 2, h - cardM * 2);
            if (cardR.Width <= 0 || cardR.Height <= 0) return;

            using var cardBrush = new SolidBrush(Color.FromArgb(24, 24, 44));
            using var cardPath  = AppTheme.CrearPathRedondeado(cardR, 18);
            g.FillPath(cardBrush, cardPath);

            using var cardBorder = new Pen(
                Color.FromArgb(50, ColorConstants.AcentoPrincipal), 1f);
            g.DrawPath(cardBorder, cardPath);

            // Línea acento superior
            var accentR = new Rectangle(cardR.X, cardR.Y, cardR.Width, 4);
            using var accentPath = AppTheme.CrearPathRedondeado(accentR, 3);
            using var accentBrush = new LinearGradientBrush(
                accentR.Width > 0 ? accentR : new Rectangle(0, 0, 1, 4),
                ColorConstants.AcentoPrincipal,
                ColorConstants.TarjetaLectores,
                LinearGradientMode.Horizontal);
            g.FillPath(accentBrush, accentPath);

            // Contenido — margen interior generoso
            int px  = cardR.X + 24;
            int py  = cardR.Y + 24;
            int mxW = cardR.Width - 48;
            if (mxW < 40) return;

            // Nombre sistema
            using var fNombre = new Font("Segoe UI Emoji", 16f, FontStyle.Bold);
            py = DibujarTextoSeguro(g, "🍽  RestaurantOS",
                fNombre, px, py, mxW,
                ColorConstants.TextoPrincipal, 6);
            py += 2;

            // Tagline
            py = DibujarTextoSeguro(g,
                "Restaurant Operating Environment",
                AppTheme.FuenteSmall, px, py, mxW,
                ColorConstants.AcentoPrincipal, 4);
            py += 12;

            // Separador
            using var sp = new Pen(
                Color.FromArgb(45, ColorConstants.AcentoPrincipal), 1);
            g.DrawLine(sp, px, py, px + mxW, py);
            py += 14;

            // Secciones
            py = DibujarBloque(g, px, py, mxW,
                "DESCRIPCIÓN",
                "Simulador educativo de problemas clásicos de\n" +
                "concurrencia en Sistemas Operativos, ambientado\n" +
                "en la operación diaria de un restaurante.",
                ColorConstants.AcentoPrincipal);
            py += 6;

            py = DibujarBloque(g, px, py, mxW,
                "MÓDULOS",
                "🔄  Productor — Consumidor\n" +
                "📖  Lectores — Escritores\n" +
                "⚡  Ataque: Ingeniería Social\n" +
                "🎣  Ataque: Phishing al Gerente",
                ColorConstants.TarjetaLectores);
            py += 6;

            py = DibujarBloque(g, px, py, mxW,
                "PRIMITIVAS DE CONCURRENCIA",
                "SemaphoreSlim\nReaderWriterLockSlim\n" +
                "ConcurrentQueue  •  CancellationToken\n" +
                "ManualResetEventSlim  •  Interlocked",
                ColorConstants.AcentoSecundario);
            py += 6;

            py = DibujarBloque(g, px, py, mxW,
                "TECNOLOGÍAS",
                "C# 13  •  .NET 9  •  WinForms\n" +
                "Clean Architecture  •  SOLID",
                ColorConstants.AcentoExito);
            py += 6;

            DibujarBloque(g, px, py, mxW,
                "CÓMO NAVEGAR",
                "Usá el Dock inferior para abrir\n" +
                "cada módulo de simulación.",
                ColorConstants.Advertencia);
        }

        /// <summary>
        /// Dibuja texto con medición previa garantizando que no se recorte.
        /// Devuelve la Y siguiente disponible.
        /// </summary>
        private static int DibujarTextoSeguro(
            Graphics g, string texto, Font font,
            int x, int y, int maxW, Color color,
            int paddingV = 6)
        {
            if (string.IsNullOrEmpty(texto)) return y;

            var flags = TextFormatFlags.Left | TextFormatFlags.Top |
                        TextFormatFlags.WordBreak | TextFormatFlags.NoPadding;

            var medida = TextRenderer.MeasureText(
                texto, font, new Size(maxW, int.MaxValue), flags);

            // Rect con padding vertical extra para no cortar descendentes
            var rect = new Rectangle(x, y, maxW,
                medida.Height + paddingV * 2);

            TextRenderer.DrawText(g, texto, font,
                new Rectangle(x, y + paddingV, maxW, medida.Height + paddingV),
                color, flags);

            return y + rect.Height;
        }

        /// <summary>
        /// Dibuja un bloque de título + contenido con espaciado garantizado.
        /// </summary>
        private static int DibujarBloque(
            Graphics g, int x, int y, int maxW,
            string titulo, string contenido, Color colorTitulo)
        {
            // Título
            y = DibujarTextoSeguro(g, titulo, AppTheme.FuenteSmallBold,
                x, y, maxW, colorTitulo, 3);

            // Líneas de contenido
            foreach (var linea in contenido.Split('\n'))
            {
                y = DibujarTextoSeguro(g, linea, AppTheme.FuenteSmall,
                    x + 6, y, maxW - 6,
                    ColorConstants.TextoSecundario, 2);
            }

            return y + 6;
        }

        // ─── PANEL CENTRAL — RELOJ ────────────────────────────────────────────

        private void PanelCentral_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            int w = _panelCentral.Width;
            int h = _panelCentral.Height;
            if (w < 40 || h < 40) return;

            // Fondo
            using var fondo = new LinearGradientBrush(
                new Rectangle(0, 0, w, h),
                Color.FromArgb(14, 16, 40),
                Color.FromArgb( 4,  4, 12),
                LinearGradientMode.Vertical);
            g.FillRectangle(fondo, 0, 0, w, h);

            // Glow central
            float cx = w / 2f;
            float cy = h / 2f;
            float gr = Math.Min(w, h) * 0.42f;

            using var glowBrush = new PathGradientBrush(
                new PointF[]
                {
                    new(cx - gr, cy - gr), new(cx + gr, cy - gr),
                    new(cx + gr, cy + gr), new(cx - gr, cy + gr)
                })
            {
                CenterPoint    = new PointF(cx, cy),
                CenterColor    = Color.FromArgb(20, ColorConstants.AcentoPrincipal),
                SurroundColors = new[] { Color.FromArgb(0, 0, 0, 0) }
            };
            g.FillEllipse(glowBrush, cx - gr, cy - gr, gr * 2, gr * 2);

            // ── Hora ──────────────────────────────────────────────────────────
            // Tamaño adaptativo con mínimo/máximo generosos
            float horaSize = Math.Clamp(w * 0.10f, 48f, 80f);
            using var horaFont = new Font("Segoe UI", horaSize, FontStyle.Bold);

            // Medir EXACTAMENTE cuánto ocupa la hora
            var horaMedida = TextRenderer.MeasureText(
                _horaTexto, horaFont,
                new Size(w, int.MaxValue),
                TextFormatFlags.NoPadding);

            // Centrar verticalmente con offset hacia arriba del centro
            // Reservamos 45% del alto para la hora y 25% para la fecha
            int horaH   = (int)(horaMedida.Height * 1.25f); // padding 25%
            int horaY   = (int)(cy - horaH * 0.65f);
            var horaRect = new Rectangle(0, horaY, w, horaH);

            // Sombra
            TextRenderer.DrawText(g, _horaTexto, horaFont,
                new Rectangle(2, horaY + 3, w, horaH),
                Color.FromArgb(35, ColorConstants.AcentoPrincipal),
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter   |
                TextFormatFlags.NoPadding);

            // Hora
            TextRenderer.DrawText(g, _horaTexto, horaFont,
                horaRect,
                ColorConstants.TextoPrincipal,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter   |
                TextFormatFlags.NoPadding);

            // ── Fecha ─────────────────────────────────────────────────────────
            float fechaSize = Math.Clamp(w * 0.022f, 16f, 28f);
            using var fechaFont = new Font("Segoe UI", fechaSize, FontStyle.Regular);

            var fechaMedida = TextRenderer.MeasureText(
                _fechaTexto, fechaFont,
                new Size(w, int.MaxValue),
                TextFormatFlags.NoPadding);

            int fechaH = (int)(fechaMedida.Height * 1.5f);
            int fechaY = horaY + horaH + 8;
            var fechaRect = new Rectangle(0, fechaY, w, fechaH);

            TextRenderer.DrawText(g, _fechaTexto, fechaFont,
                fechaRect,
                ColorConstants.TextoSecundario,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter   |
                TextFormatFlags.NoPadding);

            // ── Separador ─────────────────────────────────────────────────────
            int sepY = fechaY + fechaH + 18;
            int sepW = (int)(w * 0.14f);
            int sepX = (w - sepW) / 2;

            if (sepW > 0)
            {
                var blend = new ColorBlend(3)
                {
                    Colors    = new[]
                    {
                        Color.FromArgb(0,   ColorConstants.AcentoPrincipal),
                        ColorConstants.AcentoPrincipal,
                        Color.FromArgb(0,   ColorConstants.AcentoPrincipal)
                    },
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

            // ── Hint ──────────────────────────────────────────────────────────
            int hintY = sepY + 22;
            using var hintFont = new Font("Segoe UI", 12f, FontStyle.Regular);
            var hintMedida = TextRenderer.MeasureText(
                "Seleccioná un módulo desde el Dock inferior",
                hintFont, new Size(w, int.MaxValue),
                TextFormatFlags.NoPadding);
            int hintH = (int)(hintMedida.Height * 1.4f);

            TextRenderer.DrawText(g,
                "Seleccioná un módulo desde el Dock inferior",
                hintFont,
                new Rectangle(0, hintY, w, hintH),
                Color.FromArgb(60, ColorConstants.TextoSecundario),
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter   |
                TextFormatFlags.NoPadding);
        }

        // ─── TIMERS ───────────────────────────────────────────────────────────

        private void IniciarTimers()
        {
            ActualizarReloj();

            _timerReloj = new System.Windows.Forms.Timer { Interval = 1000 };
            _timerReloj.Tick += (_, _) =>
            {
                ActualizarReloj();
                _panelCentral?.Invalidate();
            };
            _timerReloj.Start();

            _timerStats = new System.Windows.Forms.Timer { Interval = 2500 };
            _timerStats.Tick += (_, _) =>
            {
                try
                {
                    var proc = System.Diagnostics.Process.GetCurrentProcess();
                    if (_lblTopHilos != null)
                        _lblTopHilos.Text =
                            $"Hilos: {proc.Threads.Count}  |  " +
                            $"{proc.WorkingSet64 / 1024 / 1024} MB";
                }
                catch { }
            };
            _timerStats.Start();
        }

        private void ActualizarReloj()
        {
            var ahora   = DateTime.Now;
            _horaTexto  = ahora.ToString("HH:mm:ss");
            _fechaTexto = ahora.ToString(
                "dddd, d 'de' MMMM 'de' yyyy",
                new System.Globalization.CultureInfo("es-ES"));
            if (_fechaTexto.Length > 0)
                _fechaTexto = char.ToUpper(_fechaTexto[0]) +
                              _fechaTexto[1..];
        }

        // ─── NAVEGACIÓN ───────────────────────────────────────────────────────

        private void AbrirPC()
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
                        _dockPC.Abierto = false;
                        _dockPC.Estado  = "Detenido";
                    };
                }
                _dockPC.Abierto = true;
                _dockPC.Estado  = "Abierto";
                AbrirVentanaHija(_frmPC);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error:\n\n{ex.Message}\n\n{ex.StackTrace}",
                    "Debug", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
                private void AbrirLE()
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
                            _dockPC.Abierto = false;
                            _dockPC.Estado  = "Detenido";
                        };
                    }
                    _dockPC.Abierto = true;
                    _dockPC.Estado  = "Abierto";
                    AbrirVentanaHija(_frmPC);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error:\n\n{ex.Message}\n\n{ex.StackTrace}",
                        "Debug", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
        }

        private void AbrirAPC()
        {
            try
            {
                if (_frmAPC == null || _frmAPC.IsDisposed)
                {
                    var svc    = AppSettings.Resolver<ProductorConsumidorService>();
                    var ataque = AppSettings.Resolver<AtaqueProductorConsumidorService>();
                    _frmAPC = new FrmAtaqueProductorConsumidor(svc, ataque, _logger);
                    _frmAPC.FormClosed += (_, _) =>
                    {
                        _frmAPC = null;
                        _dockAPC.Abierto = false;
                        _dockAPC.Estado  = "Educativo";
                    };
                }
                _dockAPC.Abierto = true;
                _dockAPC.Estado  = "Activo";
                AbrirVentanaHija(_frmAPC);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error:\n\n{ex.Message}\n\n{ex.StackTrace}",
                    "Debug", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void AbrirALE()
        {
            try
            {
                if (_frmALE == null || _frmALE.IsDisposed)
                {
                    var svc    = AppSettings.Resolver<LectoresEscritoresService>();
                    var ataque = AppSettings.Resolver<AtaqueLectoresEscritoresService>();
                    _frmALE = new FrmAtaqueLectoresEscritores(svc, ataque, _logger);
                    _frmALE.FormClosed += (_, _) =>
                    {
                        _frmALE = null;
                        _dockALE.Abierto = false;
                        _dockALE.Estado  = "Educativo";
                    };
                }
                _dockALE.Abierto = true;
                _dockALE.Estado  = "Activo";
                AbrirVentanaHija(_frmALE);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error:\n\n{ex.Message}\n\n{ex.StackTrace}",
                    "Debug", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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

        private static void MostrarError(string modulo, Exception ex)
        {
            MessageBox.Show(
                $"Error al abrir {modulo}:\n\n{ex.Message}",
                "RestaurantOS — Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}