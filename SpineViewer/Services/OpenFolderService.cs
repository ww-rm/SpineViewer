using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Services
{
    public static class OpenFolderService
    {
        /// <summary>
        /// 获取用户选择的文件夹
        /// </summary>
        /// <param name="selectedPath"></param>
        /// <returns>是否确认了选择</returns>
        public static bool OpenFolder(out string? selectedPath)
        {
            // XXX: 此处使用了 System.Windows.Forms 的文件夹浏览对话框
            using var folderDialog = new System.Windows.Forms.FolderBrowserDialog();
            if (folderDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                selectedPath = folderDialog.SelectedPath;
                return true;
            }
            selectedPath = null;
            return false;
        }
    }
}
