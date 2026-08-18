using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Spine.Implementations
{
    /// <summary>
    /// 实现不同版本的空附件加载器, 用于无 atlas 文件的情况
    /// </summary>
    public class EmptyAttachmentLoader :
        SpineRuntime21.AttachmentLoader,
        SpineRuntime34.AttachmentLoader,
        SpineRuntime35.AttachmentLoader,
        SpineRuntime36.AttachmentLoader,
        SpineRuntime37.AttachmentLoader,
        SpineRuntime38.Attachments.AttachmentLoader,
        SpineRuntime40.AttachmentLoader,
        SpineRuntime41.AttachmentLoader,
        SpineRuntime42.AttachmentLoader
    {
        /// <summary>
        /// 默认的全局空附件加载器
        /// </summary>
        public static EmptyAttachmentLoader DefaultLoader { get; } = new();

        public SpineRuntime21.BoundingBoxAttachment NewBoundingBoxAttachment(SpineRuntime21.Skin skin, string name) => new(name);
        public SpineRuntime21.MeshAttachment NewMeshAttachment(SpineRuntime21.Skin skin, string name, string path) => new(name);
        public SpineRuntime21.RegionAttachment NewRegionAttachment(SpineRuntime21.Skin skin, string name, string path) => new(name);
        public SpineRuntime21.SkinnedMeshAttachment NewSkinnedMeshAttachment(SpineRuntime21.Skin skin, string name, string path) => new(name);

        public SpineRuntime34.BoundingBoxAttachment NewBoundingBoxAttachment(SpineRuntime34.Skin skin, string name) => new(name);
        public SpineRuntime34.MeshAttachment NewMeshAttachment(SpineRuntime34.Skin skin, string name, string path) => new(name);
        public SpineRuntime34.PathAttachment NewPathAttachment(SpineRuntime34.Skin skin, string name) => new(name);
        public SpineRuntime34.RegionAttachment NewRegionAttachment(SpineRuntime34.Skin skin, string name, string path) => new(name);

        public SpineRuntime35.BoundingBoxAttachment NewBoundingBoxAttachment(SpineRuntime35.Skin skin, string name) => new(name);
        public SpineRuntime35.MeshAttachment NewMeshAttachment(SpineRuntime35.Skin skin, string name, string path) => new(name);
        public SpineRuntime35.PathAttachment NewPathAttachment(SpineRuntime35.Skin skin, string name) => new(name);
        public SpineRuntime35.RegionAttachment NewRegionAttachment(SpineRuntime35.Skin skin, string name, string path) => new(name);

        public SpineRuntime36.BoundingBoxAttachment NewBoundingBoxAttachment(SpineRuntime36.Skin skin, string name) => new(name);
        public SpineRuntime36.MeshAttachment NewMeshAttachment(SpineRuntime36.Skin skin, string name, string path) => new(name);
        public SpineRuntime36.PathAttachment NewPathAttachment(SpineRuntime36.Skin skin, string name) => new(name);
        public SpineRuntime36.RegionAttachment NewRegionAttachment(SpineRuntime36.Skin skin, string name, string path) => new(name);
        public SpineRuntime36.PointAttachment NewPointAttachment(SpineRuntime36.Skin skin, string name) => new(name);
        public SpineRuntime36.ClippingAttachment NewClippingAttachment(SpineRuntime36.Skin skin, string name) => new(name);

        public SpineRuntime37.BoundingBoxAttachment NewBoundingBoxAttachment(SpineRuntime37.Skin skin, string name) => new(name);
        public SpineRuntime37.MeshAttachment NewMeshAttachment(SpineRuntime37.Skin skin, string name, string path) => new(name);
        public SpineRuntime37.PathAttachment NewPathAttachment(SpineRuntime37.Skin skin, string name) => new(name);
        public SpineRuntime37.RegionAttachment NewRegionAttachment(SpineRuntime37.Skin skin, string name, string path) => new(name);
        public SpineRuntime37.PointAttachment NewPointAttachment(SpineRuntime37.Skin skin, string name) => new(name);
        public SpineRuntime37.ClippingAttachment NewClippingAttachment(SpineRuntime37.Skin skin, string name) => new(name);

        public SpineRuntime38.Attachments.BoundingBoxAttachment NewBoundingBoxAttachment(SpineRuntime38.Skin skin, string name) => new(name);
        public SpineRuntime38.Attachments.MeshAttachment NewMeshAttachment(SpineRuntime38.Skin skin, string name, string path) => new(name);
        public SpineRuntime38.Attachments.PathAttachment NewPathAttachment(SpineRuntime38.Skin skin, string name) => new(name);
        public SpineRuntime38.Attachments.RegionAttachment NewRegionAttachment(SpineRuntime38.Skin skin, string name, string path) => new(name);
        public SpineRuntime38.Attachments.PointAttachment NewPointAttachment(SpineRuntime38.Skin skin, string name) => new(name);
        public SpineRuntime38.Attachments.ClippingAttachment NewClippingAttachment(SpineRuntime38.Skin skin, string name) => new(name);

        public SpineRuntime40.BoundingBoxAttachment NewBoundingBoxAttachment(SpineRuntime40.Skin skin, string name) => new(name);
        public SpineRuntime40.MeshAttachment NewMeshAttachment(SpineRuntime40.Skin skin, string name, string path) => new(name);
        public SpineRuntime40.PathAttachment NewPathAttachment(SpineRuntime40.Skin skin, string name) => new(name);
        public SpineRuntime40.RegionAttachment NewRegionAttachment(SpineRuntime40.Skin skin, string name, string path) => new(name);
        public SpineRuntime40.PointAttachment NewPointAttachment(SpineRuntime40.Skin skin, string name) => new(name);
        public SpineRuntime40.ClippingAttachment NewClippingAttachment(SpineRuntime40.Skin skin, string name) => new(name);

        public SpineRuntime41.BoundingBoxAttachment NewBoundingBoxAttachment(SpineRuntime41.Skin skin, string name) => new(name);
        public SpineRuntime41.MeshAttachment NewMeshAttachment(SpineRuntime41.Skin skin, string name, string path, SpineRuntime41.Sequence sequence) => new(name);
        public SpineRuntime41.PathAttachment NewPathAttachment(SpineRuntime41.Skin skin, string name) => new(name);
        public SpineRuntime41.RegionAttachment NewRegionAttachment(SpineRuntime41.Skin skin, string name, string path, SpineRuntime41.Sequence sequence) => new(name);
        public SpineRuntime41.PointAttachment NewPointAttachment(SpineRuntime41.Skin skin, string name) => new(name);
        public SpineRuntime41.ClippingAttachment NewClippingAttachment(SpineRuntime41.Skin skin, string name) => new(name);

        public SpineRuntime42.BoundingBoxAttachment NewBoundingBoxAttachment(SpineRuntime42.Skin skin, string name) => new(name);
        public SpineRuntime42.MeshAttachment NewMeshAttachment(SpineRuntime42.Skin skin, string name, string path, SpineRuntime42.Sequence sequence) => new(name);
        public SpineRuntime42.PathAttachment NewPathAttachment(SpineRuntime42.Skin skin, string name) => new(name);
        public SpineRuntime42.RegionAttachment NewRegionAttachment(SpineRuntime42.Skin skin, string name, string path, SpineRuntime42.Sequence sequence) => new(name);
        public SpineRuntime42.PointAttachment NewPointAttachment(SpineRuntime42.Skin skin, string name) => new(name);
        public SpineRuntime42.ClippingAttachment NewClippingAttachment(SpineRuntime42.Skin skin, string name) => new(name);
    }
}
