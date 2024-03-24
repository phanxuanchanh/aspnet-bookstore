using CuaHangSach.Common;
using CuaHangSach.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using X.PagedList;

/*
 * Phần quản trị được viết trong thư mục Areas
 */


namespace CuaHangSach.Controllers
{
    [RoutePrefix("product")]
    public class BookController : Controller
    {
        CuaHangSachDbContext db = new CuaHangSachDbContext();

        public List<BookAndImage> GetBookAndImage(List<Book> books)
        {
            List<BookAndImage> bookAndImages = new List<BookAndImage>();
            foreach (Book item in books)
            {
                bool check = bookAndImages.Any(b => b.book.name == item.name);
                if (check == false)
                {
                    List<ImageDistribution> imageDistributions = db.ImageDistributions
                            .Include(i => i.Image)
                            .Where(i => i.bookId == item.ID)
                            .ToList();

                    List<Image> imgs = imageDistributions.Select(i => new Image
                    {
                        ID = i.Image.ID,
                        name = i.Image.name,
                        source = i.Image.source
                    }).ToList();

                    bookAndImages.Add(new BookAndImage
                    {
                        book = item,
                        images = imgs
                    });
                }
            }
            return bookAndImages;
        }

        [Route("details/{url}/{id}")]
        public async Task<ActionResult> Details(long id, string url)
        {
            try
            {
                bool check = await db.Books.AnyAsync(b => b.ID == id);
                if (!check)
                {
                    return RedirectToAction("Index", "Home");
                }
                Book book = await db.Books.Include(b => b.Category).SingleOrDefaultAsync(b => b.ID == id);
                book.views += 1;
                book.description = HttpUtility.HtmlDecode(book.description);
                TryUpdateModel(book, new string[] { "views" });
                await db.SaveChangesAsync();
                List<ImageDistribution> imageDistributions = await db.ImageDistributions
                   .Include(i => i.Image).Where(c => c.bookId == id).ToListAsync();
                ViewBag.ImageDistributions = imageDistributions;
                ViewBag.SiteName = book.name;
                return View(book);
            }
            catch(Exception ex)
            {
                return View("_Error", model: "Không thể truy cập dữ liệu");
            }
        }

        [Route("get-books")]
        public ActionResult GetBooks()
        {
            try
            {
                List<BookAndImage> bookAndImages_raw = db.ImageDistributions
                    .Include(i => i.Book).Include(i => i.Image)
                    .OrderByDescending(i => i.Book.views)
                    .Select(i => new BookAndImage
                    {
                        book = i.Book,
                        image = i.Image
                    }).ToList();

                List<BookAndImage> bookAndImages = BookAndImage.GetBook(bookAndImages_raw);
                ViewBag.SiteName = "Xem tất cả sách";
                return View(bookAndImages);
            }
            catch (Exception)
            {
                return View("Error", model: "Lỗi");
            }
        }

        [Route("top-books-by-view/{page?}")]
        public async Task<ActionResult> TopBooksByView(int? page)
        {
            try
            {
                int pageNumber = (page == null || page < 1) ? 1 : page.Value;
                int pageSize = 10;
                int n = (pageNumber - 1) * pageSize;
                int totalItemCount = await db.Books.CountAsync();
                List<Book> books = await db.Books.OrderByDescending(p => p.views).Skip(n).Take(pageSize).ToListAsync();
                List<BookAndImage> onePageOfData = GetBookAndImage(books);
                StaticPagedList<BookAndImage> bookAndImages = new StaticPagedList<BookAndImage>(onePageOfData, pageNumber, pageSize, totalItemCount);
                ViewBag.BookAndImages = bookAndImages;
                ViewBag.SiteName = "Xem tất cả sách dựa theo xếp hạng lượt xem";

                if (Request.IsAjaxRequest())
                {
                    return PartialView("_BookListPartial");
                }
                return View();
            }
            catch(Exception ex)
            {
                return View("Error", model: "Lỗi");
            }
            
        }

