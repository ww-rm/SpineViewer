using NLog.Config;
using NLog.Layouts;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;

namespace NLog.Windows.Wpf
{
    [NLogConfigurationItem]
    public class RichTextBoxWordColoringRule
    {
        public Layout Regex { get; set; }
        public Layout Text { get; set; }
        public Layout<bool> WholeWords { get; set; }
        public Layout<bool> IgnoreCase { get; set; }

        public Layout FontColor { get; set; }
        public Layout BackgroundColor { get; set; }

        public FontStyle FontStyle { get; set; }
        public FontWeight FontWeight { get; set; }

        internal Regex ResolveRegEx(string pattern, string text, bool wholeWords, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(pattern) && text != null)
            {
                pattern = System.Text.RegularExpressions.Regex.Escape(text);
                if (wholeWords)
                    pattern = "\b" + pattern + "\b";
            }

            RegexOptions options = RegexOptions.None;
            if (ignoreCase)
                options |= RegexOptions.IgnoreCase;

            return new Regex(pattern, options);   // RegEx-Cache
        }

        public RichTextBoxWordColoringRule() : this(null, "Empty", "Empty", FontStyles.Normal, FontWeights.Normal) { }

        public RichTextBoxWordColoringRule(string text, string fontColor, string backgroundColor)
        {
            this.Text = text;
            this.FontColor = Layout.FromString(fontColor);
            this.BackgroundColor = Layout.FromString(backgroundColor);
            this.FontStyle = FontStyles.Normal;
            this.FontWeight = FontWeights.Normal;
        }

        public RichTextBoxWordColoringRule(string text, string textColor, string backgroundColor, FontStyle fontStyle, FontWeight fontWeight)
        {
            this.Text = text;
            this.FontColor = Layout.FromString(textColor);
            this.BackgroundColor = Layout.FromString(backgroundColor);
            this.FontStyle = fontStyle;
            this.FontWeight = fontWeight;
        }
    }
}
