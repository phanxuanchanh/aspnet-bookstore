using CuaHangSach.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace CuaHangSach.Common
{
    public class CreateUrl
    {
        public static string TextToUrl(string text)
        {
            Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
            string url = text.Normalize(NormalizationForm.FormD).Trim().ToLower();

            url = regex.Replace(url, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D').Replace(",", "-").Replace(".", "-")
                        .Replace("!", "").Replace("(", "").Replace(")", "").Replace(";", "-").Replace("/", "-")
                        .Replace("%", "ptram").Replace("&", "va").Replace("?", "").Replace('"', '-').Replace(' ', '-');
            return url;
        }
    }
}