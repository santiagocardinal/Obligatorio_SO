// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Domain/Models/SimulationStatistics.cs
// Propósito: DTO con estadísticas de rendimiento de la simulación.
//            Snapshot inmutable tomado en un punto del tiempo.
// SOLID    : Single Responsibility. Solo datos, sin lógica.
// =============================================================================

namespace RestauranteSO.Domain.Models
{
    /// <summary>
    /// Data Transfer Object con estadísticas de la simulación en un momento dado.
    /// 
    /// Es un SNAPSHOT: se crea en un instante y no cambia.
    /// Los servicios crean una nueva instancia en cada llamada a ObtenerEstadisticas().
    /// Esto garantiza que la UI siempre trabaja con datos consistentes.
    /// 
    /// Thread Safety: Immutable por diseño. No requiere sincronización.
    /// </summary>
    public sealed class SimulationStatistics
    {
        // ─── GENERAL ─────────────────────────────────────────────────────────

        /// <summary>Tiempo total que lleva corriendo la simulación.</summary>
        public TimeSpan TiempoEjecucion { get; init; }

        /// <summary>Estado actual de la simulación.</summary>
        public string EstadoActual { get; init; } = string.Empty;

        // ─── PRODUCTOR-CONSUMIDOR ────────────────────────────────────────────

        /// <summary>Cantidad de pedidos actualmente en la cola de espera.</summary>
        public int PedidosEnCola { get; init; }

        /// <summary>Capacidad máxima configurada para la cola.</summary>
        public int CapacidadMaximaCola { get; init; }

        /// <summary>Porcentaje de ocupación de la cola (0-100).</summary>
        public int PorcentajeOcupacionCola =>
            CapacidadMaximaCola > 0
                ? (int)((PedidosEnCola / (double)CapacidadMaximaCola) * 100)
                : 0;

        /// <summary>Cantidad de productores (clientes) activos.</summary>
        public int ProductoresActivos { get; init; }

        /// <summary>Cantidad de consumidores (cocineros) activos.</summary>
        public int ConsumidoresActivos { get; init; }

        /// <summary>Total acumulado de pedidos generados.</summary>
        public int TotalPedidosGenerados { get; init; }

        /// <summary>Total acumulado de pedidos completados.</summary>
        public int TotalPedidosCompletados { get; init; }

        /// <summary>Total de pedidos cancelados.</summary>
        public int TotalPedidosCancelados { get; init; }

        /// <summary>Total de pedidos alterados por ataques.</summary>
        public int TotalPedidosAlterados { get; init; }

        /// <summary>Throughput: pedidos completados por segundo.</summary>
        public double PedidosPorSegundo =>
            TiempoEjecucion.TotalSeconds > 0
                ? TotalPedidosCompletados / TiempoEjecucion.TotalSeconds
                : 0;

        // ─── LECTORES-ESCRITORES ─────────────────────────────────────────────

        /// <summary>Cantidad de lectores leyendo actualmente.</summary>
        public int LectoresActivos { get; init; }

        /// <summary>Cantidad de lectores esperando (bloqueados).</summary>
        public int LectoresEsperando { get; init; }

        /// <summary>Indica si hay un escritor escribiendo ahora.</summary>
        public bool EscritorActivo { get; init; }

        /// <summary>Indica si hay escritores esperando para escribir.</summary>
        public bool EscritorEsperando { get; init; }

        /// <summary>Total de lecturas completadas.</summary>
        public int TotalLecturas { get; init; }

        /// <summary>Total de escrituras completadas.</summary>
        public int TotalEscrituras { get; init; }

        /// <summary>Lectores que leyeron menú comprometido.</summary>
        public int LectoresComprometidos { get; init; }

        // ─── SEGURIDAD ───────────────────────────────────────────────────────

        /// <summary>Indica si hay un ataque activo.</summary>
        public bool AtaqueActivo { get; init; }

        /// <summary>Total de eventos de ataque registrados.</summary>
        public int TotalEventosAtaque { get; init; }

        /// <summary>Tipo del ataque activo (o "Ninguno").</summary>
        public string TipoAtaque { get; init; } = "Ninguno";
    }
}