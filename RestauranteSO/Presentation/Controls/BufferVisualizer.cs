using System.Drawing.Drawing2D;
using System.Drawing.Text;
using RestauranteSO.Constants;

namespace RestauranteSO.Presentation.Controls
{
    /// <summary>
    /// Visualiza el buffer/cola del Productor-Consumidor como
    /// una grilla de celdas que se llenan y vacían con animación.
    /// </summary>
    public sealed class BufferVisualizer : Control
    {
        private int   _ocupacion    = 0;
        private int   _capacidad    = 15;
        private int   _maliciosos   = 0;
        private bool  _ataque       = false;
        private float _animOffset   = 0f;

        private readonly System.Windows.Forms.Timer _timer;

        public BufferVisualizer()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.DoubleBuffer |
                ControlStyles.ResizeRedraw, true);
            BackColor = ColorConstants.FondoPanel;

            _timer = new System.Windows.Forms.Timer { Interval = 32 };
            _timer.Tick += (_, _) =>
            {
                _animOffset += 0.05f;
                if (_animOffset > MathF.Tau) _animOffset -= MathF.Tau;
                Invalidate();
            };
            _timer.Start();
        }

        public void Actualizar(int ocupacion, int capacidad,
            int maliciosos, bool ataqueActivo)
        {
            _ocupacion  = Math.Clamp(ocupacion,  0, capacidad);
            _capacidad  = Math.Max(capacidad, 1);
            _maliciosos = Math.Clamp(maliciosos, 0, ocupacion);
            _ataque     = ataqueActivo;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.Clear(BackColor);

            int w = Width, h = Height;
            if (w < 40 || h < 40) return;

            // ── Título ────────────────────────────────────────────────────────
            using var fTit = new Font("Segoe UI", 11f, FontStyle.Bold);
            TextRenderer.DrawText(g, "📦 BUFFER — Cola de Pedidos", fTit,
                new Rectangle(0, 4, w, 24),
                ColorConstants.TextoPrincipal,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);

            // ── Grilla de celdas ──────────────────────────────────────────────
            int cols     = Math.Min(_capacidad, 15);
            int rows     = (int)Math.Ceiling(_capacidad / (double)cols);
            int cellSize = Math.Min((w - 20) / cols, 28);
            cellSize = Math.Max(cellSize, 14);
            int gridW = cols * cellSize;
            int startX = (w - gridW) / 2;
            int startY = 36;

            for (int i = 0; i < _capacidad; i++)
            {
                int col = i % cols;
                int row = i / cols;
                int cx  = startX + col * cellSize + 2;
                int cy  = startY + row * (cellSize + 4) + 2;
                int cs  = cellSize - 4;

                var cellRect = new Rectangle(cx, cy, cs, cs);

                bool llena     = i < _ocupacion;
                bool maliciosa = llena && i < _maliciosos;

                Color colorCelda = maliciosa
                    ? ColorConstants.AlertaAtaque
                    : llena
                        ? ColorConstants.AcentoExito
                        : ColorConstants.Separador;

                // Pulso en celdas maliciosas
                if (maliciosa)
                {
                    float p = (MathF.Sin(_animOffset * 3f + i * 0.5f) + 1f) / 2f;
                    colorCelda = Color.FromArgb(
                        (int)(180 + 75 * p),
                        colorCelda.R, colorCelda.G, colorCelda.B);
                }

                using var cellBrush = new SolidBrush(
                    Color.FromArgb(llena ? 200 : 60, colorCelda));
                using var cellPath  =
                    AppTheme.CrearPathRedondeado(cellRect, 4);
                g.FillPath(cellBrush, cellPath);

                if (llena)
                {
                    using var cellBorder = new Pen(colorCelda, 1f);
                    g.DrawPath(cellBorder, cellPath);
                }

                // Ícono en celdas llenas
                if (llena && cs >= 14)
                {
                    using var fIco = new Font("Segoe UI Emoji",
                        Math.Max(cs * 0.45f, 7f));
                    TextRenderer.DrawText(g,
                        maliciosa ? "⚡" : "📋",
                        fIco, cellRect,
                        Color.White,
                        TextFormatFlags.HorizontalCenter |
                        TextFormatFlags.VerticalCenter   |
                        TextFormatFlags.NoPadding);
                }
            }

            // ── Stats ─────────────────────────────────────────────────────────
            int statsY = startY + rows * (cellSize + 4) + 12;

            // Barra de ocupación
            var barBg = new Rectangle(20, statsY, w - 40, 12);
            using var barBgBrush = new SolidBrush(ColorConstants.Separador);
            using var barBgPath  = AppTheme.CrearPathRedondeado(barBg, 6);
            g.FillPath(barBgBrush, barBgPath);

            if (_ocupacion > 0)
            {
                int barW = (int)((w - 40) * ((float)_ocupacion / _capacidad));
                var barFill = new Rectangle(20, statsY, barW, 12);
                Color barColor = _ataque
                    ? ColorConstants.AlertaAtaque
                    : _ocupacion > _capacidad * 0.8f
                        ? ColorConstants.Advertencia
                        : ColorConstants.AcentoExito;
                using var barFillBrush = new SolidBrush(barColor);
                using var barFillPath  = AppTheme.CrearPathRedondeado(barFill, 6);
                g.FillPath(barFillBrush, barFillPath);
            }

            statsY += 18;

            // Contador
            using var fStats = new Font("Segoe UI", 10f, FontStyle.Bold);
            string stats = $"Ocupación: {_ocupacion}/{_capacidad}";
            if (_ataque && _maliciosos > 0)
                stats += $"  |  ⚡ Maliciosos: {_maliciosos}";
            TextRenderer.DrawText(g, stats, fStats,
                new Rectangle(0, statsY, w, 20),
                _ataque ? ColorConstants.AlertaAtaque : ColorConstants.TextoSecundario,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);

            statsY += 22;

            // Estado semáforo
            string semEstado = _ocupacion == 0
                ? "⚪ Buffer vacío — Cocineros esperando"
                : _ocupacion >= _capacidad
                    ? "🔴 Buffer LLENO — Clientes bloqueados"
                    : _ataque
                        ? "🟠 Buffer bajo ATAQUE — Pedidos contaminados"
                        : "🟢 Buffer operando normalmente";

            using var fSem = new Font("Segoe UI", 9f);
            TextRenderer.DrawText(g, semEstado, fSem,
                new Rectangle(4, statsY, w - 8, 18),
                _ataque ? ColorConstants.AlertaAtaque
                        : _ocupacion == 0 ? ColorConstants.TextoHint
                        : ColorConstants.AcentoExito,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);

            // Glow de ataque
            if (_ataque)
            {
                float pulse = (MathF.Sin(_animOffset * 2f) + 1f) / 2f;
                using var glowPen = new Pen(
                    Color.FromArgb((int)(40 + 40 * pulse),
                        ColorConstants.AlertaAtaque), 3f);
                g.DrawRectangle(glowPen, 1, 1, w - 3, h - 3);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { _timer.Stop(); _timer.Dispose(); }
            base.Dispose(disposing);
        }
    }

    // Referencia a AppTheme para evitar using circular
    file static class AppTheme
    {
        public static System.Drawing.Drawing2D.GraphicsPath
            CrearPathRedondeado(Rectangle rect, int radio)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            int d    = Math.Min(radio * 2, Math.Min(rect.Width, rect.Height));
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