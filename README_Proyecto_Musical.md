# Music Database — MusicApp & MusicMiner

Sistema de **gestión de una biblioteca musical local**, desarrollado en **C# y .NET 8** como proyecto académico de Modelado y Programación. Combina una aplicación gráfica para consultar y administrar la información musical con un minero ejecutable independiente que recorre directorios, analiza archivos MP3 y registra sus metadatos en **SQLite**.

La base de datos almacena principalmente **metadatos y rutas de archivos**, no el audio de las canciones. Por eso, las búsquedas de la aplicación no requieren volver a abrir cada MP3.

## Componentes del proyecto

| Componente | Función |
| --- | --- |
| **MusicApp** | Interfaz gráfica para consultar la biblioteca, buscar canciones, artistas y álbumes, añadir o editar registros y solicitar un minado. |
| **MusicMiner** | Programa de consola independiente que descubre MP3 de manera recursiva, lee etiquetas ID3 mediante TagLib# y registra los datos en SQLite. Su ejecutable se llama **`MinerExecutable`**. |
| **SQLite** | Base de datos relacional compartida por los dos programas. |

### Flujo general

```text
Usuario
   |
   v
MusicApp (Avalonia UI)
   |                      \
   | búsqueda/edición      \ botón Mine
   v                        v
SQLite <---------------- MusicMiner (proceso externo)
                              |
                              v
                     Directorios con MP3
                              |
                              v
                     Etiquetas ID3 / TagLib#
```

**MusicApp y MusicMiner son proyectos diferentes.** MusicApp no incorpora el código del minero: inicia su ejecutable como un proceso externo y utiliza la misma base de datos para mostrar la biblioteca actualizada.

## Tecnologías

- **C# y .NET 8** para ambos programas.
- **Avalonia UI 11** para la interfaz de escritorio de MusicApp (compatible con Linux y otras plataformas admitidas por Avalonia).
- **SQLite** como base de datos.
- **Microsoft.Data.Sqlite** para ejecutar SQL parametrizado desde C#.
- **TagLibSharp** para leer los metadatos de archivos MP3.
- Organización basada en **MVC**: modelos, controladores y vistas.

## Organización esperada del repositorio

```text
proyecto2/
├── MusicApp/
│   ├── Model/          # Entidades y operaciones SQLite
│   ├── Controller/     # Búsquedas, edición e integración del minero
│   ├── View/           # Ventanas y formularios Avalonia
│   ├── MusicApp.csproj
│   └── rolas.sql       # Esquema para inicializar SQLite
│
└── MusicMiner/
    ├── model/          # Song, Search, Analyze, AddDataBase, MiningResult
    ├── controller/     # Miner
    ├── view/           # Message
    ├── Program.cs
    └── MinerExecutable.csproj
```

> Los nombres de carpetas pueden variar según cómo se haya organizado la copia local. Lo importante es mantener **los dos archivos `.csproj` como proyectos independientes**.

## Funcionalidades

### MusicApp

- Búsqueda por nombre de **canciones, intérpretes y álbumes**.
- Visualización de los registros almacenados en SQLite.
- Formularios para **agregar y editar** canciones, intérpretes y álbumes.
- Selección de una carpeta de música desde la interfaz para iniciar MusicMiner.
- Presentación del resultado del minado y actualización de la biblioteca.
- Validación de entradas y notificación de errores de acceso a datos.

La interfaz sigue los bocetos de `InterfazApp.pdf`, con tema oscuro y secciones **Home**, **Search** y **Your Library**.

### MusicMiner

1. Recibe una carpeta de origen y la ruta de una base SQLite existente.
2. Busca archivos `.mp3` en esa carpeta y sus subdirectorios.
3. Lee las etiquetas ID3 utilizando **TagLib#**.
4. Construye un objeto `Song` por archivo.
5. Comprueba que estén presentes **título, álbum y al menos un intérprete**.
6. Busca o crea los registros de intérprete y álbum necesarios; después consulta o inserta la canción.
7. Comunica un resumen de los archivos procesados, omitidos y sus motivos.

Los MP3 con metadatos obligatorios incompletos se omiten. Una canción inválida no debe impedir procesar las siguientes.

## Base de datos

El esquema se define en el archivo **`rolas.sql`**. Entre sus tablas se encuentran:

| Tabla | Contenido |
| --- | --- |
| `rolas` | Canciones: título, ruta, pista, año, género y referencias a álbum e intérprete. |
| `albums` | Álbumes: nombre, ruta y año. |
| `performers` | Intérpretes y su tipo. |
| `types` | Clasificación de intérpretes. |
| `persons` | Datos específicos de personas. |
| `groups` | Información de grupos musicales. |
| `in_group` | Relaciones entre personas y grupos. |

Las canciones se vinculan con los registros de `albums` y `performers` mediante identificadores. Una ruta MP3 es un dato del sistema de archivos; no debe confundirse con la ruta del archivo `.db`.

> **Importante:** `rolas.sql` es un **script de creación**. Los cambios y las inserciones de la aplicación se guardan en el archivo **`.db`**, no en el `.sql`. No vuelvas a ejecutar el script sobre una base ya inicializada.

## Instalación en Fedora

### Requisitos

- SDK de .NET 8.
- SQLite CLI para crear e inspeccionar la base.
- Acceso a los paquetes NuGet requeridos por cada proyecto.

Instalar las herramientas básicas:

```bash
sudo dnf install dotnet-sdk-8.0 sqlite
```

### 1. Preparar la base de datos (solo una vez)

