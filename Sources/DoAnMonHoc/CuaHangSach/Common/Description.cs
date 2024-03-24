using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuaHangSach.Common
{
    public class Description
    {
        public static string ShortDesc(string description, int length)
        {
            if (string.IsNullOrEmpty(description))
                return "...";
            if(description.Length <= length)
                return $"{description}...";
            return $"{description.Substring(0, length)}...";
        }
    }
}