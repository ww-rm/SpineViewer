using SpineViewer.Exporter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SpineViewer.Dialogs
{
    public partial class ExportDialog: Form
    {
        /// <summary>
        /// 要绑定的导出参数
        /// </summary>
        public required ExportArgs ExportArgs
        {
            get => propertyGrid_ExportArgs.SelectedObject as ExportArgs;
            init
            {
                propertyGrid_ExportArgs.SelectedObject = value;

                #region XXX: 通过反射默认高亮指定的项
                var categories = propertyGrid_ExportArgs.SelectedGridItem?.Parent?.Parent?.GridItems;
                if (categories is null) return;

                foreach (var category in categories)
                {
                    // 查找 "导出" 分组
                    if (category == null) continue;
                    PropertyInfo? labelProp = category.GetType().GetProperty("Label", BindingFlags.Instance | BindingFlags.Public);
                    if (labelProp == null) continue;
                    string? label = labelProp.GetValue(category) as string;
                    if (label != "导出") continue;

                    // 获取该分组下的所有属性项
                    PropertyInfo? gridItemsProp = category.GetType().GetProperty("GridItems", BindingFlags.Instance | BindingFlags.Public);
                    if (gridItemsProp == null) continue;
                    var gridItemsObj = gridItemsProp.GetValue(category);
                    if (gridItemsObj is not IEnumerable gridItems) continue;

                    foreach (object item in gridItems)
                    {
                        if (item == null) continue;
                        PropertyInfo? propDescProp = item.GetType().GetProperty("PropertyDescriptor", BindingFlags.Instance | BindingFlags.Public);
                        if (propDescProp == null) continue;
                        var propDesc = propDescProp.GetValue(item) as PropertyDescriptor;
                        if (propDesc == null) continue;
                        if (propDesc.Name == "OutputDir")
                        {

                            if (item is GridItem gridItem)
                                propertyGrid_ExportArgs.SelectedGridItem = gridItem; // 找到后，将此项设为选中项
                            else
                                propertyGrid_ExportArgs.SelectedGridItem = (GridItem)item; // 如果转换失败，则尝试直接赋值
                        }
                        return; // 设置成功后退出
                    }
                }
                #endregion
            }
        }

        public ExportDialog()
        {
            InitializeComponent();
        }

        private void button_Ok_Click(object sender, EventArgs e)
        {
            if (ExportArgs.Validate() is string error)
            { 
                MessageBox.Info(error, "参数错误");
                return;
            }
            DialogResult = DialogResult.OK;
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
