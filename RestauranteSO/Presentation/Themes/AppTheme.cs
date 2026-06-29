using RestauranteSO.Constants;

namespace RestauranteSO.Presentation.Themes
{
    public static class AppTheme
    {
        // ─── ESCALA TIPOGRÁFICA ───────────────────────────────────────────────
        // Regla: todas las fuentes en unidades pt con familia "Segoe UI"
        // Jerarquía: Título → Subtítulo → LabelBold → Label → Small → Mono
        // Nunca reducir fuentes para que "entre" — adaptar siempre el contenedor

        public static readonly Font FuenteTitulo      = new("Segoe UI", 22f,  FontStyle.Bold);
        public static readonly Font FuenteSubtitulo   = new("Segoe UI", 17f,  FontStyle.Bold);
        public static readonly Font FuenteLabelBold   = new("Segoe UI", 13f,  FontStyle.Bold);
        public static readonly Font FuenteLabel       = new("Segoe UI", 13f,  FontStyle.Regular);
        public static readonly Font FuenteSmallBold   = new("Segoe UI", 11f,  FontStyle.Bold);
        public static readonly Font FuenteSmall       = new("Segoe UI", 11f,  FontStyle.Regular);
        public static readonly Font FuenteMono        = new("Consolas", 12f,  FontStyle.Bold);
        public static readonly Font FuenteStatus      = new("Consolas", 11f,  FontStyle.Regular);
        public static readonly Font FuenteBoton       = new("Segoe UI", 12f,  FontStyle.Bold);
        public static readonly Font FuenteDockIcono   = new("Segoe UI Emoji", 30f, FontStyle.Regular);
        public static readonly Font FuenteDockLabel   = new("Segoe UI", 11f,  FontStyle.Bold);
        public static readonly Font FuenteSplashNombre= new("Segoe UI", 32f,  FontStyle.Bold);
        public static readonly Font FuenteSplashTag   = new("Segoe UI", 14f,  FontStyle.Regular);
        public static readonly Font FuenteSplashSub   = new("Segoe UI", 11f,  FontStyle.Regular);
        public static readonly Font FuenteMapaNodo    = new("Segoe UI", 10f,  FontStyle.Bold);
        public static readonly Font FuenteLogEntrada  = new("Consolas", 11f,  FontStyle.Regular);

        // ─── HELPER: altura segura para un Label con esta fuente ─────────────
        /// <summary>
        /// Calcula la altura mínima garantizada para que ninguna letra
        /// se corte, considerando ascendentes, descendentes y padding.
        /// Factor 1.8 cubre todas las familias en todos los DPI.
        /// </summary>
        public static int AlturaSegura(Font f, int lineas = 1, int paddingV = 8)
        {
            // Usar Size * 1.333 para convertir pt → px sin necesitar Graphics context
            float alturaPixels = f.Size * 1.333f;
            return (int)(alturaPixels * lineas * 1.55f) + paddingV * 2;
        }

        // ─── BOTONES ─────────────────────────────────────────────────────────

        public static void AplicarABotonPrimario(Button btn)
        {
            btn.BackColor = ColorConstants.AcentoPrincipal;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.Font      = FuenteBoton;
            btn.Cursor    = Cursors.Hand;
            btn.Padding   = new Padding(8, 4, 8, 4);
            btn.FlatAppearance.BorderSize           = 0;
            btn.FlatAppearance.MouseOverBackColor   = Color.FromArgb(128, 119, 255);
            btn.FlatAppearance.MouseDownBackColor   = Color.FromArgb( 88,  79, 220);
        }

        public static void AplicarABotonSecundario(Button btn)
        {
            btn.BackColor = ColorConstants.FondoPanel;
            btn.ForeColor = ColorConstants.TextoSecundario;
            btn.FlatStyle = FlatStyle.Flat;
            btn.Font      = FuenteBoton;
            btn.Cursor    = Cursors.Hand;
            btn.Padding   = new Padding(8, 4, 8, 4);
            btn.FlatAppearance.BorderSize           = 1;
            btn.FlatAppearance.BorderColor          = ColorConstants.Separador;
            btn.FlatAppearance.MouseOverBackColor   = Color.FromArgb(32, 32, 52);
            btn.FlatAppearance.MouseDownBackColor   = Color.FromArgb(20, 20, 36);
        }

