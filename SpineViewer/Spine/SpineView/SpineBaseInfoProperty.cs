using SpineViewer.Spine;
using SpineViewer.Utils;
using SpineViewer.Utils.Localize;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Spine.SpineView
{
    /// <summary>
    /// 用于在 PropertyGrid 上显示 Spine 基本信息的包装类
    /// </summary>
    public class SpineBaseInfoProperty(SpineObject spine)
    {
        [Browsable(false)]
        public SpineObject Spine { get; } = spine;

        /// <summary>
        /// 获取所属版本
        /// </summary>
        [TypeConverter(typeof(SpineVersionConverter))]
		[LocalizedDisplayName(typeof(Properties.Resources), "runtimeVersion")]
        public SpineVersion Version => Spine.Version;

		/// <summary>
		/// 资源所在完整目录
		/// </summary>
		[LocalizedDisplayName(typeof(Properties.Resources), "resourcesPath")]
		public string AssetsDir => Spine.AssetsDir;

		/// <summary>
		/// skel 文件完整路径
		/// </summary>
		[LocalizedDisplayName(typeof(Properties.Resources), "skelPath")]
		public string SkelPath => Spine.SkelPath;

		/// <summary>
		/// atlas 文件完整路径
		/// </summary>
		[LocalizedDisplayName(typeof(Properties.Resources), "atlasPath")]
		public string AtlasPath => Spine.AtlasPath;

		/// <summary>
		/// 名称
		/// </summary>
		[LocalizedDisplayName(typeof(Properties.Resources), "name")]
		public string Name => Spine.Name;

		/// <summary>
		/// 获取所属文件版本
		/// </summary>
		[LocalizedDisplayName(typeof(Properties.Resources), "fileVersion")]
		public string FileVersion => Spine.FileVersion;
    }
}
