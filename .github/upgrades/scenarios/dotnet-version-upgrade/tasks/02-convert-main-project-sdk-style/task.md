# 02-convert-main-project-sdk-style: Convert FiniteElementSimple to SDK-style

Convert `FiniteElementSimple\FiniteElementSimple.csproj` from the legacy (non-SDK-style) project format to SDK-style, while remaining on `net48`. This includes migrating `packages.config` to `PackageReference` as part of the conversion. The project currently references WinForms, System.Drawing, and several third-party packages (MathNet.Numerics, OpenTK, ScottPlot, SkiaSharp family, ZedGraph, HarfBuzzSharp, StapletonMathPackage) via old-style `<Reference>`/`HintPath` entries — these must resolve correctly as `PackageReference` items after conversion. `FiniteElementSimple.Tests.csproj` is already SDK-style and is not affected by this task.

**Done when**: `FiniteElementSimple.csproj` is SDK-style, uses `PackageReference` instead of `packages.config`, still targets `net48`, and the solution builds successfully with no regressions.
