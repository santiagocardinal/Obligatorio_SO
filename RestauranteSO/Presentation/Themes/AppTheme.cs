using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using RestauranteSO.Constants;

namespace RestauranteSO.Presentation.Themes
{
    public static class AppTheme
    {
        // ─── FUENTES ─────────────────────────────────────────────────────────
        // Escala tipográfica clara con jerarquía visual fuerte

        public static readonly Font FuenteHero        = new Font("Segoe UI", 36f, FontStyle.Bold);
        public static readonly Font FuenteTitulo      = new Font("Segoe UI", 22f, FontStyle.Bold);
        public static readonly Font FuenteSubtitulo   = new Font("Segoe UI", 16f, FontStyle.Regular);
        public static readonly Font FuenteCard        = new Font("Segoe UI", 14f, FontStyle.Bold);
        public static readonly Font FuenteCardDesc    = new Font("Segoe UI", 12f, FontStyle.Regular);
        public static readonly Font FuenteBoton       = new Font("Segoe UI", 11f, FontStyle.Bold);
        public static readonly Font FuenteLabel       = new Font("Segoe UI", 11f, FontStyle.Regular);
        public static readonly Font FuenteLabelBold   = new Font("Segoe UI", 11f, FontStyle.Bold);
        public static readonly Font FuenteLog         = new Font("Consolas",  10f, FontStyle.Regular);
        public static readonly Font FuenteSmall       = new Font("Segoe UI",  9f,  FontStyle.Regular);
        public static readonly Font FuenteSmallBold   = new Font("Segoe UI",  9f,  FontStyle.Bold);
        public static readonly Font FuenteIconGrande  = new Font("Segoe UI Emoji", 36f, FontStyle.Regular);
        public static readonly Font FuenteStatus      = new Font("Consolas",  10f, FontStyle.Regular);

        // ─── APLICAR A FORM ───────────────────────────────────────────────────

        public static void AplicarAForm(Form form)
        {
            form.BackColor     = ColorConstants.FondoPrincipal;
            form.ForeColor     = ColorConstants.TextoPrincipal;
            form.Font          = FuenteLabel;
            form.StartPosition = FormStartPosition.CenterScreen;
        }

        // ─── BOTONES ─────────────────────────────────────────────────────────

        public static void AplicarABotonPrimario(Button btn)
        {
            btn.BackColor    = ColorConstants.AcentoPrincipal;
            btn.ForeColor    = Color.White;
            btn.Font         = FuenteBoton;
            btn.FlatStyle    = FlatStyle.Flat;
            btn.Cursor       = Cursors.Hand;
            btn.Height       = 44;
            btn.FlatAppearance.BorderSize           = 0;
            btn.FlatAppearance.MouseOverBackColor   =
                ControlPaint.Light(ColorConstants.AcentoPrincipal, 0.2f);
            btn.FlatAppearance.MouseDownBackColor   =
                ControlPaint.Dark(ColorConstants.AcentoPrincipal, 0.1f);
        }

        public static void AplicarABotonSecundario(Button btn)
        {
            btn.BackColor    = ColorConstants.FondoCard;
            btn.ForeColor    = ColorConstants.TextoSecundario;
            btn.Font         = FuenteBoton;
            btn.FlatStyle    = FlatStyle.Flat;
            btn.Cursor       = Cursors.Hand;
            btn.Height       = 44;
            btn.FlatAppearance.BorderSize           = 1;
            btn.FlatAppearance.BorderColor          = ColorConstants.BordeCard;
            btn.FlatAppearance.MouseOverBackColor   = ColorConstants.FondoCardHover;
            btn.FlatAppearance.MouseDownBackColor   = ColorConstants.FondoPanel;
        }

