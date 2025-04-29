using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace SpineViewer.Utils.Localize
{
	public class LocalizedDisplayNameAttribute : DisplayNameAttribute
	{
		private readonly ResourceManager _resourceManager;
		private readonly string _resourceKey;

		public LocalizedDisplayNameAttribute(Type resourceSource, string resourceKey)
		{
			_resourceManager = new ResourceManager(resourceSource);
			_resourceKey = resourceKey;
		}

		public override string DisplayName => _resourceManager.GetString(_resourceKey) ?? $"[{_resourceKey}]";
	}

	public class LocalizedCategoryAttribute : CategoryAttribute
	{
		private readonly ResourceManager _resourceManager;
		private readonly string _resourceKey;

		public LocalizedCategoryAttribute(Type resourceSource, string resourceKey)
		{
			_resourceManager = new ResourceManager(resourceSource);
			_resourceKey = resourceKey;
		}

		protected override string GetLocalizedString(string value)
		{
			return _resourceManager.GetString(_resourceKey) ?? $"[{_resourceKey}]";
		}
	}

	public class LocalizedDescriptionAttribute : DescriptionAttribute
	{
		private readonly ResourceManager _resourceManager;
		private readonly string _resourceKey;

		public LocalizedDescriptionAttribute(Type resourceSource, string resourceKey)
		{
			_resourceManager = new ResourceManager(resourceSource);
			_resourceKey = resourceKey;
		}

		public override string Description => _resourceManager.GetString(_resourceKey) ?? $"[{_resourceKey}]";
	}
}
