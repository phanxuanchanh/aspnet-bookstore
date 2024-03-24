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

namespace CuaHangSach.Areas.Admin.Controllers
{
    [RouteArea("Admin", AreaPrefix = "administration")]
    [RoutePrefix("status-management")]
    [Authorize(Roles = "Super Admin, Admin")]
    public class StatusManagementController : Controller
    {
        private CuaHangSachDbContext db = new CuaHangSachDbContext();

        [Route("all")]
        public async Task<ActionResult> Index()
        {
            try
            {
                List<Status> status = await db.Status.ToListAsync();
                ViewBag.SiteName = "Danh sách trạng thái";
                return View(status);
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("details/{id?}")]
        public async Task<ActionResult> Details(int? id)
        {
            if (id < 0 || id == null)
            {
                return RedirectToAction("Index");
            }
            try
            {
                Status status = await db.Status.FindAsync(id);
                if (status == null)
                {
                    return View("_AdminError", model: "Not Found");
                }
                ViewBag.SiteName = "Chi tiết về trạng thái";
                return View(status);
            }
            catch(Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("create")]
        public ActionResult Create()
        {
            ViewBag.SiteName = "Chỉnh sửa trạng thái";
            return View();
        }

        [Route("create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ID,name,description")] Status status)
        {
            try
            {
                int check = await db.Status.CountAsync(s => s.name == status.name);
                if (check > 0)
                {
                    return View("_AdminError", model: "Dữ liệu bị trùng");
                }
                if (ModelState.IsValid)
                {
                    db.Status.Add(status);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                ViewBag.SiteName = "Chỉnh sửa trạng thái";
                return View(status);
            }
            catch(Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("edit/{id?}")]
        public async Task<ActionResult> Edit(int? id)
        {
            if (id < 0 || id == null)
            {
                return RedirectToAction("Index");
            }
            try
            {
                Status status = await db.Status.FindAsync(id);
                if (status == null)
                {
                    return View("_AdminError", model: "Not Found");
                }
                ViewBag.SiteName = "Chỉnh sửa trạng thái";
                return View(status);
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("edit/{id?}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,name,description")] Status status)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Entry(status).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                ViewBag.SiteName = "Chỉnh sửa trạng thái";
                return View(status);
            }
            catch(Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("delete/{id?}")]
        public async Task<ActionResult> Delete(int? id)
        {
            if (id < 0 || id == null)
            {
                return RedirectToAction("Index");
            }
            try
            {
                Status status = await db.Status.FindAsync(id);
                if (status == null)
                {
                    return View("_AdminError", model: "Not Found");
                }
                ViewBag.SiteName = "Xóa trạng thái";
                return View(status);
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("delete/{id?}")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            try
            {
                Status status = await db.Status.FindAsync(id);
                db.Status.Remove(status);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }catch(Exception ex)
            {
                int count = await db.Books.CountAsync(b => b.statusId == id);
                if(count > 0)
                {
                    return View("_AdminError", model: $"Không thể xóa được dữ liệu, do có {count} nội dung liên quan tới trạng thái này!");
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
