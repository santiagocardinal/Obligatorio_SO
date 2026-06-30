using RestauranteSO.Constants;
using RestauranteSO.Domain.Enums;
using RestauranteSO.Domain.Interfaces;
using RestauranteSO.Presentation.Components;
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

        private Panel _titleBar = null!;
        private Label _lblTitulo = null!;
        private Button _btnClose = null!;
        private Button _btnMinimize = null!;
        private Button _btnMaximize = null!;
        private StatusBadge _badgeEstado = null!;

        private Panel _contenedor = null!;
        private FrmProductorConsumidor? _frmPC = null;

        private const int TITLE_BAR_HEIGHT = 38;

        public FrmAtaqueProductorConsumidor(
            ProductorConsumidorService service,
            AtaqueProductorConsumidorService attackService,
            ISimulationLogger logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _attackService = attackService ?? throw new ArgumentNullException(nameof(attackService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1280, 800);
            BackColor = ColorConstants.FondoPrincipal;
            ForeColor = ColorConstants.TextoPrincipal;
            Font = AppTheme.FuenteLabel;

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);

            ConstruirTitleBar();

            _contenedor = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorConstants.FondoPrincipal,
                Padding = new Padding(0)
            };
            Controls.Add(_contenedor);

            ResumeLayout(true);

            Load += async (_, _) =>
            {
                await Task.Delay(400);
                _frmPC = new FrmProductorConsumidor(_service, _attackService, _logger)
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill,
                    Font = SystemFonts.DefaultFont
                };
                _contenedor.Controls.Add(_frmPC);
                _frmPC.Visible = true;
                _frmPC.BringToFront();
                _frmPC.Size = _contenedor.Size;
                _contenedor.Resize += (_, _) =>
                {
                    if (!_frmPC.IsDisposed)
                        _frmPC.Size = _contenedor.Size;
                };

                await Task.Delay(800);
                _service.Iniciar();

                await Task.Delay(2000);
                using var dlg = new FrmIngenieriaSocial(
                    "🔧 Soporte Técnico — Actualización del Sistema",
                    "Estimado Encargado:\n\nSoy el Técnico Carlos Martínez de SistemaResto S.A.\n" +
                    "Estoy aquí para instalar la actualización crítica.\n\n" +
                    "⚠ Si no la instalamos ahora, el sistema de pedidos puede perder datos esta noche.\n\n" +
                    "¿Me da acceso al sistema por 5 minutos?",
                    "✓ Sí, adelante",
                    "✗ No, esperaré autorización escrita");
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    _attackService.ActivarAtaque(AttackType.InyeccionDePedidos);
            };

            FormClosing += (_, _) =>
            {
                if (_service.EstaCorreindo) _service.Detener();
                if (_attackService.IsAttackActive) _attackService.DesactivarAtaque();
            };
        }

        private void ConstruirTitleBar()
        {
            _titleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = TITLE_BAR_HEIGHT,
                BackColor = ColorConstants.FondoAtaque,
                Padding = new Padding(0)
            };
            _titleBar.Paint += (s, e) =>
            {
                using var pen = new Pen(ColorConstants.AlertaAtaque, 3);
                e.Graphics.DrawLine(pen, 0, _titleBar.Height - 2, _titleBar.Width, _titleBar.Height - 2);
            };

            _lblTitulo = new Label
            {
                Text = "⚡ MÓDULO EDUCATIVO — Ingeniería Social + Inyección en Cola",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = ColorConstants.AlertaAtaque,
                BackColor = ColorConstants.FondoAtaque,
                AutoSize = false,
                Dock = DockStyle.Left,
                Width = 520,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 0, 0, 0)
            };

            _badgeEstado = new StatusBadge
            {
                Text = "⚠ SIMULACIÓN EDUCATIVA",
                ColorAcento = ColorConstants.AlertaAtaque,
                Size = new Size(190, 30),
                Anchor = AnchorStyles.Right,
                Font = AppTheme.FuenteSmallBold
            };

            _btnMinimize = CrearBotonVentana("─", ColorConstants.TextoHint);
            _btnMaximize = CrearBotonVentana("☐", ColorConstants.TextoHint);
            _btnClose = CrearBotonVentana("✕", ColorConstants.AlertaAtaque);
            _btnClose.Click += (_, _) => CerrarVentana();
            _btnMinimize.Click += (_, _) => WindowState = FormWindowState.Minimized;
            _btnMaximize.Click += (_, _) =>
            {
                WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
                _btnMaximize.Text = WindowState == FormWindowState.Maximized ? "☒" : "☐";
            };

            var panelDerecho = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Height = TITLE_BAR_HEIGHT,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = ColorConstants.FondoAtaque,
                Padding = new Padding(0, 0, 12, 0)
            };
            panelDerecho.Controls.AddRange(new Control[] { _btnClose, _btnMaximize, _btnMinimize, _badgeEstado });
            panelDerecho.Resize += (_, _) => _badgeEstado.Location = new Point(panelDerecho.Width - 200, 4);

            _titleBar.Controls.Add(_lblTitulo);
            _titleBar.Controls.Add(panelDerecho);

            _titleBar.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    NativeMethods.ReleaseCapture();
                    NativeMethods.SendMessage(Handle, 0xA1, 0x2, 0);
                }
            };

            Controls.Add(_titleBar);
        }

        private Button CrearBotonVentana(string texto, Color color)
        {
            var btn = new Button
            {
                Text = texto,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = color,
                BackColor = ColorConstants.FondoAtaque,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(38, TITLE_BAR_HEIGHT),
                Cursor = Cursors.Hand,
                Margin = new Padding(0)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 20, 20);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(40, 10, 10);
            return btn;
        }

        private void CerrarVentana()
        {
            if (_service.EstaCorreindo) _service.Detener();
            if (_attackService.IsAttackActive) _attackService.DesactivarAtaque();
            Close();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                const int CS_DROPSHADOW = 0x20000;
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }
    }
}