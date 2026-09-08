# 04-upgrade-tests-project-tfm: Upgrade FiniteElementSimple.Tests to net10.0

Update `FiniteElementSimple.Tests\FiniteElementSimple.Tests.csproj` target framework from `net48` to `net10.0` to match the now-upgraded `FiniteElementSimple` project it references. The test project is already SDK-style with no flagged package or API issues, so this should be a straightforward TFM change plus a project-reference/build validation pass.

**Done when**: `FiniteElementSimple.Tests.csproj` targets `net10.0`, the project builds, and all existing tests pass against the upgraded `FiniteElementSimple` project.
