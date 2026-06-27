// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Presentation/Controls/ThreadStatusIndicator.cs
// Propósito: Control visual que muestra el estado de un hilo individual.
//            Semáforo visual con indicador de color y nombre del hilo.
// SOLID    : SRP - solo visualiza el estado de un hilo.
// =============================================================================

using RestauranteSO.Constants;
using RestauranteSO.Presentation.Themes;

namespace RestauranteSO.Presentation.Controls
{
    /// <summary>
    /// Indicador visual del estado de un hilo de la simulación.
    ///
    /// Muestra:
    /// - Círculo de color (verde=activo, amarillo=esperando, rojo=detenido)
    /// - Nombre del hilo
    /// - Acción actual (ej: "Preparando #042")
    /// - ProgressBar de progreso (visible solo para cocineros)
    ///
    /// Se actualiza desde el UI timer del formulario.
    /// </summary>
    public sealed class ThreadStatusIndicator : UserControl
    {
        // ─── CONTROLES INTERNOS ───────────────────────────────────────────────

        private readonly Panel _indicadorCirculo;
        private readonly Label _lblNombre;
        private readonly Label _lblAccion;
        private readonly ProgressBar _progressBar;

        // ─── ESTADO ──────────────────────────────────────────────────────────

        private EstadoHilo _estadoActual = EstadoHilo.Detenido;
        private readonly System.Windows.Forms.Timer _pulseTimer;
        private float _pulseAlpha = 1f;
        private bool _pulsando    = false;

        // ─── ENUM INTERNO ────────────────────────────────────────────────────

        public enum EstadoHilo
        {
            Activo,
            Esperando,
            Detenido,
            BajoAtaque
        }

        // ─── CONSTRUCTOR ─────────────────────────────────────────────────────

        public ThreadStatusIndicator()
        {
            SetStyle(ControlStyles.UserPaint |
                     ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.DoubleBuffer, true);

            BackColor = ColorConstants.FondoPanel;
            Height    = 52;
            Padding   = new Padding(4);

            // Círculo de estado
            _indicadorCirculo = new Panel
            {
                Size      = new Size(12, 12),
                Location  = new Point(8, 20),
                BackColor = ColorConstants.TextoHint
            };
            // Hacer circular con región
            _indicadorCirculo.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode =
                    System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                using var b = new SolidBrush(_indicadorCirculo.BackColor);
                e.Graphics.FillEllipse(b,
                    0, 0,
                    _indicadorCirculo.Width - 1,
                    _indicadorCirculo.Height - 1);
            };

            // Label nombre del hilo
            _lblNombre = new Label
            {
                Location  = new Point(26, 6),
                Size      = new Size(Width - 34, 18),
                Font      = AppTheme.FuenteLabelBold,
                ForeColor = ColorConstants.TextoPrincipal,
                BackColor = Color.Transparent,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // Label acción actual
            _lblAccion = new Label
            {
                Location  = new Point(26, 24),
                Size      = new Size(Width - 34, 14),
                Font      = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.TextoHint,
                BackColor = Color.Transparent,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // ProgressBar de progreso (visible solo con progreso > 0)
            _progressBar = new ProgressBar
            {
                Location  = new Point(26, 39),
                Size      = new Size(Width - 34, 6),
                Minimum   = 0,
                Maximum   = 100,
                Value     = 0,
                Style     = ProgressBarStyle.Continuous,
                Visible   = false,
                Anchor    = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            Controls.AddRange(new Control[]
                { _indicadorCirculo, _lblNombre, _lblAccion, _progressBar });

            // Timer para animación de pulso cuando está activo
            _pulseTimer = new System.Windows.Forms.Timer { Interval = 50 };
            _pulseTimer.Tick += PulseTimer_Tick;
        }

        // ─── API PÚBLICA ─────────────────────────────────────────────────────

        public void ActualizarEstado(
            string nombre,
            EstadoHilo estado,
            string accion  = "",
            int progreso   = 0)
        {
            _lblNombre.Text  = nombre;
            _lblAccion.Text  = accion;
            _estadoActual    = estado;

            // Color del indicador según estado
            _indicadorCirculo.BackColor = estado switch
            {
                EstadoHilo.Activo     => ColorConstants.AcentoExito,
                EstadoHilo.Esperando  => ColorConstants.EstadoEsperando,
                EstadoHilo.Detenido   => ColorConstants.TextoHint,
                EstadoHilo.BajoAtaque => ColorConstants.AlertaAtaque,
                _                    => ColorConstants.TextoHint
            };

            // Pulso solo cuando está activo
            if (estado == EstadoHilo.Activo && !_pulsando)
            {
                _pulsando = true;
                _pulseTimer.Start();
            }
            else if (estado != EstadoHilo.Activo && _pulsando)
            {
                _pulsando = false;
                _pulseTimer.Stop();
                _pulseAlpha = 1f;
                _indicadorCirculo.Invalidate();
            }

            // Progreso
            if (progreso > 0)
            {
                _progressBar.Visible = true;
                _progressBar.Value   = Math.Min(100, progreso);
            }
            else
            {
                _progressBar.Visible = false;
            }

            _indicadorCirculo.Invalidate();
        }

        private void PulseTimer_Tick(object? sender, EventArgs e)
        {
            // Oscilación sinusoidal de opacidad para efecto de pulso
            _pulseAlpha = (float)(0.5 + 0.5 * Math.Sin(
                Environment.TickCount64 / 300.0));
            _indicadorCirculo.Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _pulseTimer.Stop();
                _pulseTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}