// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Domain/Interfaces/ISimulationLogger.cs
// Propósito: Contrato para el sistema de logging de las simulaciones.
// SOLID    : ISP, DIP. Los servicios dependen de esta interfaz.
// =============================================================================

using RestauranteSO.Domain.Enums;
using RestauranteSO.Domain.Models;

namespace RestauranteSO.Domain.Interfaces
{
    /// <summary>
    /// Contrato para el logger utilizado por todos los servicios de simulación.
    /// 
    /// Permite que los servicios registren eventos sin conocer el destino
    /// del log (puede ser UI, archivo, memoria, etc.).
    /// 
    /// Thread-safe: Implementado con ConcurrentQueue internamente.
    /// </summary>
    public interface ISimulationLogger
    {
        /// <summary>
        /// Registra un mensaje con el nivel especificado.
        /// Thread-safe. Puede llamarse desde cualquier hilo.
        /// </summary>
        /// <param name="mensaje">Texto del mensaje a registrar</param>
        /// <param name="nivel">Nivel de severidad/categoría del mensaje</param>
        /// <param name="fuente">Nombre del hilo o componente que origina el log</param>
        void Log(string mensaje, LogLevel nivel = LogLevel.Info, string fuente = "Sistema");

        /// <summary>
        /// Retorna todos los entries de log acumulados hasta ahora.
        /// Ordenados del más nuevo al más antiguo.
        /// Thread-safe.
        /// </summary>
        IReadOnlyList<LogEntry> ObtenerLogs();

        /// <summary>
        /// Retorna solo los logs más recientes.
        /// Optimizado para actualización periódica de la UI.
        /// </summary>
        /// <param name="cantidad">Cantidad máxima de logs a retornar</param>
        IReadOnlyList<LogEntry> ObtenerUltimosLogs(int cantidad);

        /// <summary>
        /// Elimina todos los logs acumulados.
        /// Llamado al iniciar una nueva sesión de simulación.
        /// </summary>
        void Limpiar();

        /// <summary>
        /// Cantidad total de entries registrados.
        /// </summary>
        int CantidadEntries { get; }

        /// <summary>
        /// Evento disparado cuando se agrega un nuevo log.
        /// La UI puede subscribirse para actualizar en tiempo real.
        /// IMPORTANTE: El handler debe hacer Invoke() para actualizar la UI.
        /// </summary>
        event EventHandler<LogEntry> NuevoLogAgregado;
    }
}