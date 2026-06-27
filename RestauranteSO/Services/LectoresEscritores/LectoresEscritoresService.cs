// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Services/LectoresEscritores/LectoresEscritoresService.cs
// Propósito: Orquestador de la simulación Lectores-Escritores.
//            Gestiona el ReaderWriterLockSlim, los LectorWorkers y EscritorWorker.
// SOLID    : SRP, OCP, DIP. Patrón Facade.
// =============================================================================

using RestauranteSO.Constants;
using RestauranteSO.Domain.Entities;
using RestauranteSO.Domain.Enums;
using RestauranteSO.Domain.Interfaces;
using RestauranteSO.Domain.Models;
using RestauranteSO.Infrastructure.Synchronization;

namespace RestauranteSO.Services.LectoresEscritores
{
    /// <summary>
    /// Servicio principal de la simulación Lectores-Escritores.
    ///
    /// ARQUITECTURA DE CONCURRENCIA:
    ///
    ///   _rwLock (ReaderWriterLockSlim):
    ///     Permite N lectores leyendo simultáneamente.
    ///     O 1 escritor con acceso exclusivo.
    ///     Nunca ambos al mismo tiempo.
    ///     Previene starvation de escritores.
    ///
    ///   Todos los LectorWorkers y el EscritorWorker comparten la MISMA
    ///   instancia de _rwLock y _menuRepo.
    ///
    ///   El menuRepo es el RECURSO COMPARTIDO que simula el menú del restaurante.
    /// </summary>
    public sealed class LectoresEscritoresService : ISimulationService, IDisposable
    {
        // ─── DEPENDENCIAS ─────────────────────────────────────────────────────

        private readonly IAttackService _attackService;
        private readonly ISimulationLogger _logger;
        private readonly IMenuRepository _menuRepo;

        // ─── PRIMITIVAS ───────────────────────────────────────────────────────

        private ReaderWriterLockSlim _rwLock = null!;
        private ManualResetEventSlim _eventoPausa = null!;
        private CancellationTokenSource _cts = null!;

        // ─── WORKERS ─────────────────────────────────────────────────────────

        private readonly List<LectorWorker> _lectorWorkers = new();
        private readonly List<Task> _lectorTasks = new();
        private EscritorWorker? _escritorWorker;
        private Task? _escritorTask;

        // ─── ENTIDADES ───────────────────────────────────────────────────────

        private readonly List<Mesero> _meseros = new();
        private int _contadorMeseros = 0;

        // ─── ESTADO ──────────────────────────────────────────────────────────

        private readonly SimulationState _estadoSimulacion = new();
        private volatile int _velocidadLectorMs = AppConstants.VelocidadLectorBaseMs;
        private volatile int _velocidadEscritorMs = AppConstants.IntervaloModificacionGerenteMs;
        private readonly object _lockWorkers = new object();

        // ─── EVENTOS ─────────────────────────────────────────────────────────

        public event EventHandler<SimulationStatus>? EstadoCambiado;
        public event EventHandler? ActualizacionDisponible;

        // ─── CONSTRUCTOR ─────────────────────────────────────────────────────

        public LectoresEscritoresService(
            IAttackService attackService,
            ISimulationLogger logger,
            IMenuRepository menuRepo)
        {
            _attackService = attackService ?? throw new ArgumentNullException(nameof(attackService));
            _logger        = logger        ?? throw new ArgumentNullException(nameof(logger));
            _menuRepo      = menuRepo      ?? throw new ArgumentNullException(nameof(menuRepo));
        }

        // ─── PROPIEDADES ─────────────────────────────────────────────────────

        public SimulationStatus Estado => _estadoSimulacion.Estado;
        public bool EstaCorreindo => _estadoSimulacion.Estado == SimulationStatus.Corriendo;
        public bool EstaUnderAttack => _attackService.IsAttackActive;

        public IReadOnlyList<Mesero> Meseros
        {
            get { lock (_lockWorkers) return _meseros.ToList().AsReadOnly(); }
        }

