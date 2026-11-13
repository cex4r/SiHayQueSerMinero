# MusicMiner

Minero de archivos MP3 escrito en **C# / .NET 8**. Recorre un directorio y sus subdirectorios, extrae metadatos musicales mediante **TagLib#** y registra la información en una base de datos **SQLite** existente. Se puede ejecutar de forma independiente desde la terminal o como proceso externo desde **MusicApp**.

> **Estado del proyecto:** implementación funcional básica. Los contadores de inserciones y duplicados todavía tienen limitaciones descritas más abajo.

## Requisitos

- .NET SDK 8 para compilar y ejecutar con `dotnet run`.
- Una base SQLite **ya creada**, con las tablas `rolas`, `performers` y `albums` compatibles con el esquema del proyecto.
- Acceso de lectura al directorio de música y permisos de escritura sobre la base de datos.

Dependencias NuGet declaradas en `MinerExecutable.csproj`:

| Paquete | Versión | Uso |
| --- | --- | --- |
| `Microsoft.Data.Sqlite` | `10.0.14` | Acceso a SQLite |
| `TagLibSharp` | `2.3.0` | Lectura de etiquetas del audio |

Los paquetes se restauran con `dotnet restore`.

## Estructura

```text
MusicMiner/
├── Program.cs                  # Argumentos, ejecución y salida
├── MinerExecutable.csproj
├── model/
│   ├── Search.cs               # Búsqueda recursiva de archivos .mp3
│   ├── Analyze.cs              # Lectura de etiquetas con TagLib#
│   ├── Song.cs                 # Modelo de metadatos
│   ├── AddDataBase.cs          # Consultas e inserciones en SQLite
│   └── MiningResult.cs         # Estadísticas y motivos de omisión
├── controller/
│   └── Miner.cs                # Orquestación del minado
└── view/
    └── Message.cs              # Resumen en la terminal
```

Las rutas y mayúsculas del árbol son ilustrativas: conserva los nombres de carpeta que tenga tu proyecto.

## Compilación

Desde la carpeta que contiene `MinerExecutable.csproj`:

```bash
dotnet restore
dotnet build
```

La compilación Debug en Linux genera normalmente un ejecutable llamado **`MinerExecutable`** dentro de `bin/Debug/net8.0/` (el nombre viene del proyecto, no de la carpeta).

## Uso desde terminal

```bash
dotnet run -- --db "/ruta/a/rolas.db" "/ruta/a/Musica"
```

Para obtener una respuesta JSON que pueda interpretar MusicApp:

```bash
dotnet run -- --db "/ruta/a/rolas.db" --json "/ruta/a/Musica"
```

También puedes invocar el ejecutable ya compilado:

```bash
./bin/Debug/net8.0/MinerExecutable \
  --db "/ruta/a/rolas.db" \
  --json "/ruta/a/Musica"
```

### Argumentos

| Argumento | Obligatorio | Descripción |
| --- | --- | --- |
| `--db <ruta>` | Sí | Ruta del archivo SQLite existente |
| `--json` | No | Emite un objeto JSON en lugar del resumen legible |
| `<directorio>` | Sí | Carpeta raíz en la que se buscarán archivos MP3 |

Las rutas pueden incluir espacios si se escriben entre comillas. El programa comprueba que el archivo de base de datos y el directorio existan antes de iniciar.

### Salida normal

```text
Minado terminado.

Archivos encontrados: 6
Canciones registradas: 1
Canciones omitidas: 5

Motivo:
- ejemplo.mp3: Metadatos obligatorios incompletos.
```

Los valores son ilustrativos.

### Salida JSON

```json
{
  "found": 6,
  "inserted": 1,
  "duplicates": 0,
  "skipped": 5,
  "failed": 0,
  "reasons": ["ejemplo.mp3: Metadatos obligatorios incompletos."]
}
```

**Importante:** actualmente `inserted` toma el valor de `MiningResult.Registered`, que cuenta los archivos procesados satisfactoriamente, **incluidos los que ya existían**. Los campos `duplicates` y `failed` están establecidos en `0` en `Program.cs`; todavía no son mediciones reales. No uses esos valores como estadísticas definitivas hasta adaptar el resultado de `GetOrCreateSong`.

