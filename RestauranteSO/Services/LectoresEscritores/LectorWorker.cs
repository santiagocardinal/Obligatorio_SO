// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Services/LectoresEscritores/LectorWorker.cs
// Propósito: Hilo Lector. Simula un mesero que lee el menú constantemente.
//            Múltiples lectores pueden estar leyendo simultáneamente.
//            Solo bloquean cuando el Gerente (escritor) está escribiendo.
// SOLID    : SRP, DIP.
// Thread   : Task (no Thread) porque son operaciones más livianas y
//            necesitamos soportar muchos lectores simultáneos.
//            ¿Por qué Task aquí y Thread en Productor-Consumidor?
//            - En PC necesitamos Thread.Name visible y control de ciclo de vida.
//            - En LE los lectores son más numerosos y de menor peso;
//              Task usa el ThreadPool eficientemente.
// =============================================================================

using RestauranteSO.Constants;
using RestauranteSO.Domain.Entities;
using RestauranteSO.Domain.Enums;
using RestauranteSO.Domain.Interfaces;
using RestauranteSO.Domain.Models;

namespace RestauranteSO.Services.LectoresEscritores
{
    /// <summary>
    /// Worker Lector para la simulación Lectores-Escritores.
    ///
    /// FLUJO DE EJECUCIÓN:
    ///   1. Esperar si pausado.
    ///   2. Marcar como "esperando ReadLock".
    ///   3. Adquirir ReadLock (ReaderWriterLockSlim.EnterReadLock()).
    ///      → Si un escritor tiene WriteLock, BLOQUEA aquí.
    ///      → Si no hay escritor, pasa inmediatamente.
    ///      → N lectores pueden tener ReadLock simultáneamente.
    ///   4. Leer el menú (simular tiempo de lectura).
    ///   5. Si ataque activo: detectar si la info es comprometida.
    ///   6. Liberar ReadLock (rwLock.ExitReadLock()).
    ///   7. Notificar a la UI.
    ///   8. Dormir intervalo de lectura.
    ///   9. Volver al paso 1.
    /// </summary>
    public sealed class LectorWorker
    {
        // ─── DEPENDENCIAS ─────────────────────────────────────────────────────

        private readonly Mesero _mesero;
        private readonly ReaderWriterLockSlim _rwLock;
        private readonly IMenuRepository _menuRepo;
        private readonly IAttackService _attackService;
        private readonly ISimulationLogger _logger;
        private readonly SimulationState _estado;
        private readonly ManualResetEventSlim _eventoPausa;

        private volatile int _velocidadMs;
        private readonly Random _random;

        // ─── EVENTOS ─────────────────────────────────────────────────────────

        public event EventHandler<Mesero>? LecturaCompletada;
        public event EventHandler<Mesero>? LecturaIniciada;

        // ─── CONSTRUCTOR ─────────────────────────────────────────────────────

        public LectorWorker(
            Mesero mesero,
            ReaderWriterLockSlim rwLock,
            IMenuRepository menuRepo,
            IAttackService attackService,
            ISimulationLogger logger,
            SimulationState estado,
            ManualResetEventSlim eventoPausa,
            int velocidadMs = AppConstants.VelocidadLectorBaseMs)
        {
            _mesero        = mesero        ?? throw new ArgumentNullException(nameof(mesero));
            _rwLock        = rwLock        ?? throw new ArgumentNullException(nameof(rwLock));
            _menuRepo      = menuRepo      ?? throw new ArgumentNullException(nameof(menuRepo));
            _attackService = attackService ?? throw new ArgumentNullException(nameof(attackService));
            _logger        = logger        ?? throw new ArgumentNullException(nameof(logger));
            _estado        = estado        ?? throw new ArgumentNullException(nameof(estado));
            _eventoPausa   = eventoPausa   ?? throw new ArgumentNullException(nameof(eventoPausa));
            _velocidadMs   = velocidadMs;
            _random        = new Random(mesero.Id.GetHashCode());
        }

        public void AjustarVelocidad(int velocidadMs)
            => _velocidadMs = Math.Max(100, velocidadMs);

        // ─── MÉTODO PRINCIPAL ─────────────────────────────────────────────────

