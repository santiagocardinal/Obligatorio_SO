using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using RestauranteSO.Constants;
using RestauranteSO.Presentation.Themes;

namespace RestauranteSO.Presentation.Controls
{
    /// <summary>
    /// Card completamente rediseñada.
    /// Tamaño mínimo 300x240. Icono 64px. Fuentes grandes.
    /// Animación hover con gradiente y sombra.
    /// </summary>
    public sealed class SimulationCard : Control
    {
        // ─── CAMPOS ───────────────────────────────────────────────────────────

        private string _icono       = "🍽";
        private string _titulo      = "Título";
        private string _descripcion = "Descripción de la simulación.";
        private string _estadoTexto = "● Listo";
        private Color  _estadoColor = ColorConstants.AcentoExito;
        private Color  _colorAcento = ColorConstants.AcentoPrincipal;
        private string _subtexto    = "";
        private string _numeracion  = "01";

        private bool  _isHovered  = false;
        private bool  _isPressed  = false;
        private float _hoverAlpha = 0f;

        private readonly System.Windows.Forms.Timer _animTimer;
        private readonly System.Windows.Forms.Timer _clickTimer;
        private const int Radio = 16;

        // ─── PROPIEDADES ──────────────────────────────────────────────────────

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Icono
        {
            get => _icono;
            set { _icono = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Titulo
        {
            get => _titulo;
            set { _titulo = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Descripcion
        {
            get => _descripcion;
            set { _descripcion = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string EstadoTexto
        {
            get => _estadoTexto;
            set { _estadoTexto = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color EstadoColor
        {
            get => _estadoColor;
            set { _estadoColor = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color ColorAcento
        {
            get => _colorAcento;
            set { _colorAcento = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Subtexto
        {
            get => _subtexto;
            set { _subtexto = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Numeracion
        {
            get => _numeracion;
            set { _numeracion = value; Invalidate(); }
        }

        // ─── CONSTRUCTOR ─────────────────────────────────────────────────────

        public SimulationCard()
        {
            SetStyle(
                ControlStyles.UserPaint              |
                ControlStyles.AllPaintingInWmPaint   |
                ControlStyles.DoubleBuffer           |
                ControlStyles.ResizeRedraw           |
                ControlStyles.SupportsTransparentBackColor,
                true);

            BackColor   = Color.Transparent;
            Cursor      = Cursors.Hand;
            MinimumSize = new Size(300, 240);
            Size        = new Size(340, 260);

            // Timer animación hover ~60fps
            _animTimer = new System.Windows.Forms.Timer { Interval = 16 };
            _animTimer.Tick += (_, _) =>
            {
                float target = _isHovered ? 1f : 0f;
                float prev   = _hoverAlpha;
                float step   = 0.07f;

                _hoverAlpha = _hoverAlpha < target
                    ? Math.Min(target, _hoverAlpha + step)
                    : Math.Max(target, _hoverAlpha - step);

                if (Math.Abs(_hoverAlpha - prev) > 0.004f)
                    Invalidate();
            };
            _animTimer.Start();

            // Timer clic
            _clickTimer = new System.Windows.Forms.Timer { Interval = 120 };
            _clickTimer.Tick += (_, _) =>
            {
                _isPressed = false;
                _clickTimer.Stop();
                Invalidate();
            };
        }

        // ─── MOUSE ───────────────────────────────────────────────────────────

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            _isPressed = true;
            _clickTimer.Start();
            Invalidate();
        }

        // ─── PINTADO PRINCIPAL ────────────────────────────────────────────────

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Área de la card (dejamos margen para la sombra)
            int margen   = 6;
            var cardRect = new Rectangle(
                margen, margen,
                Width  - margen * 2 - 1,
                Height - margen * 2 - 1);

            // ── Sombra ────────────────────────────────────────────────────
            int sombra = (int)(4 + _hoverAlpha * 8);
            for (int i = sombra; i >= 1; i--)
            {
                var sr = new Rectangle(
                    cardRect.X + i,  cardRect.Y + i,
                    cardRect.Width,  cardRect.Height);
                int alpha = (int)(20.0 / i * (1 + _hoverAlpha));
                using var sb = new SolidBrush(Color.FromArgb(
                    Math.Min(alpha, 60), 0, 0, 0));
                using var sp = AppTheme.CrearPathRedondeado(sr, Radio);
                g.FillPath(sb, sp);
            }

            // ── Fondo con gradiente ───────────────────────────────────────
            Color fondoArriba = InterpolColor(
                ColorConstants.FondoCard,
                Color.FromArgb(48, 48, 65),
                _hoverAlpha);
            Color fondoAbajo = InterpolColor(
                Color.FromArgb(28, 28, 40),
                Color.FromArgb(35, 35, 52),
                _hoverAlpha);

            AppTheme.RellenarGradienteRedondeado(
                g, cardRect, Radio, fondoArriba, fondoAbajo);

            // ── Borde izquierdo de acento (barra vertical) ────────────────
            int barraAncho = (int)(3 + _hoverAlpha * 2);
            using (var barraPath = new System.Drawing.Drawing2D.GraphicsPath())
            {
                barraPath.AddRectangle(new Rectangle(
                    cardRect.X, cardRect.Y + Radio,
                    barraAncho,
                    cardRect.Height - Radio * 2));
                using var barraBrush = new SolidBrush(
                    Color.FromArgb((int)(180 + 75 * _hoverAlpha), _colorAcento));
                g.FillPath(barraBrush, barraPath);
            }

            // ── Borde exterior ────────────────────────────────────────────
            Color bordeColor = InterpolColor(
                ColorConstants.BordeCard, _colorAcento, _hoverAlpha);
            int bordeGrosor = _hoverAlpha > 0.5f ? 2 : 1;
            AppTheme.DibujarBordeRedondeado(
                g, cardRect, Radio, bordeColor, bordeGrosor);

            // ── Número de card (decorativo, esquina superior derecha) ─────
            using (var nf = new Font("Segoe UI", 32f, FontStyle.Bold))
            {
                var nr = new Rectangle(
                    cardRect.Right - 70, cardRect.Y + 10, 60, 44);
                using var nb = new SolidBrush(
                    Color.FromArgb(18, _colorAcento));
                g.FillRectangle(nb, nr);
                TextRenderer.DrawText(g, _numeracion, nf, nr,
                    Color.FromArgb((int)(30 + 40 * _hoverAlpha), _colorAcento),
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter);
            }

            // ── Icono (64px, centrado en la mitad superior) ───────────────
            int iconoSize = 64;
            int iconoY    = cardRect.Y + 28;
            var iconoRect = new Rectangle(
                cardRect.X + (cardRect.Width - iconoSize) / 2,
                iconoY,
                iconoSize, iconoSize);

            // Círculo de fondo del icono
            using (var circuloBrush = new SolidBrush(
                Color.FromArgb((int)(30 + 30 * _hoverAlpha), _colorAcento)))
            {
                int padding = 8;
                g.FillEllipse(circuloBrush,
                    iconoRect.X - padding, iconoRect.Y - padding,
                    iconoRect.Width + padding * 2,
                    iconoRect.Height + padding * 2);
            }

            using (var iconFont = new Font(
                "Segoe UI Emoji",
                32f + _hoverAlpha * 4f,
                FontStyle.Regular))
            {
                TextRenderer.DrawText(g, _icono, iconFont, iconoRect,
                    ColorConstants.TextoPrincipal,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter);
            }

            // ── Título ────────────────────────────────────────────────────
            int tituloY = iconoY + iconoSize + 14;
            using (var tf = new Font("Segoe UI", 13f, FontStyle.Bold))
            {
                var tr = new Rectangle(
                    cardRect.X + 16, tituloY,
                    cardRect.Width - 32, 28);
                TextRenderer.DrawText(g, _titulo, tf, tr,
                    ColorConstants.TextoPrincipal,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.Top              |
                    TextFormatFlags.SingleLine        |
                    TextFormatFlags.EndEllipsis);
            }

            // ── Descripción ───────────────────────────────────────────────
            int descY = tituloY + 34;
            using (var df = new Font("Segoe UI", 10.5f, FontStyle.Regular))
            {
                int descH = cardRect.Bottom - descY - 44;
                var dr    = new Rectangle(
                    cardRect.X + 20, descY,
                    cardRect.Width - 40,
                    Math.Max(descH, 40));
                TextRenderer.DrawText(g, _descripcion, df, dr,
                    ColorConstants.TextoSecundario,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.Top              |
                    TextFormatFlags.WordBreak);
            }

            // ── Badge de estado (parte inferior) ──────────────────────────
            int badgeH   = 26;
            int badgeY   = cardRect.Bottom - badgeH - 12;
            using (var bf = new Font("Segoe UI", 9f, FontStyle.Bold))
            {
                var bMeasure = TextRenderer.MeasureText(_estadoTexto, bf);
                int badgeW   = bMeasure.Width + 24;
                int badgeX   = cardRect.X + (cardRect.Width - badgeW) / 2;
                var badgeRect = new Rectangle(badgeX, badgeY, badgeW, badgeH);

                using var bgBrush = new SolidBrush(
                    Color.FromArgb(45, _estadoColor));
                AppTheme.RellenarRedondeado(
                    g, badgeRect, 13, bgBrush.Color);

                using var borderBrush = new SolidBrush(
                    Color.FromArgb(80, _estadoColor));
                AppTheme.DibujarBordeRedondeado(
                    g, badgeRect, 13, borderBrush.Color, 1);

                TextRenderer.DrawText(g, _estadoTexto, bf, badgeRect,
                    _estadoColor,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter);
            }

            // ── Efecto press ──────────────────────────────────────────────
            if (_isPressed)
            {
                using var pb = new SolidBrush(
                    Color.FromArgb(25, 255, 255, 255));
                AppTheme.RellenarRedondeado(
                    g, cardRect, Radio, pb.Color);
            }

            // ── Brillo superior (highlight) en hover ──────────────────────
            if (_hoverAlpha > 0.1f)
            {
                var brilloRect = new Rectangle(
                    cardRect.X + 2, cardRect.Y + 2,
                    cardRect.Width - 4, 40);
                using var brilloBrush = new LinearGradientBrush(
                    brilloRect,
                    Color.FromArgb((int)(15 * _hoverAlpha), 255, 255, 255),
                    Color.FromArgb(0, 255, 255, 255),
                    LinearGradientMode.Vertical);
                using var brilloPath =
                    AppTheme.CrearPathRedondeado(brilloRect, Radio);
                g.FillPath(brilloBrush, brilloPath);
            }
        }

        // ─── HELPERS ─────────────────────────────────────────────────────────

        private static Color InterpolColor(Color c1, Color c2, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return Color.FromArgb(
                (int)(c1.A + (c2.A - c1.A) * t),
                (int)(c1.R + (c2.R - c1.R) * t),
                (int)(c1.G + (c2.G - c1.G) * t),
                (int)(c1.B + (c2.B - c1.B) * t));
        }

        // ─── DISPOSE ─────────────────────────────────────────────────────────

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animTimer.Stop();  _animTimer.Dispose();
                _clickTimer.Stop(); _clickTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}