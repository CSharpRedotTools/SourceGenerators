using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

/*
    Source Generators are separate libraries from the main Godot project.
    They target the .netstandard2.0 framework and are Class Libraries,
    not Console Applications. They should be in their own separate projects.

    To loop through files from the main project, add the following to the
    main project's .csproj file:

    <ItemGroup>
        <AdditionalFiles Include="Scenes\Prefabs\**\*.tscn" />
    </ItemGroup>

    Additionally, include the following to link the source generator:

    <ItemGroup>
        <ProjectReference Include="..\SourceGenerators\MySourceGenerator\MySourceGenerator.csproj"
            OutputItemType="Analyzer"
            ReferenceOutputAssembly="false" />
    </ItemGroup>

    For debugging the output of the source generator, in the main project in
    VS2022 Solution Explorer, navigate to Dependencies > Analyzers >
    MySourceGenerator > ... > Prefabs.g.cs.

    Note: The entire VS2022 IDE must be closed and re-opened if the source
    generator is rebuilt.

    To avoid this inconvenience, add the following to the main .csproj file:

    <PropertyGroup>
        <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
        <CompilerGeneratedFilesOutputPath>Generated</CompilerGeneratedFilesOutputPath>
    </PropertyGroup>

    This will place the Prefabs.g.cs file in the Generated folder in the root
    folder of the main Godot project. You will not need to close and re-open
    VS2022 whenever the source generator and main Godot projects are rebuilt.

    Remember to delete the Generated/ folder and set <EmitCompilerGeneratedFiles>
    to false when you are done debugging the source generator output or you will
    run into a duplicate scripts error in the assembly later on.

    Always build the source generator project first, followed by the main
    Godot project.
*/

