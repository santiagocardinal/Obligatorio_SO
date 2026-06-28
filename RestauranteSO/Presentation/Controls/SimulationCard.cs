using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using RestauranteSO.Constants;

namespace RestauranteSO.Presentation.Controls
{
    /// <summary>
    /// Card pintada 100% con GDI+. Sin controles hijos.
    /// Todos los eventos de mouse van directo al control base.
    /// Los textos se miden con MeasureText para nunca cortarse.
    /// </summary>
    public sealed class SimulationCard : Control
    {
        // ─── CAMPOS ───────────────────────────────────────────────────────────

        private string _icono       = "🍽";
        private string _titulo      = "Título";
        private string _descripcion = "Descripción";
        private string _estadoTexto = "● Listo";
        private Color  _estadoColor = ColorConstants.AcentoExito;
        private Color  _colorAcento = ColorConstants.AcentoPrincipal;
        private string _numeracion  = "01";

        private bool  _isHovered  = false;
        private bool  _isPressed  = false;
        private float _hoverAlpha = 0f;

        private readonly System.Windows.Forms.Timer _animTimer;
        private const int Radio = 14;

        // ─── EVENTO PROPIO ────────────────────────────────────────────────────

        public event EventHandler? CardClicked;

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
        public string Numeracion
        {
            get => _numeracion;
            set { _numeracion = value; Invalidate(); }
        }

        // ─── CONSTRUCTOR ─────────────────────────────────────────────────────

        public SimulationCard()
        {
            // Sin ContainerControl — sin hijos — eventos directos
            SetStyle(
                ControlStyles.UserPaint            |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.DoubleBuffer         |
                ControlStyles.ResizeRedraw,
                true);

            BackColor   = ColorConstants.FondoCard;
            Cursor      = Cursors.Hand;
            MinimumSize = new Size(260, 200);
            Size        = new Size(340, 260);

            _animTimer = new System.Windows.Forms.Timer { Interval = 16 };
            _animTimer.Tick += (_, _) =>
            {
                float target = _isHovered ? 1f : 0f;
                float prev   = _hoverAlpha;
                _hoverAlpha  = _hoverAlpha < target
                    ? Math.Min(target, _hoverAlpha + 0.07f)
                    : Math.Max(target, _hoverAlpha - 0.07f);

                if (Math.Abs(_hoverAlpha - prev) > 0.004f)
                    Invalidate();
            };
            _animTimer.Start();
        }