Desde la carpeta donde esté `rolas.sql`:

```bash
mkdir -p "$HOME/.local/share/MusicApp"
sqlite3 "$HOME/.local/share/MusicApp/rolas.db" < rolas.sql
```

Este comando es **solo para una base de datos nueva**. Si ya existe, conserva el archivo y omite este paso.

### 2. Compilar MusicMiner

```bash
cd MusicMiner
dotnet restore
dotnet build
```

En la configuración de desarrollo utilizada, el ejecutable Linux se genera en:

```text
MusicMiner/bin/Debug/net8.0/MinerExecutable
```

### 3. Compilar y ejecutar MusicApp

Desde `MusicApp`:

```bash
dotnet restore
dotnet build
dotnet run -- \
  --db "$HOME/.local/share/MusicApp/rolas.db" \
  --miner "/ruta/completa/MusicMiner/bin/Debug/net8.0/MinerExecutable"
```

Sustituye `/ruta/completa/` por la ubicación real. **`--miner` apunta al archivo ejecutable**, no a la carpeta del proyecto ni al archivo `.csproj`.

MusicApp también admite la variable de entorno `MUSICMINER_PATH` para indicar la ubicación del minero, y `MUSICAPP_DB` para la base de datos, según su configuración actual.

## Uso de MusicMiner desde terminal

MusicMiner puede ejecutarse sin abrir MusicApp:

```bash
./bin/Debug/net8.0/MinerExecutable \
  --db "$HOME/.local/share/MusicApp/rolas.db" \
  "/ruta/a/la/carpeta/de/musica"
```

Para devolver un resultado estructurado a MusicApp, añade `--json`:

```bash
./bin/Debug/net8.0/MinerExecutable \
  --db "$HOME/.local/share/MusicApp/rolas.db" \
  --json \
  "/ruta/a/la/carpeta/de/musica"
```

La respuesta JSON contiene los campos `found`, `inserted`, `duplicates`, `skipped`, `failed` y `reasons`.

**Limitación de la versión actual:** `inserted` corresponde al contador `Registered` de operaciones de registro completadas; puede incluir canciones que ya existían. Los valores `duplicates` y `failed` se envían actualmente como `0` fijo y todavía no representan un conteo real. El código también puede devolver estado de salida `0` después de ciertos errores generales capturados dentro de `Miner`. Estas situaciones deben corregirse para considerar fiable el reporte de integración.

## Consultar la base de datos

Para verificar los registros desde Fedora:

```bash
sqlite3 "$HOME/.local/share/MusicApp/rolas.db"
```

Dentro del intérprete SQLite:

```sql
.headers on
.mode column
SELECT id_rola, title, path FROM rolas;
SELECT id_performer, name FROM performers;
SELECT id_album, name FROM albums;
PRAGMA foreign_key_check;
```

Para investigar rutas duplicadas:

```sql
SELECT path, COUNT(*) AS total
FROM rolas
GROUP BY path
HAVING COUNT(*) > 1;
```

Usa una **copia** de la base de datos para pruebas de inserción o edición. No elimines registros duplicados sin verificar antes sus rutas y relaciones.

## Pruebas recomendadas

- Abrir MusicApp, navegar por las pantallas y realizar búsquedas con y sin resultados.
- Agregar y editar un artista, un álbum y una canción, y comprobar los cambios con `SELECT`.
- Minar una carpeta con MP3 con etiquetas completas.
- Probar MP3 con metadatos incompletos y archivos corruptos para verificar la omisión y el manejo de errores.
- Probar etiquetas **ID3v2.2, ID3v2.3 e ID3v2.4** con archivos preparados para cada versión.
- Ejecutar dos veces el minado del mismo directorio y comprobar que no se creen registros duplicados.
- Verificar que MusicApp y MusicMiner apuntan al **mismo archivo SQLite**.
- Ejecutar MusicApp con una ruta de minero incorrecta para comprobar el mensaje de error.

## Limitaciones conocidas y trabajo pendiente

- El control de duplicados se basa principalmente en la **ruta del archivo**. La lógica `GetOrCreateSong()` consulta esa ruta antes de insertar, pero se debe comprobar por qué se observaron duplicados durante las pruebas de integración. Un índice `UNIQUE` sobre una ruta normalizada podría reforzar la protección, previa limpieza de duplicados existentes y si el esquema puede modificarse.
- Los contadores del modo JSON no distinguen todavía de manera fiable entre canciones nuevas, existentes y fallidas.
- MusicMiner almacena el **primer intérprete** y **primer género** al poblar el esquema actual; no todas las etiquetas múltiples tienen representación relacional completa.
- La ruta de álbum se infiere de la carpeta que contiene el MP3; esto es una convención práctica, no una identificación universal de álbumes.
- No se han implementado las imágenes de artistas o portadas en MusicApp porque el esquema compartido no define su almacenamiento.
- El historial persistente de búsquedas tampoco forma parte de la implementación documentada.
- MusicApp es un **gestor de metadatos**; no se afirma aquí que reproduzca audio.

## Propósito académico

El proyecto permite practicar **programación orientada a objetos, separación de responsabilidades, patrón MVC, manejo de archivos, bibliotecas externas, SQL, claves foráneas, transacciones y comunicación entre procesos**. MusicMiner y MusicApp se diseñaron para funcionar de forma independiente y colaborar por medio de SQLite y una interfaz de línea de comandos.

---

Documentación general del proyecto. Para detalles de implementación específicos, consulta los README de cada subproyecto.
