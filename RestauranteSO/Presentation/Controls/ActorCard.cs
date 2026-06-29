using System.Drawing.Drawing2D;
using System.Drawing.Text;
using RestauranteSO.Constants;

namespace RestauranteSO.Presentation.Controls
{
    /// <summary>
    /// Tarjeta visual para representar un actor del sistema
    /// (cliente, cocinero, mesero, gerente). 100% GDI+.
    /// </summary>
    public sealed class ActorCard : Control
    {
        public enum EstadoActor
        {
            Libre, Activo, Esperando, Bloqueado, Atacado
        }

        private string      _icono        = "👤";
        private string      _nombre       = "Actor";
        private string      _estadoTexto  = "Libre";
        private EstadoActor _estado       = EstadoActor.Libre;
        private float       _progreso     = 0f;
        private float       _animOffset   = 0f;
        private Color       _colorAcento  = ColorConstants.AcentoPrincipal;

        private readonly System.Windows.Forms.Timer _timer;

        public ActorCard()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.DoubleBuffer |
                ControlStyles.ResizeRedraw, true);
            BackColor = ColorConstants.FondoCard;
            Size      = new Size(130, 110);

            _timer = new System.Windows.Forms.Timer { Interval = 32 };
            _timer.Tick += (_, _) =>
            {
                _animOffset += 0.06f;
                if (_animOffset > MathF.Tau) _animOffset -= MathF.Tau;
                Invalidate();
            };
            _timer.Start();
        }

        public void Actualizar(
            string icono, string nombre, string estadoTexto,
            EstadoActor estado, float progreso = 0f,
            Color? colorAcento = null)
        {
            _icono       = icono;
            _nombre      = nombre;
            _estadoTexto = estadoTexto;
            _estado      = estado;
            _progreso    = progreso;
            if (colorAcento.HasValue) _colorAcento = colorAcento.Value;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.Clear(Parent?.BackColor ?? ColorConstants.FondoLateral);

            int w = Width, h = Height;
            if (w < 20 || h < 20) return;

            Color colorEstado = _estado switch
            {
                EstadoActor.Activo    => ColorConstants.AcentoExito,
                EstadoActor.Esperando => ColorConstants.EstadoEsperando,
                EstadoActor.Bloqueado => ColorConstants.TextoHint,
                EstadoActor.Atacado   => ColorConstants.AlertaAtaque,
                _                    => _colorAcento
            };

            float pulse = (MathF.Sin(_animOffset * 2f) + 1f) / 2f;

            // Fondo card
            var cardRect = new Rectangle(2, 2, w - 4, h - 4);
            using var cardBrush = new SolidBrush(ColorConstants.FondoCard);
            using var cardPath  = CrearPath(cardRect, 12);
            g.FillPath(cardBrush, cardPath);

            // Borde
            float borderAlpha = _estado == EstadoActor.Libre ? 0.3f : 0.8f;
            using var borderPen = new Pen(
                Color.FromArgb((int)(255 * borderAlpha), colorEstado), 1.5f);
            g.DrawPath(borderPen, cardPath);

            // Halo si activo/atacado
            if (_estado == EstadoActor.Activo || _estado == EstadoActor.Atacado)
            {
                float hr = 6f + pulse * 5f;
                using var haloPen = new Pen(
                    Color.FromArgb((int)(40 + 30 * pulse), colorEstado), hr);
                g.DrawPath(haloPen, cardPath);
            }

            // Acento lateral
            using var acentoBrush = new SolidBrush(colorEstado);
            g.FillRectangle(acentoBrush,
                new Rectangle(2, 12, 3, h - 24));

            // Icono
            int iconoY = 8;
            int iconoH = (int)(h * 0.45f);
            using var fIcono = new Font("Segoe UI Emoji",
                Math.Max(iconoH * 0.5f, 10f));
            TextRenderer.DrawText(g, _icono, fIcono,
                new Rectangle(0, iconoY, w, iconoH),
                ColorConstants.TextoPrincipal,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter   |
                TextFormatFlags.NoPadding);

            // Nombre
            int nombreY = iconoY + iconoH + 2;
            using var fNombre = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            TextRenderer.DrawText(g, _nombre, fNombre,
                new Rectangle(4, nombreY, w - 8, 18),
                ColorConstants.TextoPrincipal,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.Top | TextFormatFlags.NoPadding |
                TextFormatFlags.EndEllipsis);

            // Estado
            int estadoY = nombreY + 18;
            using var fEstado = new Font("Segoe UI", 8f);
            TextRenderer.DrawText(g, _estadoTexto, fEstado,
                new Rectangle(4, estadoY, w - 8, 16),
                colorEstado,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.Top | TextFormatFlags.NoPadding |
                TextFormatFlags.EndEllipsis);

            // Barra de progreso
            if (_progreso > 0f)
            {
                int barY = h - 10;
                var barBg = new Rectangle(6, barY, w - 12, 5);
                using var bgBrush = new SolidBrush(ColorConstants.Separador);
                g.FillRectangle(bgBrush, barBg);
                int barW = (int)((w - 12) * _progreso);
                if (barW > 0)
                {
                    using var fillBrush = new SolidBrush(colorEstado);
                    g.FillRectangle(fillBrush,
                        new Rectangle(6, barY, barW, 5));
                }
            }
        }

        private static GraphicsPath CrearPath(Rectangle rect, int radio)
        {
            var path = new GraphicsPath();
            int d    = Math.Min(radio * 2, Math.Min(rect.Width, rect.Height));
            if (d <= 0) { path.AddRectangle(rect); return path; }
            path.AddArc(rect.X,         rect.Y,          d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y,          d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d,   0, 90);
            path.AddArc(rect.X,         rect.Bottom - d, d, d,  90, 90);
            path.CloseFigure();
            return path;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { _timer.Stop(); _timer.Dispose(); }
            base.Dispose(disposing);
        }
    }
}