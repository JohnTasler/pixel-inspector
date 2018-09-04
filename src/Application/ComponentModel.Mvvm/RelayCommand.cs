namespace ZoomIn.ComponentModel.Mvvm
{
	using System;
	using System.Diagnostics;
	using System.Windows.Input;

	/// <summary>
	/// A command whose sole purpose is to 
	/// relay its functionality to other
	/// objects by invoking delegates. The
	/// default return value for the CanExecute
	/// method is 'true'.
	/// </summary>
	public class RelayCommand<T> : ICommand
	{
		#region Instance Fields
		private readonly Action<T> execute;
		private readonly Func<T, bool> canExecute;
		#endregion Instance Fields

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="RelayCommand{T}"/> class.
		/// </summary>
		/// <param name="execute">The execution logic.</param>
		public RelayCommand(Action<T> execute)
			: this(execute, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayCommand{T}"/> class.
		/// </summary>
		/// <param name="execute">The execution logic.</param>
		/// <param name="canExecute">The execution status logic.</param>
		public RelayCommand(Action<T> execute, Func<T, bool> canExecute)
		{
			if (execute == null)
				throw new ArgumentNullException("execute");

			this.execute = execute;
			this.canExecute = canExecute;
		}

		#endregion Constructors

		#region ICommand Members

		[DebuggerStepThrough]
		public bool CanExecute(object parameter)
		{
			return this.canExecute == null ? true : this.canExecute((T)parameter);
		}

		public event EventHandler CanExecuteChanged
		{
			add
			{
				if (this.canExecute != null)
					CommandManager.RequerySuggested += value;
			}
			remove
			{
				if (this.canExecute != null)
					CommandManager.RequerySuggested -= value;
			}
		}

		public void Execute(object parameter)
		{
			if (this.CanExecute(parameter))
				this.execute((T)parameter);
		}

		#endregion ICommand Members
	}

	/// <summary>
	/// A command whose sole purpose is to 
	/// relay its functionality to other
	/// objects by invoking delegates. The
	/// default return value for the CanExecute
	/// method is 'true'.
	/// </summary>
	public class RelayCommand : ICommand
	{
		#region Instance Fields
		private readonly Action execute;
		private readonly Func<bool> canExecute;
		#endregion Instance Fields

		#region Constructors

		/// <summary>
		/// Creates a new command that can always execute.
		/// </summary>
		/// <param name="execute">The execution logic.</param>
		public RelayCommand(Action execute)
			: this(execute, null)
		{
		}

		/// <summary>
		/// Creates a new command.
		/// </summary>
		/// <param name="execute">The execution logic.</param>
		/// <param name="canExecute">The execution status logic.</param>
		public RelayCommand(Action execute, Func<bool> canExecute)
		{
			if (execute == null)
				throw new ArgumentNullException("execute");

			this.execute = execute;
			this.canExecute = canExecute;
		}

		#endregion Constructors

		#region ICommand Members

		[DebuggerStepThrough]
		public bool CanExecute(object parameter)
		{
			return this.canExecute == null ? true : this.canExecute();
		}

		public event EventHandler CanExecuteChanged
		{
			add
			{
				if (this.canExecute != null)
					CommandManager.RequerySuggested += value;
			}
			remove
			{
				if (this.canExecute != null)
					CommandManager.RequerySuggested -= value;
			}
		}

		public void Execute(object parameter)
		{
			if (this.CanExecute(parameter))
				this.execute();
		}

		#endregion ICommand Members
	}
}
