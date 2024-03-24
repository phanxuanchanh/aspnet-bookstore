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
using System.ComponentModel;

namespace CuaHangSach.Areas.Admin.Controllers
{
    [RouteArea("Admin", AreaPrefix = "administration")]
    [RoutePrefix("author-management")]
    //[Authorize]
    public class AuthorManagementController : Controller
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
                int totalItemCount = await db.Authors.CountAsync();
                List<Author> onePageOfData = await db.Authors.OrderBy(p => p.ID).Skip(n).Take(pageSize).ToListAsync();
                StaticPagedList<Author> authors = new StaticPagedList<Author>(onePageOfData, pageNumber, pageSize, totalItemCount);
                ViewBag.Authors = authors;
                ViewBag.SiteName = "Danh sách tác giả";

                if (Request.IsAjaxRequest())
                {
                    return PartialView("_AuthorListPartial");
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
                Author author = await db.Authors.FindAsync(id);
                if (author == null)
                {
                    return View("_AdminError", model: "Not Found");
                }
                ViewBag.SiteName = "Chi tiết tác giả";
                return View(author);
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("create")]
        public ActionResult Create()
        {
            ViewBag.SiteName = "Tạo mới tác giả";
            return View();
        }

        [Route("create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ID,name,email,phoneNumber,description,location")] Author author)
        {
            try
            {
                int check = await db.Authors.CountAsync(a => a.name == author.name && a.email == author.email && a.phoneNumber == author.phoneNumber);
                if (check > 0)
                {
                    return View("_AdminError", model: "Dữ liệu bị trùng");
                }
                if (ModelState.IsValid)
                {
                    author.createTime = DateTime.Now;
                    author.updateTime = DateTime.Now;
                    db.Authors.Add(author);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                ViewBag.SiteName = "Tạo mới tác giả";
                return View(author);
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
                Author author = await db.Authors.FindAsync(id);
                if (author == null)
                {
                    return View("_AdminError", model: "Not Found");
                }
                ViewBag.SiteName = "Chi tiết tác giả";
                return View(author);
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("edit/{id?}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,name,email,phoneNumber,description,location")] Author author)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Author auth = await db.Authors.FindAsync(author.ID);
                    auth.updateTime = DateTime.Now;
                    TryUpdateModel(auth, new string[] { "name", "email", "phoneNumber", "description", "location", "updateTime"});
                    await db.SaveChangesAsync();
                    return RedirectToAction("Details", new { id = author.ID });
                }
                ViewBag.SiteName = "Chỉnh sửa tác giả";
                return View(author);
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
                Author author = await db.Authors.FindAsync(id);
                if (author == null)
                {
                    return View("_AdminError", model: "Not Found");
                }
                ViewBag.SiteName = "Xóa tác giả";
                return View(author);
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
                Author author = await db.Authors.FindAsync(id);
                db.Authors.Remove(author);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                long count = await db.Contributes.CountAsync(c => c.authorId == id);
                if (count > 0)
                {
                    return View("_AdminError", model: $"Không thể xóa được dữ liệu, do có {count} nội dung liên quan tới tác giả này!");
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