        public static void AplicarABotonAtaque(Button btn)
        {
            btn.BackColor    = ColorConstants.AlertaAtaque;
            btn.ForeColor    = Color.White;
            btn.Font         = FuenteBoton;
            btn.FlatStyle    = FlatStyle.Flat;
            btn.Cursor       = Cursors.Hand;
            btn.Height       = 44;
            btn.FlatAppearance.BorderSize           = 0;
            btn.FlatAppearance.MouseOverBackColor   =
                ControlPaint.Light(ColorConstants.AlertaAtaque, 0.2f);
        }

        public static void AplicarABotonSeguridad(Button btn)
        {
            btn.BackColor    = ColorConstants.AcentoExito;
            btn.ForeColor    = Color.White;
            btn.Font         = FuenteBoton;
            btn.FlatStyle    = FlatStyle.Flat;
            btn.Cursor       = Cursors.Hand;
            btn.Height       = 44;
            btn.FlatAppearance.BorderSize           = 0;
            btn.FlatAppearance.MouseOverBackColor   =
                ControlPaint.Light(ColorConstants.AcentoExito, 0.2f);
        }

        public static void AplicarABotonVolver(Button btn)
        {
            btn.BackColor    = ColorConstants.FondoCard;
            btn.ForeColor    = ColorConstants.AcentoPrincipal;
            btn.Font         = FuenteBoton;
            btn.FlatStyle    = FlatStyle.Flat;
            btn.Cursor       = Cursors.Hand;
            btn.Height       = 44;
            btn.FlatAppearance.BorderSize           = 1;
            btn.FlatAppearance.BorderColor          = ColorConstants.AcentoPrincipal;
            btn.FlatAppearance.MouseOverBackColor   =
                Color.FromArgb(20, ColorConstants.AcentoPrincipal);
        }

        // ─── DATAGRIDVIEW ─────────────────────────────────────────────────────

        public static void AplicarADataGrid(DataGridView grid)
        {
            grid.BackgroundColor      = ColorConstants.FondoPanel;
            grid.GridColor            = ColorConstants.Separador;
            grid.ForeColor            = ColorConstants.TextoPrincipal;
            grid.Font                 = FuenteSmall;
            grid.DefaultCellStyle.BackColor          = ColorConstants.FondoPanel;
            grid.DefaultCellStyle.ForeColor          = ColorConstants.TextoPrincipal;
            grid.DefaultCellStyle.SelectionBackColor = ColorConstants.AcentoPrincipal;
            grid.DefaultCellStyle.SelectionForeColor = Color.White;
            grid.DefaultCellStyle.Font               = FuenteSmall;
            grid.DefaultCellStyle.Padding            = new Padding(4, 2, 4, 2);
            grid.AlternatingRowsDefaultCellStyle.BackColor =
                Color.FromArgb(35, 35, 48);
            grid.ColumnHeadersDefaultCellStyle.BackColor  = ColorConstants.FondoSuperior;
            grid.ColumnHeadersDefaultCellStyle.ForeColor  = ColorConstants.TextoSecundario;
            grid.ColumnHeadersDefaultCellStyle.Font       = FuenteSmallBold;
            grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = ColorConstants.FondoSuperior;
            grid.ColumnHeadersBorderStyle         = DataGridViewHeaderBorderStyle.None;
            grid.ColumnHeadersHeightSizeMode      =
                DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.ColumnHeadersHeight              = 36;
            grid.RowTemplate.Height               = 32;
            grid.EnableHeadersVisualStyles        = false;
            grid.RowHeadersVisible                = false;
            grid.BorderStyle                      = BorderStyle.None;
            grid.CellBorderStyle                  = DataGridViewCellBorderStyle.SingleHorizontal;
            grid.SelectionMode                    = DataGridViewSelectionMode.FullRowSelect;
            grid.ReadOnly                         = true;
            grid.AllowUserToAddRows               = false;
            grid.AllowUserToDeleteRows            = false;
            grid.AllowUserToResizeRows            = false;
            grid.AutoSizeColumnsMode              = DataGridViewAutoSizeColumnsMode.Fill;
            grid.ScrollBars                       = ScrollBars.Vertical;
        }

        // ─── STATUS STRIP ─────────────────────────────────────────────────────

