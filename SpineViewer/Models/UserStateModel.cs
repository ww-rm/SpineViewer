using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace SpineViewer.Models
{
    public class UserStateModel
    {
        #region 画面布局状态

        public double WindowLeft { get; set; }
        public double WindowTop { get; set; }
        public double WindowWidth { get; set; } = 1280;
        public double WindowHeight { get; set; } = 720;
        public WindowState WindowState { get; set; }
        
        public bool RootGridCol0Folded { get; set; }
        public double RootGridCol0Width { get; set; } = 100;
        public double RootGridCol2Width { get; set; } = 100;

        public double ModelListRow0Height { get; set; } = 100;
        public double ModelListRow2Height { get; set; } = 100;

        public double LocalAssetsGridRow0Height { get; set; } = 100;
        public double LocalAssetsGridRow2Height { get; set; } = 100;

        public double RightPanelGridRow0Height { get; set; } = 100;
        public double RightPanelGridRow2Height { get; set; } = 100;

        #endregion

        #region 预览画面状态

        public uint ResolutionX { get; set; } = 1500;
        public uint ResolutionY { get; set; } = 1000;
        public float Speed { get; set; } = 1f;
        public bool ShowAxis { get; set; } = true;
        public Color BackgroundColor { get; set; } = Color.FromRgb(105, 105, 105);
        public Stretch BackgroundImageMode { get; set; } = Stretch.Uniform;

        #endregion

    }
}
