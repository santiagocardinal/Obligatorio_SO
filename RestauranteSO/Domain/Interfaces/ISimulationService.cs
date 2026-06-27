// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Domain/Interfaces/ISimulationService.cs
// Propósito: Contrato que deben cumplir todos los servicios de simulación.
//            Permite que la UI dependa de abstracciones, no implementaciones.
// SOLID    : Interface Segregation (ISP) + Dependency Inversion (DIP).
// Patrón   : Strategy - la UI puede trabajar con cualquier simulación.
// =============================================================================

using RestauranteSO.Domain.Enums;
using RestauranteSO.Domain.Models;

namespace RestauranteSO.Domain.Interfaces
{
    /// <summary>
    /// Contrato base para todos los servicios de simulación del sistema.
    /// 
    /// Principio ISP: Esta interfaz solo define las operaciones COMUNES
    /// a todas las simulaciones. Las operaciones específicas están en
    /// interfaces derivadas (IProductorConsumidorService, etc.).
    /// 
    /// Principio DIP: Los formularios WinForms dependen de esta interfaz,
    /// no de las clases concretas ProductorConsumidorService.
    /// 
    /// Patrón Strategy: Permite que un mismo formulario "base" pueda
    /// trabajar con cualquier tipo de simulación.
    /// </summary>
    public interface ISimulationService
    {
        // ─── CICLO DE VIDA ────────────────────────────────────────────────────

        /// <summary>
        /// Inicia la simulación creando y arrancando todos los hilos.
        /// Lanza InvalidOperationException si ya está corriendo.
        /// </summary>
        void Iniciar();

        /// <summary>
        /// Detiene la simulación cancelando todos los hilos gracefully.
        /// Espera a que todos los hilos terminen antes de retornar.
        /// </summary>
        void Detener();

        /// <summary>
        /// Pausa la simulación bloqueando los hilos en un ManualResetEvent.
        /// Los hilos no son destruidos, solo suspendidos.
        /// </summary>
        void Pausar();

        /// <summary>
        /// Reanuda una simulación pausada liberando el ManualResetEvent.
        /// </summary>
        void Reanudar();

        // ─── ESTADO ───────────────────────────────────────────────────────────

        /// <summary>
        /// Estado actual de la simulación.
        /// </summary>
        SimulationStatus Estado { get; }

        /// <summary>
        /// Retorna true si la simulación está actualmente en ejecución.
        /// Equivalente a Estado == SimulationStatus.Corriendo.
        /// </summary>
        bool EstaCorreindo { get; }

        /// <summary>
        /// Retorna true si hay un ataque activo sobre esta simulación.
        /// </summary>
        bool EstaUnderAttack { get; }

        // ─── ESTADÍSTICAS ─────────────────────────────────────────────────────

        /// <summary>
        /// Retorna un snapshot de las estadísticas actuales de la simulación.
        /// Este método es thread-safe y puede llamarse desde el UI thread.
        /// </summary>
        SimulationStatistics ObtenerEstadisticas();

        // ─── CONFIGURACIÓN ────────────────────────────────────────────────────

        /// <summary>
        /// Ajusta la velocidad de la simulación en tiempo real.
        /// </summary>
        /// <param name="velocidadMs">Intervalo entre operaciones en milisegundos</param>
        void AjustarVelocidad(int velocidadMs);

        // ─── EVENTOS ──────────────────────────────────────────────────────────

        /// <summary>
        /// Evento disparado cuando el estado de la simulación cambia.
        /// Subscrito por la UI para actualizar controles.
        /// IMPORTANTE: Siempre hacer Invoke() al actualizar la UI desde este evento.
        /// </summary>
        event EventHandler<SimulationStatus> EstadoCambiado;

        /// <summary>
        /// Evento disparado cuando hay una actualización disponible para la UI.
        /// Se dispara aproximadamente cada 100ms desde los workers.
        /// </summary>
        event EventHandler ActualizacionDisponible;
    }
}