        public int LectoresActivos    => _rwLock?.CurrentReadCount ?? 0;
        public int LectoresEsperando  => _rwLock?.WaitingReadCount ?? 0;
        public bool EscritorActivo    => _rwLock?.IsWriteLockHeld ?? false;
        public bool EscritorEsperando => (_rwLock?.WaitingWriteCount ?? 0) > 0;
        public string? UltimaModificacion => _escritorWorker?.UltimaModificacion;
        public bool GerenteEscribiendo => _escritorWorker?.EstaEscribiendo ?? false;

        // ─── ISimulationService ───────────────────────────────────────────────

        public void Iniciar()
        {
            if (_estadoSimulacion.Estado == SimulationStatus.Corriendo)
                throw new InvalidOperationException("La simulación ya está corriendo.");

            _logger.Limpiar();
            _estadoSimulacion.Reset();

            _rwLock      = SyncPrimitiveFactory.CrearReaderWriterLock();
            _eventoPausa = SyncPrimitiveFactory.CrearEventoPausa();
            _cts         = SyncPrimitiveFactory.CrearCancellationTokenSource();

            _estadoSimulacion.InicioSimulacion = DateTime.Now;
            CambiarEstado(SimulationStatus.Corriendo);

            _logger.Log("═══════════════════════════════════════", LogLevel.Info);
            _logger.Log("   RESTAURANTE - MENÚ COMPARTIDO       ", LogLevel.Info);
            _logger.Log("═══════════════════════════════════════", LogLevel.Info);
            _logger.Log(
                $"Simulación Lectores-Escritores iniciada | " +
                $"ReaderWriterLockSlim creado | Modo: NoRecursion",
                LogLevel.Sync);

            // Crear escritor (Gerente)
            CrearEscritor();

            // Crear lectores iniciales (Meseros)
            for (int i = 0; i < AppConstants.LectoresIniciales; i++)
                AgregarLectorInterno();
        }

        public void Detener()
        {
            if (_estadoSimulacion.Estado == SimulationStatus.Detenida) return;

            CambiarEstado(SimulationStatus.Deteniendo);
            _logger.Log("Deteniendo simulación Lectores-Escritores...", LogLevel.Warning);

            _eventoPausa?.Set();
            _cts?.Cancel();

            // Esperar tasks (con timeout)
            var todasLasTasks = new List<Task>();
            lock (_lockWorkers) todasLasTasks.AddRange(_lectorTasks);
            if (_escritorTask != null) todasLasTasks.Add(_escritorTask);

            try { Task.WhenAll(todasLasTasks).Wait(5000); }
            catch { /* ignorar excepciones de cancelación */ }

            // Verificar que el lock no está adquirido antes de disponer
            // Si un task terminó con error y no liberó el lock, forzar liberación
            TryForzarLiberacionLock();

            lock (_lockWorkers)
            {
                _lectorWorkers.Clear();
                _lectorTasks.Clear();
                _meseros.Clear();
            }

            _escritorWorker = null;
            _escritorTask   = null;
            _contadorMeseros = 0;

            _rwLock?.Dispose();
            _eventoPausa?.Dispose();
            _cts?.Dispose();

            CambiarEstado(SimulationStatus.Detenida);
            _logger.Log("Simulación Lectores-Escritores detenida.", LogLevel.Info);
        }

        public void Pausar()
        {
            if (_estadoSimulacion.Estado != SimulationStatus.Corriendo) return;
            _eventoPausa.Reset();
            CambiarEstado(SimulationStatus.Pausada);
            _logger.Log("⏸ Simulación L-E PAUSADA.", LogLevel.Warning);
        }

        public void Reanudar()
        {
            if (_estadoSimulacion.Estado != SimulationStatus.Pausada) return;
            _eventoPausa.Set();
            CambiarEstado(SimulationStatus.Corriendo);
            _logger.Log("▶ Simulación L-E REANUDADA.", LogLevel.Info);
        }

