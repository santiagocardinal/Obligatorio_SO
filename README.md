# RestauranteSO – Simulador de Sistemas Operativos

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Windows Forms](https://img.shields.io/badge/WinForms-UI-0078D4?logo=windows)](https://learn.microsoft.com/en-us/dotnet/desktop/winforms/)
[![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)
[![Status](https://img.shields.io/badge/status-completo-brightgreen)]()

> **Simulador educativo de problemas clásicos de Sistemas Operativos**, ambientado en un restaurante, con concurrencia real, sincronización de hilos y módulos de ciberseguridad (ingeniería social). Desarrollado en **C# .NET 8 con Windows Forms**.

*Proyecto universitario – Arquitectura de Software, Concurrencia y Sistemas Operativos.*

---

## Tabla de Contenidos

- [Introducción](#introduccion)
- [Características Principales](#caracteristicas-principales)
- [Arquitectura y Diseño](#arquitectura-y-diseno)
  - [Clean Architecture](#clean-architecture)
  - [Principios SOLID](#principios-solid)
  - [Patrones de Diseño](#patrones-de-diseno)
  - [Diagrama de Clases](#diagrama-de-clases)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Tecnologías y Dependencias](#tecnologias-y-dependencias)
- [Instalación y Ejecución](#instalacion-y-ejecucion)
- [Simulaciones](#simulaciones)
  - [Productor – Consumidor (Clientes – Cocineros)](#productor--consumidor-clientes--cocineros)
  - [Lectores – Escritores (Meseros – Gerente)](#lectores--escritores-meseros--gerente)
  - [Módulos de Ataque (Ingeniería Social)](#modulos-de-ataque-ingenieria-social)
- [Sincronización y Concurrencia en Detalle](#sincronizacion-y-concurrencia-en-detalle)
- [Guía de Uso](#guia-de-uso)
- [Capturas de Pantalla](#capturas-de-pantalla)
- [Conclusiones y Aprendizaje](#conclusiones-y-aprendizaje)


---

## Introducción

**RestauranteSO** nace como un proyecto académico para **visualizar y experimentar** con dos problemas clásicos de **Sistemas Operativos**:

- **Productor‑Consumidor** (cola de pedidos)
- **Lectores‑Escritores** (menú compartido)

Además, incorpora dos **simulaciones educativas de ataques de ingeniería social** que muestran vulnerabilidades reales y sus correspondientes **políticas de prevención** (MFA, backups, EDR, etc.).

Toda la aplicación está **ambientada en un restaurante** y presenta una **interfaz moderna, oscura y profesional** con animaciones, paneles de control y visualización en tiempo real de hilos, semáforos y locks.

> [!NOTE]
> Este proyecto fue desarrollado íntegramente con **.NET 8** y **Windows Forms**, sin dependencias externas.

---

## Características Principales

| Área                     | Funcionalidad                                                                                             |
|--------------------------|-----------------------------------------------------------------------------------------------------------|
| **Concurrencia real**    | Hilos `Thread` y `Task` con `SemaphoreSlim`, `ReaderWriterLockSlim`, `ConcurrentQueue`, `ManualResetEventSlim`, `CancellationToken`. |
| **Visualización en tiempo real** | Kanban de pedidos, indicadores de estado de hilos, barras de semáforos, logs coloreados.                  |
| **Interactividad**       | Agregar/eliminar productores/consumidores, pausar, reanudar, ajustar velocidad, vaciar cola.              |
| **Módulos de ataque educativo** | Ingeniería social (falso técnico, phishing) que altera la simulación sin ejecutar código malicioso real. |
| **Políticas de prevención** | Panel detallado con vulnerabilidades explotadas, implementación de controles y marcos de referencia (NIST, ISO 27001). |
| **Arquitectura limpia**  | Separación en capas: *Domain*, *Infrastructure*, *Services*, *Presentation*. Inyección de dependencias manual. |
| **Diseño profesional**   | Tema oscuro, gradientes, sombras, cards animadas, fuentes jerarquizadas y responsive.                     |

---

## Arquitectura y Diseño

## Estructura del Proyecto

La estructura de carpetas sigue una arquitectura por capas (Clean Architecture). A continuación se muestra el árbol de directorios:

```plaintext
RestauranteSO/
│
├── Configuration/                     # Configuración e inyección de dependencias
│   └── AppSettings.cs
│
├── Constants/                         # Constantes globales (colores, valores, etc.)
│   ├── AppConstants.cs
│   ├── ColorConstants.cs
│   └── SimulationConstants.cs
│
├── Domain/                            # Capa de dominio (entidades, enums, interfaces)
│   ├── Entities/                      # Entidades de negocio
│   │   ├── Pedido.cs
│   │   ├── MenuItem.cs
│   │   ├── Cliente.cs
│   │   ├── Cocinero.cs
│   │   └── Mesero.cs
│   ├── Enums/                         # Enumeraciones
│   │   ├── PedidoEstado.cs
│   │   ├── SimulationStatus.cs
│   │   ├── AttackType.cs
│   │   └── LogLevel.cs
│   ├── Interfaces/                    # Contratos de servicios y repositorios
│   │   ├── ISimulationService.cs
│   │   ├── IAttackService.cs
│   │   ├── ISimulationLogger.cs
│   │   ├── IMenuRepository.cs
│   │   └── IPedidoRepository.cs
│   └── Models/                        # DTOs y modelos de datos
│       ├── SimulationStatistics.cs
│       ├── SimulationState.cs
│       ├── AttackEvent.cs
│       └── LogEntry.cs
│
├── Infrastructure/                    # Capa de infraestructura (implementaciones)
│   ├── Logging/
│   │   └── SimulationLogger.cs
│   ├── Synchronization/
│   │   ├── SyncPrimitiveFactory.cs
│   │   └── SharedBufferMonitor.cs
│   └── Repositories/
│       ├── MenuRepository.cs
│       └── PedidoRepository.cs
│
├── Services/                          # Capa de servicios (lógica de negocio)
│   ├── ProductorConsumidor/
│   │   ├── ProductorWorker.cs
│   │   ├── ConsumidorWorker.cs
│   │   └── ProductorConsumidorService.cs
│   ├── LectoresEscritores/
│   │   ├── LectorWorker.cs
│   │   ├── EscritorWorker.cs
│   │   └── LectoresEscritoresService.cs
│   └── Ataques/
│       ├── AtaqueProductorConsumidorService.cs
│       └── AtaqueLectoresEscritoresService.cs
│
├── Presentation/                      # Capa de presentación (UI)
│   ├── Forms/                         # Formularios WinForms
│   │   ├── FrmDashboard.cs
│   │   ├── FrmProductorConsumidor.cs
│   │   ├── FrmLectoresEscritores.cs
│   │   ├── FrmAtaqueProductorConsumidor.cs
│   │   ├── FrmAtaqueLectoresEscritores.cs
│   │   ├── FrmPoliticas.cs
│   │   └── FrmIngenieriaSocial.cs
│   ├── Controls/                      # Controles personalizados
│   │   ├── SimulationCard.cs
│   │   ├── LogViewer.cs
│   │   ├── ThreadStatusIndicator.cs
│   │   └── SemaphoreVisualizer.cs
│   ├── Components/                    # Componentes reutilizables
│   │   ├── AnimatedPanel.cs
│   │   └── StatusBadge.cs
│   └── Themes/                        # Temas y estilos
│       └── AppTheme.cs
│
├── Program.cs                         # Punto de entrada
└── RestauranteSO.csproj               # Archivo de proyecto
```

---

## Clean Architecture (Arquitectura por Capas)

El proyecto sigue el principio de **inversión de dependencias**, donde las capas internas (Domain) no dependen de las externas (Infrastructure, Services, Presentation). La dirección de las dependencias es hacia adentro:

| Capa | Responsabilidad | Dependencias |
|------|-----------------|--------------|
| **Domain** | Entidades, enums, interfaces de servicios y repositorios. No contiene lógica de implementación. | Ninguna (es la capa más interna). |
| **Infrastructure** | Implementación concreta de repositorios, logging y fábricas de sincronización. | Depende de las interfaces definidas en **Domain**. |
| **Services** | Orquestación de hilos, lógica de negocio de las simulaciones y ataques. | Depende de **Domain** y **Infrastructure** (a través de interfaces). |
| **Presentation** | Interfaz de usuario (WinForms). Controles, formularios y temas. | Depende de **Services** (a través de interfaces), **Domain** y **Infrastructure**. |

---

## Diagrama de Clases

A continuación se muestra un diagrama de clases simplificado con las principales entidades, servicios y relaciones:

| Clase / Interfaz | Descripción | Relaciones |
|------------------|-------------|------------|
| `ISimulationService` | Interfaz que define el contrato para cualquier simulación (iniciar, detener, pausar, obtener estadísticas). | Implementada por `ProductorConsumidorService` y `LectoresEscritoresService`. |
| `IAttackService` | Interfaz para servicios de ataque (activar, desactivar, historial). | Implementada por `AtaqueProductorConsumidorService` y `AtaqueLectoresEscritoresService`. |
| `ProductorConsumidorService` | Orquestador de productores y consumidores. Gestiona `SemaphoreSlim`, `ConcurrentQueue` y `ManualResetEventSlim`. | Compone con `ProductorWorker` y `ConsumidorWorker`. |
| `LectoresEscritoresService` | Orquestador de lectores y escritores. Gestiona `ReaderWriterLockSlim`. | Compone con `LectorWorker` y `EscritorWorker`. |
| `ProductorWorker` | Hilo que genera pedidos (cliente). | Depende de `IAttackService` y `ISimulationLogger`. |
| `ConsumidorWorker` | Hilo que consume pedidos (cocinero). | Depende de `ISimulationLogger`. |
| `LectorWorker` | Tarea que lee el menú (mesero). | Depende de `ReaderWriterLockSlim` y `IAttackService`. |
| `EscritorWorker` | Tarea que modifica el menú (gerente). | Depende de `ReaderWriterLockSlim`. |
| `Pedido` | Entidad que representa un pedido (con estado, descripción, etc.). | Utilizado por `ProductorWorker` y `ConsumidorWorker`. |
| `MenuItem` | Entidad que representa un ítem del menú. | Utilizado por `LectorWorker` y `EscritorWorker`. |
| `ISimulationLogger` | Interfaz para el logging de eventos. | Implementada por `SimulationLogger`. |
| `IMenuRepository` | Interfaz para el repositorio del menú. | Implementada por `MenuRepository`. |
| `IPedidoRepository` | Interfaz para el repositorio de pedidos. | Implementada por `PedidoRepository`. |

---

## Simulaciones

### Productor – Consumidor (Clientes – Cocineros)

**Problema clásico**: múltiples *clientes* (productores) generan pedidos y los colocan en una cola compartida; varios *cocineros* (consumidores) toman los pedidos y los preparan.

#### Mecanismos de sincronización:

- **SemaphoreSlim espacios libres** – controla la capacidad máxima de la cola (inicia en `CapacidadMaxima`). Los productores hacen `Wait()` antes de encolar; si la cola está llena, el hilo se bloquea.
- **SemaphoreSlim items disponibles** – controla cuántos pedidos hay en la cola (inicia en 0). Los consumidores hacen `Wait()`; si la cola está vacía, esperan.
- **ConcurrentQueue** – cola thread‑safe sin bloqueos manuales.
- **ManualResetEventSlim** – implementa *pausa/reanudación*; todos los hilos chequean este evento en cada iteración.
- **CancellationToken** – detención cooperativa de hilos.

#### Visualización:

- Kanban con 4 columnas: *Esperando*, *En Preparación*, *Listos*, *Entregados*.
- Indicadores individuales para cada hilo cliente y cocinero, con progreso de preparación.
- Barras visuales para semáforos y cola.

---

### Lectores – Escritores (Meseros – Gerente)

**Problema clásico**: múltiples *meseros* (lectores) consultan el menú simultáneamente, mientras que un *gerente* (escritor) lo modifica ocasionalmente. Se debe permitir la lectura concurrente pero **excluir la escritura durante cualquier lectura** y viceversa.

#### Mecanismo de sincronización:

- **ReaderWriterLockSlim** – permite `N` lectores simultáneos **o** 1 escritor exclusivo.
  - `EnterReadLock()` / `ExitReadLock()`
  - `EnterWriteLock()` / `ExitWriteLock()`
- Política `NoRecursion` para evitar *deadlocks*.

#### Visualización:

- Grid del menú con versión y quién lo modificó (resalta ítems alterados).
- Panel del gerente con estado (escribiendo, esperando, modificaciones recientes).
- Visualizador del `ReaderWriterLock` con contadores de lectores activos, lectores esperando, escritor activo y escritor esperando.
- Indicadores por mesero (leyendo, esperando, comprometido).

---

### Módulos de Ataque (Ingeniería Social)

Ambos ataques son **simulaciones educativas** que **no ejecutan código malicioso real**, sino que modifican el estado de la simulación para mostrar vulnerabilidades.

#### Ataque sobre Productor‑Consumidor: *Inyección de Pedidos*

- **Vector**: un falso técnico de soporte convence al encargado para instalar una "actualización".
- **Consecuencias**: los `ProductorWorker` alteran, duplican o eliminan pedidos en la cola según probabilidades configuradas.
- **Políticas preventivas**: control de acceso físico, MFA, capacitación, registros de auditoría, integridad de datos, EDR, backups.

#### Ataque sobre Lectores‑Escritores: *Phishing al Gerente*

- **Vector**: correo falso que simula provenir del sistema de gestión; el gerente entrega sus credenciales en una página falsa.
- **Consecuencias**: el menú se modifica (precios triplicados, nombres falsos) y los meseros leen información comprometida.
- **Políticas preventivas**: filtros anti‑phishing, MFA, control de cambios, monitoreo de comportamiento, segmentación, versionado, concientización.

Ambos ataques incluyen un **panel de políticas** con explicaciones detalladas de la vulnerabilidad, el impacto y las medidas de implementación.

---

## Sincronización y Concurrencia en Detalle

| Primitiva              | Uso en RestauranteSO                                                                                   |
|------------------------|--------------------------------------------------------------------------------------------------------|
| `SemaphoreSlim`        | Control de capacidad de la cola (espacios libres / items disponibles). Soporta `WaitAsync()` y `Wait()` con timeout. |
| `ReaderWriterLockSlim` | Exclusión mutua en el menú. Permite múltiples lectores o un escritor. Previene *starvation* de escritores. |
| `ConcurrentQueue`      | Cola de pedidos thread‑safe. Operaciones `Enqueue`/`TryDequeue` lock‑free.                             |
| `ManualResetEventSlim` | Implementación de pausa/reanudación: `Reset()` cierra la "puerta" (bloquea), `Set()` la abre.         |
| `CancellationTokenSource` | Cancelación cooperativa de hilos: los workers chequean `IsCancellationRequested` en cada ciclo.       |
| `Interlocked`          | Operaciones atómicas para contadores globales (`Increment`, `Decrement`).                               |
| `volatile`             | Garantiza visibilidad del estado de ataque entre hilos sin caché local.                                 |
| `Thread` vs `Task`     | `Thread` para workers de larga duración con nombre visible; `Task` para lectores/escritores que usan `ThreadPool`. |

### Flujo típico del Productor
Productor:

Chequear pausa (ManualResetEventSlim.Wait)

SemEspacios.Wait() → si cola llena, bloquea

Crear pedido

Aplicar lógica de ataque (si activo)

Cola.Enqueue(pedido)

SemItems.Release() → despierta a un consumidor

Notificar UI

Thread.Sleep(velocidad)


---

### Flujo típico del Consumidor

onsumidor:

Chequear pausa

SemItems.Wait() → si cola vacía, bloquea

Cola.TryDequeue(out pedido)

Marcar pedido como EnPreparación

Simular preparación (progreso)

Marcar Listo → Entregado

SemEspacios.Release() → despierta a un productor

Notificar UI


---

## Guía de Uso

### Dashboard principal

- Se abre al ejecutar. Muestra **4 cards animadas**.
- Haz clic en cualquier card para abrir la simulación correspondiente.
- El panel lateral muestra estadísticas del sistema y estado de las simulaciones.

### Simulación Productor‑Consumidor

- Presiona **Iniciar** para comenzar.
- Usa **Pausar/Reanudar**, **Detener**.
- Agrega **clientes** o **cocineros** en tiempo real.
- Ajusta la velocidad de producción y consumo con los sliders.
- El botón **Activar Ataque** simula la ingeniería social (aparecerá un diálogo).

### Simulación Lectores‑Escritores

- Similar: **Iniciar**, **Pausar**, **Detener**.
- Agrega **meseros** (lectores).
- El gerente modifica el menú automáticamente cada cierto intervalo.
- El botón **Simular Phishing** activa el ataque sobre el menú.

### Módulos de Ataque (acceso directo desde las cards 3 y 4)

- Abren la simulación correspondiente con el ataque ya activo (incluyen narrativa y diálogos automáticos).

### Políticas de Prevención

- Se accede desde el panel de ataque visible.
- Muestra tres pestañas: *Políticas*, *Evidencia* y *Resumen Ejecutivo*.

### Volver al Inicio

- Toda simulación tiene un botón **Volver al Inicio** que cierra la ventana y restaura el dashboard.

---

## Conclusiones y Aprendizaje

**RestauranteSO** demuestra la aplicación práctica de conceptos fundamentales de **Sistemas Operativos** en un entorno visual y educativo:

1. **Concurrencia real** – el uso de hilos, semáforos y locks permite comprender la complejidad de la programación concurrente y la importancia de la sincronización para evitar *race conditions* y *deadlocks*.

2. **Arquitectura limpia** – la separación en capas facilita el mantenimiento y la extensibilidad. Se puede añadir una nueva simulación (ej. *Filósofos comensales*) simplemente implementando `ISimulationService` y registrándola en `AppSettings`.

3. **Ingeniería social** – los módulos de ataque educativos refuerzan la idea de que la seguridad no es solo técnica; el factor humano es el eslabón más débil. Las políticas de prevención ofrecen un enfoque integral.

4. **Diseño UI/UX** – la interfaz moderna con feedback en tiempo real mejora la experiencia de aprendizaje, haciendo visible el comportamiento de los hilos y las primitivas de sincronización.

5. **Iteración y mejora** – el rediseño completo de la UI (cards, fuentes, distribución) muestra la importancia de la retroalimentación visual para lograr una aplicación profesional y usable.

**Reflexión final**: este proyecto ha permitido integrar conocimientos de **programación concurrente**, **arquitectura de software**, **seguridad informática** y **diseño de interfaces**, resultando en una herramienta didáctica completa y funcional.
