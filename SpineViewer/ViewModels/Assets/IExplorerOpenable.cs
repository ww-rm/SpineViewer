using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.ViewModels.Assets
{
    public interface IExplorerOpenable
    {
        /// <summary>
        /// 本地目录
        /// </summary>
        public string LocalDirectory { get; }
    }
}
