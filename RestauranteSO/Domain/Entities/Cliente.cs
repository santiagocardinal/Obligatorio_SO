// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Domain/Entities/Cliente.cs
// Propósito: Representa un cliente del restaurante que genera pedidos.
//            Cada cliente corresponde a un hilo Productor en la simulación.
// SOLID    : Single Responsibility.
// =============================================================================

using RestauranteSO.Domain.Enums;

namespace RestauranteSO.Domain.Entities
{
    /// <summary>
    /// Representa un cliente del restaurante.
    /// En la simulación Productor-Consumidor, cada instancia de Cliente
    /// tiene un Thread dedicado que genera pedidos a intervalos regulares.
    /// 
    /// La UI muestra el estado de cada cliente en tiempo real.
    /// </summary>
    public class Cliente
    {
        /// <summary>
        /// Identificador único del cliente.
        /// También es el nombre del Thread asociado.
        /// Ejemplo: "Cliente-1", "Cliente-3"
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Número de mesa asignada a este cliente.
        /// Los pedidos de este cliente llevan este número de mesa.
        /// </summary>
        public int NumeroMesa { get; }

        /// <summary>
        /// Indica si el hilo de este cliente está activo y generando pedidos.
        /// </summary>
        public bool EstaActivo { get; set; }

        /// <summary>
        /// Total de pedidos que este cliente ha generado desde el inicio.
        /// Usado en las estadísticas de la simulación.
        /// </summary>
        public int PedidosGenerados { get; set; }

        /// <summary>
        /// Último pedido creado por este cliente.
        /// Puede ser null si todavía no generó ninguno.
        /// </summary>
        public Pedido? UltimoPedido { get; set; }

        /// <summary>
        /// Timestamp de creación del cliente (cuando se agregó el hilo).
        /// </summary>
        public DateTime FechaCreacion { get; }

        /// <summary>
        /// Estado actual del hilo de este cliente.
        /// Refleja si está generando un pedido, esperando, o pausado.
        /// </summary>
        public SimulationStatus Estado { get; set; }

        public Cliente(string id, int numeroMesa)
        {
            Id = id;
            NumeroMesa = numeroMesa;
            EstaActivo = false;
            PedidosGenerados = 0;
            FechaCreacion = DateTime.Now;
            Estado = SimulationStatus.Detenida;
        }

        public override string ToString() => 
            $"{Id} (Mesa {NumeroMesa}) - {PedidosGenerados} pedidos";
    }
}