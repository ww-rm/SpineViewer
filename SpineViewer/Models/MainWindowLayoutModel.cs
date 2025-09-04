using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SpineViewer.Models
{
    public class MainWindowLayoutModel
    {
        public double WindowLeft { get; set; }
        public double WindowTop { get; set; }
        public double WindowWidth { get; set; }
        public double WindowHeight { get; set; }
        public WindowState WindowState { get; set; }
        
        public double RootGridCol0Width { get; set; }
        public double ModelListRow0Height { get; set; }
        public double ExplorerGridRow0Height { get; set; }
        public double RightPanelGridRow0Height { get; set; }
    }
}
