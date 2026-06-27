// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Services/Ataques/AtaqueProductorConsumidorService.cs
// Propósito: Servicio de simulación de ataque sobre Productor-Consumidor.
//            Gestiona el estado del ataque, el historial de eventos,
//            y provee las políticas de prevención.
//            ⚠ SOLO SIMULACIÓN EDUCATIVA. NINGÚN CÓDIGO MALICIOSO REAL. ⚠
// SOLID    : SRP, DIP. Implementa IAttackService.
// =============================================================================

using System.Collections.Concurrent;
using RestauranteSO.Domain.Enums;
using RestauranteSO.Domain.Interfaces;
using RestauranteSO.Domain.Models;

namespace RestauranteSO.Services.Ataques
{
    /// <summary>
    /// Servicio de ataque simulado sobre la cola Productor-Consumidor.
    ///
    /// ESCENARIO EDUCATIVO:
    /// Un atacante (disfrazado de "técnico de soporte") convence al encargado
    /// del restaurante para que le permita "actualizar el sistema de pedidos".
    /// El encargado, sin verificar la identidad, le da acceso físico.
    /// El atacante instala un "agente de monitoreo" que en realidad:
    ///   - Duplica pedidos (crea trabajo innecesario en la cocina)
    ///   - Elimina pedidos (clientes que no reciben su comida)
    ///   - Altera pedidos (clientes reciben platos incorrectos)
    ///
    /// VULNERABILIDADES EXPLOTADAS:
    ///   1. Falta de verificación de identidad del visitante
    ///   2. Acceso físico no supervisado al sistema
    ///   3. Sin autenticación de dos factores para el encargado
    ///   4. Sin alertas de integridad en la cola de pedidos
    ///   5. Sin registro de auditoría de accesos físicos
    ///
    /// Este servicio es inyectado en los ProductorWorkers.
    /// Los workers consultan IsAttackActive en cada iteración.
    /// </summary>
    public sealed class AtaqueProductorConsumidorService : IAttackService
    {
        // ─── ESTADO ──────────────────────────────────────────────────────────

        /// <summary>
        /// volatile: garantiza que todos los hilos ven el cambio inmediatamente.
        /// Sin volatile, el JIT podría cachear este valor en un registro
        /// y un hilo podría no ver la actualización del UI thread.
        /// </summary>
        private volatile bool _isAttackActive = false;

        private AttackType _tipoAtaqueActivo = AttackType.Ninguno;

        private readonly ConcurrentQueue<AttackEvent> _historial = new();

        private readonly Random _random = new();

        // ─── IMPLEMENTACIÓN IAttackService ────────────────────────────────────

        public bool IsAttackActive => _isAttackActive;

        public AttackType TipoAtaqueActivo => _tipoAtaqueActivo;

        public IReadOnlyList<AttackEvent> HistorialAtaques =>
            _historial.ToArray().ToList().AsReadOnly();

        public event EventHandler<AttackType>? AtaqueActivado;
        public event EventHandler? AtaqueDesactivado;

        // ─── ACTIVACIÓN ───────────────────────────────────────────────────────

        public void ActivarAtaque(AttackType tipo)
        {
            _tipoAtaqueActivo = tipo;
            _isAttackActive   = true;   // volatile write: visible a todos los hilos

            var evento = new AttackEvent
            {
                TipoAtaque         = tipo,
                Descripcion        = "🚨 ATAQUE ACTIVADO: Agente malicioso instalado en el sistema de pedidos",
                ComponenteAfectado = "Cola de Pedidos (ConcurrentQueue)",
                FuenteAtaque       = "Falso Técnico de Soporte (Ingeniería Social)",
                ValorOriginal      = "Sistema de pedidos normal",
                ValorAlterado      = "Sistema comprometido con agente de inyección",
                ImpactoNegocio     = "Pedidos duplicados, eliminados y alterados. " +
                                     "Clientes reciben platos incorrectos. " +
                                     "Cocineros preparan pedidos inexistentes."
            };

            _historial.Enqueue(evento);
            AtaqueActivado?.Invoke(this, tipo);
        }

        public void DesactivarAtaque()
        {
            _isAttackActive   = false;
            _tipoAtaqueActivo = AttackType.Ninguno;
            AtaqueDesactivado?.Invoke(this, EventArgs.Empty);
        }

        public AttackEvent ObtenerEventoAtaque()
        {
            // Retorna el tipo de acción maliciosa a ejecutar
            // Los workers llaman a esto para decidir qué hacer
            int dado = _random.Next(100);

            string descripcion;
            if (dado < 25)
                descripcion = "Duplicar pedido en cola";
            else if (dado < 45)
                descripcion = "Eliminar pedido de cola";
            else
                descripcion = "Alterar descripción de pedido";

            return new AttackEvent
            {
                TipoAtaque         = AttackType.InyeccionDePedidos,
                Descripcion        = descripcion,
                ComponenteAfectado = "Cola de Pedidos",
                FuenteAtaque       = "Agente Malicioso (Sistema Comprometido)",
                ImpactoNegocio     = "Integridad de la cola de pedidos comprometida"
            };
        }

        // ─── POLÍTICAS DE PREVENCIÓN ─────────────────────────────────────────

