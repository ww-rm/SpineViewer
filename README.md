# [SpineViewer](https://github.com/ww-rm/SpineViewer)

[![Build and Release](https://github.com/ww-rm/SpineViewer/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/ww-rm/SpineViewer/actions/workflows/dotnet-desktop.yml)
[![GitHub Release](https://img.shields.io/github/v/release/ww-rm/SpineViewer?logo=github&logoColor=959da5&label=Release&labelColor=3f4850)](https://github.com/ww-rm/SpineViewer/releases)
[![Downloads](https://img.shields.io/github/downloads/ww-rm/SpineViewer/total?logo=github&logoColor=959da5&label=Downloads&labelColor=3f4850)](https://github.com/ww-rm/SpineViewer/releases)

[中文](README.md) | [English](README.en.md)

*所见即所得* 的 Spine 文件查看&导出程序.

![previewer](img/preview.webp)

## 功能

- 支持多版本 spine 文件
- 支持拖拽/复制粘贴批量打开文件
- 支持列表式多骨骼查看和渲染层级管理
- 支持列表多选批量设置骨骼参数
- 支持多轨道动画设置
- 支持皮肤/自定义插槽附件设置
- 支持调试渲染
- 支持全屏预览
- 支持单帧/动图/视频文件导出
- 支持自动分辨率批量导出
- 支持 FFmpeg 自定义导出
- ...

### Spine 版本支持

| 版本 | 查看&导出 | 格式转换 | 版本转换 |
| :---: | :---: | :---: | :---: |
| `2.1.x` | :white_check_mark: |  |  |
| `3.6.x` | :white_check_mark: |  |  |
| `3.7.x` | :white_check_mark: |  |  |
| `3.8.x` | :white_check_mark: | :white_check_mark: |  |
| `4.0.x` | :white_check_mark: |  |  |
| `4.1.x` | :white_check_mark: |  |  |
| `4.2.x` | :white_check_mark: | :white_check_mark: |  |
| `4.3.x` |  |  |  |

更多版本正在施工 :rocket: :rocket: :rocket:

### 导出格式支持

| 导出格式 | 适用场景 |
| --- | --- |
| 单帧画面 | 支持生成高清模型画面图像, 可手动调节需要的一帧. |
| 帧序列 | 支持 PNG 格式帧序列, 可保留透明通道且无损压缩. |
| GIF/WebP/AVIF | 适合生成预览动图. |
| MP4 | 最常见的视频格式, 兼容性最好. |
| WebM | 适合浏览器在线播放格式, 支持透明背景. |
| MKV/MOV | 适合折腾. |
| 自定义导出 | 除上述预设方案, 支持提供任意 FFmpeg 参数进行导出, 满足自定义复杂需求. |

## 安装

前往 [Release](https://github.com/ww-rm/SpineViewer/releases) 界面下载压缩包.

软件需要安装依赖框架 [.NET 桌面运行时 8.0.x](https://dotnet.microsoft.com/zh-cn/download/dotnet/8.0).

也可以下载带有 `SelfContained` 后缀的压缩包, 可以独立运行.

导出 GIF 等视频格式需要在本地安装 ffmpeg 命令行, 并且添加至环境变量, [点击前往 FFmpeg-Windows 下载页面](https://ffmpeg.org/download.html#build-windows), 也可以点这个下载最新版本 [ffmpeg-release-full.7z](https://www.gyan.dev/ffmpeg/builds/ffmpeg-release-full.7z).

## 使用方法

### 骨骼导入

有 3 种方式导入骨骼文件:

- 拖放/粘贴需要导入的骨骼文件/目录到模型列表
- 从文件菜单里批量打开骨骼文件
- 从文件菜单选择单个模型打开

### 内容调整

模型列表支持右键菜单以及部分快捷键, 并且可以多选进行模型参数的批量调整.

预览画面除了使用面板进行参数设置外, 支持部分鼠标动作:

- 左键可以选择和拖拽模型, 按下 `Ctrl` 键可以实现多选, 与左侧列表选择是联动的.
- 右键对整体画面进行拖动.
- 滚轮进行画面缩放, 按住 `Ctrl` 可以对选中的模型进行批量缩放.
- 仅渲染选中模式, 在该模式下, 预览画面仅包含被选中的模型, 并且只能通过左侧列表改变选中状态.

预览画面下方按钮支持对画面时间进行调整, 可以当作一个简易的播放器.

### 内容导出

导出遵循 "所见即所得" 原则, 即实时预览的画面就是你导出的画面.

导出有以下几个关键参数:

- 仅渲染选中. 这个参数不仅影响预览模式, 也影响导出, 如果仅渲染选中, 那么在导出时只有被选中的模型会被考虑, 忽略其他模型.
- 输出文件夹. 这个参数某些时候可选, 当不提供时, 则将输出产物输出到每个模型各自的模型文件夹, 否则输出产物全部输出到提供的输出文件夹.
- 导出单个. 默认是每个模型独立导出, 即对模型列表进行批量操作, 如果选择仅导出单个, 那么被导出的所有模型将在同一个画面上被渲染, 输出产物只有一份.
- 自动分辨率. 该模式会忽略预览画面的分辨率和视区参数, 导出产物的分辨率与被导出内容的实际大小一致, 如果是动图或者视频则会与完整显示动画的必需大小一致.

### 更多

更为详细的使用方法和说明见 [Wiki](https://github.com/ww-rm/SpineViewer/wiki), 有使用上的问题或者 BUG 可以提个 [Issue](https://github.com/ww-rm/SpineViewer/issues).

## Acknowledgements

- [spine-runtimes](https://github.com/EsotericSoftware/spine-runtimes)
- [SFML.Net](https://github.com/SFML/SFML.Net)
- [FFMpegCore](https://github.com/rosenbjerg/FFMpegCore)

---

*如果你觉得这个项目不错请给个 :star:, 并分享给更多人知道! :)*

[![Stargazers over time](https://starchart.cc/ww-rm/SpineViewer.svg?variant=adaptive)](https://starchart.cc/ww-rm/SpineViewer)
