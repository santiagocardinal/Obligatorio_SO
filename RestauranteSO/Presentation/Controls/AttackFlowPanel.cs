using System.Drawing.Drawing2D;
using System.Drawing.Text;
using RestauranteSO.Constants;

namespace RestauranteSO.Presentation.Controls
{
    /// <summary>
    /// Panel de flujo visual para los módulos de ataque.
    /// Pinta nodos, flechas animadas y partículas sin controles hijos.
    /// </summary>
    public sealed class AttackFlowPanel : Control
    {
        public enum NodoTipo { Productor, Buffer, Consumidor, Lector, RecursoCompartido, Escritor }
        public enum NodoEstado { Libre, Activo, Esperando, Atacado, Bloqueado }

        public record NodoInfo(
            string Id,
            string Icono,
            string Nombre,
            string Estado,
            NodoTipo Tipo,
            NodoEstado EstadoEnum,
            float ProgresoNorm = 0f);

        private readonly List<NodoInfo>    _nodos       = new();
        private readonly List<(int, int)>  _conexiones  = new();
        private readonly List<Particula>   _particulas  = new();
        private readonly Random            _rng         = new();
        private float _animOffset = 0f;

        private readonly System.Windows.Forms.Timer _timer;

        private record Particula(
            float X, float Y,
            float Vx, float Vy,
            Color Color,
            float Vida,
            float VidaMax);

        public AttackFlowPanel()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.DoubleBuffer |
                ControlStyles.ResizeRedraw, true);
            BackColor = ColorConstants.FondoPrincipal;

