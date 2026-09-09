# FE Input File Format

This document describes the simple text-based finite-element input file format read by
[`FiniteElementSimple.IO.InputFileReader`](../FiniteElementSimple/IO/InputFileReader.cs).

**Scope:** this first implementation supports existing linear 2-D plane-stress problems only
(Q4 and Q8 elements). The reader does not perform any FE mechanics itself - it only parses the
file and constructs the same `Assembly`, `Element`, `Material`, and `BC` objects that hard-coded
example problems already build directly in code.

## File structure

The file is a sequence of **sections**. Each section begins with a line starting with `*`
followed by the section keyword, and is followed by zero or more comma-separated data lines
until the next `*` section header or end of file:

```
*SECTIONNAME
field1, field2, field3, ...
field1, field2, field3, ...
```

### Comments and blank lines

- Blank lines are ignored, anywhere in the file.
- Lines starting with `#` are treated as comments and ignored, anywhere in the file.
- Leading/trailing whitespace around each line and around each comma-separated field is trimmed.

## Supported sections

Section keywords are case-insensitive. All are required except where noted.

### `*MATERIALS`

One line per material:

```
materialId, PLANESTRESS, E, nu
```

| Field | Meaning |
|---|---|
| `materialId` | Integer, unique material id referenced by `*ELEMENTS` |
| type | Material type keyword (see [Supported material types](#supported-material-types)) |
| `E` | Young's modulus |
| `nu` | Poisson's ratio |

### `*NODES`

One line per node:

```
nodeId, x, y
```

| Field | Meaning |
|---|---|
| `nodeId` | Integer, unique node number (see [Node numbering](#node-numbering-convention)) |
| `x` | X coordinate |
| `y` | Y coordinate |

### `*THICKNESS`

A single line with a single value, applied to every element in the file:

```
thickness
```

This section may only appear once. There is currently no support for per-element or
per-material thickness.

### `*ELEMENTS`

One line per element:

```
elementId, Q4|Q8, materialId, node1, node2, ...
```

| Field | Meaning |
|---|---|
| `elementId` | Integer, element id (used only for error messages) |
| type | `Q4` or `Q8` (see [Supported element types](#supported-element-types)) |
| `materialId` | Must match a `materialId` defined in `*MATERIALS` |
| `node1, node2, ...` | Node ids in the element's local node order; exactly 4 ids for `Q4`, exactly 8 for `Q8` |

### `*BCS`

Prescribed displacement boundary conditions, one line per constrained DOF:

```
nodeId, dofInNode, magnitude
```

### `*LOADS`

Nodal point loads, one line per loaded DOF:

```
nodeId, dofInNode, magnitude
```

Both `*BCS` and `*LOADS` share the same fields:

| Field | Meaning |
|---|---|
| `nodeId` | Must match a node defined in `*NODES` |
| `dofInNode` | `0` for X, `1` for Y (see [DOF names](#dof-names)) |
| `magnitude` | Prescribed displacement (`*BCS`) or applied load (`*LOADS`) |

## Node numbering convention

Node ids are arbitrary positive integers assigned by the user in `*NODES`; they do not need to
be contiguous or start at 1, but each id must be unique (duplicates are a parse error). The same
node ids are reused in `*ELEMENTS` (as element connectivity), `*BCS`, and `*LOADS` to refer back
to the coordinates defined in `*NODES`.

## DOF names

Every problem read by this format is 2-D, with 2 degrees of freedom per node:

| `dofInNode` | Meaning |
|---|---|
| `0` | X displacement |
| `1` | Y displacement |

## Supported element types

| Keyword | Meaning | Required node count |
|---|---|---|
| `Q4` | 4-node bilinear plane-stress quadrilateral (`Node4Element2D`) | 4 |
| `Q8` | 8-node quadratic plane-stress quadrilateral (`Node8Element2D`) | 8 |

Node ids in an element's `*ELEMENTS` line must be given in that element's own local node order
(the same ordering used by the corresponding `Node4Element2D`/`Node8Element2D` shape functions).
No other element types are currently supported.

## Supported material types

| Keyword | Meaning | Fields |
|---|---|---|
| `PLANESTRESS` | Linear elastic plane stress (`LinElastic2DPlaneStress`) | `E`, `nu` |

No other material types are currently supported.

## Units policy

The reader and the underlying FE code assume a single, consistent unit system chosen by the
user (e.g. all lengths in meters, all forces in newtons, or all lengths in inches and all forces
in pounds). **No unit conversion is performed anywhere in the parser or the solver.** It is the
user's responsibility to ensure that coordinates, thickness, material properties, prescribed
displacements, and loads are all expressed in one mutually consistent set of units.

## Complete working example

This example is a single Q8 element and is also used by the automated test
`FiniteElementSimple.Tests.InputFileReaderTests.TestReadAndSolve_MatchesHardCodedSingleQ8Model`,
which verifies its solved displacements match an equivalent hard-coded model. The file itself
lives at [`FiniteElementSimple/ExampleInputFiles/SingleQ8Element.fem`](../FiniteElementSimple/ExampleInputFiles/SingleQ8Element.fem).

```
# Example FE input file: single 8-node (Q8) plane-stress quadrilateral element.

*MATERIALS
1, PLANESTRESS, 43, 0.3

*NODES
1, 0, 0
2, 2, 0.3
3, 1.9, 3.4
4, -0.1, 3
5, 0.7, 0
6, 1.8, 1.9
7, 1.1, 3.2
8, 0, 3

*THICKNESS
1.3

*ELEMENTS
1, Q8, 1, 1, 2, 3, 4, 5, 6, 7, 8

*BCS
1, 0, 0.0
1, 1, 0.0
2, 1, 0.0

*LOADS
3, 0, 100.0
```

## Common input errors

The parser raises a `FormatException` with a message that includes the offending line number
and, where relevant, the invalid value. The most common cases are:

| Situation | Example | Parser behavior |
|---|---|---|
| Data before any `*` section header | A non-comment line appears before the first `*SECTION` line | Error: data found before any section header |
| Unknown section keyword | `*BOGUS` | Error: unknown section keyword; lists the supported sections |
| Wrong number of fields on a data line | `*NODES` line with only 2 fields | Error: malformed `*SECTION` line, with the expected format shown |
| Duplicate node number | Two `*NODES` lines with the same `nodeId` | Error: duplicate node number `N` |
| Duplicate material id | Two `*MATERIALS` lines with the same `materialId` | Error: duplicate material id `N` |
| `*THICKNESS` given more than once | Two `*THICKNESS` sections/lines | Error: `*THICKNESS` may only be specified once |
| Missing `*THICKNESS` section | File has no `*THICKNESS` section | Error: missing required `*THICKNESS` section |
| Missing `*ELEMENTS` section | File has no `*ELEMENTS` section | Error: missing required `*ELEMENTS` section |
| Element with wrong node count | `Q4` element listing 3 or 5 node ids | Error: element requires exactly 4 (or 8) node ids |
| Element/BC/Load referencing an undefined node | `*ELEMENTS`/`*BCS`/`*LOADS` line uses a `nodeId` not defined in `*NODES` | Error: references undefined node `N` |
| Element referencing an undefined material | `*ELEMENTS` line uses a `materialId` not defined in `*MATERIALS` | Error: references undefined material id `N` |
| Unsupported element type | `TRI3`, `HEX8`, etc. | Error: unsupported element type; only `Q4` and `Q8` are supported |
| Unsupported material type | `ORTHOTROPIC`, etc. | Error: unsupported material type; only `PLANESTRESS` is supported |
| `dofInNode` out of range | `*BCS`/`*LOADS` line with `dofInNode` other than `0` or `1` | Error: `dofInNode` must be 0 or 1 for a 2-D problem |
| Non-numeric field | Text where an integer or number is expected | Error: could not parse `<field>` as an integer/number |
