# BinaryViewer

A hex editor and binary file viewer for inspecting firmware images and raw binary data.
Supports memory image parsing, pattern search, and in-place value editing — useful for
verifying flash contents or analyzing binary artifacts.

---

## Projects

| Project | Description |
| --- | --- |
| `BinaryViewer` | The application itself. |
| `FirmwareImageFormat` | Parsing and generation of firmware image formats (Intel HEX, Motorola S-record). |
| `BinaryViewer.Controls` | The `HexBox` hex-editing control. |
| `UnitTestUtilities`<br>`UnitTestIntelHexFormat`<br>`UnitTestSrecFormat` | Unit tests for `FirmwareImageFormat`. |

`BinaryViewer.Controls` is based on [Be.Windows.Forms.HexBox](https://sourceforge.net/projects/hexbox/)
by Bernhard Elbl, by way of the [Be.HexEditor](https://github.com/harborsiem/Be.HexEditor) fork,
with modifications for this application. MIT license — see `BinaryViewer.Controls/LICENSE.txt`.

---

## Requirements

- Windows
- .NET 10 SDK
- Visual Studio 2022 or later (recommended)

NuGet packages are restored automatically via `PackageReference`.

## Build

```
dotnet build BinaryViewer.sln -c Release
```

Alternatively, open `BinaryViewer.sln` in Visual Studio.

## Test

```
dotnet test BinaryViewer.sln
```