## Funcionamiento

1. **Search:** enumera archivos `*.mp3` recursivamente. Ignora directorios inaccesibles y busca la extensión sin distinguir mayúsculas y minúsculas.
2. **Analyze:** abre cada MP3 con TagLib# y construye un objeto `Song` con título, intérpretes, álbum, artistas del álbum, año, pista, géneros y ruta.
3. **Miner:** omite archivos cuyo título, álbum o primer artista esté vacío. Registra el motivo y continúa con el siguiente archivo.
4. **AddDataBase:** busca una canción existente por su `path`; si no existe, busca o crea el intérprete y el álbum, y luego inserta la canción en `rolas`.
5. **Message / JSON:** presenta el resumen del trabajo realizado.

La inserción de cada canción usa **una conexión y una transacción SQLite**, compartidas por la búsqueda o creación del intérprete y del álbum. No se mantiene una única conexión durante todo el lote.

## Integración con MusicApp

MusicMiner es un ejecutable independiente. MusicApp puede iniciarlo como un proceso externo, indicando la ruta real de `MinerExecutable`, la base compartida y el directorio seleccionado por el usuario.

Ejemplo de ejecución externa:

```bash
"/ruta/a/MusicMiner/bin/Debug/net8.0/MinerExecutable" \
  --db "$HOME/.local/share/MusicApp/rolas.db" \
  --json \
  "$HOME/Musica"
```

**MusicApp y MusicMiner deben apuntar al mismo archivo `.db`.** El archivo `rolas.sql` es el script de creación del esquema, no la base de datos de trabajo.

## Verificación en SQLite

Consulta las canciones insertadas:

```bash
sqlite3 "/ruta/a/rolas.db" \
  "SELECT id_rola, title, path FROM rolas ORDER BY id_rola DESC LIMIT 20;"
```

Comprueba si hay rutas repetidas:

```bash
sqlite3 "/ruta/a/rolas.db" \
  "SELECT path, COUNT(*) FROM rolas GROUP BY path HAVING COUNT(*) > 1;"
```

La aplicación intenta evitar duplicar una **misma ruta literal**; dos copias de una canción en rutas distintas pueden registrarse como canciones diferentes. Para garantizar unicidad incluso ante inserciones simultáneas, sería conveniente una restricción `UNIQUE` sobre `rolas.path`, previa revisión de los registros existentes.

## Casos de prueba recomendados

- MP3 con etiquetas ID3v2.3 completas.
- MP3 con etiquetas ID3v2.4 completas.
- MP3 sin título, álbum o artista: debe omitirse con una explicación.
- MP3 dañado o ilegible: debe registrar el error y continuar con otros archivos.
- Dos canciones del mismo intérprete y álbum: deben reutilizar los IDs existentes.
- Ejecutar dos veces sobre la misma carpeta: no debería insertar dos veces la misma ruta.
- Buscar las nuevas canciones desde MusicApp sobre la **misma** base SQLite.

## Limitaciones conocidas y mejoras pendientes

- `Registered` no distingue inserciones nuevas de canciones existentes. `duplicates` y `failed` en JSON aún son valores provisionales.
- `Miner.Mina` captura errores generales y los agrega a `Errors`; `Program.Main` puede devolver el código `0` aunque haya ocurrido un fallo durante el recorrido. Conviene añadir un estado explícito de error fatal.
- Se conserva únicamente el **primer intérprete** y el **primer género** al insertar en SQLite, aunque el objeto `Song` pueda contener varios.
- El álbum se identifica usando el nombre y la ruta de su carpeta; esto es una convención del proyecto.
- La búsqueda de canciones existentes depende de la coincidencia exacta de la ruta, sin detectar copias idénticas en ubicaciones diferentes.
- No se utiliza información biográfica de `persons`, `groups` o `in_group` durante el minado.
- Los archivos con metadatos obligatorios ausentes se omiten; no se les inventan etiquetas a partir del nombre del archivo.

## Notas de desarrollo

El proyecto usa una organización similar a MVC: el modelo realiza búsqueda, lectura y persistencia; `Miner` coordina el proceso; `Message` muestra el resultado. La salida `--json` permite integrar el minero sin depender de los mensajes de consola legibles para personas.
