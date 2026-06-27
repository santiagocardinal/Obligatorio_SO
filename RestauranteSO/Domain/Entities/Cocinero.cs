// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Domain/Entities/Cocinero.cs
// Propósito: Representa un cocinero que consume pedidos de la cola.
//            Cada cocinero corresponde a un hilo Consumidor.
// SOLID    : Single Responsibility.
// =============================================================================

using RestauranteSO.Domain.Enums;

namespace RestauranteSO.Domain.Entities
{
    /// <summary>
    /// Representa un cocinero del restaurante.
    /// En la simulación Productor-Consumidor, cada Cocinero tiene un Thread
    /// que toma pedidos de la ConcurrentQueue y los "prepara" (simula trabajo).
    /// 
    /// El Cocinero solo puede preparar UN pedido a la vez.
    /// Si la cola está vacía, su hilo queda bloqueado en el semáforo.
    /// </summary>
    public class Cocinero
    {
        /// <summary>
        /// Identificador único del cocinero.
        /// También es el nombre del Thread asociado.
        /// Ejemplo: "Cocinero-1", "Chef-Principal"
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Especialidad del cocinero para mostrar en la UI.
        /// Ejemplo: "Parrilla", "Pastas", "Postres"
        /// </summary>
        public string Especialidad { get; }

        /// <summary>
        /// Pedido que el cocinero está preparando actualmente.
        /// null cuando está esperando en el semáforo (cola vacía).
        /// </summary>
        public Pedido? PedidoActual { get; set; }

        /// <summary>
        /// Total de pedidos preparados por este cocinero.
        /// Usado en las estadísticas de rendimiento.
        /// </summary>
        public int PedidosPreparados { get; set; }

        /// <summary>
        /// Indica si el hilo de este cocinero está activo.
        /// </summary>
        public bool EstaActivo { get; set; }

        /// <summary>
        /// Estado actual del hilo del cocinero.
        /// Refleja si está preparando, esperando, o pausado.
        /// </summary>
        public SimulationStatus Estado { get; set; }

        /// <summary>
        /// Timestamp de cuando este cocinero fue agregado a la simulación.
        /// </summary>
        public DateTime FechaCreacion { get; }

        /// <summary>
        /// Progreso de preparación del pedido actual (0-100).
        /// Usado para la ProgressBar en la UI.
        /// </summary>
        public int ProgresoActual { get; set; }

        public Cocinero(string id, string especialidad)
        {
            Id = id;
            Especialidad = especialidad;
            PedidosPreparados = 0;
            EstaActivo = false;
            Estado = SimulationStatus.Detenida;
            FechaCreacion = DateTime.Now;
            ProgresoActual = 0;
        }

        public override string ToString() =>
            $"{Id} [{Especialidad}] - {PedidosPreparados} preparados" +
            (PedidoActual != null ? $" | Preparando: #{PedidoActual.NumeroPedido}" : " | Esperando pedido...");
    }
}