// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Infrastructure/Synchronization/SharedBufferMonitor.cs
// Propósito: Buffer compartido con Monitor (lock + Pulse/Wait) para
//            la versión alternativa de Productor-Consumidor.
//            Muestra el uso de Monitor como mecanismo de sincronización clásico.
//            Complementa la implementación con SemaphoreSlim.
// SOLID    : SRP - solo gestiona el buffer compartido.
// Thread   : Todos los métodos son thread-safe mediante lock(this).
// =============================================================================

using RestauranteSO.Domain.Entities;

namespace RestauranteSO.Infrastructure.Synchronization
{
    /// <summary>
    /// Buffer compartido implementado con Monitor (lock + Pulse/Wait).
    ///
    /// ¿Por qué Monitor además de SemaphoreSlim?
    /// Monitor es el mecanismo clásico de los libros de Sistemas Operativos.
    /// SemaphoreSlim es la implementación moderna y más eficiente.
    /// Esta clase existe para MOSTRAR la diferencia y educar al alumno.
    ///
    /// CÓMO FUNCIONA Monitor.Wait() y Monitor.Pulse():
    ///
    ///   Monitor.Wait(objeto):
    ///   - Libera el lock del objeto temporalmente.
    ///   - Suspende el hilo en la cola de espera del objeto.
    ///   - Cuando otro hilo hace Pulse(), este hilo se reactiva
    ///     y vuelve a adquirir el lock antes de continuar.
    ///
    ///   Monitor.Pulse(objeto):
    ///   - Señala a UN hilo esperando en Wait() que puede reactivarse.
    ///   - El hilo pulsado no corre inmediatamente; entra en la cola
    ///     de listos y espera a que el lock esté disponible.
    ///
    ///   Monitor.PulseAll(objeto):
    ///   - Como Pulse() pero señala a TODOS los hilos en Wait().
    ///
    /// COMPARACIÓN:
    ///   Monitor       → clásico, complejo, parte del CLR
    ///   SemaphoreSlim → moderno, más fácil, soporta async/await
    ///   En producción: preferir SemaphoreSlim. En educación: ambos.
    /// </summary>
    public class SharedBufferMonitor
    {
        // ─── ESTADO INTERNO ──────────────────────────────────────────────────

        /// <summary>
        /// Buffer circular de pedidos.
        /// Accedido solo con lock(this) adquirido.
        /// </summary>
        private readonly Queue<Pedido> _buffer;

        /// <summary>Capacidad máxima del buffer.</summary>
        private readonly int _capacidadMaxima;

        /// <summary>
        /// Objeto de sincronización explícito.
        /// Usar un objeto dedicado es mejor práctica que lock(this)
        /// porque evita deadlocks con código externo que también lockea 'this'.
        /// </summary>
        private readonly object _syncObject = new object();

        // ─── PROPIEDADES ─────────────────────────────────────────────────────

        /// <summary>
        /// Cantidad actual de items en el buffer.
        /// Thread-safe: lee bajo lock.
        /// </summary>
        public int CantidadActual
        {
            get
            {
                lock (_syncObject)
                    return _buffer.Count;
            }
        }

        /// <summary>Capacidad máxima configurada.</summary>
        public int CapacidadMaxima => _capacidadMaxima;

        /// <summary>true si el buffer está lleno.</summary>
        public bool EstaLleno
        {
            get
            {
                lock (_syncObject)
                    return _buffer.Count >= _capacidadMaxima;
            }
        }

        /// <summary>true si el buffer está vacío.</summary>
        public bool EstaVacio
        {
            get
            {
                lock (_syncObject)
                    return _buffer.Count == 0;
            }
        }

        // ─── CONSTRUCTOR ─────────────────────────────────────────────────────

