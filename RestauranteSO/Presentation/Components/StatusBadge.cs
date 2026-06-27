using System.ComponentModel;
using RestauranteSO.Constants;

namespace RestauranteSO.Presentation.Components
{
    public sealed class StatusBadge : Control
    {
        private Color _colorAcento = ColorConstants.AcentoExito;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color ColorAcento
        {
            get => _colorAcento;
            set { _colorAcento = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string Text
        {
            get => base.Text ?? string.Empty;
            set { base.Text = value; Invalidate(); }
        }

        public StatusBadge()
        {
            SetStyle(
                ControlStyles.UserPaint              |
                ControlStyles.AllPaintingInWmPaint   |
                ControlStyles.DoubleBuffer           |
                ControlStyles.ResizeRedraw           |
                ControlStyles.SupportsTransparentBackColor,
                true);

            // No usar Transparent: usar el mismo color del padre
            BackColor = ColorConstants.FondoLateral;
            Height    = 24;
            Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            ForeColor = ColorConstants.TextoPrincipal;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode =
                System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint =
                System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var rect = new Rectangle(0, 0, Width - 1, Height - 1);

            // Fondo: pintar el color del padre primero (evita el error Transparent)
            using (var fondoPadre = new SolidBrush(
                Parent?.BackColor ?? ColorConstants.FondoLateral))
                g.FillRectangle(fondoPadre, rect);

            // Pastilla con borde redondeado
            using (var path = CrearPath(rect, 10))
            {
                using var fb = new SolidBrush(
                    Color.FromArgb(50, _colorAcento));
                g.FillPath(fb, path);

                using var bp = new Pen(
                    Color.FromArgb(120, _colorAcento), 1);
                g.DrawPath(bp, path);
            }

            // Texto
            if (!string.IsNullOrEmpty(Text))
            {
                TextRenderer.DrawText(g, Text, Font, rect, _colorAcento,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter   |
                    TextFormatFlags.SingleLine);
            }
        }

        // Cuando el padre cambia, actualizar el BackColor para que coincida
        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            if (Parent != null)
                BackColor = Parent.BackColor;
        }

        // Cuando el padre se repinta, repintarse también
        protected override void OnParentBackColorChanged(EventArgs e)
        {
            base.OnParentBackColorChanged(e);
            if (Parent != null)
                BackColor = Parent.BackColor;
            Invalidate();
        }

        private static System.Drawing.Drawing2D.GraphicsPath
            CrearPath(Rectangle rect, int radio)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            int d    = Math.Min(radio * 2,
                       Math.Min(rect.Width, rect.Height));

            if (d <= 0) { path.AddRectangle(rect); return path; }

            path.AddArc(rect.X,         rect.Y,          d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y,          d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d,   0, 90);
            path.AddArc(rect.X,         rect.Bottom - d, d, d,  90, 90);
            path.CloseFigure();
            return path;
        }
    }
}