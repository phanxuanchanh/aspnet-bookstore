using CuaHangSach.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace CuaHangSach.Areas.Admin.Controllers
{
    [RouteArea("Admin", AreaPrefix = "administration")]
    [RoutePrefix("general")]
    [Authorize]
    public class AdminController : Controller
    {
        CuaHangSachDbContext db = new CuaHangSachDbContext();

        [Route("access")]
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View();
        }

        [Route("analytics")]
        [Authorize(Roles = "Super Admin")]
        public async Task<ActionResult> General()
        {
            try
            {
                ViewBag.BookNumber = await db.Books.CountAsync();
                ViewBag.AuthorNumber = await db.Authors.CountAsync();
                ViewBag.CategoryNumber = await db.Categories.CountAsync();
                ViewBag.PublisherNumber = await db.Publishers.CountAsync();
                ViewBag.SiteName = "Trang tổng quan";
                return View();
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}