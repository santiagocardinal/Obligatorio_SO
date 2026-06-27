// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Presentation/Forms/FrmIngenieriaSocial.cs
// Propósito: Diálogo que simula un intento de ingeniería social.
//            Aparece cuando el usuario activa el ataque en cualquier simulación.
//            Muestra el escenario del atacante de forma convincente.
// SOLID    : SRP - solo muestra el diálogo de ingeniería social.
// =============================================================================

using RestauranteSO.Constants;
using RestauranteSO.Presentation.Themes;

namespace RestauranteSO.Presentation.Forms
{
    /// <summary>
    /// Formulario de simulación de ingeniería social.
    ///
    /// Diseñado para parecer un diálogo legítimo de soporte técnico
    /// con el fin de demostrar cómo un atacante engaña a los usuarios.
    ///
    /// AVISO EDUCATIVO: Este formulario muestra técnicas de ingeniería
    /// social ÚNICAMENTE con fines educativos. Ningún código malicioso
    /// es ejecutado en ningún caso.
    /// </summary>
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
            Text          = _tituloBandeja;
            Size          = new Size(540, 420);
            MaximizeBox   = false;
            MinimizeBox   = false;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            BackColor     = ColorConstants.FondoPanel;
            ForeColor     = ColorConstants.TextoPrincipal;
            Font          = AppTheme.FuenteLabel;

            // ── Banner de "soporte técnico" ───────────────────────────────
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
                AutoSize  = true,
                Location  = new Point(66, 12)
            };

            var lblBannerSub = new Label
            {
                Text      = "Departamento de Tecnología y Sistemas",
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(200, 220, 255),
                AutoSize  = true,
                Location  = new Point(68, 34)
            };

            panelBanner.Controls.AddRange(new Control[]
                { lblBannerIco, lblBannerTit, lblBannerSub });

            // ── Badge educativo ───────────────────────────────────────────
            var panelEdu = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 36,
                BackColor = Color.FromArgb(30, 150, 80, 0),
                Padding   = new Padding(12, 0, 12, 0)
            };

            var lblEdu = new Label
            {
                Text =
                    "⚠ SIMULACIÓN EDUCATIVA — Este mensaje simula un ataque " +
                    "de Ingeniería Social. Nunca ocurre nada real.",
                Font      = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.AcentoSecundario,
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };
            panelEdu.Controls.Add(lblEdu);

            // ── Contenido del mensaje ─────────────────────────────────────
            var panelContenido = new Panel
            {
                Padding   = new Padding(20),
                BackColor = Color.Transparent
            };

            var lblMensaje = new RichTextBox
            {
                Text       = _mensajeAtacante,
                Font       = AppTheme.FuenteLabel,
                BackColor  = Color.FromArgb(35, 35, 50),
                ForeColor  = ColorConstants.TextoPrincipal,
                BorderStyle = BorderStyle.None,
                ReadOnly   = true,
                ScrollBars = RichTextBoxScrollBars.None,
                Location   = new Point(20, 12),
                Size       = new Size(480, 160)
            };

            // ── Señales de alerta ─────────────────────────────────────────
            var panelAlertas = new Panel
            {
                Location  = new Point(20, 180),
                Size      = new Size(480, 100),
                BackColor = Color.FromArgb(40, 220, 50, 50)
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
                AutoSize  = true,
                Location  = new Point(8, 6)
            };

            var lblAlertas = new Label
            {
                Text =
                    "• Urgencia artificial ('actualización crítica')\n" +
                    "• Solicita acceso sin verificación de identidad previa\n" +
                    "• Presión temporal para decidir rápidamente\n" +
                    "• Nombre genérico de empresa sin credenciales verificables",
                Font      = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.TextoSecundario,
                AutoSize  = false,
                Size      = new Size(464, 72),
                Location  = new Point(8, 24)
            };

            panelAlertas.Controls.AddRange(new Control[]
                { lblAlertaTit, lblAlertas });

            panelContenido.Controls.AddRange(new Control[]
                { lblMensaje, panelAlertas });

            // ── Botones ───────────────────────────────────────────────────
            var panelBotones = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 56,
                BackColor = ColorConstants.FondoSuperior,
                Padding   = new Padding(16, 10, 16, 10)
            };

            var btnAceptar = new Button
            {
                Text      = _textoAceptar,
                Size      = new Size(200, 34),
                Font      = AppTheme.FuenteBoton,
                BackColor = ColorConstants.AlertaAtaque,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                Location  = new Point(16, 11)
            };
            btnAceptar.FlatAppearance.BorderSize = 0;
            btnAceptar.Click += (_, _) =>
            {
                DialogResult = DialogResult.OK;
                Close();
            };

            var btnRechazar = new Button
            {
                Text      = _textoRechazar + " (correcto ✓)",
                Size      = new Size(200, 34),
                Font      = AppTheme.FuenteBoton,
                BackColor = ColorConstants.AcentoExito,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                Location  = new Point(224, 11)
            };
            btnRechazar.FlatAppearance.BorderSize = 0;
            btnRechazar.Click += (_, _) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };

            panelBotones.Controls.AddRange(
                new Control[] { btnAceptar, btnRechazar });

            // ── Ensamblar form ────────────────────────────────────────────
            int cuerpoY  = 96;   // banner + edu
            int cuerpoH  = Height - cuerpoY - 56 - 8;
            panelContenido.Location = new Point(0, cuerpoY);
            panelContenido.Size     = new Size(Width - 16, cuerpoH);

            Controls.AddRange(new Control[]
            {
                panelBanner, panelEdu, panelContenido, panelBotones
            });
        }
    }
}