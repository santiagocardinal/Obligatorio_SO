// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Domain/Interfaces/IAttackService.cs
// Propósito: Contrato para los servicios de simulación de ataques.
// SOLID    : ISP - segregado de ISimulationService.
//            DIP - los workers dependen de esta abstracción.
// =============================================================================

using RestauranteSO.Domain.Enums;
using RestauranteSO.Domain.Models;

namespace RestauranteSO.Domain.Interfaces
{
    /// <summary>
    /// Contrato para todos los servicios de ataque educativo simulado.
    /// 
    /// Los workers de la simulación inyectan este servicio y consultan
    /// IsAttackActive antes de cada operación para decidir si
    /// comportarse normalmente o de forma "comprometida".
    /// 
    /// IMPORTANTE: Ningún método de esta interfaz realiza ataques reales.
    /// Todo es simulación educativa para mostrar vulnerabilidades.
    /// </summary>
    public interface IAttackService
    {
        // ─── ESTADO DEL ATAQUE ───────────────────────────────────────────────

        /// <summary>
        /// Indica si hay un ataque simulado activo actualmente.
        /// Los workers consultan esta propiedad en cada iteración.
        /// Thread-safe mediante volatile o Interlocked.
        /// </summary>
        bool IsAttackActive { get; }

        /// <summary>
        /// Tipo de ataque que está activo actualmente.
        /// AttackType.Ninguno cuando no hay ataque.
        /// </summary>
        AttackType TipoAtaqueActivo { get; }

        // ─── ACTIVACIÓN / DESACTIVACIÓN ──────────────────────────────────────

        /// <summary>
        /// Activa el ataque simulado del tipo especificado.
        /// Dispara el evento AtaqueActivado con la descripción del ataque.
        /// </summary>
        /// <param name="tipo">Tipo de ataque a simular</param>
        void ActivarAtaque(AttackType tipo);

        /// <summary>
        /// Desactiva el ataque activo y restaura el estado normal.
        /// Dispara el evento AtaqueDesactivado.
        /// </summary>
        void DesactivarAtaque();

        // ─── LÓGICA DEL ATAQUE ───────────────────────────────────────────────

        /// <summary>
        /// Retorna el evento de ataque más reciente para mostrar en la UI.
        /// Los workers llaman a este método para obtener qué acción maliciosa
        /// deben simular (duplicar, eliminar, alterar).
        /// </summary>
        AttackEvent ObtenerEventoAtaque();

        /// <summary>
        /// Lista completa de eventos de ataque ocurridos en esta sesión.
        /// Usada para construir el reporte de evidencia.
        /// </summary>
        IReadOnlyList<AttackEvent> HistorialAtaques { get; }

        // ─── EVENTOS ─────────────────────────────────────────────────────────

        /// <summary>
        /// Disparado cuando se activa un ataque simulado.
        /// La UI lo usa para mostrar el diálogo de ingeniería social.
        /// </summary>
        event EventHandler<AttackType> AtaqueActivado;

        /// <summary>
        /// Disparado cuando se desactiva el ataque.
        /// La UI lo usa para mostrar el panel de políticas de prevención.
        /// </summary>
        event EventHandler AtaqueDesactivado;
    }
}