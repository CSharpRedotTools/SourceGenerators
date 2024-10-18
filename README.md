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

## Switching Scenes
> [!IMPORTANT]
> You will need to restart your IDE if you want to see the new changes to the `Prefab` and `Scene` classes.

Any `.tscn` files placed in the following paths will be added to `Prefab` and `Scene` classes.

- **Prefab Resources**:
  - **Search Path**: `res://**/Prefabs/**/*.tscn`
  - **Associated Class**: `Prefab`

- **Scene Resources**:
  - **Search Path**: `res://Scenes/**/*.tscn`
  - **Associated Class**: `Scene`

**Example Usage**

```cs
Game.SwitchScene(Scene.UICredits);
Game.SwitchScene(Prefab.UIOptions);
```
