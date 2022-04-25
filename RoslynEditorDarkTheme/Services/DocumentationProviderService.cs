using System.Collections.Concurrent;
using System.IO;
using Microsoft.CodeAnalysis;
using System.Runtime.InteropServices;
using System;
using Microsoft.CodeAnalysis.Shared.Utilities;
using System.Linq;
using Microsoft.CodeAnalysis.Host;

namespace RoslynEditorDarkTheme.Services
{
    internal sealed class DocumentationProviderService : IDocumentationProviderService
    {
        #region Fields

        private static readonly Lazy<(string assemblyPath, string docPath)> _referenceAssembliesPath = 
            new(GetReferenceAssembliesPath);

        private readonly ConcurrentDictionary<string, DocumentationProvider> _assemblyPathToDocumentationProviderMap 
            = new();
        #endregion

        #region Properties

        public static (string assemblyPath, string docPath) ReferenceAssembliesPath => _referenceAssembliesPath.Value;
        #endregion

        #region Public Methods

        public DocumentationProvider GetDocumentationProvider(string location)
        {
            string finalPath = Path.ChangeExtension(location, "xml");

            return _assemblyPathToDocumentationProviderMap.GetOrAdd(location,
                _ =>
                {
                    if (!File.Exists(finalPath))
                        finalPath = GetFilePath(ReferenceAssembliesPath.docPath, finalPath) ??
                        GetFilePath(ReferenceAssembliesPath.assemblyPath, finalPath);

                    return finalPath == null ? null : XmlDocumentationProvider.CreateFromFile(finalPath);
                });
        }
        #endregion

        #region Private Methods

        private static string GetFilePath(string path, string location)
        {
            if (path != null)
            {
                // ReSharper disable once AssignNullToNotNullAttribute
                var referenceLocation = Path.Combine(path, Path.GetFileName(location));
                if (File.Exists(referenceLocation))
                    return referenceLocation;
            }

            return null;
        }

        private static (string assemblyPath, string docPath) GetReferenceAssembliesPath()
        {
            string assemblyPath = null;
            string docPath = null;

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                // all NuGet
                return (assemblyPath, docPath);

            var programFiles = Environment.GetEnvironmentVariable("ProgramFiles(x86)");

            if (string.IsNullOrEmpty(programFiles))
                programFiles = Environment.GetEnvironmentVariable("ProgramFiles");

            if (string.IsNullOrEmpty(programFiles))
                return (assemblyPath, docPath);

            var path = Path.Combine(programFiles, @"Reference Assemblies\Microsoft\Framework\.NETFramework");
            if (Directory.Exists(path))
            {
                assemblyPath = IOUtilities.PerformIO(() => Directory.GetDirectories(path), Array.Empty<string>())
                    .Select(x => new { path = x, version = GetFxVersionFromPath(x) })
                    .OrderByDescending(x => x.version)
                    .FirstOrDefault(x => File.Exists(Path.Combine(x.path, "System.dll")))?.path;

                if (assemblyPath == null || !File.Exists(Path.Combine(assemblyPath, "System.xml")))
                    docPath = GetReferenceDocumentationPath(path);
            }

            return (assemblyPath, docPath);
        }

        private static string GetReferenceDocumentationPath(string path)
        {
            string docPath = null;

            var docPathTemp = Path.Combine(path, "V4.X");
            if (File.Exists(Path.Combine(docPathTemp, "System.xml")))
                docPath = docPathTemp;
            else
            {
                var localeDirectory = IOUtilities.PerformIO(() => Directory.GetDirectories(docPathTemp),
                    Array.Empty<string>()).FirstOrDefault();
                if (localeDirectory != null && File.Exists(Path.Combine(localeDirectory, "System.xml")))
                    docPath = localeDirectory;
            }

            return docPath;
        }

        private static Version GetFxVersionFromPath(string path)
        {
            var name = Path.GetFileName(path);
            if (name?.StartsWith("v", StringComparison.OrdinalIgnoreCase) == true)
            {
                if (Version.TryParse(name.Substring(1), out var version))
                    return version;
            }

            return new Version(0, 0);
        }
        #endregion
    }
}
