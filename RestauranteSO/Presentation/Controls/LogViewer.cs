using RestauranteSO.Constants;
using RestauranteSO.Domain.Models;
using RestauranteSO.Presentation.Themes;

namespace RestauranteSO.Presentation.Controls
{
    public sealed class LogViewer : UserControl
    {
        // ─── CONTROLES (sin readonly) ─────────────────────────────────────────

        private RichTextBox _richText    = null!;
        private Panel       _headerPanel = null!;
        private Label       _lblTitulo   = null!;
        private Button      _btnLimpiar  = null!;

        private int  _cantidadLineas = 0;
        private bool _autoScroll     = true;
        private const int MaxLineas  = AppConstants.MaxLogsVisibles;

        // ─── CONSTRUCTOR ─────────────────────────────────────────────────────

        public LogViewer()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            BackColor = ColorConstants.FondoPanel;
            Padding   = new Padding(0);

            // Header
            _headerPanel = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 32,
                BackColor = ColorConstants.FondoSuperior,
                Padding   = new Padding(8, 0, 4, 0)
            };

            _lblTitulo = new Label
            {
                Text      = "📋 Log de Eventos",
                ForeColor = ColorConstants.TextoSecundario,
                Font      = AppTheme.FuenteLabelBold,
                Dock      = DockStyle.Left,
                AutoSize  = false,
                Width     = 200,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _btnLimpiar = new Button
            {
                Text      = "Limpiar",
                Dock      = DockStyle.Right,
                Width     = 65,
                Font      = AppTheme.FuenteSmall,
                FlatStyle = FlatStyle.Flat,
                ForeColor = ColorConstants.TextoHint,
                BackColor = Color.Transparent,
                Cursor    = Cursors.Hand
            };
            _btnLimpiar.FlatAppearance.BorderSize = 0;
            _btnLimpiar.Click += (_, _) => Limpiar();

            _headerPanel.Controls.Add(_lblTitulo);
            _headerPanel.Controls.Add(_btnLimpiar);

            // RichTextBox
            _richText = new RichTextBox
            {
                Dock        = DockStyle.Fill,
                BackColor   = ColorConstants.FondoPanel,
                ForeColor   = ColorConstants.TextoPrincipal,
                Font        = AppTheme.FuenteLog,
                ReadOnly    = true,
                BorderStyle = BorderStyle.None,
                ScrollBars  = RichTextBoxScrollBars.Vertical,
                WordWrap    = false
            };

            _richText.VScroll += (_, _) =>
            {
                var pos   = _richText.GetPositionFromCharIndex(
                    _richText.TextLength);
                _autoScroll = pos.Y <= _richText.Height + 20;
            };

            Controls.Add(_richText);
            Controls.Add(_headerPanel);
        }

        // ─── API PÚBLICA ─────────────────────────────────────────────────────

        public void AgregarLog(LogEntry entry)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => AgregarLogInterno(entry));
                return;
            }
            AgregarLogInterno(entry);
        }

        public void AgregarLogs(IEnumerable<LogEntry> entries)
        {
            if (InvokeRequired)
            {
                BeginInvoke(() => AgregarLogsInterno(entries));
                return;
            }
            AgregarLogsInterno(entries);
        }

        public void Limpiar()
        {
            if (InvokeRequired) { BeginInvoke(Limpiar); return; }
            _richText.Clear();
            _cantidadLineas = 0;
        }

        public void SetTitulo(string titulo)
        {
            _lblTitulo.Text = titulo;
        }

        // ─── PRIVADOS ─────────────────────────────────────────────────────────

        private void AgregarLogInterno(LogEntry entry)
        {
            // Proteger contra acceso después de dispose
            if (_richText == null || _richText.IsDisposed) return;

            if (_cantidadLineas >= MaxLineas)
            {
                int fin = _richText.Text.IndexOf('\n');
                if (fin >= 0)
                {
                    _richText.Select(0, fin + 1);
                    _richText.SelectedText = string.Empty;
                    _cantidadLineas--;
                }
            }

            _richText.SelectionStart  = _richText.TextLength;
            _richText.SelectionLength = 0;
            _richText.SelectionColor  = entry.ColorTexto;
            _richText.AppendText(entry.TextoCompleto + "\n");
            _cantidadLineas++;

            if (_autoScroll)
            {
                _richText.SelectionStart = _richText.TextLength;
                _richText.ScrollToCaret();
            }
        }

        private void AgregarLogsInterno(IEnumerable<LogEntry> entries)
        {
            _richText.SuspendLayout();
            try
            {
                foreach (var e in entries)
                    AgregarLogInterno(e);
            }
            finally
            {
                _richText.ResumeLayout();
            }
        }
    }
}