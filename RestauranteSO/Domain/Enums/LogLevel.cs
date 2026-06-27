// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Domain/Enums/LogLevel.cs
// Propósito: Niveles de severidad para el sistema de logging en tiempo real.
// SOLID    : Single Responsibility.
// =============================================================================

namespace RestauranteSO.Domain.Enums
{
    /// <summary>
    /// Niveles de log utilizados por ISimulationLogger.
    /// Permite filtrar y colorear los mensajes en el LogViewer.
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// Información general de operación normal.
        /// Color en UI: Blanco / Gris claro.
        /// </summary>
        Info = 0,

        /// <summary>
        /// Evento de concurrencia: lock adquirido, semáforo decrementado, etc.
        /// Color en UI: Azul claro.
        /// </summary>
        Sync = 1,

        /// <summary>
        /// Advertencia: cola casi llena, hilo demorado, etc.
        /// Color en UI: Amarillo.
        /// </summary>
        Warning = 2,

        /// <summary>
        /// Error en la simulación o condición inesperada.
        /// Color en UI: Rojo.
        /// </summary>
        Error = 3,

        /// <summary>
        /// Evento relacionado con un ataque simulado.
        /// Color en UI: Naranja brillante.
        /// </summary>
        Attack = 4,

        /// <summary>
        /// Acción preventiva o política de seguridad.
        /// Color en UI: Verde.
        /// </summary>
        Security = 5
    }
}