// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Domain/Entities/Mesero.cs
// Propósito: Representa un mesero que lee el menú continuamente.
//            Cada mesero es un hilo Lector en Lectores-Escritores.
// SOLID    : Single Responsibility.
// =============================================================================

namespace RestauranteSO.Domain.Entities
{
    /// <summary>
    /// Representa un mesero del restaurante.
    /// En la simulación Lectores-Escritores, cada Mesero tiene un Task/Thread
    /// que lee el menú repetidamente para consultar precios y disponibilidad.
    /// 
    /// Múltiples meseros pueden leer el menú SIMULTÁNEAMENTE.
    /// Ninguno puede leer mientras el Gerente está modificando el menú.
    /// </summary>
    public class Mesero
    {
        /// <summary>
        /// Identificador único del mesero.
        /// Ejemplo: "Mesero-1", "Mesero-3"
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Indica si el mesero está leyendo el menú en este momento.
        /// true = tiene el ReadLock adquirido.
        /// </summary>
        public bool EstaLeyendo { get; set; }

        /// <summary>
        /// Indica si el mesero está esperando para leer
        /// porque el Gerente tiene el WriteLock.
        /// </summary>
        public bool EstaEsperando { get; set; }

        /// <summary>
        /// Total de lecturas completadas por este mesero.
        /// </summary>
        public int LecturasCompletadas { get; set; }

        /// <summary>
        /// Versión del menú que este mesero leyó por última vez.
        /// Si es menor que la versión actual, el mesero tiene info desactualizada.
        /// Durante un ataque, puede ser una versión comprometida.
        /// </summary>
        public int VersionMenuLeida { get; set; }

        /// <summary>
        /// Indica si este mesero está operando con un menú comprometido.
        /// true cuando leyó el menú durante o después de un ataque.
        /// Se muestra en rojo en la UI.
        /// </summary>
        public bool LeyoMenuComprometido { get; set; }

        /// <summary>
        /// Último ítem del menú que este mesero consultó.
        /// </summary>
        public string? UltimoItemLeido { get; set; }

        /// <summary>
        /// Timestamp del inicio de la última sesión de lectura.
        /// </summary>
        public DateTime? InicioUltimaLectura { get; set; }

        /// <summary>
        /// Indica si este hilo está activo.
        /// </summary>
        public bool EstaActivo { get; set; }

        public Mesero(string id)
        {
            Id = id;
            EstaLeyendo = false;
            EstaEsperando = false;
            LecturasCompletadas = 0;
            VersionMenuLeida = 0;
            LeyoMenuComprometido = false;
            EstaActivo = false;
        }

        public override string ToString() =>
            $"{Id} - {LecturasCompletadas} lecturas" +
            (LeyoMenuComprometido ? " ⚠ COMPROMETIDO" : "");
    }
}