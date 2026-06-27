// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Domain/Enums/PedidoEstado.cs
// Propósito: Define todos los estados posibles que puede tener un pedido
//            a lo largo de su ciclo de vida en el restaurante.
// SOLID    : Single Responsibility - solo define estados, nada más.
// =============================================================================

namespace RestauranteSO.Domain.Enums
{
    /// <summary>
    /// Representa el ciclo de vida completo de un pedido en el restaurante.
    /// Utilizado por la simulación Productor-Consumidor para rastrear
    /// el estado de cada pedido en tiempo real.
    /// </summary>
    public enum PedidoEstado
    {
        /// <summary>
        /// El pedido fue creado por un cliente y espera en la cola.
        /// En este estado el pedido está en la ConcurrentQueue.
        /// </summary>
        Esperando = 0,

        /// <summary>
        /// Un cocinero tomó el pedido de la cola y lo está preparando.
        /// El semáforo fue decrementado al pasar a este estado.
        /// </summary>
        EnPreparacion = 1,

        /// <summary>
        /// El cocinero terminó de preparar el pedido.
        /// Está listo para ser entregado al cliente.
        /// </summary>
        Listo = 2,

        /// <summary>
        /// El mesero entregó el pedido al cliente.
        /// Estado final normal del ciclo.
        /// </summary>
        Entregado = 3,

        /// <summary>
        /// El pedido fue cancelado antes de ser preparado.
        /// Puede ocurrir por cancelación del cliente o por simulación de ataque.
        /// </summary>
        Cancelado = 4,

        /// <summary>
        /// El pedido fue alterado por un ataque de seguridad simulado.
        /// Solo ocurre cuando la simulación de ataque está activa.
        /// Permite distinguir visualmente pedidos comprometidos.
        /// </summary>
        Alterado = 5,

        /// <summary>
        /// El pedido fue duplicado por un ataque simulado.
        /// Existe en la cola como copia fraudulenta de otro pedido.
        /// </summary>
        Duplicado = 6
    }
}