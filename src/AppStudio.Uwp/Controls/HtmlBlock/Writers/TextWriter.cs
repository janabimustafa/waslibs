﻿using AppStudio.Uwp.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml;

namespace AppStudio.Uwp.Controls.Html.Writers
{
    class TextWriter : HtmlWriter
    {
        public override string[] TargetTags
        {
            get { return new string[] { "text" }; }
        }

        public override DependencyObject GetControl(HtmlFragment fragment)
        {
            var text = fragment?.AsText();
            if (text != null && !string.IsNullOrEmpty(text.Content))
            {
                return new Run
                {
                    Text = text.Content
                };
            }
            return null;
        }

        internal void ApplyAnchorStyles(DocumentStyle _docStyles, DependencyObject ctrl, HtmlFragment fragment, HtmlFragment parentFragment, Dictionary<string, TextPointer> referenceAnchors, HtmlBlock referenceBlock)
        {
            var node = parentFragment as HtmlNode;
            if (node != null && ctrl is Run)
            {
                var run = ctrl as Run;
                if (node.Attributes.ContainsKey("class") && node.Attributes["class"].Contains("line-ref"))
                {
                    var lineRef = fragment.AsText().Content;
                    if (lineRef.Contains("{line:"))
                    {
                        lineRef = lineRef.Substring(lineRef.IndexOf(':') + 1, lineRef.Length - (lineRef.IndexOf(':') + 2));
                        if (referenceAnchors.ContainsKey(lineRef))
                        {
                            var rect = referenceAnchors[lineRef].GetCharacterRect(referenceAnchors[lineRef].LogicalDirection);
                            run.Text = (Math.Round(rect.Top / referenceBlock.LineHeight)).ToString();
                        }
                    }

                }
            }
        }
    }
}
