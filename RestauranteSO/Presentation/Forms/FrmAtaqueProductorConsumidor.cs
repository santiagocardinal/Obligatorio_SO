// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Presentation/Forms/FrmAtaqueProductorConsumidor.cs
// Propósito: Abre directamente la simulación Productor-Consumidor con el
//            ataque ya activo. Incluye narrativa completa del escenario.
// SOLID    : SRP - solo presenta el ataque sobre PC.
// =============================================================================

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
        private readonly ProductorConsumidorService _service;
        private readonly AtaqueProductorConsumidorService _attackService;
        private readonly ISimulationLogger _logger;

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

            Text          = "⚡ Ataque: Inyección de Pedidos | RestauranteSO";
            Size          = new Size(1300, 860);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor     = ColorConstants.FondoPrincipal;
            Font          = AppTheme.FuenteLabel;

            // Banner de ataque en la parte superior
            var panelBannerAtaque = ConstruirBannerAtaque();
            Controls.Add(panelBannerAtaque);

            // Embeber el FrmProductorConsumidor como panel dentro de este form
            var frmPC = new FrmProductorConsumidor(
                _service, _attackService, _logger);
            frmPC.TopLevel   = false;
            frmPC.FormBorderStyle = FormBorderStyle.None;
            frmPC.Dock       = DockStyle.Fill;
            frmPC.Visible    = true;

            Controls.Add(frmPC);

            ResumeLayout(true);

            // Auto-activar el ataque al cargar el formulario
            Load += async (_, _) =>
            {
                await Task.Delay(800);

                // Iniciar la simulación
                _service.Iniciar();

                await Task.Delay(2000);

                // Mostrar el diálogo de ingeniería social automáticamente
                var dlg = new FrmIngenieriaSocial(
                    "🔧 Soporte Técnico — Actualización del Sistema",
                    "Estimado Encargado:\n\n" +
                    "Soy el Técnico Carlos Martínez de SistemaResto S.A.\n" +
                    "Estoy aquí para instalar la actualización crítica #RS-2024-11.\n\n" +
                    "⚠ Si no la instalamos ahora, el sistema de pedidos\n" +
                    "puede perder datos esta noche.\n\n" +
                    "¿Me da acceso al sistema por 5 minutos?",
                    "✓ Sí, adelante (instalar actualización)",
                    "✗ No, esperaré autorización escrita");

                if (dlg.ShowDialog(this) == DialogResult.OK)
                    _attackService.ActivarAtaque(AttackType.InyeccionDePedidos);
            };

            FormClosing += (_, _) =>
            {
                if (_service.EstaCorreindo)    _service.Detener();
                if (_attackService.IsAttackActive) _attackService.DesactivarAtaque();
            };
        }

        private Panel ConstruirBannerAtaque()
        {
            var panel = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 42,
                BackColor = Color.FromArgb(60, 15, 15),
                Padding   = new Padding(12, 0, 12, 0)
            };
            panel.Paint += (_, e) =>
            {
                using var pen = new Pen(ColorConstants.AlertaAtaque, 2);
                e.Graphics.DrawLine(pen, 0, panel.Height - 2,
                    panel.Width, panel.Height - 2);
            };

            var lblTit = new Label
            {
                Text =
                    "⚡ MÓDULO DE ATAQUE EDUCATIVO — " +
                    "Ingeniería Social + Inyección en Cola de Pedidos",
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = ColorConstants.AlertaAtaque,
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