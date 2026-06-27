// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Program.cs
// Propósito: Punto de entrada de la aplicación.
//            Configura el entorno, inicializa DI y arranca el Dashboard.
// =============================================================================

using RestauranteSO.Configuration;
using RestauranteSO.Presentation.Forms;

namespace RestauranteSO
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal de RestauranteSO.
        ///
        /// ORDEN DE INICIALIZACIÓN:
        ///   1. Configurar el rendering de WinForms (DPI, visual styles).
        ///   2. Configurar el manejador global de excepciones no capturadas.
        ///   3. Inicializar el contenedor de DI (AppSettings.Configurar()).
        ///   4. Abrir el Dashboard principal.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // ── Configuración de WinForms ─────────────────────────────────
            // ApplicationConfiguration.Initialize() hace tres cosas:
            //   1. Application.EnableVisualStyles(): activa temas de Windows.
            //   2. Application.SetCompatibleTextRenderingDefault(false):
            //      usa GDI+ para texto en lugar del antiguo GDI.
            //   3. Application.SetHighDpiMode(): soporte para pantallas 4K.
            ApplicationConfiguration.Initialize();

            // ── Manejador global de excepciones ───────────────────────────
            // Captura excepciones no manejadas para mostrar mensaje amigable
            // en lugar de crashear silenciosamente.
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException +=
                CurrentDomain_UnhandledException;

            // ── Inicializar DI ────────────────────────────────────────────
            try
            {
                AppSettings.Configurar();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al inicializar la aplicación:\n\n{ex.Message}",
                    "RestauranteSO - Error de Inicialización",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            // ── Abrir Dashboard ───────────────────────────────────────────
            Application.Run(new FrmDashboard());
        }

        private static void Application_ThreadException(
            object sender, ThreadExceptionEventArgs e)
        {
            MostrarErrorFatal(e.Exception);
        }

        private static void CurrentDomain_UnhandledException(
            object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
                MostrarErrorFatal(ex);
        }

        private static void MostrarErrorFatal(Exception ex)
        {
            MessageBox.Show(
                $"Error inesperado en RestauranteSO:\n\n" +
                $"{ex.Message}\n\n" +
                $"Fuente: {ex.Source}\n\n" +
                $"Por favor reinicie la aplicación.",
                "RestauranteSO - Error Inesperado",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}