using AppStudio.Uwp.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml;

namespace AppStudio.Uwp.Controls.Html.Writers
{
    class SpanWriter : HtmlWriter
    {
        public override string[] TargetTags
        {
            get { return new string[] { "span" }; }
        }

        public override DependencyObject GetControl(HtmlFragment fragment)
        {
            return new Span();
        }

        public override void ApplyStyles(DocumentStyle style, DependencyObject ctrl, HtmlFragment fragment)
        {
            ApplyTextStyles(ctrl as Span, style.Span);
        }

        //public void ApplySpanStyles(DocumentStyle style, DependencyObject ctrl, HtmlFragment fragment, Dictionary<string, TextPointer> referenceAnchors)
        //{
        //    var node = fragment as HtmlNode;
        //    if (node != null && ctrl is Span)
        //    {
        //        var span = ctrl as Span;
        //        if (node.Attributes.ContainsKey("class") && node.Attributes["class"].Contains("line-ref"))
        //        {
        //            var lineRef = node.AsText().Content;
        //            if (lineRef.Contains("{line:"))
        //            {
        //                lineRef = lineRef.Substring(lineRef.IndexOf("{line:"), lineRef.Length - 1);
        //                if(referenceAnchors.ContainsKey(lineRef))
        //                {
        //                    return;
        //                }
        //            }

        //        }
        //    }
        //    ApplyTextStyles(ctrl as Span, style.Span);
        //}



    }
}
