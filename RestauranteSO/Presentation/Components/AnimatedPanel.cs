using RestauranteSO.Constants;

namespace RestauranteSO.Presentation.Components
{
    public sealed class AnimatedPanel : Panel
    {
        private readonly System.Windows.Forms.Timer _fadeTimer;
        private bool _fadingIn = false;

        public AnimatedPanel()
        {
            _fadeTimer = new System.Windows.Forms.Timer { Interval = 16 };
            _fadeTimer.Tick += FadeTimer_Tick;
            BackColor = ColorConstants.FondoPanel;
        }

        public void MostrarConFade()
        {
            Visible   = true;
            _fadingIn = true;
            _fadeTimer.Start();
        }

        private void FadeTimer_Tick(object? sender, EventArgs e)
        {
            _fadeTimer.Stop();
            _fadingIn = false;
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fadeTimer.Stop();
                _fadeTimer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}