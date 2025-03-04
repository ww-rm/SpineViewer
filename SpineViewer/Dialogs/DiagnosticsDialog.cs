﻿using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpineViewer.Dialogs
{
    public partial class DiagnosticsDialog : Form
    {
        public DiagnosticsDialog()
        {
            InitializeComponent();
            propertyGrid.SelectedObject = new DiagnosticsInformation();
        }

        private class DiagnosticsInformation
        {
            [Category("Versions")]
            public string WindowsVersion
            {
                get
                {
                    var registryKeyPath = @"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion";
                    var productName = Registry.GetValue(registryKeyPath, "ProductName", "Unknown") as string;
                    var editionId = Registry.GetValue(registryKeyPath, "EditionID", "Unknown") as string;
                    var osVersion = Environment.OSVersion.ToString();
                    return $"{productName}, {editionId}, {osVersion}";
                }
            }

            [Category("Versions")]
            public string Version
            {
                get => Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
            }

            [Category("Versions")]
            public string DotNetVersion
            {
                get => Environment.Version.ToString();
            }

            [Category("Versions")]
            public string SFMLVersion
            {
                get => typeof(SFML.ObjectBase).Assembly.GetName().Version.ToString();
            }

            [Category("Hardwares")]
            public string CPU
            {
                get => Registry.GetValue(@"HKEY_LOCAL_MACHINE\HARDWARE\DESCRIPTION\System\CentralProcessor\0", "ProcessorNameString", "Unknown").ToString();
            }

            [Category("Hardwares")]
            public string Memory
            {
                get => $"{new Microsoft.VisualBasic.Devices.ComputerInfo().TotalPhysicalMemory / 1024f / 1024f / 1024f:F1} GB";
            }

            [Category("Hardwares")]
            public string GPU
            {
                get
                {
                    var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_VideoController");
                    return string.Join("; ", searcher.Get().Cast<ManagementObject>().Select(mo => mo["Name"].ToString()));
                }
            }
        }

        private void button_Copy_Click(object sender, EventArgs e)
        {
            var selectedObject = propertyGrid.SelectedObject as DiagnosticsInformation;
            var properties = selectedObject.GetType().GetProperties();
            var result = string.Join(Environment.NewLine, properties.Select(p => $"{p.Name}\t{p.GetValue(selectedObject)?.ToString()}"));
            Clipboard.SetText(result);
        }
    }
}
