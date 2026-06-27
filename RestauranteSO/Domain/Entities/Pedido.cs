// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Domain/Entities/Pedido.cs
// Propósito: Entidad principal del sistema Productor-Consumidor.
//            Representa un pedido real de un cliente del restaurante.
// SOLID    : Single Responsibility - solo datos del pedido.
//            Open/Closed - extensible mediante herencia si se necesita.
// Patrón   : Entity (DDD - Domain Driven Design).
// =============================================================================

using RestauranteSO.Domain.Enums;  // PedidoEstado

namespace RestauranteSO.Domain.Entities
{
    /// <summary>
    /// Representa un pedido de comida en el restaurante.
    /// Es el objeto que viaja a través de la cola compartida
    /// entre productores (clientes) y consumidores (cocineros).
    /// 
    /// Thread Safety: Esta clase es INMUTABLE en su creación.
    /// El campo Estado es el único que se modifica, y solo
    /// desde los workers con sincronización apropiada.
    /// </summary>
    public class Pedido
    {
        // ─── IDENTIDAD ───────────────────────────────────────────────────────

        /// <summary>
        /// Identificador único universal del pedido.
        /// Generado automáticamente en el constructor.
        /// Nunca se modifica después de la creación.
        /// </summary>
        public Guid Id { get; }

        /// <summary>
        /// Número visible del pedido para mostrar en la UI.
        /// Es un número secuencial más legible que el GUID.
        /// Ejemplo: "Pedido #42"
        /// </summary>
        public int NumeroPedido { get; }

        // ─── CONTENIDO ───────────────────────────────────────────────────────

        /// <summary>
        /// Descripción del plato pedido.
        /// Ejemplo: "Milanesa con papas fritas", "Pizza Margherita"
        /// Puede ser alterada durante un ataque simulado.
        /// </summary>
        public string Descripcion { get; set; }

        /// <summary>
        /// Descripción original antes de cualquier alteración.
        /// Usada para mostrar la diferencia en el panel de ataque.
        /// </summary>
        public string DescripcionOriginal { get; }

        // ─── ORIGEN ──────────────────────────────────────────────────────────

        /// <summary>
        /// ID del cliente que generó este pedido.
        /// Coincide con el nombre del hilo productor.
        /// Ejemplo: "Cliente-1", "Cliente-3"
        /// </summary>
        public string ClienteId { get; }

        /// <summary>
        /// Mesa del cliente en el restaurante.
        /// Número del 1 al 10 asignado aleatoriamente.
        /// </summary>
        public int NumeroMesa { get; }

        // ─── ESTADO Y TIEMPO ─────────────────────────────────────────────────

        /// <summary>
        /// Estado actual en el ciclo de vida del pedido.
        /// Modificado por los workers con la sincronización apropiada.
        /// Valores: Esperando → EnPreparacion → Listo → Entregado
        /// </summary>
        public PedidoEstado Estado { get; set; }

        /// <summary>
        /// Momento exacto en que el pedido fue creado por el productor.
        /// Usado para calcular tiempos de espera y métricas.
        /// </summary>
        public DateTime FechaCreacion { get; }

        /// <summary>
        /// Momento en que el cocinero comenzó a preparar el pedido.
        /// null si todavía está esperando en la cola.
        /// </summary>
        public DateTime? FechaInicioPreparacion { get; set; }

        /// <summary>
        /// Momento en que el pedido quedó listo.
        /// null si todavía está en preparación.
        /// </summary>
        public DateTime? FechaFinPreparacion { get; set; }

        /// <summary>
        /// Nombre del hilo cocinero que tomó este pedido.
        /// Ejemplo: "Cocinero-2"
        /// null si todavía está en la cola.
        /// </summary>
        public string? CocineroAsignado { get; set; }

        // ─── SEGURIDAD / ATAQUE ──────────────────────────────────────────────

        /// <summary>
        /// Indica si este pedido fue alterado por un ataque simulado.
        /// Si es true, se muestra con color de advertencia en la UI.
        /// </summary>
        public bool FueAlterado { get; set; }

        /// <summary>
        /// Indica si este pedido fue duplicado fraudulentamente.
        /// Los pedidos duplicados tienen FueDuplicado = true y
        /// hacen referencia al pedido original mediante PedidoOriginalId.
        /// </summary>
        public bool FueDuplicado { get; set; }

        /// <summary>
        /// ID del pedido original del cual este fue duplicado.
        /// Solo relevante si FueDuplicado es true.
        /// </summary>
        public Guid? PedidoOriginalId { get; set; }

        /// <summary>
        /// Tiempo estimado de preparación en milisegundos.
        /// Usado por los workers para simular el tiempo de cocción.
        /// </summary>
        public int TiempoPreparacionMs { get; }

        // ─── CONSTRUCTOR ─────────────────────────────────────────────────────

        /// <summary>
        /// Crea un nuevo pedido con todos sus datos iniciales.
        /// El Id se genera automáticamente.
        /// El Estado inicial es siempre Esperando.
        /// </summary>
        /// <param name="numeroPedido">Número secuencial visible del pedido</param>
        /// <param name="descripcion">Nombre del plato pedido</param>
        /// <param name="clienteId">Identificador del hilo cliente</param>
        /// <param name="numeroMesa">Mesa asignada al cliente</param>
        /// <param name="tiempoPreparacionMs">Milisegundos que tarda en prepararse</param>
        public Pedido(
            int numeroPedido,
            string descripcion,
            string clienteId,
            int numeroMesa,
            int tiempoPreparacionMs)
        {
            // Asignar identidad única
            Id = Guid.NewGuid();
            NumeroPedido = numeroPedido;

            // Asignar contenido
            Descripcion = descripcion;
            DescripcionOriginal = descripcion;  // guardar original para comparar

            // Asignar origen
            ClienteId = clienteId;
            NumeroMesa = numeroMesa;

            // Estado inicial siempre Esperando
            Estado = PedidoEstado.Esperando;

            // Tiempo de creación
            FechaCreacion = DateTime.Now;

            // Tiempo de preparación
            TiempoPreparacionMs = tiempoPreparacionMs;

            // Flags de ataque inicializados en false
            FueAlterado = false;
            FueDuplicado = false;
        }

        // ─── MÉTODOS DE NEGOCIO ──────────────────────────────────────────────

        /// <summary>
        /// Calcula cuánto tiempo lleva el pedido esperando en la cola.
        /// Retorna TimeSpan.Zero si ya fue tomado por un cocinero.
        /// </summary>
        public TimeSpan TiempoEspera =>
            FechaInicioPreparacion.HasValue
                ? FechaInicioPreparacion.Value - FechaCreacion
                : DateTime.Now - FechaCreacion;

        /// <summary>
        /// Calcula cuánto tiempo tardó en prepararse.
        /// Retorna TimeSpan.Zero si todavía está en preparación.
        /// </summary>
        public TimeSpan TiempoPreparacion =>
            (FechaInicioPreparacion.HasValue && FechaFinPreparacion.HasValue)
                ? FechaFinPreparacion.Value - FechaInicioPreparacion.Value
                : TimeSpan.Zero;

        /// <summary>
        /// Representación en texto del pedido para el LogViewer.
        /// Incluye estado, número y descripción.
        /// </summary>
        public override string ToString() =>
            $"[{Estado}] #{NumeroPedido:D3} - {Descripcion} (Mesa {NumeroMesa}, {ClienteId})";
    }
}