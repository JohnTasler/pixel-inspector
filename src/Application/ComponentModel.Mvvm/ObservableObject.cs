namespace ZoomIn.ComponentModel.Mvvm
{
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Linq.Expressions;

	/// <summary>
	/// This is the abstract base class for any object that provides property change notifications.  
	/// </summary>
	public abstract class ObservableObject : INotifyPropertyChanged
	{
		#region Constructor

		/// <summary>
		/// Initializes a new instance of the <see cref="ObservableObject"/> class.
		/// </summary>
		protected ObservableObject()
		{
		}

		#endregion Constructor

		#region RaisePropertyChanged

		/// <summary>
		/// Raises this object's PropertyChanged event for the specified <paramref name="propertyName"/>.
		/// </summary>
		/// <param name="PropertyName">The name of the property that has changed.</param>
		protected void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
		{
			var handler = this.PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(PropertySupport.ExtractPropertyName(propertyExpression)));
		}

		/// <summary>
		/// Raises this object's PropertyChanged event for the specified <paramref name="propertyName"/>.
		/// </summary>
		/// <param name="PropertyName">The name of the property that has changed.</param>
		protected void RaisePropertyChanged(string propertyName)
		{
			var handler = this.PropertyChanged;
			if (handler != null)
				handler(this, new PropertyChangedEventArgs(propertyName));
		}

		/// <summary>
		/// Raises this object's PropertyChanged event for the specified <paramref name="propertyNames"/>.
		/// </summary>
		/// <param name="PropertyName">The names of the properties that have changed.</param>
		protected void RaisePropertyChanged(params string[] propertyNames)
		{
			if (propertyNames == null)
				throw new ArgumentNullException("propertyNames");

			var handler = this.PropertyChanged;
			if (handler != null)
			{
				foreach (var propertyName in propertyNames)
					handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		#endregion RaisePropertyChanged

		#region Property Setter Methods

		/// <summary>
		/// Compares a field value to the specified value and, if not equal, sets the field to the new value and invokes
		/// the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
		/// </summary>
		/// <typeparam name="T">The type of the field being compared. This is inferred from the <paramref name="field"/>
		/// and <paramref name="newValue"/> parameters, and does not normally need to be specified explicitly.</typeparam>
		/// <param name="field">A reference to the member field that backs the property being set.</param>
		/// <param name="newValue">The new value the property is being set to.</param>
		/// <param name="PropertyName">The name of the property being set.</param>
		/// <returns><b>true</b> if the <paramref name="field"/> value was changed to <paramref name="newValue"/>;
		/// otherwise <b>false</b>.</returns>
		protected bool SetProperty<T>(ref T field, T newValue, string propertyName)
		{
			return this.SetProperty(ref field, newValue, EqualityComparer<T>.Default, propertyName);
		}

		/// <summary>
		/// Compares a field value to the specified value and, if not equal, sets the field to the new value and invokes
		/// the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
		/// </summary>
		/// <typeparam name="T">The type of the field being compared. This is inferred from the <paramref name="field"/>
		/// and <paramref name="newValue"/> parameters, and does not normally need to be specified explicitly.</typeparam>
		/// <param name="field">A reference to the member field that backs the property being set.</param>
		/// <param name="newValue">The new value the property is being set to.</param>
		/// <param name="equalityComparer">An object that implements the <see cref="IEqualityComparer{T}"/> interface, used
		/// to test for the equality of the <paramref name="field"/> and <paramref name="newValue"/> parameters. If not
		/// specified, then <see cref="EqualityComparer{T}"/> is used.</param>
		/// <param name="PropertyName">The name of the property being set.</param>
		/// <returns><b>true</b> if the <paramref name="field"/> value was changed to <paramref name="newValue"/>;
		/// otherwise <b>false</b>.</returns>
		protected bool SetProperty<T>(ref T field, T newValue, IEqualityComparer<T> equalityComparer, string propertyName)
		{
			bool hasChanged = !equalityComparer.Equals(field, newValue);
			if (hasChanged)
			{
				field = newValue;
				this.RaisePropertyChanged(propertyName);
			}
			return hasChanged;
		}

		/// <summary>
		/// Compares a field value to the specified value and, if not equal, sets the field to the new value and invokes
		/// the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
		/// </summary>
		/// <typeparam name="T">The type of the field being compared. This is inferred from the <paramref name="field"/>
		/// and <paramref name="newValue"/> parameters, and does not normally need to be specified explicitly.</typeparam>
		/// <param name="field">A reference to the member field that backs the property being set.</param>
		/// <param name="newValue">The new value the property is being set to.</param>
		/// <param name="propertyNames">The names of zero or more properties affected by the changed property value.</param>
		/// <returns><b>true</b> if the <paramref name="field"/> value was changed to <paramref name="newValue"/>;
		/// otherwise <b>false</b>.</returns>
		protected bool SetProperty<T>(ref T field, T newValue, params string[] propertyNames)
		{
			return this.SetProperty(ref field, newValue, EqualityComparer<T>.Default, propertyNames);
		}

		/// <summary>
		/// Compares a field value to the specified value and, if not equal, sets the field to the new value and invokes
		/// the <see cref="INotifyPropertyChanged.PropertyChanged"/> event.
		/// </summary>
		/// <typeparam name="T">The type of the field being compared. This is inferred from the <paramref name="field"/>
		/// and <paramref name="newValue"/> parameters, and does not normally need to be specified explicitly.</typeparam>
		/// <param name="field">A reference to the member field that backs the property being set.</param>
		/// <param name="newValue">The new value the property is being set to.</param>
		/// <param name="equalityComparer">An object that implements the <see cref="IEqualityComparer{T}"/> interface, used
		/// to test for the equality of the <paramref name="field"/> and <paramref name="newValue"/> parameters. If not
		/// specified, then <see cref="EqualityComparer{T}"/> is used.</param>
		/// <param name="propertyNames">The names of zero or more properties affected by the changed property value.</param>
		/// <returns><b>true</b> if the <paramref name="field"/> value was changed to <paramref name="newValue"/>;
		/// otherwise <b>false</b>.</returns>
		protected bool SetProperty<T>(ref T field, T newValue, IEqualityComparer<T> equalityComparer, params string[] propertyNames)
		{
			bool hasChanged = !equalityComparer.Equals(field, newValue);
			if (hasChanged)
			{
				field = newValue;
				this.RaisePropertyChanged(propertyNames);
			}
			return hasChanged;
		}

		#endregion Property Setter Methods

		#region INotifyPropertyChanged Members

		/// <summary>
		/// Raised when a property on this object has a new value.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion INotifyPropertyChanged Members
	}
}