            _timer = new System.Windows.Forms.Timer { Interval = 32 };
            _timer.Tick += (_, _) =>
            {
                _animOffset += 0.04f;
                if (_animOffset > MathF.Tau) _animOffset -= MathF.Tau;
                ActualizarParticulas();
                Invalidate();
            };
            _timer.Start();
        }

        public void SetNodos(List<NodoInfo> nodos, List<(int, int)> conexiones)
        {
            _nodos.Clear();
            _nodos.AddRange(nodos);
            _conexiones.Clear();
            _conexiones.AddRange(conexiones);
            Invalidate();
        }

        public void AgregarParticulas(int desde, int hasta, Color color, int cantidad = 3)
        {
            if (desde < 0 || hasta >= _nodos.Count || desde >= _nodos.Count) return;
            var pDesde = ObtenerCentroNodo(desde);
            var pHasta = ObtenerCentroNodo(hasta);
            for (int i = 0; i < cantidad; i++)
            {
                float t  = (float)_rng.NextDouble();
                float px = pDesde.X + (pHasta.X - pDesde.X) * t;
                float py = pDesde.Y + (pHasta.Y - pDesde.Y) * t;
                float vx = (pHasta.X - pDesde.X) * 0.012f;
                float vy = (pHasta.Y - pDesde.Y) * 0.012f;
                _particulas.Add(new Particula(px, py, vx, vy, color, 1f, 1f));
            }
        }

        private void ActualizarParticulas()
        {
            for (int i = _particulas.Count - 1; i >= 0; i--)
            {
                var p = _particulas[i];
                float vida = p.Vida - 0.03f;
                if (vida <= 0) { _particulas.RemoveAt(i); continue; }
                _particulas[i] = p with
                {
                    X    = p.X + p.Vx,
                    Y    = p.Y + p.Vy,
                    Vida = vida
                };
            }
        }

        private PointF ObtenerCentroNodo(int idx)
        {
            if (_nodos.Count == 0 || idx < 0 || idx >= _nodos.Count)
                return new PointF(Width / 2f, Height / 2f);

            int cols   = Math.Max(1, _nodos.Count);
            float colW = Width / (float)cols;
            float cx   = colW * idx + colW / 2f;
            float cy   = Height / 2f;
            return new PointF(cx, cy);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.Clear(BackColor);

            if (_nodos.Count == 0) return;

            // Calcular posiciones
            var posiciones = new PointF[_nodos.Count];
            float colW = Width / (float)_nodos.Count;
            for (int i = 0; i < _nodos.Count; i++)
                posiciones[i] = new PointF(
                    colW * i + colW / 2f,
                    Height / 2f);

            // Dibujar conexiones
            foreach (var (desde, hasta) in _conexiones)
            {
                if (desde < 0 || hasta >= _nodos.Count) continue;
                var p1 = posiciones[desde];
                var p2 = posiciones[hasta];
                DibujarConexion(g, p1, p2, _nodos[desde], _nodos[hasta]);
            }

            // Dibujar nodos
            for (int i = 0; i < _nodos.Count; i++)
                DibujarNodo(g, posiciones[i], _nodos[i], i);

            // Dibujar partículas
            foreach (var p in _particulas)
            {
                int alpha = (int)(p.Vida / p.VidaMax * 220);
                using var b = new SolidBrush(Color.FromArgb(alpha, p.Color));
                g.FillEllipse(b, p.X - 5, p.Y - 5, 10, 10);
            }
        }

        private void DibujarConexion(
            Graphics g, PointF p1, PointF p2,
            NodoInfo n1, NodoInfo n2)
        {
            Color color = (n1.EstadoEnum == NodoEstado.Atacado ||
                           n2.EstadoEnum == NodoEstado.Atacado)
                ? ColorConstants.AlertaAtaque
                : Color.FromArgb(60, ColorConstants.AcentoPrincipal);

            // Línea animada con dash
            float pulse = (MathF.Sin(_animOffset * 2f) + 1f) / 2f;
            using var pen = new Pen(color, 2f);
            pen.DashStyle    = DashStyle.Dash;
            pen.DashOffset   = _animOffset * 8f;
            g.DrawLine(pen, p1, p2);

            // Flecha
            DibujarFlecha(g, p1, p2, color);
        }

        private static void DibujarFlecha(
            Graphics g, PointF p1, PointF p2, Color color)
        {
            float mx  = (p1.X + p2.X) / 2f;
            float my  = (p1.Y + p2.Y) / 2f;
            float dx  = p2.X - p1.X;
            float dy  = p2.Y - p1.Y;
            float len = MathF.Sqrt(dx * dx + dy * dy);
            if (len < 1f) return;
            float ux = dx / len, uy = dy / len;

            var puntos = new PointF[]
            {
                new(mx + ux * 10, my + uy * 10),
                new(mx - uy * 5,  my + ux * 5),
                new(mx + uy * 5,  my - ux * 5)
            };
            using var b = new SolidBrush(color);
            g.FillPolygon(b, puntos);
        }

        private void DibujarNodo(
            Graphics g, PointF centro, NodoInfo nodo, int idx)
        {
            float radio = Math.Min(Width / _nodos.Count / 2.4f, 70f);
            radio = Math.Max(radio, 36f);

            Color colorBase = nodo.EstadoEnum switch
            {
                NodoEstado.Activo    => ColorConstants.AcentoExito,
                NodoEstado.Esperando => ColorConstants.EstadoEsperando,
                NodoEstado.Atacado   => ColorConstants.AlertaAtaque,
                NodoEstado.Bloqueado => ColorConstants.TextoHint,
                _                   => ColorConstants.AcentoPrincipal
            };

            // Pulso
            float pulse = (MathF.Sin(_animOffset * 2f + idx * 0.8f) + 1f) / 2f;

            // Halo
            if (nodo.EstadoEnum != NodoEstado.Libre)
            {
                float hr = radio + 8f + pulse * 8f;
                using var haloBrush = new SolidBrush(
                    Color.FromArgb(30, colorBase));
                g.FillEllipse(haloBrush,
                    centro.X - hr, centro.Y - hr,
                    hr * 2, hr * 2);
            }

            // Círculo fondo
            var rect = new RectangleF(
                centro.X - radio, centro.Y - radio,
                radio * 2, radio * 2);
            using var fondoBrush = new SolidBrush(ColorConstants.FondoCard);
            g.FillEllipse(fondoBrush, rect);

            // Borde
            using var borderPen = new Pen(colorBase, 2.5f);
            g.DrawEllipse(borderPen, rect);

            // Icono
            float iconoSize = radio * 0.72f;
            using var fIcono = new Font("Segoe UI Emoji",
                Math.Max(iconoSize * 0.55f, 10f));
            TextRenderer.DrawText(g, nodo.Icono, fIcono,
                new Rectangle(
                    (int)(centro.X - radio),
                    (int)(centro.Y - radio - 8),
                    (int)(radio * 2), (int)(radio * 2)),
                ColorConstants.TextoPrincipal,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter   |
                TextFormatFlags.NoPadding);

            // Nombre
            using var fNombre = new Font("Segoe UI", 9f, FontStyle.Bold);
            TextRenderer.DrawText(g, nodo.Nombre, fNombre,
                new Rectangle(
                    (int)(centro.X - radio * 1.3f),
                    (int)(centro.Y + radio + 4),
                    (int)(radio * 2.6f), 20),
                colorBase,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.Top | TextFormatFlags.NoPadding);

            // Estado
            using var fEstado = new Font("Segoe UI", 8f);
            TextRenderer.DrawText(g, nodo.Estado, fEstado,
                new Rectangle(
                    (int)(centro.X - radio * 1.3f),
                    (int)(centro.Y + radio + 22),
                    (int)(radio * 2.6f), 18),
                ColorConstants.TextoSecundario,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.Top | TextFormatFlags.NoPadding);

            // Barra de progreso si aplica
            if (nodo.ProgresoNorm > 0f)
            {
                var barRect = new RectangleF(
                    centro.X - radio * 0.8f,
                    centro.Y + radio + 42,
                    radio * 1.6f, 5f);
                using var barFondo = new SolidBrush(ColorConstants.Separador);
                g.FillRectangle(barFondo, barRect);
                using var barRell = new SolidBrush(colorBase);
                g.FillRectangle(barRell,
                    new RectangleF(barRect.X, barRect.Y,
                        barRect.Width * nodo.ProgresoNorm, barRect.Height));
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { _timer.Stop(); _timer.Dispose(); }
            base.Dispose(disposing);
        }
    }
}