// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Presentation/Controls/SemaphoreVisualizer.cs
// Propósito: Visualización gráfica del estado de los semáforos y del
//            ReaderWriterLockSlim. Muestra barras de nivel y textos.
// SOLID    : SRP - solo visualiza el estado de primitivas de sincronización.
// =============================================================================

using RestauranteSO.Constants;
using RestauranteSO.Presentation.Themes;

namespace RestauranteSO.Presentation.Controls
{
    /// <summary>
    /// Visualizador de primitivas de sincronización.
    ///
    /// Puede mostrar:
    /// - Estado de SemaphoreSlim (barra de nivel, valor actual/máximo)
    /// - Estado de ReaderWriterLockSlim (lectores activos, escritor, colas)
    /// - Cola de pedidos (cantidad actual / máxima)
    ///
    /// Se actualiza desde el timer de la UI llamando a Actualizar*().
    /// </summary>
    public sealed class SemaphoreVisualizer : UserControl
    {
        // ─── MODO ────────────────────────────────────────────────────────────

        public enum ModoVisualizacion
        {
            Semaforo,
            ReaderWriterLock,
            Cola
        }

        private ModoVisualizacion _modo = ModoVisualizacion.Semaforo;

        // ─── DATOS ACTUALES ──────────────────────────────────────────────────

        // Semáforo
        private int _semValorActual  = 0;
        private int _semValorMaximo  = 10;
        private string _semNombre    = "Semáforo";

        // ReaderWriterLock
        private int _rwLectoresActivos  = 0;
        private int _rwLectoresEsperando = 0;
        private bool _rwEscritorActivo  = false;
        private bool _rwEscritorEsperando = false;

        // Cola
        private int _colaActual  = 0;
        private int _colaMaximo  = 15;

        // ─── CONSTRUCTOR ─────────────────────────────────────────────────────

        public SemaphoreVisualizer()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.DoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = ColorConstants.FondoPanel;
            MinimumSize = new Size(180, 80);
        }

        // ─── API PÚBLICA ─────────────────────────────────────────────────────

        public void ActualizarSemaforo(string nombre, int actual, int maximo)
        {
            _modo           = ModoVisualizacion.Semaforo;
            _semNombre      = nombre;
            _semValorActual = actual;
            _semValorMaximo = Math.Max(1, maximo);
            Invalidate();
        }

        public void ActualizarReaderWriterLock(
            int lectoresActivos,
            int lectoresEsperando,
            bool escritorActivo,
            bool escritorEsperando)
        {
            _modo                  = ModoVisualizacion.ReaderWriterLock;
            _rwLectoresActivos     = lectoresActivos;
            _rwLectoresEsperando   = lectoresEsperando;
            _rwEscritorActivo      = escritorActivo;
            _rwEscritorEsperando   = escritorEsperando;
            Invalidate();
        }

        public void ActualizarCola(int actual, int maximo)
        {
            _modo        = ModoVisualizacion.Cola;
            _colaActual  = actual;
            _colaMaximo  = Math.Max(1, maximo);
            Invalidate();
        }

