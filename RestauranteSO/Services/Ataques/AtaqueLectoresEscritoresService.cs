// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Services/Ataques/AtaqueLectoresEscritoresService.cs
// Propósito: Servicio de ataque simulado sobre Lectores-Escritores.
//            Simula phishing al gerente que resulta en menú comprometido.
//            ⚠ SOLO SIMULACIÓN EDUCATIVA. ⚠
// SOLID    : SRP, DIP. Implementa IAttackService.
// =============================================================================

using System.Collections.Concurrent;
using RestauranteSO.Constants;
using RestauranteSO.Domain.Entities;
using RestauranteSO.Domain.Enums;
using RestauranteSO.Domain.Interfaces;
using RestauranteSO.Domain.Models;

namespace RestauranteSO.Services.Ataques
{
    /// <summary>
    /// Servicio de ataque simulado sobre el menú compartido (Lectores-Escritores).
    ///
    /// ESCENARIO EDUCATIVO:
    /// El gerente del restaurante recibe un correo electrónico que parece
    /// provenir del "Sistema de Gestión del Restaurante". El correo dice:
    /// "Su acceso expirará en 24 horas. Haga clic aquí para renovar."
    /// El gerente, presionado por el tiempo, hace clic y entrega sus
    /// credenciales en una página falsa.
    /// El atacante usa esas credenciales para:
    ///   - Cambiar precios del menú (×3)
    ///   - Reemplazar nombres de platos
    ///   - Marcar platos disponibles como no disponibles
    /// Los meseros (lectores) comienzan a leer el menú comprometido
    /// y dan información incorrecta a los clientes.
    ///
    /// VULNERABILIDADES EXPLOTADAS:
    ///   1. Correo de phishing sin verificación de dominio
    ///   2. Sin MFA: credenciales = acceso total
    ///   3. Sin control de cambios en el menú (quién cambió qué, cuándo)
    ///   4. Sin validación de origen de modificaciones
    ///   5. Meseros sin forma de verificar integridad del menú
    /// </summary>
    public sealed class AtaqueLectoresEscritoresService : IAttackService
    {
        // ─── DEPENDENCIAS ─────────────────────────────────────────────────────

        private readonly IMenuRepository _menuRepo;
        private readonly ISimulationLogger _logger;

        // ─── ESTADO ──────────────────────────────────────────────────────────

        private volatile bool _isAttackActive = false;
        private AttackType _tipoAtaqueActivo  = AttackType.Ninguno;

        private readonly ConcurrentQueue<AttackEvent> _historial = new();
        private readonly Random _random = new();

        // ─── LISTA DE CAMBIOS ────────────────────────────────────────────────

        /// <summary>
        /// Registro de qué cambió en el menú durante el ataque.
        /// Mostrado en el panel de evidencia de la UI.
        /// </summary>
        public IReadOnlyList<CambioMenu> CambiosRealizados =>
            _cambiosRealizados.ToList().AsReadOnly();

        private readonly List<CambioMenu> _cambiosRealizados = new();
        private readonly object _lockCambios = new object();

        // ─── CONSTRUCTOR ─────────────────────────────────────────────────────

        public AtaqueLectoresEscritoresService(
            IMenuRepository menuRepo,
            ISimulationLogger logger)
        {
            _menuRepo = menuRepo ?? throw new ArgumentNullException(nameof(menuRepo));
            _logger   = logger   ?? throw new ArgumentNullException(nameof(logger));
        }

        // ─── IAttackService ───────────────────────────────────────────────────

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
            _isAttackActive   = true;

            // Ejecutar la modificación del menú en un Task separado
            // para no bloquear el UI thread
            Task.Run(() => EjecutarAtaqueMenuAsync());

            var evento = new AttackEvent
            {
                TipoAtaque         = tipo,
                Descripcion        = "🎣 PHISHING exitoso: Credenciales del Gerente comprometidas",
                ComponenteAfectado = "Menú del Restaurante (Recurso Compartido)",
                FuenteAtaque       = "Atacante via Phishing (correo falso)",
                ValorOriginal      = "Menú con precios y nombres correctos",
                ValorAlterado      = "Menú con precios triplicados y platos falsos",
                ImpactoNegocio     = "Meseros informan precios incorrectos. " +
                                     "Clientes pagan hasta 3x más. " +
                                     "Reputación del restaurante dañada."
            };

            _historial.Enqueue(evento);
            AtaqueActivado?.Invoke(this, tipo);
        }

        public void DesactivarAtaque()
        {
            _isAttackActive   = false;
            _tipoAtaqueActivo = AttackType.Ninguno;

            // Restaurar el menú a su estado original
            _menuRepo.Restaurar();

            lock (_lockCambios)
                _cambiosRealizados.Clear();

            _logger.Log(
                "🔄 Menú restaurado a su estado original.",
                LogLevel.Security);

            AtaqueDesactivado?.Invoke(this, EventArgs.Empty);
        }

