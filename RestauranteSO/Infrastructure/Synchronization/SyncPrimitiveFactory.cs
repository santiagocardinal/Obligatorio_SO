// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Infrastructure/Synchronization/SyncPrimitiveFactory.cs
// Propósito: Fábrica de primitivas de sincronización.
//            Centraliza la creación de SemaphoreSlim, ReaderWriterLockSlim,
//            ConcurrentQueue, ManualResetEventSlim y CancellationTokenSource.
//            Documenta por qué se usa cada mecanismo.
// SOLID    : SRP - solo crea primitivas de sincronización.
//            OCP - extensible para nuevos tipos de primitivas.
// Patrón   : Factory Method - encapsula la creación de objetos complejos.
// =============================================================================

using System.Collections.Concurrent;
using RestauranteSO.Domain.Entities;

namespace RestauranteSO.Infrastructure.Synchronization
{
    /// <summary>
    /// Fábrica de primitivas de sincronización para las simulaciones.
    ///
    /// ¿Por qué una Factory?
    /// - Centraliza la configuración de cada primitiva.
    /// - Documenta el propósito de cada mecanismo de sincronización.
    /// - Facilita pruebas (puede reemplazarse por una fábrica mock).
    /// - Aplica DIP: los servicios reciben las primitivas sin conocer
    ///   los detalles de construcción.
    ///
    /// EXPLICACIÓN DE CADA PRIMITIVA:
    ///
    /// SemaphoreSlim:
    ///   Controla cuántos hilos pueden acceder simultáneamente a un recurso.
    ///   En Productor-Consumidor:
    ///   - semFull: cuántos ESPACIOS libres hay en la cola (inicia en capacidad).
    ///   - semEmpty: cuántos ITEMS hay disponibles (inicia en 0).
    ///   Un productor hace Wait() en semFull (espera si la cola está llena)
    ///   y Release() en semEmpty (señala que agregó un item).
    ///   Un consumidor hace Wait() en semEmpty y Release() en semFull.
    ///
    /// ReaderWriterLockSlim:
    ///   Permite N lectores simultáneos O 1 escritor exclusivo.
    ///   Más eficiente que lock() cuando las lecturas son frecuentes.
    ///   EnterReadLock() / ExitReadLock(): múltiples hilos simultáneos.
    ///   EnterWriteLock() / ExitWriteLock(): exclusivo, bloquea todo.
    ///
    /// ManualResetEventSlim:
    ///   Funciona como un semáforo binario que permanece en su estado.
    ///   Set() = "puerta abierta" (todos los Wait() pasan).
    ///   Reset() = "puerta cerrada" (todos los Wait() bloquean).
    ///   Usado para implementar PAUSA en las simulaciones.
    ///   Cuando el usuario pausa: Reset(). Cuando reanuda: Set().
    ///
    /// CancellationTokenSource:
    ///   Mecanismo cooperativo para cancelar hilos/tasks gracefully.
    ///   Cancel() señala la cancelación. Los workers chequean
    ///   IsCancellationRequested en cada iteración y terminan limpiamente.
    ///   Nunca usar Thread.Abort() (deprecated y peligroso).
    /// </summary>
    public static class SyncPrimitiveFactory
    {
        // ─── SEMÁFOROS ───────────────────────────────────────────────────────

        /// <summary>
        /// Crea el semáforo "espacios libres" para Productor-Consumidor.
        ///
        /// Inicia en 'capacidadMaxima' porque al comenzar la cola está vacía
        /// y hay capacidadMaxima espacios libres.
        /// Los productores hacen WaitAsync() antes de encolar: si llega a 0,
        /// el productor se bloquea hasta que un consumidor libere espacio.
        /// </summary>
        /// <param name="capacidadMaxima">Tamaño máximo de la cola</param>
        public static SemaphoreSlim CrearSemaforoEspaciosLibres(int capacidadMaxima)
            => new SemaphoreSlim(capacidadMaxima, capacidadMaxima);

