// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Domain/Models/LogEntry.cs
// Propósito: Entrada individual del log de la simulación.
//            Immutable por diseño para thread-safety.
// SOLID    : Single Responsibility.
// =============================================================================

using RestauranteSO.Domain.Enums;

namespace RestauranteSO.Domain.Models
{
    /// <summary>
    /// Entrada inmutable del log de la simulación.
    /// 
    /// Cada acción importante de los workers genera un LogEntry.
    /// La UI los muestra en el LogViewer con colores según el nivel.
    /// 
    /// Thread Safety: Immutable después de la creación.
    /// No requiere sincronización para leer.
    /// </summary>
    public sealed class LogEntry
    {
        /// <summary>Timestamp exacto del evento.</summary>
        public DateTime Timestamp { get; } = DateTime.Now;

        /// <summary>Nivel de severidad/categoría del log.</summary>
        public LogLevel Nivel { get; init; }

        /// <summary>
        /// Nombre del hilo o componente que generó el log.
        /// Ejemplo: "Productor-1", "Cocinero-3", "ReaderWriterLock"
        /// </summary>
        public string Fuente { get; init; } = string.Empty;

        /// <summary>Mensaje descriptivo del evento.</summary>
        public string Mensaje { get; init; } = string.Empty;

        /// <summary>
        /// Color de fondo para este entry en el LogViewer.
        /// Determinado por el nivel del log.
        /// </summary>
        public Color ColorFondo => Nivel switch
        {
            LogLevel.Info    => Color.FromArgb(30, 30, 30),
            LogLevel.Sync    => Color.FromArgb(20, 40, 70),
            LogLevel.Warning => Color.FromArgb(60, 50, 10),
            LogLevel.Error   => Color.FromArgb(70, 15, 15),
            LogLevel.Attack  => Color.FromArgb(70, 35, 10),
            LogLevel.Security=> Color.FromArgb(15, 60, 25),
            _                => Color.FromArgb(30, 30, 30)
        };

        /// <summary>
        /// Color del texto para este entry.
        /// </summary>
        public Color ColorTexto => Nivel switch
        {
            LogLevel.Info    => Color.FromArgb(200, 200, 200),
            LogLevel.Sync    => Color.FromArgb(100, 180, 255),
            LogLevel.Warning => Color.FromArgb(255, 220, 50),
            LogLevel.Error   => Color.FromArgb(255, 80, 80),
            LogLevel.Attack  => Color.FromArgb(255, 140, 0),
            LogLevel.Security=> Color.FromArgb(80, 220, 120),
            _                => Color.White
        };

        /// <summary>
        /// Prefijo emoji para identificar el nivel visualmente.
        /// </summary>
        public string Prefijo => Nivel switch
        {
            LogLevel.Info    => "ℹ",
            LogLevel.Sync    => "🔒",
            LogLevel.Warning => "⚠",
            LogLevel.Error   => "❌",
            LogLevel.Attack  => "⚡",
            LogLevel.Security=> "🛡",
            _                => "•"
        };

        /// <summary>
        /// Texto completo formateado para mostrar en el log.
        /// </summary>
        public string TextoCompleto =>
            $"[{Timestamp:HH:mm:ss.fff}] {Prefijo} [{Fuente,-15}] {Mensaje}";

        public override string ToString() => TextoCompleto;
    }
}