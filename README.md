# 🏋️ Anotador Gym App - .NET MAUI

[![.NET MAUI](https://img.shields.io/badge/.NET%20MAUI-8.0-blueviolet)](https://dotnet.microsoft.com/apps/maui)
[![C#](https://img.shields.io/badge/C%23-12.0-green)](https://docs.microsoft.com/dotnet/csharp/)
![EF Core](https://img.shields.io/badge/EF%20Core-8.0-orange)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql)
![Docker](https://img.shields.io/badge/Docker-%E2%9C%94-2496ED?logo=docker)
![Railway](https://img.shields.io/badge/Railway-deployed-0B0D0E?logo=railway)
[![GitHub stars](https://img.shields.io/github/stars/lisandrosemperez-collab/AnotadorGymApp)](https://github.com/lisandrosemperez-collab/AnotadorGymApp/stargazers)

<div align="center">
  <img src="https://raw.githubusercontent.com/lisandrosemperez-collab/AnotadorGymApp/master/screenshots/Splash.jpeg" width="300">
  <br>
  <em>Splash screen de la aplicación</em>
</div>

## 🌟 Descripción General

Anotador Gym es un ecosistema completo de tracking de entrenamientos que combina una aplicación móvil nativa multiplataforma (desarrollada con .NET MAUI) con una API RESTful backend desplegada en la nube. La app funciona offline-first y sincroniza datos con la API cuando hay conectividad, permitiendo una experiencia fluida en cualquier contexto.


- 🔗 [App Móvil (este repositorio)](https://github.com/lisandrosemperez-collab/AnotadorGymApp)
- 🔗 [API Backend](https://github.com/lisandrosemperez-collab/AnotadorGymAppApi)
- 🔗 [API en producción](https://anotadorgymappapi-production.up.railway.app/)

### Interfaz Principal
| Tema Claro | Tema Oscuro |
| :---: | :---: |
| <img src="https://raw.githubusercontent.com/lisandrosemperez-collab/AnotadorGymApp/master/screenshots/MainLightTheme.jpeg" width="250"> | <img src="https://raw.githubusercontent.com/lisandrosemperez-collab/AnotadorGymApp/master/screenshots/MainDarkTheme.jpeg" width="250" > |

### Funcionalidades Clave
| Gestión Rutinas | Seguimiento | Gráficos | Configuración |
| :---: | :---: | :---: | :---: |
| <img src="https://raw.githubusercontent.com/lisandrosemperez-collab/AnotadorGymApp/master/screenshots/Rutines.jpeg" width="250"> | <img src="https://raw.githubusercontent.com/lisandrosemperez-collab/AnotadorGymApp/master/screenshots/ChartsViews.jpeg" width="250" > | <img src="https://raw.githubusercontent.com/lisandrosemperez-collab/AnotadorGymApp/master/screenshots/ChartsViews1.jpeg" width="250" > | <img src="https://raw.githubusercontent.com/lisandrosemperez-collab/AnotadorGymApp/master/screenshots/Config.jpeg" width="250"> |

## 🏗️ **Arquitectura del Proyecto**

La aplicación sigue una **arquitectura limpia** con separación física en dos proyectos: la **capa de presentación (UI)** y la **capa de dominio/datos** (biblioteca de clases). Esto garantiza una dependencia unidireccional y facilita el mantenimiento y las pruebas.

```csharp
AnotadorGymApp/                          # Proyecto principal .NET MAUI
│
├── ConfiguracionPage/                    # Página de ajustes
├── MainPage/                              # Página principal y splash screen
├── MetricasPage/                          # Página de gráficos y estadísticas
├── Platforms/                             # Código específico por plataforma
├── PopUp/                                  # Diálogos modales reutilizables
├── RegistroEjercicios/                     # Servicios y modelos para métricas
├── Resources/                               # Imágenes, fuentes, estilos, JSON
│   ├── Fonts/
│   ├── Images/
│   ├── Raw/                                 # Archivos JSON de datos semilla
│   └── Styles/                              # Temas claro/oscuro dinámicos
├── RutinasPage/                             # Gestión de rutinas
└── Services/                                 # Servicios de aplicación (ConfigService, ImagenPersistenteService, etc.)
```
## 📁 Estructura de la Biblioteca de Datos (AnotadorGymApp.Data)

```csharp
AnotadorGymApp.Data/                        # Librería de clases (dominio + persistencia)
│
├── DataBase/
│   ├── DataBaseContext.cs                   # DbContext de EF Core
│   ├── Initialization/                       # Inicializador de BD
│   │   └── DbInitializer.cs
│   └── Services/                             # Repositorios (EjercicioService, RutinaService, etc.)
│       ├── BulkInsertScope.cs                 # Optimización para inserciones masivas
│       ├── EjercicioService.cs
│       ├── RegistrosService.cs
│       └── RutinaService.cs
│
├── Initialization/                           # Importación de datos semilla
│   └── Importers/
│       ├── Abstractions/
│       │   └── IDataImporter.cs               # Interfaz para importadores
│       ├── DebugDiaEntrenamientosPrueba.cs
│       ├── EjercicioImporter.cs
│       └── RutinaImporter.cs
│
├── Migrations/                                # Migraciones automáticas de EF Core
├── Models/
│   ├── DTOs/                                   # Objetos de transferencia de datos
│   ├── Entities/                                # Entidades de dominio (Ejercicio, Muscles, Rutinas)
│   └── Sources/                                  # Fuentes de datos (EjerciciosSource, RutinasSource)
```

## 🔁 Flujo de Datos
- **Inicio de la app:** SplashPage muestra progreso mientras se inicializa la BD y se cargan datos semilla (desde JSON y API).
- **Acceso a datos:** Las vistas consumen servicios (EjercicioService, etc.) que operan sobre el DataBaseContext.
- **Sincronización:** La app puede consumir la API REST para obtener actualizaciones del catálogo de ejercicios y enviar datos locales cuando hay conexión (modo offline-first).

## 🌐 Backend API (Repositorio Separado)
La API está desarrollada en .NET 9 y desplegada en Railway con Docker. Proporciona un catálogo de más de 900 ejercicios con sus relaciones musculares, y endpoints protegidos con JWT para importación/validación.

- 🔗 [Repositorio](https://github.com/lisandrosemperez-collab/AnotadorGymAppApi)
- 🔗 [Documentación Swagger en vivo](https://anotadorgymappapi-production.up.railway.app/)

⚙️ Características Técnicas de la API
- **.NET 9** con controladores y servicios.
- **Autenticación JWT** para endpoints de importación/validación.
- **Entity Framework Core** con PostgreSQL (Neon).
- **Importación masiva** desde JSON con validaciones.
- **Documentación Swagger** interactiva.
- **Contenerización con Docker** y despliegue continuo a Railway.
- **Rendimiento optimizado:** endpoint principal sirve 900+ ejercicios en 3.65s (tiempo total desde cliente) con procesamiento interno de 2ms.

## ✨ Características Principales (App)

### 🏗️ Arquitectura y Diseño
- Clean Architecture con separación física de capas (UI + Data).
- Patrón MVVM con data binding y comandos.
- Inyección de dependencias manual en App.xaml.cs.
- Repository Pattern centralizado en servicios.
- Entity Framework Core 8 (Code-First, migraciones automáticas).

### 💾 Persistencia de Datos
- **SQLite** con **Entity Framework Core** para almacenamiento local
- **Migraciones automáticas** y manejo optimizado de esquema
- **Importación masiva eficiente** de ejercicios (1,000+ registros)
- **Modelo relacional completo**: ejercicios, músculos, rutinas, historial de entrenamientos.
- **Optimizaciones:** WAL mode, batching, caché en memoria (diccionarios O(1)).

### 🎨 Experiencia de Usuario
- **Temas claro/oscuro** dinámicos sin reiniciar la app.
- **Gráficos interactivos** con Microcharts para seguimiento visual.
- **Splash screen**  con progreso en tiempo real.

### 🔐 Seguridad y Sincronización
- **Arquitectura offline-first:** la app funciona sin conexión y sincroniza cuando hay red.
- **Consumo de API REST** con manejo de errores y reintentos.
- **Variables de entorno** para configuración segura (no aplica directamente en app, pero sí en API).

## 🛠️ Stack Tecnológico
| Categoría | Tecnologías |
|-----------|-------------|
| **Backend** | .NET 9, ASP.NET Core, Entity Framework Core, JWT, Swagger, PostgreSQL (Neon), Docker, Railway |
| **Frontend Móvil** | .NET MAUI, C#, XAML, MVVM, Microcharts, SQLite, Entity Framework Core |
| **Arquitectura** | Clean Architecture, Repository Pattern, Dependency Injection, Offline-first |
| **Herramientas** | Visual Studio 2022, Git, GitHub, GitHub Actions (básico) |

## 🚀 Cómo Ejecutar el Proyecto

### Prerrequisitos
- **Visual Studio 2022** (con carga de trabajo ".NET Multi-platform App UI development")
- **.NET 8.0 SDK** o superior
- **Dispositivo Android/iOS** o emulador

### Instalación
```bash
# 1. Clonar el repositorio
git clone https://github.com/lisandrosemperez-collab/AnotadorGymApp.git
cd AnotadorGymApp

# 2. Abrir en Visual Studio
# 3. Seleccionar plataforma objetivo (Android/iOS)
# 4. Compilar y ejecutar (F5)
``` 

### Configuración Inicial
La aplicación creará automáticamente la base de datos SQLite y cargará los datos iniciales:
- Ejercicios: desde la API (900+ precargados).
- Rutinas y otros datos: desde archivos JSON en `Resources/Raw/`.

## ⚙️ Decisiones Técnicas Destacadas

## 📈 Roadmap y Mejoras Futuras
### ✅ Completado
- **App MAUI** con funcionalidades básicas y gráficos.
- **API REST** con autenticación JWT y despliegue en Railway.
- **Base de datos PostgreSQL** con 900+ ejercicios precargados.
- **Sincronización** básica entre app y API.

### 🚧 En Progreso
- [ ] Sincronización bidireccional completa (offline-first robusto).
- [ ] Sistema de caché avanzado en la app.
- [ ] Tests unitarios en API y app.

## ✉️ Soporte y Contacto

Este proyecto es mantenido activamente por **Lisandro Semperez**.

- **📫 LinkedIn:** [LinkedIn](https://www.linkedin.com/in/lisandro-semperez-24b1782b8/)
- **📫 📧 Email:** [Email](mailto:lisandrosemperez@gmail.com)
- **🐛 Reportar un problema:** [Abrir un Issue](https://github.com/lisandrosemperez-collab/AnotadorGymApp/issues)
- **💡 Sugerir una funcionalidad:** [Iniciar una Discusión](https://github.com/lisandrosemperez-collab/AnotadorGymApp/discussions)

⭐ **Si este proyecto te resulta útil o interesante, ¡considera darle una estrella en GitHub!**
