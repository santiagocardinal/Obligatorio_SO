using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using RestauranteSO.Constants;
using RestauranteSO.Presentation.Themes;

namespace RestauranteSO.Presentation.Controls
{
    public sealed class DockItem : Control
    {
        private string _icono       = "🍽";
        private string _nombre      = "Módulo";
        private string _estado      = "Listo";
        private Color  _colorAcento = ColorConstants.AcentoPrincipal;
        private bool   _activo      = false;
        private bool   _abierto     = false;

        private bool  _isHovered  = false;
        private bool  _isPressed  = false;
        private float _hoverAlpha = 0f;
        private float _escala     = 1f;

        private readonly System.Windows.Forms.Timer _animTimer;

        private const int PaddingTopBottom  = 10;
        private const int PaddingLateral    = 8;
        private const int GapIconoNombre    = 8;
        private const int GapNombreEstado   = 4;
        private const int RadioBorde        = 14;
        private const int AlturaIndicador   = 4;
        private const float TamIcono = 28f;

        public event EventHandler? ItemClicked;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Icono
        {
            get => _icono;
            set { _icono = value; RecalcularTamano(); Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Nombre
        {
            get => _nombre;
            set { _nombre = value; RecalcularTamano(); Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Estado
        {
            get => _estado;
            set { _estado = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color ColorAcento
        {
            get => _colorAcento;
            set { _colorAcento = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Activo
        {
            get => _activo;
            set { _activo = value; Invalidate(); }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool Abierto
        {
            get => _abierto;
            set { _abierto = value; Invalidate(); }
        }

        public DockItem()
        {
            SetStyle(
                ControlStyles.UserPaint            |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.DoubleBuffer         |
                ControlStyles.ResizeRedraw,
                true);

            BackColor   = ColorConstants.FondoDock;
            Cursor      = Cursors.Hand;

            RecalcularTamano();

            _animTimer = new System.Windows.Forms.Timer { Interval = 16 };
            _animTimer.Tick += AnimTimer_Tick;
            _animTimer.Start();
        }

        private void RecalcularTamano()
        {
            using var fIcono  = new Font("Segoe UI Emoji", TamIcono);
            using var fNombre = AppTheme.FuenteLabelBold;
            using var fEstado = AppTheme.FuenteSmall;

            var mIcono = TextRenderer.MeasureText(
                _icono, fIcono, new Size(400, 400),
                TextFormatFlags.NoPadding);

            var mNombre = TextRenderer.MeasureText(
                _nombre, fNombre, new Size(160, 400),
                TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);

            var mEstado = TextRenderer.MeasureText(
                _estado, fEstado, new Size(160, 200),
                TextFormatFlags.NoPadding);

            int anchoContenido = Math.Max(mIcono.Width,
                Math.Max(mNombre.Width, mEstado.Width));
            int anchoTotal = anchoContenido + PaddingLateral * 2;

            int altoTotal =
                PaddingTopBottom +
                mIcono.Height    +
                GapIconoNombre   +
                mNombre.Height   +
                GapNombreEstado  +
                mEstado.Height   +
                PaddingTopBottom +
                AlturaIndicador  + 4;

            anchoTotal = Math.Max(anchoTotal, 110);
            altoTotal  = Math.Max(altoTotal,  100);

            MinimumSize = new Size(anchoTotal, altoTotal);
            Size        = new Size(anchoTotal, altoTotal);
        }

        private void AnimTimer_Tick(object? sender, EventArgs e)
        {
            float targetAlpha = _isHovered ? 1f : 0f;
            float targetEsc   = _isPressed ? 0.94f
                              : _isHovered ? 1.08f
                              : 1f;

            float prevA = _hoverAlpha;
            float prevE = _escala;

            _hoverAlpha = _hoverAlpha < targetAlpha
                ? Math.Min(targetAlpha, _hoverAlpha + 0.09f)
                : Math.Max(targetAlpha, _hoverAlpha - 0.09f);

            _escala = _escala < targetEsc
                ? Math.Min(targetEsc, _escala + 0.012f)
                : Math.Max(targetEsc, _escala - 0.012f);

            if (Math.Abs(_hoverAlpha - prevA) > 0.003f ||
                Math.Abs(_escala     - prevE) > 0.001f)
                Invalidate();
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _isHovered = true;
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _isHovered = false;
            _isPressed = false;
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
            ItemClicked?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            g.Clear(Parent?.BackColor ?? ColorConstants.FondoDock);

            int w = Width;
            int h = Height;
            if (w < 20 || h < 20) return;

            if (_hoverAlpha > 0.01f || _activo)
            {
                float alpha = _activo ? 0.15f : 0.10f * _hoverAlpha;
                var fondoRect = new Rectangle(2, 2, w - 4, h - AlturaIndicador - 4);
                using var fondoBrush = new SolidBrush(
                    Color.FromArgb((int)(255 * alpha), _colorAcento));
                using var fondoPath  =
                    AppTheme.CrearPathRedondeado(fondoRect, RadioBorde);
                g.FillPath(fondoBrush, fondoPath);

                if (_hoverAlpha > 0.3f)
                {
                    using var borderPen = new Pen(
                        Color.FromArgb((int)(60 * _hoverAlpha), _colorAcento), 1f);
                    g.DrawPath(borderPen, fondoPath);
                }
            }

            if (_hoverAlpha > 0.05f)
            {
                float gr = Math.Min(w, h) * 0.5f;
                float cx = w / 2f;
                float cy = (h - AlturaIndicador) / 2f;

                using var glowBrush = new PathGradientBrush(
                    new PointF[]
                    {
                        new(cx - gr, cy - gr), new(cx + gr, cy - gr),
                        new(cx + gr, cy + gr), new(cx - gr, cy + gr)
                    })
                {
                    CenterPoint    = new PointF(cx, cy),
                    CenterColor    = Color.FromArgb(
                        (int)(30 * _hoverAlpha), _colorAcento),
                    SurroundColors = new[] { Color.FromArgb(0, _colorAcento) }
                };
                g.FillEllipse(glowBrush, cx - gr, cy - gr, gr * 2, gr * 2);
            }

            using var fIcono  = new Font("Segoe UI Emoji", TamIcono * _escala);
            using var fNombre = new Font(
                AppTheme.FuenteLabelBold.FontFamily,
                AppTheme.FuenteLabelBold.Size,
                FontStyle.Bold);
            using var fEstado = AppTheme.FuenteSmall;

            var mIcono  = TextRenderer.MeasureText(_icono,  fIcono,
                new Size(w, h), TextFormatFlags.NoPadding);
            var mNombre = TextRenderer.MeasureText(_nombre, fNombre,
                new Size(w - PaddingLateral * 2, h),
                TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
            var mEstado = TextRenderer.MeasureText(_estado, fEstado,
                new Size(w - PaddingLateral * 2, h), TextFormatFlags.NoPadding);

            int contenidoH =
                mIcono.Height   +
                GapIconoNombre  +
                mNombre.Height  +
                GapNombreEstado +
                mEstado.Height;

            int espacioDisponible = h - AlturaIndicador - 6;
            int inicioY = Math.Max(PaddingTopBottom,
                (espacioDisponible - contenidoH) / 2);

            int iconoY = inicioY;
            var iconoRect = new Rectangle(
                0, iconoY,
                w, mIcono.Height + 4);

            TextRenderer.DrawText(g, _icono, fIcono, iconoRect,
                ColorConstants.TextoPrincipal,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.Top              |
                TextFormatFlags.NoPadding);

            int nombreY = iconoY + mIcono.Height + 4 + GapIconoNombre;
            Color colorNombre = _hoverAlpha > 0.3f || _activo
                ? _colorAcento
                : ColorConstants.TextoSecundario;

            var nombreRect = new Rectangle(
                PaddingLateral, nombreY,
                w - PaddingLateral * 2,
                mNombre.Height + 4);

            TextRenderer.DrawText(g, _nombre, fNombre, nombreRect,
                colorNombre,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.Top              |
                TextFormatFlags.WordBreak        |
                TextFormatFlags.NoPadding);

            if (_hoverAlpha > 0.2f || _activo || _abierto)
            {
                int estadoY = nombreY + mNombre.Height + 4 + GapNombreEstado;
                Color colorEstado = Color.FromArgb(
                    (int)(200 * Math.Max(_hoverAlpha, _activo || _abierto ? 1f : 0f)),
                    _colorAcento);

                var estadoRect = new Rectangle(
                    PaddingLateral, estadoY,
                    w - PaddingLateral * 2,
                    mEstado.Height + 4);

                TextRenderer.DrawText(g, _estado, fEstado, estadoRect,
                    colorEstado,
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.Top              |
                    TextFormatFlags.NoPadding);
            }

            if (_abierto || _activo)
            {
                int barraW = _activo ? (int)(w * 0.55f) : (int)(w * 0.25f);
                int barraH = 3;
                int barraX = (w - barraW) / 2;
                int barraY = h - barraH - 2;

                using var barraBrush = new LinearGradientBrush(
                    new Rectangle(barraX, barraY, barraW, barraH),
                    Color.FromArgb(0,   _colorAcento),
                    Color.FromArgb(0,   _colorAcento),
                    LinearGradientMode.Horizontal);

                var blend = new ColorBlend(3)
                {
                    Colors    = new[]
                    {
                        Color.FromArgb(0,   _colorAcento),
                        _colorAcento,
                        Color.FromArgb(0,   _colorAcento)
                    },
                    Positions = new[] { 0f, 0.5f, 1f }
                };
                barraBrush.InterpolationColors = blend;

                using var barraPath =
                    AppTheme.CrearPathRedondeado(
                        new Rectangle(barraX, barraY, barraW, barraH), 2);
                g.FillPath(barraBrush, barraPath);
            }

            if (_isPressed)
            {
                var pressRect = new Rectangle(2, 2, w - 4, h - AlturaIndicador - 4);
                using var pressBrush = new SolidBrush(
                    Color.FromArgb(15, 255, 255, 255));
                using var pressPath  =
                    AppTheme.CrearPathRedondeado(pressRect, RadioBorde);
                g.FillPath(pressBrush, pressPath);
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