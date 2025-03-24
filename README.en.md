# [SpineViewer](https://github.com/ww-rm/SpineViewer)

[![Build and Release](https://github.com/ww-rm/SpineViewer/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/ww-rm/SpineViewer/actions/workflows/dotnet-desktop.yml)

[中文](README.md) | [English](README.en.md)

A simple and user-friendly tool for viewing and exporting Spine files.

![previewer](img/preview.webp)

---

## Installation

Go to the [Release](https://github.com/ww-rm/SpineViewer/releases) page to download the compressed package.

The software requires the dependency framework [.NET Desktop Runtime 8.0.x](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0).

You can also download the package with the `SelfContained` suffix, which can run independently.

## Version Support

| Version   | View & Export          | Format Conversion      | Version Conversion |
| :-------: | :--------------------: | :--------------------: | :----------------: |
| `2.1.x`   | :white_check_mark:     |                        |                    |
| `3.1.x`   |                        |                        |                    |
| `3.4.x`   |                        |                        |                    |
| `3.5.x`   |                        |                        |                    |
| `3.6.x`   | :white_check_mark:     |                        |                    |
| `3.7.x`   | :white_check_mark:     |                        |                    |
| `3.8.x`   | :white_check_mark:     | :white_check_mark:     |                    |
| `4.1.x`   | :white_check_mark:     |                        |                    |
| `4.2.x`   | :white_check_mark:     |                        |                    |
| `4.3.x`   |                        |                        |                    |

## Usage

### Importing Skeletons

There are 3 ways to import skeleton files:

- **Drag & Drop/Paste:**  
  Drag and drop or paste the skeleton file/directory into the model list.  
  This method automatically searches through the provided files and subdirectories. Although convenient, it relies on the file structure and has its limitations.
  
  - Only standard files with `*.json`, `*.skel`, or `.atlas` extensions are automatically detected.
  - The skeleton file and atlas file must have the same name.
  - The version string in the skeleton file must not be modified.

- **Batch Open from the File Menu:**  
  This method offers more file flexibility. You can drag and drop or paste files into the file selection dialog, and additional options are available.
  
  - The filename restrictions are similar to the above, but you can use the panel’s file selection button to choose skeleton files with non-standard extensions.
  - You can set a fixed load version to handle cases where the version number has been modified.

- **Open a Single Model:**  
  This method offers the highest degree of freedom, allowing you to select any skeleton file and atlas file without filename restrictions. You can also set the load version.

### Adjusting Preview Content

The model list supports right-click menus and various shortcut keys, and you can select multiple models to adjust their parameters in bulk.

In addition to the parameter panel, the preview area supports several mouse actions:

- **Left-click:** Select and drag models. Holding down the `Ctrl` key enables multi-selection, which syncs with the model list.
- **Right-click:** Drag the overall canvas.
- **Scroll wheel:** Zoom the view.
- **Selective Rendering:** The preview area supports a mode to render only the selected models. In this mode, only the selected models are displayed, and selection changes must be made through the model list.

In the function menu, you can reset and synchronize the animation time for all skeletons.

### Exporting Preview Content

Both preview images and videos can be exported.

- **Preview Image:**  
  The exported preview image shows the model in its default state, with one image per model.
  
- **Video (TODO: Currently only supports frame sequence export):**  
  The complete animation duration for each skeleton can be viewed in the model parameters.
  
When the preview area is set to render only the selected models, the exported content will include only the models that are displayed.

### Format & Version Conversion

You can use the tools menu to convert skeleton files. This feature supports conversion between binary and text formats, as well as between different versions.

Currently under development, it only supports converting `3.8.x` binary format to text format.

---

*If you like this project, please give it a :star: and share it with more people! :)*
