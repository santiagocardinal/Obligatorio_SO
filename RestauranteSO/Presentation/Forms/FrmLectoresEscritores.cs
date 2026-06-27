// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Presentation/Forms/FrmLectoresEscritores.cs
// Propósito: Formulario de simulación Lectores-Escritores.
//            Visualiza el ReaderWriterLockSlim, meseros y gerente en tiempo real.
// SOLID    : SRP, DIP.
// =============================================================================

using RestauranteSO.Constants;
using RestauranteSO.Domain.Enums;
using RestauranteSO.Domain.Interfaces;
using RestauranteSO.Presentation.Components;
using RestauranteSO.Presentation.Controls;
using RestauranteSO.Presentation.Themes;
using RestauranteSO.Services.Ataques;
using RestauranteSO.Services.LectoresEscritores;

namespace RestauranteSO.Presentation.Forms
{
    public sealed class FrmLectoresEscritores : Form
    {
        // ─── SERVICIOS ────────────────────────────────────────────────────────

        private readonly LectoresEscritoresService _service;
        private readonly AtaqueLectoresEscritoresService _attackService;
        private readonly ISimulationLogger _logger;

        // ─── CONTROLES ────────────────────────────────────────────────────────

        private Panel _panelHeader = null!;
        private Label _lblTitulo = null!;
        private StatusBadge _badgeEstado = null!;

        private Panel _panelToolbar = null!;
        private Button _btnIniciar = null!;
        private Button _btnPausar = null!;
        private Button _btnDetener = null!;
        private Button _btnAgregarLector = null!;
        private Button _btnActivarAtaque = null!;
        private TrackBar _trackVelLector = null!;
        private TrackBar _trackVelEscritor = null!;
        private Label _lblVelL = null!;
        private Label _lblVelE = null!;

        // Panel izquierdo: estado del lock + meseros
        private Panel _panelIzq = null!;
        private SemaphoreVisualizer _vizRWLock = null!;
        private Panel _panelGerenteVisual = null!;
        private Label _lblGerenteEstado = null!;
        private Label _lblGerenteAccion = null!;
        private FlowLayoutPanel _flowMeseros = null!;

        // Panel central: menú en tiempo real
        private Panel _panelMenu = null!;
        private DataGridView _gridMenu = null!;
        private Label _lblMenuTitulo = null!;
        private Label _lblMenuVersion = null!;

        // Panel derecho: estadísticas + log
        private Panel _panelDer = null!;
        private Label _lblStatLecturas = null!;
        private Label _lblStatEscrituras = null!;
        private Label _lblStatComprometidos = null!;
        private Label _lblStatTiempo = null!;
        private LogViewer _logViewer = null!;

        // Panel ataque
        private Panel _panelAtaque = null!;
        private Label _lblAtaqueTitulo = null!;
        private Label _lblAtaqueDesc = null!;
        private Button _btnVerPoliticas = null!;

        private StatusStrip _statusStrip = null!;
        private ToolStripStatusLabel _lblStatus = null!;
        private ToolStripStatusLabel _lblLockStatus = null!;

        private System.Windows.Forms.Timer _timerUI = null!;

        public event EventHandler<bool>? SimulacionCambiada;

        // ─── CONSTRUCTOR ─────────────────────────────────────────────────────

        public FrmLectoresEscritores(
            LectoresEscritoresService service,
            AtaqueLectoresEscritoresService attackService,
            ISimulationLogger logger)
        {
            _service       = service       ?? throw new ArgumentNullException(nameof(service));
            _attackService = attackService ?? throw new ArgumentNullException(nameof(attackService));
            _logger        = logger        ?? throw new ArgumentNullException(nameof(logger));

            InitializeComponent();
            ConfigurarEventos();
        }

        // ─── INIT ─────────────────────────────────────────────────────────────

