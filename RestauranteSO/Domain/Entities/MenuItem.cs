// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Domain/Entities/MenuItem.cs
// Propósito: Representa un ítem del menú del restaurante.
//            Es el recurso compartido en la simulación Lectores-Escritores.
//            Los meseros leen esta entidad; el gerente la modifica.
// SOLID    : Single Responsibility - solo datos del ítem del menú.
// Patrón   : Entity (DDD).
// =============================================================================

namespace RestauranteSO.Domain.Entities
{
    /// <summary>
    /// Ítem del menú del restaurante.
    /// Es el RECURSO COMPARTIDO del problema Lectores-Escritores.
    /// 
    /// Múltiples hilos Mesero leen esta entidad simultáneamente.
    /// Un único hilo Gerente puede modificarla (escritura exclusiva).
    /// El acceso está controlado por ReaderWriterLockSlim en el servicio.
    /// </summary>
    public class MenuItem
    {
        // ─── IDENTIDAD ───────────────────────────────────────────────────────

        /// <summary>
        /// Identificador único del ítem en el menú.
        /// Asignado secuencialmente al crear el menú inicial.
        /// </summary>
        public int Id { get; }

        // ─── DATOS ACTUALES ──────────────────────────────────────────────────

        /// <summary>
        /// Nombre del plato tal como aparece actualmente en el menú.
        /// Puede ser modificado por el escritor (Gerente).
        /// Si fue atacado, puede contener un nombre falso.
        /// </summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Precio actual del plato en pesos.
        /// Puede ser modificado por el escritor.
        /// Durante un ataque puede ser multiplicado fraudulentamente.
        /// </summary>
        public decimal Precio { get; set; }

        /// <summary>
        /// Categoría del plato en el menú.
        /// Ejemplos: "Entradas", "Principales", "Postres", "Bebidas"
        /// </summary>
        public string Categoria { get; set; }

        /// <summary>
        /// Descripción detallada del plato para los meseros.
        /// Incluye ingredientes principales.
        /// </summary>
        public string Descripcion { get; set; }

        /// <summary>
        /// Indica si el plato está disponible actualmente.
        /// false cuando un ingrediente se agotó en el inventario.
        /// </summary>
        public bool Disponible { get; set; }

        // ─── HISTÓRICO / SEGURIDAD ───────────────────────────────────────────

        /// <summary>
        /// Nombre original antes de cualquier modificación.
        /// Usado en la simulación de ataque para mostrar qué cambió.
        /// </summary>
        public string NombreOriginal { get; private set; }

        /// <summary>
        /// Precio original antes de cualquier modificación.
        /// Permite calcular cuánto fue alterado durante un ataque.
        /// </summary>
        public decimal PrecioOriginal { get; private set; }

        /// <summary>
        /// Indica si este ítem fue alterado por un ataque simulado.
        /// Se muestra en rojo en el visor de lectores comprometidos.
        /// </summary>
        public bool FueAlterado { get; set; }

        /// <summary>
        /// Timestamp de la última modificación del ítem.
        /// Usado para el log de auditoría en la simulación de ataque.
        /// </summary>
        public DateTime UltimaModificacion { get; set; }

        /// <summary>
        /// Nombre del "usuario" que realizó la última modificación.
        /// Durante un ataque, muestra "Atacante" en lugar del gerente.
        /// </summary>
        public string ModificadoPor { get; set; }

        /// <summary>
        /// Versión del ítem. Se incrementa con cada modificación.
        /// Permite detectar lectores que tienen versiones desactualizadas.
        /// </summary>
        public int Version { get; set; }

        // ─── CONSTRUCTOR ─────────────────────────────────────────────────────

        /// <summary>
        /// Crea un nuevo ítem del menú con sus datos iniciales.
        /// Los campos "Original" se setean aquí y no vuelven a cambiar.
        /// </summary>
        public MenuItem(
            int id,
            string nombre,
            decimal precio,
            string categoria,
            string descripcion)
        {
            Id = id;
            Nombre = nombre;
            Precio = precio;
            Categoria = categoria;
            Descripcion = descripcion;
            Disponible = true;

            // Guardar valores originales para comparar durante el ataque
            NombreOriginal = nombre;
            PrecioOriginal = precio;

            // Estado inicial limpio
            FueAlterado = false;
            UltimaModificacion = DateTime.Now;
            ModificadoPor = "Sistema";
            Version = 1;
        }

        // ─── MÉTODOS ─────────────────────────────────────────────────────────

        /// <summary>
        /// Aplica una modificación legítima al ítem del menú.
        /// Llamado por el Gerente con el WriteLock adquirido.
        /// Incrementa la versión y registra quién modificó.
        /// </summary>
        /// <param name="nuevoNombre">Nuevo nombre del plato</param>
        /// <param name="nuevoPrecio">Nuevo precio</param>
        /// <param name="modificadoPor">Nombre del escritor que modifica</param>
        public void AplicarModificacion(string nuevoNombre, decimal nuevoPrecio, string modificadoPor)
        {
            Nombre = nuevoNombre;
            Precio = nuevoPrecio;
            UltimaModificacion = DateTime.Now;
            ModificadoPor = modificadoPor;
            Version++;
            // FueAlterado no se toca en modificaciones legítimas
        }

        /// <summary>
        /// Aplica una modificación fraudulenta (ataque simulado).
        /// Marca el ítem como alterado y registra al atacante.
        /// </summary>
        public void AplicarModificacionAtaque(string nombreFalso, decimal precioFalso)
        {
            Nombre = nombreFalso;
            Precio = precioFalso;
            FueAlterado = true;
            UltimaModificacion = DateTime.Now;
            ModificadoPor = "⚠ ATACANTE DESCONOCIDO";
            Version++;
        }

        /// <summary>
        /// Restaura los valores originales del ítem.
        /// Usado cuando se desactiva el ataque para mostrar recuperación.
        /// </summary>
        public void Restaurar()
        {
            Nombre = NombreOriginal;
            Precio = PrecioOriginal;
            FueAlterado = false;
            UltimaModificacion = DateTime.Now;
            ModificadoPor = "Sistema (Restaurado)";
            Version++;
        }

        /// <summary>
        /// Formato de texto para el LogViewer.
        /// </summary>
        public override string ToString() =>
            $"[v{Version}] {Nombre} - ${Precio:N2} ({Categoria})" +
            (FueAlterado ? " ⚠ ALTERADO" : "");
    }
}