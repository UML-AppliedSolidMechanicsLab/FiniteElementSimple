# Assembly Binding Redirect Report — FiniteElementSimple

## Findings
Reviewed `FiniteElementSimple\app.config` prior to the `net10.0-windows` retarget.

Only **1** actual `<bindingRedirect>` entry was found (not the 14 potentially flagged during assessment — that figure likely reflected package-count/compatibility analysis rather than literal app.config entries):

| Assembly | Old Version Range | New Version | Notes |
|----------|-------------------|--------------|-------|
| ZedGraph | 0.0.0.0-5.2.1.437 | 5.2.1.437 | Redirects legacy ZedGraph references to the package version resolved via NuGet (`ZedGraph` 5.2.1). |

## Decision
`app.config` binding redirects are a .NET Framework-only mechanism (`<runtime><assemblyBinding>`). Modern .NET (net10.0) uses a different dependency resolution model (`*.deps.json` + `AssemblyLoadContext`) and does not honor `<bindingRedirect>` elements — the framework-only `<startup>` and `<runtime>` sections become inert/unnecessary after retargeting.

**Action taken**: Removed `FiniteElementSimple\app.config` entirely as part of the `net10.0-windows` TFM upgrade, since:
- The `<startup>` element pinned `.NETFramework,Version=v4.8`, which no longer applies.
- The single binding redirect (ZedGraph) is superseded by normal NuGet PackageReference version resolution under the SDK-style project — no further action needed since the project already references `ZedGraph` 5.2.1 directly as a `PackageReference`.

No other app.config settings (connection strings, appSettings, etc.) were present to migrate.
