using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CuaHangSach.Models;
using X.PagedList;

namespace CuaHangSach.Areas.Admin.Controllers
{
    [RouteArea("Admin", AreaPrefix = "administration")]
    [RoutePrefix("category-management")]
    [Authorize(Roles = "Super Admin, Admin")]
    public class CategoryManagementController : Controller
    {
        private CuaHangSachDbContext db = new CuaHangSachDbContext();

        [Route("all/{page?}")]
        public async Task<ActionResult> Index(int? page)
        {
            try
            {
                int pageNumber = (page == null || page < 1) ? 1 : page.Value;
                int pageSize = 10;
                int n = (pageNumber - 1) * pageSize;
                int totalItemCount = await db.Categories.CountAsync();
                List<Category> onePageOfData = await db.Categories.OrderBy(p => p.ID).Skip(n).Take(pageSize).ToListAsync();
                StaticPagedList<Category> categories = new StaticPagedList<Category>(onePageOfData, pageNumber, pageSize, totalItemCount);
                ViewBag.Categories = categories;
                ViewBag.SiteName = "Danh sách thể loại";

                if (Request.IsAjaxRequest())
                {
                    return PartialView("_CategoryListPartial");
                }
                return View();
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("details/{id?}")]
        public async Task<ActionResult> Details(long? id)
        {
            if (id == null || id < 1)
                return RedirectToAction("Index");
            try
            {
                Category category = await db.Categories.FindAsync(id);
                if (category == null)
                {
                    return View("_AdminError", model: "Not Found");
                }
                ViewBag.SiteName = "Chi tiết thể loại";
                return View(category);
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("create")]
        public ActionResult Create()
        {
            ViewBag.SiteName = "Tạo mới thể loại";
            return View();
        }

        [Route("create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ID,name")] Category category)
        {
            try
            {
                int check = await db.Categories.CountAsync(c => c.name == category.name);
                if (check > 0)
                {
                    return View("_AdminError", model: "Dữ liệu bị trùng");
                }
                if (ModelState.IsValid)
                {
                    category.createTime = DateTime.Now;
                    category.updateTime = DateTime.Now;
                    db.Categories.Add(category);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                ViewBag.SiteName = "Tạo mới thể loại";
                return View(category);
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }

        }

        [Route("edit/{id?}")]
        public async Task<ActionResult> Edit(long? id)
        {
            if (id == null || id < 1)
                return RedirectToAction("Index");
            try
            {
                Category category = await db.Categories.FindAsync(id);
                if (category == null)
                {
                    return View("_AdminError", model: "Not Found");
                }
                ViewBag.SiteName = "Chỉnh sửa thể loại";
                return View(category);
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("edit/{id?}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,name")] Category category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Category ctgr = await db.Categories.FindAsync(category.ID);
                    ctgr.updateTime = DateTime.Now;
                    TryUpdateModel(ctgr, new string[] { "name", "updateTime" });
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                ViewBag.SiteName = "Chỉnh sửa thể loại";
                return View(category);
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("delete/{id?}")]
        public async Task<ActionResult> Delete(long? id)
        {
            if (id == null || id < 1)
                return RedirectToAction("Index");
            try
            {
                Category category = await db.Categories.FindAsync(id);
                if (category == null)
                {
                    return View("_AdminError", model: "Not Found");
                }
                ViewBag.SiteName = "Xóa thể loại";
                return View(category);
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("delete/{id?}")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(long id)
        {
            try
            {
                Category category = await db.Categories.FindAsync(id);
                db.Categories.Remove(category);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                long count = await db.Books.CountAsync(b => b.categoryId == id);
                if (count > 0)
                {
                    return View("_AdminError", model: $"Không thể xóa được dữ liệu, do có {count} nội dung sử dụng tới thể loại này!");
                }
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
