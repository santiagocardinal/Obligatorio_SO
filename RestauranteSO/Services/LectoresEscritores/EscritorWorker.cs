// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Services/LectoresEscritores/EscritorWorker.cs
// Propósito: Hilo Escritor. Simula al Gerente modificando el menú.
//            Solo puede haber UN escritor activo y NINGÚN lector simultáneo.
// SOLID    : SRP, DIP.
// Thread   : Task (consistente con los lectores en esta simulación).
// =============================================================================

using RestauranteSO.Constants;
using RestauranteSO.Domain.Entities;
using RestauranteSO.Domain.Enums;
using RestauranteSO.Domain.Interfaces;
using RestauranteSO.Domain.Models;

namespace RestauranteSO.Services.LectoresEscritores
{
    /// <summary>
    /// Worker Escritor para la simulación Lectores-Escritores.
    ///
    /// FLUJO DE EJECUCIÓN:
    ///   1. Esperar intervalo largo (el gerente no modifica el menú constantemente).
    ///   2. Esperar si pausado.
    ///   3. Adquirir WriteLock (EnterWriteLock()).
    ///      → BLOQUEA hasta que TODOS los lectores liberen ReadLock.
    ///      → Una vez adquirido, NINGÚN lector puede adquirir ReadLock.
    ///   4. Modificar un item del menú.
    ///   5. Liberar WriteLock (ExitWriteLock()).
    ///      → Desbloquea todos los lectores que estaban esperando.
    ///   6. Notificar a la UI.
    ///   7. Volver al paso 1.
    ///
    /// STARVATION:
    ///   ReaderWriterLockSlim en .NET usa un mecanismo de fairness que evita
    ///   que los escritores esperen indefinidamente cuando hay muchos lectores.
    ///   Si hay escritores esperando, nuevos lectores deben esperar también.
    /// </summary>
    public sealed class EscritorWorker
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
            _logger.Log(
                $"🟢 Escritor iniciado: {_gerente} (Gerente del restaurante)",
                LogLevel.Sync,
                _gerente);

            while (!token.IsCancellationRequested)
            {
                try
                {
                    // Esperar el intervalo entre modificaciones
                    await Task.Delay(_velocidadMs, token);

                    _eventoPausa.Wait(200, token);
                    if (token.IsCancellationRequested) break;

                    // ── Preparar modificación ─────────────────────────────────
                    EstaEsperando  = true;
                    EstaEscribiendo = false;

                    _logger.Log(
                        $"✏ {_gerente} esperando WriteLock " +
                        $"[Lectores activos={_rwLock.CurrentReadCount}, " +
                        $"EscritorEsperando={_rwLock.WaitingWriteCount > 0}]",
                        LogLevel.Sync,
                        _gerente);

                    // ── Adquirir WriteLock ────────────────────────────────────
                    // EnterWriteLock() bloquea hasta que:
                    //   1. Todos los ReadLocks activos sean liberados.
                    //   2. No haya otros escritores.
                    // Durante este bloqueo, NINGÚN nuevo ReadLock es concedido.
                    // Esto garantiza exclusión mutua total durante la escritura.
                    _rwLock.EnterWriteLock();

                    try
                    {
                        EstaEsperando   = false;
                        EstaEscribiendo = true;

                        _logger.Log(
                            $"🔐 {_gerente} adquirió WriteLock EXCLUSIVO " +
                            $"[Lectores bloqueados, solo escritor activo]",
                            LogLevel.Sync,
                            _gerente);

                        // ── Modificar el menú ─────────────────────────────────
                        var items = _menuRepo.ObtenerCompleto();
                        if (items.Count > 0)
                        {
                            var item = items[_random.Next(items.Count)];

                            string nombreNuevo  = item.Nombre + " (actualizado)";
                            decimal precioNuevo = item.Precio * (decimal)(0.9 + _random.NextDouble() * 0.3);

                            item.AplicarModificacion(nombreNuevo, precioNuevo, _gerente);
                            _menuRepo.Actualizar(item);

                            UltimaModificacion = $"{item.NombreOriginal} → {nombreNuevo} " +
                                                 $"${item.PrecioOriginal:N2} → ${precioNuevo:N2}";

                            _estado.IncrementarEscrituras();

                            // Simular tiempo de escritura
                            await Task.Delay(
                                AppConstants.VelocidadEscritorBaseMs, token);

                            _logger.Log(
                                $"✅ {_gerente} modificó menú: {UltimaModificacion}",
                                LogLevel.Info,
                                _gerente);

                            ModificacionRealizada?.Invoke(this, UltimaModificacion);
                        }
                    }
                    finally
                    {
                        // ── Liberar WriteLock SIEMPRE ─────────────────────────
                        // CRITICAL: ExitWriteLock() en finally.
                        // Si no se libera, todos los lectores quedan bloqueados
                        // permanentemente (deadlock).
                        _rwLock.ExitWriteLock();
                        EstaEscribiendo = false;

                        _logger.Log(
                            $"🔓 {_gerente} liberó WriteLock " +
                            $"[Lectores pueden continuar]",
                            LogLevel.Sync,
                            _gerente);
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (_rwLock.IsWriteLockHeld)
                        _rwLock.ExitWriteLock();

                    EstaEscribiendo = false;
                    EstaEsperando   = false;

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
    }
}