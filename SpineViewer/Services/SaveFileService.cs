using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Services
{
    public static class SaveService
    {
        /// <summary>
        /// 获取用户选择的文件夹
        /// </summary>
        /// <param name="selectedPath"></param>
        /// <returns>是否确认了选择</returns>
        public static bool SaveFile(out string? selectedPath)
        {
            var dialog = new SaveFileDialog() { };
            selectedPath = null;
            // TODO
            return false;
        }
    }
}
