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
    class UWriter : HtmlWriter
    {
        public override string[] TargetTags
        {
            get { return new string[] { "u" }; }
        }

        public override DependencyObject GetControl(HtmlFragment fragment)
        {
            return new Underline();
        }

        public override void ApplyStyles(DocumentStyle style, DependencyObject ctrl, HtmlFragment fragment)
        {
            ApplyTextStyles(ctrl as Underline, style.Q);
        }
    }
}
