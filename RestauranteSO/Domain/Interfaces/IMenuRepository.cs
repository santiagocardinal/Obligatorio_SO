// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Domain/Interfaces/IMenuRepository.cs
// Propósito: Contrato para el repositorio del menú del restaurante.
// SOLID    : ISP, DIP. Patrón Repository.
// =============================================================================

using RestauranteSO.Domain.Entities;

namespace RestauranteSO.Domain.Interfaces
{
    /// <summary>
    /// Repositorio del menú del restaurante.
    /// Abstrae el almacenamiento y recuperación de ítems del menú.
    /// 
    /// En esta implementación el menú vive en memoria, pero la interfaz
    /// permite reemplazarlo por una base de datos sin cambiar los servicios.
    /// 
    /// Patrón Repository: Desacopla la lógica de negocio del acceso a datos.
    /// </summary>
    public interface IMenuRepository
    {
        /// <summary>
        /// Retorna todos los ítems del menú.
        /// Thread-safe con ReadLock externo al llamar.
        /// </summary>
        IReadOnlyList<MenuItem> ObtenerTodos();

        /// <summary>
        /// Retorna un ítem específico por su ID.
        /// Retorna null si no existe.
        /// </summary>
        MenuItem? ObtenerPorId(int id);

        /// <summary>
        /// Retorna todos los ítems disponibles de una categoría.
        /// </summary>
        IReadOnlyList<MenuItem> ObtenerPorCategoria(string categoria);

        /// <summary>
        /// Agrega un nuevo ítem al menú.
        /// </summary>
        void Agregar(MenuItem item);

        /// <summary>
        /// Actualiza un ítem existente.
        /// Thread-safe con WriteLock externo al llamar.
        /// </summary>
        void Actualizar(MenuItem item);

        /// <summary>
        /// Elimina un ítem del menú por su ID.
        /// </summary>
        void Eliminar(int id);

        /// <summary>
        /// Retorna el menú completo incluyendo ítems no disponibles.
        /// Usado por el Gerente para ver el estado completo.
        /// </summary>
        IReadOnlyList<MenuItem> ObtenerCompleto();

        /// <summary>
        /// Reinicia el menú a su estado original.
        /// Usado al desactivar un ataque para restaurar los datos.
        /// </summary>
        void Restaurar();

        /// <summary>
        /// Cantidad de ítems en el menú.
        /// </summary>
        int CantidadItems { get; }
    }
}