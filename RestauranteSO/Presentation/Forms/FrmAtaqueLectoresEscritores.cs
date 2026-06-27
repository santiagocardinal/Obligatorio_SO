// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Presentation/Forms/FrmAtaqueLectoresEscritores.cs
// Propósito: Abre la simulación Lectores-Escritores con ataque de phishing
//            ya preparado para activarse. Incluye correo falso visual.
// SOLID    : SRP.
// =============================================================================

using RestauranteSO.Constants;
using RestauranteSO.Domain.Enums;
using RestauranteSO.Domain.Interfaces;
using RestauranteSO.Presentation.Themes;
using RestauranteSO.Services.Ataques;
using RestauranteSO.Services.LectoresEscritores;

namespace RestauranteSO.Presentation.Forms
{
    public sealed class FrmAtaqueLectoresEscritores : Form
    {
        private readonly LectoresEscritoresService _service;
        private readonly AtaqueLectoresEscritoresService _attackService;
        private readonly ISimulationLogger _logger;

        public FrmAtaqueLectoresEscritores(
            LectoresEscritoresService service,
            AtaqueLectoresEscritoresService attackService,
            ISimulationLogger logger)
        {
            _service       = service;
            _attackService = attackService;
            _logger        = logger;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            Text          = "🎣 Ataque: Phishing al Gerente | RestauranteSO";
            Size          = new Size(1300, 860);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor     = ColorConstants.FondoPrincipal;
            Font          = AppTheme.FuenteLabel;

            var bannerAtaque = ConstruirBannerAtaque();
            Controls.Add(bannerAtaque);

            var frmLE = new FrmLectoresEscritores(
                _service, _attackService, _logger);
            frmLE.TopLevel        = false;
            frmLE.FormBorderStyle = FormBorderStyle.None;
            frmLE.Dock            = DockStyle.Fill;
            frmLE.Visible         = true;
            Controls.Add(frmLE);

            ResumeLayout(true);

            Load += async (_, _) =>
            {
                await Task.Delay(800);
                _service.Iniciar();
                await Task.Delay(3000);
                MostrarCorreoPhishing();
            };

            FormClosing += (_, _) =>
            {
                if (_service.EstaCorreindo)    _service.Detener();
                if (_attackService.IsAttackActive) _attackService.DesactivarAtaque();
            };
        }

        private void MostrarCorreoPhishing()
        {
            using var dlg = new FrmIngenieriaSocial(
                "📧 [BANDEJA DE ENTRADA] — 1 mensaje nuevo",
                "De: noreply@sistema-gestion-restaurante.net\n" +
                "Para: gerente@restaurantedoncod.com\n" +
                "Asunto: ⚠ ACCIÓN REQUERIDA: Verificación de cuenta obligatoria\n\n" +
                "Estimado/a Gerente,\n\n" +
                "Hemos detectado actividad inusual en su cuenta del\n" +
                "Sistema de Menús. Para proteger su cuenta, debe\n" +
                "verificar sus credenciales en las próximas 2 horas.\n\n" +
                "De lo contrario su acceso será suspendido temporalmente.",
                "🔗 Verificar mi cuenta ahora",
                "🗑 Mover a Spam (correcto ✓)");

            if (dlg.ShowDialog(this) == DialogResult.OK)
                _attackService.ActivarAtaque(AttackType.PhishingMenuAlterado);
        }

        private Panel ConstruirBannerAtaque()
        {
            var panel = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 42,
                BackColor = Color.FromArgb(50, 10, 30),
                Padding   = new Padding(12, 0, 12, 0)
            };
            panel.Paint += (_, e) =>
            {
                using var pen = new Pen(ColorConstants.TarjetaAtaque2, 2);
                e.Graphics.DrawLine(pen, 0, panel.Height - 2,
                    panel.Width, panel.Height - 2);
            };

            var lblTit = new Label
            {
                Text =
                    "🎣 MÓDULO DE ATAQUE EDUCATIVO — " +
                    "Phishing + Compromiso del Menú Compartido (Lectores-Escritores)",
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = ColorConstants.TarjetaAtaque2,
                AutoSize  = true,
                Location  = new Point(12, 10)
            };

            var lblSub = new Label
            {
                Text      = "⚠ SIMULACIÓN EDUCATIVA — Ningún ataque real es ejecutado",
                Font      = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.TextoHint,
                Anchor    = AnchorStyles.Top | AnchorStyles.Right,
                AutoSize  = true
            };
            lblSub.Location = new Point(panel.Width - 340, 14);
            panel.Resize += (_, _) =>
                lblSub.Location = new Point(panel.Width - 340, 14);

            panel.Controls.AddRange(new Control[] { lblTit, lblSub });
            return panel;
        }
    }
}