namespace MySourceGenerator
{
    [Generator]
    public class GameAssetsSrcGen : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            // No initialization required for this generator
        }

        public void Execute(GeneratorExecutionContext context)
        {
            // Get all additional text files
            IEnumerable<AdditionalText> tscnFiles = context.AdditionalFiles
                .Where(file => Path.GetExtension(file.Path)
                .Equals(".tscn", StringComparison.OrdinalIgnoreCase));

            char sep = Path.DirectorySeparatorChar;

            // Separate tscn files into Prefabs and Scenes
            IEnumerable<AdditionalText> prefabFiles = tscnFiles.Where(file => file.Path.Contains($"{sep}Prefabs{sep}"));
            IEnumerable<AdditionalText> sceneFiles = tscnFiles.Where(file => file.Path.Contains($"{sep}Scenes{sep}") && !file.Path.Contains($"{sep}Prefabs{sep}"));

            // Generate the Prefabs class
            string prefabSourceCode = GeneratePrefabsClass(context, prefabFiles);

            // Generate the Scenes class
            string sceneSourceCode = GenerateScenesClass(context, sceneFiles);

            // Add the generated source code to the compilation
            context.AddSource("Prefabs.g.cs", SourceText.From(prefabSourceCode, Encoding.UTF8));
            context.AddSource("Scenes.g.cs", SourceText.From(sceneSourceCode, Encoding.UTF8));
        }

        private static string GeneratePrefabsClass(GeneratorExecutionContext context, IEnumerable<AdditionalText> tscnFiles)
        {
            StringBuilder sb = new StringBuilder();
            string rootFolderName = GetRootFolderName(context);
            List<string> relativePaths = new List<string>();
            List<string> enumNames = new List<string>();

            foreach (AdditionalText file in tscnFiles)
            {
                string relativePath = GetRelativePath(file.Path, rootFolderName);
                string enumName = GetEnumName(relativePath, "Prefabs");

                relativePaths.Add(relativePath);
                enumNames.Add(enumName);
            }

            sb.AppendLine($"namespace {context.Compilation.AssemblyName};");
            sb.AppendLine();
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendLine("public enum Prefab");
            sb.AppendLine("{");

            for (int i = 0; i < enumNames.Count; i++)
            {
                sb.AppendLine($"    {enumNames[i]},");
            }

            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine("public static class MapPrefabsToPaths");
            sb.AppendLine("{");
            sb.AppendLine("    private static readonly Dictionary<Prefab, string> prefabPaths = new Dictionary<Prefab, string>");
            sb.AppendLine("    {");

            for (int i = 0; i < enumNames.Count; i++)
            {
                string resourcePath = $"res://{relativePaths[i]}";
                sb.AppendLine($"        {{ Prefab.{enumNames[i]}, \"{resourcePath}\" }},");
            }

            sb.AppendLine("    };");
            sb.AppendLine();
            sb.AppendLine("    public static string GetPath(Prefab prefab)");
            sb.AppendLine("    {");
            sb.AppendLine("        if (prefabPaths.TryGetValue(prefab, out string path))");
            sb.AppendLine("        {");
            sb.AppendLine("            return path;");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        return null;");
            sb.AppendLine("    }");

            sb.AppendLine("}");

            return sb.ToString();
        }

        private static string GenerateScenesClass(GeneratorExecutionContext context, IEnumerable<AdditionalText> tscnFiles)
        {
            StringBuilder sb = new StringBuilder();
            string rootFolderName = GetRootFolderName(context);
            List<string> relativePaths = new List<string>();
            List<string> enumNames = new List<string>();

            foreach (AdditionalText file in tscnFiles)
            {
                string relativePath = GetRelativePath(file.Path, rootFolderName);
                string enumName = GetEnumName(relativePath, "Scenes");

                relativePaths.Add(relativePath);
                enumNames.Add(enumName);
            }

            sb.AppendLine($"namespace {context.Compilation.AssemblyName};");
            sb.AppendLine();
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine();
            sb.AppendLine("public enum Scene");
            sb.AppendLine("{");

            for (int i = 0; i < enumNames.Count; i++)
            {
                sb.AppendLine($"    {enumNames[i]},");
            }

            sb.AppendLine("}");
            sb.AppendLine();

            sb.AppendLine("public static class MapScenesToPaths");
            sb.AppendLine("{");
            sb.AppendLine("    private static readonly Dictionary<Scene, string> scenePaths = new Dictionary<Scene, string>");
            sb.AppendLine("    {");

            for (int i = 0; i < enumNames.Count; i++)
            {
                string resourcePath = $"res://{relativePaths[i]}";
                sb.AppendLine($"        {{ Scene.{enumNames[i]}, \"{resourcePath}\" }},");
            }

            sb.AppendLine("    };");
            sb.AppendLine();
            sb.AppendLine("    public static string GetPath(Scene scene)");
            sb.AppendLine("    {");
            sb.AppendLine("        if (scenePaths.TryGetValue(scene, out string path))");
            sb.AppendLine("        {");
            sb.AppendLine("            return path;");
            sb.AppendLine("        }");
            sb.AppendLine();
            sb.AppendLine("        return null;");
            sb.AppendLine("    }");

            sb.AppendLine("}");

            return sb.ToString();
        }

        private static string GetRootFolderName(GeneratorExecutionContext context)
        {
            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.projectdir", out string projectDir))
            {
                return Path.GetFileName(projectDir.TrimEnd('\\', '/'));
            }

            Debug.Print("Could not find goodot root folder! Defaulting to current directory.");
            return "";
        }

        private static string GetRelativePath(string filePath, string rootFolderName)
        {
            string normalizedFilePath = filePath.Replace("\\", "/");
            string rootFolderIdentifier = $"{rootFolderName.ToLower()}/";
            int rootFolderIndex = normalizedFilePath.ToLower().IndexOf(rootFolderIdentifier);

            Debug.Assert(rootFolderIndex != -1, $"Root folder identifier not found in file path: {normalizedFilePath}");

            return normalizedFilePath.Substring(rootFolderIndex + rootFolderIdentifier.Length);
        }

        private static string GetEnumName(string relativePath, string folderName)
        {
            string folderIdentifier = folderName + "/";
            string enumName = relativePath
                .Substring(relativePath.IndexOf(folderIdentifier) + folderIdentifier.Length)
                .Replace("/", "_")
                .Replace(".tscn", "")
                .SnakeCaseToPascalCase();

            return enumName;
        }
    }
}

