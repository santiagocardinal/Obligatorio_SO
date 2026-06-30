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
        private readonly IPedidoRepository _pedidoRepo;

        // ─── BARRA DE TÍTULO ─────────────────────────────────────────────────
        private Panel _titleBar = null!;
        private Label _lblTitulo = null!;
        private Button _btnClose = null!;
        private Button _btnMinimize = null!;
        private Button _btnMaximize = null!;
        private StatusBadge _badgeEstado = null!;

        // ─── LAYOUT PRINCIPAL ──────────────────────────────────────────────
        private TableLayoutPanel _mainLayout = null!;
        private Panel _panelSimulacion = null!;
        private Panel _panelAtaque = null!;

        // ─── CONTROLES DEL PANEL DE ATAQUE ─────────────────────────────────
        private Label _lblAtaqueTitulo = null!;
        private Panel _panelActor = null!;
        private Label _lblActorIcono = null!;
        private Label _lblActorNombre = null!;
        private FlowLayoutPanel _flowPasos = null!;
        private ListBox _listEventos = null!;

        // ─── FORMULARIO HIJO ──────────────────────────────────────────────
        private FrmProductorConsumidor? _frmPC = null;

        // ─── TIMERS Y ESTADO ───────────────────────────────────────────────
        private System.Windows.Forms.Timer _timerAtaque = null!;
        private readonly Queue<AtaquePaso> _pasosPendientes = new();
        private AtaquePaso? _pasoActual = null;
        private int _contadorPasos = 0;
        private readonly Random _random = new();

        private const int TITLE_BAR_HEIGHT = 38;
        private const int ANCHO_COLUMNA_ATAQUE = 380;

        // ─── CONSTRUCTOR ────────────────────────────────────────────────────

        public FrmAtaqueProductorConsumidor(
            ProductorConsumidorService service,
            AtaqueProductorConsumidorService attackService,
            ISimulationLogger logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _attackService = attackService ?? throw new ArgumentNullException(nameof(attackService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _pedidoRepo = Configuration.AppSettings.Resolver<IPedidoRepository>();

            InitializeComponent();
            ConfigurarEventos();
        }

        // ─── INICIALIZACIÓN ────────────────────────────────────────────────

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
            ConstruirLayoutPrincipal();

            ResumeLayout(true);
        }

        private void ConstruirTitleBar()
        {
            _titleBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = TITLE_BAR_HEIGHT,
                BackColor = ColorConstants.FondoAtaque
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

        private void ConstruirLayoutPrincipal()
        {
            _mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = ColorConstants.FondoPrincipal,
                Padding = new Padding(0)
            };
            _mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f));
            _mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, ANCHO_COLUMNA_ATAQUE));
            _mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            // Panel de simulación (izquierda)
            _panelSimulacion = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorConstants.FondoPrincipal,
                Padding = new Padding(0)
            };

            // Panel de ataque (derecha)
            _panelAtaque = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorConstants.FondoAtaque,
                Padding = new Padding(8, 8, 8, 8)
            };

            _mainLayout.Controls.Add(_panelSimulacion, 0, 0);
            _mainLayout.Controls.Add(_panelAtaque, 1, 0);

            // Construir el contenido del panel de ataque
            ConstruirPanelAtaque();

            Controls.Add(_mainLayout);
        }

        private void ConstruirPanelAtaque()
        {
            // Título
            _lblAtaqueTitulo = new Label
            {
                Text = "⚡ Actividad Maliciosa",
                Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = ColorConstants.AlertaAtaque,
                BackColor = ColorConstants.FondoAtaque,
                Dock = DockStyle.Top,
                Height = 34,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 0)
            };
            _panelAtaque.Controls.Add(_lblAtaqueTitulo);

            // Actor (atacante)
            var actorPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = ColorConstants.FondoAtaque,
                Padding = new Padding(4)
            };
            _lblActorIcono = new Label
            {
                Text = "👤",
                Font = new Font("Segoe UI Emoji", 28f),
                ForeColor = ColorConstants.TextoPrincipal,
                BackColor = ColorConstants.FondoAtaque,
                AutoSize = false,
                Size = new Size(50, 50),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(4, 4)
            };
            _lblActorNombre = new Label
            {
                Text = "Atacante (proceso malicioso)",
                Font = AppTheme.FuenteLabelBold,
                ForeColor = ColorConstants.AlertaAtaque,
                BackColor = ColorConstants.FondoAtaque,
                AutoSize = false,
                Size = new Size(220, 30),
                Location = new Point(60, 14),
                TextAlign = ContentAlignment.MiddleLeft
            };
            actorPanel.Controls.Add(_lblActorIcono);
            actorPanel.Controls.Add(_lblActorNombre);
            _panelAtaque.Controls.Add(actorPanel);

            // Separador
            _panelAtaque.Controls.Add(new Panel
            {
                Dock = DockStyle.Top,
                Height = 2,
                BackColor = ColorConstants.Separador
            });

            // Flow de pasos (visualización del flujo)
            _flowPasos = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 140,
                BackColor = ColorConstants.FondoAtaque,
                Padding = new Padding(4),
                AutoScroll = false,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };
            _panelAtaque.Controls.Add(_flowPasos);

            // Lista de eventos
            var lblEventos = new Label
            {
                Text = "📋 Registro de acciones",
                Font = AppTheme.FuenteSmallBold,
                ForeColor = ColorConstants.TextoSecundario,
                BackColor = ColorConstants.FondoAtaque,
                Dock = DockStyle.Top,
                Height = 28,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(4, 0, 0, 0)
            };
            _panelAtaque.Controls.Add(lblEventos);

            _listEventos = new ListBox
            {
                Dock = DockStyle.Fill,
                BackColor = ColorConstants.FondoPanel,
                ForeColor = ColorConstants.TextoPrincipal,
                Font = new Font("Consolas", 9f),
                BorderStyle = BorderStyle.None,
                DrawMode = DrawMode.OwnerDrawFixed,
                ItemHeight = 20,
                IntegralHeight = false
            };
            _listEventos.DrawItem += ListEventos_DrawItem;
            _panelAtaque.Controls.Add(_listEventos);

            // Inicializar con un mensaje
            AgregarEvento("🟡 Esperando inicio del ataque...");
        }

        private void ListEventos_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var lb = (ListBox)sender!;
            string texto = lb.Items[e.Index].ToString() ?? "";
            e.DrawBackground();
            TextRenderer.DrawText(e.Graphics, texto, lb.Font, e.Bounds,
                lb.ForeColor, TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            e.DrawFocusRectangle();
        }

        // ─── EVENTOS ────────────────────────────────────────────────────────

        private void ConfigurarEventos()
        {
            _attackService.AtaqueActivado += (_, tipo) =>
            {
                if (InvokeRequired) { BeginInvoke(() => OnAtaqueActivado(tipo)); return; }
                OnAtaqueActivado(tipo);
            };

            _attackService.AtaqueDesactivado += (_, _) =>
            {
                if (InvokeRequired) { BeginInvoke(OnAtaqueDesactivado); return; }
                OnAtaqueDesactivado();
            };

            FormClosing += (_, _) =>
            {
                _timerAtaque?.Stop();
                if (_service.EstaCorreindo) _service.Detener();
                if (_attackService.IsAttackActive) _attackService.DesactivarAtaque();
            };

            // Cargar el formulario hijo cuando el formulario esté visible
            this.Load += async (_, _) =>
            {
                await Task.Delay(400);
                _frmPC = new FrmProductorConsumidor(_service, _attackService, _logger)
                {
                    TopLevel = false,
                    FormBorderStyle = FormBorderStyle.None,
                    Dock = DockStyle.Fill,
                    Font = SystemFonts.DefaultFont
                };
                _panelSimulacion.Controls.Add(_frmPC);
                _frmPC.Visible = true;
                _frmPC.BringToFront();
                _frmPC.Size = _panelSimulacion.Size;

                // Redimensionar el hijo cuando cambie el tamaño del panel
                _panelSimulacion.Resize += (_, _) =>
                {
                    if (_frmPC != null && !_frmPC.IsDisposed)
                        _frmPC.Size = _panelSimulacion.Size;
                };

                await Task.Delay(800);
                _service.Iniciar();
            };
        }

        // ─── MANEJO DE ATAQUE ──────────────────────────────────────────────

        private void OnAtaqueActivado(AttackType tipo)
        {
            _badgeEstado.Text = "⚡ ATAQUE ACTIVO";
            _badgeEstado.ColorAcento = ColorConstants.AlertaAtaque;

            _lblActorNombre.Text = "👤 Atacante (proceso malicioso) — ACTIVO";
            _lblActorNombre.ForeColor = ColorConstants.AlertaAtaque;

            AgregarEvento("🚨 ATAQUE ACTIVADO — Inyección de pedidos en cola");

            // Iniciar timer de visualización si no está corriendo
            if (_timerAtaque == null)
            {
                _timerAtaque = new System.Windows.Forms.Timer { Interval = 1800 };
                _timerAtaque.Tick += TimerAtaque_Tick;
            }
            _timerAtaque.Start();

            // Limpiar pasos anteriores
            _flowPasos.Controls.Clear();
            _pasosPendientes.Clear();
            _pasoActual = null;
            _contadorPasos = 0;

            // Agregar paso inicial: Atacante listo
            AgregarPaso("👤", "Atacante listo", ColorConstants.TextoPrincipal);
        }

        private void OnAtaqueDesactivado()
        {
            _badgeEstado.Text = "🛡 ATAQUE DESACTIVADO";
            _badgeEstado.ColorAcento = ColorConstants.AcentoExito;

            _lblActorNombre.Text = "👤 Atacante (detenido)";
            _lblActorNombre.ForeColor = ColorConstants.TextoSecundario;

            AgregarEvento("🛡 ATAQUE DESACTIVADO — Políticas aplicadas");

            _timerAtaque?.Stop();

            // Limpiar pasos pendientes
            _flowPasos.Controls.Clear();
            _pasosPendientes.Clear();
            _pasoActual = null;

            // Mostrar mensaje final
            AgregarPaso("🛡", "Sistema protegido", ColorConstants.AcentoExito);
        }

        private void TimerAtaque_Tick(object? sender, EventArgs e)
        {
            // Si no hay pasos pendientes, generar uno nuevo
            if (_pasosPendientes.Count == 0)
            {
                GenerarNuevoPaso();
            }

            // Mostrar el siguiente paso
            if (_pasosPendientes.Count > 0)
            {
                var paso = _pasosPendientes.Dequeue();
                MostrarPaso(paso);
                _pasoActual = paso;

                // Agregar a la lista de eventos
                AgregarEvento($"{paso.Icono} {paso.Texto}");
            }
        }

        private void GenerarNuevoPaso()
        {
            _contadorPasos++;
            int numPedido = _contadorPasos;

            // Decidir aleatoriamente si es bloqueado o aceptado (probabilidades del ataque)
            bool bloqueado = _random.Next(100) < 40; // 40% bloqueado, 60% aceptado

            // Secuencia de pasos
            var pasos = new List<AtaquePaso>
            {
                new AtaquePaso { Icono = "👤", Texto = $"Atacante genera pedido falso #{numPedido}", Color = ColorConstants.AlertaAtaque },
                new AtaquePaso { Icono = "📦", Texto = $"Intenta ingresar #{numPedido} al Buffer", Color = ColorConstants.EstadoEsperando },
                new AtaquePaso { Icono = "🛡", Texto = "Política de seguridad revisa", Color = ColorConstants.AcentoPrincipal }
            };

            if (bloqueado)
            {
                pasos.Add(new AtaquePaso { Icono = "🚫", Texto = $"Pedido #{numPedido} RECHAZADO", Color = ColorConstants.AlertaAtaque });
                pasos.Add(new AtaquePaso { Icono = "🗑️", Texto = $"Pedido #{numPedido} eliminado", Color = ColorConstants.TextoHint });
            }
            else
            {
                pasos.Add(new AtaquePaso { Icono = "✅", Texto = $"Pedido #{numPedido} ACEPTADO", Color = ColorConstants.AcentoExito });
                pasos.Add(new AtaquePaso { Icono = "👨‍🍳", Texto = $"Cocinero procesa #{numPedido}", Color = ColorConstants.AcentoSecundario });
            }

            foreach (var p in pasos)
                _pasosPendientes.Enqueue(p);
        }

        private void MostrarPaso(AtaquePaso paso)
        {
            // Limpiar el flow y agregar solo el paso actual (para mantener la vista simple)
            _flowPasos.Controls.Clear();

            var panelPaso = new Panel
            {
                AutoSize = false,
                Size = new Size(_flowPasos.Width - 12, 40),
                BackColor = Color.FromArgb(40, paso.Color),
                Margin = new Padding(4)
            };
            panelPaso.Paint += (s, e) =>
            {
                var rect = new Rectangle(0, 0, panelPaso.Width - 1, panelPaso.Height - 1);
                using var pen = new Pen(Color.FromArgb(180, paso.Color), 1);
                e.Graphics.DrawRectangle(pen, rect);
            };

            var lblIcono = new Label
            {
                Text = paso.Icono,
                Font = new Font("Segoe UI Emoji", 18f),
                ForeColor = paso.Color,
                BackColor = Color.Transparent,
                AutoSize = false,
                Size = new Size(40, 40),
                TextAlign = ContentAlignment.MiddleCenter,
                Location = new Point(4, 0)
            };
            var lblTexto = new Label
            {
                Text = paso.Texto,
                Font = AppTheme.FuenteLabelBold,
                ForeColor = paso.Color,
                BackColor = Color.Transparent,
                AutoSize = false,
                Size = new Size(panelPaso.Width - 52, 40),
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(44, 0)
            };

            panelPaso.Controls.Add(lblIcono);
            panelPaso.Controls.Add(lblTexto);

            // Ajustar ancho al tamaño del flow
            panelPaso.Size = new Size(_flowPasos.Width - 12, 40);
            _flowPasos.Controls.Add(panelPaso);
        }

        private void AgregarPaso(string icono, string texto, Color color)
        {
            var paso = new AtaquePaso { Icono = icono, Texto = texto, Color = color };
            MostrarPaso(paso);
        }

        private void AgregarEvento(string mensaje)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            string linea = $"{timestamp}  {mensaje}";
            _listEventos.Items.Insert(0, linea);
            if (_listEventos.Items.Count > 200)
                _listEventos.Items.RemoveAt(_listEventos.Items.Count - 1);
        }

        // ─── CIERRE ──────────────────────────────────────────────────────────

        private void CerrarVentana()
        {
            _timerAtaque?.Stop();
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

        // ─── CLASE AUXILIAR ─────────────────────────────────────────────────

        private sealed class AtaquePaso
        {
            public string Icono { get; init; } = "";
            public string Texto { get; init; } = "";
            public Color Color { get; init; }
        }
    }
}