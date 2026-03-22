# 🏋️ Anotador Gym App - .NET MAUI

[![.NET MAUI](https://img.shields.io/badge/.NET%20MAUI-8.0-blueviolet)](https://dotnet.microsoft.com/apps/maui)
[![C#](https://img.shields.io/badge/C%23-12.0-green)](https://docs.microsoft.com/dotnet/csharp/)
![EF Core](https://img.shields.io/badge/EF%20Core-8.0-orange)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql)
![Docker](https://img.shields.io/badge/Docker-%E2%9C%94-2496ED?logo=docker)
![Render](https://img.shields.io/badge/Render-deployed-46E3B7?logo=render)
[![GitHub stars](https://img.shields.io/github/stars/lisandrosemperez-collab/AnotadorGym)](https://github.com/lisandrosemperez-collab/AnotadorGym/stargazers)

<div align="center">
  <img src="https://raw.githubusercontent.com/lisandrosemperez-collab/AnotadorGym/main/screenshots/Splash.jpeg" width="300">
  <br>
  <em>Splash screen de la aplicación</em>
</div>

## 🌟 Descripción General

Anotador Gym es un ecosistema completo de tracking de entrenamientos que combina una aplicación móvil nativa multiplataforma (desarrollada con .NET MAUI) con una API RESTful backend desplegada en la nube. La app funciona offline-first y sincroniza datos con la API cuando hay conectividad, permitiendo una experiencia fluida en cualquier contexto.

## 🎯 Objetivo del Proyecto

Desarrollar una aplicación completa de seguimiento de entrenamientos aplicando buenas prácticas de arquitectura, persistencia de datos y sincronización con backend, simulando un entorno de desarrollo profesional real.

- 🔗 [App Móvil (este repositorio)](https://github.com/lisandrosemperez-collab/AnotadorGym)
- 🔗 [API Backend](https://github.com/lisandrosemperez-collab/AnotadorGymAppApi)
- 🔗 [API en producción](https://anotadorgymappapi.onrender.com)

### Interfaz Principal
| Tema Claro | Tema Oscuro |
| :---: | :---: |
| <img src="https://raw.githubusercontent.com/lisandrosemperez-collab/AnotadorGym/main/screenshots/MainLightTheme.jpeg" width="250"> | <img src="https://raw.githubusercontent.com/lisandrosemperez-collab/AnotadorGym/main/screenshots/MainDarkTheme.jpeg" width="250" > |

### Funcionalidades Clave
| Gestión Rutinas | Seguimiento | Gráficos | Configuración |
| :---: | :---: | :---: | :---: |
| <img src="https://raw.githubusercontent.com/lisandrosemperez-collab/AnotadorGym/main/screenshots/Rutines.jpeg" width="250"> | <img src="https://raw.githubusercontent.com/lisandrosemperez-collab/AnotadorGym/main/screenshots/ChartsViews.jpeg" width="250" > | <img src="https://raw.githubusercontent.com/lisandrosemperez-collab/AnotadorGym/main/screenshots/ChartsViews1.jpeg" width="250" > | <img src="https://raw.githubusercontent.com/lisandrosemperez-collab/AnotadorGym/main/screenshots/Config.jpeg" width="250"> |

## 🏗️ **Arquitectura del Proyecto**

La aplicación sigue una **arquitectura limpia** con separación física en dos proyectos: la **capa de presentación (UI)** y la **capa de dominio/datos** (biblioteca de clases). Esto garantiza una dependencia unidireccional y facilita el mantenimiento y las pruebas.

```csharp
AnotadorGymApp/                          # Proyecto principal .NET MAUI
│
├── ConfiguracionPage/                   # Página de ajustes
├── MainPage/                            # Página principal y splash screen
├── MetricasPage/                        # Página de métricas y gráficos
├── Platforms/                           # Código específico por plataforma
├── PopUp/                               # Diálogos modales reutilizables
├── Resources/                           # Recursos (imágenes, fuentes, estilos, JSON)
│   ├── Fonts/
│   ├── Images/
│   ├── Raw/                             # Datos semilla en JSON
│   └── Styles/                          # Temas dinámicos
├── RutinasPage/                         # Gestión de rutinas
└── Services/                            # Servicios de aplicación
    ├── AppInitPersistence/              # Estado e inicialización de la app
    └── RegistroEjercicios/              # Lógica de métricas
```
## 📁 Estructura de la Biblioteca de Datos (AnotadorGymApp.Data)

```csharp
AnotadorGymApp.Data/                     # Dominio + persistencia
│
├── DataBase/
│   ├── DataBaseContext.cs
│   ├── Initialization/
│   │   └── DbInitializer.cs
│   └── Services/                        # Repositorios
│       ├── BulkInsertScope.cs
│       ├── EjercicioService.cs
│       ├── RegistrosService.cs
│       └── RutinaService.cs
│
├── Initialization/
│   └── Importers/
│       ├── Abstractions/
│       │   └── IDataImporter.cs
│       ├── DebugDiaEntrenamientosPrueba.cs
│       ├── EjercicioImporter.cs
│       └── RutinaImporter.cs
│
├── Migrations/
├── Models/
│   ├── DTOs/
│   ├── Entities/
│   ├── Interfaces/                      # Interfaces de dominio
│   ├── Results/                         # Respuestas de API
│   └── Sources/
```

## 🔁 Flujo de Datos

- **Inicio de la app:** SplashPage muestra el progreso mientras se inicializa la base de datos, se cargan datos semilla (JSON) y se sincroniza con la API si hay conexión.
- **Acceso a datos:** Las vistas consumen servicios de aplicación (Services) que coordinan la lógica de negocio y delegan operaciones a los servicios de la capa de datos (repositorios basados en Entity Framework Core).
- **Persistencia:** La capa de datos utiliza un DbContext central para manejar almacenamiento local en SQLite, incluyendo migraciones y optimizaciones.
- **Sincronización:** La app implementa un enfoque *offline-first*, permitiendo trabajar sin conexión y sincronizando con la API REST cuando hay disponibilidad de red.

## 🌐 Backend API (Repositorio Separado)
La API está desarrollada en .NET 9 y desplegada en Render utilizando contenedores Docker. Proporciona un catálogo de más de 900 ejercicios con sus relaciones musculares, y endpoints protegidos con JWT para importación/validación.

- 🔗 [Repositorio](https://github.com/lisandrosemperez-collab/AnotadorGymAppApi)
- 🔗 [Documentación Swagger en vivo](https://anotadorgymappapi.onrender.com)

⚙️ Características Técnicas de la API
- **Deploy en Render:** la API está desplegada en un entorno cloud con soporte para contenedores Docker, permitiendo escalabilidad y despliegues automatizados.
- **.NET 9** con controladores y servicios.
- **Autenticación JWT** para endpoints de importación/validación.
- **Entity Framework Core** con PostgreSQL (Neon).
- **Importación masiva** desde JSON con validaciones.
- **Documentación Swagger** interactiva.
- **Rendimiento optimizado:** endpoint principal sirve 900+ ejercicios en 3.65s (tiempo total desde cliente) con procesamiento interno de 2ms.

## ✨ Características Principales (App)

### 🏗️ Arquitectura y Diseño
- Clean Architecture con separación física de capas (UI + Data).
- Patrón MVVM con data binding y comandos.
- Inyección de dependencias manual en App.xaml.cs.
- Repository Pattern implementado en la capa de datos mediante servicios especializados.
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
| **Backend** | .NET 9, ASP.NET Core, Entity Framework Core, JWT, Swagger, PostgreSQL (Neon), Docker, Render |
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
- **API REST** con autenticación JWT y despliegue en Render.
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
- **💡 Sugerir una funcionalidad:** [Iniciar una Discusión](https://github.com/lisandrosemperez-collab/AnotadorGym/discussions)

⭐ **Si este proyecto te resulta útil o interesante, ¡considera darle una estrella en GitHub!**
