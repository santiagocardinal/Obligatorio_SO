// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Domain/Enums/AttackType.cs
// Propósito: Tipo de ataque simulado para la sección de ciberseguridad.
// SOLID    : Single Responsibility. Open/Closed para nuevos ataques.
// =============================================================================

namespace RestauranteSO.Domain.Enums
{
    /// <summary>
    /// Categorías de ataques simulados utilizados en la Parte 4 del proyecto.
    /// Cada valor corresponde a una simulación educativa con ingeniería social.
    /// </summary>
    public enum AttackType
    {
        /// <summary>
        /// Sin ataque activo. Estado normal de la simulación.
        /// </summary>
        Ninguno = 0,

        /// <summary>
        /// Ataque sobre Productor-Consumidor.
        /// Vector: Ingeniería Social → Falso técnico de soporte.
        /// Consecuencia: Inyección de pedidos falsos, duplicados y eliminación
        ///               de pedidos legítimos en la cola compartida.
        /// Vulnerabilidad explotada: Falta de validación de identidad,
        ///                           acceso físico no autorizado al sistema.
        /// </summary>
        InyeccionDePedidos = 1,

        /// <summary>
        /// Ataque sobre Lectores-Escritores.
        /// Vector: Phishing → Correo electrónico fraudulento al gerente.
        /// Consecuencia: Credenciales comprometidas, menú alterado,
        ///               lectores operando con información falsa.
        /// Vulnerabilidad explotada: Falta de MFA, ausencia de verificación
        ///                           de origen de correos electrónicos.
        /// </summary>
        PhishingMenuAlterado = 2,

        /// <summary>
        /// Ataque de race condition simulado.
        /// Explota la ventana de tiempo entre lectura y escritura
        /// cuando el lock no está correctamente implementado.
        /// Usado con fines educativos para mostrar qué pasa SIN sincronización.
        /// </summary>
        RaceConditionMenul = 3
    }
}