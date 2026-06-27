// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Domain/Interfaces/IPedidoRepository.cs
// Propósito: Contrato para el repositorio de pedidos históricos.
// SOLID    : ISP, DIP. Patrón Repository.
// =============================================================================

using RestauranteSO.Domain.Entities;
using RestauranteSO.Domain.Enums;

namespace RestauranteSO.Domain.Interfaces
{
    /// <summary>
    /// Repositorio de pedidos del restaurante.
    /// Almacena el historial completo de pedidos para estadísticas y auditoría.
    /// 
    /// Thread-safe: Implementado con ConcurrentDictionary.
    /// No requiere locks externos para operaciones básicas.
    /// </summary>
    public interface IPedidoRepository
    {
        /// <summary>
        /// Agrega un nuevo pedido al repositorio histórico.
        /// Thread-safe.
        /// </summary>
        void Agregar(Pedido pedido);

        /// <summary>
        /// Busca un pedido por su ID único.
        /// </summary>
        Pedido? ObtenerPorId(Guid id);

        /// <summary>
        /// Retorna todos los pedidos en un estado específico.
        /// </summary>
        IReadOnlyList<Pedido> ObtenerPorEstado(PedidoEstado estado);

        /// <summary>
        /// Retorna los últimos N pedidos.
        /// Optimizado para la tabla de historial en la UI.
        /// </summary>
        IReadOnlyList<Pedido> ObtenerUltimos(int cantidad);

        /// <summary>
        /// Retorna todos los pedidos que fueron alterados por un ataque.
        /// Usado para construir el reporte de evidencia.
        /// </summary>
        IReadOnlyList<Pedido> ObtenerAlterados();

        /// <summary>
        /// Total de pedidos registrados.
        /// </summary>
        int TotalPedidos { get; }

        /// <summary>
        /// Total de pedidos completados exitosamente.
        /// </summary>
        int TotalCompletados { get; }

        /// <summary>
        /// Total de pedidos alterados por ataques.
        /// </summary>
        int TotalAlterados { get; }

        /// <summary>
        /// Elimina todos los pedidos del repositorio.
        /// Llamado al reiniciar la simulación.
        /// </summary>
        void Limpiar();
    }
}