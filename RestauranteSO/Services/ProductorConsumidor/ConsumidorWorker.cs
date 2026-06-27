// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Services/ProductorConsumidor/ConsumidorWorker.cs
// Propósito: Hilo Consumidor. Representa un cocinero que toma pedidos
//            de la cola compartida y los prepara.
// SOLID    : SRP - solo consume/prepara pedidos.
//            DIP - dependencias inyectadas.
// Thread   : Thread dedicado, IsBackground = true.
// =============================================================================

using System.Collections.Concurrent;
using RestauranteSO.Constants;
using RestauranteSO.Domain.Entities;
using RestauranteSO.Domain.Enums;
using RestauranteSO.Domain.Interfaces;
using RestauranteSO.Domain.Models;

namespace RestauranteSO.Services.ProductorConsumidor
{
    /// <summary>
    /// Worker Consumidor: simula un cocinero del restaurante.
    ///
    /// FLUJO DE EJECUCIÓN:
    ///   1. Esperar si la simulación está pausada.
    ///   2. Adquirir un item de la cola (_semItems.Wait).
    ///      → Si la cola está vacía, el hilo BLOQUEA aquí.
    ///   3. Desencolar el pedido (ConcurrentQueue.TryDequeue).
    ///   4. Marcar el pedido como "En Preparación".
    ///   5. Simular tiempo de preparación (Thread.Sleep con progreso).
    ///   6. Marcar pedido como "Listo" → "Entregado".
    ///   7. Liberar un espacio en la cola (_semEspacios.Release).
    ///   8. Notificar a la UI.
    ///   9. Volver al paso 1.
    /// </summary>
    public sealed class ConsumidorWorker
    {
        // ─── DEPENDENCIAS ─────────────────────────────────────────────────────

        private readonly Cocinero _cocinero;
        private readonly ConcurrentQueue<Pedido> _cola;
        private readonly SemaphoreSlim _semEspacios;
        private readonly SemaphoreSlim _semItems;
        private readonly ManualResetEventSlim _eventoPausa;
        private readonly ISimulationLogger _logger;
        private readonly SimulationState _estado;
        private readonly IPedidoRepository _pedidoRepo;

        // ─── CONFIGURACIÓN ────────────────────────────────────────────────────

        private volatile int _velocidadMs;
        private readonly Random _random;
        private Thread? _hilo;

        // ─── EVENTOS ─────────────────────────────────────────────────────────

        /// <summary>Disparado cuando el cocinero toma un pedido de la cola.</summary>
        public event EventHandler<Pedido>? PedidoIniciado;

        /// <summary>Disparado cuando el cocinero termina de preparar un pedido.</summary>
        public event EventHandler<Pedido>? PedidoCompletado;

        /// <summary>Disparado durante la preparación para actualizar la ProgressBar.</summary>
        public event EventHandler<(Pedido pedido, int progreso)>? ProgresoActualizado;

        // ─── CONSTRUCTOR ─────────────────────────────────────────────────────

        public ConsumidorWorker(
            Cocinero cocinero,
            ConcurrentQueue<Pedido> cola,
            SemaphoreSlim semEspacios,
            SemaphoreSlim semItems,
            ManualResetEventSlim eventoPausa,
            ISimulationLogger logger,
            SimulationState estado,
            IPedidoRepository pedidoRepo,
            int velocidadMs = AppConstants.VelocidadConsumidorBaseMs)
        {
            _cocinero    = cocinero  ?? throw new ArgumentNullException(nameof(cocinero));
            _cola        = cola      ?? throw new ArgumentNullException(nameof(cola));
            _semEspacios = semEspacios ?? throw new ArgumentNullException(nameof(semEspacios));
            _semItems    = semItems  ?? throw new ArgumentNullException(nameof(semItems));
            _eventoPausa = eventoPausa ?? throw new ArgumentNullException(nameof(eventoPausa));
            _logger      = logger   ?? throw new ArgumentNullException(nameof(logger));
            _estado      = estado   ?? throw new ArgumentNullException(nameof(estado));
            _pedidoRepo  = pedidoRepo ?? throw new ArgumentNullException(nameof(pedidoRepo));
            _velocidadMs = velocidadMs;
            _random      = new Random(cocinero.Id.GetHashCode());
        }

        // ─── MÉTODOS PÚBLICOS ─────────────────────────────────────────────────

        public void Iniciar(CancellationToken token)
        {
            _hilo = new Thread(() => Ejecutar(token))
            {
                Name         = _cocinero.Id,
                IsBackground = true,
                Priority     = ThreadPriority.Normal
            };

            _cocinero.EstaActivo = true;
            _cocinero.Estado     = SimulationStatus.Corriendo;
            _hilo.Start();

            _logger.Log(
                $"🟢 Hilo iniciado: {_cocinero.Id} [{_cocinero.Especialidad}]",
                LogLevel.Sync,
                _cocinero.Id);
        }

        public void AjustarVelocidad(int velocidadMs)
            => _velocidadMs = Math.Max(100, velocidadMs);

        public Thread? ObtenerHilo() => _hilo;

        // ─── LÓGICA PRINCIPAL ─────────────────────────────────────────────────

