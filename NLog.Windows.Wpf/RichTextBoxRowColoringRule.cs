using NLog;
using NLog.Conditions;
using NLog.Config;
using NLog.Layouts;
using System.Windows;

namespace NLog.Windows.Wpf
{
    [NLogConfigurationItem]
    public class RichTextBoxRowColoringRule
    {
        public static RichTextBoxRowColoringRule Default { get; private set; }

        [RequiredParameter]
        public ConditionExpression Condition { get; set; }

        public Layout FontColor { get; set; }
        public Layout BackgroundColor { get; set; }

        public FontStyle FontStyle { get; set; }
        public FontWeight FontWeight { get; set; }

        static RichTextBoxRowColoringRule()
        {
            RichTextBoxRowColoringRule.Default = new RichTextBoxRowColoringRule();
        }

        public RichTextBoxRowColoringRule() : this(null, "Empty", "Empty", FontStyles.Normal, FontWeights.Normal) { }

        public RichTextBoxRowColoringRule(string condition, string fontColor, string backColor)
        {
            this.Condition = (ConditionExpression)condition;
            this.FontColor = Layout.FromString(fontColor);
            this.BackgroundColor = Layout.FromString(backColor);
            this.FontStyle = FontStyles.Normal;
            this.FontWeight = FontWeights.Normal;
        }

        public RichTextBoxRowColoringRule(string condition, string fontColor, string backColor, FontStyle fontStyle, FontWeight fontWeight)
        {
            this.Condition = (ConditionExpression)condition;
            this.FontColor = Layout.FromString(fontColor);
            this.BackgroundColor = Layout.FromString(backColor);
            this.FontStyle = fontStyle;
            this.FontWeight = fontWeight;
        }

        public bool CheckCondition(LogEventInfo logEvent)
        {
            return true.Equals(this.Condition.Evaluate(logEvent));
        }
    }
}
