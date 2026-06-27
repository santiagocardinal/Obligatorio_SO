// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Infrastructure/Logging/SimulationLogger.cs
// Propósito: Implementación thread-safe del sistema de logging.
//            Almacena todos los eventos de la simulación en memoria.
//            Notifica a la UI cuando hay nuevos logs disponibles.
// SOLID    : SRP - solo registra logs.
//            DIP - implementa ISimulationLogger.
// Patrón   : Observer (notifica a suscriptores via evento NuevoLogAgregado).
// Thread   : ConcurrentQueue garantiza acceso seguro desde múltiples hilos.
// =============================================================================

using System.Collections.Concurrent;
using RestauranteSO.Domain.Enums;
using RestauranteSO.Domain.Interfaces;
using RestauranteSO.Domain.Models;

namespace RestauranteSO.Infrastructure.Logging
{
    /// <summary>
    /// Logger thread-safe para las simulaciones de RestauranteSO.
    ///
    /// Diseño de concurrencia:
    /// - ConcurrentQueue: permite Enqueue desde N hilos sin lock.
    /// - El evento NuevoLogAgregado se dispara desde el hilo que llama a Log().
    /// - La UI DEBE usar Invoke() en el handler para no actualizar controles
    ///   desde un hilo que no sea el UI thread.
    ///
    /// Límite de memoria: mantiene solo los últimos MaxEntradas logs.
    /// Los más antiguos se descartan automáticamente (sliding window).
    /// </summary>
    public sealed class SimulationLogger : ISimulationLogger
    {
        // ─── CONSTANTES ───────────────────────────────────────────────────────

        /// <summary>
        /// Máximo de entries que se mantienen en memoria.
        /// Al superar este límite, los más antiguos son descartados.
        /// Evita que la aplicación consuma memoria ilimitada en simulaciones largas.
        /// </summary>
        private const int MaxEntradas = 1000;

        // ─── ALMACENAMIENTO ──────────────────────────────────────────────────

        /// <summary>
        /// Cola concurrente que almacena todos los log entries.
        ///
        /// ¿Por qué ConcurrentQueue?
        /// - Multiple threads (Productor-1, Productor-2, Cocinero-1...) llaman
        ///   a Log() simultáneamente.
        /// - ConcurrentQueue.Enqueue() es lock-free para casos comunes.
        /// - No necesitamos acceso aleatorio, solo FIFO (primero en entrar,
        ///   primero en salir).
        /// </summary>
        private readonly ConcurrentQueue<LogEntry> _entries = new();

        /// <summary>
        /// Contador atómico de la cantidad de entries.
        /// Evita usar Count de la queue (que hace snapshot completo).
        /// </summary>
        private int _cantidadEntries = 0;

        // ─── EVENTO ──────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public event EventHandler<LogEntry>? NuevoLogAgregado;

        // ─── PROPIEDADES ─────────────────────────────────────────────────────

        /// <inheritdoc/>
        public int CantidadEntries => _cantidadEntries;

        // ─── MÉTODOS PÚBLICOS ─────────────────────────────────────────────────

        /// <inheritdoc/>
        /// <remarks>
        /// Thread-safe: puede llamarse desde cualquier Thread o Task.
        /// El evento NuevoLogAgregado se dispara en el hilo del llamador.
        /// </remarks>
        public void Log(
            string mensaje,
            LogLevel nivel = LogLevel.Info,
            string fuente = "Sistema")
        {
            // Crear el entry inmutable
            var entry = new LogEntry
            {
                Nivel   = nivel,
                Fuente  = fuente,
                Mensaje = mensaje
            };

            // Agregar a la cola concurrente (thread-safe, lock-free)
            _entries.Enqueue(entry);
            Interlocked.Increment(ref _cantidadEntries);

            // Limpiar entradas antiguas si superamos el límite
            // Esto es eventual (no sincrónico) para no penalizar el hilo llamador
            if (_cantidadEntries > MaxEntradas)
                DescartarEntradaAntigua();

            // Notificar a los suscriptores (normalmente la UI)
            // El handler de la UI DEBE usar Invoke() para actualizar controles
            NuevoLogAgregado?.Invoke(this, entry);
        }

        /// <inheritdoc/>
        public IReadOnlyList<LogEntry> ObtenerLogs()
        {
            // ToArray() hace un snapshot thread-safe de la queue
            // Invertimos el orden para que el log más reciente esté primero
            return _entries.ToArray().Reverse().ToList().AsReadOnly();
        }

        /// <inheritdoc/>
        public IReadOnlyList<LogEntry> ObtenerUltimosLogs(int cantidad)
        {
            // Tomamos los últimos N del array
            // Más eficiente que ObtenerLogs() para actualizaciones periódicas de la UI
            return _entries
                .ToArray()              // snapshot completo thread-safe
                .TakeLast(cantidad)     // tomar los últimos N
                .Reverse()              // más reciente primero
                .ToList()
                .AsReadOnly();
        }

        /// <inheritdoc/>
        public void Limpiar()
        {
            // ConcurrentQueue no tiene Clear() directo en todas las versiones
            // La técnica estándar es desencolar hasta vaciar
            while (_entries.TryDequeue(out _)) { }
            Interlocked.Exchange(ref _cantidadEntries, 0);
        }

        // ─── MÉTODOS PRIVADOS ─────────────────────────────────────────────────

        /// <summary>
        /// Descarta la entrada más antigua de la cola.
        /// Llamado cuando se supera MaxEntradas para gestionar memoria.
        /// </summary>
        private void DescartarEntradaAntigua()
        {
            if (_entries.TryDequeue(out _))
                Interlocked.Decrement(ref _cantidadEntries);
        }
    }
}