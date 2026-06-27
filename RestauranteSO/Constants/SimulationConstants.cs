// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Constants/SimulationConstants.cs
// Propósito: Constantes específicas de las simulaciones de concurrencia.
// SOLID    : Single Responsibility. DRY.
// =============================================================================

namespace RestauranteSO.Constants
{
    /// <summary>
    /// Constantes de la lógica de simulación.
    /// Nombres de platos, especialidades y datos del restaurante.
    /// </summary>
    public static class SimulationConstants
    {
        // ─── PLATOS DEL MENÚ ─────────────────────────────────────────────────

        /// <summary>
        /// Lista de platos que los clientes pueden pedir.
        /// Usada para generar pedidos aleatorios en el Productor.
        /// </summary>
        public static readonly string[] PlatosMenu = {
            "Milanesa napolitana con papas fritas",
            "Pizza Margherita",
            "Bife de chorizo con ensalada",
            "Pasta carbonara",
            "Pollo grillé con vegetales",
            "Hamburguesa doble con cheddar",
            "Risotto de hongos",
            "Salmón al horno con limón",
            "Ravioles de ricota y espinaca",
            "Lomo al champignon",
            "Ensalada César con pollo",
            "Tarta de verduras",
            "Sorrentinos de jamón y queso",
            "Provoleta a la parrilla",
            "Empanadas de carne (x4)"
        };

        /// <summary>
        /// Platos modificados que el atacante introduce al menú.
        /// Usados en la simulación de ataque de Lectores-Escritores.
        /// </summary>
        public static readonly string[] PlatosAtaque = {
            "Pedido COMPROMETIDO #ERR",
            "Plato NO DISPONIBLE (Datos falsos)",
            "ERROR: Item eliminado",
            "INYECCIÓN: Orden inválida",
            "DATOS CORRUPTOS - Ver gerente"
        };

        // ─── ESPECIALIDADES DE COCINEROS ─────────────────────────────────────

        public static readonly string[] EspecialidadesCocineros = {
            "Parrilla", "Pastas", "Pizzas", "Ensaladas",
            "Postres", "Vegetariano", "Chef Principal"
        };

        // ─── CATEGORÍAS DEL MENÚ ─────────────────────────────────────────────

        public static readonly string[] CategoriasMenu = {
            "Entradas", "Principales", "Pastas", "Carnes",
            "Pescados", "Ensaladas", "Postres", "Bebidas"
        };

        // ─── MENÚ INICIAL DEL RESTAURANTE ───────────────────────────────────

        /// <summary>
        /// Datos del menú inicial para poblar el MenuRepository.
        /// Tupla: (nombre, precio, categoria, descripcion)
        /// </summary>
        public static readonly (string nombre, decimal precio, string cat, string desc)[] MenuInicial = {
            ("Provoleta a la parrilla",     850m,  "Entradas",    "Queso provolone gratinado con orégano"),
            ("Tabla de fiambres",           1200m, "Entradas",    "Selección de embutidos y quesos"),
            ("Empanadas de carne (x4)",     900m,  "Entradas",    "Empanadas criollas al horno"),
            ("Milanesa napolitana",         1800m, "Principales", "Con jamón, mozzarella y salsa de tomate"),
            ("Bife de chorizo 400g",        2500m, "Carnes",      "Con chimichurri y papas fritas"),
            ("Lomo al champignon",          2800m, "Carnes",      "Con salsa de champignons y puré"),
            ("Salmón al horno",             2200m, "Pescados",    "Con limón, alcaparras y vegetales"),
            ("Pasta carbonara",             1400m, "Pastas",      "Spaghetti con panceta, huevo y parmesano"),
            ("Ravioles de ricota",          1350m, "Pastas",      "Con salsa pomodoro y albahaca fresca"),
            ("Ensalada César",              1100m, "Ensaladas",   "Con pollo grillé, crutones y aderezo"),
            ("Tiramisú",                    850m,  "Postres",     "Clásico italiano con mascarpone"),
            ("Agua mineral (500ml)",        300m,  "Bebidas",     "Con o sin gas"),
            ("Gaseosa",                     350m,  "Bebidas",     "Coca-Cola, Sprite o Fanta"),
            ("Vino de la casa (copa)",      550m,  "Bebidas",     "Tinto, blanco o rosado"),
        };

        // ─── PROBABILIDADES DE ATAQUE ────────────────────────────────────────

        /// <summary>Probabilidad de duplicar un pedido durante el ataque (0-100).</summary>
        public const int ProbabilidadDuplicarPedido = 25;

        /// <summary>Probabilidad de eliminar un pedido durante el ataque (0-100).</summary>
        public const int ProbabilidadEliminarPedido = 20;

        /// <summary>Probabilidad de alterar la descripción de un pedido (0-100).</summary>
        public const int ProbabilidadAlterarPedido = 55;

        /// <summary>Multiplicador de precio durante el ataque al menú.</summary>
        public const decimal MultiplicadorPrecioAtaque = 3.0m;

        // ─── TIEMPOS DE PREPARACIÓN ──────────────────────────────────────────

        /// <summary>Tiempo mínimo de preparación de cualquier plato (ms).</summary>
        public const int TiempoPreparacionMinMs = 1500;

        /// <summary>Tiempo máximo de preparación de un plato (ms).</summary>
        public const int TiempoPreparacionMaxMs = 5000;
    }
}