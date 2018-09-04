namespace ZoomIn.ComponentModel.Mvvm
{

	public abstract class ParentedObservableObject<TParent> : ObservableObject
	{
		private TParent parent;

		protected ParentedObservableObject(TParent parent)
		{
			this.parent = parent;
		}

		public TParent Parent
		{
			get { return this.parent; }
		}
	}
}