        [ChildActionOnly]
        public PartialViewResult _TopBooksByView(int? bookNumber)
        {
            try
            {
                int bkNumber = (bookNumber == null || bookNumber < 1) ? 5 : bookNumber.Value;
                List<Book> books = db.Books.OrderByDescending(b => b.views).Take(bkNumber).ToList();
                List<BookAndImage> bookAndImages = GetBookAndImage(books);
                return PartialView(bookAndImages);
            }
            catch(Exception ex)
            {
                return PartialView("_ErrorPartial");
            }
        }

       
        [ChildActionOnly]
        public PartialViewResult _Latest(int? bookNumber)
        {
            try
            {
                int bkNumber = (bookNumber == null || bookNumber < 1) ? 4 : bookNumber.Value;

                List<BookAndImage> bookAndImages = new List<BookAndImage>();
                List<Book> books = db.Books.OrderByDescending(b => b.createTime).Take(bkNumber).ToList();
                foreach (Book item in books)
                {
                    bool check = bookAndImages.Any(b => b.book.name == item.name);
                    if (check == false)
                    {
                        List<ImageDistribution> imageDistributions = db.ImageDistributions
                                .Include(i => i.Image)
                                .Where(i => i.bookId == item.ID)
                                .ToList();

                        List<Image> imgs = imageDistributions.Select(i => new Image
                        {
                            ID = i.Image.ID,
                            name = i.Image.name,
                            source = i.Image.source
                        }).ToList();

                        bookAndImages.Add(new BookAndImage
                        {
                            book = item,
                            images = imgs
                        });
                    }
                }
                return PartialView(bookAndImages);
            }
            catch(Exception ex)
            {
                return PartialView("_ErrorPartial");
            }
        }

        [Route("list-by-category/{id}")]
        public async Task<ActionResult> ListByCategory(int id, int? page)
        {
            try
            {
                int pageNumber = (page == null || page < 1) ? 1 : page.Value;
                int pageSize = 10;
                int n = (pageNumber - 1) * pageSize;
                int totalItemCount = await db.Books.Where(b => b.categoryId == id).CountAsync();
                List<Book> books = await db.Books.Where(b => b.categoryId == id).OrderBy(b => b.name).Skip(n).Take(pageSize).ToListAsync();
                List<BookAndImage> onePageOfData = GetBookAndImage(books);
                StaticPagedList<BookAndImage> bookAndImages = new StaticPagedList<BookAndImage>(onePageOfData, pageNumber, pageSize, totalItemCount);
                ViewBag.BookAndImages = bookAndImages;
                Category category = await db.Categories.FindAsync(id);
                ViewBag.ID = id;
                ViewBag.SiteName = $"Xem tất cả sách thuộc thể loại {category.name}";

                if (Request.IsAjaxRequest())
                {
                    return PartialView("_ListByCategoryPartial");
                }
                return View();
            }
            catch(Exception ex)
            {
                return View("Error", model: "Lỗi");
            }
        }

        [ChildActionOnly]
        public PartialViewResult _ListByCategory(int id, int? bookNumber, string mode = null)
        {
            try
            {
                int bkNumber = (bookNumber == null || bookNumber < 1) ? 10 : bookNumber.Value;
                bool check1 = db.Categories.Any(c => c.ID == id);
                if (!check1)
                {
                    return PartialView();
                }
                List<Book> books = db.Books.Where(b => b.categoryId == id).Take(bkNumber).ToList();
                List<BookAndImage> bookAndImages = GetBookAndImage(books);
                ViewBag.CategoryName = db.Categories.Find(id).name;
                ViewBag.CategoryId = id;
                ViewBag.Mode = mode;
                return PartialView(bookAndImages);
            }
            catch(Exception ex)
            {
                return PartialView("_ErrorPartial");
            }
        }

        [Route("search")]
        public async Task<ActionResult> Search(string keyword)
        {
            try
            {
                List<Book> books = await db.Books.Where(b => b.name.Contains(keyword)).ToListAsync();

                List<BookAndImage> bookAndImages = GetBookAndImage(books);
                if (bookAndImages.Count == 0)
                {
                    return View(model: "Không tìm thấy dữ liệu mà bạn yêu cầu");
                }
                ViewBag.SiteName = $"Tìm kiếm với từ khóa \"{keyword}\"";
                return View(bookAndImages);
            }
            catch(Exception ex)
            {
                return View("Error", model: "Lỗi");
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