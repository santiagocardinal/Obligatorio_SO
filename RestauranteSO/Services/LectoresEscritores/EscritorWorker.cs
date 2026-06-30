using RestauranteSO.Constants;
using RestauranteSO.Domain.Entities;
using RestauranteSO.Domain.Enums;
using RestauranteSO.Domain.Interfaces;
using RestauranteSO.Domain.Models;

namespace RestauranteSO.Services.LectoresEscritores
{
    public sealed class EscritorWorker : IDisposable
    {
        // ─── DEPENDENCIAS ─────────────────────────────────────────────────────

        private readonly string _gerente;
        private readonly ReaderWriterLockSlim _rwLock;
        private readonly IMenuRepository _menuRepo;
        private readonly IAttackService _attackService;
        private readonly ISimulationLogger _logger;
        private readonly SimulationState _estado;
        private readonly ManualResetEventSlim _eventoPausa;

        private volatile int _velocidadMs;
        private readonly Random _random;
        private bool _disposed = false;

        // ─── ESTADO OBSERVABLE ────────────────────────────────────────────────

        public bool EstaEscribiendo { get; private set; }
        public bool EstaEsperando   { get; private set; }
        public string? UltimaModificacion { get; private set; }

        // ─── EVENTO ──────────────────────────────────────────────────────────

        public event EventHandler<string>? ModificacionRealizada;

        // ─── CONSTRUCTOR ─────────────────────────────────────────────────────

        public EscritorWorker(
            string gerente,
            ReaderWriterLockSlim rwLock,
            IMenuRepository menuRepo,
            IAttackService attackService,
            ISimulationLogger logger,
            SimulationState estado,
            ManualResetEventSlim eventoPausa,
            int velocidadMs = AppConstants.IntervaloModificacionGerenteMs)
        {
            _gerente       = gerente       ?? throw new ArgumentNullException(nameof(gerente));
            _rwLock        = rwLock        ?? throw new ArgumentNullException(nameof(rwLock));
            _menuRepo      = menuRepo      ?? throw new ArgumentNullException(nameof(menuRepo));
            _attackService = attackService ?? throw new ArgumentNullException(nameof(attackService));
            _logger        = logger        ?? throw new ArgumentNullException(nameof(logger));
            _estado        = estado        ?? throw new ArgumentNullException(nameof(estado));
            _eventoPausa   = eventoPausa   ?? throw new ArgumentNullException(nameof(eventoPausa));
            _velocidadMs   = velocidadMs;
            _random        = new Random();
        }

        public void AjustarVelocidad(int velocidadMs)
            => _velocidadMs = Math.Max(500, velocidadMs);

        // ─── MÉTODO PRINCIPAL ─────────────────────────────────────────────────

        public async Task EjecutarAsync(CancellationToken token)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(EscritorWorker));

            _logger.Log(
                $"🟢 Escritor iniciado: {_gerente} (Gerente del restaurante)",
                LogLevel.Sync,
                _gerente);

            while (!token.IsCancellationRequested && !_disposed)
            {
                try
                {
                    // Esperar el intervalo entre modificaciones (FUERA del lock)
                    await Task.Delay(_velocidadMs, token);

                    // Verificar pausa y cancelación
                    if (!_eventoPausa.Wait(200, token))
                        continue;
                    if (token.IsCancellationRequested)
                        break;

                    // ── Preparar escritura ────────────────────────────────────
                    EstaEsperando  = true;
                    EstaEscribiendo = false;

                    _logger.Log(
                        $"✏ {_gerente} esperando WriteLock [Lectores activos={_rwLock.CurrentReadCount}, EscritorEsperando={_rwLock.WaitingWriteCount > 0}]",
                        LogLevel.Sync,
                        _gerente);

                    // ── Adquirir WriteLock ────────────────────────────────────
                    bool lockAdquirido = false;
                    try
                    {
                        _rwLock.EnterWriteLock();
                        lockAdquirido = true;
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(
                            $"Error al adquirir WriteLock en {_gerente}: {ex.Message}",
                            LogLevel.Error,
                            _gerente);
                        await Task.Delay(1000, token);
                        continue;
                    }

                    if (lockAdquirido)
                    {
                        try
                        {
                            EstaEsperando   = false;
                            EstaEscribiendo = true;

                            _logger.Log(
                                $"🔐 {_gerente} adquirió WriteLock EXCLUSIVO [Lectores bloqueados, solo escritor activo]",
                                LogLevel.Sync,
                                _gerente);

                            // ── Modificar el menú (operación rápida) ──────────
                            var items = _menuRepo.ObtenerCompleto();
                            if (items.Count > 0)
                            {
                                var item = items[_random.Next(items.Count)];

                                string nombreNuevo  = item.Nombre + " (actualizado)";
                                decimal precioNuevo = item.Precio * (decimal)(0.9 + _random.NextDouble() * 0.3);

                                item.AplicarModificacion(nombreNuevo, precioNuevo, _gerente);
                                _menuRepo.Actualizar(item);

                                UltimaModificacion = $"{item.NombreOriginal} → {nombreNuevo} ${item.PrecioOriginal:N2} → ${precioNuevo:N2}";
                                _estado.IncrementarEscrituras();

                                _logger.Log(
                                    $"✅ {_gerente} modificó menú: {UltimaModificacion}",
                                    LogLevel.Info,
                                    _gerente);

                                ModificacionRealizada?.Invoke(this, UltimaModificacion);
                            }
                        }
                        finally
                        {
                            // ── Liberar WriteLock SIEMPRE ─────────────────────
                            if (lockAdquirido)
                            {
                                _rwLock.ExitWriteLock();
                                EstaEscribiendo = false;

                                _logger.Log(
                                    $"🔓 {_gerente} liberó WriteLock [Lectores pueden continuar]",
                                    LogLevel.Sync,
                                    _gerente);
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // Si ocurre un error inesperado y el lock está tomado, liberarlo
                    if (_rwLock.IsWriteLockHeld)
                    {
                        try { _rwLock.ExitWriteLock(); } catch { }
                        EstaEscribiendo = false;
                    }
                    EstaEsperando = false;

                    _logger.Log(
                        $"Error en {_gerente}: {ex.Message}",
                        LogLevel.Error,
                        _gerente);

                    await Task.Delay(1000, token);
                }
            }

            EstaEscribiendo = false;
            EstaEsperando   = false;

            _logger.Log(
                $"🔴 Escritor finalizado: {_gerente}",
                LogLevel.Sync,
                _gerente);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
        }
    }
}