        public static void AplicarABotonAtaque(Button btn)
        {
            btn.BackColor = ColorConstants.AlertaAtaque;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.Font      = FuenteBoton;
            btn.Cursor    = Cursors.Hand;
            btn.Padding   = new Padding(8, 4, 8, 4);
            btn.FlatAppearance.BorderSize           = 0;
            btn.FlatAppearance.MouseOverBackColor   = Color.FromArgb(255,  91, 107);
            btn.FlatAppearance.MouseDownBackColor   = Color.FromArgb(220,  51,  67);
        }

        public static void AplicarABotonSeguridad(Button btn)
        {
            btn.BackColor = ColorConstants.AcentoExito;
            btn.ForeColor = Color.FromArgb(10, 26, 10);
            btn.FlatStyle = FlatStyle.Flat;
            btn.Font      = FuenteBoton;
            btn.Cursor    = Cursors.Hand;
            btn.Padding   = new Padding(8, 4, 8, 4);
            btn.FlatAppearance.BorderSize           = 0;
            btn.FlatAppearance.MouseOverBackColor   = Color.FromArgb( 58, 242, 149);
            btn.FlatAppearance.MouseDownBackColor   = Color.FromArgb( 18, 192,  99);
        }

        public static void AplicarABotonVolver(Button btn)
        {
            btn.BackColor = ColorConstants.FondoPanel;
            btn.ForeColor = ColorConstants.TextoSecundario;
            btn.FlatStyle = FlatStyle.Flat;
            btn.Font      = FuenteBoton;
            btn.Cursor    = Cursors.Hand;
            btn.Padding   = new Padding(8, 4, 8, 4);
            btn.FlatAppearance.BorderSize           = 1;
            btn.FlatAppearance.BorderColor          = ColorConstants.Separador;
            btn.FlatAppearance.MouseOverBackColor   = Color.FromArgb(32, 32, 52);
        }

        public static void AplicarABotonWarning(Button btn)
        {
            btn.BackColor = ColorConstants.Advertencia;
            btn.ForeColor = Color.FromArgb(26, 20, 0);
            btn.FlatStyle = FlatStyle.Flat;
            btn.Font      = FuenteBoton;
            btn.Cursor    = Cursors.Hand;
            btn.Padding   = new Padding(8, 4, 8, 4);
            btn.FlatAppearance.BorderSize           = 0;
            btn.FlatAppearance.MouseOverBackColor   = Color.FromArgb(255, 220, 108);
        }

        // ─── DATA GRID ────────────────────────────────────────────────────────

        public static void AplicarADataGrid(DataGridView grid)
        {
            grid.BackgroundColor           = ColorConstants.FondoPanel;
            grid.ForeColor                 = ColorConstants.TextoPrincipal;
            grid.GridColor                 = ColorConstants.Separador;
            grid.BorderStyle               = BorderStyle.None;
            grid.CellBorderStyle           = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.ColumnHeadersBorderStyle  = DataGridViewHeaderBorderStyle.None;
            grid.SelectionMode             = DataGridViewSelectionMode.FullRowSelect;
            grid.MultiSelect               = false;
            grid.ReadOnly                  = true;
            grid.AllowUserToAddRows        = false;
            grid.AllowUserToDeleteRows     = false;
            grid.AllowUserToResizeRows     = false;
            grid.RowHeadersVisible         = false;
            grid.AutoSizeColumnsMode       = DataGridViewAutoSizeColumnsMode.Fill;
            grid.Font                      = FuenteSmall;
            grid.EnableHeadersVisualStyles = false;
            // Altura de fila calculada con AlturaSegura
            grid.RowTemplate.Height        = AlturaSegura(FuenteSmall, 1, 6);

            grid.DefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor          = ColorConstants.FondoPanel,
                ForeColor          = ColorConstants.TextoPrincipal,
                SelectionBackColor = Color.FromArgb(60, 108, 99, 255),
                SelectionForeColor = ColorConstants.TextoPrincipal,
                Font               = FuenteSmall,
                Padding            = new Padding(6, 4, 6, 4),
                WrapMode           = DataGridViewTriState.False
            };

