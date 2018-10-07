using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace WebApplication1.Class
{
    public static class Utilities
    {
        // http://www.codingeverything.com/2014/05/mvcbootstrapactivenavbar.html
        public static string IsActive(this HtmlHelper html, string control, string action)
        {
            var routeData = html.ViewContext.RouteData;

            var routeAction = (string)routeData.Values["action"];
            var routeControl = (string)routeData.Values["controller"];

            // both must match
            var returnActive = control == routeControl && action == routeAction;

            return returnActive ? "active" : "";
        }

        /// <summary> Metoda pro odstranění diakritiky z řetězce </summary>
        /// <param name="s">předávaný řetězec</param>
        /// <returns>řetězec bez diakritiky</returns>
        public static string RemoveDiacritics(string s)
        {
            s = s.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < s.Length; i++)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(s[i]) != UnicodeCategory.NonSpacingMark) sb.Append(s[i]);
            }

            return sb.ToString();
        }


    }
}