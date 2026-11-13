# MusicApp

Aplicación de escritorio (C# / .NET 8 / Avalonia 11) para administrar la base SQLite de música, con arquitectura MVC.
Usa `Microsoft.Data.Sqlite` con consultas parametrizadas, sin Entity Framework.

## Compilar y ejecutar en Fedora

```bash
sudo dnf install dotnet-sdk-8.0 sqlite

# 1. Crear la base de datos con el esquema (una sola vez)
mkdir -p ~/.local/share/MusicApp
sqlite3 ~/.local/share/MusicApp/rolas.db < rolas.sql

# 2. Compilar y ejecutar
cd proyecto2/MusicApp
dotnet restore
dotnet run
```

La app **no crea** la base de datos: si la ruta no existe, muestra un error de conexión.

### Configuración (prioridad: argumento, variable de entorno, valor por defecto)

| Qué | Argumento | Variable de entorno | Por defecto |
|---|---|---|---|
| Base de datos | `--db <ruta>` | `MUSICAPP_DB` | `~/.local/share/MusicApp/rolas.db` |
| Ejecutable de MusicMiner | `--miner <ruta>` | `MUSICMINER_PATH` | `MusicMiner/MusicMiner` junto a la app, o carpeta hermana `../MusicMiner/MusicMiner` |

Ejemplo: `dotnet run -- --db ~/Música/rolas.db --miner ../MusicMiner/bin/Release/net8.0/MusicMiner`

### Publicar

```bash
dotnet publish MusicApp -c Release -o dist/MusicApp
dotnet publish MusicMiner -c Release -o dist/MusicMiner
```

Con las carpetas hermanas `dist/MusicApp` y `dist/MusicMiner`, la app encuentra el minero sin configuración.

## Contrato de línea de comandos que MusicApp espera de MusicMiner

```
MusicMiner --db <ruta_sqlite> [--json] <directorio_de_musica>
```

- Código de salida `0` si el minado terminó bien; distinto de `0` si falló (el mensaje va por stderr).
- Con `--json`, la **última línea de stdout** debe ser un objeto JSON con estos campos:

```json
{"found":6,"inserted":1,"duplicates":0,"skipped":5,"failed":0,"reasons":["5 archivos con metadatos obligatorios incompletos"]}
```

MusicApp trata como fallo un código `0` sin ese JSON, porque que el proceso termine no prueba que minó bien.

## Arquitectura

```
Model/       Song, Artist, Album, Find (lecturas), LibraryEditor (escrituras), Database (abre la conexión)
Controller/  Finder (FindGlobal), LibraryController (validación + altas/ediciones), MiningController (proceso externo),
             Result (resultado tipado con tipo de error), DbAccess (abre/cierra conexión y traduce excepciones)
View/        MainWindow (navegación inferior), HomeView, SearchView, LibraryView, AddMenuView,
             ArtistForm, SongForm, AlbumForm, EditView, MiningResultView, Styles.axaml
```

- Solo `Model` contiene SQL. Los controladores abren **una** conexión por operación y la cierran con `using`.
- Cada conexión se abre con `Mode=ReadWrite` (no crea archivos) y `Foreign Keys=True` (SQLite las trae apagadas).
- Las altas y ediciones usan transacciones. Una búsqueda sin resultados es un éxito con lista vacía; los fallos técnicos viajan en `Result.Error`/`Message`.

## Decisiones sobre el esquema y diferencias con el diseño

1. **`persons` no tiene llave foránea hacia `performers`.** Convención adoptada: `persons.id_person = performers.id_performer`. Si MusicMiner relaciona `persons` de otra forma, hay que ajustar esto antes de usar la edición de intérpretes: un `UPSERT` con esa convención podría sobrescribir una fila que no corresponda.
2. **Grupos:** el diseño solo define datos extra para Person. `groups` e `in_group` no se tocan, así que un intérprete de tipo Group se guarda solo en `performers`.
3. **Imágenes («Add picture»):** omitidas. El esquema no tiene dónde guardarlas.
4. **«Recent searches»:** omitido. Sin una tabla para el historial, no inventé persistencia. Las carátulas de las listas tampoco se muestran, por la misma razón que las imágenes.
5. **Canciones:** al guardar, el intérprete se busca por nombre (sin distinguir mayúsculas) y, si no existe, se crea como `Unknown`. El álbum vacío usa el álbum `0` («Unknown»). Dos canciones no pueden compartir la misma ruta.
6. **Intérprete Person:** el botón se llama *Next* y lleva a la pantalla de datos personales (*Add - Artist - Person*) antes de guardar.
7. **Edición:** se accede desde *Home → Edit*. Se busca por tipo (Artist, Song, Album), se selecciona el resultado y se modifica por su id real. El álbum `0` no es editable.
8. **Selección del directorio de música:** la hace la vista (selector de carpetas de Avalonia); `MiningController` recibe la ruta ya elegida.
9. Si cambias un intérprete de Person a otro tipo, la fila de `persons` se conserva para no perder datos.
