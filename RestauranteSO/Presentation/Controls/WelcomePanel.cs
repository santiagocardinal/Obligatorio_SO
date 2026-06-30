using System.Drawing.Drawing2D;
using System.Drawing.Text;
using RestauranteSO.Constants;
using RestauranteSO.Presentation.Themes;

namespace RestauranteSO.Presentation.Controls
{
    public sealed class WelcomePanel : Control
    {
        private float _animOffset = 0f;
        private readonly System.Windows.Forms.Timer _animTimer;

        private record Badge(string Texto, Color Color);

        private readonly Badge[] _badges =
        {
            new("SemaphoreSlim",          ColorConstants.TarjetaProductor),
            new("ReaderWriterLockSlim",   ColorConstants.TarjetaLectores),
            new("ConcurrentQueue",        ColorConstants.AcentoSecundario),
            new("CancellationToken",      ColorConstants.AcentoExito),
            new("Ingeniería Social",      ColorConstants.AlertaAtaque),
            new("Phishing Educativo",     ColorConstants.TarjetaAtaque2),
            new("Producer-Consumer",      ColorConstants.TarjetaProductor),
            new("Readers-Writers",        ColorConstants.TarjetaLectores),
            new(".NET 9  •  WinForms",    ColorConstants.TextoHint),
            new("Clean Architecture",     ColorConstants.AcentoPrincipal),
        };

        public WelcomePanel()
        {
            SetStyle(
                ControlStyles.UserPaint            |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.DoubleBuffer         |
                ControlStyles.ResizeRedraw,
                true);

            BackColor = ColorConstants.FondoWelcome;

            _animTimer = new System.Windows.Forms.Timer { Interval = 50 };
            _animTimer.Tick += (_, _) =>
            {
                _animOffset += 0.008f;
                if (_animOffset > MathF.Tau) _animOffset -= MathF.Tau;
                Invalidate();
            };
            _animTimer.Start();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.Clear(ColorConstants.FondoWelcome);

            if (Width < 40 || Height < 40) return;

            using var accentBrush = new LinearGradientBrush(
                new Rectangle(0, 0, Width, 2),
                ColorConstants.AcentoPrincipal,
                ColorConstants.TarjetaLectores,
                LinearGradientMode.Horizontal);
            g.FillRectangle(accentBrush, 0, 0, Width, 2);

            int pad     = 20;
            int col1W   = (int)(Width * 0.52f);
            int col2X   = col1W + pad;
            int col2W   = Width - col2X - pad;
            int yBase   = 14;

            TextRenderer.DrawText(g,
                "Bienvenido a RestaurantOS",
                AppTheme.FuenteSubtitulo,
                new Rectangle(pad, yBase, col1W - pad, 28),
                ColorConstants.TextoPrincipal,
                TextFormatFlags.Left | TextFormatFlags.Top);

            string desc =
                "Un entorno de simulación educativa que modela los " +
                "principales problemas de concurrencia en Sistemas Operativos, " +
                "ambientado en la operación diaria de un restaurante.";

            TextRenderer.DrawText(g, desc,
                AppTheme.FuenteSmall,
                new Rectangle(pad, yBase + 34, col1W - pad, 54),
                ColorConstants.TextoSecundario,
                TextFormatFlags.Left | TextFormatFlags.Top |
                TextFormatFlags.WordBreak);

            int sepY = yBase + 94;
            using var sepPen = new Pen(ColorConstants.Separador, 1);
            g.DrawLine(sepPen, pad, sepY, col1W, sepY);

            var infoItems = new (string Icono, string Texto)[]
            {
                ("🔄", "Productor-Consumidor con SemaphoreSlim"),
                ("📖", "Lectores-Escritores con ReaderWriterLockSlim"),
                ("⚡", "Inyección en cola — Ingeniería Social"),
                ("🎣", "Compromiso del menú — Phishing educativo"),
            };

            int infoY = sepY + 10;
            foreach (var item in infoItems)
            {
                TextRenderer.DrawText(g, item.Icono,
                    AppTheme.FuenteLabel,
                    new Rectangle(pad, infoY, 22, 18),
                    ColorConstants.TextoPrincipal,
                    TextFormatFlags.Left | TextFormatFlags.Top);

                TextRenderer.DrawText(g, item.Texto,
                    AppTheme.FuenteSmall,
                    new Rectangle(pad + 24, infoY, col1W - pad - 24, 18),
                    ColorConstants.TextoSecundario,
                    TextFormatFlags.Left | TextFormatFlags.Top);

                infoY += 20;
            }

            TextRenderer.DrawText(g,
                "Tecnologías y Conceptos",
                AppTheme.FuenteSmallBold,
                new Rectangle(col2X, yBase, col2W, 18),
                ColorConstants.TextoHint,
                TextFormatFlags.Left | TextFormatFlags.Top);

            DibujarBadges(g, col2X, yBase + 24, col2W);

            int navY = yBase + 24 + CalcularAlturaBadges(col2W) + 12;
            DibujarNavegacion(g, col2X, navY, col2W);

            using var borderPen = new Pen(ColorConstants.Separador, 1);
            g.DrawLine(borderPen, 0, Height - 1, Width, Height - 1);
        }

        private void DibujarBadges(Graphics g, int startX, int startY, int maxW)
        {
            int x    = startX;
            int y    = startY;
            int padH = 6;
            int gap  = 6;
            int lineH = 24;

            foreach (var badge in _badges)
            {
                var medida = TextRenderer.MeasureText(badge.Texto,
                    AppTheme.FuenteSmallBold);
                int bW = medida.Width + padH * 2;
                int bH = lineH;

                if (x + bW > startX + maxW)
                {
                    x  = startX;
                    y += bH + gap;
                }

                if (y + bH > Height - 40) break;

                var bRect = new Rectangle(x, y, bW, bH);

                using var fbBrush = new SolidBrush(
                    Color.FromArgb(35, badge.Color));
                using var path = AppTheme.CrearPathRedondeado(bRect, 8);
                g.FillPath(fbBrush, path);

                using var borderPen = new Pen(
                    Color.FromArgb(70, badge.Color), 1f);
                g.DrawPath(borderPen, path);

                TextRenderer.DrawText(g, badge.Texto,
                    AppTheme.FuenteSmallBold,
                    bRect,
                    badge.Color,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter);

                x += bW + gap;
            }
        }

        private int CalcularAlturaBadges(int maxW)
        {
            int x      = 0;
            int y      = 0;
            int gap    = 6;
            int lineH  = 24;

            foreach (var badge in _badges)
            {
                var medida = TextRenderer.MeasureText(badge.Texto,
                    AppTheme.FuenteSmallBold);
                int bW = medida.Width + 12;

                if (x + bW > maxW)
                {
                    x  = 0;
                    y += lineH + gap;
                }
                x += bW + gap;
            }
            return y + lineH + gap;
        }

        private void DibujarNavegacion(
            Graphics g, int x, int y, int w)
        {
            if (y + 36 > Height - 8) return;

            var navRect = new Rectangle(x, y, w, 36);
            using var navBrush = new SolidBrush(ColorConstants.FondoPanel);
            using var navPath  = AppTheme.CrearPathRedondeado(navRect, 8);
            g.FillPath(navBrush, navPath);

            using var navBorder = new Pen(ColorConstants.Separador, 1f);
            g.DrawPath(navBorder, navPath);

            TextRenderer.DrawText(g,
                "💡  Usá el Dock inferior para navegar entre módulos",
                AppTheme.FuenteSmall,
                new Rectangle(x + 10, y, w - 10, 36),
                ColorConstants.TextoSecundario,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
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