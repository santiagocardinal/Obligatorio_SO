// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Infrastructure/Repositories/PedidoRepository.cs
// Propósito: Repositorio en memoria de pedidos históricos.
//            Almacena todos los pedidos generados durante la simulación.
//            Usado para estadísticas, auditoría y reporte de ataques.
// SOLID    : SRP - solo almacena pedidos.
//            DIP - implementa IPedidoRepository.
// Patrón   : Repository.
// Thread   : Thread-safe mediante ConcurrentDictionary.
//            No requiere locks externos.
// =============================================================================

using System.Collections.Concurrent;
using RestauranteSO.Domain.Entities;
using RestauranteSO.Domain.Enums;
using RestauranteSO.Domain.Interfaces;

namespace RestauranteSO.Infrastructure.Repositories
{
    /// <summary>
    /// Repositorio thread-safe de pedidos históricos.
    ///
    /// ¿Por qué ConcurrentDictionary?
    /// - Los productores agregan pedidos constantemente desde múltiples hilos.
    /// - Los consumidores actualizan el estado de los pedidos.
    /// - La UI lee para mostrar estadísticas.
    /// - ConcurrentDictionary maneja todas estas operaciones concurrentes
    ///   sin locks explícitos, usando particionamiento interno.
    ///
    /// ¿Por qué Guid como key?
    /// - Cada pedido tiene un Guid único desde su creación.
    /// - Es globalmente único sin coordinación central.
    /// - Evita colisiones incluso con muchos productores simultáneos.
    /// </summary>
    public sealed class PedidoRepository : IPedidoRepository
    {
        // ─── ALMACENAMIENTO ──────────────────────────────────────────────────

        /// <summary>
        /// Diccionario concurrente: Key=Guid del pedido, Value=Pedido.
        ///
        /// ConcurrentDictionary vs Dictionary + lock:
        /// - ConcurrentDictionary: granular locking (lockea buckets individuales).
        /// - Dictionary + lock: lockea TODO el diccionario en cada operación.
        /// - Con N productores/consumidores, ConcurrentDictionary es mucho más eficiente.
        /// </summary>
        private readonly ConcurrentDictionary<Guid, Pedido> _pedidos = new();

        // ─── IMPLEMENTACIÓN ──────────────────────────────────────────────────

        /// <inheritdoc/>
        public void Agregar(Pedido pedido)
        {
            if (pedido == null)
                throw new ArgumentNullException(nameof(pedido));

            // TryAdd es atómica y thread-safe
            // Si ya existe el key (muy improbable con Guid), no hace nada
            _pedidos.TryAdd(pedido.Id, pedido);
        }

        /// <inheritdoc/>
        public Pedido? ObtenerPorId(Guid id)
        {
            _pedidos.TryGetValue(id, out var pedido);
            return pedido;
        }

        /// <inheritdoc/>
        public IReadOnlyList<Pedido> ObtenerPorEstado(PedidoEstado estado)
        {
            // Values hace un snapshot thread-safe
            return _pedidos.Values
                .Where(p => p.Estado == estado)
                .OrderByDescending(p => p.FechaCreacion)
                .ToList()
                .AsReadOnly();
        }

        /// <inheritdoc/>
        public IReadOnlyList<Pedido> ObtenerUltimos(int cantidad)
        {
            return _pedidos.Values
                .OrderByDescending(p => p.FechaCreacion)
                .Take(cantidad)
                .ToList()
                .AsReadOnly();
        }

        /// <inheritdoc/>
        public IReadOnlyList<Pedido> ObtenerAlterados()
        {
            return _pedidos.Values
                .Where(p => p.FueAlterado || p.FueDuplicado)
                .OrderByDescending(p => p.FechaCreacion)
                .ToList()
                .AsReadOnly();
        }

        /// <inheritdoc/>
        public int TotalPedidos => _pedidos.Count;

        /// <inheritdoc/>
        public int TotalCompletados =>
            _pedidos.Values.Count(p => p.Estado == PedidoEstado.Entregado);

        /// <inheritdoc/>
        public int TotalAlterados =>
            _pedidos.Values.Count(p => p.FueAlterado || p.FueDuplicado);

        /// <inheritdoc/>
        public void Limpiar()
        {
            _pedidos.Clear();
        }
    }
}