        // ─── PINTADO ─────────────────────────────────────────────────────────

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode =
                System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.TextRenderingHint =
                System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            switch (_modo)
            {
                case ModoVisualizacion.Semaforo:
                    PintarSemaforo(g);
                    break;
                case ModoVisualizacion.ReaderWriterLock:
                    PintarRWLock(g);
                    break;
                case ModoVisualizacion.Cola:
                    PintarCola(g);
                    break;
            }
        }

        private void PintarSemaforo(Graphics g)
        {
            float porcentaje = _semValorActual / (float)_semValorMaximo;
            Color colorBarra = porcentaje > 0.6f
                ? ColorConstants.AcentoExito
                : porcentaje > 0.3f
                    ? ColorConstants.EstadoEsperando
                    : ColorConstants.AlertaAtaque;

            // Título
            using var fontTitulo = new Font("Segoe UI", 8f, FontStyle.Bold);
            g.DrawString(_semNombre, fontTitulo,
                new SolidBrush(ColorConstants.TextoSecundario),
                new RectangleF(8, 6, Width - 16, 16));

            // Valor numérico
            string valor = $"{_semValorActual} / {_semValorMaximo}";
            using var fontValor = new Font("Consolas", 10f, FontStyle.Bold);
            var tamValor = g.MeasureString(valor, fontValor);
            g.DrawString(valor, fontValor,
                new SolidBrush(colorBarra),
                Width - tamValor.Width - 8, 4);

            // Barra de progreso
            int barraY   = 28;
            int barraH   = 12;
            int barraAncho = Width - 16;

            // Fondo de la barra
            using var fondoBrush = new SolidBrush(ColorConstants.Separador);
            g.FillRoundedRectangle(fondoBrush,
                new RectangleF(8, barraY, barraAncho, barraH), 6);

            // Relleno de la barra
            if (_semValorActual > 0)
            {
                int rellenoAncho = (int)(barraAncho * porcentaje);
                using var rellenoBrush = new SolidBrush(colorBarra);
                g.FillRoundedRectangle(rellenoBrush,
                    new RectangleF(8, barraY, rellenoAncho, barraH), 6);
            }

            // Descripción
            using var fontDesc = new Font("Segoe UI", 7.5f, FontStyle.Regular);
            string desc = _semValorActual == 0
                ? "⛔ Bloqueado (productores esperando)"
                : _semValorActual == _semValorMaximo
                    ? "✅ Libre (cola vacía)"
                    : $"🔄 {_semValorActual} posiciones disponibles";

            g.DrawString(desc, fontDesc,
                new SolidBrush(ColorConstants.TextoHint),
                new RectangleF(8, 46, Width - 16, 28));
        }

        private void PintarRWLock(Graphics g)
        {
            using var fontTitulo = new Font("Segoe UI", 8f, FontStyle.Bold);
            g.DrawString("ReaderWriterLockSlim", fontTitulo,
                new SolidBrush(ColorConstants.TextoSecundario),
                new RectangleF(8, 4, Width - 16, 16));

            int y = 24;
            using var fontItem = new Font("Segoe UI", 8.5f, FontStyle.Regular);

            // Lectores activos
            Color colorLect = _rwLectoresActivos > 0
                ? ColorConstants.AcentoPrincipal
                : ColorConstants.TextoHint;
            DibujarFila(g, fontItem, "📖 Lectores activos:",
                _rwLectoresActivos.ToString(), colorLect, y);
            y += 18;

            // Lectores esperando
            Color colorLectEsp = _rwLectoresEsperando > 0
                ? ColorConstants.EstadoEsperando
                : ColorConstants.TextoHint;
            DibujarFila(g, fontItem, "⏳ Lectores esperando:",
                _rwLectoresEsperando.ToString(), colorLectEsp, y);
            y += 18;

            // Escritor activo
            Color colorEsc = _rwEscritorActivo
                ? ColorConstants.AlertaAtaque
                : ColorConstants.TextoHint;
            DibujarFila(g, fontItem, "✏ Escritor activo:",
                _rwEscritorActivo ? "SÍ (EXCLUSIVO)" : "No", colorEsc, y);
            y += 18;

            // Escritor esperando
            Color colorEscEsp = _rwEscritorEsperando
                ? ColorConstants.AcentoSecundario
                : ColorConstants.TextoHint;
            DibujarFila(g, fontItem, "⏳ Escritor esperando:",
                _rwEscritorEsperando ? "Sí" : "No", colorEscEsp, y);
        }

        private void PintarCola(Graphics g)
        {
            float pct = _colaActual / (float)_colaMaximo;
            Color color = pct > 0.8f
                ? ColorConstants.AlertaAtaque
                : pct > 0.5f
                    ? ColorConstants.EstadoEsperando
                    : ColorConstants.AcentoPrincipal;

            using var fontTit = new Font("Segoe UI", 8f, FontStyle.Bold);
            g.DrawString("Cola de Pedidos", fontTit,
                new SolidBrush(ColorConstants.TextoSecundario),
                new RectangleF(8, 4, Width - 16, 16));

            // Valor
            string val = $"{_colaActual} / {_colaMaximo}";
            using var fontVal = new Font("Consolas", 10f, FontStyle.Bold);
            var sz = g.MeasureString(val, fontVal);
            g.DrawString(val, fontVal, new SolidBrush(color),
                Width - sz.Width - 8, 2);

            // Barra
            int barraAncho = Width - 16;
            using var fondoBrush = new SolidBrush(ColorConstants.Separador);
            g.FillRoundedRectangle(fondoBrush,
                new RectangleF(8, 26, barraAncho, 14), 7);

            if (_colaActual > 0)
            {
                int relleno = (int)(barraAncho * pct);
                using var rellenoBrush = new SolidBrush(color);
                g.FillRoundedRectangle(rellenoBrush,
                    new RectangleF(8, 26, relleno, 14), 7);
            }

            // Estado
            string estado = pct >= 1f
                ? "🔴 COLA LLENA - Productores bloqueados"
                : pct == 0f
                    ? "🟡 Cola vacía - Consumidores esperando"
                    : $"🟢 {_colaActual} pedidos en espera";

            using var fontEst = new Font("Segoe UI", 7.5f, FontStyle.Regular);
            g.DrawString(estado, fontEst,
                new SolidBrush(ColorConstants.TextoHint),
                new RectangleF(8, 46, Width - 16, 28));
        }

        private void DibujarFila(
            Graphics g, Font fuente,
            string etiqueta, string valor,
            Color colorValor, int y)
        {
            g.DrawString(etiqueta, fuente,
                new SolidBrush(ColorConstants.TextoSecundario),
                new RectangleF(8, y, Width * 0.65f, 16));

            using var boldFont = new Font(fuente, FontStyle.Bold);
            g.DrawString(valor, boldFont,
                new SolidBrush(colorValor),
                new RectangleF(Width * 0.65f, y, Width * 0.35f - 8, 16));
        }
    }

    // ─── EXTENSIÓN GDI+ ──────────────────────────────────────────────────────

    /// <summary>
    /// Extensiones para Graphics que agregan soporte de rectángulos redondeados.
    /// Necesario porque Graphics.FillRoundedRectangle no existe de forma nativa.
    /// </summary>
    internal static class GraphicsExtensions
    {
        public static void FillRoundedRectangle(
            this Graphics g, Brush brush,
            RectangleF rect, float radio)
        {
            if (rect.Width <= 0 || rect.Height <= 0) return;

            float d = radio * 2;
            using var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(rect.X, rect.Y, Math.Min(d, rect.Width), Math.Min(d, rect.Height), 180, 90);
            path.AddArc(rect.Right - Math.Min(d, rect.Width), rect.Y,
                        Math.Min(d, rect.Width), Math.Min(d, rect.Height), 270, 90);
            path.AddArc(rect.Right - Math.Min(d, rect.Width),
                        rect.Bottom - Math.Min(d, rect.Height),
                        Math.Min(d, rect.Width), Math.Min(d, rect.Height), 0, 90);
            path.AddArc(rect.X, rect.Bottom - Math.Min(d, rect.Height),
                        Math.Min(d, rect.Width), Math.Min(d, rect.Height), 90, 90);
            path.CloseFigure();
            g.FillPath(brush, path);
        }
    }
}