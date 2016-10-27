using AppStudio.Uwp.Html;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml;
using System.Diagnostics;

namespace AppStudio.Uwp.Controls.Html.Writers
{
    class AnchorWriter : HtmlWriter
    {
        public override string[] TargetTags
        {
            get { return new string[] { "a" }; }
        }

        public override DependencyObject GetControl(HtmlFragment fragment)
        {
            var node = fragment as HtmlNode;
            if (node != null)
            {
                if (node.Attributes.ContainsKey("href"))
                {
                    Hyperlink a = new Hyperlink();

                    Uri uri;

                    if (Uri.TryCreate(node.Attributes["href"], UriKind.Absolute, out uri))
                    {
                        try
                        {
                            a.NavigateUri = uri;
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error loading a@href '{uri?.ToString()}': {ex.Message}");
                        }
                    }

                    return a;
                }
                else
                {



                    return new Underline();
                }
            }
            return null;
        }

        public override void ApplyStyles(DocumentStyle style, DependencyObject ctrl, HtmlFragment fragment)
        {
            ApplyTextStyles(ctrl as Span, style.A);
        }

        internal Dictionary<string, TextPointer> ApplyAnchorStyles(DocumentStyle style, DependencyObject ctrl, HtmlFragment fragment)
        {
            Dictionary<string, TextPointer> dict = new Dictionary<string, TextPointer>();
            var node = fragment as HtmlNode;
            if (node != null && ctrl is Underline)
            {
                var underline = ctrl as Underline;
                if (node.Attributes.ContainsKey("id") && node.Attributes.ContainsKey("name"))
                    dict[node.Attributes["name"]] = underline.ContentStart;
            }
            ApplyTextStyles(ctrl as Span, style.A);
            return dict;
        }
    }
}
