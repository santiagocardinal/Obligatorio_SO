using RestauranteSO.Constants;
using RestauranteSO.Domain.Models;
using RestauranteSO.Presentation.Themes;
using RestauranteSO.Services.Ataques;

namespace RestauranteSO.Presentation.Forms
{
    public sealed class FrmPoliticas : Form
    {
        private readonly string _titulo;
        private readonly IReadOnlyList<PoliticaSeguridad> _politicas;
        private readonly IReadOnlyList<AttackEvent> _historialAtaques;

        // Controles principales
        private TabControl _tabs = null!;
        private Panel _panelHeader = null!;
        private Panel _panelBotones = null!;

        public FrmPoliticas(
            string titulo,
            IReadOnlyList<PoliticaSeguridad> politicas,
            IReadOnlyList<AttackEvent> historialAtaques)
        {
            _titulo = titulo;
            _politicas = politicas;
            _historialAtaques = historialAtaques;

            // NO asignar fuentes personalizadas aquí - usar fuentes seguras
            InitializeComponent();

            // Asignar fuentes después de la creación
            this.Load += (s, e) => AplicarFuentes();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            Text = $"🛡 {_titulo}";
            Size = new Size(940, 760);
            MinimumSize = new Size(840, 640);
            StartPosition = FormStartPosition.CenterParent;
            BackColor = ColorConstants.FondoPrincipal;
            ForeColor = ColorConstants.TextoPrincipal;
            FormBorderStyle = FormBorderStyle.Sizable;

            // Usar fuente del sistema para evitar problemas de creación
            this.Font = SystemFonts.DefaultFont;

            ConstruirHeader();
            ConstruirTabs();
            ConstruirBotones();

            ResumeLayout(true);
        }

        private void ConstruirHeader()
        {
            _panelHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 68,
                BackColor = ColorConstants.FondoPoliticas,
                Padding = new Padding(24, 0, 24, 0)
            };
            _panelHeader.Paint += (_, e) =>
            {
                using var pen = new Pen(ColorConstants.ColorSeguridad, 2);
                e.Graphics.DrawLine(pen, 0, _panelHeader.Height - 2, _panelHeader.Width, _panelHeader.Height - 2);
            };

            var lblIco = new Label
            {
                Text = "🛡",
                Font = new Font("Segoe UI Emoji", 26f),
                ForeColor = ColorConstants.ColorSeguridad,
                AutoSize = false,
                Size = new Size(52, 64),
                Location = new Point(20, 2),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = ColorConstants.FondoPoliticas
            };

            var lblTit = new Label
            {
                Text = _titulo,
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = ColorConstants.ColorSeguridad,
                AutoSize = true,
                Location = new Point(76, 16),
                BackColor = ColorConstants.FondoPoliticas
            };

            var lblSub = new Label
            {
                Text = $"{_historialAtaques.Count} eventos de ataque registrados  |  {_politicas.Count} políticas de prevención",
                Font = new Font("Segoe UI", 10f, FontStyle.Regular),
                ForeColor = ColorConstants.TextoHint,
                AutoSize = true,
                Location = new Point(78, 42),
                BackColor = ColorConstants.FondoPoliticas
            };

            _panelHeader.Controls.AddRange(new Control[] { lblIco, lblTit, lblSub });
            Controls.Add(_panelHeader);
        }

        private void ConstruirTabs()
        {
            _tabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Appearance = TabAppearance.Normal,
                Padding = new Point(8, 4)
            };
            _tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            _tabs.DrawItem += (s, e) =>
            {
                var tab = (TabControl)s!;
                var page = tab.TabPages[e.Index];
                bool selected = e.Index == tab.SelectedIndex;
                var backColor = selected ? ColorConstants.FondoCard : ColorConstants.FondoPanel;
                var foreColor = selected ? ColorConstants.ColorSeguridad : ColorConstants.TextoSecundario;

                using var backBrush = new SolidBrush(backColor);
                e.Graphics.FillRectangle(backBrush, e.Bounds);
                TextRenderer.DrawText(e.Graphics, page.Text,
                    new Font("Segoe UI", 9f, selected ? FontStyle.Bold : FontStyle.Regular),
                    e.Bounds, foreColor,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                if (selected)
                {
                    using var pen = new Pen(ColorConstants.ColorSeguridad, 2);
                    e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 2, e.Bounds.Right, e.Bounds.Bottom - 2);
                }
            };

            var tabPoliticas = new TabPage("🛡 Políticas de Prevención");
            var tabEvidencia = new TabPage("🔍 Evidencia del Ataque");
            var tabResumen = new TabPage("📋 Resumen Ejecutivo");

            tabPoliticas.BackColor = ColorConstants.FondoPrincipal;
            tabEvidencia.BackColor = ColorConstants.FondoPrincipal;
            tabResumen.BackColor = ColorConstants.FondoPrincipal;

