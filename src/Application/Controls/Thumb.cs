using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace ZoomIn.Controls
{
	/// <summary>
	/// </summary>
	public class Thumb : System.Windows.Controls.Primitives.Thumb
	{
		#region Instance Fields
		private bool wasFocusable;
		private IInputElement previouslyFocusedElement;
		#endregion Instance Fields

		#region Constructors
		/// <summary>
		/// Initializes the <see cref="Thumb"/> class.
		/// </summary>
		static Thumb()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(Thumb), new FrameworkPropertyMetadata(typeof(Thumb)));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Thumb"/> class.
		/// </summary>
		public Thumb()
		{
			this.DragStarted += this.this_DragStarted;
			this.DragCompleted += this.this_DragCompleted;
		}
		#endregion Constructors

		#region Overrides

		/// <summary>
		/// Invoked when an unhandled <see cref="Keyboard.KeyDown"/> attached event reaches an element
		/// in its route that is derived from this class.
		/// </summary>
		/// <param name="e">The <see cref="KeyEventArgs"/> that contains the event data.</param>
		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (e.Key == Key.Escape && e.KeyboardDevice.Modifiers == ModifierKeys.None)
			{
				this.CancelDrag();
				e.Handled = true;
			}
		
			base.OnKeyDown(e);
		}

		/// <summary>
		/// Invoked when an unhandled <see cref="PreviewMouseLeftButtonDown"/> routed event reaches an
		/// element in its route that is derived from this class.
		/// </summary>
		/// <param name="e">The <see cref="MouseButtonEventArgs"/> that contains the event data.
		/// The event data reports that the left mouse button was pressed.</param>
		protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
		{
			if (e.ClickCount == 2)
			{
				this.CancelDrag();
				e.Handled = true;
			}
			else
			{
				base.OnPreviewMouseLeftButtonDown(e);
			}
		}

		#endregion Overrides

		#region Event Handlers

		private void this_DragStarted(object sender, DragStartedEventArgs e)
		{
			// Save the element with keyboard focus
			this.previouslyFocusedElement = Keyboard.FocusedElement;

			// Set the keyboard focus to the thumb
			this.wasFocusable = this.Focusable;
			this.Focusable = true;
			this.Focus();
		}

		private void this_DragCompleted(object sender, DragCompletedEventArgs e)
		{
			this.Focusable = this.wasFocusable;

			// Restore keyboard focus to the previous element
			if (this.previouslyFocusedElement != null)
				this.previouslyFocusedElement.Focus();
		}

		#endregion Event Handlers
	}
}
