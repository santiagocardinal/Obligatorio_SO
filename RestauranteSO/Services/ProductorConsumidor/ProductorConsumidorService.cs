// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Services/ProductorConsumidor/ProductorConsumidorService.cs
// Propósito: Orquestador principal de la simulación Productor-Consumidor.
//            Crea, inicia, pausa, reanuda y detiene todos los workers.
//            Es el punto de entrada que usa la UI.
// SOLID    : SRP - orquesta la simulación, no implementa lógica de workers.
//            OCP - extensible para agregar workers sin modificar.
//            DIP - depende de interfaces.
// Patrón   : Facade (simplifica la interacción con el subsistema de workers).
//            Observer (expone eventos para que la UI reaccione).
// =============================================================================

using System.Collections.Concurrent;
using RestauranteSO.Constants;
using RestauranteSO.Domain.Entities;
using RestauranteSO.Domain.Enums;
using RestauranteSO.Domain.Interfaces;
using RestauranteSO.Domain.Models;
using RestauranteSO.Infrastructure.Synchronization;

namespace RestauranteSO.Services.ProductorConsumidor
{
    /// <summary>
    /// Servicio principal de la simulación Productor-Consumidor.
    ///
    /// ARQUITECTURA DE CONCURRENCIA:
    ///
    ///   _semEspacios (SemaphoreSlim, init=capacidad):
    ///     Representa espacios libres en la cola.
    ///     Productores hacen Wait() antes de encolar.
    ///     Consumidores hacen Release() después de desencolar.
    ///
    ///   _semItems (SemaphoreSlim, init=0):
    ///     Representa items disponibles en la cola.
    ///     Productores hacen Release() después de encolar.
    ///     Consumidores hacen Wait() antes de desencolar.
    ///
    ///   _cola (ConcurrentQueue):
    ///     Buffer real de pedidos. Thread-safe por diseño.
    ///
    ///   _eventoPausa (ManualResetEventSlim, init=true):
    ///     Gate abierto = corriendo.
    ///     Gate cerrado = pausado. Todos los workers esperan aquí.
    ///
    ///   _cts (CancellationTokenSource):
    ///     Token compartido entre todos los workers.
    ///     Cancel() = señal de detención cooperativa.
    ///
    /// INVARIANTE DE SEMÁFOROS:
    ///   En todo momento: _semEspacios.Current + _semItems.Current == capacidad
    ///   (Los items en cola más los espacios libres siempre suman la capacidad)
    /// </summary>
    public sealed class ProductorConsumidorService : ISimulationService, IDisposable
    {
        // ─── DEPENDENCIAS ─────────────────────────────────────────────────────

        private readonly IAttackService _attackService;
        private readonly ISimulationLogger _logger;
        private readonly IPedidoRepository _pedidoRepo;

        // ─── PRIMITIVAS DE SINCRONIZACIÓN ────────────────────────────────────

        private SemaphoreSlim _semEspacios = null!;
        private SemaphoreSlim _semItems    = null!;
        private ManualResetEventSlim _eventoPausa = null!;
        private CancellationTokenSource _cts = null!;

        // ─── COLA COMPARTIDA ─────────────────────────────────────────────────

        private ConcurrentQueue<Pedido> _cola = null!;

        // ─── WORKERS ─────────────────────────────────────────────────────────

        private readonly List<ProductorWorker> _productores = new();
        private readonly List<ConsumidorWorker> _consumidores = new();

        // ─── ENTIDADES ───────────────────────────────────────────────────────

        private readonly List<Cliente> _clientes = new();
        private readonly List<Cocinero> _cocineros = new();

        // ─── ESTADO ──────────────────────────────────────────────────────────

        private readonly SimulationState _estadoSimulacion = new();
        private int _contadorClientes = 0;
        private int _contadorCocineros = 0;
        private volatile int _velocidadProductorMs = AppConstants.VelocidadProductorBaseMs;
        private volatile int _velocidadConsumidorMs = AppConstants.VelocidadConsumidorBaseMs;
        private DateTime _tiempoInicio;

        // ─── LOCK PARA LISTAS DE WORKERS ────────────────────────────────────
        // Las listas de workers son modificadas desde el UI thread (Agregar/Quitar)
        // pero leídas desde workers. Necesitamos protegerlas.
        private readonly object _lockWorkers = new object();

        // ─── EVENTOS ─────────────────────────────────────────────────────────

        public event EventHandler<SimulationStatus>? EstadoCambiado;
        public event EventHandler? ActualizacionDisponible;

        // ─── CONSTRUCTOR ─────────────────────────────────────────────────────

