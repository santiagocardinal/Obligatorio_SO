using RestauranteSO.Constants;
using RestauranteSO.Domain.Entities;
using RestauranteSO.Domain.Enums;
using RestauranteSO.Domain.Interfaces;
using RestauranteSO.Domain.Models;

namespace RestauranteSO.Services.LectoresEscritores
{
    public sealed class LectorWorker : IDisposable
    {
        private readonly Mesero _mesero;
        private readonly ReaderWriterLockSlim _rwLock;
        private readonly IMenuRepository _menuRepo;
        private readonly IAttackService _attackService;
        private readonly ISimulationLogger _logger;
        private readonly SimulationState _estado;
        private readonly ManualResetEventSlim _eventoPausa;

        private volatile int _velocidadMs;
        private readonly Random _random;
        private bool _disposed = false;

        public event EventHandler<Mesero>? LecturaCompletada;
        public event EventHandler<Mesero>? LecturaIniciada;

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

        public async Task EjecutarAsync(CancellationToken token)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(LectorWorker));

            _mesero.EstaActivo = true;

            _logger.Log($"🟢 Lector iniciado: {_mesero.Id}", LogLevel.Sync, _mesero.Id);

            while (!token.IsCancellationRequested && !_disposed)
            {
                try
                {
                    if (!_eventoPausa.Wait(100, token)) continue;
                    if (token.IsCancellationRequested) break;

                    _mesero.EstaEsperando = true;
                    _mesero.EstaLeyendo = false;
                    _mesero.InicioUltimaLectura = DateTime.Now;

                    LecturaIniciada?.Invoke(this, _mesero);

                    _logger.Log(
                        $"📖 {_mesero.Id} esperando ReadLock [Lectores={_rwLock.CurrentReadCount}, EscritorActivo={_rwLock.IsWriteLockHeld}]",
                        LogLevel.Sync, _mesero.Id);

                    // 🔴 VERIFICACIÓN: si el escritor tiene el WriteLock, mostramos la X
                    if (_rwLock.IsWriteLockHeld)
                    {
                        _logger.Log(
                            $"❌ {_mesero.Id} NO puede acceder al menú — Escritor activo (WriteLock exclusivo)",
                            LogLevel.Warning, _mesero.Id);
                        _mesero.EstaBloqueadoPorEscritor = true;
                    }
                    else
                    {
                        _mesero.EstaBloqueadoPorEscritor = false;
                    }

                    bool lockAdquirido = false;
                    try
                    {
                        _rwLock.EnterReadLock();
                        lockAdquirido = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.Log($"Error al adquirir ReadLock en {_mesero.Id}: {ex.Message}", LogLevel.Error, _mesero.Id);
                        await Task.Delay(500, token);
                        continue;
                    }

                    if (lockAdquirido)
                    {
                        try
                        {
                            _mesero.EstaEsperando = false;
                            _mesero.EstaLeyendo = true;
                            _mesero.EstaBloqueadoPorEscritor = false;

                            _logger.Log(
                                $"🔓 {_mesero.Id} adquirió ReadLock [Lectores simultáneos={_rwLock.CurrentReadCount}]",
                                LogLevel.Sync, _mesero.Id);

                            var items = _menuRepo.ObtenerTodos();

                            if (items.Count > 0)
                            {
                                var item = items[_random.Next(items.Count)];
                                _mesero.UltimoItemLeido = $"{item.Nombre} - ${item.Precio:N2}";
                                _mesero.VersionMenuLeida = item.Version;

                                if (item.FueAlterado && _attackService.IsAttackActive)
                                {
                                    _mesero.LeyoMenuComprometido = true;
                                    _estado.IncrementarEventosAtaque();
                                    _logger.Log(
                                        $"⚡ {_mesero.Id} leyó item COMPROMETIDO: '{item.Nombre}' (original: '{item.NombreOriginal}')",
                                        LogLevel.Attack, _mesero.Id);
                                }

                                _estado.IncrementarLecturas();
                                _mesero.LecturasCompletadas++;
                            }
                        }
                        finally
                        {
                            if (lockAdquirido)
                            {
                                _rwLock.ExitReadLock();
                                _mesero.EstaLeyendo = false;
                                _logger.Log(
                                    $"🔒 {_mesero.Id} liberó ReadLock [Lectores restantes={_rwLock.CurrentReadCount}]",
                                    LogLevel.Sync, _mesero.Id);
                            }
                        }
                    }

                    LecturaCompletada?.Invoke(this, _mesero);

                    await Task.Delay(_velocidadMs, token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.Log($"Error inesperado en {_mesero.Id}: {ex.Message}", LogLevel.Error, _mesero.Id);
                    await Task.Delay(1000, token);
                }
            }

            _mesero.EstaActivo = false;
            _mesero.EstaLeyendo = false;
            _mesero.EstaEsperando = false;
            _mesero.EstaBloqueadoPorEscritor = false;

            _logger.Log($"🔴 Lector finalizado: {_mesero.Id} ({_mesero.LecturasCompletadas} lecturas)", LogLevel.Sync, _mesero.Id);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
        }
    }
}