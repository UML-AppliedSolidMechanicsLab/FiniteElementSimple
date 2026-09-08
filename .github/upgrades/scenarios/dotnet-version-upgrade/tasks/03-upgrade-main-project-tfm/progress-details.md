# 03-upgrade-main-project-tfm: Progress Details

## Changes
- Retargeted `FiniteElementSimple.csproj` from `net48` to `net10.0-windows7.0` (explicit OS version needed to resolve WinForms API `CA1416` platform-compatibility warnings cleanly).
- Package updates for net10.0-windows7.0 compatibility:
  - `OpenTK`: 3.1.0 → 4.9.4
  - `OpenTK.GLControl`: 3.1.0 → 4.0.2
  - Kept `ScottPlot`/`ScottPlot.WinForms` at 5.1.59 (assessment's suggested `ScottPlot.WinForms` 4.1.73 was a different major version incompatible with the already-referenced `ScottPlot` 5.1.59 API surface — using 4.1.73 caused `FormsPlot` type-not-found errors). 5.1.59 is confirmed via `get_supported_package_version` as compatible with net10.0-windows.
  - Kept `SkiaSharp.Views.WindowsForms` at 3.119.0 (required by `ScottPlot.WinForms` 5.1.59's dependency graph; downgrading to the assessment-suggested 2.88.9 produced an `NU1605` downgrade error). This produces an expected `NU1701` compatibility-shim warning (package has no net-native asset, loads via .NET Framework compat mode) — functional, not a build blocker.
  - Removed `System.Buffers`, `System.Memory`, `System.Numerics.Vectors`, `System.Runtime.CompilerServices.Unsafe`, `System.ValueTuple` — functionality is included in the .NET 10 base class library.
  - Removed `SinglePlotZedGraph` package reference — dead reference; the code was already migrated to a ScottPlot-based implementation (`Plotting/SinglePlotForm.cs`), only referenced in comments, not code.
- Removed legacy .NET-Framework-only project properties no longer applicable under SDK-style/net10.0: ClickOnce/publish properties (`PublishUrl`, `Install`, `InstallFrom`, `UpdateEnabled`, etc.), `BootstrapperPackage` items, `PlatformTarget=x86` conditional (AnyCPU/any-CPU is now the default and correct for net10.0), and obsolete `<Reference>` items for `Microsoft.CSharp`/`System.Core`/`System.Data.DataSetExtensions`/`System.Xml.Linq` (these are implicit framework references under the modern SDK and caused `MSB3245`/`MSB3243` conflicts when left in).
- Fixed 22 WinForms binary-incompatibility / `CA1416` platform-compatibility warnings by adding `[module: SupportedOSPlatform("windows")]` to `AssemblyInfo.cs` — since the whole assembly is Windows-only WinForms code, this cleanly asserts platform support at the assembly level rather than requiring per-call-site annotations.
- Removed 2 pre-existing unused-variable warnings (`CS0219`) in `Homework/HW10.cs` (`delta`, `stophere`) unrelated to the TFM change but caught during warning cleanup.
- Removed `FiniteElementSimple\app.config` — see `binding-redirects-report.md` in this task folder for the reviewed binding-redirect analysis (only 1 real redirect existed, for ZedGraph, now superseded by direct PackageReference resolution).

## Validation
- `dotnet build FiniteElementSimple.csproj` (isolated from the test project, which still targets net48 and is out of scope for this task): succeeded with 2 warnings (both `NU1701` compat-shim advisories for `SinglePlotZedGraph`-adjacent... note: after removing `SinglePlotZedGraph`, only the expected `SkiaSharp.Views.WindowsForms` NU1701 remains).
- Full solution build (`run_build`) still fails only on `FiniteElementSimple.Tests.csproj`, which is expected — it targets `net48` and cannot reference a `net10.0-windows7.0` project. This is addressed by task `04-upgrade-tests-project-tfm`.

## Result
`FiniteElementSimple.csproj` now targets `net10.0-windows7.0`, all packages are updated/compatible (one expected NU1701 shim warning remains, documented above), all 22 WinForms API compatibility issues are resolved via assembly-level `SupportedOSPlatform`, and binding redirects were reviewed and removed. The project builds successfully in isolation.
