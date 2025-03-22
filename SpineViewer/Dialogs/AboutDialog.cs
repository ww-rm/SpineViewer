﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpineViewer.Dialogs
{
    partial class AboutDialog : Form
    {
        public AboutDialog()
        {
            InitializeComponent();
            Text = $"关于 {Program.Name}";
            label_Version.Text = $"v{InformationalVersion}";
        }

        public string InformationalVersion => 
            Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;

        private void linkLabel_RepoUrl_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            string url = linkLabel_RepoUrl.Text;
            if (Control.ModifierKeys == Keys.Control)
            {
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            else
            {
                Clipboard.SetText(url);
                MessageBox.Info("链接已复制到剪贴板，请前往浏览器进行访问");
            }
        }
    }
}