        public ProductorConsumidorService(
            IAttackService attackService,
            ISimulationLogger logger,
            IPedidoRepository pedidoRepo)
        {
            _attackService = attackService ?? throw new ArgumentNullException(nameof(attackService));
            _logger        = logger        ?? throw new ArgumentNullException(nameof(logger));
            _pedidoRepo    = pedidoRepo    ?? throw new ArgumentNullException(nameof(pedidoRepo));
        }

        // ─── PROPIEDADES ISimulationService ──────────────────────────────────

        public SimulationStatus Estado => _estadoSimulacion.Estado;
        public bool EstaCorreindo => _estadoSimulacion.Estado == SimulationStatus.Corriendo;
        public bool EstaUnderAttack => _attackService.IsAttackActive;

        // ─── PROPIEDADES ADICIONALES PARA LA UI ─────────────────────────────

        public IReadOnlyList<Cliente> Clientes
        {
            get { lock (_lockWorkers) return _clientes.ToList().AsReadOnly(); }
        }

        public IReadOnlyList<Cocinero> Cocineros
        {
            get { lock (_lockWorkers) return _cocineros.ToList().AsReadOnly(); }
        }

        public ConcurrentQueue<Pedido> Cola => _cola;

        public int EspaciosLibres => _semEspacios?.CurrentCount ?? 0;
        public int ItemsDisponibles => _semItems?.CurrentCount ?? 0;

        // ─── IMPLEMENTACIÓN ISimulationService ───────────────────────────────

        /// <inheritdoc/>
        public void Iniciar()
        {
            if (_estadoSimulacion.Estado == SimulationStatus.Corriendo)
                throw new InvalidOperationException("La simulación ya está corriendo.");

            _logger.Limpiar();
            _pedidoRepo.Limpiar();
            _estadoSimulacion.Reset();

            // Inicializar primitivas de sincronización
            InicializarPrimitivas();

            _estadoSimulacion.InicioSimulacion = DateTime.Now;
            _tiempoInicio = DateTime.Now;
            CambiarEstado(SimulationStatus.Corriendo);

            _logger.Log("═══════════════════════════════════════", LogLevel.Info);
            _logger.Log("   RESTAURANTE DON CÓDIGO - ABRIENDO   ", LogLevel.Info);
            _logger.Log("═══════════════════════════════════════", LogLevel.Info);
            _logger.Log(
                $"Simulación iniciada | Cola max: {AppConstants.CapacidadMaximaCola} | " +
                $"Sem[espacios]={_semEspacios.CurrentCount} | Sem[items]={_semItems.CurrentCount}",
                LogLevel.Sync);

            // Crear workers iniciales
            for (int i = 0; i < AppConstants.ProductoresIniciales; i++)
                AgregarProductorInterno();

            for (int i = 0; i < AppConstants.ConsumidoresIniciales; i++)
                AgregarConsumidorInterno();
        }

        /// <inheritdoc/>
        public void Detener()
        {
            if (_estadoSimulacion.Estado == SimulationStatus.Detenida)
                return;

            CambiarEstado(SimulationStatus.Deteniendo);
            _logger.Log("Deteniendo simulación...", LogLevel.Warning);

            // Asegurar que los hilos no estén pausados (si no, no chequean el token)
            _eventoPausa?.Set();

            // Señal de cancelación cooperativa a todos los workers
            _cts?.Cancel();

            // Esperar a que todos los hilos terminen (máximo 5 segundos por hilo)
            EsperarFinDeHilos();

            // Limpiar listas
            lock (_lockWorkers)
            {
                _productores.Clear();
                _consumidores.Clear();
                _clientes.Clear();
                _cocineros.Clear();
            }

            _contadorClientes = 0;
            _contadorCocineros = 0;

            // Liberar recursos
            _semEspacios?.Dispose();
            _semItems?.Dispose();
            _eventoPausa?.Dispose();
            _cts?.Dispose();

            CambiarEstado(SimulationStatus.Detenida);
            _logger.Log("Simulación detenida correctamente.", LogLevel.Info);
        }

        /// <inheritdoc/>
        public void Pausar()
        {
            if (_estadoSimulacion.Estado != SimulationStatus.Corriendo) return;

            // Reset() cierra la "puerta": los workers se bloquearán en Wait()
            _eventoPausa.Reset();
            CambiarEstado(SimulationStatus.Pausada);
            _logger.Log("⏸ Simulación PAUSADA - Todos los hilos suspendidos.", LogLevel.Warning);
        }

