// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Services/ProductorConsumidor/ProductorWorker.cs
// Propósito: Hilo Productor. Representa un cliente que genera pedidos
//            y los coloca en la cola compartida del restaurante.
//            Implementa la mitad "Productor" del patrón Productor-Consumidor.
// SOLID    : SRP - solo genera pedidos.
//            DIP - recibe dependencias por constructor.
// Patrón   : Strategy (el comportamiento de ataque es inyectado).
// Thread   : Corre en su propio Thread dedicado. NO es un Task.
//            ¿Por qué Thread y no Task?
//            - Necesitamos Thread.CurrentThread.Name para mostrar en la UI
//              qué hilo específico está generando cada pedido.
//            - Thread.IsBackground = true para que no bloquee el cierre
//              de la aplicación.
//            - Tasks son para operaciones async de corta duración;
//              un worker que corre indefinidamente es un Thread.
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
    /// Worker Productor: simula un cliente del restaurante generando pedidos.
    ///
    /// FLUJO DE EJECUCIÓN:
    ///   1. Esperar si la simulación está pausada (ManualResetEventSlim).
    ///   2. Chequear cancelación (CancellationToken).
    ///   3. Adquirir espacio en la cola (SemaphoreSlim _semEspacios.Wait).
    ///      → Si la cola está llena, este hilo BLOQUEA aquí.
    ///   4. Crear un Pedido nuevo.
    ///   5. Si ataque activo: posiblemente alterar/duplicar el pedido.
    ///   6. Encolar el pedido (ConcurrentQueue.Enqueue).
    ///   7. Señalar que hay un item disponible (_semItems.Release).
    ///   8. Notificar actualización a la UI.
    ///   9. Dormir el intervalo configurado (simular tiempo entre pedidos).
    ///  10. Volver al paso 1.
    /// </summary>
    public sealed class ProductorWorker
    {
        // ─── DEPENDENCIAS ─────────────────────────────────────────────────────

        private readonly Cliente _cliente;
        private readonly ConcurrentQueue<Pedido> _cola;
        private readonly SemaphoreSlim _semEspacios;
        private readonly SemaphoreSlim _semItems;
        private readonly ManualResetEventSlim _eventoPausa;
        private readonly IAttackService _attackService;
        private readonly ISimulationLogger _logger;
        private readonly SimulationState _estado;
        private readonly IPedidoRepository _pedidoRepo;

        // ─── CONFIGURACIÓN ────────────────────────────────────────────────────

        /// <summary>
        /// Intervalo entre generaciones de pedidos en milisegundos.
        /// Configurable en tiempo real desde la UI.
        /// volatile: modificado desde el UI thread, leído desde el worker thread.
        /// </summary>
        private volatile int _velocidadMs;

        // ─── ESTADO INTERNO ──────────────────────────────────────────────────

        /// <summary>Contador secuencial de pedidos para numeración visible.</summary>
        private static int _contadorGlobal = 0;

        /// <summary>Random con seed basada en el ID del cliente para variedad.</summary>
        private readonly Random _random;

        /// <summary>Referencia al Thread de este worker (asignada en Iniciar).</summary>
        private Thread? _hilo;

        // ─── EVENTO ──────────────────────────────────────────────────────────

        /// <summary>
        /// Disparado cada vez que se genera o altera un pedido.
        /// La UI se subscribe para actualizar la tabla en tiempo real.
        /// </summary>
        public event EventHandler<Pedido>? PedidoGenerado;

        // ─── CONSTRUCTOR ─────────────────────────────────────────────────────

        public ProductorWorker(
            Cliente cliente,
            ConcurrentQueue<Pedido> cola,
            SemaphoreSlim semEspacios,
            SemaphoreSlim semItems,
            ManualResetEventSlim eventoPausa,
            IAttackService attackService,
            ISimulationLogger logger,
            SimulationState estado,
            IPedidoRepository pedidoRepo,
            int velocidadMs = AppConstants.VelocidadProductorBaseMs)
        {
            _cliente      = cliente ?? throw new ArgumentNullException(nameof(cliente));
            _cola         = cola    ?? throw new ArgumentNullException(nameof(cola));
            _semEspacios  = semEspacios ?? throw new ArgumentNullException(nameof(semEspacios));
            _semItems     = semItems    ?? throw new ArgumentNullException(nameof(semItems));
            _eventoPausa  = eventoPausa ?? throw new ArgumentNullException(nameof(eventoPausa));
            _attackService= attackService ?? throw new ArgumentNullException(nameof(attackService));
            _logger       = logger  ?? throw new ArgumentNullException(nameof(logger));
            _estado       = estado  ?? throw new ArgumentNullException(nameof(estado));
            _pedidoRepo   = pedidoRepo ?? throw new ArgumentNullException(nameof(pedidoRepo));
            _velocidadMs  = velocidadMs;
            _random       = new Random(cliente.Id.GetHashCode());
        }

        // ─── MÉTODOS PÚBLICOS ─────────────────────────────────────────────────

        /// <summary>
        /// Crea e inicia el Thread de este worker.
        /// El Thread es Background para no bloquear el cierre de la app.
        /// </summary>
        public void Iniciar(CancellationToken token)
        {
            _hilo = new Thread(() => Ejecutar(token))
            {
                Name         = _cliente.Id,         // visible en el debugger y en la UI
                IsBackground = true,                 // no bloquea Application.Exit()
                Priority     = ThreadPriority.Normal
            };

            _cliente.EstaActivo = true;
            _cliente.Estado     = SimulationStatus.Corriendo;
            _hilo.Start();

            _logger.Log(
                $"🟢 Hilo iniciado: {_cliente.Id} (Mesa {_cliente.NumeroMesa})",
                LogLevel.Sync,
                _cliente.Id);
        }

        /// <summary>
        /// Ajusta la velocidad de generación de pedidos en tiempo real.
        /// Thread-safe: el campo es volatile.
        /// </summary>
        public void AjustarVelocidad(int velocidadMs)
            => _velocidadMs = Math.Max(100, velocidadMs);

        /// <summary>Retorna el Thread subyacente (puede ser null antes de Iniciar).</summary>
        public Thread? ObtenerHilo() => _hilo;

        // ─── LÓGICA PRINCIPAL ─────────────────────────────────────────────────

        /// <summary>
        /// Método principal del hilo. Corre indefinidamente hasta cancelación.
        /// </summary>
        private void Ejecutar(CancellationToken token)
        {
            _logger.Log(
                $"Productor {_cliente.Id} comenzando en Mesa {_cliente.NumeroMesa}",
                LogLevel.Info,
                _cliente.Id);

            while (!token.IsCancellationRequested)
            {
                try
                {
                    // ── PASO 1: Respetar pausa ────────────────────────────────
                    // ManualResetEventSlim.Wait(): bloquea si el evento está en
                    // Reset (pausa activa). Continúa cuando está en Set.
                    // El timeout de 200ms evita que el hilo quede bloqueado
                    // indefinidamente si se cancela durante la pausa.
                    _eventoPausa.Wait(200, token);

                    if (token.IsCancellationRequested) break;

                    // ── PASO 2: Adquirir espacio en la cola ───────────────────
                    // SemaphoreSlim.Wait(token): decrementa el semáforo.
                    // Si _semEspacios.CurrentCount == 0 (cola llena):
                    //   → Este hilo BLOQUEA aquí hasta que un consumidor
                    //     libere un espacio con _semEspacios.Release().
                    // El token permite salir del bloqueo si se cancela.
                    _semEspacios.Wait(token);

                    if (token.IsCancellationRequested) break;

                    // ── PASO 3: Crear el pedido ───────────────────────────────
                    var pedido = CrearPedido();

                    // ── PASO 4: Verificar ataque ──────────────────────────────
                    // Si el ataque está activo, posiblemente alterar el pedido
                    if (_attackService.IsAttackActive)
                        pedido = AplicarLogicaAtaque(pedido);

                    // ── PASO 5: Encolar el pedido ─────────────────────────────
                    // ConcurrentQueue.Enqueue es thread-safe y lock-free.
                    // Múltiples productores pueden llamar a Enqueue simultáneamente.
                    _cola.Enqueue(pedido);

                    // Registrar en el repositorio histórico
                    _pedidoRepo.Agregar(pedido);

                    // Actualizar contadores del estado compartido (Interlocked)
                    _estado.IncrementarPedidosGenerados();
                    _cliente.PedidosGenerados++;
                    _cliente.UltimoPedido = pedido;

                    // ── PASO 6: Señalar item disponible ───────────────────────
                    // _semItems.Release(): incrementa el semáforo de items.
                    // Desbloquea UN consumidor que esté esperando en
                    // _semItems.Wait() porque la cola estaba vacía.
                    _semItems.Release();

                    // ── PASO 7: Notificar a la UI ─────────────────────────────
                    PedidoGenerado?.Invoke(this, pedido);

                    LogearGeneracion(pedido);

                    // ── PASO 8: Dormir hasta el próximo pedido ────────────────
                    // Thread.Sleep libera el CPU durante este tiempo.
                    // El tiempo varía ±30% para hacerlo más realista.
                    int jitter = _random.Next(
                        (int)(_velocidadMs * 0.7),
                        (int)(_velocidadMs * 1.3));

                    Thread.Sleep(jitter);
                }
                catch (OperationCanceledException)
                {
                    // Cancelación limpia: salir del loop sin error
                    break;
                }
                catch (Exception ex)
                {
                    _logger.Log(
                        $"Error en {_cliente.Id}: {ex.Message}",
                        LogLevel.Error,
                        _cliente.Id);
                    Thread.Sleep(500); // evitar spin infinito en caso de error
                }
            }

            // ── FINALIZACIÓN LIMPIA ───────────────────────────────────────────
            _cliente.EstaActivo = false;
            _cliente.Estado     = SimulationStatus.Detenida;

            _logger.Log(
                $"🔴 Hilo finalizado: {_cliente.Id} " +
                $"({_cliente.PedidosGenerados} pedidos generados)",
                LogLevel.Sync,
                _cliente.Id);
        }

        // ─── MÉTODOS PRIVADOS ─────────────────────────────────────────────────

        /// <summary>
        /// Crea un nuevo pedido con datos aleatorios del menú.
        /// </summary>
        private Pedido CrearPedido()
        {
            // Número secuencial global (Interlocked para thread-safety)
            int numero = Interlocked.Increment(ref _contadorGlobal);

            // Plato aleatorio del menú
            string plato = SimulationConstants.PlatosMenu[
                _random.Next(SimulationConstants.PlatosMenu.Length)];

            // Tiempo de preparación aleatorio dentro del rango configurado
            int tiempoPrep = _random.Next(
                SimulationConstants.TiempoPreparacionMinMs,
                SimulationConstants.TiempoPreparacionMaxMs);

            return new Pedido(
                numeroPedido:       numero,
                descripcion:        plato,
                clienteId:          _cliente.Id,
                numeroMesa:         _cliente.NumeroMesa,
                tiempoPreparacionMs: tiempoPrep);
        }

        /// <summary>
        /// Aplica la lógica de ataque sobre el pedido recién creado.
        /// Según probabilidades definidas en SimulationConstants:
        ///   - 25%: duplicar el pedido (encola una copia adicional)
        ///   - 20%: eliminar el pedido (no se encola, pero se libera el semáforo)
        ///   - 55%: alterar la descripción del pedido
        /// </summary>
        private Pedido AplicarLogicaAtaque(Pedido pedido)
        {
            int dado = _random.Next(100);

            if (dado < SimulationConstants.ProbabilidadDuplicarPedido)
            {
                // DUPLICAR: crear copia fraudulenta y agregarla también
                var copia = new Pedido(
                    numeroPedido:        pedido.NumeroPedido * 100 + _random.Next(99),
                    descripcion:         pedido.Descripcion + " [DUPLICADO FRAUDULENTO]",
                    clienteId:           "⚡ ATACANTE",
                    numeroMesa:          pedido.NumeroMesa,
                    tiempoPreparacionMs: pedido.TiempoPreparacionMs)
                {
                    FueDuplicado     = true,
                    FueAlterado      = true,
                    PedidoOriginalId = pedido.Id,
                    Estado           = PedidoEstado.Duplicado
                };

                // Encolar la copia adicional (si hay espacio extra en el semáforo)
                // Intentamos adquirir espacio sin bloquear (Wait con timeout=0)
                if (_semEspacios.Wait(0))
                {
                    _cola.Enqueue(copia);
                    _pedidoRepo.Agregar(copia);
                    _estado.IncrementarPedidosAlterados();
                    _semItems.Release();

                    _logger.Log(
                        $"⚡ ATAQUE: Pedido #{pedido.NumeroPedido} DUPLICADO → " +
                        $"#{copia.NumeroPedido}",
                        LogLevel.Attack,
                        "ATACANTE");
                }

                // El pedido original también se marca como alterado
                pedido.FueAlterado = true;
                pedido.Estado      = PedidoEstado.Alterado;
                _estado.IncrementarPedidosAlterados();

                // Registrar evento de ataque
                RegistrarEventoAtaque(pedido, "Pedido duplicado fraudulentamente",
                    pedido.Descripcion, copia.Descripcion);
            }
            else if (dado < SimulationConstants.ProbabilidadDuplicarPedido
                           + SimulationConstants.ProbabilidadEliminarPedido)
            {
                // ELIMINAR: marcar como cancelado, no encolar
                // Pero YA adquirimos _semEspacios.Wait() antes de llamar aquí,
                // así que debemos liberar el espacio que no usamos.
                pedido.Estado     = PedidoEstado.Cancelado;
                pedido.FueAlterado = true;
                _estado.IncrementarPedidosCancelados();
                _estado.IncrementarPedidosAlterados();

                // Liberar el espacio que habíamos reservado (ya no lo usamos)
                _semEspacios.Release();

                // Liberar también el semáforo de items porque NO vamos a encolarlo
                // pero el llamador lo va a hacer igualmente → compensar
                // (nota: el llamador hace Release después; aquí marcamos para que
                //  el llamador sepa que no debe encolarlo)
                // Usamos un flag especial: retornamos null e indicamos cancelación
                // Solución: retornar el pedido con estado Cancelado;
                // el llamador lo detecta y no hace Release de _semItems
                _logger.Log(
                    $"⚡ ATAQUE: Pedido #{pedido.NumeroPedido} ELIMINADO de la cola",
                    LogLevel.Attack,
                    "ATACANTE");

                RegistrarEventoAtaque(pedido, "Pedido eliminado de la cola",
                    pedido.Descripcion, "(eliminado)");
            }
            else
            {
                // ALTERAR: cambiar la descripción por un plato incorrecto
                string descripcionOriginal = pedido.Descripcion;
                string descripcionFalsa = SimulationConstants.PlatosAtaque[
                    _random.Next(SimulationConstants.PlatosAtaque.Length)];

                pedido.Descripcion = descripcionFalsa;
                pedido.FueAlterado = true;
                pedido.Estado      = PedidoEstado.Alterado;
                _estado.IncrementarPedidosAlterados();

                _logger.Log(
                    $"⚡ ATAQUE: Pedido #{pedido.NumeroPedido} ALTERADO: " +
                    $"'{descripcionOriginal}' → '{descripcionFalsa}'",
                    LogLevel.Attack,
                    "ATACANTE");

                RegistrarEventoAtaque(pedido,
                    "Descripción del pedido alterada",
                    descripcionOriginal,
                    descripcionFalsa);
            }

            return pedido;
        }

        /// <summary>
        /// Registra un evento de ataque en el servicio de ataques.
        /// </summary>
        private void RegistrarEventoAtaque(
            Pedido pedido, string descripcion,
            string valorOriginal, string valorAlterado)
        {
            var evento = new AttackEvent
            {
                TipoAtaque         = AttackType.InyeccionDePedidos,
                Descripcion        = $"#{pedido.NumeroPedido}: {descripcion}",
                ComponenteAfectado = "Cola de Pedidos",
                FuenteAtaque       = Thread.CurrentThread.Name ?? "Desconocido",
                ValorOriginal      = valorOriginal,
                ValorAlterado      = valorAlterado,
                ImpactoNegocio     = "Cliente recibe plato incorrecto o inexistente"
            };

            _estado.IncrementarEventosAtaque();
        }

        /// <summary>
        /// Registra en el logger la generación del pedido con detalles de sincronización.
        /// </summary>
        private void LogearGeneracion(Pedido pedido)
        {
            var nivel = pedido.FueAlterado ? LogLevel.Attack : LogLevel.Info;
            var prefijo = pedido.FueAlterado ? "⚡" : "🟡";

            _logger.Log(
                $"{prefijo} {_cliente.Id} generó Pedido #{pedido.NumeroPedido:D3} " +
                $"'{pedido.Descripcion.Substring(0, Math.Min(25, pedido.Descripcion.Length))}...' " +
                $"| Cola: {_cola.Count}/{AppConstants.CapacidadMaximaCola} " +
                $"| Sem[espacios]={_semEspacios.CurrentCount}",
                nivel,
                _cliente.Id);
        }
    }
}