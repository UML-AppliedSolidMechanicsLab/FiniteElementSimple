# Upgrade Options — FiniteElementSimple

Assessment: 2 projects (FiniteElementSimple WinForms app, FiniteElementSimple.Tests class library), both currently net48, targeting net10.0/net10.0-windows. Main project is non-SDK-style with 9 package issues, 22 binary-incompatible WinForms APIs, and 14 potential binding-redirect issues.

## Strategy

### Upgrade Strategy
Two projects with a .NET Framework → modern .NET boundary crossing — Bottom-Up is fixed for this shape (no alternatives).

| Value | Description |
|-------|-------------|
| **Bottom-Up** (selected) | Upgrade FiniteElementSimple.Tests (leaf, low difficulty) first, validate, then upgrade FiniteElementSimple (WinForms app) which depends on it conceptually is reversed here — tests depend on main project, so main project upgrades first, tests validated after. Each tier is validated independently before moving to the next. |

## Project Structure

### Package Management
2 non-CPM projects using packages.config / per-project PackageReference; this is a Framework → modern .NET migration, so CPM is deferred.

| Value | Description |
|-------|-------------|
| **Per-Project (defer CPM to post-migration)** (selected) | Each project keeps its own package versions during the migration. A deferred CPM recommendation is added to the final cleanup phase once both projects are SDK-style and on a single TFM. |
| Central Package Management (CPM) | Creates `Directory.Packages.props` immediately — not recommended while packages.config and old-style project format are still in use. |

## Compatibility

### Unsupported API Handling
22 binary-incompatible Windows Forms API usages detected — all mechanical (enabling Windows Desktop support via `net10.0-windows` TFM), not complex/architectural.

| Value | Description |
|-------|-------------|
| **Fix Inline** (selected) | Resolve all WinForms API compatibility issues in the same task by retargeting to `net10.0-windows` and enabling Windows Desktop SDK support. No deferred work. |
| Defer Complex Changes | Not warranted — these are simple TFM/SDK changes, not complex architectural replacements. |

## Modernization

### Assembly Binding Redirects
14 potential binding-redirect entries found in `app.config` of the main project — above the 10-redirect review threshold.

| Value | Description |
|-------|-------------|
| Remove Binding Redirects | Removes all redirects immediately; risk of masking real version conflicts given the volume. |
| **Document and Review Before Removing** (selected) | Generates a report of all 14 redirects and their purposes before removal, since the volume (>10) warrants review. |
