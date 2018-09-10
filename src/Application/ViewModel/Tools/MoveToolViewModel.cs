namespace PixelInspector.ViewModel
{
    using System.Windows;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using Tasler.ComponentModel;

    public class MoveToolViewModel
        : ChildViewModel<MainViewModel>
        , IToolMode
        , IProvideSourceOrigin
    {
        #region Instance Fields
        private Point _previousSourceOrigin;
        private double _lastHorizontalChange;
        private double _lastVerticalChange;
        #endregion Instance Fields

        #region Constructors
        public MoveToolViewModel(MainViewModel parent)
            : base(parent)
        {
        }
        #endregion Constructors

        #region Properties

        public Point SourceOrigin
        {
            get { return _sourceOrigin; }
            private set { this.PropertyChanged.SetProperty(this, value, ref _sourceOrigin); }
        }
        private Point _sourceOrigin;

        public Point SourceOriginActual
        {
            get { return _sourceOriginActual; }
            set
            {
                if (this.PropertyChanged.SetProperty(this, value, ref _sourceOriginActual))
                    this.SourceOrigin = new Point((int)value.X, (int)value.Y);
            }
        }
        private Point _sourceOriginActual;

        #endregion Properties

        #region Commands

        #region DragStartedCommand
        public ICommand DragStartedCommand
        {
            get
            {
                return _dragStartedCommand ??
                    (_dragStartedCommand = new RelayCommand<DragStartedEventArgs>(this.DragStarted));
            }
        }
        private RelayCommand<DragStartedEventArgs> _dragStartedCommand;

        private void DragStarted(DragStartedEventArgs e)
        {
            _lastHorizontalChange = _lastVerticalChange = 0;
            e.Handled = true;
        }
        #endregion DragStartedCommand

        #region DragDeltaCommand
        public ICommand DragDeltaCommand
        {
            get
            {
                return _dragDeltaCommand ??
                    (_dragDeltaCommand = new RelayCommand<DragDeltaEventArgs>(this.DragDelta));
            }
        }
        private RelayCommand<DragDeltaEventArgs> _dragDeltaCommand;

        private void DragDelta(DragDeltaEventArgs e)
        {
            var horizontalChange = e.HorizontalChange - _lastHorizontalChange;
            var verticalChange = e.VerticalChange - _lastVerticalChange;
            _lastHorizontalChange = e.HorizontalChange;
            _lastVerticalChange = e.VerticalChange;

            var sourceOrigin = this.SourceOriginActual;
            var zoomFactor = this.Parent.ViewSettings.Model.ZoomFactor;
            var xOffset = -horizontalChange / zoomFactor;
            var yOffset = -verticalChange / zoomFactor;

            sourceOrigin.Offset(xOffset, yOffset);
            this.SourceOriginActual = sourceOrigin;

            e.Handled = true;
        }
        #endregion DragDeltaCommand

        #region DragCompletedCommand
        public ICommand DragCompletedCommand
        {
            get
            {
                return _dragCompletedCommand ??
                    (_dragCompletedCommand = new RelayCommand<DragCompletedEventArgs>(this.DragCompleted));
            }
        }
        private RelayCommand<DragCompletedEventArgs> _dragCompletedCommand;

        private void DragCompleted(DragCompletedEventArgs e)
        {
            this.ExitMode(e.Canceled);
            e.Handled = true;
        }
        #endregion DragCompletedCommand

        #endregion Commands

        #region IToolMode Members

        /// <summary>
        /// Called before the mode is entered.
        /// </summary>
        public void EnterMode()
        {
            _previousSourceOrigin = this.SourceOriginActual = this.Parent.ViewSettings.Model.SourceOrigin;
        }

        /// <summary>
        /// Called to exit the mode.
        /// </summary>
        /// <param name="isReverting">If set to <c>true</c> the tool should revert any changes it made;
        /// otherwise, it should commit its changes.</param>
        public void ExitMode(bool isReverting)
        {
            if (isReverting)
            {
                this.SourceOriginActual = _previousSourceOrigin;

                // TODO: Restore previous bitmap images
            }
            else
            {
                this.Parent.ViewSettings.Model.SourceOrigin = this.SourceOrigin;
                this.EnterMode();
            }
        }

        #endregion IToolMode Members
    }
}
