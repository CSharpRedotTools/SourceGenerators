# SourceGenerators

In order for prefab / scene enum gen to work the following needs to be added in the main Godot .csproj

```xml
<!-- Include Prefabs source generator -->
  <ItemGroup>
    <ProjectReference Include="Template\SourceGenerators\MySourceGenerator\MySourceGenerator.csproj" OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
    <Compile Remove="Template\SourceGenerators\**" />
  </ItemGroup>

  <!-- These files will be available to the source generators -->
  <ItemGroup>
    <!-- Include prefab files -->
    <AdditionalFiles Include="**\Prefabs\**\*.tscn" />
    <!-- Include scene files -->
    <AdditionalFiles Include="Scenes\**\*.tscn" />
  </ItemGroup>
```
