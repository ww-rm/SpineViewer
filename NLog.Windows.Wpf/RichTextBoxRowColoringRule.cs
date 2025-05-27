// 
// Copyright (c) 2004-2011 Jaroslaw Kowalski <jaak@jkowalski.net>
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// 

using NLog.Conditions;
using NLog.Config;
using NLog;
using System.ComponentModel;
using System.Windows;
namespace NLog.Windows.Wpf
{
    [NLogConfigurationItem]
    public class RichTextBoxRowColoringRule
    {
        static RichTextBoxRowColoringRule()
        {
            Default = new RichTextBoxRowColoringRule();
        }

        public RichTextBoxRowColoringRule()
            : this(null, "Empty", "Empty", FontStyles.Normal, FontWeights.Normal)
        {
        }

        public RichTextBoxRowColoringRule(string condition, string fontColor, string backColor, FontStyle fontStyle, FontWeight fontWeight)
        {
            Condition = condition;
            FontColor = fontColor;
            BackgroundColor = backColor;
            Style = fontStyle;
            Weight = fontWeight;
        }

        public RichTextBoxRowColoringRule(string condition, string fontColor, string backColor)
        {
            Condition = condition;
            FontColor = fontColor;
            BackgroundColor = backColor;
            Style = FontStyles.Normal;
            Weight = FontWeights.Normal;
        }

        public static RichTextBoxRowColoringRule Default { get; private set; }

        [RequiredParameter]
        public ConditionExpression Condition { get; set; }

        [DefaultValue("Empty")]
        public string FontColor { get; set; }

        [DefaultValue("Empty")]
        public string BackgroundColor { get; set; }

        public FontStyle Style { get; set; }

        public FontWeight Weight { get; set; }

        public bool CheckCondition(LogEventInfo logEvent)
        {
            return true.Equals(Condition.Evaluate(logEvent));
        }
    }
}
