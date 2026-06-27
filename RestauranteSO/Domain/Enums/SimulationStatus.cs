// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Domain/Enums/SimulationStatus.cs
// Propósito: Estado global de una simulación en ejecución.
// SOLID    : Single Responsibility.
// =============================================================================

namespace RestauranteSO.Domain.Enums
{
    /// <summary>
    /// Estado operativo de cualquier simulación del sistema.
    /// Compartido por ProductorConsumidor y LectoresEscritores.
    /// Implementa el principio de Open/Closed: nuevos estados
    /// pueden agregarse sin modificar el código existente.
    /// </summary>
    public enum SimulationStatus
    {
        /// <summary>
        /// La simulación no ha sido iniciada todavía.
        /// Estado inicial al abrir la ventana.
        /// </summary>
        Detenida = 0,

        /// <summary>
        /// La simulación está corriendo. Los hilos están activos.
        /// Todos los workers están ejecutándose.
        /// </summary>
        Corriendo = 1,

        /// <summary>
        /// La simulación fue pausada temporalmente.
        /// Los hilos están suspendidos mediante ManualResetEvent.
        /// </summary>
        Pausada = 2,

        /// <summary>
        /// La simulación está siendo detenida.
        /// El CancellationToken fue cancelado, esperando que
        /// todos los hilos finalicen limpiamente.
        /// </summary>
        Deteniendo = 3,

        /// <summary>
        /// Un ataque está activo sobre esta simulación.
        /// Los workers están operando en modo comprometido.
        /// </summary>
        BajoAtaque = 4
    }
}