        private void InitializeComponent()
        {
            SuspendLayout();

            Text          = "📖 Simulación Lectores — Escritores | RestauranteSO";
            Size          = new Size(1300, 820);
            MinimumSize   = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor     = ColorConstants.FondoPrincipal;
            ForeColor     = ColorConstants.TextoPrincipal;
            Font          = AppTheme.FuenteLabel;

            ConstruirHeader();
            ConstruirToolbar();
            ConstruirCuerpo();
            ConstruirStatusStrip();

            _timerUI = new System.Windows.Forms.Timer
                { Interval = AppConstants.IntervalActualizacionUIMs };
            _timerUI.Tick += TimerUI_Tick;

            ResumeLayout(true);
        }

        private void ConstruirHeader()
        {
            _panelHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 52,
                BackColor = ColorConstants.FondoSuperior
            };
            _panelHeader.Paint += (_, e) =>
            {
                using var pen = new Pen(ColorConstants.TarjetaLectores, 2);
                e.Graphics.DrawLine(pen, 0, _panelHeader.Height - 2,
                    _panelHeader.Width, _panelHeader.Height - 2);
            };

            _lblTitulo = new Label
            {
                Text      = "📖  Simulación Lectores — Escritores",
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = ColorConstants.TextoPrincipal,
                AutoSize  = true,
                Location  = new Point(16, 12)
            };

            _badgeEstado = new StatusBadge
            {
                Text        = "● Detenida",
                ColorAcento = ColorConstants.TextoHint,
                Size        = new Size(130, 24),
                Anchor      = AnchorStyles.Top | AnchorStyles.Right
            };
            _badgeEstado.Location = new Point(_panelHeader.Width - 150, 14);
            _panelHeader.Resize += (_, _) =>
                _badgeEstado.Location = new Point(_panelHeader.Width - 150, 14);

            _panelHeader.Controls.AddRange(
                new Control[] { _lblTitulo, _badgeEstado });
            Controls.Add(_panelHeader);
        }

        private void ConstruirToolbar()
        {
            _panelToolbar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 52,
                BackColor = ColorConstants.FondoPanel,
                Padding   = new Padding(8)
            };
            _panelToolbar.Paint += (_, e) =>
            {
                using var pen = new Pen(ColorConstants.Separador, 1);
                e.Graphics.DrawLine(pen, 0, _panelToolbar.Height - 1,
                    _panelToolbar.Width, _panelToolbar.Height - 1);
            };

            int x = 8;

            _btnIniciar = CrearBtn("▶ Iniciar",  x, 10, 90, true);  x += 98;
            _btnPausar  = CrearBtn("⏸ Pausar",   x, 10, 85, false); x += 93;
            _btnDetener = CrearBtn("⏹ Detener",  x, 10, 85, false); x += 105;

            var sep = new Panel { Location = new Point(x, 8),
                Size = new Size(1, 34), BackColor = ColorConstants.Separador };
            x += 12;

            _btnAgregarLector = CrearBtn("+ Mesero", x, 10, 90, false); x += 100;

            var sep2 = new Panel { Location = new Point(x, 8),
                Size = new Size(1, 34), BackColor = ColorConstants.Separador };
            x += 12;

            var lblVel = new Label
            {
                Text      = "Velocidad:",
                Font      = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.TextoHint,
                AutoSize  = true,
                Location  = new Point(x, 18)
            };
            x += 62;

            _lblVelL = new Label { Text = "Lect", Font = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.TextoSecundario, AutoSize = true,
                Location = new Point(x, 8) };
            _trackVelLector = new TrackBar { Minimum = 1, Maximum = 10, Value = 5,
                TickStyle = TickStyle.None, Location = new Point(x, 20),
                Size = new Size(90, 26), BackColor = ColorConstants.FondoPanel };
            x += 98;

            _lblVelE = new Label { Text = "Escr", Font = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.TextoSecundario, AutoSize = true,
                Location = new Point(x, 8) };
            _trackVelEscritor = new TrackBar { Minimum = 1, Maximum = 10, Value = 3,
                TickStyle = TickStyle.None, Location = new Point(x, 20),
                Size = new Size(90, 26), BackColor = ColorConstants.FondoPanel };

