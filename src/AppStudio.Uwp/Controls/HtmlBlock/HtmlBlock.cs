using AppStudio.Uwp.Controls.Html.Containers;
using AppStudio.Uwp.Controls.Html.Writers;
using AppStudio.Uwp.Html;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace AppStudio.Uwp.Controls
{
    public sealed class HtmlBlock : Control
    {
        public Grid _container;
        private Canvas _lineNumCanvas;
        private GridDocumentContainer documentContainer;
        private DocumentStyle _docStyles;
        private HtmlDocument _doc;
        private SemaphoreSlim _writeSema = new SemaphoreSlim(1, 1);
        public Dictionary<string, TextPointer> Anchors { get; set; } = new Dictionary<string, TextPointer>();
        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register("Source", typeof(string), typeof(HtmlBlock), new PropertyMetadata(null, SourcePropertyChanged));

        public string Source
        {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        private async static void SourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = d as HtmlBlock;
            await self.UpdateContentAsync();
        }

        public static readonly DependencyProperty DocumentStyleProperty = DependencyProperty.Register("DocumentStyle", typeof(DocumentStyle), typeof(HtmlBlock), new PropertyMetadata(new DocumentStyle(), DocumentStylesChanged));

        public DocumentStyle DocumentStyle
        {
            get { return (DocumentStyle)GetValue(DocumentStyleProperty); }
            set { SetValue(DocumentStyleProperty, value); }
        }




        public HtmlBlock LineReferenceBlock
        {
            get { return (HtmlBlock)GetValue(LineReferenceBlockProperty); }
            set { SetValue(LineReferenceBlockProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineReferenceBlock.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineReferenceBlockProperty =
            DependencyProperty.Register("LineReferenceBlock", typeof(HtmlBlock), typeof(HtmlBlock), new PropertyMetadata(default(HtmlBlock), LineReferenceBlockChanged));

        private static void LineReferenceBlockChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var htmlBlock = d as HtmlBlock;
            if (d == null)
                return;
            var newBlock = e.NewValue as HtmlBlock;
            var oldBlock = e.OldValue as HtmlBlock;
            if (newBlock != null) newBlock.SizeChanged += htmlBlock.LineReferenceBlock_SizeChanged;
            if (oldBlock != null) oldBlock.SizeChanged -= htmlBlock.LineReferenceBlock_SizeChanged;
        }

        private async void LineReferenceBlock_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            await UpdateContentAsync();
        }

        public bool IsLineNumbersEnabled
        {
            get { return (bool)GetValue(IsLineNumbersEnabledProperty); }
            set { SetValue(IsLineNumbersEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsLineNumbersEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsLineNumbersEnabledProperty =
            DependencyProperty.Register("IsLineNumbersEnabled", typeof(bool), typeof(HtmlBlock), new PropertyMetadata(false));


        public int LineHeight
        {
            get { return (int)GetValue(LineHeightProperty); }
            set { SetValue(LineHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for LineHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LineHeightProperty =
            DependencyProperty.Register("LineHeight", typeof(int), typeof(HtmlBlock), new PropertyMetadata(0));




        internal static readonly DependencyProperty DefaultDocumentStyleProperty = DependencyProperty.Register("DefaultDocumentStyle", typeof(DocumentStyle), typeof(HtmlBlock), new PropertyMetadata(new DocumentStyle(), DocumentStylesChanged));

        internal DocumentStyle DefaultDocumentStyle
        {
            get { return (DocumentStyle)GetValue(DefaultDocumentStyleProperty); }
            set { SetValue(DefaultDocumentStyleProperty, value); }
        }

        private static void DocumentStylesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var self = d as HtmlBlock;

            self._docStyles.Reset(self);
            self._docStyles.Merge(self.DefaultDocumentStyle, self.DocumentStyle);
        }


        private bool IsLoaded = false;


        public HtmlBlock()
        {
            this.DefaultStyleKey = typeof(HtmlBlock);

            _docStyles = new DocumentStyle();

        }

        protected override async void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _container = base.GetTemplateChild("_container") as Grid;
            _lineNumCanvas = GetTemplateChild("_lineNumCanvas") as Canvas;
            _container.SizeChanged += _container_SizeChanged;

            await UpdateContentAsync();
        }

        private void _container_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawLineNumbers();
        }

        private async Task UpdateContentAsync()
        {
            await _writeSema.WaitAsync();
            if (_container != null && !string.IsNullOrEmpty(Source))
            {
                _container.RowDefinitions.Clear();
                _container.ColumnDefinitions.Clear();
                _container.Children.Clear();

                documentContainer = new GridDocumentContainer(_container) { LineHeight = LineHeight };
                //if (LineReferenceBlock != null && LineHeight != 0)
                //{
                //    documentContainer.ReferenceTextBlockAnchors = LineReferenceBlock.Anchors;
                //}
                try
                {
                    _doc = await HtmlDocument.LoadAsync(Source);

                    HtmlFragment body = _doc?.Body;
                    if (body == null)
                    {
                        body = _doc;
                    }

                    await WriteFragments(body, documentContainer);
                }
                catch (Exception ex)
                {
                    ShowError(ex, documentContainer);
                }
                if (_lineNumCanvas != null && LineHeight != 0 && IsLineNumbersEnabled)
                {

                    DrawLineNumbers();
                }
                IsLoaded = true;

            }
            _writeSema.Release();
        }

        private void DrawLineNumbers()
        {
            _lineNumCanvas.Children.Clear();
            if (documentContainer != null && documentContainer.TextBlock != null)
            {

                documentContainer.TextBlock.UpdateLayout();
                int count = 1;
                double lastLocation = 0;
                var textPointer = documentContainer.TextBlock.ContentStart;
                for (int i = 0; i < documentContainer.TextBlock.ContentEnd.Offset; i += 10)
                {
                    var position = textPointer.GetPositionAtOffset(i, textPointer.LogicalDirection).GetCharacterRect(textPointer.LogicalDirection).Top;
                    if (position != lastLocation)
                    {
                        var lineNumber = new TextBlock() { Text = count.ToString() };
                        _lineNumCanvas.Children.Add(lineNumber);
                        Canvas.SetTop(lineNumber, position);
                        lastLocation = position;
                        count++;
                    }
                }

            }
        }
        private async Task WriteFragments(HtmlFragment fragment, DocumentContainer parentContainer)
        {
            if (parentContainer != null)
            {
                foreach (var childFragment in fragment.Fragments)
                {
                    var writer = HtmlWriterFactory.Find(childFragment);
                    DependencyObject ctrl = null;
                    ctrl = writer?.GetControl(childFragment);

                    if (ctrl != null)
                    {
                        if (!parentContainer.CanContain(ctrl))
                        {
                            var antecesorContainer = parentContainer.Find(ctrl);

                            if (antecesorContainer == null)
                            {
                                continue;
                            }
                            else
                            {
                                parentContainer = antecesorContainer;
                            }
                        }

                        var currentContainer = parentContainer.Append(ctrl);

                        await WriteFragments(childFragment, currentContainer);
                        if (writer is AnchorWriter && LineHeight != 0)
                            Anchors = Anchors.Union((writer as AnchorWriter)?.ApplyAnchorStyles(_docStyles, ctrl, childFragment)).ToDictionary(pair => pair.Key, pair => pair.Value);
                        else
                        if (writer is TextWriter && LineReferenceBlock != null && LineReferenceBlock.LineHeight != 0 && fragment.Name == "span")
                        {
                            //var referenceLine = LineReferenceBlock;
                            //await Task.Run(() => { while (!ref.IsLoaded) { } });
                            while (!LineReferenceBlock.IsLoaded)
                                await Task.Delay(500);
                            var textWriter = writer as TextWriter;
                            textWriter.ApplyAnchorStyles(_docStyles, ctrl, childFragment, fragment, LineReferenceBlock.Anchors, LineReferenceBlock);

                        }
                        else
                            writer?.ApplyStyles(_docStyles, ctrl, childFragment);

                    }
                }
            }
        }



        private static void ShowError(Exception ex, GridDocumentContainer gridContainer)
        {
            var p = new Paragraph();
            p.FontFamily = new FontFamily("Courier New");

            p.Inlines.Add(new Run
            {
                Text = $"Error rendering document: {ex.Message}"
            });
            gridContainer.Append(p);

            Debug.WriteLine($"HtmlBlock: Error rendering document. Ex: {ex.ToString()}");
        }
    }
}
