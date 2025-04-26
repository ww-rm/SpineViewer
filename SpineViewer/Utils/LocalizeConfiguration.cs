using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Utils
{
	public static class LocalizeConfiguration
	{	

		public static void SetCulture(string cultureName = null)
		{
			CultureInfo culture;

			if (string.IsNullOrWhiteSpace(cultureName))
				culture = CultureInfo.InstalledUICulture;
			else
				culture = new CultureInfo(cultureName);

			Thread.CurrentThread.CurrentCulture = culture;
			Thread.CurrentThread.CurrentUICulture = culture;
		}
	}
}