        public AttackEvent ObtenerEventoAtaque()
        {
            return new AttackEvent
            {
                TipoAtaque         = AttackType.PhishingMenuAlterado,
                Descripcion        = "Lectura de menú comprometido",
                ComponenteAfectado = "Menú (ReaderWriterLockSlim - ReadLock)",
                FuenteAtaque       = "Atacante Phishing",
                ImpactoNegocio     = "Mesero informa precio/plato incorrecto al cliente"
            };
        }

        // ─── LÓGICA DEL ATAQUE ────────────────────────────────────────────────

        /// <summary>
        /// Ejecuta las modificaciones fraudulentas al menú.
        /// Simula que el atacante, con las credenciales robadas,
        /// modifica el menú usando el sistema legítimo.
        /// 
        /// IMPORTANTE: Este método NO tiene WriteLock porque simula que
        /// el atacante usa las credenciales del gerente para entrar al sistema
        /// de forma "legítima". El sistema no puede distinguir entre el
        /// gerente real y el atacante usando sus credenciales.
        /// </summary>
        private async Task EjecutarAtaqueMenuAsync()
        {
            await Task.Delay(1500); // Simular tiempo de "login" del atacante

            _logger.Log(
                "⚡ ATACANTE: Accediendo con credenciales robadas del Gerente...",
                LogLevel.Attack,
                "ATACANTE");

            var items = _menuRepo.ObtenerCompleto();

            // Alterar TODOS los items del menú
            foreach (var item in items)
            {
                // Pequeña demora entre cambios para que sea visible en la UI
                await Task.Delay(_random.Next(200, 600));

                if (!_isAttackActive) break;

                string nombreOriginal  = item.NombreOriginal;
                decimal precioOriginal = item.PrecioOriginal;

                // Triplicar precios
                decimal precioFalso = item.Precio * SimulationConstants.MultiplicadorPrecioAtaque;

                // Nombre con indicador de compromiso
                string nombreFalso = SimulationConstants.PlatosAtaque[
                    _random.Next(SimulationConstants.PlatosAtaque.Length)];

                // Aplicar modificación fraudulenta
                item.AplicarModificacionAtaque(nombreFalso, precioFalso);
                _menuRepo.Actualizar(item);

                // Registrar el cambio
                var cambio = new CambioMenu
                {
                    ItemId           = item.Id,
                    NombreOriginal   = nombreOriginal,
                    NombreAlterado   = nombreFalso,
                    PrecioOriginal   = precioOriginal,
                    PrecioAlterado   = precioFalso,
                    Timestamp        = DateTime.Now,
                    RealizadoPor     = "⚡ ATACANTE (credenciales robadas)"
                };

                lock (_lockCambios)
                    _cambiosRealizados.Add(cambio);

                var evento = new AttackEvent
                {
                    TipoAtaque         = AttackType.PhishingMenuAlterado,
                    Descripcion        = $"Ítem alterado: '{nombreOriginal}' → '{nombreFalso}'",
                    ComponenteAfectado = $"Menú - {item.Categoria}",
                    FuenteAtaque       = "ATACANTE (credenciales phishing)",
                    ValorOriginal      = $"{nombreOriginal} - ${precioOriginal:N2}",
                    ValorAlterado      = $"{nombreFalso} - ${precioFalso:N2}",
                    ImpactoNegocio     = "Mesero informa información falsa al cliente"
                };

                _historial.Enqueue(evento);

                _logger.Log(
                    $"⚡ ATAQUE: '{nombreOriginal}' → '{nombreFalso}' " +
                    $"${precioOriginal:N2} → ${precioFalso:N2}",
                    LogLevel.Attack,
                    "ATACANTE");
            }

            _logger.Log(
                $"⚡ ATAQUE COMPLETADO: {items.Count} ítems del menú comprometidos. " +
                $"Los meseros ahora leen información falsa.",
                LogLevel.Attack,
                "ATACANTE");
        }

        // ─── POLÍTICAS DE PREVENCIÓN ─────────────────────────────────────────

