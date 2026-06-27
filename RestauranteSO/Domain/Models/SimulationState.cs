// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Domain/Models/SimulationState.cs
// Propósito: Estado mutable compartido entre workers de una simulación.
//            Accedido por múltiples hilos con sincronización apropiada.
// SOLID    : Single Responsibility.
// =============================================================================

using RestauranteSO.Domain.Enums;

namespace RestauranteSO.Domain.Models
{
    /// <summary>
    /// Estado compartido y mutable de una simulación en ejecución.
    /// 
    /// A diferencia de SimulationStatistics (que es un snapshot inmutable),
    /// SimulationState es el estado VIVO que los workers modifican en tiempo real.
    /// 
    /// Thread Safety:
    /// - Los campos numéricos usan Interlocked para operaciones atómicas.
    /// - El estado general usa volatile para visibilidad entre hilos.
    /// - Las colecciones usan tipos Concurrent del namespace System.Collections.
    /// </summary>
    public class SimulationState
    {
        // ─── ESTADO GENERAL ──────────────────────────────────────────────────

        /// <summary>
        /// Estado actual de la simulación.
        /// volatile garantiza que todos los hilos ven el valor actualizado
        /// sin que el compilador lo cache en un registro.
        /// </summary>
        private volatile SimulationStatus _estado = SimulationStatus.Detenida;
        public SimulationStatus Estado
        {
            get => _estado;
            set => _estado = value;
        }

        /// <summary>Timestamp de inicio de la simulación.</summary>
        public DateTime? InicioSimulacion { get; set; }

        // ─── CONTADORES ATÓMICOS ─────────────────────────────────────────────
        // Usar Interlocked.Increment/Decrement para operaciones thread-safe
        // sin necesidad de lock explícito.

        private int _totalPedidosGenerados = 0;
        /// <summary>Total de pedidos creados. Usar Interlocked.Increment.</summary>
        public int TotalPedidosGenerados => _totalPedidosGenerados;
        public int IncrementarPedidosGenerados() =>
            Interlocked.Increment(ref _totalPedidosGenerados);

        private int _totalPedidosCompletados = 0;
        /// <summary>Total de pedidos completados. Usar Interlocked.Increment.</summary>
        public int TotalPedidosCompletados => _totalPedidosCompletados;
        public int IncrementarPedidosCompletados() =>
            Interlocked.Increment(ref _totalPedidosCompletados);

        private int _totalPedidosCancelados = 0;
        public int TotalPedidosCancelados => _totalPedidosCancelados;
        public int IncrementarPedidosCancelados() =>
            Interlocked.Increment(ref _totalPedidosCancelados);

        private int _totalPedidosAlterados = 0;
        public int TotalPedidosAlterados => _totalPedidosAlterados;
        public int IncrementarPedidosAlterados() =>
            Interlocked.Increment(ref _totalPedidosAlterados);

        private int _totalLecturas = 0;
        public int TotalLecturas => _totalLecturas;
        public int IncrementarLecturas() =>
            Interlocked.Increment(ref _totalLecturas);

        private int _totalEscrituras = 0;
        public int TotalEscrituras => _totalEscrituras;
        public int IncrementarEscrituras() =>
            Interlocked.Increment(ref _totalEscrituras);

        private int _totalEventosAtaque = 0;
        public int TotalEventosAtaque => _totalEventosAtaque;
        public int IncrementarEventosAtaque() =>
            Interlocked.Increment(ref _totalEventosAtaque);

        // ─── ESTADO DE ATAQUE ────────────────────────────────────────────────

        private volatile bool _ataqueActivo = false;
        public bool AtaqueActivo
        {
            get => _ataqueActivo;
            set => _ataqueActivo = value;
        }

        // ─── MÉTODO DE RESET ─────────────────────────────────────────────────

        /// <summary>
        /// Resetea todos los contadores a cero.
        /// Llamado al reiniciar la simulación.
        /// No es thread-safe: llamar solo cuando la simulación está detenida.
        /// </summary>
        public void Reset()
        {
            _totalPedidosGenerados = 0;
            _totalPedidosCompletados = 0;
            _totalPedidosCancelados = 0;
            _totalPedidosAlterados = 0;
            _totalLecturas = 0;
            _totalEscrituras = 0;
            _totalEventosAtaque = 0;
            _ataqueActivo = false;
            InicioSimulacion = null;
            Estado = SimulationStatus.Detenida;
        }
    }
}