using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CuaHangSach.Controllers
{
    public class OnlineController : Controller
    {
        [ChildActionOnly]
        public long _Online()
        {
            long count = 0;
            if (System.Web.HttpContext.Current != null && System.Web.HttpContext.Current.Application != null)
            {
                System.Web.HttpContext.Current.Application.Lock();
                count = (int)System.Web.HttpContext.Current.Application["Online"];
                System.Web.HttpContext.Current.Application.UnLock();
            }
            return count;
        }
    }
}