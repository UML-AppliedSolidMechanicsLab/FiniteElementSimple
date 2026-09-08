# .NET Version Upgrade

## Preferences
- **Flow Mode**: Automatic
- **Target Framework**: net10.0

## Source Control
- **Source Branch**: dev_MECH6140
- **Working Branch**: dev_MECH6140 (current branch, no new branch created)
- **Commit Strategy**: After Each Task
- **Branch Sync**: Auto (Merge)

## Upgrade Options
**Source**: .github/upgrades/scenarios/dotnet-version-upgrade/upgrade-options.md

### Strategy
- Upgrade Strategy: Bottom-Up

### Project Structure
- Package Management: Per-Project (defer CPM to post-migration)

### Compatibility
- Unsupported API Handling: Fix Inline

### Modernization
- Assembly Binding Redirects: Document and Review Before Removing

## Strategy
**Selected**: Bottom-Up (Dependency-First)
**Rationale**: 2 projects, 2-tier dependency graph, crossing the .NET Framework → modern .NET boundary (non-negotiable for Framework migrations with 2+ projects).

### Execution Constraints
- Strict tier ordering: Tier 1 (FiniteElementSimple) must be SDK-style-converted, TFM-upgraded, and validated before Tier 2 (FiniteElementSimple.Tests) starts
- SDK-style conversion is a separate task from the TFM upgrade — never merge
- Between-tier validation: confirm the solution still builds after each tier
- Unsupported API Handling = Fix Inline: resolve all WinForms API issues within the TFM upgrade task, no stubs/deferrals
- Assembly Binding Redirects = Document and Review: generate a short report of the 14 redirects before removing them in the TFM upgrade task
- Package Management = Per-Project (CPM deferred): document CPM as a post-migration recommendation in the final validation task
