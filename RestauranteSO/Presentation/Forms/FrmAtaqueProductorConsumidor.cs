using System.Drawing.Drawing2D;
using System.Drawing.Text;
using RestauranteSO.Constants;
using RestauranteSO.Domain.Enums;
using RestauranteSO.Domain.Interfaces;
using RestauranteSO.Presentation.Controls;
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

        private Panel            _panelHeader       = null!;
        private Panel            _panelCuerpo       = null!;
        private Panel            _panelProductores  = null!;
        private Panel            _panelBuffer       = null!;
        private Panel            _panelConsumidores = null!;
        private Panel            _panelStats        = null!;
        private Panel            _panelLog          = null!;
        private BufferVisualizer _bufferViz         = null!;
        private Panel            _flowActoresP      = null!;
        private Panel            _flowActoresC      = null!;
        private Label            _lblEstadoAtaque   = null!;
        private Label            _lblStatPed        = null!;
        private Label            _lblStatMal        = null!;
        private Label            _lblStatProc       = null!;
        private Label            _lblStatThrp       = null!;
        private Panel            _panelActivityFeed = null!;

        private readonly List<(string Hora, Color Color, string Msg)> _logEntries = new();

        private System.Windows.Forms.Timer _timerUI   = null!;
        private System.Windows.Forms.Timer _timerAnim = null!;
        private float _animOffset   = 0f;
        private bool  _ataqueActivo = false;

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
            Text          = "⚡ Inyección de Pedidos | RestaurantOS";
            WindowState   = FormWindowState.Maximized;
            MinimumSize   = new Size(1200, 750);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor     = ColorConstants.FondoPrincipal;

            ConstruirHeader();
            ConstruirCuerpo();
            ConstruirStats();

            _timerUI = new System.Windows.Forms.Timer
                { Interval = AppConstants.IntervalActualizacionUIMs };
            _timerUI.Tick += TimerUI_Tick;

            _timerAnim = new System.Windows.Forms.Timer { Interval = 32 };
            _timerAnim.Tick += (_, _) =>
            {
                _animOffset += 0.04f;
                if (_animOffset > MathF.Tau) _animOffset -= MathF.Tau;
                _panelHeader?.Invalidate();
            };
            _timerAnim.Start();

            ResumeLayout(true);

            Load += async (_, _) =>
            {
                await Task.Delay(400);
                _service.Iniciar();
                _timerUI.Start();
                AgregarLog("🟢",
                    "Simulación iniciada — esperando activación del ataque",
                    ColorConstants.AcentoExito);

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
                {
                    _attackService.ActivarAtaque(AttackType.InyeccionDePedidos);
                    _ataqueActivo = true;
                    AgregarLog("⚡",
                        "ATAQUE ACTIVADO — Agente malicioso inyectado en la cola",
                        ColorConstants.AlertaAtaque);
                }
                else
                {
                    AgregarLog("🛡",
                        "Ataque RECHAZADO — Sistema seguro",
                        ColorConstants.AcentoExito);
                }
            };

            FormClosing += (_, _) =>
            {
                _timerUI?.Stop();
                _timerAnim?.Stop();
                if (_service.EstaCorreindo)        _service.Detener();
                if (_attackService.IsAttackActive) _attackService.DesactivarAtaque();
            };
        }

        // ─── HEADER ──────────────────────────────────────────────────────────

        private void ConstruirHeader()
        {
            _panelHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 56,
                BackColor = Color.FromArgb(16, 8, 8)
            };
            _panelHeader.Paint += Header_Paint;

            var tbl = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 3,
                RowCount    = 1,
                BackColor   = Color.FromArgb(16, 8, 8),
                Padding     = new Padding(16, 0, 16, 0)
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34f));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            var lblTit = new Label
            {
                Text      = "⚡ MÓDULO EDUCATIVO — Productor–Consumidor",
                Font      = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = ColorConstants.AlertaAtaque,
                BackColor = Color.FromArgb(16, 8, 8),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _lblEstadoAtaque = new Label
            {
                Text      = "● Simulación en curso",
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = ColorConstants.AcentoExito,
                BackColor = Color.FromArgb(16, 8, 8),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var btnVolver = new Button
            {
                Text      = "⬅ Volver",
                Width     = 110,
                Height    = 36,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                BackColor = ColorConstants.FondoPanel,
                ForeColor = ColorConstants.TextoSecundario,
                FlatStyle = FlatStyle.Flat,
                Cursor    = Cursors.Hand,
                Anchor    = AnchorStyles.Right
            };
            btnVolver.FlatAppearance.BorderColor = ColorConstants.Separador;
            btnVolver.Click += (_, _) =>
            {
                _timerUI?.Stop();
                _timerAnim?.Stop();
                if (_service.EstaCorreindo)        _service.Detener();
                if (_attackService.IsAttackActive) _attackService.DesactivarAtaque();
                Close();
            };

            var panelBtnVolver = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.FromArgb(16, 8, 8)
            };
            panelBtnVolver.Controls.Add(btnVolver);
            panelBtnVolver.Resize += (_, _) =>
                btnVolver.Location = new Point(panelBtnVolver.Width - 118, 10);

            tbl.Controls.Add(lblTit,           0, 0);
            tbl.Controls.Add(_lblEstadoAtaque, 1, 0);
            tbl.Controls.Add(panelBtnVolver,   2, 0);
            _panelHeader.Controls.Add(tbl);
            Controls.Add(_panelHeader);
        }

        private void Header_Paint(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            using var pen = new Pen(ColorConstants.AlertaAtaque, 2);
            g.DrawLine(pen, 0, _panelHeader.Height - 2,
                _panelHeader.Width, _panelHeader.Height - 2);

            if (_ataqueActivo)
            {
                float pulse = (MathF.Sin(_animOffset * 3f) + 1f) / 2f;
                using var glowPen = new Pen(
                    Color.FromArgb((int)(20 + 30 * pulse),
                        ColorConstants.AlertaAtaque), 6f);
                g.DrawLine(glowPen, 0, _panelHeader.Height - 4,
                    _panelHeader.Width, _panelHeader.Height - 4);
            }
        }

        // ─── CUERPO ───────────────────────────────────────────────────────────

        private void ConstruirCuerpo()
        {
            _panelCuerpo = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = ColorConstants.FondoPrincipal
            };

            _panelProductores = new Panel
            {
                Dock      = DockStyle.Left,
                BackColor = Color.FromArgb(13, 13, 28),
                Padding   = new Padding(10)
            };
            _panelProductores.Paint += (_, e) => PintarPanelProductores(e);

            _panelBuffer = new Panel
            {
                Dock      = DockStyle.Left,
                BackColor = ColorConstants.FondoPanel
            };

            _bufferViz = new BufferVisualizer
            {
                Dock      = DockStyle.Fill,
                BackColor = ColorConstants.FondoPanel
            };
            _panelBuffer.Controls.Add(_bufferViz);

            _panelConsumidores = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.FromArgb(13, 28, 13),
                Padding   = new Padding(10)
            };
            _panelConsumidores.Paint += (_, e) => PintarPanelConsumidores(e);

            _flowActoresP = new Panel
            {
                Dock       = DockStyle.Fill,
                BackColor  = Color.FromArgb(13, 13, 28),
                AutoScroll = true
            };
            _panelProductores.Controls.Add(_flowActoresP);

            _flowActoresC = new Panel
            {
                Dock       = DockStyle.Fill,
                BackColor  = Color.FromArgb(13, 28, 13),
                AutoScroll = true
            };
            _panelConsumidores.Controls.Add(_flowActoresC);

            _panelLog = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 160,
                BackColor = ColorConstants.FondoSuperior
            };
            _panelLog.Paint += PintarLog;

            _panelActivityFeed = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = ColorConstants.FondoSuperior
            };
            _panelLog.Controls.Add(_panelActivityFeed);

            _panelCuerpo.Resize += (_, _) => RedistribuirCuerpo();
            Shown               += (_, _) => RedistribuirCuerpo();
            Resize              += (_, _) => RedistribuirCuerpo();

            _panelCuerpo.Controls.Add(_panelConsumidores);
            _panelCuerpo.Controls.Add(_panelBuffer);
            _panelCuerpo.Controls.Add(_panelProductores);
            _panelCuerpo.Controls.Add(_panelLog);
            Controls.Add(_panelCuerpo);
        }

        private void RedistribuirCuerpo()
        {
            if (_panelCuerpo.Width < 100) return;
            int w = _panelCuerpo.Width;
            _panelProductores.Width = (int)(w * 0.30f);
            _panelBuffer.Width      = (int)(w * 0.25f);
        }

        private void PintarPanelProductores(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            int w = _panelProductores.Width;

            using var fTit = new Font("Segoe UI", 12f, FontStyle.Bold);
            TextRenderer.DrawText(g, "👥 PRODUCTORES", fTit,
                new Rectangle(0, 8, w, 26),
                ColorConstants.TarjetaProductor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);

            TextRenderer.DrawText(g, "(Clientes generando pedidos)",
                new Font("Segoe UI", 9f),
                new Rectangle(0, 34, w, 18),
                ColorConstants.TextoHint,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);

            using var pen = new Pen(
                Color.FromArgb(60, ColorConstants.TarjetaProductor), 1);
            g.DrawLine(pen, 10, 56, w - 10, 56);

            _flowActoresP.Location = new Point(0, 62);
            _flowActoresP.Size     = new Size(w, _panelProductores.Height - 62);
        }

        private void PintarPanelConsumidores(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            int w = _panelConsumidores.Width;

            using var fTit = new Font("Segoe UI", 12f, FontStyle.Bold);
            TextRenderer.DrawText(g, "👨‍🍳 CONSUMIDORES", fTit,
                new Rectangle(0, 8, w, 26),
                ColorConstants.AcentoExito,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);

            TextRenderer.DrawText(g, "(Cocineros procesando pedidos)",
                new Font("Segoe UI", 9f),
                new Rectangle(0, 34, w, 18),
                ColorConstants.TextoHint,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);

            using var pen = new Pen(
                Color.FromArgb(60, ColorConstants.AcentoExito), 1);
            g.DrawLine(pen, 10, 56, w - 10, 56);

            _flowActoresC.Location = new Point(0, 62);
            _flowActoresC.Size     = new Size(w, _panelConsumidores.Height - 62);
        }

        private void PintarLog(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.Clear(ColorConstants.FondoSuperior);

            using var fTit = new Font("Segoe UI", 10f, FontStyle.Bold);
            TextRenderer.DrawText(g,
                "📡 ACTIVITY FEED — Eventos en tiempo real",
                fTit,
                new Rectangle(8, 4, _panelLog.Width - 16, 20),
                ColorConstants.TextoSecundario,
                TextFormatFlags.Left | TextFormatFlags.NoPadding);

            using var pen = new Pen(ColorConstants.Separador, 1);
            g.DrawLine(pen, 8, 26, _panelLog.Width - 8, 26);

            int y = 30;
            using var fLog = new Font("Consolas", 9.5f);
            foreach (var (hora, color, msg) in _logEntries.TakeLast(8))
            {
                if (y > _panelLog.Height - 16) break;
                TextRenderer.DrawText(g, $"{hora}  {msg}", fLog,
                    new Rectangle(10, y, _panelLog.Width - 20, 16),
                    color,
                    TextFormatFlags.Left | TextFormatFlags.NoPadding |
                    TextFormatFlags.EndEllipsis);
                y += 16;
            }
        }

        // ─── STATS ────────────────────────────────────────────────────────────

        private void ConstruirStats()
        {
            _panelStats = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 52,
                BackColor = Color.FromArgb(18, 18, 30)
            };

            var flow = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false,
                BackColor     = Color.FromArgb(18, 18, 30),
                Padding       = new Padding(12, 8, 12, 8)
            };

            _lblStatPed  = CrearStat("📥 Pedidos: 0",    ColorConstants.TarjetaProductor);
            _lblStatMal  = CrearStat("⚡ Alterados: 0",  ColorConstants.AlertaAtaque);
            _lblStatProc = CrearStat("✅ Procesados: 0", ColorConstants.AcentoExito);
            _lblStatThrp = CrearStat("📊 0.0/s",        ColorConstants.AcentoPrincipal);

            flow.Controls.AddRange(new Control[]
                { _lblStatPed, _lblStatMal, _lblStatProc, _lblStatThrp });
            _panelStats.Controls.Add(flow);
            Controls.Add(_panelStats);
        }

        private static Label CrearStat(string texto, Color color) => new Label
        {
            Text      = texto,
            Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
            ForeColor = color,
            BackColor = Color.FromArgb(18, 18, 30),
            AutoSize  = false,
            Width     = 200,
            Height    = 36,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin    = new Padding(12, 0, 12, 0)
        };

        // ─── ACTUALIZACIÓN ────────────────────────────────────────────────────

        private void TimerUI_Tick(object? sender, EventArgs e)
        {
            if (_service.Estado == SimulationStatus.Detenida) return;

            var stats = _service.ObtenerEstadisticas();

            _lblStatPed.Text  = $"📥 Pedidos: {stats.TotalPedidosGenerados}";
            _lblStatMal.Text  = $"⚡ Alterados: {stats.TotalPedidosAlterados}";
            _lblStatProc.Text = $"✅ Procesados: {stats.TotalPedidosCompletados}";
            _lblStatThrp.Text = $"📊 {stats.PedidosPorSegundo:F1}/s";

            if (_attackService.IsAttackActive && !_ataqueActivo)
            {
                _ataqueActivo                 = true;
                _lblEstadoAtaque.Text         = "⚡ ATAQUE ACTIVO";
                _lblEstadoAtaque.ForeColor    = ColorConstants.AlertaAtaque;
            }

            _bufferViz.Actualizar(
                stats.PedidosEnCola,
                stats.CapacidadMaximaCola,
                stats.TotalPedidosAlterados > 0
                    ? Math.Min(stats.PedidosEnCola, stats.TotalPedidosAlterados)
                    : 0,
                _attackService.IsAttackActive);

            // ── Productores ───────────────────────────────────────────────────
            var iconosClientes = new[] { "👨", "👩", "👦", "👴" };
            ActualizarActores(
                _flowActoresP,
                _service.Clientes.Count,
                i =>
                {
                    if (i >= _service.Clientes.Count)
                        return null;

                    var c     = _service.Clientes[i];
                    bool fake = _attackService.IsAttackActive &&
                                i == _service.Clientes.Count - 1;

                    var estadoE = fake
                        ? ActorCard.EstadoActor.Atacado
                        : c.EstaActivo
                            ? ActorCard.EstadoActor.Activo
                            : ActorCard.EstadoActor.Libre;

                    string icono = fake
                        ? "🦹"
                        : iconosClientes[i % iconosClientes.Length];

                    (string, string, string,
                    ActorCard.EstadoActor, float, Color)? r =
                        (icono,
                        c.Id,
                        fake ? "⚡ ATACANTE"
                            : c.EstaActivo ? "Enviando pedido" : "Esperando",
                        estadoE,
                        0f,
                        fake ? ColorConstants.AlertaAtaque
                            : ColorConstants.TarjetaProductor);
                    return r;
                },
                Color.FromArgb(13, 13, 28));

            // ── Consumidores ──────────────────────────────────────────────────
            var iconosCocineros = new[] { "🍳", "🧑\u200d🍳", "👩\u200d🍳" };
            ActualizarActores(
                _flowActoresC,
                _service.Cocineros.Count,
                i =>
                {
                    if (i >= _service.Cocineros.Count)
                        return null;

                    var c       = _service.Cocineros[i];
                    var estadoE = c.PedidoActual != null
                        ? ActorCard.EstadoActor.Activo
                        : c.EstaActivo
                            ? ActorCard.EstadoActor.Esperando
                            : ActorCard.EstadoActor.Libre;

                    string icono = iconosCocineros[i % iconosCocineros.Length];

                    (string, string, string,
                    ActorCard.EstadoActor, float, Color)? r =
                        (icono,
                        c.Id,
                        c.PedidoActual != null
                            ? $"Cocinando #{c.PedidoActual.NumeroPedido:D3}"
                            : c.EstaActivo ? "Esperando pedido" : "Libre",
                        estadoE,
                        c.ProgresoActual,
                        ColorConstants.AcentoExito);
                    return r;
                },
                Color.FromArgb(13, 28, 13));
        }

        // ─── HELPER ACTORES ───────────────────────────────────────────────────

        private void ActualizarActores(
            Panel flowPanel,
            int cantidad,
            Func<int, (string ico, string nom, string est,
                ActorCard.EstadoActor estadoE, float prog,
                Color color)?> getInfo,
            Color backColor)
        {
            int cols  = Math.Max(1, flowPanel.Width / 140);
            int cardW = (flowPanel.Width - 12) / Math.Max(1, cols);
            int cardH = 110;

            while (flowPanel.Controls.Count < cantidad)
            {
                flowPanel.Controls.Add(new ActorCard
                {
                    Size      = new Size(cardW, cardH),
                    BackColor = backColor
                });
            }

            for (int i = 0; i < cantidad && i < flowPanel.Controls.Count; i++)
            {
                var info = getInfo(i);
                if (info == null) continue;

                var card = (ActorCard)flowPanel.Controls[i];
                var (ico, nom, est, estadoE, prog, color) = info.Value;

                int col = i % cols;
                int row = i / cols;
                card.Location = new Point(
                    6 + col * (cardW + 4),
                    4 + row * (cardH + 6));
                card.Size    = new Size(cardW - 4, cardH);
                card.Visible = true;

                card.Actualizar(ico, nom, est, estadoE, prog, color);
            }

            for (int i = cantidad; i < flowPanel.Controls.Count; i++)
                flowPanel.Controls[i].Visible = false;
        }

        // ─── LOG ──────────────────────────────────────────────────────────────

        private void AgregarLog(string icono, string mensaje, Color color)
        {
            _logEntries.Add((
                DateTime.Now.ToString("HH:mm:ss"),
                color,
                $"{icono} {mensaje}"));
            if (_logEntries.Count > 100) _logEntries.RemoveAt(0);
            _panelLog?.Invalidate();
        }
    }
}