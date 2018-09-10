namespace PixelInspector.ViewModel
{
    using System;
    using System.Windows;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using PixelInspector.Interop.User;
    using Tasler.ComponentModel;
    using Tasler.Windows;

    public class SelectToolViewModel
        : ChildViewModel<MainViewModel>
        , IToolMode
    {
        #region Instance Fields
        private Point _dragStart;
        private Rect _dragRect = Rect.Empty;
        private double _lastHorizontalChange;
        private double _lastVerticalChange;
        #endregion Instance Fields

        #region Constructors
        public SelectToolViewModel(MainViewModel parent)
            : base(parent)
        {
        }
        #endregion Constructors

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
            // Get the point where the mouse was clicked
            _dragStart = this.Parent.ZoomedMousePosition;

            // Get the drag rect
            _dragRect = new Rect(_dragStart.X, _dragStart.Y, 0, 0);
            _dragRect.Inflate(
                Math.Abs(UserApi.GetSystemMetrics(SM.CxDrag)),
                Math.Abs(UserApi.GetSystemMetrics(SM.CyDrag)));

            _lastHorizontalChange = _lastVerticalChange = 0;
            this.Parent.Selection.UpdateZoomedRectangleActualFromInput(null);
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
            // Calculate the change
            var horizontalChange = e.HorizontalChange - _lastHorizontalChange;
            var verticalChange = e.VerticalChange - _lastVerticalChange;
            _lastHorizontalChange = e.HorizontalChange;
            _lastVerticalChange = e.VerticalChange;

            // Check to see if the mosue has moved outside of the drag rect
            if (!_dragRect.IsEmpty && _dragRect.Contains(this.Parent.ZoomedMousePosition))
                _dragRect = Rect.Empty;

            // Only process if the mouse has moved outside of the drag rect
            if (_dragRect.IsEmpty)
            {
                var rect = new ExtentRect(_dragStart, this.Parent.ZoomedMousePosition);
                this.Parent.Selection.UpdateZoomedRectangleActualFromInput(rect);
            }

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
        }

        /// <summary>
        /// Called to exit the mode.
        /// </summary>
        /// <param name="isReverting">If set to <c>true</c> the tool should revert any changes it made;
        /// otherwise, it should commit its changes.</param>
        public void ExitMode(bool isReverting)
        {
        }

        #endregion IToolMode Members
    }
}
