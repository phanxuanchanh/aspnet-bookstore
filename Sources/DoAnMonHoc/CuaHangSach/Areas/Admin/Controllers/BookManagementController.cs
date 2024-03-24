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
using System.Drawing.Printing;
using System.Web.Razor.Generator;

namespace CuaHangSach.Areas.Admin.Controllers
{
    [RouteArea("Admin", AreaPrefix = "administration")]
    [RoutePrefix("book-management")]
    [Authorize(Roles = "Super Admin, Admin")]
    public class BookManagementController : Controller
    {
        private CuaHangSachDbContext db = new CuaHangSachDbContext();

        [Route("all/{page?}")]
        [Authorize(Roles = "Super Admin, Admin, SEOer")]
        public async Task<ActionResult> Index(int? page)
        {
            try
            {
                int pageNumber = (page == null || page < 1) ? 1 : page.Value;
                int pageSize = 10;
                int n = (pageNumber - 1) * pageSize;
                int totalItemCount = await db.Books.CountAsync();
                List<Book> onePageOfData = await db.Books
                    .Include(b => b.Category).Include(b => b.Publisher).Include(b => b.Status)
                    .OrderBy(p => p.ID).Skip(n).Take(pageSize).ToListAsync();
                StaticPagedList<Book> books = new StaticPagedList<Book>(onePageOfData, pageNumber, pageSize, totalItemCount);
                ViewBag.Books = books;
                ViewBag.SiteName = "Danh sách sách";

                if (Request.IsAjaxRequest())
                {
                    return PartialView("_BookListPartial");
                }
                return View();
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("details/{id?}")]
        [Authorize(Roles = "Super Admin, Admin, SEOer")]
        public async Task<ActionResult> Details(long? id)
        {
            if (id == null || id < 1)
                return RedirectToAction("Index");
            try
            {
                Book book = await db.Books.Include(b => b.Category).Include(b => b.Publisher).SingleOrDefaultAsync(b => b.ID == id);
                if (book == null)
                {
                    return View("_AdminError", model: "Not Found");
                }
                List<Contribute> contributes = await db.Contributes
                   .Include(c => c.Author).Where(c => c.bookId == id).ToListAsync();
                List<ImageDistribution> imageDistributions = await db.ImageDistributions
                   .Include(i => i.Image).Where(c => c.bookId == id).ToListAsync();
                ViewBag.ImageDistributions = imageDistributions;
                ViewBag.Contributes = contributes;
                ViewBag.SiteName = "Chi tiết sách";
                book.description = HttpUtility.HtmlDecode(book.description);
                return View(book);
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("create")]
        public ActionResult Create()
        {
            ViewBag.categoryId = new SelectList(db.Categories, "ID", "name");
            ViewBag.publisherId = new SelectList(db.Publishers, "ID", "name");
            ViewBag.statusId = new SelectList(db.Status, "ID", "name");
            ViewBag.SiteName = "Tạo mới sách";
            return View();
        }

        [Route("create")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "ID,name,price,categoryId,publisherId,description,statusId")]Book book)
        {
            try
            {
                int check = await db.Books.CountAsync(b => b.name == book.name);
                if (check > 0)
                {
                    return View("_AdminError", model: "Dữ liệu bị trùng");
                }
                if (ModelState.IsValid)
                {
                    book.createTime = DateTime.Now;
                    book.updateTime = DateTime.Now;
                    book.views = 0;
                    book.url = CreateUrl.TextToUrl(book.name);
                    db.Books.Add(book);
                    await db.SaveChangesAsync();
                    return RedirectToAction("AddAuthor", new { id = book.ID });
                }
                ViewBag.categoryId = new SelectList(db.Categories, "ID", "name", book.categoryId);
                ViewBag.publisherId = new SelectList(db.Publishers, "ID", "name", book.publisherId);
                ViewBag.statusId = new SelectList(db.Status, "ID", "name", book.statusId);
                ViewBag.SiteName = "Tạo mới sách";
                return View(book);
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("add-author-for-book/{id?}")]
        public async Task<ActionResult> AddAuthor(long? id)
        {
            if (id == null || id < 0)
                return RedirectToAction("Index");
            try
            {
                ContributeIO contributeIO = await db.Books.Where(b => b.ID == id)
                    .Select(b => new ContributeIO
                    {
                        bookId = b.ID,
                        bookName = b.name,
                        authorId = 0,
                        role = null
                    }).SingleOrDefaultAsync();
                if (contributeIO == null)
                {
                    return View("_AdminError", model: "Not Found");
                }
                ViewBag.authorId = new SelectList(db.Authors, "ID", "name");
                ViewBag.SiteName = "Thêm tác giả cho sách";
                return View(contributeIO);
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("add-author-for-book/{id?}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddAuthor([Bind(Include = "ID, bookName, bookId, authorId, role")]ContributeIO contributeIO)
        {
            try
            {
                int check = await db.Contributes.CountAsync(c => c.bookId == contributeIO.bookId && c.authorId == contributeIO.authorId);
                if (check > 0)
                {
                    TempData["addAuthor"] = $"Đã tồn tại tác giả với vai trò {contributeIO.role} trong sách {contributeIO.bookName}";
                }
                else
                {
                    if (ModelState.IsValid)
                    {
                        Contribute contribute = new Contribute
                        {
                            bookId = contributeIO.bookId,
                            authorId = contributeIO.authorId,
                            role = contributeIO.role
                        };
                        db.Contributes.Add(contribute);
                        await db.SaveChangesAsync();
                        TempData["addAuthor"] = $"Đã thêm thành công tác giả vào sách: {contributeIO.bookName}";
                    }
                    if (!ModelState.IsValid)
                    {
                        TempData["addAuthor"] = $"Thêm thất bại tác giả vào sách: {contributeIO.bookName}";
                    }
                }
                ViewBag.authorId = new SelectList(db.Authors, "ID", "name");
                ViewBag.SiteName = "Thêm tác giả cho sách";
                return View(contributeIO);
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("edit-author-for-book/{id?}")]
        public async Task<ActionResult> EditAuthor(long? id)
        {
            if (id == null || id < 0)
                return RedirectToAction("Index");
            try
            {
                Book book = await db.Books.FindAsync(id);
                if (book == null)
                {
                    return View("_AdminError", model: "Not Found");
                }
                List<Contribute> contributes = await db.Contributes
                  .Include(c => c.Author).Where(c => c.bookId == id).ToListAsync();
                ViewBag.bookName = book.name;
                ViewBag.SiteName = "Chỉnh sửa tác giả cho sách";
                return View(contributes);
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("edit-author-for-book/{id?}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditAuthor(Contribute contribute, string authorName)
        {
            try
            {
                Book book = await db.Books.FindAsync(contribute.bookId);
                if (ModelState.IsValid)
                {
                    db.Entry(contribute).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    TempData["editAuthor"] = $"Chỉnh sửa thành công cho sách {book.name} và tác giả {authorName} với vai trò {contribute.role}";
                    return RedirectToAction("EditAuthor", new { id = book.ID });
                }
                List<Contribute> contributes = await db.Contributes
                  .Include(c => c.Author).Where(c => c.bookId == contribute.bookId).ToListAsync();
                TempData["editAuthor"] = "Chỉnh sửa thất bại";
                ViewBag.bookName = book.name;
                ViewBag.SiteName = "Chỉnh sửa tác giả cho sách";
                return View(contributes);
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("delete-author-for-book/{bookId?}/{authorId?}")]
        public async Task<ActionResult> DeleteAuthor(int? bookId, int? authorId)
        {
            if ((bookId == null || bookId < 1) && (authorId == null || authorId < 1))
                return RedirectToAction("EditAuthor");
            try
            {
                Contribute contribute = await db.Contributes.SingleOrDefaultAsync(c => c.bookId == bookId && c.authorId == authorId);
                if (contribute == null)
                {
                    return View("_AdminError", model: "Not Found");
                }
                db.Contributes.Remove(contribute);
                await db.SaveChangesAsync();
                return RedirectToAction("EditAuthor", new { id = bookId });
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("add-image-for-book/{id?}")]
        public async Task<ActionResult> AddImage(long? id)
        {
            if (id == null || id < 0)
                return RedirectToAction("Index");
            try
            {
                ImageIO imageIO = await db.Books.Where(b => b.ID == id)
                    .Select(b => new ImageIO
                    {
                        bookId = b.ID,
                        bookName = b.name
                    }).SingleOrDefaultAsync();
                if (imageIO == null)
                {
                    return View("_AdminError", model: "Not Found");
                }
                ViewBag.Images = await db.Images.ToListAsync();
                ViewBag.SiteName = "Thêm hình ảnh cho sách";
                return View(imageIO);
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("add-image-for-book/{id?}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddImage(ImageIO imageIO)
        {
            try
            {
                string status = null;
                if (ModelState.IsValid)
                {
                    string[] imagesId = imageIO.imagesId.Split(';');
                    foreach (string imageId in imagesId)
                    {
                        long id = int.Parse(imageId);
                        bool check = await db.ImageDistributions.AnyAsync(i => i.bookId == imageIO.bookId && i.imageId == id);
                        string imageName = db.Images.Find(id).name;
                        //string bookName = db.Books.Find(imageIO.bookId).name;
                        if (!check)
                        {
                            ImageDistribution imageDistribution = new ImageDistribution
                            {
                                bookId = imageIO.bookId,
                                imageId = id
                            };
                            db.ImageDistributions.Add(imageDistribution);
                            await db.SaveChangesAsync();
                            status += $"Thêm thành công :{imageName} -- ";
                        }
                        else
                        {
                            status += $"Đã tồn tại: {imageName} -- ";
                        }
                    }
                    TempData["addImage"] = status;
                    return RedirectToAction("AddImage");
                }
                TempData["addImage"] = "Thêm ảnh thất bại";
                return RedirectToAction("AddImage");
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("images-used-in-book/{id?}")]
        [Authorize(Roles = "Super Admin, Admin, SEOer")]
        public async Task<ActionResult> ImagesUsedInBook(long? id)
        {
            if (id == null || id < 0)
                return RedirectToAction("Index");
            try
            {
                Book book = await db.Books.FindAsync(id);
                if (book == null)
                {
                    return View("_AdminError", model: "Not Found");
                }
                List<ImageDistribution> imageDistributions = await db.ImageDistributions
                    .Include(i => i.Image).Where(i => i.bookId == id).ToListAsync();
                ViewBag.bookName = book.name;
                ViewBag.SiteName = "Hình ảnh đã được sử dụng trong sách";
                return View(imageDistributions);
            }catch(Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("delete-image-for-book")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteImage(long? bookId, long? imageId)
        {
            if ((bookId == null || bookId < 1) && (imageId == null || imageId < 1))
                return RedirectToAction("Index");

            try
            {
                ImageDistribution imageDistribution = await db.ImageDistributions.SingleOrDefaultAsync(i => i.bookId == bookId && i.imageId == imageId);
                if(imageDistribution == null)
                {
                    return View("_AdminError", model: "Not Found");
                }
                db.ImageDistributions.Remove(imageDistribution);
                await db.SaveChangesAsync();
                return RedirectToAction("ImagesUsedInBook", new { id = bookId });
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("edit/{id?}")]
        [Authorize(Roles = "Super Admin, Admin, SEOer")]
        public async Task<ActionResult> Edit(long? id)
        {
            if (id == null || id < 1)
                return RedirectToAction("Index");
            try
            {
                Book book = await db.Books.FindAsync(id);
                if (book == null)
                {
                    return View("_AdminError", model: "Not Found");
                }
                ViewBag.SiteName = "Chỉnh sửa sách";
                ViewBag.categoryId = new SelectList(db.Categories, "ID", "name", book.categoryId);
                ViewBag.publisherId = new SelectList(db.Publishers, "ID", "name", book.publisherId);
                ViewBag.statusId = new SelectList(db.Status, "ID", "name", book.statusId);
                book.description = HttpUtility.HtmlDecode(book.description);
                return View(book);
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("edit/{id?}")]
        [Authorize(Roles = "Super Admin, Admin, SEOer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "ID,name,price,categoryId,publisherId,description,status")] Book book)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Book bk = await db.Books.FindAsync(book.ID);
                    bk.updateTime = DateTime.Now;
                    bk.description = HttpUtility.HtmlEncode(book.description);
                    TryUpdateModel(bk, new string[] { "name", "price", "categoryId", "publisherId", "description", "status" });
                    await db.SaveChangesAsync();
                    return RedirectToAction("Details", new { id = book.ID });
                }
                ViewBag.SiteName = "Chỉnh sửa sách";
                ViewBag.categoryId = new SelectList(db.Categories, "ID", "name", book.categoryId);
                ViewBag.publisherId = new SelectList(db.Publishers, "ID", "name", book.publisherId);
                return View(book);
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
                Book book = await db.Books.Include(b => b.Category).Include(b => b.Publisher).SingleOrDefaultAsync(b => b.ID == id);
                if (book == null)
                {
                    return View("_AdminError", model: "Not Found");
                }
                ViewBag.SiteName = "Xóa sách";
                return View(book);
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
                Book book = await db.Books.FindAsync(id);
                db.Books.Remove(book);
                await db.SaveChangesAsync();
                return RedirectToAction("Index");
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
