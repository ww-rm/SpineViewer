using NLog.Config;
using NLog.Layouts;
using NLog.Targets;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows;

namespace NLog.Windows.Wpf
{
    // TODO: 完善日志实现
    [Target("RichTextBox")]
    public sealed class RichTextBoxTarget : TargetWithLayout
    {
        private int lineCount;
        private int _width = 500;
        private int _height = 500;
        private static readonly TypeConverter colorConverter = new ColorConverter();

        static RichTextBoxTarget()
        {
            var rules = new List<RichTextBoxRowColoringRule>()
            {
                new RichTextBoxRowColoringRule("level == LogLevel.Fatal", "White", "Red", FontStyles.Normal, FontWeights.Bold),
                new RichTextBoxRowColoringRule("level == LogLevel.Error", "Red", "Empty", FontStyles.Italic, FontWeights.Bold),
                new RichTextBoxRowColoringRule("level == LogLevel.Warn", "Orange", "Empty"),
                new RichTextBoxRowColoringRule("level == LogLevel.Info", "Black", "Empty"),
                new RichTextBoxRowColoringRule("level == LogLevel.Debug", "Gray", "Empty"),
                new RichTextBoxRowColoringRule("level == LogLevel.Trace", "DarkGray", "Empty", FontStyles.Italic, FontWeights.Normal),
            };

            DefaultRowColoringRules = rules.AsReadOnly();
        }

        public RichTextBoxTarget()
        {
            WordColoringRules = new List<RichTextBoxWordColoringRule>();
            RowColoringRules = new List<RichTextBoxRowColoringRule>();
            ToolWindow = true;
        }

        private delegate void DelSendTheMessageToRichTextBox(string logMessage, RichTextBoxRowColoringRule rule);

        private delegate void FormCloseDelegate();

        public static ReadOnlyCollection<RichTextBoxRowColoringRule> DefaultRowColoringRules { get; private set; }

        public string ControlName { get; set; }

        public string FormName { get; set; }

        [DefaultValue(false)]
        public bool UseDefaultRowColoringRules { get; set; }

        [ArrayParameter(typeof(RichTextBoxRowColoringRule), "row-coloring")]
        public IList<RichTextBoxRowColoringRule> RowColoringRules { get; private set; }

        [ArrayParameter(typeof(RichTextBoxWordColoringRule), "word-coloring")]
        public IList<RichTextBoxWordColoringRule> WordColoringRules { get; private set; }

        [DefaultValue(true)]
        public bool ToolWindow { get; set; }