        public static IReadOnlyList<PoliticaSeguridad> ObtenerPoliticas()
        {
            return new List<PoliticaSeguridad>
            {
                new PoliticaSeguridad
                {
                    Titulo         = "📧 Protección Anti-Phishing",
                    Categoria      = "Email Security",
                    Descripcion    = "Implementar filtros de correo avanzados con análisis " +
                                     "de reputación de dominio, SPF, DKIM y DMARC. " +
                                     "Entrenar al personal para verificar el dominio exacto " +
                                     "del remitente antes de hacer clic en cualquier enlace.",
                    Vulnerabilidad = "El Gerente hizo clic en un enlace de phishing sin verificar el dominio.",
                    Implementacion = "Microsoft Defender for Office 365, Proofpoint, " +
                                     "capacitación con simulaciones de phishing mensuales."
                },
                new PoliticaSeguridad
                {
                    Titulo         = "🔐 Autenticación Multifactor (MFA)",
                    Categoria      = "Autenticación",
                    Descripcion    = "Con MFA activo, el atacante habría obtenido la contraseña " +
                                     "pero no el segundo factor. El acceso habría sido denegado. " +
                                     "MFA reduce el riesgo de compromiso de credenciales en >99%.",
                    Vulnerabilidad = "Las credenciales solas (sin segundo factor) daban acceso total.",
                    Implementacion = "Microsoft Authenticator, Google Authenticator, " +
                                     "YubiKey para accesos críticos. Obligatorio para Gerencia."
                },
                new PoliticaSeguridad
                {
                    Titulo         = "📝 Control de Cambios en el Menú",
                    Categoria      = "Auditoría",
                    Descripcion    = "Implementar sistema de control de cambios con aprobación " +
                                     "de dos personas (Four Eyes Principle) para cualquier " +
                                     "modificación del menú. Cada cambio debe tener justificación " +
                                     "y ser notificado al equipo.",
                    Vulnerabilidad = "Una sola persona podía modificar el menú sin aprobación.",
                    Implementacion = "Workflow de aprobación, notificaciones SMS/email de cambios, " +
                                     "log inmutable de modificaciones con timestamp."
                },
                new PoliticaSeguridad
                {
                    Titulo         = "🔍 Monitoreo de Comportamiento Anómalo",
                    Categoria      = "Detección",
                    Descripcion    = "Alertas automáticas cuando se detectan cambios masivos " +
                                     "en poco tiempo (ej: 14 items modificados en 8 segundos). " +
                                     "Bloqueo automático de cuentas con comportamiento anómalo.",
                    Vulnerabilidad = "No había detección de cambios masivos no habituales.",
                    Implementacion = "SIEM con reglas de correlación, User and Entity " +
                                     "Behavior Analytics (UEBA), alertas en tiempo real."
                },
                new PoliticaSeguridad
                {
                    Titulo         = "🌐 Segmentación de Red",
                    Categoria      = "Infraestructura",
                    Descripcion    = "El sistema de menú debería estar en una VLAN separada " +
                                     "con acceso solo desde terminales autorizadas. " +
                                     "Un atacante externo no podría acceder aunque tenga credenciales " +
                                     "si se conecta desde una IP/dispositivo no autorizado.",
                    Vulnerabilidad = "El sistema era accesible desde cualquier dispositivo.",
                    Implementacion = "VLANs, Zero Trust Network Access, " +
                                     "acceso condicional basado en dispositivo y ubicación."
                },
                new PoliticaSeguridad
                {
                    Titulo         = "💾 Backup y Versionado del Menú",
                    Categoria      = "Recuperación",
                    Descripcion    = "Mantener versiones históricas del menú con posibilidad " +
                                     "de rollback inmediato. Ante cualquier compromiso, " +
                                     "restaurar la última versión buena en segundos.",
                    Vulnerabilidad = "No había forma rápida de restaurar el menú original.",
                    Implementacion = "Versionado automático cada hora, snapshots antes de cambios, " +
                                     "proceso de rollback documentado y probado."
                },
                new PoliticaSeguridad
                {
                    Titulo         = "🎓 Concientización del Personal",
                    Categoria      = "Educación",
                    Descripcion    = "El eslabón más débil en seguridad siempre es el humano. " +
                                     "Capacitación obligatoria mensual en: reconocimiento de " +
                                     "phishing, manejo de credenciales, reporte de incidentes, " +
                                     "y qué hacer al recibir un correo sospechoso.",
                    Vulnerabilidad = "El Gerente no reconoció las señales de phishing.",
                    Implementacion = "KnowBe4, Proofpoint Security Awareness, " +
                                     "simulaciones regulares, métricas de click-rate."
                }
            };
        }
    }

    /// <summary>
    /// Registro de un cambio realizado al menú durante el ataque.
    /// Mostrado en la tabla de evidencia de la UI.
    /// </summary>
    public sealed class CambioMenu
    {
        public int     ItemId           { get; init; }
        public string  NombreOriginal   { get; init; } = string.Empty;
        public string  NombreAlterado   { get; init; } = string.Empty;
        public decimal PrecioOriginal   { get; init; }
        public decimal PrecioAlterado   { get; init; }
        public DateTime Timestamp       { get; init; }
        public string  RealizadoPor     { get; init; } = string.Empty;

        public decimal DiferenciaPrecio => PrecioAlterado - PrecioOriginal;
        public string  ResumenCambio    =>
            $"{NombreOriginal} (${PrecioOriginal:N2}) → {NombreAlterado} (${PrecioAlterado:N2})";
    }
}