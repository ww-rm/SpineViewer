# [SpineViewer](https://github.com/ww-rm/SpineViewer)

[![Build and Release](https://github.com/ww-rm/SpineViewer/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/ww-rm/SpineViewer/actions/workflows/dotnet-desktop.yml)

[中文](README.md) | [English](README.en.md)

A simple and user-friendly Spine file viewer and exporter.

![previewer](img/preview.jpg)

---

## Installation

Download the zip package from the [Releases](https://github.com/ww-rm/SpineViewer/releases) page.

The application requires the [.NET Desktop Runtime 8.0.x](https://dotnet.microsoft.com/en-us/download/dotnet/8.0).

Alternatively, you can download the zip package with the `SelfContained` suffix, which can run independently.

## Features

- Supports viewing Spine files of different versions:
    - [x] `v2.1.x`
    - [x] `v3.6.x`
    - [x] `v3.7.x`
    - [x] `v3.8.x`
    - [x] `v4.0.x`
    - [x] `v4.1.x`
    - [x] `v4.2.x`
    - [ ] `v4.3.x`
- Supports animation preview for multi-skeleton files
- Allows independent parameter settings for each skeleton
- Supports exporting animation as PNG frame sequences
- Provides export settings such as zoom and rotation
- More features coming soon...

## Usage

### Importing Skeletons

Use the **File** menu to select **Open** or **Batch Open** to import skeleton files.

### Adjusting Skeletons

Select one or more items in the **Model List** to display adjustable parameters in the **Model Parameters** panel.

Right-clicking in the **Model List** allows you to add, delete, or adjust list items. You can also drag items with the left mouse button to rearrange them.

### Adjusting the View

Mouse operations supported in the **Preview** window:

- Left-click to drag the skeleton
- Right-click to drag the view
- Scroll wheel to zoom in/out

Additionally, you can adjust export and preview parameters through the **View Parameters** panel.

In the **Functions** menu, you can reset and synchronize the animation time for all skeletons.

### Exporting Animations

Select **Export** from the **File** menu to export all loaded skeleton animations as PNG frame sequences, based on the current preview settings.

You can view the full duration of each animation in the **Model Parameters** of each skeleton.

---

*If you find this project helpful, please give it a :star: and share it with others! :)*
