﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpineViewer.Forms
{
    [ToolboxItem(true)]
    [Designer(typeof(ComponentDesigner), typeof(IDesigner))]
    [DesignTimeVisible(true)]
    public partial class SpinePreviewFullScreenForm: Form
    {
        public SpinePreviewFullScreenForm()
        {
            InitializeComponent();
        }
    }
}
