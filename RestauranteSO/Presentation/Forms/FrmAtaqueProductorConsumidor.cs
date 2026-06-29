using RestauranteSO.Constants;
using RestauranteSO.Domain.Enums;
using RestauranteSO.Domain.Interfaces;
using RestauranteSO.Presentation.Themes;
using RestauranteSO.Services.Ataques;
using RestauranteSO.Services.ProductorConsumidor;

namespace RestauranteSO.Presentation.Forms
{
    public sealed class FrmAtaqueProductorConsumidor : Form
    {
        private readonly ProductorConsumidorService       _service;
        private readonly AtaqueProductorConsumidorService _attackService;
        private readonly ISimulationLogger                _logger;

        public FrmAtaqueProductorConsumidor(
            ProductorConsumidorService service,
            AtaqueProductorConsumidorService attackService,
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
            Text          = "⚡ Ataque: Inyección de Pedidos | RestaurantOS";
            WindowState   = FormWindowState.Maximized;
            MinimumSize   = new Size(1200, 750);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor     = ColorConstants.FondoPrincipal;
            // NO asignar Font del form — evita Font.ToLogFont crash

            var banner = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 48,
                BackColor = Color.FromArgb(40, 10, 10)
            };
            banner.Paint += (_, e) =>
            {
                using var pen = new Pen(ColorConstants.AlertaAtaque, 2);
                e.Graphics.DrawLine(pen, 0, banner.Height - 2,
                    banner.Width, banner.Height - 2);
            };

            var bannerTbl = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 1,
                BackColor   = Color.FromArgb(40, 10, 10),
                Padding     = new Padding(16, 0, 16, 0)
            };
            bannerTbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 65f));
            bannerTbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 35f));
            bannerTbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            bannerTbl.Controls.Add(new Label
            {
                Text      = "⚡ MÓDULO EDUCATIVO — Ingeniería Social + Inyección en Cola",
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = ColorConstants.AlertaAtaque,
                BackColor = Color.FromArgb(40, 10, 10),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                AutoSize  = false
            }, 0, 0);

            bannerTbl.Controls.Add(new Label
            {
                Text      = "⚠ SIMULACIÓN EDUCATIVA — Ningún ataque real es ejecutado",
                Font      = new Font("Segoe UI", 10f, FontStyle.Regular),
                ForeColor = ColorConstants.TextoHint,
                BackColor = Color.FromArgb(40, 10, 10),
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

                var frmPC = new FrmProductorConsumidor(
                    _service, _attackService, _logger);
                frmPC.TopLevel        = false;
                frmPC.FormBorderStyle = FormBorderStyle.None;
                frmPC.Dock            = DockStyle.Fill;
                frmPC.Visible         = true;
                contenedor.Controls.Add(frmPC);
                frmPC.BringToFront();

                frmPC.Size = contenedor.Size;
                contenedor.Resize += (_, _) => frmPC.Size = contenedor.Size;

                await Task.Delay(800);
                _service.Iniciar();

                await Task.Delay(2000);

                var dlg = new FrmIngenieriaSocial(
                    "🔧 Soporte Técnico — Actualización del Sistema",
                    "Estimado Encargado:\n\n" +
                    "Soy el Técnico Carlos Martínez de SistemaResto S.A.\n" +
                    "Estoy aquí para instalar la actualización crítica.\n\n" +
                    "⚠ Si no la instalamos ahora, el sistema de pedidos\n" +
                    "puede perder datos esta noche.\n\n" +
                    "¿Me da acceso al sistema por 5 minutos?",
                    "✓ Sí, adelante",
                    "✗ No, esperaré autorización escrita");

                if (dlg.ShowDialog(this) == DialogResult.OK)
                    _attackService.ActivarAtaque(AttackType.InyeccionDePedidos);
            };

            FormClosing += (_, _) =>
            {
                if (_service.EstaCorreindo)        _service.Detener();
                if (_attackService.IsAttackActive) _attackService.DesactivarAtaque();
            };
        }
    }
}