        public static void AplicarAStatusStrip(StatusStrip ss)
        {
            ss.BackColor  = ColorConstants.FondoInferior;
            ss.ForeColor  = ColorConstants.TextoSecundario;
            ss.Font       = FuenteSmall;
            ss.SizingGrip = false;
            ss.RenderMode = ToolStripRenderMode.Professional;
            ss.Height     = 28;
        }

        // ─── HELPERS GDI+ ────────────────────────────────────────────────────

        public static void DibujarSeparador(
            Graphics g, int x, int y, int ancho)
        {
            using var pen = new Pen(ColorConstants.Separador, 1);
            g.DrawLine(pen, x, y, x + ancho, y);
        }

        public static void DibujarBordeRedondeado(
            Graphics g, Rectangle rect, int radio,
            Color color, int grosor = 1)
        {
            using var pen  = new Pen(color, grosor);
            using var path = CrearPathRedondeado(rect, radio);
            g.DrawPath(pen, path);
        }

        public static void RellenarRedondeado(
            Graphics g, Rectangle rect, int radio, Color color)
        {
            if (rect.Width <= 0 || rect.Height <= 0) return;
            using var brush = new SolidBrush(color);
            using var path  = CrearPathRedondeado(rect, radio);
            g.FillPath(brush, path);
        }

        /// <summary>
        /// Dibuja un rectángulo redondeado con gradiente vertical.
        /// Usado para el fondo de las cards en hover.
        /// </summary>
        public static void RellenarGradienteRedondeado(
            Graphics g, Rectangle rect, int radio,
            Color colorArriba, Color colorAbajo)
        {
            if (rect.Width <= 0 || rect.Height <= 0) return;
            using var brush = new LinearGradientBrush(
                rect, colorArriba, colorAbajo,
                LinearGradientMode.Vertical);
            using var path = CrearPathRedondeado(rect, radio);
            g.FillPath(brush, path);
        }

        /// <summary>
        /// Simula una sombra dibujando capas semitransparentes desplazadas.
        /// Efecto de elevación para las cards.
        /// </summary>
        public static void DibujarSombra(
            Graphics g, Rectangle rect, int radio, int intensidad = 3)
        {
            for (int i = intensidad; i >= 1; i--)
            {
                var shadowRect = new Rectangle(
                    rect.X + i, rect.Y + i,
                    rect.Width, rect.Height);
                int alpha = (int)(15.0 / i);
                using var shadowBrush = new SolidBrush(
                    Color.FromArgb(alpha, 0, 0, 0));
                using var path = CrearPathRedondeado(shadowRect, radio);
                g.FillPath(shadowBrush, path);
            }
        }

        public static GraphicsPath CrearPathRedondeado(
            Rectangle rect, int radio)
        {
            var path = new GraphicsPath();
            int d    = radio * 2;

            d = Math.Min(d, Math.Min(rect.Width, rect.Height));
            if (d <= 0) { path.AddRectangle(rect); return path; }

            path.AddArc(rect.X,          rect.Y,          d, d, 180, 90);
            path.AddArc(rect.Right - d,  rect.Y,          d, d, 270, 90);
            path.AddArc(rect.Right - d,  rect.Bottom - d, d, d,   0, 90);
            path.AddArc(rect.X,          rect.Bottom - d, d, d,  90, 90);
            path.CloseFigure();
            return path;
        }

        public static Color ColorParaEstadoPedido(string estado) =>
            estado switch
            {
                "Esperando"     => ColorConstants.EstadoEsperando,
                "EnPreparacion" => ColorConstants.EstadoEnPreparacion,
                "Listo"         => ColorConstants.EstadoListo,
                "Entregado"     => ColorConstants.EstadoEntregado,
                "Cancelado"     => ColorConstants.EstadoEntregado,
                "Alterado"      => ColorConstants.EstadoAlterado,
                "Duplicado"     => ColorConstants.EstadoDuplicado,
                _               => ColorConstants.TextoPrincipal
            };
    }
}