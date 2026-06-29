using System.Drawing.Drawing2D;
using System.Drawing.Text;
using RestauranteSO.Constants;
using RestauranteSO.Presentation.Themes;

namespace RestauranteSO.Presentation.Controls
{
    public sealed class RestaurantMap : Control
    {
        private float _animOffset = 0f;
        private readonly System.Windows.Forms.Timer _animTimer;

        private record Nodo(string Icono, string Label, Color Color, float X, float Y);
        private record Conexion(int Desde, int Hasta, Color Color);

        private readonly Nodo[] _nodos =
        {
            new("👤", "Clientes",    ColorConstants.MapaCliente,  0.12f, 0.30f),
            new("📦", "Cola",        ColorConstants.MapaCola,     0.35f, 0.30f),
            new("👨‍🍳", "Cocina",    ColorConstants.MapaCocina,   0.58f, 0.30f),
            new("🧑‍🍽", "Meseros",  ColorConstants.MapaMesero,   0.35f, 0.68f),
            new("👔", "Gerente",     ColorConstants.MapaGerente,  0.58f, 0.68f),
            new("🔒", "Semáforos",   ColorConstants.AcentoPrincipal, 0.82f, 0.30f),
            new("📋", "Menú",        ColorConstants.TarjetaLectores, 0.82f, 0.68f),
        };

        private readonly Conexion[] _conexiones =
        {
            new(0, 1, ColorConstants.MapaCliente),
            new(1, 2, ColorConstants.MapaCocina),
            new(1, 5, ColorConstants.AcentoPrincipal),
            new(3, 6, ColorConstants.MapaMesero),
            new(4, 6, ColorConstants.MapaGerente),
            new(2, 5, ColorConstants.AcentoPrincipal),
        };

        public RestaurantMap()
        {
            SetStyle(
                ControlStyles.UserPaint            |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.DoubleBuffer         |
                ControlStyles.ResizeRedraw,
                true);

            BackColor = ColorConstants.MapaFondo;

            _animTimer = new System.Windows.Forms.Timer { Interval = 32 };
            _animTimer.Tick += (_, _) =>
            {
                _animOffset += 0.018f;
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
            g.Clear(ColorConstants.MapaFondo);

            if (Width < 40 || Height < 40) return;

            // ── Título ────────────────────────────────────────────────────────
            var tituloRect = new Rectangle(0, 8, Width, 22);
            TextRenderer.DrawText(g, "Arquitectura del Sistema — Vista Conceptual",
                AppTheme.FuenteSmallBold, tituloRect,
                ColorConstants.TextoHint,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.Top);

            // ── Conexiones animadas ────────────────────────────────────────────
            foreach (var con in _conexiones)
            {
                var desde = _nodos[con.Desde];
                var hasta = _nodos[con.Hasta];

                float x1 = desde.X * Width;
                float y1 = desde.Y * Height;
                float x2 = hasta.X * Width;
                float y2 = hasta.Y * Height;

                // Línea base
                using var penBase = new Pen(
                    Color.FromArgb(40, con.Color), 1.5f);
                g.DrawLine(penBase, x1, y1, x2, y2);

                // Partícula animada sobre la línea
                float t = (MathF.Sin(_animOffset + con.Desde * 0.8f) + 1f) / 2f;
                float px = x1 + (x2 - x1) * t;
                float py = y1 + (y2 - y1) * t;

                using var particBrush = new SolidBrush(
                    Color.FromArgb(200, con.Color));
                g.FillEllipse(particBrush, px - 3, py - 3, 6, 6);

                // Flecha
                DibujarFlecha(g, x1, y1, x2, y2,
                    Color.FromArgb(80, con.Color));
            }

            // ── Nodos ─────────────────────────────────────────────────────────
            float pulse = (MathF.Sin(_animOffset * 1.5f) + 1f) / 2f;

            for (int i = 0; i < _nodos.Length; i++)
            {
                var n  = _nodos[i];
                float cx = n.X * Width;
                float cy = n.Y * Height;
                float r  = Math.Min(Width, Height) * 0.065f;
                r = Math.Clamp(r, 28f, 52f);

                // Halo pulsante
                float haloR = r + 4f + pulse * 4f;
                using var haloBrush = new SolidBrush(
                    Color.FromArgb(25, n.Color));
                g.FillEllipse(haloBrush,
                    cx - haloR, cy - haloR,
                    haloR * 2, haloR * 2);

                // Fondo nodo
                var nodoRect = new RectangleF(cx - r, cy - r, r * 2, r * 2);
                using var fondoBrush = new SolidBrush(ColorConstants.MapaNodo);
                g.FillEllipse(fondoBrush, nodoRect);

                // Borde nodo
                using var bordePen = new Pen(
                    Color.FromArgb(180, n.Color), 1.5f);
                g.DrawEllipse(bordePen, nodoRect);

                // Icono
                float iconoSize = r * 0.9f;
                var iconoRect = new RectangleF(
                    cx - iconoSize / 2f,
                    cy - iconoSize / 2f - 4f,
                    iconoSize, iconoSize);
                TextRenderer.DrawText(g, n.Icono,
                    new Font("Segoe UI Emoji",
                        Math.Max(8f, r * 0.52f)),
                    Rectangle.Round(iconoRect),
                    ColorConstants.TextoPrincipal,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter);

                // Label debajo
                var labelRect = new RectangleF(
                    cx - r * 1.4f,
                    cy + r + 2f,
                    r * 2.8f, 16f);
                TextRenderer.DrawText(g, n.Label,
                    AppTheme.FuenteMapaNodo,
                    Rectangle.Round(labelRect),
                    n.Color,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.Top);
            }

            // ── Leyenda ───────────────────────────────────────────────────────
            DibujarLeyenda(g);
        }

        private void DibujarFlecha(
            Graphics g,
            float x1, float y1, float x2, float y2,
            Color color)
        {
            float dx  = x2 - x1;
            float dy  = y2 - y1;
            float len = MathF.Sqrt(dx * dx + dy * dy);
            if (len < 1f) return;

            float ux = dx / len;
            float uy = dy / len;

            // Punta de flecha a 60% del recorrido
            float mx = x1 + dx * 0.6f;
            float my = y1 + dy * 0.6f;
            float ah = 7f;
            float aw = 4f;

            var puntos = new PointF[]
            {
                new(mx + ux * ah,           my + uy * ah),
                new(mx - uy * aw - ux * 0f, my + ux * aw - uy * 0f),
                new(mx + uy * aw - ux * 0f, my - ux * aw - uy * 0f),
            };

            using var brush = new SolidBrush(color);
            g.FillPolygon(brush, puntos);
        }

        private void DibujarLeyenda(Graphics g)
        {
            var items = new (Color Color, string Texto)[]
            {
                (ColorConstants.MapaCliente,   "Productor"),
                (ColorConstants.MapaCocina,    "Consumidor"),
                (ColorConstants.MapaMesero,    "Lector"),
                (ColorConstants.MapaGerente,   "Escritor"),
                (ColorConstants.AcentoPrincipal,"Semáforo"),
            };

            int startX = 12;
            int startY = Height - 22;
            int stepX  = (Width - 24) / items.Length;

            for (int i = 0; i < items.Length; i++)
            {
                int x = startX + i * stepX;
                using var b = new SolidBrush(items[i].Color);
                g.FillEllipse(b, x, startY + 4, 8, 8);

                TextRenderer.DrawText(g, items[i].Texto,
                    new Font("Segoe UI", 7f),
                    new Rectangle(x + 11, startY, stepX - 14, 16),
                    ColorConstants.TextoHint,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            }
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