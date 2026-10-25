//Esta clase tiene los dos siguientes propositos:
// 1. Encontrar archivos en los directorios/subdirectorios.
// 2. Filtrar los archivos correctos para poder analizarlos.

using System;
using System.IO;
using System.Collections.Generic;

class Search
{
    public IEnumerable<string> FindMp3Files(string sourceDirectory)
    {
        var options = new EnumerationOptions
        {
            RecurseSubdirectories = true,
            IgnoreInaccessible = true,
            MatchCasing = MatchCasing.CaseInsensitive
        };

        return Directory.EnumerateFiles(
            sourceDirectory,
            "*.mp3",
            options
        );
    }
}