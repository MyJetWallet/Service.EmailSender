using System;
using System.Globalization;
using System.Linq;

namespace Service.EmailSender.Helpers
{
	public static class CultureInfoHelper
	{
		public static string GetCountryName(string code)
		{
			if (string.IsNullOrWhiteSpace(code))
				return null;

			CultureInfo culture = CultureInfo
				.GetCultures(CultureTypes.AllCultures)
				.FirstOrDefault(info =>
					info.TwoLetterISOLanguageName.Equals(code, StringComparison.InvariantCultureIgnoreCase)
						|| info.Name.Equals(code, StringComparison.InvariantCultureIgnoreCase)
				);

			return culture == null
				? code
				: new RegionInfo(culture.Name).EnglishName;
		}
	}
}