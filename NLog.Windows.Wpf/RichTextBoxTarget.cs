using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace NLog.Windows.Wpf
{
    [Target("RichTextBox")]
    public sealed class RichTextBoxTarget : TargetWithLayout
    {
        public static ReadOnlyCollection<RichTextBoxRowColoringRule> DefaultRowColoringRules { get; } = CreateDefaultColoringRules();

        private static ReadOnlyCollection<RichTextBoxRowColoringRule> CreateDefaultColoringRules()
        {
            return new List<RichTextBoxRowColoringRule>()
            {
                new RichTextBoxRowColoringRule("level == LogLevel.Fatal", "White", "Red", FontStyles.Normal, FontWeights.Bold),
                new RichTextBoxRowColoringRule("level == LogLevel.Error", "Red", "Empty", FontStyles.Italic, FontWeights.Bold),
                new RichTextBoxRowColoringRule("level == LogLevel.Warn", "Orange", "Empty"),
                new RichTextBoxRowColoringRule("level == LogLevel.Info", "Black", "Empty"),
                new RichTextBoxRowColoringRule("level == LogLevel.Debug", "Gray", "Empty"),
                new RichTextBoxRowColoringRule("level == LogLevel.Trace", "DarkGray", "Empty", FontStyles.Italic, FontWeights.Normal),
            }.AsReadOnly();
        }

        public RichTextBoxTarget() { }

        public string ControlName { get; set; }

        public string WindowName { get; set; }

        public bool UseDefaultRowColoringRules { get; set; }

        public bool AutoScroll { get; set; }

        public int MaxLines { get; set; }

        [ArrayParameter(typeof(RichTextBoxRowColoringRule), "row-coloring")]
        public IList<RichTextBoxRowColoringRule> RowColoringRules { get; } = new List<RichTextBoxRowColoringRule>();

        [ArrayParameter(typeof(RichTextBoxWordColoringRule), "word-coloring")]
        public IList<RichTextBoxWordColoringRule> WordColoringRules { get; } = new List<RichTextBoxWordColoringRule>();

        [NLogConfigurationIgnoreProperty]
        public Window TargetWindow { get; set; }

        [NLogConfigurationIgnoreProperty]
        public RichTextBox TargetRichTextBox { get; set; }

        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            if (TargetRichTextBox != null)
                return;

            if (WindowName == null)
            {
                HandleError("WindowName should be specified for {0}.{1}", GetType().Name, Name);
                return;
            }

            if (string.IsNullOrEmpty(ControlName))
            {
                HandleError("Rich text box control name must be specified for {0}.{1}", GetType().Name, Name);
                return;
            }

            var targetWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w.Name == WindowName);
            if (targetWindow == null)
            {
                InternalLogger.Info("{0}: WindowName '{1}' not found", this, WindowName);
                return;
            }

            var targetControl = targetWindow.FindName(ControlName) as RichTextBox;
            if (targetControl == null)
            {
                InternalLogger.Info("{0}: WIndowName '{1}' does not contain ControlName '{2}'", this, WindowName, ControlName);
                return;
            }

            AttachToControl(targetWindow, targetControl);
        }

        private static void HandleError(string message, params object[] args)
        {
            if (LogManager.ThrowExceptions)
            {
                throw new NLogConfigurationException(string.Format(message, args));
            }
            InternalLogger.Error(message, args);
        }

        private void AttachToControl(Window window, RichTextBox textboxControl)
        {
            InternalLogger.Info("{0}: Attaching target to textbox {1}.{2}", this, window.Name, textboxControl.Name);
            DetachFromControl();
            TargetWindow = window;
            TargetRichTextBox = textboxControl;
        }

        private void DetachFromControl()
        {
            TargetWindow = null;
            TargetRichTextBox = null;
        }

        protected override void CloseTarget()
        {
            DetachFromControl();
        }

        protected override void Write(LogEventInfo logEvent)
        {
            RichTextBox textbox = TargetRichTextBox;
            if (textbox == null || textbox.Dispatcher.HasShutdownStarted || textbox.Dispatcher.HasShutdownFinished)
            {
                //no last logged textbox
                InternalLogger.Trace("{0}: Attached Textbox is {1}, skipping logging", this, textbox == null ? "null" : "disposed");
                return;
            }

            string logMessage = RenderLogEvent(Layout, logEvent);
            RichTextBoxRowColoringRule matchingRule = FindMatchingRule(logEvent);
            _ = DoSendMessageToTextbox(logMessage, matchingRule, logEvent);
        }

        private bool DoSendMessageToTextbox(string logMessage, RichTextBoxRowColoringRule rule, LogEventInfo logEvent)
        {
            RichTextBox textbox = TargetRichTextBox;
            try
            {
                if (textbox != null && !textbox.Dispatcher.HasShutdownStarted && !textbox.Dispatcher.HasShutdownFinished)
                {
                    if (!textbox.Dispatcher.CheckAccess())
                    {
                        textbox.Dispatcher.BeginInvoke(() => SendTheMessageToRichTextBox(textbox, logMessage, rule, logEvent));
                    }
                    else
                    {
                        SendTheMessageToRichTextBox(textbox, logMessage, rule, logEvent);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                InternalLogger.Warn(ex, "{0}: Failed to append RichTextBox", this);

                if (LogManager.ThrowExceptions)
                {
                    throw;
                }
            }
            return false;
        }

        private RichTextBoxRowColoringRule FindMatchingRule(LogEventInfo logEvent)
        {
            //custom rules first
            if (RowColoringRules.Count > 0)
            {
                foreach (RichTextBoxRowColoringRule coloringRule in RowColoringRules)
                {
                    if (coloringRule.CheckCondition(logEvent))
                    {
                        return coloringRule;
                    }
                }
            }

            if (UseDefaultRowColoringRules && DefaultRowColoringRules != null)
            {
                foreach (RichTextBoxRowColoringRule coloringRule in DefaultRowColoringRules)
                {
                    if (coloringRule.CheckCondition(logEvent))
                    {
                        return coloringRule;
                    }
                }
            }

            return RichTextBoxRowColoringRule.Default;
        }

        private void SendTheMessageToRichTextBox(RichTextBox textBox, string logMessage, RichTextBoxRowColoringRule rule, LogEventInfo logEvent)
        {
            if (textBox == null) return;

            var document = textBox.Document;

            // 插入文本（带换行）
            var tr = new TextRange(document.ContentEnd, document.ContentEnd)
            {
                Text = logMessage + Environment.NewLine
            };

            // 设置行级样式
            var fgColor = rule.FontColor?.Render(logEvent);
            var bgColor = rule.BackgroundColor?.Render(logEvent);

            tr.ApplyPropertyValue(TextElement.ForegroundProperty,
                string.IsNullOrEmpty(fgColor) || fgColor == "Empty"
                    ? textBox.Foreground
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString(fgColor)));

            tr.ApplyPropertyValue(TextElement.BackgroundProperty,
                string.IsNullOrEmpty(bgColor) || bgColor == "Empty"
                    ? Brushes.Transparent
                    : new SolidColorBrush((Color)ColorConverter.ConvertFromString(bgColor)));

            tr.ApplyPropertyValue(TextElement.FontStyleProperty, rule.FontStyle);
            tr.ApplyPropertyValue(TextElement.FontWeightProperty, rule.FontWeight);

            // Word coloring（在刚插入的范围内做匹配）
            if (WordColoringRules.Count > 0)
            {
                foreach (var wordRule in WordColoringRules)
                {
                    var pattern = wordRule.Regex?.Render(logEvent) ?? string.Empty;
                    var text = wordRule.Text?.Render(logEvent) ?? string.Empty;
                    var wholeWords = wordRule.WholeWords.RenderValue(logEvent);
                    var ignoreCase = wordRule.IgnoreCase.RenderValue(logEvent);

                    var regex = wordRule.ResolveRegEx(pattern, text, wholeWords, ignoreCase);
                    var matches = regex.Matches(tr.Text);

                    foreach (Match match in matches)
                    {
                        // 匹配到的部分范围
                        var start = tr.Start.GetPositionAtOffset(match.Index, LogicalDirection.Forward);
                        var endPos = tr.Start.GetPositionAtOffset(match.Index + match.Length, LogicalDirection.Backward);
                        if (start == null || endPos == null) continue;

                        var wordRange = new TextRange(start, endPos);

                        var wordFg = wordRule.FontColor?.Render(logEvent);
                        var wordBg = wordRule.BackgroundColor?.Render(logEvent);

                        wordRange.ApplyPropertyValue(TextElement.ForegroundProperty,
                            string.IsNullOrEmpty(wordFg) || wordFg == "Empty"
                                ? tr.GetPropertyValue(TextElement.ForegroundProperty)
                                : new SolidColorBrush((Color)ColorConverter.ConvertFromString(wordFg)));

                        wordRange.ApplyPropertyValue(TextElement.BackgroundProperty,
                            string.IsNullOrEmpty(wordBg) || wordBg == "Empty"
                                ? tr.GetPropertyValue(TextElement.BackgroundProperty)
                                : new SolidColorBrush((Color)ColorConverter.ConvertFromString(wordBg)));

                        wordRange.ApplyPropertyValue(TextElement.FontStyleProperty, wordRule.FontStyle);
                        wordRange.ApplyPropertyValue(TextElement.FontWeightProperty, wordRule.FontWeight);
                    }
                }
            }

            // 限制最大行数
            if (MaxLines > 0)
            {
                while (document.Blocks.Count > MaxLines)
                {
                    document.Blocks.Remove(document.Blocks.FirstBlock);
                }
            }

            // 自动滚动到最后
            if (AutoScroll)
            {
                textBox.ScrollToEnd();
            }
        }
    }
}