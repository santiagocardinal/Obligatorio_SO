// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Configuration/AppSettings.cs
// Propósito: Configuración de la aplicación y contenedor de DI.
//            Registra todas las dependencias y provee el ServiceProvider.
// SOLID    : DIP - centraliza el registro de dependencias.
// Patrón   : Dependency Injection Container.
// =============================================================================

using Microsoft.Extensions.DependencyInjection;
using RestauranteSO.Domain.Interfaces;
using RestauranteSO.Infrastructure.Logging;
using RestauranteSO.Infrastructure.Repositories;
using RestauranteSO.Services.Ataques;
using RestauranteSO.Services.LectoresEscritores;
using RestauranteSO.Services.ProductorConsumidor;

namespace RestauranteSO.Configuration
{
    /// <summary>
    /// Configurador del contenedor de Inyección de Dependencias.
    ///
    /// ¿Por qué DI?
    /// - Desacopla la creación de objetos de su uso.
    /// - Facilita pruebas unitarias (se puede inyectar mocks).
    /// - Aplica DIP: las clases dependen de interfaces, no de implementaciones.
    /// - Un solo lugar para cambiar qué implementación se usa.
    ///
    /// Lifetimes usados:
    /// - Singleton: ISimulationLogger, IMenuRepository, IPedidoRepository
    ///   → Una sola instancia compartida en toda la app.
    /// - Transient: Los servicios de simulación
    ///   → Nueva instancia cada vez que se pide (cada ventana tiene la suya).
    /// </summary>
    public static class AppSettings
    {
        private static ServiceProvider? _serviceProvider;

        /// <summary>
        /// ServiceProvider configurado con todas las dependencias.
        /// Accesible desde Program.cs y desde los formularios.
        /// </summary>
        public static ServiceProvider ServiceProvider
        {
            get
            {
                if (_serviceProvider == null)
                    throw new InvalidOperationException(
                        "AppSettings.Configurar() debe llamarse antes de acceder a ServiceProvider.");
                return _serviceProvider;
            }
        }

        /// <summary>
        /// Configura y construye el contenedor de DI.
        /// Debe llamarse UNA sola vez al inicio de la aplicación en Program.cs.
        /// </summary>
        public static void Configurar()
        {
            var services = new ServiceCollection();

            // ── Logging ───────────────────────────────────────────────────
            // Singleton: un solo logger para toda la app.
            // Los logs de todas las simulaciones van al mismo logger.
            services.AddSingleton<ISimulationLogger, SimulationLogger>();

            // ── Repositorios ──────────────────────────────────────────────
            // Singleton: los repositorios mantienen estado entre sesiones.
            services.AddSingleton<IMenuRepository, MenuRepository>();
            services.AddSingleton<IPedidoRepository, PedidoRepository>();

            // ── Servicios de Ataque ────────────────────────────────────────
            // Singleton: el estado del ataque debe ser compartido entre
            // el servicio y los workers de la simulación.
            services.AddSingleton<AtaqueProductorConsumidorService>();
            services.AddSingleton<AtaqueLectoresEscritoresService>(sp =>
                new AtaqueLectoresEscritoresService(
                    sp.GetRequiredService<IMenuRepository>(),
                    sp.GetRequiredService<ISimulationLogger>()));

            // ── Servicios de Simulación ────────────────────────────────────
            // Transient: cada vez que se abre una ventana de simulación
            // obtiene una instancia fresca del servicio.
            services.AddTransient<ProductorConsumidorService>(sp =>
                new ProductorConsumidorService(
                    sp.GetRequiredService<AtaqueProductorConsumidorService>(),
                    sp.GetRequiredService<ISimulationLogger>(),
                    sp.GetRequiredService<IPedidoRepository>()));

            services.AddTransient<LectoresEscritoresService>(sp =>
                new LectoresEscritoresService(
                    sp.GetRequiredService<AtaqueLectoresEscritoresService>(),
                    sp.GetRequiredService<ISimulationLogger>(),
                    sp.GetRequiredService<IMenuRepository>()));

            _serviceProvider = services.BuildServiceProvider();
        }

        /// <summary>
        /// Resuelve un servicio del tipo especificado.
        /// Atajo para no tener que escribir ServiceProvider.GetRequiredService() siempre.
        /// </summary>
        public static T Resolver<T>() where T : notnull
            => ServiceProvider.GetRequiredService<T>();
    }
}