// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Presentation/Forms/FrmPoliticas.cs
// Propósito: Formulario de políticas de prevención y evidencia del ataque.
//            Muestra qué ocurrió, por qué funcionó y cómo prevenirlo.
// SOLID    : SRP - solo presenta información de seguridad.
// =============================================================================

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

        public FrmPoliticas(
            string titulo,
            IReadOnlyList<PoliticaSeguridad> politicas,
            IReadOnlyList<AttackEvent> historialAtaques)
        {
            _titulo          = titulo;
            _politicas       = politicas;
            _historialAtaques = historialAtaques;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            SuspendLayout();

            Text            = $"🛡 {_titulo}";
            Size            = new Size(900, 720);
            MinimumSize     = new Size(800, 600);
            StartPosition   = FormStartPosition.CenterParent;
            BackColor       = ColorConstants.FondoPrincipal;
            ForeColor       = ColorConstants.TextoPrincipal;
            Font            = AppTheme.FuenteLabel;
            FormBorderStyle = FormBorderStyle.Sizable;

            // Header
            var panelHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 64,
                BackColor = ColorConstants.FondoPoliticas,
                Padding   = new Padding(20, 0, 20, 0)
            };
            panelHeader.Paint += (_, e) =>
            {
                using var pen = new Pen(ColorConstants.ColorSeguridad, 2);
                e.Graphics.DrawLine(pen, 0, panelHeader.Height - 2,
                    panelHeader.Width, panelHeader.Height - 2);
            };

            var lblIco = new Label
            {
                Text      = "🛡",
                Font      = new Font("Segoe UI Emoji", 24f),
                ForeColor = ColorConstants.ColorSeguridad,
                AutoSize  = false,
                Size      = new Size(48, 60),
                Location  = new Point(20, 2),
                TextAlign = ContentAlignment.MiddleCenter
            };

            var lblTit = new Label
            {
                Text      = _titulo,
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = ColorConstants.ColorSeguridad,
                AutoSize  = true,
                Location  = new Point(72, 14)
            };

            var lblSub = new Label
            {
                Text =
                    $"{_historialAtaques.Count} eventos de ataque registrados  " +
                    $"| {_politicas.Count} políticas de prevención",
                Font      = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.TextoHint,
                AutoSize  = true,
                Location  = new Point(74, 38)
            };

            panelHeader.Controls.AddRange(
                new Control[] { lblIco, lblTit, lblSub });

            // TabControl para organizar el contenido
            var tabs = new TabControl
            {
                Dock      = DockStyle.Fill,
                Font      = AppTheme.FuenteLabelBold,
                Appearance = TabAppearance.Normal
            };

            // Aplicar estilo al TabControl
            tabs.DrawMode = TabDrawMode.OwnerDrawFixed;
            tabs.DrawItem += Tabs_DrawItem;

            var tabPoliticas  = new TabPage("🛡 Políticas de Prevención");
            var tabEvidencia  = new TabPage("🔍 Evidencia del Ataque");
            var tabResumen    = new TabPage("📋 Resumen Ejecutivo");

            tabPoliticas.BackColor = ColorConstants.FondoPrincipal;
            tabEvidencia.BackColor = ColorConstants.FondoPrincipal;
            tabResumen.BackColor   = ColorConstants.FondoPrincipal;

            ConstruirTabPoliticas(tabPoliticas);
            ConstruirTabEvidencia(tabEvidencia);
            ConstruirTabResumen(tabResumen);

            tabs.TabPages.AddRange(new[]
                { tabPoliticas, tabEvidencia, tabResumen });

            // Botones inferiores
            var panelBotones = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 48,
                BackColor = ColorConstants.FondoSuperior,
                Padding   = new Padding(12, 8, 12, 8)
            };

            var btnCerrar = new Button
            {
                Text      = "✓ Cerrar",
                Size      = new Size(110, 32),
                Font      = AppTheme.FuenteBoton,
                Anchor    = AnchorStyles.Right | AnchorStyles.Top
            };
            AppTheme.AplicarABotonPrimario(btnCerrar);
            btnCerrar.Location =
                new Point(panelBotones.Width - 126, 8);
            panelBotones.Resize += (_, _) =>
                btnCerrar.Location =
                    new Point(panelBotones.Width - 126, 8);
            btnCerrar.Click += (_, _) => Close();

            var lblDisclaimer = new Label
            {
                Text =
                    "⚠ Este módulo es estrictamente educativo. " +
                    "Ningún ataque real fue ejecutado durante la simulación.",
                Font      = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.TextoHint,
                AutoSize  = true,
                Location  = new Point(12, 14)
            };

            panelBotones.Controls.AddRange(
                new Control[] { lblDisclaimer, btnCerrar });

            Controls.AddRange(new Control[]
                { tabs, panelBotones, panelHeader });

            ResumeLayout(true);
        }

        // ─── TAB POLÍTICAS ────────────────────────────────────────────────────

        private void ConstruirTabPoliticas(TabPage tab)
        {
            var scroll = new Panel
            {
                Dock      = DockStyle.Fill,
                AutoScroll = true,
                BackColor = ColorConstants.FondoPrincipal,
                Padding   = new Padding(12)
            };

            int y = 8;

            foreach (var politica in _politicas)
            {
                var card = ConstruirCardPolitica(politica, y);
                scroll.Controls.Add(card);
                y += card.Height + 10;
            }

            // Ajustar tamaño del scroll
            scroll.AutoScrollMinSize = new Size(0, y + 20);
            tab.Controls.Add(scroll);
        }

        private Panel ConstruirCardPolitica(PoliticaSeguridad p, int y)
        {
            var panel = new Panel
            {
                Location  = new Point(8, y),
                Size      = new Size(820, 118),
                BackColor = ColorConstants.FondoCard,
                Anchor    = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
            };
            panel.Paint += (_, e) =>
            {
                using var pen = new Pen(ColorConstants.ColorSeguridad, 1);
                e.Graphics.DrawRectangle(pen, 0, 0,
                    panel.Width - 1, panel.Height - 1);
                // Barra izquierda de acento
                using var brush = new SolidBrush(ColorConstants.ColorSeguridad);
                e.Graphics.FillRectangle(brush, 0, 0, 4, panel.Height);
            };

            // Badge de categoría
            var lblCat = new Label
            {
                Text      = p.Categoria,
                Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = ColorConstants.TextoHint,
                AutoSize  = true,
                Location  = new Point(14, 6)
            };

            // Título
            var lblTit = new Label
            {
                Text      = p.Titulo,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = ColorConstants.ColorSeguridad,
                AutoSize  = false,
                Size      = new Size(panel.Width - 20, 22),
                Location  = new Point(14, 20)
            };

            // Descripción
            var lblDesc = new Label
            {
                Text      = p.Descripcion,
                Font      = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.TextoPrincipal,
                AutoSize  = false,
                Size      = new Size((panel.Width / 2) - 20, 40),
                Location  = new Point(14, 46)
            };

            // Vulnerabilidad explotada
            var lblVulnTit = new Label
            {
                Text      = "Vulnerabilidad:",
                Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = ColorConstants.AlertaAtaque,
                AutoSize  = true,
                Location  = new Point(panel.Width / 2, 46)
            };

            var lblVuln = new Label
            {
                Text      = p.Vulnerabilidad,
                Font      = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.EstadoAlterado,
                AutoSize  = false,
                Size      = new Size((panel.Width / 2) - 20, 30),
                Location  = new Point(panel.Width / 2, 62)
            };

            // Implementación
            var lblImplTit = new Label
            {
                Text      = "Implementación:",
                Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = ColorConstants.AcentoPrincipal,
                AutoSize  = true,
                Location  = new Point(14, 90)
            };
            var lblImpl = new Label
            {
                Text      = p.Implementacion,
                Font      = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.TextoSecundario,
                AutoSize  = false,
                Size      = new Size(panel.Width - 20, 16),
                Location  = new Point(100, 91)
            };

            panel.Controls.AddRange(new Control[]
            {
                lblCat, lblTit, lblDesc,
                lblVulnTit, lblVuln,
                lblImplTit, lblImpl
            });

            // Ajuste responsive al resize del tab
            panel.Resize += (_, _) =>
            {
                int w = panel.Width;
                lblTit.Width   = w - 20;
                lblDesc.Width  = (w / 2) - 20;
                lblVulnTit.Location = new Point(w / 2, 46);
                lblVuln.Location    = new Point(w / 2, 62);
                lblVuln.Width       = (w / 2) - 20;
                lblImpl.Width       = w - 20;
            };

            return panel;
        }

        // ─── TAB EVIDENCIA ────────────────────────────────────────────────────

        private void ConstruirTabEvidencia(TabPage tab)
        {
            var panelMain = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = ColorConstants.FondoPrincipal,
                Padding   = new Padding(12)
            };

            // Resumen de impacto
            var panelImpacto = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 68,
                BackColor = ColorConstants.FondoAtaque,
                Padding   = new Padding(12, 8, 12, 8),
                Margin    = new Padding(0, 0, 0, 8)
            };
            panelImpacto.Paint += (_, e) =>
            {
                using var pen = new Pen(ColorConstants.AlertaAtaque, 1);
                e.Graphics.DrawRectangle(pen, 0, 0,
                    panelImpacto.Width - 1, panelImpacto.Height - 1);
            };

            var lblImpTit = new Label
            {
                Text      = $"⚡ Total de eventos de ataque registrados: {_historialAtaques.Count}",
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = ColorConstants.AlertaAtaque,
                AutoSize  = true,
                Location  = new Point(12, 8)
            };

            var lblImpSub = new Label
            {
                Text =
                    "Los siguientes eventos ocurrieron durante la simulación del ataque. " +
                    "En un escenario real, cada uno representaría un impacto concreto en el negocio.",
                Font      = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.TextoSecundario,
                AutoSize  = false,
                Size      = new Size(820, 30),
                Location  = new Point(12, 30)
            };

            panelImpacto.Controls.AddRange(
                new Control[] { lblImpTit, lblImpSub });

            // Grid de eventos
            var gridEventos = new DataGridView { Dock = DockStyle.Fill };
            AppTheme.AplicarADataGrid(gridEventos);

            gridEventos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Hora", HeaderText = "Hora", Width = 80, FillWeight = 10
            });
            gridEventos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Tipo", HeaderText = "Tipo de Ataque", FillWeight = 18
            });
            gridEventos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Componente", HeaderText = "Componente", FillWeight = 18
            });
            gridEventos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Descripcion", HeaderText = "Descripción", FillWeight = 30
            });
            gridEventos.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "Impacto", HeaderText = "Impacto en Negocio", FillWeight = 24
            });

            foreach (var ev in _historialAtaques)
            {
                int idx = gridEventos.Rows.Add(
                    ev.Timestamp.ToString("HH:mm:ss"),
                    ev.TipoAtaque.ToString(),
                    ev.ComponenteAfectado,
                    ev.Descripcion.Length > 45
                        ? ev.Descripcion[..45] + "…" : ev.Descripcion,
                    ev.ImpactoNegocio.Length > 40
                        ? ev.ImpactoNegocio[..40] + "…" : ev.ImpactoNegocio);

                gridEventos.Rows[idx].DefaultCellStyle.ForeColor =
                    ColorConstants.EstadoAlterado;
            }

            panelMain.Controls.Add(gridEventos);
            panelMain.Controls.Add(panelImpacto);
            tab.Controls.Add(panelMain);
        }

        // ─── TAB RESUMEN EJECUTIVO ────────────────────────────────────────────

        private void ConstruirTabResumen(TabPage tab)
        {
            var scroll = new RichTextBox
            {
                Dock        = DockStyle.Fill,
                BackColor   = ColorConstants.FondoPrincipal,
                ForeColor   = ColorConstants.TextoPrincipal,
                Font        = AppTheme.FuenteLabel,
                BorderStyle = BorderStyle.None,
                ReadOnly    = true,
                Padding     = new Padding(16)
            };

            scroll.SelectionFont =
                new Font("Segoe UI", 14f, FontStyle.Bold);
            scroll.SelectionColor = ColorConstants.ColorSeguridad;
            scroll.AppendText("RESUMEN EJECUTIVO DE SEGURIDAD\n\n");

            scroll.SelectionFont  = AppTheme.FuenteLabelBold;
            scroll.SelectionColor = ColorConstants.AlertaAtaque;
            scroll.AppendText("━━ QUÉ OCURRIÓ ━━━━━━━━━━━━━━━━━━━━━━━\n");
            scroll.SelectionFont  = AppTheme.FuenteLabel;
            scroll.SelectionColor = ColorConstants.TextoPrincipal;
            scroll.AppendText(
                "Se ejecutó una simulación educativa de ataque de ingeniería social " +
                "sobre el sistema del restaurante. El atacante explotó la confianza " +
                "del personal para obtener acceso no autorizado al sistema.\n\n");

            scroll.SelectionFont  = AppTheme.FuenteLabelBold;
            scroll.SelectionColor = ColorConstants.EstadoEsperando;
            scroll.AppendText("━━ VECTOR DE ATAQUE ━━━━━━━━━━━━━━━━━━\n");
            scroll.SelectionFont  = AppTheme.FuenteLabel;
            scroll.SelectionColor = ColorConstants.TextoPrincipal;
            scroll.AppendText(
                "Ingeniería Social: El atacante se presentó como personal técnico " +
                "legítimo, creando urgencia artificial para obtener acceso inmediato " +
                "sin seguir los procedimientos establecidos de verificación.\n\n");

            scroll.SelectionFont  = AppTheme.FuenteLabelBold;
            scroll.SelectionColor = ColorConstants.AlertaAtaque;
            scroll.AppendText("━━ ERROR HUMANO PRINCIPAL ━━━━━━━━━━━━━\n");
            scroll.SelectionFont  = AppTheme.FuenteLabel;
            scroll.SelectionColor = ColorConstants.TextoPrincipal;
            scroll.AppendText(
                "El personal otorgó acceso sin:\n" +
                "  • Verificar la identidad con credenciales escritas\n" +
                "  • Obtener aprobación escrita de la gerencia\n" +
                "  • Consultar al departamento de IT\n" +
                "  • Seguir el proceso de gestión de cambios establecido\n\n");

            scroll.SelectionFont  = AppTheme.FuenteLabelBold;
            scroll.SelectionColor = ColorConstants.ColorSeguridad;
            scroll.AppendText("━━ CONTROLES RECOMENDADOS ━━━━━━━━━━━━\n");
            scroll.SelectionFont  = AppTheme.FuenteLabel;
            scroll.SelectionColor = ColorConstants.TextoPrincipal;
            scroll.AppendText(
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
                "  • Plan de respuesta a incidentes documentado y probado\n\n");

            scroll.SelectionFont  = AppTheme.FuenteLabelBold;
            scroll.SelectionColor = ColorConstants.AcentoPrincipal;
            scroll.AppendText("━━ MARCOS DE REFERENCIA ━━━━━━━━━━━━━━\n");
            scroll.SelectionFont  = AppTheme.FuenteLabel;
            scroll.SelectionColor = ColorConstants.TextoSecundario;
            scroll.AppendText(
                "• NIST Cybersecurity Framework (CSF 2.0)\n" +
                "• ISO/IEC 27001:2022 — Gestión de Seguridad de la Información\n" +
                "• CIS Controls v8 — Control 14: Security Awareness\n" +
                "• OWASP — Social Engineering Prevention\n" +
                "• MITRE ATT&CK — T1566 (Phishing)\n");

            tab.Controls.Add(scroll);
        }

        // ─── DIBUJO DE TABS ───────────────────────────────────────────────────

        private void Tabs_DrawItem(object? sender, DrawItemEventArgs e)
        {
            var tab = (TabControl)sender!;
            var page = tab.TabPages[e.Index];

            bool selected = e.Index == tab.SelectedIndex;
            var backColor = selected
                ? ColorConstants.FondoCard
                : ColorConstants.FondoPanel;
            var foreColor = selected
                ? ColorConstants.ColorSeguridad
                : ColorConstants.TextoSecundario;

            using var backBrush = new SolidBrush(backColor);
            e.Graphics.FillRectangle(backBrush, e.Bounds);

            TextRenderer.DrawText(e.Graphics, page.Text,
                new Font("Segoe UI", 9f,
                    selected ? FontStyle.Bold : FontStyle.Regular),
                e.Bounds, foreColor,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter);

            if (selected)
            {
                using var pen = new Pen(ColorConstants.ColorSeguridad, 2);
                e.Graphics.DrawLine(pen,
                    e.Bounds.Left, e.Bounds.Bottom - 2,
                    e.Bounds.Right, e.Bounds.Bottom - 2);
            }
        }
    }
}