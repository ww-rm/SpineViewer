Below is the translated English version of your README:

---

# [SpineViewer](https://github.com/ww-rm/SpineViewer)

[![Build and Release](https://github.com/ww-rm/SpineViewer/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/ww-rm/SpineViewer/actions/workflows/dotnet-desktop.yml)

[中文](README.md) | [English](README.en.md)

A *WYSIWYG* Spine file viewer and exporter.

![previewer](img/preview.webp)

---

## Installation

Go to the [Release](https://github.com/ww-rm/SpineViewer/releases) page to download the zip package.

The software requires the dependency framework [.NET Desktop Runtime 8.0.x](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0).

You can also download the zip package with the `SelfContained` suffix, which can run independently.

## Supported Export Formats

- [x] Single Frame Image
- [x] Frame Sequence
- [x] Animated GIF
- [ ] MKV
- [ ] MP4
- [ ] MOV
- [ ] WebM

More formats are under development :rocket::rocket::rocket:

## Supported Spine Versions

| Version   | View & Export | Format Conversion | Version Conversion |
| :-------: | :-----------: | :---------------: | :----------------: |
| `2.1.x`   | :white_check_mark: |               |                    |
| `3.1.x`   |             |               |                    |
| `3.4.x`   |             |               |                    |
| `3.5.x`   |             |               |                    |
| `3.6.x`   | :white_check_mark: |               |                    |
| `3.7.x`   | :white_check_mark: |               |                    |
| `3.8.x`   | :white_check_mark: | :white_check_mark: |               |
| `4.1.x`   | :white_check_mark: |               |                    |
| `4.2.x`   | :white_check_mark: |               |                    |
| `4.3.x`   |             |               |                    |

More versions are under development :rocket::rocket::rocket:

## Usage

### Importing Skeletons

There are three ways to import skeleton files:

- Drag and drop or paste the skeleton file/directory into the model list.
- Open skeleton files in batch from the File menu.
- Select a single model to open from the File menu.

### Adjusting Preview Content

The model list supports right-click menus and several hotkeys, and multiple models can be selected for batch adjustments of model parameters.

In addition to using the control panel for parameter settings, the preview window supports the following mouse actions:

- Left-click to select and drag models. Hold the `Ctrl` key to enable multi-selection, which is synchronized with the model list on the left.
- Right-click to drag the overall view.
- Use the scroll wheel to zoom in/out.
- "Render selected only" mode, in which the preview only includes selected models and the selection can only be changed via the model list on the left.

The buttons below the preview window allow you to adjust the timeline, effectively serving as a simple player.

### Exporting Preview Content

Export follows the "What You See Is What You Get" principle—what you see in the live preview is exactly what gets exported.

There are a few key parameters for exporting:

- Render Selected Only: This option not only affects the preview mode but also the export; if enabled, only the selected models will be considered, and all other models will be ignored during export.
- Output Folder: This parameter is optional in some cases. If not provided, the output will be saved in each model's own directory. Otherwise, all output files will be saved to the specified folder.
- Single Export: By default, each model is exported individually in batch mode. If "Single Export" is selected, all exported models will be rendered on a single canvas, resulting in only one output file.

### More Information

For more detailed instructions and usage, please refer to the [Wiki](https://github.com/ww-rm/SpineViewer/wiki). If you encounter any issues or bugs, please open an [Issue](https://github.com/ww-rm/SpineViewer/issues).

## Acknowledgements

- [spine-runtimes](https://github.com/EsotericSoftware/spine-runtimes)
- [SFML.Net](https://github.com/SFML/SFML.Net)
- [FFMpegCore](https://github.com/rosenbjerg/FFMpegCore)

---

*If you like this project, please give it a :star: and share it with others! :)*

[![Stargazers over time](https://starchart.cc/ww-rm/SpineViewer.svg?variant=adaptive)](https://starchart.cc/ww-rm/SpineViewer)