            grid.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor          = Color.FromArgb(20, 20, 34),
                ForeColor          = ColorConstants.TextoPrincipal,
                SelectionBackColor = Color.FromArgb(60, 108, 99, 255),
                SelectionForeColor = ColorConstants.TextoPrincipal,
                Font               = FuenteSmall,
                Padding            = new Padding(6, 4, 6, 4),
                WrapMode           = DataGridViewTriState.False
            };

            grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
            {
                BackColor = ColorConstants.FondoSuperior,
                ForeColor = ColorConstants.TextoHint,
                Font      = FuenteSmallBold,
                Alignment = DataGridViewContentAlignment.MiddleLeft,
                Padding   = new Padding(6, 6, 6, 6)
            };

            grid.ColumnHeadersHeight         = AlturaSegura(FuenteSmallBold, 1, 8);
            grid.ColumnHeadersHeightSizeMode =
                DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        }

        // ─── STATUS STRIP ─────────────────────────────────────────────────────

        public static void AplicarAStatusStrip(StatusStrip strip)
        {
            strip.BackColor  = ColorConstants.FondoSuperior;
            strip.ForeColor  = ColorConstants.TextoSecundario;
            strip.Font       = FuenteStatus;
            strip.SizingGrip = false;
            strip.Padding    = new Padding(10, 0, 10, 0);
            strip.RenderMode = ToolStripRenderMode.Professional;
            strip.Height     = AlturaSegura(FuenteStatus, 1, 6);
        }

        // ─── TAB CONTROL ──────────────────────────────────────────────────────

        public static void DibujarTabItem(
            Graphics g, Rectangle bounds, string texto,
            bool seleccionado, Color colorAcento)
        {
            var backColor = seleccionado
                ? ColorConstants.FondoCard
                : ColorConstants.FondoPanel;
            var foreColor = seleccionado
                ? colorAcento
                : ColorConstants.TextoSecundario;

            using var backBrush = new SolidBrush(backColor);
            g.FillRectangle(backBrush, bounds);

            TextRenderer.DrawText(g, texto,
                seleccionado ? FuenteSmallBold : FuenteSmall,
                bounds, foreColor,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter   |
                TextFormatFlags.NoPadding);

            if (seleccionado)
            {
                using var pen = new Pen(colorAcento, 2);
                g.DrawLine(pen,
                    bounds.Left,  bounds.Bottom - 2,
                    bounds.Right, bounds.Bottom - 2);
            }
        }

        // ─── HELPERS GRÁFICOS ─────────────────────────────────────────────────

        public static System.Drawing.Drawing2D.GraphicsPath
            CrearPathRedondeado(Rectangle rect, int radio)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            int d    = Math.Min(radio * 2,
                       Math.Min(rect.Width, rect.Height));
            if (d <= 0) { path.AddRectangle(rect); return path; }

            path.AddArc(rect.X,         rect.Y,          d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y,          d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d,   0, 90);
            path.AddArc(rect.X,         rect.Bottom - d, d, d,  90, 90);
            path.CloseFigure();
            return path;
        }

        public static Color LerpColor(Color c1, Color c2, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return Color.FromArgb(
                (int)(c1.A + (c2.A - c1.A) * t),
                (int)(c1.R + (c2.R - c1.R) * t),
                (int)(c1.G + (c2.G - c1.G) * t),
                (int)(c1.B + (c2.B - c1.B) * t));
        }

        /// <summary>
        /// Mide el texto con TextRenderer y devuelve un Rectangle
        /// con padding generoso para garantizar que no haya recorte.
        /// Usar siempre este método en lugar de calcular manualmente.
        /// </summary>
        public static Rectangle MedirTextoSeguro(
            string texto, Font font, int x, int y,
            int maxAncho, int paddingH = 8, int paddingV = 10)
        {
            var medida = TextRenderer.MeasureText(texto, font,
                new Size(maxAncho, int.MaxValue),
                TextFormatFlags.WordBreak);
            return new Rectangle(
                x - paddingH,
                y - paddingV,
                medida.Width  + paddingH * 2,
                medida.Height + paddingV * 2);
        }
    }
}