﻿namespace PixelInspector.ViewModel
{
    using Tasler;
    using Tasler.ComponentModel;

    public class LocateToolViewModel
        : ChildViewModel<MainViewModel>
        , IToolMode
    {
        #region Instance Fields
        private object _previousToolState;
        private bool _isExiting;
        #endregion Instance Fields

        #region Constructors
        public LocateToolViewModel(MainViewModel parent)
            : base(parent)
        {
        }
        #endregion Constructors

        #region IToolMode Members

        /// <summary>
        /// Called before the mode is entered.
        /// </summary>
        public void EnterMode()
        {
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

            using (new DisposeScopeExit(() => _isExiting = true, () => _isExiting = false))
                this.Parent.ToolState = _previousToolState;
        }

        #endregion IToolMode Members
    }
}