        /// <inheritdoc/>
        public void Reanudar()
        {
            if (_estadoSimulacion.Estado != SimulationStatus.Pausada) return;

            // Set() abre la "puerta": todos los workers continúan
            _eventoPausa.Set();
            CambiarEstado(SimulationStatus.Corriendo);
            _logger.Log("▶ Simulación REANUDADA - Hilos liberados.", LogLevel.Info);
        }

        /// <inheritdoc/>
        public SimulationStatistics ObtenerEstadisticas()
        {
            var tiempoEjecucion = _estadoSimulacion.InicioSimulacion.HasValue
                ? DateTime.Now - _estadoSimulacion.InicioSimulacion.Value
                : TimeSpan.Zero;

            int productoresActivos;
            int consumidoresActivos;

            lock (_lockWorkers)
            {
                productoresActivos = _clientes.Count(c => c.EstaActivo);
                consumidoresActivos = _cocineros.Count(c => c.EstaActivo);
            }

            return new SimulationStatistics
            {
                TiempoEjecucion          = tiempoEjecucion,
                EstadoActual             = _estadoSimulacion.Estado.ToString(),
                PedidosEnCola            = _cola?.Count ?? 0,
                CapacidadMaximaCola      = AppConstants.CapacidadMaximaCola,
                ProductoresActivos       = productoresActivos,
                ConsumidoresActivos      = consumidoresActivos,
                TotalPedidosGenerados    = _estadoSimulacion.TotalPedidosGenerados,
                TotalPedidosCompletados  = _estadoSimulacion.TotalPedidosCompletados,
                TotalPedidosCancelados   = _estadoSimulacion.TotalPedidosCancelados,
                TotalPedidosAlterados    = _estadoSimulacion.TotalPedidosAlterados,
                AtaqueActivo             = _attackService.IsAttackActive,
                TotalEventosAtaque       = _estadoSimulacion.TotalEventosAtaque,
                TipoAtaque               = _attackService.TipoAtaqueActivo.ToString()
            };
        }

        /// <inheritdoc/>
        public void AjustarVelocidad(int velocidadMs)
        {
            _velocidadProductorMs = Math.Max(100, velocidadMs);

            lock (_lockWorkers)
                foreach (var p in _productores)
                    p.AjustarVelocidad(_velocidadProductorMs);

            _logger.Log(
                $"Velocidad productores ajustada a {velocidadMs}ms",
                LogLevel.Info);
        }

        /// <summary>Ajusta la velocidad de los consumidores independientemente.</summary>
        public void AjustarVelocidadConsumidor(int velocidadMs)
        {
            _velocidadConsumidorMs = Math.Max(100, velocidadMs);

            lock (_lockWorkers)
                foreach (var c in _consumidores)
                    c.AjustarVelocidad(_velocidadConsumidorMs);

            _logger.Log(
                $"Velocidad cocineros ajustada a {velocidadMs}ms",
                LogLevel.Info);
        }

        // ─── GESTIÓN DINÁMICA DE WORKERS ─────────────────────────────────────

        /// <summary>
        /// Agrega un nuevo productor (cliente) a la simulación en tiempo real.
        /// El hilo se inicia inmediatamente si la simulación está corriendo.
        /// </summary>
        public void AgregarProductor()
        {
            lock (_lockWorkers)
            {
                if (_clientes.Count >= AppConstants.MaxProductores)
                {
                    _logger.Log(
                        $"Máximo de productores alcanzado ({AppConstants.MaxProductores})",
                        LogLevel.Warning);
                    return;
                }
            }
            AgregarProductorInterno();
        }

        /// <summary>Agrega un nuevo consumidor (cocinero) en tiempo real.</summary>
        public void AgregarConsumidor()
        {
            lock (_lockWorkers)
            {
                if (_cocineros.Count >= AppConstants.MaxConsumidores)
                {
                    _logger.Log(
                        $"Máximo de cocineros alcanzado ({AppConstants.MaxConsumidores})",
                        LogLevel.Warning);
                    return;
                }
            }
            AgregarConsumidorInterno();
        }

        /// <summary>Vacía la cola de pedidos en espera.</summary>
        public void VaciarCola()
        {
            while (_cola.TryDequeue(out _)) { }

            // Restablecer semáforos al estado "cola vacía"
            // semItems debe quedar en 0 (no hay items)
            // semEspacios debe quedar en capacidad (hay todos los espacios)
            while (_semItems.CurrentCount > 0)
                _semItems.Wait(0);

            int espaciosActuales = _semEspacios.CurrentCount;
            int espaciosARestaurar = AppConstants.CapacidadMaximaCola - espaciosActuales;
            if (espaciosARestaurar > 0)
                _semEspacios.Release(espaciosARestaurar);

            _logger.Log("🗑 Cola de pedidos vaciada.", LogLevel.Warning);
        }

