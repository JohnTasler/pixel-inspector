namespace PixelInspector.Configuration
{
    using System;
    using System.Configuration;
    using Tasler;

    public static partial class ApplicationSettingsExtensions
    {
        private static readonly object _autoSaveHelperKey = typeof(AutoSaveHelper);

        public static void SetAutoSaveDeferral(this ApplicationSettingsBase settings, TimeSpan deferralTimeSpan)
        {
            ValidateArgument.IsNotNull(settings, nameof(settings));
            if (deferralTimeSpan < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(deferralTimeSpan));

            var helper = GetAutoSaveHelper(settings);
            if (helper == null)
            {
                helper = new AutoSaveHelper(settings, deferralTimeSpan);
                settings.Context[_autoSaveHelperKey] = helper;
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
                    settings.Context.Remove(_autoSaveHelperKey);
            }
            else
            {
                throw new InvalidOperationException(
                    "ApplicationsSettingsExtensions.ClearAutoSaveDeferral called without first calling SetAutoSaveDeferral.");
            }
        }

        private static AutoSaveHelper GetAutoSaveHelper(ApplicationSettingsBase settings)
        {
            return settings.Context.ContainsKey(_autoSaveHelperKey) ? settings.Context[_autoSaveHelperKey] as AutoSaveHelper : null;
        }
    }
}
