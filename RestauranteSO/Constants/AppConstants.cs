// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Constants/AppConstants.cs
// Propósito: Constantes globales de la aplicación.
//            Centraliza todos los valores fijos para fácil mantenimiento.
// SOLID    : DRY - un único lugar para modificar valores.
// =============================================================================

namespace RestauranteSO.Constants
{
    /// <summary>
    /// Constantes globales de la aplicación RestauranteSO.
    /// Todas las clases deben usar estas constantes en lugar de valores hardcodeados.
    /// </summary>
    public static class AppConstants
    {
        // ─── APLICACIÓN ──────────────────────────────────────────────────────

        /// <summary>Nombre completo de la aplicación.</summary>
        public const string NombreApp = "RestauranteSO";

        /// <summary>Nombre del restaurante para mostrar en la UI.</summary>
        public const string NombreRestaurante = "Restaurante CAFFA-PEÑAROL";

        /// <summary>Subtítulo descriptivo del proyecto.</summary>
        public const string Subtitulo = "Simulador de Sistemas Operativos";

        /// <summary>Versión actual de la aplicación.</summary>
        public const string Version = "1.0.0";

        /// <summary>Año del proyecto para el copyright.</summary>
        public const string Anio = "2025";

        // ─── SIMULACIÓN PRODUCTOR-CONSUMIDOR ─────────────────────────────────

        /// <summary>Capacidad máxima de la cola de pedidos.</summary>
        public const int CapacidadMaximaCola = 15;

        /// <summary>Número inicial de productores (clientes).</summary>
        public const int ProductoresIniciales = 2;

        /// <summary>Número inicial de consumidores (cocineros).</summary>
        public const int ConsumidoresIniciales = 2;

        /// <summary>Máximo de productores permitidos simultáneamente.</summary>
        public const int MaxProductores = 8;

        /// <summary>Máximo de consumidores permitidos simultáneamente.</summary>
        public const int MaxConsumidores = 6;

        /// <summary>Intervalo base entre creaciones de pedidos (ms).</summary>
        public const int VelocidadProductorBaseMs = 1500;

        /// <summary>Tiempo base de preparación de un pedido (ms).</summary>
        public const int VelocidadConsumidorBaseMs = 3000;

        // ─── SIMULACIÓN LECTORES-ESCRITORES ──────────────────────────────────

        /// <summary>Número inicial de lectores (meseros).</summary>
        public const int LectoresIniciales = 3;

        /// <summary>Máximo de lectores simultáneos.</summary>
        public const int MaxLectores = 10;

        /// <summary>Intervalo base entre lecturas del menú (ms).</summary>
        public const int VelocidadLectorBaseMs = 800;

        /// <summary>Tiempo base de escritura del menú (ms).</summary>
        public const int VelocidadEscritorBaseMs = 2000;

        /// <summary>Intervalo entre modificaciones del gerente (ms).</summary>
        public const int IntervaloModificacionGerenteMs = 5000;

        // ─── UI ──────────────────────────────────────────────────────────────

        /// <summary>Intervalo del timer de actualización de la UI (ms).</summary>
        public const int IntervalActualizacionUIMs = 150;

        /// <summary>Cantidad máxima de logs visibles en el LogViewer.</summary>
        public const int MaxLogsVisibles = 200;

        /// <summary>Cantidad máxima de pedidos en la tabla de historial.</summary>
        public const int MaxPedidosHistorial = 100;

        /// <summary>Ancho mínimo del Dashboard.</summary>
        public const int DashboardAnchoMinimo = 1100;

        /// <summary>Alto mínimo del Dashboard.</summary>
        public const int DashboardAltoMinimo = 700;
    }
}