            ConstruirTabPoliticas(tabPoliticas);
            ConstruirTabEvidencia(tabEvidencia);
            ConstruirTabResumen(tabResumen);

            _tabs.TabPages.AddRange(new[] { tabPoliticas, tabEvidencia, tabResumen });
            Controls.Add(_tabs);
        }

        private void ConstruirBotones()
        {
            _panelBotones = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 52,
                BackColor = ColorConstants.FondoSuperior,
                Padding = new Padding(16, 8, 16, 8)
            };

            var btnCerrar = new Button
            {
                Text = "✓ Cerrar",
                Size = new Size(120, 34),
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Anchor = AnchorStyles.Right | AnchorStyles.Top
            };
            AppTheme.AplicarABotonPrimario(btnCerrar);
            btnCerrar.Location = new Point(_panelBotones.Width - 136, 8);
            _panelBotones.Resize += (_, _) => btnCerrar.Location = new Point(_panelBotones.Width - 136, 8);
            btnCerrar.Click += (_, _) => Close();

            var lblDisclaimer = new Label
            {
                Text = "⚠ Este módulo es estrictamente educativo. Ningún ataque real fue ejecutado durante la simulación.",
                Font = new Font("Segoe UI", 10f, FontStyle.Regular),
                ForeColor = ColorConstants.TextoHint,
                AutoSize = true,
                Location = new Point(12, 14)
            };

            _panelBotones.Controls.AddRange(new Control[] { lblDisclaimer, btnCerrar });
            Controls.Add(_panelBotones);
        }

        private void AplicarFuentes()
        {
            // Asignar fuentes después de la creación del handle
            // Usar fuentes seguras que no causen excepciones
            _tabs.Font = new Font("Segoe UI", 10f, FontStyle.Regular);
        }

        private void ConstruirTabPoliticas(TabPage tab)
        {
            var scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = ColorConstants.FondoPrincipal,
                Padding = new Padding(16)
            };

            int y = 8;
            foreach (var politica in _politicas)
            {
                var card = ConstruirCardPolitica(politica, y);
                scroll.Controls.Add(card);
                y += card.Height + 12;
            }
            scroll.AutoScrollMinSize = new Size(0, y + 24);
            tab.Controls.Add(scroll);
        }

        private Panel ConstruirCardPolitica(PoliticaSeguridad p, int y)
        {
            var panel = new Panel
            {
                Location = new Point(8, y),
                Size = new Size(860, 130),
                BackColor = ColorConstants.FondoCard,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top,
                Padding = new Padding(0)
            };
            panel.Paint += (_, e) =>
            {
                using var pen = new Pen(ColorConstants.ColorSeguridad, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
                using var brush = new SolidBrush(ColorConstants.ColorSeguridad);
                e.Graphics.FillRectangle(brush, 0, 0, 4, panel.Height);
            };

            var lblCat = new Label
            {
                Text = p.Categoria,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = ColorConstants.TextoHint,
                AutoSize = true,
                Location = new Point(16, 6)
            };
            var lblTit = new Label
            {
                Text = p.Titulo,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = ColorConstants.ColorSeguridad,
                AutoSize = false,
                Size = new Size(panel.Width - 24, 24),
                Location = new Point(16, 22)
            };
            var lblDesc = new Label
            {
                Text = p.Descripcion,
                Font = new Font("Segoe UI", 10f, FontStyle.Regular),
                ForeColor = ColorConstants.TextoPrincipal,
                AutoSize = false,
                Size = new Size((panel.Width / 2) - 20, 48),
                Location = new Point(16, 50)
            };
            var lblVulnTit = new Label
            {
                Text = "Vulnerabilidad:",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = ColorConstants.AlertaAtaque,
                AutoSize = true,
                Location = new Point(panel.Width / 2, 50)
            };
            var lblVuln = new Label
            {
                Text = p.Vulnerabilidad,
                Font = new Font("Segoe UI", 10f, FontStyle.Regular),
                ForeColor = ColorConstants.EstadoAlterado,
                AutoSize = false,
                Size = new Size((panel.Width / 2) - 20, 32),
                Location = new Point(panel.Width / 2, 66)
            };
            var lblImplTit = new Label
            {
                Text = "Implementación:",
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = ColorConstants.AcentoPrincipal,
                AutoSize = true,
                Location = new Point(16, 102)
            };
            var lblImpl = new Label
            {
                Text = p.Implementacion,
                Font = new Font("Segoe UI", 10f, FontStyle.Regular),
                ForeColor = ColorConstants.TextoSecundario,
                AutoSize = false,
                Size = new Size(panel.Width - 24, 18),
                Location = new Point(110, 104)
            };

            panel.Controls.AddRange(new Control[] { lblCat, lblTit, lblDesc, lblVulnTit, lblVuln, lblImplTit, lblImpl });
            panel.Resize += (_, _) =>
            {
                int w = panel.Width;
                lblTit.Width = w - 24;
                lblDesc.Width = (w / 2) - 20;
                lblVulnTit.Location = new Point(w / 2, 50);
                lblVuln.Location = new Point(w / 2, 66);
                lblVuln.Width = (w / 2) - 20;
                lblImpl.Width = w - 24;
            };
            return panel;
        }

        private void ConstruirTabEvidencia(TabPage tab)
        {
            var panelMain = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorConstants.FondoPrincipal,
                Padding = new Padding(16)
            };

            var panelImpacto = new Panel
            {
                Dock = DockStyle.Top,
                Height = 72,
                BackColor = ColorConstants.FondoAtaque,
                Padding = new Padding(16, 8, 16, 8),
                Margin = new Padding(0, 0, 0, 8)
            };
            panelImpacto.Paint += (_, e) =>
            {
                using var pen = new Pen(ColorConstants.AlertaAtaque, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, panelImpacto.Width - 1, panelImpacto.Height - 1);
            };

            var lblImpTit = new Label
            {
                Text = $"⚡ Total de eventos de ataque registrados: {_historialAtaques.Count}",
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = ColorConstants.AlertaAtaque,
                AutoSize = true,
                Location = new Point(12, 8),
                BackColor = ColorConstants.FondoAtaque
            };
            var lblImpSub = new Label
            {
                Text = "Los siguientes eventos ocurrieron durante la simulación del ataque. En un escenario real, cada uno representaría un impacto concreto en el negocio.",
                Font = new Font("Segoe UI", 10f, FontStyle.Regular),
                ForeColor = ColorConstants.TextoSecundario,
                AutoSize = false,
                Size = new Size(860, 32),
                Location = new Point(12, 32),
                BackColor = ColorConstants.FondoAtaque
            };
            panelImpacto.Controls.AddRange(new Control[] { lblImpTit, lblImpSub });

            var gridEventos = new DataGridView { Dock = DockStyle.Fill };
            AppTheme.AplicarADataGrid(gridEventos);
            gridEventos.Columns.Add(new DataGridViewTextBoxColumn { Name = "Hora", HeaderText = "Hora", Width = 80, FillWeight = 10 });
            gridEventos.Columns.Add(new DataGridViewTextBoxColumn { Name = "Tipo", HeaderText = "Tipo de Ataque", FillWeight = 18 });
            gridEventos.Columns.Add(new DataGridViewTextBoxColumn { Name = "Componente", HeaderText = "Componente", FillWeight = 18 });
            gridEventos.Columns.Add(new DataGridViewTextBoxColumn { Name = "Descripcion", HeaderText = "Descripción", FillWeight = 30 });
            gridEventos.Columns.Add(new DataGridViewTextBoxColumn { Name = "Impacto", HeaderText = "Impacto en Negocio", FillWeight = 24 });

            foreach (var ev in _historialAtaques)
            {
                int idx = gridEventos.Rows.Add(
                    ev.Timestamp.ToString("HH:mm:ss"),
                    ev.TipoAtaque.ToString(),
                    ev.ComponenteAfectado,
                    ev.Descripcion.Length > 45 ? ev.Descripcion[..45] + "…" : ev.Descripcion,
                    ev.ImpactoNegocio.Length > 40 ? ev.ImpactoNegocio[..40] + "…" : ev.ImpactoNegocio);
                gridEventos.Rows[idx].DefaultCellStyle.ForeColor = ColorConstants.EstadoAlterado;
            }

            panelMain.Controls.Add(gridEventos);
            panelMain.Controls.Add(panelImpacto);
            tab.Controls.Add(panelMain);
        }

        private void ConstruirTabResumen(TabPage tab)
        {
            var scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = ColorConstants.FondoPrincipal,
                Padding = new Padding(20)
            };

            int y = 8;
            int maxWidth = scroll.Width - 40;

            // Título principal
            var lblTitulo = new Label
            {
                Text = "RESUMEN EJECUTIVO DE SEGURIDAD",
                Font = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = ColorConstants.ColorSeguridad,
                AutoSize = true,
                Location = new Point(12, y)
            };
            scroll.Controls.Add(lblTitulo);
            y += lblTitulo.Height + 12;

            // Sección 1
            AgregarSeccionLabel(scroll, ref y, maxWidth, "━━ QUÉ OCURRIÓ ━━━━━━━━━━━━━━━━━━━━━━━",
                new Font("Segoe UI", 11f, FontStyle.Bold), ColorConstants.AlertaAtaque);
            AgregarTextoLabel(scroll, ref y, maxWidth,
                "Se ejecutó una simulación educativa de ataque de ingeniería social " +
                "sobre el sistema del restaurante. El atacante explotó la confianza " +
                "del personal para obtener acceso no autorizado al sistema.",
                new Font("Segoe UI", 11f, FontStyle.Regular), ColorConstants.TextoPrincipal);
            y += 8;

            // Sección 2
            AgregarSeccionLabel(scroll, ref y, maxWidth, "━━ VECTOR DE ATAQUE ━━━━━━━━━━━━━━━━━━",
                new Font("Segoe UI", 11f, FontStyle.Bold), ColorConstants.EstadoEsperando);
            AgregarTextoLabel(scroll, ref y, maxWidth,
                "Ingeniería Social: El atacante se presentó como personal técnico " +
                "legítimo, creando urgencia artificial para obtener acceso inmediato " +
                "sin seguir los procedimientos establecidos de verificación.",
                new Font("Segoe UI", 11f, FontStyle.Regular), ColorConstants.TextoPrincipal);
            y += 8;

            // Sección 3
            AgregarSeccionLabel(scroll, ref y, maxWidth, "━━ ERROR HUMANO PRINCIPAL ━━━━━━━━━━━━━",
                new Font("Segoe UI", 11f, FontStyle.Bold), ColorConstants.AlertaAtaque);
            AgregarTextoLabel(scroll, ref y, maxWidth,
                "El personal otorgó acceso sin:\n" +
                "  • Verificar la identidad con credenciales escritas\n" +
                "  • Obtener aprobación escrita de la gerencia\n" +
                "  • Consultar al departamento de IT\n" +
                "  • Seguir el proceso de gestión de cambios establecido",
                new Font("Segoe UI", 11f, FontStyle.Regular), ColorConstants.TextoPrincipal);
            y += 8;

            // Sección 4
            AgregarSeccionLabel(scroll, ref y, maxWidth, "━━ CONTROLES RECOMENDADOS ━━━━━━━━━━━━",
                new Font("Segoe UI", 11f, FontStyle.Bold), ColorConstants.ColorSeguridad);
            AgregarTextoLabel(scroll, ref y, maxWidth,
                "INMEDIATOS (0-30 días):\n" +
                "  • Capacitación obligatoria en ingeniería social\n" +
                "  • Activar MFA en todas las cuentas privilegiadas\n" +
                "  • Implementar registro de visitantes\n\n" +
                "MEDIANO PLAZO (30-90 días):\n" +
                "  • Instalar solución EDR en todos los equipos\n" +
                "  • Implementar SIEM con alertas de comportamiento anómalo\n" +
                "  • Revisar y actualizar políticas de gestión de cambios\n\n" +
                "LARGO PLAZO (90+ días):\n" +
                "  • Programa continuo de concientización en seguridad\n" +
                "  • Auditorías de seguridad periódicas\n" +
                "  • Plan de respuesta a incidentes documentado y probado",
                new Font("Segoe UI", 11f, FontStyle.Regular), ColorConstants.TextoPrincipal);
            y += 8;

            // Sección 5
            AgregarSeccionLabel(scroll, ref y, maxWidth, "━━ MARCOS DE REFERENCIA ━━━━━━━━━━━━━━",
                new Font("Segoe UI", 11f, FontStyle.Bold), ColorConstants.AcentoPrincipal);
            AgregarTextoLabel(scroll, ref y, maxWidth,
                "• NIST Cybersecurity Framework (CSF 2.0)\n" +
                "• ISO/IEC 27001:2022 — Gestión de Seguridad de la Información\n" +
                "• CIS Controls v8 — Control 14: Security Awareness\n" +
                "• OWASP — Social Engineering Prevention\n" +
                "• MITRE ATT&CK — T1566 (Phishing)",
                new Font("Segoe UI", 11f, FontStyle.Regular), ColorConstants.TextoSecundario);
            y += 8;

            scroll.AutoScrollMinSize = new Size(0, y + 40);
            tab.Controls.Add(scroll);
        }

        private void AgregarSeccionLabel(Panel parent, ref int y, int maxWidth, string texto, Font font, Color color)
        {
            var lbl = new Label
            {
                Text = texto,
                Font = font,
                ForeColor = color,
                AutoSize = true,
                Location = new Point(12, y),
                MaximumSize = new Size(maxWidth, 0)
            };
            parent.Controls.Add(lbl);
            y += lbl.Height + 4;
        }

        private void AgregarTextoLabel(Panel parent, ref int y, int maxWidth, string texto, Font font, Color color)
        {
            var lbl = new Label
            {
                Text = texto,
                Font = font,
                ForeColor = color,
                AutoSize = true,
                Location = new Point(12, y),
                MaximumSize = new Size(maxWidth, 0)
            };
            parent.Controls.Add(lbl);
            y += lbl.Height + 4;
        }
    }
}