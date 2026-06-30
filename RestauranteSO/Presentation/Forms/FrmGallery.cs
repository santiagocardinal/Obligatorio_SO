using RestauranteSO.Constants;
using RestauranteSO.Presentation.Themes;

namespace RestauranteSO.Presentation.Forms
{
    public sealed class FrmGallery : Form
    {
        private Panel _titleBar = null!;
        private Label _lblTitulo = null!;
        private Button _btnMinimize = null!;
        private Button _btnMaximize = null!;
        private Button _btnClose = null!;
        private PictureBox _pictureBox = null!;

        private const int TITLE_BAR_HEIGHT = 38;

        public FrmGallery()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(800, 600);
            BackColor = ColorConstants.FondoPrincipal;
            ForeColor = ColorConstants.TextoPrincipal;
            Font = AppTheme.FuenteLabel;

            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer | ControlStyles.ResizeRedraw, true);

            ConstruirTitleBar();
            ConstruirCuerpo();

            ResumeLayout(true);
        }

        private void ConstruirTitleBar()
        {
            _titleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = TITLE_BAR_HEIGHT,
                BackColor = ColorConstants.FondoSuperior
            };
            _titleBar.Paint += (s, e) =>
            {
                using var pen = new Pen(ColorConstants.AcentoSecundario, 3);
                e.Graphics.DrawLine(pen, 0, _titleBar.Height - 2, _titleBar.Width, _titleBar.Height - 2);
            };

            _lblTitulo = new Label
            {
                Text = "🖼️  Galería de Imágenes",
                Font = AppTheme.FuenteLabelBold,
                ForeColor = ColorConstants.TextoPrincipal,
                BackColor = ColorConstants.FondoSuperior,
                AutoSize = false,
                Dock = DockStyle.Left,
                Width = 340,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 0, 0, 0)
            };

            _btnMinimize = CrearBotonVentana("─", ColorConstants.TextoHint);
            _btnMaximize = CrearBotonVentana("☐", ColorConstants.TextoHint);
            _btnClose = CrearBotonVentana("✕", ColorConstants.AlertaAtaque);
            _btnMinimize.Click += (_, _) => WindowState = FormWindowState.Minimized;
            _btnMaximize.Click += (_, _) =>
            {
                WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
                _btnMaximize.Text = WindowState == FormWindowState.Maximized ? "☒" : "☐";
            };
            _btnClose.Click += (_, _) => Close();

            var panelDerecho = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                Height = TITLE_BAR_HEIGHT,
                FlowDirection = FlowDirection.RightToLeft,
                WrapContents = false,
                BackColor = ColorConstants.FondoSuperior,
                Padding = new Padding(0, 0, 12, 0)
            };
            panelDerecho.Controls.AddRange(new Control[] { _btnClose, _btnMaximize, _btnMinimize });

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
                BackColor = ColorConstants.FondoSuperior,
                FlatStyle = FlatStyle.Flat,
                Size = new Size(38, TITLE_BAR_HEIGHT),
                Cursor = Cursors.Hand,
                Margin = new Padding(0)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(220, 220, 240);
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(200, 200, 220);
            return btn;
        }

      private void ConstruirCuerpo()
{
    _pictureBox = new PictureBox
    {
        Dock = DockStyle.Fill,
        BackColor = ColorConstants.FondoPanel,
        SizeMode = PictureBoxSizeMode.Zoom
    };

    // Buscar la imagen en la raíz del proyecto (subiendo 4 niveles)
    string rutaImagen = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "GalleryPlaceholder.png");
    rutaImagen = Path.GetFullPath(rutaImagen);

    try
    {
        if (File.Exists(rutaImagen))
        {
            _pictureBox.Image = Image.FromFile(rutaImagen);
        }
        else
        {
            // Si no existe, crear placeholder
            CrearPlaceholder();
        }
    }
    catch (Exception)
    {
        CrearPlaceholder();
    }

    Controls.Add(_pictureBox);
}
        private void CrearPlaceholder()
        {
            var bmp = new Bitmap(800, 600);
            using (var g = Graphics.FromImage(bmp))
            {
                g.Clear(ColorConstants.FondoCard);
                using var font = new Font("Segoe UI", 28f, FontStyle.Bold);
                using var brush = new SolidBrush(ColorConstants.TextoSecundario);
                g.DrawString("🖼️ Galería\n\nReemplace esta imagen\nen Resources", font, brush,
                    new RectangleF(0, 0, 800, 600),
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }
            _pictureBox.Image = bmp;
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