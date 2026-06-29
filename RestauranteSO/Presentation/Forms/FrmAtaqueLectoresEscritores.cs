using System.Drawing.Drawing2D;
using System.Drawing.Text;
using RestauranteSO.Constants;
using RestauranteSO.Domain.Enums;
using RestauranteSO.Domain.Interfaces;
using RestauranteSO.Presentation.Controls;
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

        // ─── CONTROLES ────────────────────────────────────────────────────────
        private Panel _panelHeader       = null!;
        private Panel _panelCuerpo       = null!;
        private Panel _panelMeseros      = null!;
        private Panel _panelRecurso      = null!;
        private Panel _panelGerente      = null!;
        private Panel _panelStats        = null!;
        private Panel _panelLog          = null!;
        private Panel _flowMeseros       = null!;
        private Label _lblEstadoAtaque   = null!;
        private Label _lblLockEstado     = null!;
        private Label _lblStatLect       = null!;
        private Label _lblStatEscr       = null!;
        private Label _lblStatComp       = null!;
        private Label _lblGerenteEstado  = null!;

        private readonly List<(string, Color, string)> _logEntries = new();
        private System.Windows.Forms.Timer _timerUI   = null!;
        private System.Windows.Forms.Timer _timerAnim = null!;
        private float _animOffset    = 0f;
        private bool  _ataqueActivo  = false;
        private bool  _escrituraActiva = false;

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
            Text          = "🎣 Phishing al Gerente | RestaurantOS";
            WindowState   = FormWindowState.Maximized;
            MinimumSize   = new Size(1100, 700);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor     = ColorConstants.FondoPrincipal;

            ConstruirHeader();
            ConstruirStats();
            ConstruirCuerpo();

            _timerUI = new System.Windows.Forms.Timer
                { Interval = AppConstants.IntervalActualizacionUIMs };
            _timerUI.Tick += TimerUI_Tick;

            _timerAnim = new System.Windows.Forms.Timer { Interval = 32 };
            _timerAnim.Tick += (_, _) =>
            {
                _animOffset += 0.04f;
                if (_animOffset > MathF.Tau) _animOffset -= MathF.Tau;
                _panelHeader?.Invalidate();
                _panelRecurso?.Invalidate();
                _panelGerente?.Invalidate();
            };
            _timerAnim.Start();

            ResumeLayout(true);

            Load += async (_, _) =>
            {
                await Task.Delay(400);
                _service.Iniciar();
                _timerUI.Start();
                AgregarLog("🟢", "Simulación iniciada — ReaderWriterLockSlim activo",
                    ColorConstants.AcentoExito);

                await Task.Delay(3000);

                var dlg = new FrmIngenieriaSocial(
                    "📧 [BANDEJA DE ENTRADA] — 1 mensaje nuevo",
                    "De: noreply@sistema-gestion-restaurante.net\n" +
                    "Para: gerente@restaurantedoncod.com\n" +
                    "Asunto: ⚠ ACCIÓN REQUERIDA: Verificación obligatoria\n\n" +
                    "Estimado/a Gerente,\n\n" +
                    "Hemos detectado actividad inusual en su cuenta.\n" +
                    "Debe verificar sus credenciales en las próximas 2 horas.\n\n" +
                    "De lo contrario su acceso será suspendido.",
                    "🔗 Verificar mi cuenta ahora",
                    "🗑 Mover a Spam (correcto ✓)");

                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    _attackService.ActivarAtaque(AttackType.PhishingMenuAlterado);
                    _ataqueActivo = true;
                    AgregarLog("🎣", "PHISHING EXITOSO — Credenciales del Gerente comprometidas",
                        ColorConstants.AlertaAtaque);
                    AgregarLog("⚡", "Menú siendo alterado por escritura maliciosa",
                        ColorConstants.AlertaAtaque);
                }
                else
                {
                    AgregarLog("🛡", "Phishing RECHAZADO — Credenciales seguras",
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
                BackColor = Color.FromArgb(14, 6, 20)
            };
            _panelHeader.Paint += (_, e) =>
            {
                using var pen = new Pen(ColorConstants.TarjetaAtaque2, 2);
                e.Graphics.DrawLine(pen,
                    0, _panelHeader.Height - 2,
                    _panelHeader.Width, _panelHeader.Height - 2);
                if (_ataqueActivo)
                {
                    float p = (MathF.Sin(_animOffset * 3f) + 1f) / 2f;
                    using var glow = new Pen(
                        Color.FromArgb((int)(20 + 30 * p),
                            ColorConstants.TarjetaAtaque2), 6f);
                    e.Graphics.DrawLine(glow,
                        0, _panelHeader.Height - 4,
                        _panelHeader.Width, _panelHeader.Height - 4);
                }
            };

            var tbl = new TableLayoutPanel
            {
                Dock        = DockStyle.Fill,
                ColumnCount = 3,
                RowCount    = 1,
                BackColor   = Color.FromArgb(14, 6, 20),
                Padding     = new Padding(16, 0, 16, 0)
            };
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 34f));
            tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33f));
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100f));

            tbl.Controls.Add(new Label
            {
                Text      = "🎣 MÓDULO EDUCATIVO — Lectores–Escritores",
                Font      = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = ColorConstants.TarjetaAtaque2,
                BackColor = Color.FromArgb(14, 6, 20),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            }, 0, 0);

            _lblEstadoAtaque = new Label
            {
                Text      = "● Simulación en curso",
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = ColorConstants.AcentoExito,
                BackColor = Color.FromArgb(14, 6, 20),
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            tbl.Controls.Add(_lblEstadoAtaque, 1, 0);

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

            var panelBtn = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.FromArgb(14, 6, 20)
            };
            panelBtn.Controls.Add(btnVolver);
            panelBtn.Resize += (_, _) =>
                btnVolver.Location = new Point(panelBtn.Width - 118, 10);
            tbl.Controls.Add(panelBtn, 2, 0);

            _panelHeader.Controls.Add(tbl);
            Controls.Add(_panelHeader);
        }

        // ─── STATS ────────────────────────────────────────────────────────────

        private void ConstruirStats()
        {
            _panelStats = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 52,
                BackColor = Color.FromArgb(18, 14, 26)
            };

            var flow = new FlowLayoutPanel
            {
                Dock          = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false,
                BackColor     = Color.FromArgb(18, 14, 26),
                Padding       = new Padding(12, 8, 12, 8)
            };

            _lblStatLect = CrearStat("📖 Lecturas: 0",      ColorConstants.TarjetaLectores);
            _lblStatEscr = CrearStat("✏ Escrituras: 0",    ColorConstants.AcentoSecundario);
            _lblStatComp = CrearStat("⚡ Comprometidos: 0", ColorConstants.AlertaAtaque);
            _lblLockEstado = CrearStat("🔓 Lock: Libre",    ColorConstants.AcentoExito);

            flow.Controls.AddRange(new Control[]
                { _lblStatLect, _lblStatEscr, _lblStatComp, _lblLockEstado });
            _panelStats.Controls.Add(flow);
            Controls.Add(_panelStats);
        }

        private static Label CrearStat(string texto, Color color) => new Label
        {
            Text      = texto,
            Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
            ForeColor = color,
            BackColor = Color.FromArgb(18, 14, 26),
            AutoSize  = false,
            Width     = 220,
            Height    = 36,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin    = new Padding(12, 0, 12, 0)
        };

        // ─── CUERPO ───────────────────────────────────────────────────────────

        private void ConstruirCuerpo()
        {
            _panelCuerpo = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = ColorConstants.FondoPrincipal
            };

            // Meseros
            _panelMeseros = new Panel
            {
                Dock      = DockStyle.Left,
                BackColor = Color.FromArgb(10, 18, 28)
            };
            _panelMeseros.Paint += PintarPanelMeseros;

            _flowMeseros = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.FromArgb(10, 18, 28),
                AutoScroll = true
            };
            _panelMeseros.Controls.Add(_flowMeseros);

            // Recurso compartido
            _panelRecurso = new Panel
            {
                Dock      = DockStyle.Left,
                BackColor = ColorConstants.FondoPanel
            };
            _panelRecurso.Paint += PintarRecursoCompartido;

            // Gerente
            _panelGerente = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.FromArgb(26, 18, 10)
            };
            _panelGerente.Paint += PintarGerente;

            // Log
            _panelLog = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 160,
                BackColor = ColorConstants.FondoSuperior
            };
            _panelLog.Paint += PintarLog;

            _panelCuerpo.Resize += (_, _) => RedistribuirCuerpo();
            Shown               += (_, _) => RedistribuirCuerpo();
            Resize              += (_, _) => RedistribuirCuerpo();

            _panelCuerpo.Controls.Add(_panelGerente);
            _panelCuerpo.Controls.Add(_panelRecurso);
            _panelCuerpo.Controls.Add(_panelMeseros);
            _panelCuerpo.Controls.Add(_panelLog);
            Controls.Add(_panelCuerpo);
        }

        private void RedistribuirCuerpo()
        {
            if (_panelCuerpo.Width < 100) return;
            int w = _panelCuerpo.Width;
            _panelMeseros.Width = (int)(w * 0.30f);
            _panelRecurso.Width = (int)(w * 0.35f);
        }

        // ─── PINTADO PANELES ──────────────────────────────────────────────────

        private void PintarPanelMeseros(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            int w = _panelMeseros.Width;

            using var fTit = new Font("Segoe UI", 12f, FontStyle.Bold);
            TextRenderer.DrawText(g, "🧑‍🍽 LECTORES", fTit,
                new Rectangle(0, 8, w, 26),
                ColorConstants.TarjetaLectores,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);

            TextRenderer.DrawText(g, "(Meseros leyendo el menú)",
                new Font("Segoe UI", 9f),
                new Rectangle(0, 34, w, 18),
                ColorConstants.TextoHint,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);

            using var pen = new Pen(
                Color.FromArgb(60, ColorConstants.TarjetaLectores), 1);
            g.DrawLine(pen, 10, 56, w - 10, 56);

            _flowMeseros.Location = new Point(0, 62);
            _flowMeseros.Size     = new Size(w, _panelMeseros.Height - 62);
        }

        private void PintarRecursoCompartido(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            int w = _panelRecurso.Width;
            int h = _panelRecurso.Height;

            g.Clear(ColorConstants.FondoPanel);

            // Título
            using var fTit = new Font("Segoe UI", 12f, FontStyle.Bold);
            TextRenderer.DrawText(g, "🗄️ RECURSO COMPARTIDO", fTit,
                new Rectangle(0, 8, w, 26),
                ColorConstants.AcentoPrincipal,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
            TextRenderer.DrawText(g, "(Menú del Restaurante)",
                new Font("Segoe UI", 9f),
                new Rectangle(0, 34, w, 18),
                ColorConstants.TextoHint,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);

            // Representación visual del DB
            int dbW = (int)(w * 0.55f);
            int dbH = (int)(h * 0.22f);
            int dbX = (w - dbW) / 2;
            int dbY = 68;

            // Cilindro DB
            Color dbColor = _ataqueActivo
                ? ColorConstants.AlertaAtaque
                : _escrituraActiva
                    ? ColorConstants.AcentoSecundario
                    : ColorConstants.AcentoPrincipal;

            float pulse = (MathF.Sin(_animOffset * 2f) + 1f) / 2f;

            // Halo
            if (_ataqueActivo || _escrituraActiva)
            {
                int hr = (int)(8 + pulse * 6);
                using var haloPen = new Pen(
                    Color.FromArgb((int)(40 + 30 * pulse), dbColor), hr);
                g.DrawEllipse(haloPen, dbX - hr, dbY - hr / 2,
                    dbW + hr * 2, dbH + hr);
            }

            // Tapa superior
            using var ellBrush = new SolidBrush(
                Color.FromArgb(200, dbColor));
            g.FillEllipse(ellBrush,
                dbX, dbY, dbW, dbH / 2);

            // Cuerpo
            using var bodyBrush = new SolidBrush(
                Color.FromArgb(120, dbColor));
            g.FillRectangle(bodyBrush,
                dbX, dbY + dbH / 4, dbW, dbH);

            // Tapa inferior
            g.FillEllipse(ellBrush,
                dbX, dbY + dbH, dbW, dbH / 2);

            // Borde
            using var dbPen = new Pen(dbColor, 2f);
            g.DrawEllipse(dbPen, dbX, dbY, dbW, dbH / 2);
            g.DrawLine(dbPen, dbX, dbY + dbH / 4, dbX, dbY + dbH + dbH / 4);
            g.DrawLine(dbPen, dbX + dbW, dbY + dbH / 4, dbX + dbW, dbY + dbH + dbH / 4);
            g.DrawEllipse(dbPen, dbX, dbY + dbH, dbW, dbH / 2);

            // Texto en el cilindro
            TextRenderer.DrawText(g, "MENÚ",
                new Font("Segoe UI", 11f, FontStyle.Bold),
                new Rectangle(dbX, dbY + dbH / 2, dbW, dbH),
                Color.White,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter   |
                TextFormatFlags.NoPadding);

            // Estado del lock
            int lockY = dbY + dbH + dbH / 2 + 20;

            // Indicador ReaderWriterLockSlim
            string lockTxt = _escrituraActiva
                ? "🔴 WriteLock EXCLUSIVO\nLecturas bloqueadas"
                : _service.Meseros.Any(m => m.EstaLeyendo)
                    ? $"🟢 ReadLock COMPARTIDO\n{_service.Meseros.Count(m => m.EstaLeyendo)} lectores activos"
                    : "⚪ Lock LIBRE";

            Color lockColor = _escrituraActiva
                ? ColorConstants.AlertaAtaque
                : _service.Meseros.Any(m => m.EstaLeyendo)
                    ? ColorConstants.AcentoExito
                    : ColorConstants.TextoHint;

            var lockRect = new Rectangle(dbX - 10, lockY, dbW + 20, 54);
            using var lockBrush = new SolidBrush(
                Color.FromArgb(40, lockColor));
            using var lockPath = CrearPath(lockRect, 8);
            g.FillPath(lockBrush, lockPath);
            using var lockPen = new Pen(Color.FromArgb(80, lockColor), 1f);
            g.DrawPath(lockPen, lockPath);

            TextRenderer.DrawText(g, lockTxt,
                new Font("Segoe UI", 10f, FontStyle.Bold),
                lockRect, lockColor,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter   |
                TextFormatFlags.NoPadding        |
                TextFormatFlags.WordBreak);

            // Flechas de lectores
            if (!_escrituraActiva && _service.Meseros.Any(m => m.EstaLeyendo))
            {
                int flechaX = dbX - 40;
                float flechaOffset = (_animOffset * 20f) % 40;
                for (int i = 0; i < 3; i++)
                {
                    int fy = dbY + (int)((dbH + flechaOffset + i * 14) % (dbH + 10));
                    using var fPen = new Pen(
                        Color.FromArgb(150, ColorConstants.TarjetaLectores), 2f);
                    g.DrawLine(fPen, flechaX, fy, dbX - 4, fy);
                    // Punta
                    using var fBrush = new SolidBrush(ColorConstants.TarjetaLectores);
                    g.FillPolygon(fBrush, new PointF[]
                    {
                        new(dbX - 4, fy),
                        new(dbX - 14, fy - 5),
                        new(dbX - 14, fy + 5)
                    });
                }
            }

            // Flecha escritura maliciosa
            if (_ataqueActivo && _escrituraActiva)
            {
                int flechaX2 = dbX + dbW + 4;
                float fOffset = (_animOffset * 25f) % 40;
                for (int i = 0; i < 3; i++)
                {
                    int fy = dbY + (int)((dbH + fOffset + i * 14) % (dbH + 10));
                    using var fPen = new Pen(
                        Color.FromArgb(200, ColorConstants.AlertaAtaque), 2.5f);
                    g.DrawLine(fPen, flechaX2, fy, flechaX2 + 36, fy);
                    using var fBrush = new SolidBrush(ColorConstants.AlertaAtaque);
                    g.FillPolygon(fBrush, new PointF[]
                    {
                        new(flechaX2, fy),
                        new(flechaX2 + 12, fy - 6),
                        new(flechaX2 + 12, fy + 6)
                    });
                }
            }

            // Versión del menú
            TextRenderer.DrawText(g,
                _ataqueActivo ? "⚠ MENÚ COMPROMETIDO" : "✅ Menú íntegro",
                new Font("Segoe UI", 10f, FontStyle.Bold),
                new Rectangle(0, lockY + 62, w, 22),
                _ataqueActivo ? ColorConstants.AlertaAtaque : ColorConstants.AcentoExito,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
        }

        private void PintarGerente(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode     = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            int w = _panelGerente.Width;
            int h = _panelGerente.Height;

            Color fondoColor = _ataqueActivo
                ? Color.FromArgb(26, 10, 10)
                : Color.FromArgb(26, 18, 10);
            g.Clear(fondoColor);

            using var fTit = new Font("Segoe UI", 12f, FontStyle.Bold);
            Color titColor = _ataqueActivo
                ? ColorConstants.AlertaAtaque
                : ColorConstants.AcentoSecundario;

            TextRenderer.DrawText(g, "👔 ESCRITOR", fTit,
                new Rectangle(0, 8, w, 26), titColor,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);
            TextRenderer.DrawText(g, "(Gerente actualizando el menú)",
                new Font("Segoe UI", 9f),
                new Rectangle(0, 34, w, 18),
                ColorConstants.TextoHint,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.NoPadding);

            using var pen = new Pen(Color.FromArgb(60, titColor), 1);
            g.DrawLine(pen, 10, 56, w - 10, 56);

            // Avatar del gerente
            int avCX = w / 2;
            int avCY = (int)(h * 0.35f);
            int avR  = Math.Min(w / 4, 60);

            float pulse = (MathF.Sin(_animOffset * 2f) + 1f) / 2f;

            if (_escrituraActiva || _ataqueActivo)
            {
                float hr = avR + 8f + pulse * 8f;
                using var haloBrush = new SolidBrush(
                    Color.FromArgb((int)(30 + 30 * pulse), titColor));
                g.FillEllipse(haloBrush,
                    avCX - hr, avCY - hr, hr * 2, hr * 2);
            }

            using var avFondo = new SolidBrush(ColorConstants.FondoCard);
            g.FillEllipse(avFondo, avCX - avR, avCY - avR, avR * 2, avR * 2);
            using var avBorder = new Pen(titColor, 2.5f);
            g.DrawEllipse(avBorder, avCX - avR, avCY - avR, avR * 2, avR * 2);

            using var fAv = new Font("Segoe UI Emoji", avR * 0.7f);
            TextRenderer.DrawText(g,
                _ataqueActivo ? "🦹" : "👔",
                fAv,
                new Rectangle(avCX - avR, avCY - avR, avR * 2, avR * 2),
                ColorConstants.TextoPrincipal,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter   |
                TextFormatFlags.NoPadding);

            // Estado
            int estY = avCY + avR + 12;
            string estTxt = _escrituraActiva
                ? (_ataqueActivo ? "✏ ESCRIBIENDO\n(maliciosamente)" : "✏ ESCRIBIENDO\n(WriteLock activo)")
                : _service.EscritorEsperando
                    ? "⏳ Esperando\nWriteLock..."
                    : "💤 En espera";

            Color estColor = _escrituraActiva
                ? (_ataqueActivo ? ColorConstants.AlertaAtaque : ColorConstants.AcentoSecundario)
                : _service.EscritorEsperando
                    ? ColorConstants.EstadoEsperando
                    : ColorConstants.TextoHint;

            var estRect = new Rectangle(10, estY, w - 20, 50);
            using var estBrush = new SolidBrush(Color.FromArgb(40, estColor));
            using var estPath  = CrearPath(estRect, 8);
            g.FillPath(estBrush, estPath);

            TextRenderer.DrawText(g, estTxt,
                new Font("Segoe UI", 10f, FontStyle.Bold),
                estRect, estColor,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter   |
                TextFormatFlags.NoPadding        |
                TextFormatFlags.WordBreak);

            // Última modificación
            string ultima = _service.UltimaModificacion ?? "—";
            TextRenderer.DrawText(g,
                $"Última modificación:\n{(ultima.Length > 30 ? ultima[..30] + "…" : ultima)}",
                new Font("Segoe UI", 9f),
                new Rectangle(10, estY + 58, w - 20, 40),
                ColorConstants.TextoHint,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.Top | TextFormatFlags.NoPadding |
                TextFormatFlags.WordBreak);

            // Si ataque: mostrar alerta
            if (_ataqueActivo)
            {
                int alertY = (int)(h * 0.72f);
                var alertRect = new Rectangle(10, alertY, w - 20, 70);
                using var alertBrush = new SolidBrush(
                    Color.FromArgb(50, ColorConstants.AlertaAtaque));
                using var alertPath  = CrearPath(alertRect, 10);
                g.FillPath(alertBrush, alertPath);
                using var alertPen = new Pen(
                    Color.FromArgb(120, ColorConstants.AlertaAtaque), 1.5f);
                g.DrawPath(alertPen, alertPath);

                float alertPulse = (MathF.Sin(_animOffset * 4f) + 1f) / 2f;
                TextRenderer.DrawText(g,
                    "⚠ CREDENCIALES COMPROMETIDAS\nMenú siendo alterado\nSistema en peligro",
                    new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    alertRect,
                    Color.FromArgb((int)(180 + 75 * alertPulse),
                        ColorConstants.AlertaAtaque),
                    TextFormatFlags.HorizontalCenter |
                    TextFormatFlags.VerticalCenter   |
                    TextFormatFlags.NoPadding        |
                    TextFormatFlags.WordBreak);
            }
        }

        private void PintarLog(object? sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            g.Clear(ColorConstants.FondoSuperior);

            using var fTit = new Font("Segoe UI", 10f, FontStyle.Bold);
            TextRenderer.DrawText(g, "📡 ACTIVITY FEED — Eventos en tiempo real",
                fTit, new Rectangle(8, 4, _panelLog.Width - 16, 20),
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

        // ─── ACTUALIZACIÓN ────────────────────────────────────────────────────

        private void TimerUI_Tick(object? sender, EventArgs e)
        {
            if (_service.Estado == SimulationStatus.Detenida) return;

            var stats = _service.ObtenerEstadisticas();

            _lblStatLect.Text  = $"📖 Lecturas: {stats.TotalLecturas}";
            _lblStatEscr.Text  = $"✏ Escrituras: {stats.TotalEscrituras}";
            _lblStatComp.Text  = $"⚡ Comprometidos: {stats.LectoresComprometidos}";

            _escrituraActiva = stats.EscritorActivo;

            _lblLockEstado.Text = stats.EscritorActivo
                ? "🔴 WriteLock ACTIVO"
                : stats.LectoresActivos > 0
                    ? $"🟢 ReadLock: {stats.LectoresActivos}"
                    : "⚪ Lock: Libre";
            _lblLockEstado.ForeColor = stats.EscritorActivo
                ? ColorConstants.AlertaAtaque
                : stats.LectoresActivos > 0
                    ? ColorConstants.AcentoExito
                    : ColorConstants.TextoHint;

            if (_attackService.IsAttackActive && !_ataqueActivo)
            {
                _ataqueActivo = true;
                _lblEstadoAtaque.Text      = "🎣 PHISHING ACTIVO";
                _lblEstadoAtaque.ForeColor = ColorConstants.AlertaAtaque;
            }

            // Actualizar meseros
            var meseros = _service.Meseros;
            ActualizarMeseros(meseros);

            _panelRecurso.Invalidate();
            _panelGerente.Invalidate();
            _panelLog.Invalidate();
        }

        private void ActualizarMeseros(
            IReadOnlyList<Domain.Entities.Mesero> meseros)
        {
            var iconosMeseros = new string[] { "🍽", "🧑\u200d🍽", "👩\u200d🍽" };

            int cols  = Math.Max(1, _flowMeseros.Width / 140);
            int cardW = (_flowMeseros.Width - 12) / Math.Max(1, cols);
            int cardH = 110;

            while (_flowMeseros.Controls.Count < meseros.Count)
            {
                var card = new ActorCard
                {
                    Size      = new Size(cardW, cardH),
                    BackColor = Color.FromArgb(10, 18, 28)
                };
                _flowMeseros.Controls.Add(card);
            }

            for (int i = 0; i < meseros.Count; i++)
            {
                var m    = meseros[i];
                var card = (ActorCard)_flowMeseros.Controls[i];

                var estadoE = m.LeyoMenuComprometido
                    ? ActorCard.EstadoActor.Atacado
                    : m.EstaLeyendo
                        ? ActorCard.EstadoActor.Activo
                        : m.EstaEsperando
                            ? ActorCard.EstadoActor.Esperando
                            : ActorCard.EstadoActor.Libre;

                string estadoTxt = m.LeyoMenuComprometido
                    ? "⚠ Leyó info falsa"
                    : m.EstaLeyendo
                        ? "📖 Leyendo"
                        : m.EstaEsperando
                            ? "⏳ Esperando lock"
                            : "Libre";

                int col = i % cols;
                int row = i / cols;
                card.Location = new Point(
                    6 + col * (cardW + 4),
                    4 + row * (cardH + 6));
                card.Size    = new Size(cardW - 4, cardH);
                card.Visible = true;

                string icono = iconosMeseros[i % iconosMeseros.Length];

                card.Actualizar(
                    icono,
                    m.Id,
                    estadoTxt,
                    estadoE,
                    0f,
                    m.LeyoMenuComprometido
                        ? ColorConstants.AlertaAtaque
                        : ColorConstants.TarjetaLectores);
            }

            for (int i = meseros.Count; i < _flowMeseros.Controls.Count; i++)
                _flowMeseros.Controls[i].Visible = false;
        }

        private void AgregarLog(string icono, string mensaje, Color color)
        {
            _logEntries.Add((DateTime.Now.ToString("HH:mm:ss"),
                color, $"{icono} {mensaje}"));
            if (_logEntries.Count > 100) _logEntries.RemoveAt(0);
        }

        private static GraphicsPath CrearPath(Rectangle rect, int radio)
        {
            var path = new GraphicsPath();
            int d    = Math.Min(radio * 2, Math.Min(rect.Width, rect.Height));
            if (d <= 0) { path.AddRectangle(rect); return path; }
            path.AddArc(rect.X,         rect.Y,          d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y,          d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d,   0, 90);
            path.AddArc(rect.X,         rect.Bottom - d, d, d,  90, 90);
            path.CloseFigure();
            return path;
        }
    }
}