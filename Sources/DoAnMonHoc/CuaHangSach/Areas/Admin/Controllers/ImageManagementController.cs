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
using CuaHangSach.Common;

namespace CuaHangSach.Areas.Admin.Controllers
{
    [RouteArea("Admin", AreaPrefix = "administration")]
    [RoutePrefix("image-management")]
    [Authorize(Roles = "Super Admin, Admin")]
    public class ImageManagementController : Controller
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
                int totalItemCount = await db.Books.CountAsync();
                List<Image> onePageOfData = await db.Images
                    .OrderBy(p => p.ID).Skip(n).Take(pageSize).ToListAsync();
                StaticPagedList<Image> images = new StaticPagedList<Image>(onePageOfData, pageNumber, pageSize, totalItemCount);
                ViewBag.Images = images;
                ViewBag.SiteName = "Danh sách hình ảnh";

                if (Request.IsAjaxRequest())
                {
                    return PartialView("_ImageListPartial");
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
                Image image = await db.Images.FindAsync(id);
                if (image == null)
                {
                    return View("_AdminError", model: "Not Found");
                }
                ViewBag.SiteName = "Chi tiết hình ảnh";
                return View(image);
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("create")]
        public ActionResult Create()
        {
            return RedirectToAction("Upload");
        }

        [Route("edit/{id?}")]
        public async Task<ActionResult> Edit(long? id)
        {
            if (id == null || id < 1)
                return RedirectToAction("Index");
            try
            {
                Image image = await db.Images.FindAsync(id);
                if (image == null)
                {
                    return View("_AdminError", model: "Not Found");
                }
                ViewBag.SiteName = "Chỉnh sửa thông tin hình ảnh";
                return View(image);
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("edit/{id?}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,name,description")] Image image)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Image img = await db.Images.FindAsync(image.ID);
                    TryUpdateModel(img, new string[] { "name", "description" });
                    await db.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
                ViewBag.SiteName = "Chỉnh sửa thông tin hình ảnh";
                return View(image);
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
                Image image = await db.Images.FindAsync(id);
                if (image == null)
                {
                    return View("_AdminError", model: "Not Found");
                }
                ViewBag.SiteName = "Xóa hình ảnh";
                return View(image);
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
                Image image = await db.Images.FindAsync(id);
                db.Images.Remove(image);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                long count = await db.ImageDistributions.CountAsync(i => i.imageId == id);
                if (count > 0)
                {
                    return View("_AdminError", model: $"Không thể xóa được dữ liệu, do có {count} nội dung sử dụng tới hình ảnh này!");
                }
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }

        }

        [Route("upload")]
        public ActionResult Upload()
        {
            ViewBag.SiteName = "Tải lên hình ảnh";
            return View();
        }

        [Route("upload")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Upload(HttpPostedFileBase file, string name, string description = null)
        {
            try
            {
                if (file == null)
                {
                    ViewBag.StatusAndResult = "Không có tập tin tải lên";
                    return View();
                }
                    
                string saveLocation = Server.MapPath("~/Photos/");
                string fileName = CreateUrl.TextToUrl(name);
                Upload upload = new Upload(saveLocation, fileName);
                upload.FileUpload = file;
                StatusAndResult<string> statusAndResult =  upload.Complete();
                ViewBag.SiteName = "Tải lên hình ảnh";
                if (statusAndResult.BoolStatus)
                {
                    ViewBag.StatusAndResult = statusAndResult.StringStatus;
                    bool check = await db.Images.AnyAsync(i => i.name == name && i.source == statusAndResult.Result);
                    if (check)
                    {
                        ViewBag.StatusAndResult = "Đã tồn tại hình này trong database";
                        return View();
                    }
                    Image image = new Image
                    {
                        name = name,
                        description = description,
                        source = statusAndResult.Result.Substring(statusAndResult.Result.LastIndexOf("/") + 1),
                        uploadTime = DateTime.Now
                    };
                    db.Images.Add(image);
                    await db.SaveChangesAsync();
                    return View();
                }
                ViewBag.StatusAndResult = statusAndResult.StringStatus;
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