        public SimulationStatistics ObtenerEstadisticas()
        {
            var tiempo = _estadoSimulacion.InicioSimulacion.HasValue
                ? DateTime.Now - _estadoSimulacion.InicioSimulacion.Value
                : TimeSpan.Zero;

            int comprometidos;
            lock (_lockWorkers)
                comprometidos = _meseros.Count(m => m.LeyoMenuComprometido);

            return new SimulationStatistics
            {
                TiempoEjecucion       = tiempo,
                EstadoActual          = _estadoSimulacion.Estado.ToString(),
                LectoresActivos       = _rwLock?.CurrentReadCount ?? 0,
                LectoresEsperando     = _rwLock?.WaitingReadCount ?? 0,
                EscritorActivo        = _rwLock?.IsWriteLockHeld ?? false,
                EscritorEsperando     = (_rwLock?.WaitingWriteCount ?? 0) > 0,
                TotalLecturas         = _estadoSimulacion.TotalLecturas,
                TotalEscrituras       = _estadoSimulacion.TotalEscrituras,
                LectoresComprometidos = comprometidos,
                AtaqueActivo          = _attackService.IsAttackActive,
                TotalEventosAtaque    = _estadoSimulacion.TotalEventosAtaque,
                TipoAtaque            = _attackService.TipoAtaqueActivo.ToString()
            };
        }

        public void AjustarVelocidad(int velocidadMs)
        {
            _velocidadLectorMs = Math.Max(100, velocidadMs);
            lock (_lockWorkers)
                foreach (var w in _lectorWorkers)
                    w.AjustarVelocidad(_velocidadLectorMs);
        }

        // ─── GESTIÓN DE WORKERS ───────────────────────────────────────────────

        public void AgregarLector()
        {
            lock (_lockWorkers)
            {
                if (_meseros.Count >= AppConstants.MaxLectores)
                {
                    _logger.Log(
                        $"Máximo de lectores alcanzado ({AppConstants.MaxLectores})",
                        LogLevel.Warning);
                    return;
                }
            }
            AgregarLectorInterno();
        }

        // ─── PRIVADOS ─────────────────────────────────────────────────────────

        private void CrearEscritor()
        {
            _escritorWorker = new EscritorWorker(
                "Gerente", _rwLock, _menuRepo,
                _attackService, _logger,
                _estadoSimulacion, _eventoPausa,
                _velocidadEscritorMs);

            _escritorWorker.ModificacionRealizada += (_, _) =>
                ActualizacionDisponible?.Invoke(this, EventArgs.Empty);

            _escritorTask = Task.Run(
                () => _escritorWorker.EjecutarAsync(_cts.Token),
                _cts.Token);
        }

        private void AgregarLectorInterno()
        {
            int numero  = Interlocked.Increment(ref _contadorMeseros);
            var mesero  = new Mesero($"Mesero-{numero}");
            var worker  = new LectorWorker(
                mesero, _rwLock, _menuRepo,
                _attackService, _logger,
                _estadoSimulacion, _eventoPausa,
                _velocidadLectorMs);

            worker.LecturaCompletada += (_, _) =>
                ActualizacionDisponible?.Invoke(this, EventArgs.Empty);

            var task = Task.Run(
                () => worker.EjecutarAsync(_cts.Token),
                _cts.Token);

            lock (_lockWorkers)
            {
                _meseros.Add(mesero);
                _lectorWorkers.Add(worker);
                _lectorTasks.Add(task);
            }
        }

        private void CambiarEstado(SimulationStatus nuevoEstado)
        {
            _estadoSimulacion.Estado = nuevoEstado;
            EstadoCambiado?.Invoke(this, nuevoEstado);
        }

        private void TryForzarLiberacionLock()
        {
            try
            {
                if (_rwLock.IsWriteLockHeld) _rwLock.ExitWriteLock();
                if (_rwLock.IsReadLockHeld)  _rwLock.ExitReadLock();
            }
            catch { /* ignorar: el lock puede no estar inicializado */ }
        }

        public void Dispose()
        {
            if (_estadoSimulacion.Estado != SimulationStatus.Detenida)
                Detener();

            _rwLock?.Dispose();
            _eventoPausa?.Dispose();
            _cts?.Dispose();
        }
    }
}