        // ─── PRIVADOS ─────────────────────────────────────────────────────────

        private void InicializarPrimitivas()
        {
            _cola = SyncPrimitiveFactory.CrearColaPedidos();

            // SemaphoreSlim espacios: inicia lleno (cola vacía = todos los espacios libres)
            _semEspacios = SyncPrimitiveFactory.CrearSemaforoEspaciosLibres(
                AppConstants.CapacidadMaximaCola);

            // SemaphoreSlim items: inicia vacío (no hay items para consumir)
            _semItems = SyncPrimitiveFactory.CrearSemaforoItemsDisponibles(
                AppConstants.CapacidadMaximaCola);

            // Evento de pausa: inicia en Set (puerta abierta = corriendo)
            _eventoPausa = SyncPrimitiveFactory.CrearEventoPausa();

            // Token de cancelación fresco para esta sesión
            _cts = SyncPrimitiveFactory.CrearCancellationTokenSource();
        }

        private void AgregarProductorInterno()
        {
            int numero = Interlocked.Increment(ref _contadorClientes);
            int mesa   = (numero % 10) + 1;

            var cliente = new Cliente($"Cliente-{numero}", mesa);
            var worker  = new ProductorWorker(
                cliente, _cola,
                _semEspacios, _semItems,
                _eventoPausa,
                _attackService, _logger,
                _estadoSimulacion, _pedidoRepo,
                _velocidadProductorMs);

            // Suscribir para notificar a la UI
            worker.PedidoGenerado += (_, p) =>
                ActualizacionDisponible?.Invoke(this, EventArgs.Empty);

            lock (_lockWorkers)
            {
                _clientes.Add(cliente);
                _productores.Add(worker);
            }

            if (_estadoSimulacion.Estado == SimulationStatus.Corriendo ||
                _estadoSimulacion.Estado == SimulationStatus.BajoAtaque)
            {
                worker.Iniciar(_cts.Token);
            }
        }

        private void AgregarConsumidorInterno()
        {
            int numero      = Interlocked.Increment(ref _contadorCocineros);
            string espec    = SimulationConstants.EspecialidadesCocineros[
                (numero - 1) % SimulationConstants.EspecialidadesCocineros.Length];

            var cocinero = new Cocinero($"Cocinero-{numero}", espec);
            var worker   = new ConsumidorWorker(
                cocinero, _cola,
                _semEspacios, _semItems,
                _eventoPausa,
                _logger, _estadoSimulacion,
                _pedidoRepo,
                _velocidadConsumidorMs);

            worker.PedidoCompletado += (_, p) =>
                ActualizacionDisponible?.Invoke(this, EventArgs.Empty);

            worker.ProgresoActualizado += (_, datos) =>
                ActualizacionDisponible?.Invoke(this, EventArgs.Empty);

            lock (_lockWorkers)
            {
                _cocineros.Add(cocinero);
                _consumidores.Add(worker);
            }

            if (_estadoSimulacion.Estado == SimulationStatus.Corriendo ||
                _estadoSimulacion.Estado == SimulationStatus.BajoAtaque)
            {
                worker.Iniciar(_cts.Token);
            }
        }

        private void CambiarEstado(SimulationStatus nuevoEstado)
        {
            _estadoSimulacion.Estado = nuevoEstado;
            EstadoCambiado?.Invoke(this, nuevoEstado);
        }

        private void EsperarFinDeHilos()
        {
            List<Thread> hilos = new();

            lock (_lockWorkers)
            {
                hilos.AddRange(_productores
                    .Select(p => p.ObtenerHilo())
                    .Where(h => h != null)!);
                hilos.AddRange(_consumidores
                    .Select(c => c.ObtenerHilo())
                    .Where(h => h != null)!);
            }

            foreach (var hilo in hilos)
            {
                // Esperar máximo 3 segundos por hilo
                if (!hilo.Join(3000))
                {
                    _logger.Log(
                        $"Advertencia: hilo {hilo.Name} no terminó en tiempo.",
                        LogLevel.Warning);
                }
            }
        }

        // ─── IDisposable ──────────────────────────────────────────────────────

        public void Dispose()
        {
            if (_estadoSimulacion.Estado != SimulationStatus.Detenida)
                Detener();

            _semEspacios?.Dispose();
            _semItems?.Dispose();
            _eventoPausa?.Dispose();
            _cts?.Dispose();
        }
    }
}