        private void Ejecutar(CancellationToken token)
        {
            _logger.Log(
                $"Cocinero {_cocinero.Id} listo en cocina [{_cocinero.Especialidad}]",
                LogLevel.Info,
                _cocinero.Id);

            while (!token.IsCancellationRequested)
            {
                try
                {
                    // ── PASO 1: Respetar pausa ────────────────────────────────
                    _eventoPausa.Wait(200, token);
                    if (token.IsCancellationRequested) break;

                    // ── PASO 2: Esperar que haya un item disponible ───────────
                    // _semItems.Wait(token): decrementa el semáforo de items.
                    // Si CurrentCount == 0 (cola vacía):
                    //   → El hilo BLOQUEA hasta que un productor haga Release().
                    // Sin esta espera, haríamos polling activo (busy-wait),
                    // que desperdiciaría CPU chequeando repetidamente si hay items.
                    _semItems.Wait(token);
                    if (token.IsCancellationRequested) break;

                    // ── PASO 3: Desencolar pedido ─────────────────────────────
                    // TryDequeue debería SIEMPRE tener éxito aquí porque
                    // acabamos de adquirir el semáforo (que garantiza que hay item).
                    // El if es defensa extra contra condiciones de carrera edge-case.
                    if (!_cola.TryDequeue(out Pedido? pedido) || pedido == null)
                    {
                        // No debería pasar, pero si pasa, liberamos el espacio
                        _semEspacios.Release();
                        continue;
                    }

                    // ── PASO 4: Verificar si el pedido fue cancelado/eliminado ─
                    // Un pedido Cancelado por el ataque no debe prepararse
                    if (pedido.Estado == PedidoEstado.Cancelado)
                    {
                        _semEspacios.Release();
                        _logger.Log(
                            $"⚡ {_cocinero.Id}: Pedido #{pedido.NumeroPedido:D3} " +
                            "cancelado/eliminado, descartando.",
                            LogLevel.Attack,
                            _cocinero.Id);
                        continue;
                    }

                    // ── PASO 5: Iniciar preparación ───────────────────────────
                    pedido.Estado                = PedidoEstado.EnPreparacion;
                    pedido.FechaInicioPreparacion = DateTime.Now;
                    pedido.CocineroAsignado      = _cocinero.Id;
                    _cocinero.PedidoActual       = pedido;
                    _cocinero.ProgresoActual     = 0;

                    PedidoIniciado?.Invoke(this, pedido);

                    _logger.Log(
                        $"🔵 {_cocinero.Id} tomó Pedido #{pedido.NumeroPedido:D3} " +
                        $"'{pedido.Descripcion.Substring(0, Math.Min(20, pedido.Descripcion.Length))}' " +
                        $"| Sem[items]={_semItems.CurrentCount}",
                        pedido.FueAlterado ? LogLevel.Attack : LogLevel.Sync,
                        _cocinero.Id);

                    // ── PASO 6: Simular preparación con progreso ──────────────
                    PrepararPedidoConProgreso(pedido, token);

                    if (token.IsCancellationRequested) break;

                    // ── PASO 7: Finalizar pedido ──────────────────────────────
                    pedido.FechaFinPreparacion = DateTime.Now;
                    pedido.Estado              = PedidoEstado.Listo;
                    _cocinero.ProgresoActual   = 100;

                    // Pequeña pausa simulando que el mesero recoge el plato
                    Thread.Sleep(300);
                    pedido.Estado = PedidoEstado.Entregado;

                    _cocinero.PedidosPreparados++;
                    _cocinero.PedidoActual   = null;
                    _cocinero.ProgresoActual = 0;
                    _estado.IncrementarPedidosCompletados();

                    // ── PASO 8: Liberar espacio en la cola ────────────────────
                    // _semEspacios.Release(): señala que hay un espacio libre.
                    // Esto desbloquea un productor que esté esperando en
                    // _semEspacios.Wait() porque la cola estaba llena.
                    _semEspacios.Release();

                    PedidoCompletado?.Invoke(this, pedido);

                    _logger.Log(
                        $"✅ {_cocinero.Id} completó Pedido #{pedido.NumeroPedido:D3} " +
                        $"en {pedido.TiempoPreparacion.TotalSeconds:F1}s " +
                        $"| Sem[espacios]={_semEspacios.CurrentCount}",
                        LogLevel.Info,
                        _cocinero.Id);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.Log(
                        $"Error en {_cocinero.Id}: {ex.Message}",
                        LogLevel.Error,
                        _cocinero.Id);
                    Thread.Sleep(500);
                }
            }

            // Limpieza final
            _cocinero.EstaActivo     = false;
            _cocinero.PedidoActual   = null;
            _cocinero.ProgresoActual = 0;
            _cocinero.Estado         = SimulationStatus.Detenida;

            _logger.Log(
                $"🔴 Hilo finalizado: {_cocinero.Id} " +
                $"({_cocinero.PedidosPreparados} pedidos preparados)",
                LogLevel.Sync,
                _cocinero.Id);
        }

        // ─── MÉTODOS PRIVADOS ─────────────────────────────────────────────────

        /// <summary>
        /// Simula la preparación del pedido en pasos de 10% con actualizaciones
        /// de progreso para la ProgressBar de la UI.
        /// El tiempo total es pedido.TiempoPreparacionMs (afectado por velocidad).
        /// </summary>
        private void PrepararPedidoConProgreso(Pedido pedido, CancellationToken token)
        {
            // El tiempo real se escala según la velocidad configurada
            // _velocidadMs actúa como divisor: mayor velocidad = menos tiempo
            double factorVelocidad = AppConstants.VelocidadConsumidorBaseMs / (double)_velocidadMs;
            int tiempoTotal = (int)(pedido.TiempoPreparacionMs / factorVelocidad);
            int pasos = 10;
            int tiempoPorPaso = Math.Max(50, tiempoTotal / pasos);

            for (int i = 1; i <= pasos && !token.IsCancellationRequested; i++)
            {
                // Respetar pausa durante la preparación también
                _eventoPausa.Wait(100, token);
                if (token.IsCancellationRequested) break;

                Thread.Sleep(tiempoPorPaso);

                int progreso = i * 10;
                _cocinero.ProgresoActual = progreso;

                ProgresoActualizado?.Invoke(this, (pedido, progreso));
            }
        }
    }
}