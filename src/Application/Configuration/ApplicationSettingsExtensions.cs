namespace ZoomIn.Configuration
{
	using System;
	using System.Configuration;

	public static partial class ApplicationSettingsExtensions
	{
		private static readonly object autoSaveHelperKey = typeof(AutoSaveHelper);

		public static void SetAutoSaveDeferral(this ApplicationSettingsBase settings, TimeSpan deferralTimeSpan)
		{
			if (settings == null)
				throw new ArgumentNullException("settings");
			if (deferralTimeSpan < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException("deferralTimeSpan");

			var helper = GetAutoSaveHelper(settings);
			if (helper == null)
			{
				helper = new AutoSaveHelper(settings, deferralTimeSpan);
				settings.Context[autoSaveHelperKey] = helper;
			}
			else
			{
				helper.Expire(deferralTimeSpan);
			}
		}

		public static void ExpireAutoSaveDeferral(this ApplicationSettingsBase settings)
		{
			var helper = GetAutoSaveHelper(settings);
			if (helper != null)
				helper.Expire();
			else
				throw new InvalidOperationException(
					"ApplicationsSettingsExtensions.ExpireAutoSaveDeferral called without first calling SetAutoSaveDeferral.");
		}

		public static void ClearAutoSaveDeferral(this ApplicationSettingsBase settings)
		{
			var helper = GetAutoSaveHelper(settings);
			if (helper != null)
			{
				using (helper)
					settings.Context.Remove(autoSaveHelperKey);
			}
			else
			{
				throw new InvalidOperationException(
					"ApplicationsSettingsExtensions.ClearAutoSaveDeferral called without first calling SetAutoSaveDeferral.");
			}
		}

		private static AutoSaveHelper GetAutoSaveHelper(ApplicationSettingsBase settings)
		{
			return settings.Context.ContainsKey(autoSaveHelperKey) ? settings.Context[autoSaveHelperKey] as AutoSaveHelper : null;
		}
	}
}
