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
            _tituloBandeja = tituloBandeja;
            _mensajeAtacante = mensajeAtacante;
            _textoAceptar = textoAceptar;
            _textoRechazar = textoRechazar;
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            Text = _tituloBandeja;
            Size = new Size(680, 760);
            MaximizeBox = false;
            MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            BackColor = ColorConstants.FondoPanel;
            ForeColor = ColorConstants.TextoPrincipal;
            Font = AppTheme.FuenteLabel;

            // Banner
            var panelBanner = new Panel
            {
                Dock = DockStyle.Top,
                Height = 68,
                BackColor = Color.FromArgb(30, 100, 200),
                Padding = new Padding(16, 0, 16, 0)
            };
            panelBanner.Controls.AddRange(new Control[]
            {
                new Label
                {
                    Text = "🔧",
                    Font = new Font("Segoe UI Emoji", 24f),
                    ForeColor = Color.White,
                    BackColor = Color.FromArgb(30, 100, 200),
                    AutoSize = false,
                    Size = new Size(60, 60),
                    Location = new Point(16, 4),
                    TextAlign = ContentAlignment.MiddleCenter
                },
                new Label
                {
                    Text = "Soporte Técnico — SistemaResto S.A.",
                    Font = new Font("Segoe UI", 13f, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = Color.FromArgb(30, 100, 200),
                    AutoSize = true,
                    Location = new Point(80, 14)
                },
                new Label
                {
                    Text = "Departamento de Tecnología y Sistemas",
                    Font = new Font("Segoe UI", 10f),
                    ForeColor = Color.FromArgb(200, 220, 255),
                    BackColor = Color.FromArgb(30, 100, 200),
                    AutoSize = true,
                    Location = new Point(82, 40)
                }
            });

            // Badge educativo
            var panelEdu = new Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(60, 80, 20, 0),
                Padding = new Padding(16, 0, 16, 0)
            };
            panelEdu.Controls.Add(new Label
            {
                Text = "⚠ SIMULACIÓN EDUCATIVA — Este mensaje simula ingeniería social. Nada real ocurre.",
                Font = AppTheme.FuenteSmallBold,
                ForeColor = ColorConstants.AcentoSecundario,
                BackColor = Color.FromArgb(60, 80, 20, 0),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft
            });

            // Botones
            var panelBotones = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 72,
                BackColor = ColorConstants.FondoSuperior,
                Padding = new Padding(20, 12, 20, 12)
            };
            var tblBotones = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = ColorConstants.FondoSuperior
            };
            tblBotones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tblBotones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
            tblBotones.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var btnAceptar = new Button
            {
                Text = _textoAceptar,
                Dock = DockStyle.Fill,
                Font = AppTheme.FuenteBoton,
                BackColor = ColorConstants.AlertaAtaque,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 0)
            };
            btnAceptar.FlatAppearance.BorderSize = 0;
            btnAceptar.Click += (_, _) => { DialogResult = DialogResult.OK; Close(); };

            var btnRechazar = new Button
            {
                Text = _textoRechazar + " ✓",
                Dock = DockStyle.Fill,
                Font = AppTheme.FuenteBoton,
                BackColor = ColorConstants.AcentoExito,
                ForeColor = Color.FromArgb(10, 26, 10),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(8, 0, 0, 0)
            };
            btnRechazar.FlatAppearance.BorderSize = 0;
            btnRechazar.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

            tblBotones.Controls.Add(btnAceptar, 0, 0);
            tblBotones.Controls.Add(btnRechazar, 1, 0);
            panelBotones.Controls.Add(tblBotones);

            // Scroll central
            var scroll = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = ColorConstants.FondoPanel,
                Padding = new Padding(20, 16, 8, 16)
            };

            int anchoTexto = 590;
            int y = 8;

            var lblMsgTit = CrearLabel("📨 Mensaje recibido:", 10f, FontStyle.Bold,
                ColorConstants.TextoSecundario, ColorConstants.FondoPanel, new Point(0, y), true);
            scroll.Controls.Add(lblMsgTit);
            y += 28;

            var rtb = new RichTextBox
            {
                Text = _mensajeAtacante,
                Font = AppTheme.FuenteLabel,
                BackColor = Color.FromArgb(28, 28, 46),
                ForeColor = ColorConstants.TextoPrincipal,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                ScrollBars = RichTextBoxScrollBars.None,
                WordWrap = true,
                Location = new Point(12, 12),
                Size = new Size(anchoTexto - 12, 300)
            };
            rtb.Height = TextRenderer.MeasureText(_mensajeAtacante, rtb.Font, new Size(rtb.Width, int.MaxValue), TextFormatFlags.WordBreak).Height + 24;
            var panelMensaje = new Panel
            {
                Location = new Point(0, y),
                Size = new Size(anchoTexto + 8, rtb.Height + 24),
                BackColor = Color.FromArgb(28, 28, 46)
            };
            panelMensaje.Controls.Add(rtb);
            scroll.Controls.Add(panelMensaje);
            y += panelMensaje.Height + 16;

            // Señales de alerta
            var lblAlertaTit = CrearLabel("🔴 Señales de Ingeniería Social detectadas:",
                10f, FontStyle.Bold, ColorConstants.AlertaAtaque, ColorConstants.FondoPanel, new Point(0, y), true);
            scroll.Controls.Add(lblAlertaTit);
            y += 28;

            var alertas = new[]
            {
                "• Urgencia artificial — \"instalación crítica urgente\"",
                "• Solicita acceso sin verificación de identidad previa",
                "• Presión temporal para decidir sin consultar a superiores",
                "• Nombre genérico de empresa sin credenciales verificables",
                "• Amenaza implícita de pérdida de datos si no actúa ya"
            };
            var panelAlertas = CrearPanelLista(alertas, y, Color.FromArgb(40, 220, 50, 50), ColorConstants.AlertaAtaque, ColorConstants.TextoPrincipal);
            scroll.Controls.Add(panelAlertas);
            y += panelAlertas.Height + 16;

            // Consecuencias
            var lblConsecTit = CrearLabel("⚡ ¿Qué ocurre si presionás cada botón?",
                10f, FontStyle.Bold, ColorConstants.AcentoPrincipal, ColorConstants.FondoPanel, new Point(0, y), true);
            scroll.Controls.Add(lblConsecTit);
            y += 30;

            var consecSi = new[]
            {
                "🔴 Se activa el agente malicioso en la cola de pedidos",
                "🔴 25% de los pedidos serán DUPLICADOS en la cola",
                "🔴 20% de los pedidos serán ELIMINADOS silenciosamente",
                "🔴 55% de los pedidos tendrán su contenido ALTERADO",
                "🔴 Los cocineros procesarán pedidos incorrectos",
                "🔴 El sistema pierde integridad — datos no confiables",
                "🔴 Los clientes recibirán platos equivocados o nada"
            };
            var panelSi = CrearPanelListaConTitulo("✓ Sí, adelante — CONSECUENCIAS DEL ATAQUE:",
                consecSi, y, Color.FromArgb(35, 220, 50, 50), ColorConstants.AlertaAtaque, Color.FromArgb(255, 200, 200));
            scroll.Controls.Add(panelSi);
            y += panelSi.Height + 10;

            var consecNo = new[]
            {
                "✅ El ataque NO se activa — la cola queda íntegra",
                "✅ Los pedidos se procesan correctamente",
                "✅ Se aplicó verificación de identidad previa",
                "✅ Correcto: nunca dar acceso sin autorización escrita",
                "✅ Sistema operando con total integridad y confiabilidad"
            };
            var panelNo = CrearPanelListaConTitulo("✗ No, esperaré — ACCIÓN CORRECTA:",
                consecNo, y, Color.FromArgb(30, 38, 222, 129), ColorConstants.AcentoExito, Color.FromArgb(180, 255, 220));
            scroll.Controls.Add(panelNo);
            y += panelNo.Height + 10;

            // Concepto SO
            var panelConcepto = new Panel
            {
                Location = new Point(0, y),
                Width = anchoTexto + 8,
                BackColor = Color.FromArgb(25, 108, 99, 255),
                Padding = new Padding(12)
            };
            panelConcepto.Paint += (_, e) =>
            {
                using var pen = new Pen(Color.FromArgb(80, ColorConstants.AcentoPrincipal), 1);
                e.Graphics.DrawRectangle(pen, 0, 0, panelConcepto.Width - 1, panelConcepto.Height - 1);
            };
            var lblConTit = CrearLabel("📚 Concepto de Sistemas Operativos:",
                10f, FontStyle.Bold, ColorConstants.AcentoPrincipal, Color.FromArgb(25, 108, 99, 255),
                new Point(12, 10), false, anchoTexto - 20);
            lblConTit.Height = 24;
            string textoConcepto =
                "Este ataque simula una violación al problema Productor-Consumidor.\n" +
                "Un agente malicioso se introduce en el buffer compartido (ConcurrentQueue)\n" +
                "e inyecta, duplica o elimina elementos antes de que el consumidor\n" +
                "(cocinero) los procese. El SemaphoreSlim controla acceso concurrente\n" +
                "pero no puede detectar contenido malicioso. La seguridad del contenido\n" +
                "requiere validación adicional en la capa de aplicación.";
            var lblConTexto = new Label
            {
                Text = textoConcepto,
                Font = AppTheme.FuenteSmall,
                ForeColor = ColorConstants.TextoSecundario,
                BackColor = Color.FromArgb(25, 108, 99, 255),
                AutoSize = false,
                Width = anchoTexto - 24,
                Height = 120,
                Location = new Point(12, 40),
                TextAlign = ContentAlignment.TopLeft
            };
            panelConcepto.Controls.Add(lblConTit);
            panelConcepto.Controls.Add(lblConTexto);
            panelConcepto.Height = 40 + lblConTexto.Height + 20;
            scroll.Controls.Add(panelConcepto);
            y += panelConcepto.Height + 16;

            scroll.AutoScrollMinSize = new Size(0, y + 24);

            Controls.AddRange(new Control[] { panelBotones, scroll, panelEdu, panelBanner });
        }

        private static Label CrearLabel(string texto, float size, FontStyle style, Color fore, Color back, Point loc, bool autoSize, int w = 520)
        {
            var lbl = new Label
            {
                Text = texto,
                Font = new Font("Segoe UI", size, style),
                ForeColor = fore,
                BackColor = back,
                Location = loc,
                AutoSize = autoSize,
                TextAlign = ContentAlignment.MiddleLeft
            };
            if (!autoSize) lbl.Width = w;
            return lbl;
        }

        private static Panel CrearPanelLista(string[] items, int y, Color back, Color borderColor, Color foreColor)
        {
            var panel = new Panel
            {
                Location = new Point(0, y),
                Width = 598,
                BackColor = back,
                Padding = new Padding(12, 8, 12, 8)
            };
            panel.Paint += (_, e) =>
            {
                using var pen = new Pen(borderColor, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
            };

            int iy = 4;
            foreach (var item in items)
            {
                panel.Controls.Add(new Label
                {
                    Text = item,
                    Font = AppTheme.FuenteSmall,
                    ForeColor = foreColor,
                    BackColor = back,
                    AutoSize = false,
                    Width = panel.Width - 20,
                    Height = 24,
                    Location = new Point(8, iy),
                    TextAlign = ContentAlignment.MiddleLeft
                });
                iy += 26;
            }
            panel.Height = iy + 8;
            return panel;
        }

        private static Panel CrearPanelListaConTitulo(string titulo, string[] items, int y, Color back, Color colorTitulo, Color foreColor)
        {
            var panel = new Panel
            {
                Location = new Point(0, y),
                Width = 598,
                BackColor = back,
                Padding = new Padding(12, 8, 12, 8)
            };
            panel.Paint += (_, e) =>
            {
                using var pen = new Pen(colorTitulo, 1);
                e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
            };

            panel.Controls.Add(new Label
            {
                Text = titulo,
                Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = colorTitulo,
                BackColor = back,
                AutoSize = false,
                Width = panel.Width - 20,
                Height = 26,
                Location = new Point(8, 4),
                TextAlign = ContentAlignment.MiddleLeft
            });

            int iy = 34;
            foreach (var item in items)
            {
                panel.Controls.Add(new Label
                {
                    Text = item,
                    Font = AppTheme.FuenteSmall,
                    ForeColor = foreColor,
                    BackColor = back,
                    AutoSize = false,
                    Width = panel.Width - 20,
                    Height = 24,
                    Location = new Point(8, iy),
                    TextAlign = ContentAlignment.MiddleLeft
                });
                iy += 26;
            }
            panel.Height = iy + 8;
            return panel;
        }
    }
}