        // ─── EVENTOS DE MOUSE ─────────────────────────────────────────────────

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left) return;
            _isPressed = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button != MouseButtons.Left) return;
            _isPressed = false;
            Invalidate();
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);
            CardClicked?.Invoke(this, EventArgs.Empty);
        }

        // ─── PINTADO COMPLETO GDI+ ────────────────────────────────────────────

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            // Limpiar con color del padre
            g.Clear(Parent?.BackColor ?? ColorConstants.FondoPrincipal);

            int m    = 6;
            var card = new Rectangle(m, m, Width - m * 2 - 1, Height - m * 2 - 1);
            if (card.Width <= 0 || card.Height <= 0) return;

            // ── Sombra ────────────────────────────────────────────────────
            int prof = (int)(2 + _hoverAlpha * 6);
            for (int i = prof; i >= 1; i--)
            {
                var sr = new Rectangle(
                    card.X + i, card.Y + i, card.Width, card.Height);
                int alpha = Math.Min(50, (int)(14.0 / i * (1 + _hoverAlpha)));
                using var sb = new SolidBrush(Color.FromArgb(alpha, 0, 0, 0));
                using var sp = CrearPath(sr, Radio);
                g.FillPath(sb, sp);
            }

            // ── Fondo ─────────────────────────────────────────────────────
            Color fA = Lerp(ColorConstants.FondoCard,
                            Color.FromArgb(44, 44, 60), _hoverAlpha);
            Color fB = Lerp(Color.FromArgb(26, 26, 38),
                            Color.FromArgb(32, 32, 50), _hoverAlpha);

            using var lgb = new LinearGradientBrush(
                card, fA, fB, LinearGradientMode.Vertical);
            using var cp  = CrearPath(card, Radio);
            g.FillPath(lgb, cp);

            // ── Barra de acento ───────────────────────────────────────────
            int bw = (int)(3 + _hoverAlpha * 2);
            g.FillRectangle(
                new SolidBrush(
                    Color.FromArgb((int)(160 + 95 * _hoverAlpha), _colorAcento)),
                new Rectangle(card.X, card.Y + Radio, bw, card.Height - Radio * 2));

            // ── Borde ─────────────────────────────────────────────────────
            using var bp2 = new Pen(
                Lerp(ColorConstants.BordeCard, _colorAcento, _hoverAlpha),
                _hoverAlpha > 0.5f ? 2f : 1f);
            g.DrawPath(bp2, cp);

            // ── Número (esquina superior derecha) ─────────────────────────
            using var numFont = new Font("Segoe UI", 20f, FontStyle.Bold);
            var numRect = new Rectangle(
                card.Right - 64, card.Y + 6, 58, 30);
            TextRenderer.DrawText(g, _numeracion, numFont, numRect,
                Color.FromArgb((int)(25 + 40 * _hoverAlpha), _colorAcento),
                TextFormatFlags.Right | TextFormatFlags.Top);

            // ── Icono ─────────────────────────────────────────────────────
            int iconoY = card.Y + 18;
            int iconoH = 72;
            using var iconFont = new Font("Segoe UI Emoji", 36f);
            var iconRect = new Rectangle(card.X, iconoY, card.Width, iconoH);
            TextRenderer.DrawText(g, _icono, iconFont, iconRect,
                ColorConstants.TextoPrincipal,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);

            // ── Título ────────────────────────────────────────────────────
            int tY = iconoY + iconoH + 4;
            using var titleFont = new Font("Segoe UI", 12f, FontStyle.Bold);
            var titleRect = new Rectangle(
                card.X + 12, tY, card.Width - 24, 28);
            TextRenderer.DrawText(g, _titulo, titleFont, titleRect,
                ColorConstants.TextoPrincipal,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.Top              |
                TextFormatFlags.EndEllipsis);

            // ── Descripción ───────────────────────────────────────────────
            int dY   = tY + 32;
            int dH   = card.Bottom - dY - 50;
            using var descFont = new Font("Segoe UI", 9.5f);
            var descRect = new Rectangle(
                card.X + 16, dY, card.Width - 32, Math.Max(dH, 40));
            TextRenderer.DrawText(g, _descripcion, descFont, descRect,
                ColorConstants.TextoSecundario,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.Top              |
                TextFormatFlags.WordBreak);

            // ── Divider ───────────────────────────────────────────────────
            int divY = card.Bottom - 42;
            using var divPen = new Pen(ColorConstants.Separador, 1);
            g.DrawLine(divPen,
                card.X + 20, divY,
                card.Right - 20, divY);

            // ── Badge de estado ───────────────────────────────────────────
            using var badgeFont = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            var bMed  = TextRenderer.MeasureText(_estadoTexto, badgeFont);
            int bW    = bMed.Width + 24;
            int bH    = 22;
            int bX    = card.X + (card.Width - bW) / 2;
            int bY    = card.Bottom - 36;
            var bRect = new Rectangle(bX, bY, bW, bH);

            using var bgBrush = new SolidBrush(
                Color.FromArgb(40, _estadoColor));
            using var bPath = CrearPath(bRect, 11);
            g.FillPath(bgBrush, bPath);

            using var bPen = new Pen(Color.FromArgb(80, _estadoColor), 1);
            g.DrawPath(bPen, bPath);

            TextRenderer.DrawText(g, _estadoTexto, badgeFont, bRect,
                _estadoColor,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter);

            // ── Efecto press ──────────────────────────────────────────────
            if (_isPressed)
            {
                using var pressBrush = new SolidBrush(
                    Color.FromArgb(20, 255, 255, 255));
                g.FillPath(pressBrush, cp);
            }
        }

        // ─── HELPERS ─────────────────────────────────────────────────────────

        private static GraphicsPath CrearPath(Rectangle rect, int radio)
        {
            var path = new GraphicsPath();
            int d    = Math.Min(radio * 2,
                       Math.Min(rect.Width, rect.Height));
            if (d <= 0) { path.AddRectangle(rect); return path; }

            path.AddArc(rect.X,          rect.Y,          d, d, 180, 90);
            path.AddArc(rect.Right - d,  rect.Y,          d, d, 270, 90);
            path.AddArc(rect.Right - d,  rect.Bottom - d, d, d,   0, 90);
            path.AddArc(rect.X,          rect.Bottom - d, d, d,  90, 90);
            path.CloseFigure();
            return path;
        }

        private static Color Lerp(Color c1, Color c2, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return Color.FromArgb(
                (int)(c1.A + (c2.A - c1.A) * t),
                (int)(c1.R + (c2.R - c1.R) * t),
                (int)(c1.G + (c2.G - c1.G) * t),
                (int)(c1.B + (c2.B - c1.B) * t));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _animTimer.Stop();
                _animTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}