using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace ZoomIn.Controls
{
	public static class EventService
	{
		#region Event Bindings
		public static IList GetEventBindings(FrameworkElement d)
		{
			if (d == null)
				throw new ArgumentNullException("d");

			var list = d.GetValue(EventBindingsProperty) as IList;
			if (list == null)
			{
				list = new FreezableCollection<RoutedEventBinding>();
				var notifyCollectionChanged = list as INotifyCollectionChanged;
				notifyCollectionChanged.CollectionChanged += (sender, e) => EventBindings_CollectionChanged(d, e);
				SetEventBindings(d, list);
			}

			return list;
		}

		private static void SetEventBindings(FrameworkElement d, IList value)
		{
			d.SetValue(EventBindingsProperty, value);
		}

		private static readonly DependencyProperty EventBindingsProperty =
			DependencyProperty.RegisterAttached("EventBindingsPrivate", typeof(IList), typeof(EventService));

		private static void EventBindings_CollectionChanged(FrameworkElement element, NotifyCollectionChangedEventArgs e)
		{
			if (e.NewItems != null)
			{
				foreach (var item in e.NewItems.OfType<RoutedEventBinding>())
				{
					if (item.RoutedEvent != null)
					{
						element.AddHandler(item.RoutedEvent, new RoutedEventHandler((s, a) =>
						{
							var command = item.Command;
							if (command != null && command.CanExecute(a))
								command.Execute(a);
						}));
					}
				}
			}

			// TODO: Check e.OldItems

		}
		#endregion Event Bindings
	}

	public class RoutedEventBinding : Freezable
	{
		#region Properties

		#region Event
		public RoutedEvent RoutedEvent { get; set; }
		#endregion Event

		#endregion Properties

		#region Dependency Properties

		#region Command
		public static readonly DependencyProperty CommandProperty =
			DependencyProperty.Register("Command", typeof(ICommand), typeof(RoutedEventBinding));

		public ICommand Command
		{
			get { return (ICommand)this.GetValue(CommandProperty); }
			set { this.SetValue(CommandProperty, value); }
		}
		#endregion Command

		#endregion Dependency Properties

		#region Overrides
		protected override Freezable CreateInstanceCore()
		{
			return new RoutedEventBinding();
		}
		#endregion Overrides
	}
}
