using NLog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SpineViewer.Utils.Localize
{
	public static class LocalizeConfiguration
	{
		public static void UpdateLocalizeSetting(string newCulture)
		{
			Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

			if (config.AppSettings.Settings["localize"] != null)
				config.AppSettings.Settings["localize"].Value = newCulture;
			else
				config.AppSettings.Settings.Add("localize", newCulture);

			config.Save(ConfigurationSaveMode.Modified);
			ConfigurationManager.RefreshSection("appSettings");
		}

		public static void SetCulture()
		{
			string cultureName = ConfigurationManager.AppSettings["localize"];

			if (string.IsNullOrWhiteSpace(cultureName))
			{
				cultureName = "zh-CN";
			}
			try
			{
			
				var culture = new CultureInfo(cultureName);
				Thread.CurrentThread.CurrentCulture = culture;
				Thread.CurrentThread.CurrentUICulture = culture;
			}
			catch (CultureNotFoundException)
			{
				Thread.CurrentThread.CurrentUICulture = new CultureInfo("zh-CN");
				Thread.CurrentThread.CurrentCulture = new CultureInfo("zh-CN");
			}


		}
	}
}
