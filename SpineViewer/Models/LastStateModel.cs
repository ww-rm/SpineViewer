using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SpineViewer.Models
{
    public class LastStateModel
    {
        #region 画面布局状态

        public double WindowLeft { get; set; }
        public double WindowTop { get; set; }
        public double WindowWidth { get; set; }
        public double WindowHeight { get; set; }
        public WindowState WindowState { get; set; }
        
        public double RootGridCol0Width { get; set; }
        public double ModelListRow0Height { get; set; }
        public double ExplorerGridRow0Height { get; set; }
        public double RightPanelGridRow0Height { get; set; }

        #endregion

        #region 预览画面状态

        public uint ResolutionX { get; set; } = 1500;
        public uint ResolutionY { get; set; } = 1000;
        public uint MaxFps { get; set; } = 30;
        public float Speed { get; set; } = 1f;
        public bool ShowAxis { get; set; } = true;
        public Color BackgroundColor { get; set; } = Color.FromRgb(105, 105, 105);

        #endregion

    }
}
