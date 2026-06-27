// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Infrastructure/Repositories/MenuRepository.cs
// Propósito: Implementación en memoria del repositorio del menú.
//            Almacena los ítems del menú que leen los meseros
//            y escribe el gerente en la simulación Lectores-Escritores.
// SOLID    : SRP - solo gestiona el almacenamiento del menú.
//            DIP - implementa IMenuRepository.
// Patrón   : Repository - abstrae el almacenamiento de datos.
// Thread   : El acceso concurrente es gestionado EXTERNAMENTE por el servicio
//            mediante ReaderWriterLockSlim. Este repositorio confía en que
//            el llamador adquirió el lock apropiado.
// =============================================================================

using RestauranteSO.Constants;
using RestauranteSO.Domain.Entities;
using RestauranteSO.Domain.Interfaces;

namespace RestauranteSO.Infrastructure.Repositories
{
    /// <summary>
    /// Repositorio en memoria del menú del restaurante.
    ///
    /// DISEÑO DE CONCURRENCIA IMPORTANTE:
    /// Este repositorio NO implementa sincronización interna.
    /// El servicio LectoresEscritoresService es responsable de adquirir
    /// el ReaderWriterLockSlim ANTES de llamar a cualquier método de este repo.
    ///
    /// ¿Por qué no poner el lock aquí dentro?
    /// Si el repositorio lockea internamente, el servicio no puede hacer
    /// operaciones compuestas de forma atómica (ej: leer + modificar + leer).
    /// Al delegar el lock al servicio, las operaciones compuestas son seguras.
    /// Esto se llama "lock ownership" o "external locking pattern".
    ///
    /// El servicio usa:
    ///   rwLock.EnterReadLock() → ObtenerTodos() → rwLock.ExitReadLock()
    ///   rwLock.EnterWriteLock() → Actualizar() → rwLock.ExitWriteLock()
    /// </summary>
    public sealed class MenuRepository : IMenuRepository
    {
        // ─── ALMACENAMIENTO ──────────────────────────────────────────────────

        /// <summary>
        /// Lista principal del menú.
        /// Solo accesible con el lock apropiado (ver diseño arriba).
        /// </summary>
        private readonly List<MenuItem> _items = new();

        /// <summary>
        /// Copia de respaldo del menú original para restaurar tras un ataque.
        /// Inmutable después de la inicialización.
        /// </summary>
        private readonly List<MenuItem> _backupOriginal = new();

        // ─── CONSTRUCTOR ─────────────────────────────────────────────────────

        /// <summary>
        /// Inicializa el repositorio con el menú predeterminado del restaurante.
        /// Poblado con los datos de SimulationConstants.MenuInicial.
        /// También crea el backup para restaurar después de ataques.
        /// </summary>
        public MenuRepository()
        {
            InicializarMenuPredeterminado();
        }

        // ─── IMPLEMENTACIÓN DE IMenuRepository ──────────────────────────────

        /// <inheritdoc/>
        /// <remarks>
        /// REQUIERE: ReadLock o WriteLock adquirido por el llamador.
        /// </remarks>
        public IReadOnlyList<MenuItem> ObtenerTodos()
            => _items.Where(i => i.Disponible).ToList().AsReadOnly();

        /// <inheritdoc/>
        public MenuItem? ObtenerPorId(int id)
            => _items.FirstOrDefault(i => i.Id == id);

        /// <inheritdoc/>
        public IReadOnlyList<MenuItem> ObtenerPorCategoria(string categoria)
            => _items
                .Where(i => i.Categoria == categoria && i.Disponible)
                .ToList()
                .AsReadOnly();

        /// <inheritdoc/>
        public void Agregar(MenuItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            // Verificar que no exista ya un item con el mismo Id
            if (_items.Any(i => i.Id == item.Id))
                throw new InvalidOperationException(
                    $"Ya existe un ítem con Id={item.Id} en el menú.");

            _items.Add(item);
        }

        /// <inheritdoc/>
        /// <remarks>
        /// Reemplaza el item existente por el nuevo.
        /// Si no existe el Id, lanza InvalidOperationException.
        /// REQUIERE: WriteLock adquirido por el llamador.
        /// </remarks>
        public void Actualizar(MenuItem item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            var indice = _items.FindIndex(i => i.Id == item.Id);
            if (indice < 0)
                throw new InvalidOperationException(
                    $"No se encontró ítem con Id={item.Id} para actualizar.");

            _items[indice] = item;
        }

        /// <inheritdoc/>
        public void Eliminar(int id)
        {
            var item = _items.FirstOrDefault(i => i.Id == id);
            if (item != null)
                _items.Remove(item);
        }

        /// <inheritdoc/>
        public IReadOnlyList<MenuItem> ObtenerCompleto()
            => _items.ToList().AsReadOnly();

        /// <inheritdoc/>
        /// <remarks>
        /// Restaura cada ítem a sus valores originales.
        /// Llamado al desactivar un ataque.
        /// REQUIERE: WriteLock adquirido por el llamador.
        /// </remarks>
        public void Restaurar()
        {
            // Para cada ítem en el menú actual, buscar su versión original
            foreach (var item in _items)
            {
                var original = _backupOriginal.FirstOrDefault(b => b.Id == item.Id);
                if (original != null)
                    item.Restaurar();
            }
        }

        /// <inheritdoc/>
        public int CantidadItems => _items.Count;

        // ─── PRIVADOS ─────────────────────────────────────────────────────────

        /// <summary>
        /// Puebla la lista con el menú predeterminado del restaurante.
        /// También crea backups de cada ítem para restauración post-ataque.
        /// </summary>
        private void InicializarMenuPredeterminado()
        {
            int id = 1;

            // Iterar sobre los datos del menú definidos en las constantes
            foreach (var (nombre, precio, cat, desc) in SimulationConstants.MenuInicial)
            {
                var item = new MenuItem(id++, nombre, precio, cat, desc);
                _items.Add(item);

                // Crear un item de backup con los mismos valores
                // El backup es una copia independiente que nunca se modifica
                var backup = new MenuItem(item.Id, nombre, precio, cat, desc);
                _backupOriginal.Add(backup);
            }
        }
    }
}