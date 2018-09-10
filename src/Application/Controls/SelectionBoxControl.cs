namespace PixelInspector.Controls
{
    using System.Windows;
    using System.Windows.Controls;
    using Tasler.Windows;

    /// <summary>
    /// </summary>
    [TemplatePart(Name = PART_InnerContent, Type = typeof(FrameworkElement))]
    public class SelectionBoxControl : Control
    {
        #region Constants
        public const string PART_InnerContent = nameof(PART_InnerContent);
        #endregion Constants

        #region Instance Fields
        private bool _hasProcessedInitialArrange;
        private Thickness _outerContentThickness;
        private FrameworkElement _partInnerContent;
        #endregion Instance Fields

        #region Constructors
        static SelectionBoxControl()
        {
            FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(
                typeof(SelectionBoxControl), new FrameworkPropertyMetadata(typeof(SelectionBoxControl)));
        }
        #endregion Constructors

        #region Dependency Properties

        #region Rectangle
        /// <summary>
        /// Identifies the <see cref="Rectangle"/> dependency proeprty.
        /// </summary>
        public static readonly DependencyProperty RectangleProperty =
            DependencyProperty.Register(nameof(Rectangle), typeof(Rect?), typeof(SelectionBoxControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsMeasure, RectanglePropertyChanged));

        /// <summary>
        /// Gets or sets the rectangle.
        /// </summary>
        public Rect? Rectangle
        {
            get { return (Rect?)this.GetValue(RectangleProperty); }
            set { this.SetValue(RectangleProperty, value); }
        }

        private static void RectanglePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = (SelectionBoxControl)d;
            var newValue = (Rect?)e.NewValue;
            instance.RectanglePropertyChanged(newValue);
        }

        private void RectanglePropertyChanged(Rect? newValue)
        {
            if (newValue.HasValue && _hasProcessedInitialArrange && this.VisualChildrenCount > 0)
            {
                var firstElement = this.GetVisualChild(0) as FrameworkElement;
                if (firstElement != null)
                {
                    // Adjust the rectangle by the original offsets of PART_InnerContent
                    var rect = newValue.Value.Inflate(_outerContentThickness);

                    // Position and size the first element
                    firstElement.HorizontalAlignment = HorizontalAlignment.Left;
                    firstElement.VerticalAlignment = VerticalAlignment.Top;
                    firstElement.Margin = new Thickness(rect.Left, rect.Top, 0, 0);
                    firstElement.Width = rect.Width;
                    firstElement.Height = rect.Height;
                }
            }
        }
        #endregion Rectangle

        #endregion Dependency Properties

        #region Overrides
        public override void OnApplyTemplate()
        {
            // Perform default processing
            base.OnApplyTemplate();

            // Find the required template part
            _partInnerContent = this.Template.FindName(PART_InnerContent, this) as FrameworkElement;
        }

        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            // Perform default processing
            var size = base.ArrangeOverride(arrangeBounds);

            // Compute the inner content offsets
            if (!_hasProcessedInitialArrange && !size.IsEmpty)
            {
                this.ComputeInnerContentOffsets(size);
                _hasProcessedInitialArrange = true;
            }

            // Return the result from the default processing
            return size;
        }
        #endregion Overrides

        #region Private Implementation
        private void ComputeInnerContentOffsets(Size arrangeBounds)
        {
            var topLeft = new Point(0, 0);
            var bottomRight = topLeft;

            // Compute the offsets of the inner content
            if (_partInnerContent != null)
            {
                topLeft = _partInnerContent.TranslatePoint(topLeft, this);
                bottomRight = _partInnerContent.TranslatePoint(
                    new Point(_partInnerContent.ActualWidth, _partInnerContent.ActualHeight), this);
            }

            // Save the offsets
            _outerContentThickness = new Thickness(
                topLeft.X, topLeft.Y, arrangeBounds.Width - bottomRight.X, arrangeBounds.Height - bottomRight.Y);
        }
        #endregion Private Implementation
    }
}