        public SharedBufferMonitor(int capacidadMaxima)
        {
            if (capacidadMaxima <= 0)
                throw new ArgumentException("La capacidad debe ser mayor a 0.", nameof(capacidadMaxima));

            _capacidadMaxima = capacidadMaxima;
            _buffer = new Queue<Pedido>(capacidadMaxima);
        }

        // ─── MÉTODOS DE PRODUCTOR ─────────────────────────────────────────────

        /// <summary>
        /// Agrega un pedido al buffer.
        /// Si el buffer está lleno, el hilo ESPERA usando Monitor.Wait().
        /// Cuando hay espacio, encola el pedido y notifica a consumidores.
        /// </summary>
        /// <param name="pedido">Pedido a agregar</param>
        /// <param name="token">Token para cancelación cooperativa</param>
        public void Producir(Pedido pedido, CancellationToken token)
        {
            // Adquirir el lock del objeto de sincronización
            lock (_syncObject)
            {
                // ESPERA CONDICIONAL: mientras el buffer está lleno Y no hay cancelación
                while (_buffer.Count >= _capacidadMaxima && !token.IsCancellationRequested)
                {
                    // Monitor.Wait():
                    // 1. Libera el lock temporalmente (permite a consumidores entrar)
                    // 2. Suspende este hilo en la cola de espera
                    // 3. Cuando sea pulsado, vuelve a adquirir el lock y continúa
                    Monitor.Wait(_syncObject, millisecondsTimeout: 500);
                }

                // Si fue cancelado durante la espera, salir sin producir
                if (token.IsCancellationRequested)
                    return;

                // Agregar el pedido al buffer
                _buffer.Enqueue(pedido);

                // Monitor.Pulse():
                // Notifica a UN consumidor que puede haber un item disponible.
                // Usamos Pulse (no PulseAll) porque solo UN consumidor necesita
                // activarse para consumir este pedido.
                Monitor.Pulse(_syncObject);
            }
        }

        // ─── MÉTODOS DE CONSUMIDOR ────────────────────────────────────────────

        /// <summary>
        /// Extrae un pedido del buffer.
        /// Si el buffer está vacío, el hilo ESPERA usando Monitor.Wait().
        /// Cuando hay un item, lo extrae y notifica a productores.
        /// </summary>
        /// <param name="token">Token para cancelación cooperativa</param>
        /// <returns>El pedido extraído, o null si fue cancelado</returns>
        public Pedido? Consumir(CancellationToken token)
        {
            lock (_syncObject)
            {
                // ESPERA CONDICIONAL: mientras el buffer está vacío Y no hay cancelación
                while (_buffer.Count == 0 && !token.IsCancellationRequested)
                {
                    // Igual que en Producir(): libera lock, espera, vuelve a adquirir
                    Monitor.Wait(_syncObject, millisecondsTimeout: 500);
                }

                // Si fue cancelado durante la espera
                if (token.IsCancellationRequested || _buffer.Count == 0)
                    return null;

                // Extraer el pedido
                var pedido = _buffer.Dequeue();

                // Notificar a UN productor que hay espacio disponible
                Monitor.Pulse(_syncObject);

                return pedido;
            }
        }

        // ─── UTILIDADES ───────────────────────────────────────────────────────

        /// <summary>
        /// Vacía el buffer completamente.
        /// Notifica a todos los hilos esperando para que puedan salir.
        /// Llamado al cancelar la simulación.
        /// </summary>
        public void Vaciar()
        {
            lock (_syncObject)
            {
                _buffer.Clear();
                // PulseAll: desbloquear TODOS los hilos esperando
                // para que puedan chequear el CancellationToken y terminar
                Monitor.PulseAll(_syncObject);
            }
        }

        /// <summary>
        /// Retorna una copia del contenido actual del buffer.
        /// Thread-safe. Usado por la UI para mostrar el estado de la cola.
        /// </summary>
        public IReadOnlyList<Pedido> ObtenerSnapshot()
        {
            lock (_syncObject)
                return _buffer.ToList().AsReadOnly();
        }
    }
}