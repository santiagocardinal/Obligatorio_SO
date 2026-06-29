// =============================================================================
// RestaurantOS — Restaurant Operating Environment
// Archivo  : Constants/ColorConstants.cs
// Propósito: Paleta de colores unificada del Design System.
//            Única fuente de verdad para todos los colores de la aplicación.
//            NUNCA usar Color.Transparent en WinForms — siempre color real.
// =============================================================================

namespace RestauranteSO.Constants
{
    public static class ColorConstants
    {
        // ─── FONDOS ───────────────────────────────────────────────────────────

        /// <summary>Fondo principal de la aplicación — casi negro azulado</summary>
        public static readonly Color FondoPrincipal   = Color.FromArgb(13,  13,  26);

        /// <summary>Fondo del header superior</summary>
        public static readonly Color FondoSuperior    = Color.FromArgb(18,  18,  31);

        /// <summary>Fondo de paneles secundarios</summary>
        public static readonly Color FondoPanel       = Color.FromArgb(22,  22,  36);

        /// <summary>Fondo de cards y elementos elevados</summary>
        public static readonly Color FondoCard        = Color.FromArgb(26,  26,  46);

        /// <summary>Fondo de sidebars y paneles laterales</summary>
        public static readonly Color FondoLateral     = Color.FromArgb(20,  20,  34);

        /// <summary>Fondo del dock inferior</summary>
        public static readonly Color FondoDock        = Color.FromArgb(10,  10,  22);

        /// <summary>Fondo de contexto de ataque</summary>
        public static readonly Color FondoAtaque      = Color.FromArgb(26,  10,  10);

        /// <summary>Fondo de contexto de políticas de seguridad</summary>
        public static readonly Color FondoPoliticas   = Color.FromArgb(10,  26,  10);

        /// <summary>Fondo del splash screen</summary>
        public static readonly Color FondoSplash      = Color.FromArgb( 8,   8,  18);

        /// <summary>Fondo del welcome panel</summary>
        public static readonly Color FondoWelcome     = Color.FromArgb(15,  15,  28);

        // ─── ACENTOS PRINCIPALES ──────────────────────────────────────────────

        /// <summary>Acento principal del sistema — violeta marca</summary>
        public static readonly Color AcentoPrincipal  = Color.FromArgb(108,  99, 255);

        /// <summary>Acento secundario — naranja cálido</summary>
        public static readonly Color AcentoSecundario = Color.FromArgb(255, 159,  67);

        /// <summary>Acento de éxito — verde</summary>
        public static readonly Color AcentoExito      = Color.FromArgb( 38, 222, 129);

        /// <summary>Alerta de ataque — rojo</summary>
        public static readonly Color AlertaAtaque     = Color.FromArgb(255,  71,  87);

        /// <summary>Advertencia — amarillo</summary>
        public static readonly Color Advertencia      = Color.FromArgb(255, 210,  88);

        // ─── COLORES POR MÓDULO ───────────────────────────────────────────────

        /// <summary>Color del módulo Productor-Consumidor — violeta</summary>
        public static readonly Color TarjetaProductor = Color.FromArgb(108,  99, 255);

        /// <summary>Color del módulo Lectores-Escritores — cyan</summary>
        public static readonly Color TarjetaLectores  = Color.FromArgb( 38, 198, 218);

        /// <summary>Color del Ataque 1 — rojo intenso</summary>
        public static readonly Color TarjetaAtaque1   = Color.FromArgb(255,  71,  87);

        /// <summary>Color del Ataque 2 — rosa</summary>
        public static readonly Color TarjetaAtaque2   = Color.FromArgb(255, 107, 129);

        // ─── TEXTO ────────────────────────────────────────────────────────────

        /// <summary>Texto principal — blanco suave</summary>
        public static readonly Color TextoPrincipal   = Color.FromArgb(234, 234, 244);

        /// <summary>Texto secundario — gris claro</summary>
        public static readonly Color TextoSecundario  = Color.FromArgb(160, 160, 184);

        /// <summary>Texto de hint — gris oscuro</summary>
        public static readonly Color TextoHint        = Color.FromArgb( 90,  90, 122);

        /// <summary>Texto sobre fondo de acento</summary>
        public static readonly Color TextoSobreAcento = Color.White;

        // ─── BORDES Y SEPARADORES ─────────────────────────────────────────────

        /// <summary>Separador entre secciones</summary>
        public static readonly Color Separador        = Color.FromArgb( 42,  42,  64);

        /// <summary>Borde de cards</summary>
        public static readonly Color BordeCard        = Color.FromArgb( 37,  37,  56);

        /// <summary>Borde del dock</summary>
        public static readonly Color BordeDock        = Color.FromArgb( 30,  30,  50);

        /// <summary>Borde de inputs</summary>
        public static readonly Color BordeInput       = Color.FromArgb( 55,  55,  80);

        // ─── ESTADOS DE PEDIDOS ───────────────────────────────────────────────

        public static readonly Color EstadoEsperando      = Color.FromArgb(255, 210,  88);
        public static readonly Color EstadoEnPreparacion  = Color.FromArgb( 38, 198, 218);
        public static readonly Color EstadoListo          = Color.FromArgb( 38, 222, 129);
        public static readonly Color EstadoEntregado      = Color.FromArgb(160, 160, 184);
        public static readonly Color EstadoAlterado       = Color.FromArgb(255,  71,  87);
        public static readonly Color EstadoDuplicado      = Color.FromArgb(255, 159,  67);

        // ─── DOCK ─────────────────────────────────────────────────────────────

        /// <summary>Fondo del ítem de dock en reposo</summary>
        public static readonly Color DockItemFondo        = Color.FromArgb( 22,  22,  38);

        /// <summary>Fondo del ítem de dock en hover</summary>
        public static readonly Color DockItemHover        = Color.FromArgb( 32,  32,  52);

        /// <summary>Indicador de módulo activo (punto inferior)</summary>
        public static readonly Color DockIndicadorActivo  = Color.FromArgb(108,  99, 255);

        // ─── SPLASH ───────────────────────────────────────────────────────────

        public static readonly Color SplashBarraFondo     = Color.FromArgb( 30,  30,  50);
        public static readonly Color SplashBarraRelleno   = Color.FromArgb(108,  99, 255);
        public static readonly Color SplashTextoVersion   = Color.FromArgb( 90,  90, 122);

        // ─── MAPA DEL RESTAURANTE ─────────────────────────────────────────────

        public static readonly Color MapaFondo            = Color.FromArgb( 15,  15,  28);
        public static readonly Color MapaNodo             = Color.FromArgb( 26,  26,  46);
        public static readonly Color MapaConexion         = Color.FromArgb( 42,  42,  64);
        public static readonly Color MapaCliente          = Color.FromArgb(108,  99, 255);
        public static readonly Color MapaCocina           = Color.FromArgb( 38, 198, 218);
        public static readonly Color MapaMesero           = Color.FromArgb( 38, 222, 129);
        public static readonly Color MapaGerente          = Color.FromArgb(255, 159,  67);
        public static readonly Color MapaCola             = Color.FromArgb(255, 210,  88);

        // ─── SEGURIDAD ────────────────────────────────────────────────────────

        public static readonly Color ColorSeguridad       = Color.FromArgb( 38, 222, 129);
    }
}