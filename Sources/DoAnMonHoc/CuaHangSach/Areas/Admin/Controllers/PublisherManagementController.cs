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
    [RoutePrefix("publisher-management")]
    [Authorize(Roles = "Super Admin, Admin")]
    public class PublisherManagementController : Controller
    {
        private CuaHangSachDbContext db = new CuaHangSachDbContext();

        [Route("all")]
        public async Task<ActionResult> Index(int? page)
        {
            try
            {
                int pageNumber = (page == null || page < 1) ? 1 : page.Value;
                int pageSize = 10;
                int n = (pageNumber - 1) * pageSize;
                int totalItemCount = await db.Publishers.CountAsync();
                List<Publisher> onePageOfData = await db.Publishers
                    .OrderBy(p => p.ID).Skip(n).Take(pageSize).ToListAsync();
                StaticPagedList<Publisher> publishers = new StaticPagedList<Publisher>(onePageOfData, pageNumber, pageSize, totalItemCount);
                ViewBag.Publishers = publishers;
                ViewBag.SiteName = "Danh sách nhà xuất bản";

                if (Request.IsAjaxRequest())
                {
                    return PartialView("_PublisherListPartial");
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
                Publisher publisher = await db.Publishers.FindAsync(id);
                if (publisher == null)
                {
                    return View("_AdminError", model: "Not Found");
                }
                ViewBag.SiteName = "Chi tiết nhà xuất bản";
                return View(publisher);
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("create")]
        public ActionResult Create()
        {
            ViewBag.SiteName = "Tạo mới nhà xuất bản";
            return View();
        }

        [Route("create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ID,name,description,email,phoneNumber,location")] Publisher publisher)
        {
            try
            {
                int check = await db.Publishers.CountAsync(p => p.ID == publisher.ID);
                if(check > 0)
                {
                    return View("_AdminError", model: "Dữ liệu bị trùng");
                }
                if (ModelState.IsValid)
                {
                    publisher.createTime = DateTime.Now;
                    publisher.updateTime = DateTime.Now;
                    db.Publishers.Add(publisher);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                ViewBag.SiteName = "Tạo mới nhà xuất bản";
                return View(publisher);
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
                Publisher publisher = await db.Publishers.FindAsync(id);
                if (publisher == null)
                {
                    return View("_AdminError", model: "Not Found");
                }
                ViewBag.SiteName = "Chỉnh sửa nhà xuất bản";
                return View(publisher);
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("edit/{id?}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,name,description,email,phoneNumber,location")] Publisher publisher)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Publisher pub = await db.Publishers.FindAsync(publisher.ID);
                    pub.updateTime = DateTime.Now;
                    TryUpdateModel(pub, new string[] { "ID", "name", "description", "email", "phoneNumber", "location", "updateTime" });
                    await db.SaveChangesAsync();
                    return RedirectToAction("Details", new { id = publisher.ID });
                }
                ViewBag.SiteName = "Chỉnh sửa nhà xuất bản";
                return View(publisher);
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
                Publisher publisher = await db.Publishers.FindAsync(id);
                if (publisher == null)
                {
                    return View("_AdminError", model: "Not Found");
                }
                ViewBag.SiteName = "Xóa nhà xuất bản";
                return View(publisher);
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
                Publisher publisher = await db.Publishers.FindAsync(id);
                db.Publishers.Remove(publisher);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                long count = await db.Books.CountAsync(b => b.publisherId == id);
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
