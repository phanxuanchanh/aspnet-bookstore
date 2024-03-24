using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuaHangSach.Models
{
    public class StatusAndResult<T>
    {
        public bool BoolStatus { get; set; }
        public string StringStatus { get; set; }
        public T Result { get; set; }
    }
}