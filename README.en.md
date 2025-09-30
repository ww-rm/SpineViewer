# [SpineViewer](https://github.com/ww-rm/SpineViewer)

[![Build and Release](https://github.com/ww-rm/SpineViewer/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/ww-rm/SpineViewer/actions/workflows/dotnet-desktop.yml)
[![GitHub Release](https://img.shields.io/github/v/release/ww-rm/SpineViewer?logo=github\&logoColor=959da5\&label=Release\&labelColor=3f4850)](https://github.com/ww-rm/SpineViewer/releases)
[![Downloads](https://img.shields.io/github/downloads/ww-rm/SpineViewer/total?logo=github\&logoColor=959da5\&label=Downloads\&labelColor=3f4850)](https://github.com/ww-rm/SpineViewer/releases)

[中文](README.md) | [English](README.en.md)

A simple and user-friendly Spine file viewer and exporter with multi-language support (Chinese/English/Japanese).

![previewer](https://github.com/user-attachments/assets/697ae86f-ddf0-445d-951c-cf04f5206e40)

https://github.com/user-attachments/assets/37b6b730-088a-4352-827a-c338127a16f0

## Features

- Multiple versions of Spine files
- Batch file opening via drag-and-drop or copy-paste
- Batch preview
- List-based multi-skeleton viewing and render order management
- Multi-selection in lists for batch skeleton parameter settings
- Multi-track animation settings
- Skin and custom slot attachment settings
- Custom slot visibility
- Debug rendering
- Playback speed adjustment for view/model/track timelines
- Track alpha blending parameter settings
- Fullscreen preview
- Export to single frame, image sequence, animated GIF, or video file
- Automatic resolution batch export
- Custom export with FFmpeg
- Program parameter saving
- File extension association
- Texture images in formats other than PNG
- Launch at startup with persistent dynamic wallpaper
- ......

### Supported Spine Versions

| Version |     View & Export    |
| :-----: | :------------------: |
| `2.1.x` | :white\_check\_mark: |
| `3.4.x` | :white\_check\_mark: |
| `3.5.x` | :white\_check\_mark: |
| `3.6.x` | :white\_check\_mark: |
| `3.7.x` | :white\_check\_mark: |
| `3.8.x` | :white\_check\_mark: |
| `4.0.x` | :white\_check\_mark: |
| `4.1.x` | :white\_check\_mark: |
| `4.2.x` | :white\_check\_mark: |
| `4.3.x` |                      |

More versions under development \:rocket: \:rocket: \:rocket:

### Supported Export Formats

| Format         | Use Case                                                                      |
| -------------- | ----------------------------------------------------------------------------- |
| Single Frame   | Generate high-resolution images of models; manually adjust the desired frame. |
| Frame Sequence | Supports PNG format with transparency and lossless compression.               |
| GIF/Video      | Export preview animations or common video formats.                            |
| Custom Export  | Supports arbitrary FFmpeg parameters for custom, complex export needs.        |

## Installation

Download the compressed package from the [Release](https://github.com/ww-rm/SpineViewer/releases) page.

The software requires the [.NET Desktop Runtime 8.0.x](https://dotnet.microsoft.com/download/dotnet/8.0) to run.

Alternatively, download the package with the `SelfContained` suffix for standalone execution.

For exporting GIF/MP4 and other animation/video formats, FFmpeg must be installed and added to the system environment variables. Visit the [FFmpeg Windows download page](https://ffmpeg.org/download.html#build-windows) or download the latest version directly: [ffmpeg-release-full.7z](https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-full.7z).

## Usage

### How to Change the Display Language

In the menu, go to "File" -> "Preferences..." -> "Language," select your desired language, and confirm the change.

### Basic Overview

The program is organized into a left-right layout:

- **Left Panel:** Functionality panel.
- **Right Panel:** Preview display.

The left panel includes three sub-panels:

- **Browse:** Preview the content of a specified folder without importing files into the program. This panel allows generating `.webp` previews for models or importing selected models.
- **Model:** Lists imported models for rendering. Parameters and rendering order can be adjusted here, along with other model-related functionalities.
- **Display:** Adjust parameters for the right-side preview display.

Hover your mouse over buttons, labels, or input fields to see help text for most UI elements.

### Skeleton Import

Drag-and-drop or paste skeleton files/directories into the Model panel.

Alternatively, use the right-click menu in the Browse panel to import selected items.

### Content Adjustment

The Model panel supports right-click menus, some shortcuts, and batch adjustments of model parameters through multi-selection.

For preview display adjustments:

- **Left-click:** Select and drag models. Hold `Ctrl` for multi-selection, synchronized with the left-side list.
- **Right-click:** Drag the entire display.
- **Scroll wheel:** Zoom in/out. Hold `Ctrl` to scale selected models.
- **Render selected-only mode:** In this mode, the preview only shows selected models, and selection status can only be changed via the left-side list.

The buttons below the preview display allow time adjustments, serving as a simple playback control.

### Content Export

Export follows the **WYSIWYG (What You See Is What You Get)** principle, meaning the preview display reflects the exported output.

Use the right-click menu in the Model panel to export selected items.

Key export parameters include:

- **Output folder:** Optional. When not specified, output is saved to the respective model folder; otherwise, all output is saved to the provided folder.
- **Export single:** By default, each model is exported independently. Selecting "Export single" renders all selected models in a single frame, producing a unified output.
- **Auto resolution:** Ignores the preview resolution and viewport parameters, exporting output at the actual size of the content. For animations/videos, the output matches the size required for full visibility.

### Dynamic Wallpaper

Dynamic wallpaper is implemented through desktop projection, allowing the content of the current preview to be projected onto the desktop in real time.

You can enable or disable desktop projection from the program preferences or the right-click menu of the tray icon. After adjusting the model and display parameters, you can save the current configuration as a workspace file for convenient restoration later.

If you want the wallpaper to stay active after startup, you can enable auto-start in the preferences and specify which workspace file should be loaded when the program launches.

### More Information

For detailed usage and documentation, see the [Wiki](https://github.com/ww-rm/SpineViewer/wiki). For usage questions or bug reports, submit an [Issue](https://github.com/ww-rm/SpineViewer/issues).

## Acknowledgements

- [spine-runtimes](https://github.com/EsotericSoftware/spine-runtimes)
- [SFML.Net](https://github.com/SFML/SFML.Net)
- [FFMpegCore](https://github.com/rosenbjerg/FFMpegCore)
- [HandyControl](https://github.com/HandyOrg/HandyControl)
- [NLog](https://github.com/NLog/NLog)
- [SkiaSharp](https://github.com/mono/SkiaSharp)

---

*If you find this project helpful, please give it a \:star: and share it with others! :)*

[![Stargazers over time](https://starchart.cc/ww-rm/SpineViewer.svg?variant=adaptive)](https://starchart.cc/ww-rm/SpineViewer)
