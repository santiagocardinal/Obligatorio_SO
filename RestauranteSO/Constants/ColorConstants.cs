// =============================================================================
// RestauranteSO - Sistema de Simulación de Sistemas Operativos
// Archivo  : Constants/ColorConstants.cs
// Propósito: Paleta de colores centralizada para toda la aplicación.
//            Garantiza consistencia visual en todos los formularios.
// SOLID    : Single Responsibility. DRY.
// =============================================================================

namespace RestauranteSO.Constants
{
    /// <summary>
    /// Paleta de colores unificada de RestauranteSO.
    /// 
    /// Tema oscuro profesional inspirado en dashboards de monitoreo.
    /// Todos los colores están documentados con su propósito.
    /// Modificar aquí cambia el aspecto de TODA la aplicación.
    /// </summary>
    public static class ColorConstants
    {
        // ─── FONDO ───────────────────────────────────────────────────────────

        /// <summary>Fondo principal de la aplicación.</summary>
        public static readonly Color FondoPrincipal = Color.FromArgb(18, 18, 24);

        /// <summary>Fondo de paneles secundarios.</summary>
        public static readonly Color FondoPanel = Color.FromArgb(26, 26, 36);

        /// <summary>Fondo de tarjetas (Cards).</summary>
        public static readonly Color FondoCard = Color.FromArgb(32, 32, 44);

        /// <summary>Fondo de tarjetas en hover.</summary>
        public static readonly Color FondoCardHover = Color.FromArgb(42, 42, 58);

        /// <summary>Fondo del panel lateral.</summary>
        public static readonly Color FondoLateral = Color.FromArgb(22, 22, 32);

        /// <summary>Fondo de la barra superior.</summary>
        public static readonly Color FondoSuperior = Color.FromArgb(15, 15, 22);

        /// <summary>Fondo de la barra inferior.</summary>
        public static readonly Color FondoInferior = Color.FromArgb(15, 15, 22);

        // ─── TEXTO ───────────────────────────────────────────────────────────

        /// <summary>Texto principal.</summary>
        public static readonly Color TextoPrincipal = Color.FromArgb(240, 240, 245);

        /// <summary>Texto secundario / subtítulos.</summary>
        public static readonly Color TextoSecundario = Color.FromArgb(160, 160, 180);

        /// <summary>Texto de descripciones / hints.</summary>
        public static readonly Color TextoHint = Color.FromArgb(100, 100, 130);

        // ─── ACENTOS ─────────────────────────────────────────────────────────

        /// <summary>Acento principal: azul vibrante.</summary>
        public static readonly Color AcentoPrincipal = Color.FromArgb(64, 156, 255);

        /// <summary>Acento secundario: naranja cálido (restaurante).</summary>
        public static readonly Color AcentoSecundario = Color.FromArgb(255, 140, 60);

        /// <summary>Acento terciario: verde para estados OK.</summary>
        public static readonly Color AcentoExito = Color.FromArgb(80, 220, 120);

        // ─── ESTADOS ─────────────────────────────────────────────────────────

        /// <summary>Color para estado "Esperando" (amarillo).</summary>
        public static readonly Color EstadoEsperando = Color.FromArgb(255, 200, 50);

        /// <summary>Color para estado "En Preparación" (azul).</summary>
        public static readonly Color EstadoEnPreparacion = Color.FromArgb(64, 156, 255);

        /// <summary>Color para estado "Listo" (verde).</summary>
        public static readonly Color EstadoListo = Color.FromArgb(80, 220, 120);

        /// <summary>Color para estado "Entregado" (gris).</summary>
        public static readonly Color EstadoEntregado = Color.FromArgb(120, 120, 140);

        /// <summary>Color para estado "Alterado" por ataque (naranja).</summary>
        public static readonly Color EstadoAlterado = Color.FromArgb(255, 100, 50);

        /// <summary>Color para estado "Duplicado" fraudulentamente (rojo).</summary>
        public static readonly Color EstadoDuplicado = Color.FromArgb(220, 50, 80);

        // ─── ATAQUES / SEGURIDAD ─────────────────────────────────────────────

        /// <summary>Color principal de alerta de ataque.</summary>
        public static readonly Color AlertaAtaque = Color.FromArgb(220, 50, 80);

        /// <summary>Color de fondo para el panel de ataque activo.</summary>
        public static readonly Color FondoAtaque = Color.FromArgb(40, 15, 15);

        /// <summary>Color para políticas de seguridad.</summary>
        public static readonly Color ColorSeguridad = Color.FromArgb(80, 220, 120);

        /// <summary>Color de fondo para el panel de políticas.</summary>
        public static readonly Color FondoPoliticas = Color.FromArgb(15, 35, 20);

        // ─── TARJETAS DEL DASHBOARD ──────────────────────────────────────────

        /// <summary>Color de acento para Tarjeta 1 (Productor-Consumidor).</summary>
        public static readonly Color TarjetaProductor = Color.FromArgb(64, 156, 255);

        /// <summary>Color de acento para Tarjeta 2 (Lectores-Escritores).</summary>
        public static readonly Color TarjetaLectores = Color.FromArgb(150, 100, 255);

        /// <summary>Color de acento para Tarjeta 3 (Ataque PC).</summary>
        public static readonly Color TarjetaAtaque1 = Color.FromArgb(255, 100, 50);

        /// <summary>Color de acento para Tarjeta 4 (Ataque LE).</summary>
        public static readonly Color TarjetaAtaque2 = Color.FromArgb(255, 50, 100);

        // ─── SEPARADORES ─────────────────────────────────────────────────────

        /// <summary>Color de líneas separadoras.</summary>
        public static readonly Color Separador = Color.FromArgb(45, 45, 60);

        /// <summary>Color de borde de cards.</summary>
        public static readonly Color BordeCard = Color.FromArgb(50, 50, 70);

        /// <summary>Color de borde de cards en hover.</summary>
        public static readonly Color BordeCardHover = Color.FromArgb(64, 156, 255);
    }
}