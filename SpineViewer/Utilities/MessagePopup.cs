using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SpineViewer.Utilities
{
    /// <summary>
    /// 弹窗消息静态类
    /// </summary>
    public static class MessagePopup
    {
        /// <summary>
        /// 提示弹窗
        /// </summary>
        public static void Info(string text, string title = "提示信息") => 
            MessageBox.Show(text, title, MessageBoxButtons.OK, MessageBoxIcon.Information);

        /// <summary>
        /// 警告弹窗
        /// </summary>
        public static void Warn(string text, string title = "警告信息") =>
            MessageBox.Show(text, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);

        /// <summary>
        /// 错误弹窗
        /// </summary>
        public static void Error(string text, string title = "错误信息") =>
            MessageBox.Show(text, title, MessageBoxButtons.OK, MessageBoxIcon.Error);

        /// <summary>
        /// 操作确认弹窗
        /// </summary>
        public static DialogResult Quest(string text, string title = "操作确认") =>
            MessageBox.Show(text, title, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
    }
}
