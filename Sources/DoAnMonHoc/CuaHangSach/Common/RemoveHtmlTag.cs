using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace CuaHangSach.Common
{
    public class RemoveHtmlTag
    {
        public static string Remove(string strHtml)
        {
            return Regex.Replace(strHtml, "<[^>]*>", string.Empty);
        }
    }
}