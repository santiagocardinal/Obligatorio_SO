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
        private readonly LectoresEscritoresService       _service;
        private readonly AtaqueLectoresEscritoresService _attackService;
        private readonly ISimulationLogger               _logger;

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
            Text          = "🎣 Ataque: Phishing al Gerente | RestaurantOS";
            WindowState   = FormWindowState.Maximized;
            MinimumSize   = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor     = ColorConstants.FondoPrincipal;
            // NO asignar Font del form — evita Font.ToLogFont crash

            var banner = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 48,
                BackColor = Color.FromArgb(30, 8, 22)
            };
            banner.Paint += (_, e) =>
            {
                using var pen = new Pen(ColorConstants.TarjetaAtaque2, 2);
                e.Graphics.DrawLine(pen, 0, banner.Height - 2,
                    banner.Width, banner.Height - 2);
            };

            var bannerTbl = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 1,
                BackColor   = Color.FromArgb(30, 8, 22),
                Padding     = new Padding(16, 0, 16, 0)
            };
            bannerTbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65f));
            bannerTbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));
            bannerTbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            bannerTbl.Controls.Add(new Label
            {
                Text      = "🎣 MÓDULO EDUCATIVO — Phishing + Compromiso del Menú Compartido",
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = ColorConstants.TarjetaAtaque2,
                BackColor = Color.FromArgb(30, 8, 22),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize  = false
            }, 0, 0);

            bannerTbl.Controls.Add(new Label
            {
                Text      = "⚠ SIMULACIÓN EDUCATIVA — Ningún ataque real es ejecutado",
                Font      = new Font("Segoe UI", 10f, FontStyle.Regular),
                ForeColor = ColorConstants.TextoHint,
                BackColor = Color.FromArgb(30, 8, 22),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                AutoSize  = false
            }, 1, 0);

            banner.Controls.Add(bannerTbl);
            Controls.Add(banner);

            var contenedor = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = ColorConstants.FondoPrincipal
            };
            Controls.Add(contenedor);

            ResumeLayout(true);

            Load += async (_, _) =>
            {
                await Task.Delay(400);

                var frmLE = new FrmLectoresEscritores(
                    _service, _attackService, _logger);
                frmLE.TopLevel        = false;
                frmLE.FormBorderStyle = FormBorderStyle.None;
                frmLE.Dock            = DockStyle.Fill;
                frmLE.Visible         = true;
                contenedor.Controls.Add(frmLE);
                frmLE.BringToFront();

                frmLE.Size = contenedor.Size;
                contenedor.Resize += (_, _) => frmLE.Size = contenedor.Size;

                await Task.Delay(800);
                _service.Iniciar();

                await Task.Delay(3000);

                var dlg = new FrmIngenieriaSocial(
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
            };

            FormClosing += (_, _) =>
            {
                if (_service.EstaCorreindo)        _service.Detener();
                if (_attackService.IsAttackActive) _attackService.DesactivarAtaque();
            };
        }
    }
}