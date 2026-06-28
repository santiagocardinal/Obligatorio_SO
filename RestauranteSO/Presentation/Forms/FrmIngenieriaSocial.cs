using RestauranteSO.Constants;
using RestauranteSO.Presentation.Themes;

namespace RestauranteSO.Presentation.Forms
{
    public sealed class FrmIngenieriaSocial : Form
    {
        private readonly string _tituloBandeja;
        private readonly string _mensajeAtacante;
        private readonly string _textoAceptar;
        private readonly string _textoRechazar;

        public FrmIngenieriaSocial(
            string tituloBandeja,
            string mensajeAtacante,
            string textoAceptar,
            string textoRechazar)
        {
            _tituloBandeja   = tituloBandeja;
            _mensajeAtacante = mensajeAtacante;
            _textoAceptar    = textoAceptar;
            _textoRechazar   = textoRechazar;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text            = _tituloBandeja;
            Size            = new Size(560, 480);
            MaximizeBox     = false;
            MinimizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            BackColor       = ColorConstants.FondoPanel;
            ForeColor       = ColorConstants.TextoPrincipal;
            Font            = AppTheme.FuenteLabel;

            // ── Banner ────────────────────────────────────────────────────
            var panelBanner = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 60,
                BackColor = Color.FromArgb(30, 100, 200)
            };

            var lblBannerIco = new Label
            {
                Text      = "🔧",
                Font      = new Font("Segoe UI Emoji", 22f),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(30, 100, 200),
                AutoSize  = false,
                Size      = new Size(50, 56),
                Location  = new Point(12, 2),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblBannerTit = new Label
            {
                Text      = "Soporte Técnico — SistemaResto S.A.",
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(30, 100, 200),
                AutoSize  = true,
                Location  = new Point(66, 12)
            };

            var lblBannerSub = new Label
            {
                Text      = "Departamento de Tecnología y Sistemas",
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(200, 220, 255),
                BackColor = Color.FromArgb(30, 100, 200),
                AutoSize  = true,
                Location  = new Point(68, 34)
            };

            panelBanner.Controls.AddRange(
                new Control[] { lblBannerIco, lblBannerTit, lblBannerSub });

            // ── Badge educativo ───────────────────────────────────────────
            var panelEdu = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 36,
                BackColor = Color.FromArgb(60, 80, 20, 0),
                Padding   = new Padding(12, 0, 12, 0)
            };

            var lblEdu = new Label
            {
                Text =
                    "⚠ SIMULACIÓN EDUCATIVA — Este mensaje simula ingeniería social. " +
                    "Nada real ocurre.",
                Font      = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.AcentoSecundario,
                BackColor = Color.FromArgb(60, 80, 20, 0),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            panelEdu.Controls.Add(lblEdu);

            // ── Mensaje del atacante ──────────────────────────────────────
            var panelMensaje = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 180,
                BackColor = ColorConstants.FondoPanel,
                Padding   = new Padding(16, 8, 16, 8)
            };

            var lblMensaje = new RichTextBox
            {
                Text        = _mensajeAtacante,
                Font        = AppTheme.FuenteLabel,
                BackColor   = Color.FromArgb(35, 35, 50),
                ForeColor   = ColorConstants.TextoPrincipal,
                BorderStyle = BorderStyle.None,
                ReadOnly    = true,
                ScrollBars  = RichTextBoxScrollBars.None,
                Dock        = DockStyle.Fill
            };
            panelMensaje.Controls.Add(lblMensaje);

            // ── Señales de alerta ─────────────────────────────────────────
            var panelAlertas = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 110,
                BackColor = Color.FromArgb(40, 220, 50, 50),
                Padding   = new Padding(8)
            };
            panelAlertas.Paint += (_, e) =>
            {
                using var pen = new Pen(ColorConstants.AlertaAtaque, 1);
                e.Graphics.DrawRectangle(pen, 0, 0,
                    panelAlertas.Width - 1, panelAlertas.Height - 1);
            };

            var lblAlertaTit = new Label
            {
                Text      = "🔴 Señales de Ingeniería Social detectadas:",
                Font      = AppTheme.FuenteLabelBold,
                ForeColor = ColorConstants.AlertaAtaque,
                BackColor = Color.FromArgb(40, 220, 50, 50),
                AutoSize  = false,
                Dock      = DockStyle.Top,
                Height    = 22,
                TextAlign = ContentAlignment.MiddleLeft
            };

            var lblAlertas = new Label
            {
                Text =
                    "• Urgencia artificial ('actualización crítica')\n" +
                    "• Acceso sin verificación de identidad previa\n" +
                    "• Presión temporal para decidir rápidamente\n" +
                    "• Empresa genérica sin credenciales verificables",
                Font      = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.TextoSecundario,
                BackColor = Color.FromArgb(40, 220, 50, 50),
                AutoSize  = false,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.TopLeft
            };

            panelAlertas.Controls.Add(lblAlertas);
            panelAlertas.Controls.Add(lblAlertaTit);

            // ── Botones ───────────────────────────────────────────────────
            var panelBotones = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 60,
                BackColor = ColorConstants.FondoSuperior,
                Padding   = new Padding(16, 10, 16, 10)
            };

            var tblBotones = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 2,
                RowCount    = 1,
                BackColor   = ColorConstants.FondoSuperior
            };
            tblBotones.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 50f));
            tblBotones.ColumnStyles.Add(
                new ColumnStyle(SizeType.Percent, 50f));
            tblBotones.RowStyles.Add(
                new RowStyle(SizeType.Percent, 100f));

            var btnAceptar = new Button
            {
                Text      = _textoAceptar,
                Dock      = DockStyle.Fill,
                Font      = AppTheme.FuenteBoton,
                BackColor = ColorConstants.AlertaAtaque,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                Margin    = new Padding(0, 0, 4, 0)
            };
            btnAceptar.FlatAppearance.BorderSize = 0;
            btnAceptar.Click += (_, _) =>
            {
                DialogResult = DialogResult.OK;
                Close();
            };

            var btnRechazar = new Button
            {
                Text      = _textoRechazar + " ✓",
                Dock      = DockStyle.Fill,
                Font      = AppTheme.FuenteBoton,
                BackColor = ColorConstants.AcentoExito,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                Margin    = new Padding(4, 0, 0, 0)
            };
            btnRechazar.FlatAppearance.BorderSize = 0;
            btnRechazar.Click += (_, _) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            tblBotones.Controls.Add(btnAceptar,  0, 0);
            tblBotones.Controls.Add(btnRechazar, 1, 0);
            panelBotones.Controls.Add(tblBotones);

            // ── Ensamblar ─────────────────────────────────────────────────
            Controls.AddRange(new Control[]
            {
                panelBotones,
                panelAlertas,
                panelMensaje,
                panelEdu,
                panelBanner
            });
        }
    }
}