        /// <summary>
        /// Retorna la lista completa de políticas de prevención para este ataque.
        /// Mostradas en FrmPoliticas después de la simulación del ataque.
        /// </summary>
        public static IReadOnlyList<PoliticaSeguridad> ObtenerPoliticas()
        {
            return new List<PoliticaSeguridad>
            {
                new PoliticaSeguridad
                {
                    Titulo       = "🔐 Control de Acceso Físico",
                    Categoria    = "Acceso",
                    Descripcion  = "Implementar registro de visitantes con verificación " +
                                   "de identidad obligatoria. Ningún técnico externo " +
                                   "puede acceder a sistemas sin acompañamiento de " +
                                   "personal autorizado y aprobación previa por escrito.",
                    Vulnerabilidad = "El atacante obtuvo acceso físico sin verificación.",
                    Implementacion = "Libro de visitas digital, verificación de credenciales, " +
                                     "cámaras en sala de servidores, tarjetas de acceso por zonas."
                },
                new PoliticaSeguridad
                {
                    Titulo       = "📚 Capacitación en Ingeniería Social",
                    Categoria    = "Educación",
                    Descripcion  = "Todo el personal debe recibir capacitación periódica " +
                                   "para reconocer intentos de ingeniería social. " +
                                   "Incluir simulacros de ataques de phishing presencial.",
                    Vulnerabilidad = "El encargado no reconoció las señales de un atacante.",
                    Implementacion = "Talleres trimestrales, simulacros de phishing, " +
                                     "proceso de reporte de incidentes sospechosos."
                },
                new PoliticaSeguridad
                {
                    Titulo       = "🔒 Autenticación Multifactor (MFA)",
                    Categoria    = "Autenticación",
                    Descripcion  = "Implementar MFA para cualquier acción que modifique " +
                                   "la configuración del sistema de pedidos. " +
                                   "Una segunda verificación habría impedido la instalación.",
                    Vulnerabilidad = "El atacante instaló software con una sola aprobación verbal.",
                    Implementacion = "Autenticator app, SMS de verificación, " +
                                     "aprobación de gerencia senior para cambios de sistema."
                },
                new PoliticaSeguridad
                {
                    Titulo       = "📊 Registro de Auditoría (Audit Log)",
                    Categoria    = "Monitoreo",
                    Descripcion  = "Registrar todas las operaciones en la cola de pedidos: " +
                                   "quién encola, quién desencola, timestamps, checksum. " +
                                   "Alertas automáticas ante patrones anómalos " +
                                   "(ej: tasa de duplicados > 5%).",
                    Vulnerabilidad = "No había forma de detectar manipulaciones en la cola.",
                    Implementacion = "Sistema de logging centralizado, SIEM, " +
                                     "alertas en tiempo real, revisión diaria de logs."
                },
                new PoliticaSeguridad
                {
                    Titulo       = "🛡 Principio de Mínimo Privilegio",
                    Categoria    = "Autorización",
                    Descripcion  = "Ningún técnico externo debe tener privilegios de " +
                                   "instalación de software en sistemas críticos. " +
                                   "Crear cuentas de servicio con permisos mínimos " +
                                   "y acceso limitado en tiempo.",
                    Vulnerabilidad = "El 'técnico' pudo instalar software con privilegios plenos.",
                    Implementacion = "Cuentas de servicio, just-in-time access, " +
                                     "revisión trimestral de permisos, PAM (Privileged Access Management)."
                },
                new PoliticaSeguridad
                {
                    Titulo       = "🔍 Integridad de la Cola de Pedidos",
                    Categoria    = "Técnico",
                    Descripcion  = "Implementar checksums y firmas digitales en cada pedido. " +
                                   "Validar la integridad antes de procesar. " +
                                   "Detectar y rechazar pedidos con firmas inválidas.",
                    Vulnerabilidad = "Los pedidos no tenían mecanismo de validación de integridad.",
                    Implementacion = "Hash SHA-256 por pedido, firma HMAC, " +
                                     "validación en el consumidor antes de procesar."
                },
                new PoliticaSeguridad
                {
                    Titulo       = "🔥 Firewall y EDR",
                    Categoria    = "Infraestructura",
                    Descripcion  = "Instalar solución EDR (Endpoint Detection & Response) " +
                                   "que detecte comportamiento anómalo de procesos. " +
                                   "Un agente que modifica colas de pedidos a alta frecuencia " +
                                   "sería detectado y bloqueado automáticamente.",
                    Vulnerabilidad = "No había solución de seguridad endpoint activa.",
                    Implementacion = "Microsoft Defender for Endpoint, CrowdStrike Falcon, " +
                                     "SentinelOne, con políticas de comportamiento configuradas."
                },
                new PoliticaSeguridad
                {
                    Titulo       = "💾 Backups y Plan de Continuidad",
                    Categoria    = "Recuperación",
                    Descripcion  = "Mantener backups automáticos del estado del sistema " +
                                   "para recuperación rápida ante ataques. " +
                                   "Plan de continuidad documentado y probado regularmente.",
                    Vulnerabilidad = "Un ataque exitoso podría haber causado pérdida de datos.",
                    Implementacion = "Backups cada 15 minutos, RTO < 1 hora, " +
                                     "RPO < 15 minutos, pruebas de restauración mensuales."
                }
            };
        }
    }

    /// <summary>
    /// Modelo de una política de seguridad para mostrar en FrmPoliticas.
    /// </summary>
    public sealed class PoliticaSeguridad
    {
        public string Titulo         { get; init; } = string.Empty;
        public string Categoria      { get; init; } = string.Empty;
        public string Descripcion    { get; init; } = string.Empty;
        public string Vulnerabilidad { get; init; } = string.Empty;
        public string Implementacion { get; init; } = string.Empty;
    }
}