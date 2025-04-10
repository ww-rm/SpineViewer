using SpineViewer.Spine;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.PropertyGridWrappers.Spine
{
    /// <summary>
    /// 用于在 PropertyGrid 上显示 Spine 基本信息的包装类
    /// </summary>
    public class SpineBaseInfoWrapper(SpineObject spine)
    {
        [Browsable(false)]
        public SpineObject Spine { get; } = spine;

        /// <summary>
        /// 获取所属版本
        /// </summary>
        [TypeConverter(typeof(SpineVersionConverter))]
        [DisplayName("运行时版本")]
        public SpineVersion Version => Spine.Version;

        /// <summary>
        /// 资源所在完整目录
        /// </summary>
        [DisplayName("资源目录")]
        public string AssetsDir => Spine.AssetsDir;

        /// <summary>
        /// skel 文件完整路径
        /// </summary>
        [DisplayName("skel文件路径")]
        public string SkelPath => Spine.SkelPath;

        /// <summary>
        /// atlas 文件完整路径
        /// </summary>
        [DisplayName("atlas文件路径")]
        public string AtlasPath => Spine.AtlasPath;

        /// <summary>
        /// 名称
        /// </summary>
        [DisplayName("名称")]
        public string Name => Spine.Name;

        /// <summary>
        /// 获取所属文件版本
        /// </summary>
        [DisplayName("文件版本")]
        public string FileVersion => Spine.FileVersion;
    }
}