        /// <summary>
        /// Método async que corre en el ThreadPool via Task.Run().
        /// Cada iteración es una "sesión de lectura del menú".
        /// </summary>
        public async Task EjecutarAsync(CancellationToken token)
        {
            _mesero.EstaActivo = true;

            _logger.Log(
                $"🟢 Lector iniciado: {_mesero.Id}",
                LogLevel.Sync,
                _mesero.Id);

            while (!token.IsCancellationRequested)
            {
                try
                {
                    // ── Pausa ─────────────────────────────────────────────────
                    _eventoPausa.Wait(200, token);
                    if (token.IsCancellationRequested) break;

                    // ── Marcar como esperando ─────────────────────────────────
                    _mesero.EstaEsperando  = true;
                    _mesero.EstaLeyendo    = false;
                    _mesero.InicioUltimaLectura = DateTime.Now;

                    LecturaIniciada?.Invoke(this, _mesero);

                    _logger.Log(
                        $"📖 {_mesero.Id} esperando ReadLock " +
                        $"[Lectores={_rwLock.CurrentReadCount}, " +
                        $"EscritorActivo={_rwLock.IsWriteLockHeld}]",
                        LogLevel.Sync,
                        _mesero.Id);

                    // ── Adquirir ReadLock ─────────────────────────────────────
                    // EnterReadLock() es BLOQUEANTE si hay un escritor activo.
                    // Cuando el escritor libera con ExitWriteLock(),
                    // TODOS los lectores esperando son liberados simultáneamente.
                    // Esto garantiza que múltiples meseros puedan leer a la vez.
                    _rwLock.EnterReadLock();

                    try
                    {
                        _mesero.EstaEsperando = false;
                        _mesero.EstaLeyendo   = true;

                        _logger.Log(
                            $"🔓 {_mesero.Id} adquirió ReadLock " +
                            $"[Lectores simultáneos={_rwLock.CurrentReadCount}]",
                            LogLevel.Sync,
                            _mesero.Id);

                        // ── Leer el menú ──────────────────────────────────────
                        // Con el ReadLock adquirido, leer los items del menú.
                        // Esta operación es CONCURRENT con otros lectores.
                        var items = _menuRepo.ObtenerTodos();

                        if (items.Count > 0)
                        {
                            // Simular que el mesero lee un item aleatorio
                            var item = items[_random.Next(items.Count)];
                            _mesero.UltimoItemLeido  = $"{item.Nombre} - ${item.Precio:N2}";
                            _mesero.VersionMenuLeida = item.Version;

                            // Verificar si el item que leyó está comprometido
                            if (item.FueAlterado && _attackService.IsAttackActive)
                            {
                                _mesero.LeyoMenuComprometido = true;
                                _estado.IncrementarEventosAtaque();

                                _logger.Log(
                                    $"⚡ {_mesero.Id} leyó item COMPROMETIDO: " +
                                    $"'{item.Nombre}' (original: '{item.NombreOriginal}')",
                                    LogLevel.Attack,
                                    _mesero.Id);
                            }

                            // Simular tiempo de lectura
                            // await Task.Delay durante la lectura (dentro del ReadLock)
                            await Task.Delay(
                                _random.Next(
                                    (int)(_velocidadMs * 0.5),
                                    (int)(_velocidadMs * 1.5)),
                                token);
                        }

                        _estado.IncrementarLecturas();
                        _mesero.LecturasCompletadas++;
                    }
                    finally
                    {
                        // ── Liberar ReadLock SIEMPRE ──────────────────────────
                        // CRITICAL: ExitReadLock() DEBE estar en finally.
                        // Si no se libera, ningún escritor podrá escribir jamás
                        // (deadlock permanente).
                        _rwLock.ExitReadLock();
                        _mesero.EstaLeyendo = false;

                        _logger.Log(
                            $"🔒 {_mesero.Id} liberó ReadLock " +
                            $"[Lectores restantes={_rwLock.CurrentReadCount}]",
                            LogLevel.Sync,
                            _mesero.Id);
                    }

                    LecturaCompletada?.Invoke(this, _mesero);

                    // ── Pausa entre lecturas ──────────────────────────────────
                    await Task.Delay(_velocidadMs, token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // Si el lock estaba adquirido y hubo error, asegurar liberación
                    if (_rwLock.IsReadLockHeld)
                        _rwLock.ExitReadLock();

                    _mesero.EstaLeyendo   = false;
                    _mesero.EstaEsperando = false;

                    _logger.Log(
                        $"Error en {_mesero.Id}: {ex.Message}",
                        LogLevel.Error,
                        _mesero.Id);

                    await Task.Delay(500, token);
                }
            }

            _mesero.EstaActivo    = false;
            _mesero.EstaLeyendo   = false;
            _mesero.EstaEsperando = false;

            _logger.Log(
                $"🔴 Lector finalizado: {_mesero.Id} " +
                $"({_mesero.LecturasCompletadas} lecturas)",
                LogLevel.Sync,
                _mesero.Id);
        }
    }
}