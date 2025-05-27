# Spine

这个库封装了与 Spine 有关的底层功能, 下图给出主要关系.

```mermaid
classDiagram
direction TB

namespace Spine.SpineWrappers {
    class ISkeleton { <<Interface>> }
    class IAnimationState { <<Interface>> }
    class ISpineObjectData { <<Interface>> }
    class SpineObjectData { 
        <<Abstract>> 
        +CreateSkeleton() ISkeleton
        +CreateAnimationState() IAnimationState
    }
}

namespace Spine.Implementations.SpineWrappers.V38 {
    class Skeleton38
    class AnimationState38
    class SpineObjectData38
}

namespace Spine {
    class SpineObject {
        +ISpineObjectData Data
        +ISkeleton Skeleton
        +IAnimationState AnimationState
        +SpineObject(skelPath, atlasPath = null, version = null)
    }
}

ISpineObjectData <|.. SpineObjectData

Skeleton38 <.. SpineObjectData38
AnimationState38 <.. SpineObjectData38

ISkeleton <|.. Skeleton38
IAnimationState <|.. AnimationState38
SpineObjectData <|-- SpineObjectData38

SpineObjectData38 <.. SpineObject

```

```mermaid
classDiagram
direction LR

namespace Spine.Exporters {
    class BaseExporter {
         <<Abstract>>
         +Export(output, params spines)
    }

    class VideoExporter {
        <<Abstract>>
        +Export(output, ct, params spines)
    }

    class FrameExporter
    class FrameSequenceExporter
    class FFmpegVideoExporter
    class CustomFFmpegExporter
}

BaseExporter <|-- FrameExporter
BaseExporter <|-- VideoExporter
VideoExporter <|-- FrameSequenceExporter
VideoExporter <|-- FFmpegVideoExporter
VideoExporter <|-- CustomFFmpegExporter

```

## 如何扩展渲染支持的版本

在命名空间 `Spine.Implementations.SpineWrappers` 下, 增加扩展版本的子命名空间, 例如 `V38`.

随后实现 `SpineObjectData38`, 并继承自 `Spine.SpineWrappers.SpineObjectData`.

并在实现类上使用特性注解 `[SpineImplementation(3, 8)]`.
