namespace ZoomIn
{
	using System;
	using System.Reflection;
	using System.Windows;

	public class VisualStyleDictionaryExtension : ThemeDictionaryExtension
	{
		private const string componentToken = ";component/";

		/// <summary>
		/// Initializes a new instance of the <see cref="VisualStyleDictionaryExtension"/> class.
		/// </summary>
		public VisualStyleDictionaryExtension()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="VisualStyleDictionaryExtension"/> class.
		/// </summary>
		/// <param name="assemblyName">Name of the assembly.</param>
		/// <param name="subFolder">The sub folder to insert before the <c>Themes</c> subfolder.</param>
		public VisualStyleDictionaryExtension(string assemblyName, string subFolder)
			: base(assemblyName)
		{
			this.SubFolder = subFolder;
		}

		public string SubFolder { get; set; }

		/// <summary>
		/// Returns an object that should be set on the property where this extension is applied.
		/// For <see cref="T:System.Windows.ThemeDictionaryExtension"/>, this is the URI value for a particular theme dictionary extension.
		/// </summary>
		/// <param name="serviceProvider">
		/// An object that can provide services for the markup extension.
		/// This service is expected to provide results for <see cref="T:System.Windows.Markup.IXamlTypeResolver"/>.
		/// </param>
		/// <returns>
		/// The object value to set on the property where the extension is applied.
		/// </returns>
		/// <exception cref="T:System.InvalidOperationException">The <see cref="P:System.Windows.ThemeDictionaryExtension.AssemblyName"/> property is null. You must set this value during construction or before using the <see cref="M:System.Windows.ThemeDictionaryExtension.ProvideValue(System.IServiceProvider)"/>  method.-or-<paramref name="serviceProvide"/>r is null or does not provide a service for <see cref="T:System.Windows.Markup.IXamlTypeResolver"/>.-or-<paramref name="serviceProvider"/> specifies a target type that does not match <see cref="P:System.Windows.ResourceDictionary.Source"/>.</exception>
		public override object ProvideValue(IServiceProvider serviceProvider)
		{
			// Use the name of the entry assembly, if not specified
			if (string.IsNullOrWhiteSpace(base.AssemblyName))
				base.AssemblyName = Assembly.GetEntryAssembly().GetName().Name;

			// Get the Uri formatted by the base class
			var value = (Uri)base.ProvideValue(serviceProvider);

			// Insert the specified sub-folder, if any
			if (!string.IsNullOrEmpty(this.SubFolder))
			{
				var subFolder = this.SubFolder.Trim('/', '\\');
				if (subFolder.Length > 0)
				{
					var newUriString = value.OriginalString.Replace(componentToken, componentToken + subFolder + "/");
					value = new Uri(newUriString, UriKind.RelativeOrAbsolute);
				}
			}

			return value;
		}
	}
}