        public bool ShowMinimized { get; set; }

        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }

        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }

        public bool AutoScroll { get; set; }

        public int MaxLines { get; set; }

        internal Window TargetForm { get; set; }

        internal RichTextBox TargetRichTextBox { get; set; }

        internal bool CreatedForm { get; set; }

        protected override void InitializeTarget()
        {
            TargetRichTextBox = Application.Current.MainWindow.FindName(ControlName) as RichTextBox;

            if (TargetRichTextBox != null) return;
            //this.TargetForm = FormHelper.CreateForm(this.FormName, this.Width, this.Height, false, this.ShowMinimized, this.ToolWindow);
            //this.CreatedForm = true;

            var openFormByName = Application.Current.Windows.Cast<Window>().FirstOrDefault(x => x.GetType().Name == FormName);
            if (openFormByName != null)
            {
                TargetForm = openFormByName;
                if (string.IsNullOrEmpty(ControlName))
                {
                    // throw new NLogConfigurationException("Rich text box control name must be specified for " + GetType().Name + ".");
                    Trace.WriteLine("Rich text box control name must be specified for " + GetType().Name + ".");
                }

                CreatedForm = false;
                TargetRichTextBox = TargetForm.FindName(ControlName) as RichTextBox;

                if (TargetRichTextBox == null)
                {
                    // throw new NLogConfigurationException("Rich text box control '" + ControlName + "' cannot be found on form '" + FormName + "'.");
                    Trace.WriteLine("Rich text box control '" + ControlName + "' cannot be found on form '" + FormName + "'.");
                }
            }

            if (TargetRichTextBox == null)
            {
                TargetForm = new Window
                {
                    Name = FormName,
                    Width = Width,
                    Height = Height,
                    WindowStyle = ToolWindow ? WindowStyle.ToolWindow : WindowStyle.None,
                    WindowState = ShowMinimized ? WindowState.Minimized : WindowState.Normal,
                    Title = "NLog Messages"
                };
                TargetForm.Show();

                TargetRichTextBox = new RichTextBox { Name = ControlName };
                var style = new Style(typeof(Paragraph));
                TargetRichTextBox.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
                style.Setters.Add(new Setter(Block.MarginProperty, new Thickness(0, 0, 0, 0)));
                TargetRichTextBox.Resources.Add(typeof(Paragraph), style);
                TargetForm.Content = TargetRichTextBox;

                CreatedForm = true;
            }
        }

        protected override void CloseTarget()
        {
            if (CreatedForm)
            {
                try
                {
                    TargetForm.Dispatcher.Invoke(() =>
                    {
                        TargetForm.Close();
                        TargetForm = null;
                    });
                }
                catch
                {
                }



            }
        }

        protected override void Write(LogEventInfo logEvent)
        {
            RichTextBoxRowColoringRule matchingRule = RowColoringRules.FirstOrDefault(rr => rr.CheckCondition(logEvent));

            if (UseDefaultRowColoringRules && matchingRule == null)
            {
                foreach (var rr in DefaultRowColoringRules.Where(rr => rr.CheckCondition(logEvent)))
                {
                    matchingRule = rr;
                    break;
                }
            }

            if (matchingRule == null)
            {
                matchingRule = RichTextBoxRowColoringRule.Default;
            }

            var logMessage = Layout.Render(logEvent);

            if (Application.Current == null) return;

            try
            {
                if (Application.Current.Dispatcher.CheckAccess() == false)
                {
                    Application.Current.Dispatcher.Invoke(() => SendTheMessageToRichTextBox(logMessage, matchingRule));
                }
                else
                {
                    SendTheMessageToRichTextBox(logMessage, matchingRule);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

        }


        private static Color GetColorFromString(string color, Brush defaultColor)
        {

            if (color == "Empty")
            {
                return defaultColor is SolidColorBrush solidBrush ? solidBrush.Color : Colors.White;
            }

            return (Color)colorConverter.ConvertFromString(color);
        }


        private void SendTheMessageToRichTextBox(string logMessage, RichTextBoxRowColoringRule rule)
        {
            RichTextBox rtbx = TargetRichTextBox;

            var tr = new TextRange(rtbx.Document.ContentEnd, rtbx.Document.ContentEnd);
            tr.Text = logMessage + "\n";
            tr.ApplyPropertyValue(TextElement.ForegroundProperty,
                new SolidColorBrush(GetColorFromString(rule.FontColor, (Brush)tr.GetPropertyValue(TextElement.ForegroundProperty)))
            );
            tr.ApplyPropertyValue(TextElement.BackgroundProperty,
                new SolidColorBrush(GetColorFromString(rule.BackgroundColor, (Brush)tr.GetPropertyValue(TextElement.BackgroundProperty)))
            );
            tr.ApplyPropertyValue(TextElement.FontStyleProperty, rule.Style);
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, rule.Weight);


            if (MaxLines > 0)
            {
                lineCount++;
                if (lineCount > MaxLines)
                {
                    tr = new TextRange(rtbx.Document.ContentStart, rtbx.Document.ContentEnd);
                    tr.Text.Remove(0, tr.Text.IndexOf('\n'));
                    lineCount--;
                }
            }

            if (AutoScroll)
            {
                rtbx.ScrollToEnd();
            }
        }
    }
}
