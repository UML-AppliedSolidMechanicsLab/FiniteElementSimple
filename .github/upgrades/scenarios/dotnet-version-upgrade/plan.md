# .NET Version Upgrade Plan

## Overview

**Target**: FiniteElementSimple solution — upgrade from .NET Framework 4.8 to .NET 10 (`net10.0-windows` for the WinForms app, `net10.0` for the test project).
**Scope**: 2 projects, ~2.4k LOC total. Main project (`FiniteElementSimple.csproj`) is a non-SDK-style WinForms application with 9 package issues, 22 WinForms API compatibility issues, and 14 potential binding-redirect entries. Test project (`FiniteElementSimple.Tests.csproj`) is already SDK-style with no issues.

### Selected Strategy
**Bottom-Up (Dependency-First)** — Upgrade from leaf nodes to root applications, tier by tier.
**Rationale**: 2 projects, 2-tier dependency graph, crossing the .NET Framework → modern .NET boundary (non-negotiable per Framework migration rules).

**Dependency graph:**
```
Tier 2: [FiniteElementSimple.Tests]
		 ↓
Tier 1: [FiniteElementSimple]
```

**Per-tier summary:**
- **Tier 1** — `FiniteElementSimple.csproj`: no internal project dependencies. Must be SDK-style-converted and TFM-upgraded before Tier 2.
- **Tier 2** — `FiniteElementSimple.Tests.csproj`: depends on Tier 1; already SDK-style, only needs TFM update once Tier 1 is on `net10.0-windows`.

## Tasks

### 01-prerequisites: Verify toolchain and SDK compatibility

Verify the .NET 10 SDK is installed and compatible with the solution (no `global.json` pinning an incompatible SDK version). Confirm Visual Studio 2026 tooling supports the target TFMs (`net10.0-windows` for WinForms, `net10.0` for the test project).

**Done when**: .NET 10 SDK is confirmed installed and available; no `global.json` blocking issues found.

---

### 02-convert-main-project-sdk-style: Convert FiniteElementSimple to SDK-style

Convert `FiniteElementSimple\FiniteElementSimple.csproj` from the legacy (non-SDK-style) project format to SDK-style, while remaining on `net48`. This includes migrating `packages.config` to `PackageReference` as part of the conversion. The project currently references WinForms, System.Drawing, and several third-party packages (MathNet.Numerics, OpenTK, ScottPlot, SkiaSharp family, ZedGraph, HarfBuzzSharp, StapletonMathPackage) via old-style `<Reference>`/`HintPath` entries — these must resolve correctly as `PackageReference` items after conversion. `FiniteElementSimple.Tests.csproj` is already SDK-style and is not affected by this task.

**Done when**: `FiniteElementSimple.csproj` is SDK-style, uses `PackageReference` instead of `packages.config`, still targets `net48`, and the solution builds successfully with no regressions.

---

### 03-upgrade-main-project-tfm: Upgrade FiniteElementSimple to net10.0-windows

Retarget `FiniteElementSimple.csproj` from `net48` to `net10.0-windows`, enabling Windows Desktop support (WinForms) via the SDK. Resolve the 4 incompatible NuGet packages flagged in the assessment (OpenTK → 4.9.4, OpenTK.GLControl → 4.0.2, ScottPlot.WinForms → 4.1.73, SkiaSharp.Views.WindowsForms → 2.88.9) and update `System.Runtime.CompilerServices.Unsafe` to the suggested version. Remove packages whose functionality is now included in the framework (`System.Buffers`, `System.Memory`, `System.Numerics.Vectors`, `System.ValueTuple`). Fix the 22 binary-incompatible WinForms API usages (primarily `System.Windows.Forms.Form`, `DockStyle`, `DialogResult`, `Control.ControlCollection` members) — these are mechanical fixes once Windows Desktop support is enabled (per confirmed Unsupported API Handling = Fix Inline). Per the confirmed Assembly Binding Redirects option (Document and Review), generate a short report of the 14 potential binding-redirect entries in `app.config` before removing them, since SDK-style/modern TFM projects auto-manage assembly binding and hand-authored redirects are typically no longer needed.

**Done when**: `FiniteElementSimple.csproj` targets `net10.0-windows`, all packages are compatible/updated, all WinForms API compatibility issues are resolved, binding redirects are reviewed and removed where appropriate, and the project builds successfully.

---

### 04-upgrade-tests-project-tfm: Upgrade FiniteElementSimple.Tests to net10.0

Update `FiniteElementSimple.Tests\FiniteElementSimple.Tests.csproj` target framework from `net48` to `net10.0` to match the now-upgraded `FiniteElementSimple` project it references. The test project is already SDK-style with no flagged package or API issues, so this should be a straightforward TFM change plus a project-reference/build validation pass.

**Done when**: `FiniteElementSimple.Tests.csproj` targets `net10.0`, the project builds, and all existing tests pass against the upgraded `FiniteElementSimple` project.

---

### 05-final-validation: Full solution build and test validation

Run a full solution build and execute the complete test suite to confirm the upgrade is complete and functionally equivalent. Document the deferred Central Package Management (CPM) recommendation for post-migration adoption, now that both projects are SDK-style and on a single TFM (`net10.0`/`net10.0-windows`).

**Done when**: Solution builds with zero errors and zero warnings across both projects, all tests pass, and the deferred CPM recommendation is documented.
