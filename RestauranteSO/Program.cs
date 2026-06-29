using RestauranteSO.Configuration;
using RestauranteSO.Presentation.Forms;

namespace RestauranteSO
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.ThreadException += (_, e) =>
            {
                MessageBox.Show(
                    $"THREAD EXCEPTION:\n\n{e.Exception.Message}\n\n{e.Exception.StackTrace}",
                    "Debug Completo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            };

            AppSettings.Configurar();

            using (var splash = new FrmSplash())
            {
                splash.ShowDialog();
            }

            Application.Run(new FrmDesktop());
        }
    }
}