            _btnActivarAtaque = new Button
            {
                Text      = "🎣 Simular Phishing",
                Font      = AppTheme.FuenteBoton,
                BackColor = ColorConstants.AlertaAtaque,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                Size      = new Size(150, 32),
                Anchor    = AnchorStyles.Top | AnchorStyles.Right
            };
            _btnActivarAtaque.FlatAppearance.BorderSize = 0;
            _btnActivarAtaque.Location =
                new Point(_panelToolbar.Width - 166, 10);
            _panelToolbar.Resize += (_, _) =>
                _btnActivarAtaque.Location =
                    new Point(_panelToolbar.Width - 166, 10);

            _panelToolbar.Controls.AddRange(new Control[]
            {
                _btnIniciar, _btnPausar, _btnDetener, sep,
                _btnAgregarLector, sep2, lblVel,
                _lblVelL, _trackVelLector,
                _lblVelE, _trackVelEscritor,
                _btnActivarAtaque
            });
            Controls.Add(_panelToolbar);
        }

        private void ConstruirCuerpo()
        {
            var panelCuerpo = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = ColorConstants.FondoPrincipal
            };

            // Panel izquierdo (30%): Lock + Gerente + Meseros
            _panelIzq = new Panel
            {
                Dock      = DockStyle.Left,
                BackColor = ColorConstants.FondoLateral,
                Padding   = new Padding(8)
            };

            // Panel central (40%): Menú en tiempo real
            _panelMenu = new Panel
            {
                Dock      = DockStyle.Left,
                BackColor = ColorConstants.FondoPrincipal,
                Padding   = new Padding(8)
            };

            // Panel derecho (30%): Stats + Log
            _panelDer = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = ColorConstants.FondoLateral,
                Padding   = new Padding(8)
            };

            panelCuerpo.Resize += (_, _) =>
            {
                _panelIzq.Width  = (int)(panelCuerpo.Width * 0.28);
                _panelMenu.Width = (int)(panelCuerpo.Width * 0.42);
            };

            ConstruirPanelIzquierdo();
            ConstruirPanelMenu();
            ConstruirPanelDerecho();
            ConstruirPanelAtaque();

            panelCuerpo.Controls.Add(_panelDer);
            panelCuerpo.Controls.Add(_panelMenu);
            panelCuerpo.Controls.Add(_panelIzq);
            Controls.Add(panelCuerpo);
        }

        private void ConstruirPanelIzquierdo()
        {
            // ── Visualizador RWLock ───────────────────────────────────────
            var lblRW = new Label
            {
                Text      = "🔐 ReaderWriterLockSlim",
                Font      = AppTheme.FuenteLabelBold,
                ForeColor = ColorConstants.TarjetaLectores,
                Dock      = DockStyle.Top,
                Height    = 24,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(2, 0, 0, 0)
            };

            _vizRWLock = new SemaphoreVisualizer
            {
                Dock   = DockStyle.Top,
                Height = 108
            };
            _vizRWLock.ActualizarReaderWriterLock(0, 0, false, false);

            // ── Gerente Visual ────────────────────────────────────────────
            _panelGerenteVisual = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 80,
                BackColor = ColorConstants.FondoCard,
                Padding   = new Padding(8),
                Margin    = new Padding(0, 8, 0, 0)
            };
            _panelGerenteVisual.Paint += (_, e) =>
            {
                var color = _service.GerenteEscribiendo
                    ? ColorConstants.AlertaAtaque
                    : ColorConstants.Separador;
                using var pen = new Pen(color, 2);
                e.Graphics.DrawRectangle(pen, 1, 1,
                    _panelGerenteVisual.Width - 3,
                    _panelGerenteVisual.Height - 3);
            };

            var lblGerenteTit = new Label
            {
                Text      = "👔 Gerente (Escritor)",
                Font      = AppTheme.FuenteLabelBold,
                ForeColor = ColorConstants.AcentoSecundario,
                AutoSize  = true,
                Location  = new Point(8, 8)
            };

            _lblGerenteEstado = new Label
            {
                Text      = "⏳ En espera",
                Font      = AppTheme.FuenteLabel,
                ForeColor = ColorConstants.TextoHint,
                AutoSize  = true,
                Location  = new Point(8, 30)
            };

            _lblGerenteAccion = new Label
            {
                Text      = "—",
                Font      = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.TextoHint,
                AutoSize  = false,
                Size      = new Size(180, 28),
                Location  = new Point(8, 50)
            };

            _panelGerenteVisual.Controls.AddRange(new Control[]
            {
                lblGerenteTit, _lblGerenteEstado, _lblGerenteAccion
            });

            // Separador
            var sep = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 1,
                BackColor = ColorConstants.Separador,
                Margin    = new Padding(0, 4, 0, 4)
            };

            // ── Meseros ───────────────────────────────────────────────────
            var lblMeseros = new Label
            {
                Text      = "🧑‍🍽 Meseros (Lectores)",
                Font      = AppTheme.FuenteLabelBold,
                ForeColor = ColorConstants.TarjetaLectores,
                Dock      = DockStyle.Top,
                Height    = 24,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(2, 0, 0, 0)
            };

            _flowMeseros = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents  = false,
                AutoScroll    = true,
                BackColor     = ColorConstants.FondoPanel,
                Padding       = new Padding(4)
            };

            _panelIzq.Controls.Add(_flowMeseros);
            _panelIzq.Controls.Add(lblMeseros);
            _panelIzq.Controls.Add(sep);
            _panelIzq.Controls.Add(_panelGerenteVisual);
            _panelIzq.Controls.Add(_vizRWLock);
            _panelIzq.Controls.Add(lblRW);
        }

        private void ConstruirPanelMenu()
        {
            _lblMenuTitulo = new Label
            {
                Text      = "📋 Menú del Restaurante (Recurso Compartido)",
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = ColorConstants.TextoSecundario,
                Dock      = DockStyle.Top,
                Height    = 28,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(4, 0, 0, 0)
            };

            _lblMenuVersion = new Label
            {
                Text      = "Versión del menú: —",
                Font      = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.TextoHint,
                Dock      = DockStyle.Top,
                Height    = 20,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(4, 0, 0, 0)
            };

            _gridMenu = new DataGridView
            {
                Dock = DockStyle.Fill
            };
            AppTheme.AplicarADataGrid(_gridMenu);

            _gridMenu.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Id", HeaderText = "#", Width = 32, FillWeight = 5
            });
            _gridMenu.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Nombre", HeaderText = "Plato", FillWeight = 35
            });
            _gridMenu.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Precio", HeaderText = "Precio", Width = 70, FillWeight = 12
            });
            _gridMenu.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Cat", HeaderText = "Categoría", FillWeight = 18
            });
            _gridMenu.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Version", HeaderText = "Ver.", Width = 40, FillWeight = 6
            });
            _gridMenu.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "ModPor", HeaderText = "Modificado por", FillWeight = 24
            });

            _panelMenu.Controls.Add(_gridMenu);
            _panelMenu.Controls.Add(_lblMenuVersion);
            _panelMenu.Controls.Add(_lblMenuTitulo);
        }

        private void ConstruirPanelDerecho()
        {
            var lblStats = new Label
            {
                Text      = "📊 Estadísticas",
                Font      = AppTheme.FuenteLabelBold,
                ForeColor = ColorConstants.TextoSecundario,
                Dock      = DockStyle.Top,
                Height    = 24,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(4, 0, 0, 0)
            };

            var panelStats = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 92,
                BackColor = ColorConstants.FondoPanel,
                Padding   = new Padding(8)
            };

            _lblStatLecturas       = CrearLabelStat("📖 Lecturas: 0");
            _lblStatEscrituras     = CrearLabelStat("✏ Escrituras: 0");
            _lblStatComprometidos  = CrearLabelStat("⚡ Comprometidos: 0");
            _lblStatTiempo         = CrearLabelStat("⏱ Tiempo: 00:00");

            var flowStats = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                BackColor     = Color.Transparent
            };
            flowStats.Controls.AddRange(new Control[]
            {
                _lblStatLecturas, _lblStatEscrituras,
                _lblStatComprometidos, _lblStatTiempo
            });
            panelStats.Controls.Add(flowStats);

            var sep = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 1,
                BackColor = ColorConstants.Separador
            };

            _logViewer = new LogViewer { Dock = DockStyle.Fill };
            _logViewer.SetTitulo("📋 Log de ReaderWriterLockSlim");

            _panelDer.Controls.Add(_logViewer);
            _panelDer.Controls.Add(sep);
            _panelDer.Controls.Add(panelStats);
            _panelDer.Controls.Add(lblStats);
        }

        private void ConstruirPanelAtaque()
        {
            _panelAtaque = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 80,
                BackColor = ColorConstants.FondoAtaque,
                Visible   = false,
                Padding   = new Padding(12, 8, 12, 8)
            };
            _panelAtaque.Paint += (_, e) =>
            {
                using var pen = new Pen(ColorConstants.AlertaAtaque, 2);
                e.Graphics.DrawRectangle(pen, 1, 1,
                    _panelAtaque.Width - 3, _panelAtaque.Height - 3);
            };

            _lblAtaqueTitulo = new Label
            {
                Text      = "🎣 ATAQUE ACTIVO: Phishing al Gerente — Menú Comprometido",
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = ColorConstants.AlertaAtaque,
                AutoSize  = true,
                Location  = new Point(12, 10)
            };

            _lblAtaqueDesc = new Label
            {
                Text =
                    "El Gerente entregó sus credenciales a través de un correo falso. " +
                    "El menú fue alterado. Los meseros están leyendo información incorrecta.",
                Font      = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.TextoSecundario,
                AutoSize  = false,
                Location  = new Point(12, 34),
                Size      = new Size(700, 36)
            };

            _btnVerPoliticas = new Button
            {
                Text      = "🛡 Ver Políticas de Prevención",
                Font      = AppTheme.FuenteBoton,
                BackColor = ColorConstants.AcentoExito,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                Size      = new Size(220, 32),
                Anchor    = AnchorStyles.Top | AnchorStyles.Right
            };
            _btnVerPoliticas.FlatAppearance.BorderSize = 0;
            _btnVerPoliticas.Location =
                new Point(_panelAtaque.Width - 236, 24);
            _panelAtaque.Resize += (_, _) =>
                _btnVerPoliticas.Location =
                    new Point(_panelAtaque.Width - 236, 24);
            _btnVerPoliticas.Click += (_, _) => AbrirPoliticas();

            _panelAtaque.Controls.AddRange(new Control[]
            {
                _lblAtaqueTitulo, _lblAtaqueDesc, _btnVerPoliticas
            });
            Controls.Add(_panelAtaque);
        }

        private void ConstruirStatusStrip()
        {
            _statusStrip = new StatusStrip();
            AppTheme.AplicarAStatusStrip(_statusStrip);

            _lblStatus = new ToolStripStatusLabel
            {
                Text   = "Listo — presione Iniciar",
                Spring = true,
                ForeColor = ColorConstants.TextoSecundario
            };

            _lblLockStatus = new ToolStripStatusLabel
            {
                Text      = "Lock: Libre",
                ForeColor = ColorConstants.AcentoExito,
                Font      = new Font("Consolas", 8.5f)
            };

            _statusStrip.Items.AddRange(
                new ToolStripItem[] { _lblStatus, _lblLockStatus });
            Controls.Add(_statusStrip);
        }

        // ─── EVENTOS ─────────────────────────────────────────────────────────

        private void ConfigurarEventos()
        {
            _btnIniciar.Click += (_, _) => Iniciar();
            _btnPausar.Click  += (_, _) => PausarReanudar();
            _btnDetener.Click += (_, _) => Detener();
            _btnAgregarLector.Click += (_, _) => _service.AgregarLector();
            _btnActivarAtaque.Click += (_, _) => ToggleAtaque();

            _trackVelLector.ValueChanged += (_, _) =>
            {
                _service.AjustarVelocidad(VelAMs(_trackVelLector.Value));
                _lblVelL.Text = $"Lect {_trackVelLector.Value}x";
            };

            _service.EstadoCambiado += (_, estado) =>
            {
                if (InvokeRequired)
                { BeginInvoke(() => OnEstadoCambiado(estado)); return; }
                OnEstadoCambiado(estado);
            };

            _attackService.AtaqueActivado += (_, _) =>
            {
                if (InvokeRequired) { BeginInvoke(MostrarAtaque); return; }
                MostrarAtaque();
            };
            _attackService.AtaqueDesactivado += (_, _) =>
            {
                if (InvokeRequired) { BeginInvoke(OcultarAtaque); return; }
                OcultarAtaque();
            };

            _logger.NuevoLogAgregado += (_, entry) =>
                _logViewer.AgregarLog(entry);

            FormClosing += (_, _) =>
            {
                _timerUI.Stop();
                if (_service.EstaCorreindo) _service.Detener();
                if (_attackService.IsAttackActive) _attackService.DesactivarAtaque();
            };
        }

        // ─── ACCIONES ─────────────────────────────────────────────────────────

        private void Iniciar()
        {
            try
            {
                _service.Iniciar();
                _timerUI.Start();
                ActualizarBotones(SimulationStatus.Corriendo);
                SimulacionCambiada?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al iniciar: {ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PausarReanudar()
        {
            if (_service.Estado == SimulationStatus.Corriendo)
            {
                _service.Pausar();
                _btnPausar.Text = "▶ Reanudar";
            }
            else if (_service.Estado == SimulationStatus.Pausada)
            {
                _service.Reanudar();
                _btnPausar.Text = "⏸ Pausar";
            }
        }

        private void Detener()
        {
            _timerUI.Stop();
            _service.Detener();
            if (_attackService.IsAttackActive)
                _attackService.DesactivarAtaque();
            ActualizarBotones(SimulationStatus.Detenida);
            SimulacionCambiada?.Invoke(this, false);
            _gridMenu.Rows.Clear();
            _flowMeseros.Controls.Clear();
        }

        private void ToggleAtaque()
        {
            if (!_service.EstaCorreindo)
            {
                MessageBox.Show("Inicie la simulación primero.",
                    "Simulación no activa",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (_attackService.IsAttackActive)
            {
                _attackService.DesactivarAtaque();
                _btnActivarAtaque.Text      = "🎣 Simular Phishing";
                _btnActivarAtaque.BackColor = ColorConstants.AlertaAtaque;
            }
            else
            {
                MostrarDialogoPhishing();
            }
        }

        private void MostrarDialogoPhishing()
        {
            using var dlg = new FrmIngenieriaSocial(
                "📧 Correo — Sistema de Gestión Restaurante",
                "De: sistema@gestionresto-soporte.com\n" +
                "Para: gerente@restaurantedoncod.com\n" +
                "Asunto: ⚠ URGENTE: Su acceso expirará en 24 horas\n\n" +
                "Estimado Gerente:\n\n" +
                "Su sesión en el Sistema de Gestión del Restaurante expirará\n" +
                "mañana a las 08:00 AM si no renueva sus credenciales.\n\n" +
                "Haga clic en el botón de abajo para verificar su identidad\n" +
                "y mantener el acceso al sistema de menús.",
                "✉ Verificar mis credenciales",
                "Ignorar correo");

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                _attackService.ActivarAtaque(AttackType.PhishingMenuAlterado);
                _btnActivarAtaque.Text      = "🛡 Desactivar Ataque";
                _btnActivarAtaque.BackColor = ColorConstants.AcentoExito;
            }
        }

        private void AbrirPoliticas()
        {
            var politicas =
                AtaqueLectoresEscritoresService.ObtenerPoliticas();
            using var frm = new FrmPoliticas(
                "Políticas de Prevención — Phishing al Gerente",
                politicas,
                _attackService.HistorialAtaques);
            frm.ShowDialog(this);
        }

        // ─── ACTUALIZACIÓN UI ─────────────────────────────────────────────────

        private void TimerUI_Tick(object? sender, EventArgs e)
        {
            if (_service.Estado == SimulationStatus.Detenida) return;

            var stats = _service.ObtenerEstadisticas();
            ActualizarEstadisticas(stats);
            ActualizarGridMenu();
            ActualizarGerenteVisual();
            ActualizarMeseros();
            ActualizarVisualizador(stats);
        }

        private void ActualizarEstadisticas(
            Domain.Models.SimulationStatistics stats)
        {
            _lblStatLecturas.Text =
                $"📖 Lecturas: {stats.TotalLecturas}";
            _lblStatEscrituras.Text =
                $"✏ Escrituras: {stats.TotalEscrituras}";
            _lblStatComprometidos.Text =
                $"⚡ Comprometidos: {stats.LectoresComprometidos}";
            _lblStatTiempo.Text =
                $"⏱ Tiempo: {stats.TiempoEjecucion:mm\\:ss}";

            _lblLockStatus.Text = stats.EscritorActivo
                ? "🔴 Lock: ESCRITURA EXCLUSIVA"
                : stats.LectoresActivos > 0
                    ? $"🟢 Lock: {stats.LectoresActivos} leyendo"
                    : "⚪ Lock: Libre";

            _lblLockStatus.ForeColor = stats.EscritorActivo
                ? ColorConstants.AlertaAtaque
                : stats.LectoresActivos > 0
                    ? ColorConstants.AcentoExito
                    : ColorConstants.TextoHint;
        }

        private void ActualizarGridMenu()
        {
            _gridMenu.SuspendLayout();
            _gridMenu.Rows.Clear();

            // Necesitamos acceder al menú — lo obtenemos del servicio
            // a través de su propiedad pública (sin ReadLock porque es solo UI)
            // El servicio expone un snapshot seguro
            try
            {
                // Usamos reflexión mínima: accedemos al repo vía la interfaz
                // ya que el servicio no expone el menú directamente.
                // Solución: el repo es Singleton, lo podemos resolver.
                var menuRepo = Configuration.AppSettings
                    .Resolver<Domain.Interfaces.IMenuRepository>();

                var items = menuRepo.ObtenerCompleto();
                int versionMax = items.Count > 0
                    ? items.Max(i => i.Version) : 0;
                _lblMenuVersion.Text =
                    $"Versión actual del menú: {versionMax} " +
                    (_attackService.IsAttackActive
                        ? "⚠ COMPROMETIDA" : "✅ Íntegra");

                foreach (var item in items)
                {
                    int idx = _gridMenu.Rows.Add(
                        item.Id,
                        item.Nombre.Length > 28
                            ? item.Nombre[..28] + "…" : item.Nombre,
                        $"${item.Precio:N2}",
                        item.Categoria,
                        $"v{item.Version}",
                        item.ModificadoPor);

                    var row = _gridMenu.Rows[idx];
                    if (item.FueAlterado)
                    {
                        row.DefaultCellStyle.BackColor =
                            Color.FromArgb(60, 220, 50, 50);
                        row.DefaultCellStyle.ForeColor =
                            ColorConstants.EstadoAlterado;
                    }
                    else if (item.ModificadoPor != "Sistema")
                    {
                        row.DefaultCellStyle.ForeColor =
                            ColorConstants.AcentoSecundario;
                    }
                }
            }
            catch { /* ignorar en caso de race condition transitoria */ }

            _gridMenu.ResumeLayout();
        }

        private void ActualizarGerenteVisual()
        {
            bool escribiendo = _service.GerenteEscribiendo;
            bool esperando   = _service.EscritorEsperando;

            _lblGerenteEstado.Text = escribiendo
                ? "✏ ESCRIBIENDO (WriteLock exclusivo)"
                : esperando
                    ? "⏳ Esperando WriteLock..."
                    : "💤 En espera";

            _lblGerenteEstado.ForeColor = escribiendo
                ? ColorConstants.AlertaAtaque
                : esperando
                    ? ColorConstants.EstadoEsperando
                    : ColorConstants.TextoHint;

            string ultima = _service.UltimaModificacion ?? "—";
            _lblGerenteAccion.Text = ultima.Length > 35
                ? ultima[..35] + "…" : ultima;

            _panelGerenteVisual.Invalidate();
        }

        private void ActualizarMeseros()
        {
            var meseros = _service.Meseros;

            while (_flowMeseros.Controls.Count < meseros.Count)
            {
                var ind = new ThreadStatusIndicator
                {
                    Width  = _flowMeseros.Width - 12,
                    Anchor = AnchorStyles.Left | AnchorStyles.Right
                };
                _flowMeseros.Controls.Add(ind);
            }

            for (int i = 0; i < meseros.Count; i++)
            {
                var m   = meseros[i];
                var ind = (ThreadStatusIndicator)_flowMeseros.Controls[i];

                var estado = m.LeyoMenuComprometido
                    ? ThreadStatusIndicator.EstadoHilo.BajoAtaque
                    : m.EstaLeyendo
                        ? ThreadStatusIndicator.EstadoHilo.Activo
                        : m.EstaEsperando
                            ? ThreadStatusIndicator.EstadoHilo.Esperando
                            : ThreadStatusIndicator.EstadoHilo.Detenido;

                string accion = m.LeyoMenuComprometido
                    ? $"⚠ Leyó info comprometida v{m.VersionMenuLeida}"
                    : m.EstaLeyendo
                        ? $"📖 Leyendo: {m.UltimoItemLeido ?? "—"}"
                        : m.EstaEsperando
                            ? "⏳ Esperando ReadLock..."
                            : $"✓ {m.LecturasCompletadas} lecturas";

                ind.ActualizarEstado(m.Id, estado, accion);
            }
        }

        private void ActualizarVisualizador(
            Domain.Models.SimulationStatistics stats)
        {
            _vizRWLock.ActualizarReaderWriterLock(
                stats.LectoresActivos,
                stats.LectoresEsperando,
                stats.EscritorActivo,
                stats.EscritorEsperando);
        }

        private void OnEstadoCambiado(SimulationStatus estado)
        {
            ActualizarBotones(estado);
            (_badgeEstado.Text, _badgeEstado.ColorAcento) = estado switch
            {
                SimulationStatus.Corriendo  =>
                    ("▶ Corriendo", ColorConstants.AcentoExito),
                SimulationStatus.Pausada    =>
                    ("⏸ Pausada", ColorConstants.EstadoEsperando),
                SimulationStatus.BajoAtaque =>
                    ("🎣 Bajo Ataque", ColorConstants.AlertaAtaque),
                _                          =>
                    ("● Detenida", ColorConstants.TextoHint)
            };
        }

        private void ActualizarBotones(SimulationStatus estado)
        {
            bool corriendo = estado == SimulationStatus.Corriendo ||
                             estado == SimulationStatus.BajoAtaque;
            bool pausada   = estado == SimulationStatus.Pausada;
            bool detenida  = estado == SimulationStatus.Detenida;

            _btnIniciar.Enabled       = detenida;
            _btnPausar.Enabled        = corriendo || pausada;
            _btnDetener.Enabled       = !detenida;
            _btnAgregarLector.Enabled = corriendo || pausada;
            _btnActivarAtaque.Enabled = corriendo;

            AppTheme.AplicarABotonPrimario(_btnIniciar);
            if (!_btnIniciar.Enabled)
            {
                _btnIniciar.BackColor = ColorConstants.Separador;
                _btnIniciar.ForeColor = ColorConstants.TextoHint;
            }
        }

        private void MostrarAtaque()
        {
            _panelAtaque.Visible = true;
        }

        private void OcultarAtaque()
        {
            _panelAtaque.Visible = false;
        }

        private Label CrearLabelStat(string texto) => new Label
        {
            Text      = texto,
            Font      = AppTheme.FuenteLabelBold,
            ForeColor = ColorConstants.TextoSecundario,
            AutoSize  = true,
            Margin    = new Padding(2, 2, 2, 2)
        };

        private static Button CrearBtn(
            string texto, int x, int y, int w, bool primario)
        {
            var btn = new Button
            {
                Text     = texto,
                Location = new Point(x, y),
                Size     = new Size(w, 32)
            };
            if (primario) AppTheme.AplicarABotonPrimario(btn);
            else          AppTheme.AplicarABotonSecundario(btn);
            return btn;
        }

        private static int VelAMs(int v) =>
            (int)(3000 / Math.Pow(v, 0.8));
    }
}