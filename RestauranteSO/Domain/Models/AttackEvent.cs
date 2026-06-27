// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Domain/Models/AttackEvent.cs
// Propósito: Representa un evento de ataque simulado ocurrido.
//            Usado para el historial y el reporte de evidencia.
// SOLID    : Single Responsibility. Solo datos del evento.
// =============================================================================

using RestauranteSO.Domain.Enums;

namespace RestauranteSO.Domain.Models
{
    /// <summary>
    /// Representa un evento individual ocurrido durante una simulación de ataque.
    /// 
    /// Cada vez que un worker ejecuta una acción maliciosa simulada
    /// (duplicar pedido, alterar menú, etc.) se crea un AttackEvent
    /// que queda registrado en el historial del IAttackService.
    /// 
    /// Estos eventos construyen el reporte de evidencia que se muestra
    /// al desactivar el ataque y mostrar las políticas de prevención.
    /// </summary>
    public sealed class AttackEvent
    {
        /// <summary>
        /// Identificador único del evento.
        /// </summary>
        public Guid Id { get; } = Guid.NewGuid();

        /// <summary>
        /// Tipo de ataque que generó este evento.
        /// </summary>
        public AttackType TipoAtaque { get; init; }

        /// <summary>
        /// Descripción del evento en lenguaje natural.
        /// Ejemplo: "Pedido #42 duplicado fraudulentamente"
        ///          "Precio de 'Pizza Margherita' alterado de $800 a $2400"
        /// </summary>
        public string Descripcion { get; init; } = string.Empty;

        /// <summary>
        /// Componente del sistema que fue afectado.
        /// Ejemplo: "Cola de pedidos", "Menú - Platos principales"
        /// </summary>
        public string ComponenteAfectado { get; init; } = string.Empty;

        /// <summary>
        /// Timestamp exacto en que ocurrió el evento.
        /// </summary>
        public DateTime Timestamp { get; } = DateTime.Now;

        /// <summary>
        /// Nombre del hilo que ejecutó la acción maliciosa.
        /// Ejemplo: "Cocinero-1 (comprometido)", "EscritorAtacante"
        /// </summary>
        public string FuenteAtaque { get; init; } = string.Empty;

        /// <summary>
        /// Valor original antes de la alteración.
        /// Para pedidos: descripción original.
        /// Para menú: precio/nombre original.
        /// </summary>
        public string ValorOriginal { get; init; } = string.Empty;

        /// <summary>
        /// Valor después de la alteración maliciosa.
        /// </summary>
        public string ValorAlterado { get; init; } = string.Empty;

        /// <summary>
        /// Impacto estimado en el negocio del evento.
        /// Ejemplo: "Cliente recibió plato equivocado"
        ///          "Mesero cotizó precio 3x mayor al real"
        /// </summary>
        public string ImpactoNegocio { get; init; } = string.Empty;

        /// <summary>
        /// Formato para el log del ataque.
        /// </summary>
        public override string ToString() =>
            $"[{Timestamp:HH:mm:ss.fff}] {TipoAtaque} | {Descripcion}";
    }
}