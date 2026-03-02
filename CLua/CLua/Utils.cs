using System;
using System.Collections.Generic;
using System.Text;

namespace CLua
{
    public static class Utils
    {
        public static string FindFileCaseInsensitive(string relativePath, bool checkOutSide = true)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;

            // Security check
            string fullPath = Path.GetFullPath(Path.Combine(basePath, relativePath));
            if (checkOutSide && !fullPath.StartsWith(basePath))
                throw new UnauthorizedAccessException("File access outside of application folder is forbidden!");


            string[] parts = relativePath.Replace('\\', '/').Split('/');
            string current = basePath;

            List<string> resolvedParts = new List<string>();


            foreach (string part in parts)
            {
                //if (!Directory.Exists(current))
                //    return relativePath.Replace('\\', '/'); ; // fallback

                if (!Directory.Exists(current))
                {
                    // Pokud aktuální cesta neexistuje, přidej zbytek bez změny
                    // When directory does not exist, adding remaining parts without case correction");
                    resolvedParts.Add(part);
                    current = Path.Combine(current, part);
                    continue;
                }

                var matches = Directory.EnumerateFileSystemEntries(current)
                    .Where(e => string.Equals(Path.GetFileName(e), part, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (matches.Count > 1)
                    Console.WriteLine($"Warning: multiple case variants found for '{part}', using first match");

                if (!matches.Any())
                {
                    // Pokud nenalezen žádný, přidej zbytek bez změny
                    // When no match found, adding remaining parts without case correction");
                    resolvedParts.Add(part);
                    current = Path.Combine(current, part);
                    continue;
                }

                string matchedName = Path.GetFileName(matches.First());
                resolvedParts.Add(matchedName); // ← PŘIDEJ - ukládej opravenou část
                current = matches.First();
            }

            return string.Join('/', resolvedParts); //Path.GetRelativePath(basePath, current).Replace('\\', '/');
        }
    }
}