        /// <summary>
        /// Crea el semáforo "items disponibles" para Productor-Consumidor.
        ///
        /// Inicia en 0 porque al comenzar la cola está vacía.
        /// Los consumidores hacen WaitAsync() antes de desencolar: si llega a 0,
        /// el cocinero se bloquea hasta que un cliente genere un pedido.
        /// </summary>
        /// <param name="capacidadMaxima">Tamaño máximo del semáforo</param>
        public static SemaphoreSlim CrearSemaforoItemsDisponibles(int capacidadMaxima)
            => new SemaphoreSlim(0, capacidadMaxima);

        // ─── READER-WRITER LOCK ──────────────────────────────────────────────

        /// <summary>
        /// Crea el lock de lectores-escritores para el menú del restaurante.
        ///
        /// LockRecursionPolicy.NoRecursion:
        ///   El mismo hilo NO puede adquirir el lock dos veces.
        ///   Es la política recomendada para evitar deadlocks.
        ///   Si un hilo intenta adquirirlo de forma recursiva, lanza excepción
        ///   inmediatamente (falla rápido) en lugar de deadlock silencioso.
        /// </summary>
        public static ReaderWriterLockSlim CrearReaderWriterLock()
            => new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        // ─── MANUAL RESET EVENT ──────────────────────────────────────────────

        /// <summary>
        /// Crea el evento de pausa para las simulaciones.
        ///
        /// initialState = true: la simulación comienza en estado "corriendo"
        /// (puerta abierta). Los workers hacen Wait() en cada iteración;
        /// si la puerta está abierta, continúan inmediatamente.
        /// Cuando el usuario pausa: Reset() cierra la puerta.
        /// Cuando reanuda: Set() abre la puerta nuevamente.
        /// </summary>
        public static ManualResetEventSlim CrearEventoPausa()
            => new ManualResetEventSlim(initialState: true);

        // ─── CANCELLATION TOKEN ──────────────────────────────────────────────

        /// <summary>
        /// Crea el token de cancelación para controlar el ciclo de vida
        /// de todos los workers de una simulación.
        ///
        /// Al detener la simulación: cts.Cancel()
        /// Cada worker tiene: while(!token.IsCancellationRequested) { ... }
        /// Los workers terminan su iteración actual y salen limpiamente.
        /// Luego: cts.Dispose() y crear nuevo cts para la próxima sesión.
        /// </summary>
        public static CancellationTokenSource CrearCancellationTokenSource()
            => new CancellationTokenSource();

        // ─── COLA CONCURRENTE ────────────────────────────────────────────────

        /// <summary>
        /// Crea la cola concurrente de pedidos para Productor-Consumidor.
        ///
        /// ConcurrentQueue vs Queue + lock:
        /// - ConcurrentQueue usa algoritmos lock-free para Enqueue/TryDequeue.
        /// - En escenarios de alto throughput (muchos productores/consumidores)
        ///   es significativamente más rápida que Queue con lock.
        /// - TryDequeue() retorna false si la cola está vacía (no lanza excepción).
        /// - El semáforo semEmpty garantiza que solo llamamos TryDequeue()
        ///   cuando sabemos que hay items (evita polling activo).
        /// </summary>
        public static ConcurrentQueue<Pedido> CrearColaPedidos()
            => new ConcurrentQueue<Pedido>();

        // ─── MUTEX ───────────────────────────────────────────────────────────

        /// <summary>
        /// Crea un Mutex con nombre para exclusión mutua entre procesos.
        ///
        /// ¿Cuándo usar Mutex vs lock?
        /// - lock: exclusión mutua DENTRO del mismo proceso (más rápido).
        /// - Mutex: exclusión mutua ENTRE procesos distintos.
        /// En esta simulación el Mutex se usa para prevenir múltiples
        /// instancias simultáneas de la misma simulación.
        /// </summary>
        /// <param name="nombre">Nombre único del mutex (incluye el tipo de simulación)</param>
        public static Mutex CrearMutexSimulacion(string nombre)
            => new Mutex(false, $"RestauranteSO_{nombre}");
    }
}