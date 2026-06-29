using RestauranteSO.Constants;
using RestauranteSO.Domain.Models;
using RestauranteSO.Presentation.Themes;

namespace RestauranteSO.Presentation.Controls
{
    public sealed class LogViewer : UserControl
    {
        private RichTextBox _richText    = null!;
        private Panel       _headerPanel = null!;
        private Label       _lblTitulo   = null!;
        private Button      _btnLimpiar  = null!;

        private int  _cantidadLineas = 0;
        private bool _autoScroll     = true;
        private const int MaxLineas  = AppConstants.MaxLogsVisibles;

        public LogViewer()
        {
            InitializeComponents();
        }

        private void InitializeComponents()
        {
            BackColor = ColorConstants.FondoPanel;
            Padding   = new Padding(0);

            _headerPanel = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 38,
                BackColor = ColorConstants.FondoSuperior,
                Padding   = new Padding(8, 0, 4, 0)
            };

            _lblTitulo = new Label
            {
                Text      = "📋 Log de Eventos",
                ForeColor = ColorConstants.TextoSecundario,
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                BackColor = ColorConstants.FondoSuperior,
                Dock      = DockStyle.Left,
                AutoSize  = false,
                Width     = 220,
                TextAlign = ContentAlignment.MiddleLeft
            };

            _btnLimpiar = new Button
            {
                Text      = "Limpiar",
                Dock      = DockStyle.Right,
                Width     = 72,
                Font      = new Font("Segoe UI", 10f, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                ForeColor = ColorConstants.TextoHint,
                BackColor = ColorConstants.FondoSuperior,
                Cursor    = Cursors.Hand
            };
            _btnLimpiar.FlatAppearance.BorderSize = 0;
            _btnLimpiar.Click += (_, _) => Limpiar();

            _headerPanel.Controls.Add(_lblTitulo);
            _headerPanel.Controls.Add(_btnLimpiar);

            _richText = new RichTextBox
            {
                Dock        = DockStyle.Fill,
                BackColor   = ColorConstants.FondoPanel,
                ForeColor   = ColorConstants.TextoPrincipal,
                Font        = new Font("Consolas", 11f, FontStyle.Regular),
                ReadOnly    = true,
                BorderStyle = BorderStyle.None,
                ScrollBars  = RichTextBoxScrollBars.Vertical,
                WordWrap    = false
            };

            _richText.VScroll += (_, _) =>
            {
                var pos = _richText.GetPositionFromCharIndex(
                    _richText.TextLength);
                _autoScroll = pos.Y <= _richText.Height + 20;
            };

            Controls.Add(_richText);
            Controls.Add(_headerPanel);
        }

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

        private void AgregarLogInterno(LogEntry entry)
        {
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