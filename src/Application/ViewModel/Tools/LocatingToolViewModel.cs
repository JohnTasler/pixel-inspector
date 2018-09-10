namespace PixelInspector.ViewModel
{
    using System.Windows;
    using Tasler;
    using Tasler.ComponentModel;

    public class LocatingToolViewModel
        : ChildViewModel<MainViewModel>
        , IProvideSourceOrigin
        , IToolMode
    {
        #region Instance Fields
        private object _previousToolState;
        private bool _isExiting;
        #endregion Instance Fields

        #region Constructors
        public LocatingToolViewModel(MainViewModel parent, LocatingToolViewModel.Parameters parameters)
            : base(parent)
        {
            if (parameters != null)
            {
                this.Offset = parameters.Offset;
                this.IsFromMouseClick = parameters.IsFromMouseClick;
            }
        }
        #endregion Constructors

        #region Properties

        public Point Offset { get; private set; }

        public bool IsFromMouseClick { get; private set; }

        public Point SourceOrigin
        {
            get { return _sourceOrigin; }
            set { this.PropertyChanged.SetProperty(this, value, ref _sourceOrigin); }
        }
        private Point _sourceOrigin;

        #endregion Properties

        #region IToolMode Members

        /// <summary>
        /// Called before the mode is entered.
        /// </summary>
        public void EnterMode()
        {
            _sourceOrigin = this.Parent.ViewSettings.Model.SourceOrigin;
            _previousToolState = this.Parent.ToolState;
        }

        /// <summary>
        /// Called to exit the mode.
        /// </summary>
        /// <param name="isReverting">If set to <c>true</c> the tool should revert any changes it made;
        /// otherwise, it should commit its changes.</param>
        public void ExitMode(bool isReverting)
        {
            if (_isExiting)
                return;

            if (isReverting)
            {
                // TODO: Restore previous bitmap images
            }
            else
            {
                this.Parent.ViewSettings.Model.SourceOrigin = this.SourceOrigin;
            }

            using (new DisposeScopeExit(() => _isExiting = true, () => _isExiting = false))
                this.Parent.ToolState = _previousToolState;
        }

        #endregion IToolMode Members

        #region Nested Types
        public class Parameters
        {
            public Point Offset { get; set; }
            public bool IsFromMouseClick { get; set; }
        }
        #endregion Nested Types
    }
}
