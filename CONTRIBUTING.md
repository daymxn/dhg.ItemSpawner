# Contributing

Any and all contributions are entirely welcomed! Before you contribute though, there are
some things you should know.

> [!NOTE]
> Making public contributions to this repo means you accept the [LICENSE](LICENSE) agreement, and you're contributing
> code that also respects the [LICENSE](LICENSE) agreement.

### Building

Use the `dotnet` CLI to build the source files.

```sh
dotnet build
```

### Code Formatting

Code in this repo is formatted according to the [.editorconfig](./.editorconfig) defined at the root of the project.

Most IDEs natively integrate with this, but you can also perform formatting manually via the `dotnet` CLI.

```sh
dotnet format ./ItemSpawner.csproj
```

### Releasing

Bump the version defined in [ItemSpawner.csproj](./ItemSpawner.csproj).

```diff
- <Version>1.0.0</Version>
+ <Version>1.0.1</Version>
```

Build the release version of the plugin via the `dotnet` CLI.

```sh
dotnet build --configuration Release
```

This should output a folder under `bin/Release/netstandard2.1` with a name matching the plugin namespace.

Create a zip of this folder and publish a release via the
[GitHub release panel](https://github.com/daymxn/dhg.ItemSpawner/releases/new) with the zip file.
