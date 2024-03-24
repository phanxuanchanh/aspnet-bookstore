using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using CuaHangSach.Models;
using System.Threading.Tasks;

namespace CuaHangSach.Controllers
{
    [RoutePrefix("cart")]
    public class CartController : Controller
    {
        CuaHangSachDbContext db = new CuaHangSachDbContext();

        [Route("list")]
        public ActionResult Index()
        {
            var cart = Session["cart"] as CartModel;
            ViewBag.ShoppingCartAct = "active";
            ViewBag.cart = cart;
            ViewBag.SiteName = "Danh sách giỏ hàng";
            if (Request.IsAjaxRequest())
            {
                return PartialView("_IndexPartial");
            }
            return View();
        }
        [HttpPost]
        public ActionResult AddToCart(int bookId, int bookNumber = 1)
        {
            var cart = Session["cart"] as CartModel;
            if (cart == null)
            {
                cart = new CartModel();
                Session["cart"] = cart;
            }
            Book book = db.Books.Find(bookId);
            var item = new CartItem(book, bookNumber);
            cart.Add(item);
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult Edit(int bookId, int bookNumber)
        {
            var cart = Session["cart"] as CartModel;
            cart.Edit(bookId, bookNumber);
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult Delete(int bookId)
        {
            var cart = Session["cart"] as CartModel;
            cart.Delete(bookId);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Order()
        {
            ViewBag.SiteName = "Đặt hàng";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Order([Bind(Include = "ID,location,phoneNumber,email,customerName")] Invoice invoice)
        {
            var cart = Session["cart"] as CartModel;
            if (cart == null || cart.TotalProduct == 0)
            {
                return RedirectToAction("Index", "Home");
            }
            try
            {
                if (ModelState.IsValid)
                {
                    invoice.name = $"HD/{DateTime.Now}/{cart.TotalBook}/{cart.TotalMoney}";
                    invoice.orderTime = DateTime.Now;
                    invoice.totalMoney = cart.TotalMoney;
                    db.Invoices.Add(invoice);
                    foreach (var item in cart.List)
                    {
                        InvoiceDetail invoiceDetail = new InvoiceDetail
                        {
                            invoiceId = invoice.ID,
                            bookId = item.book.ID,
                            bookNumber = item.bookNumber,
                            unitPrice = item.book.price,
                            intoMoney = item.book.price * item.bookNumber
                        };
                        db.InvoiceDetails.Add(invoiceDetail);
                    }
                    await db.SaveChangesAsync();
                    cart.DeleteAll();
                    ViewBag.SiteName = "Đặt hàng thành công";
                    return View("OrderSuccess", invoice);
                }
                ViewBag.SiteName = "Đặt hàng";
                return View();
            }
            catch (Exception ex)
            {
                TempData["LoiDatHang"] = "Đặt hàng không thành công.<br>" + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [ChildActionOnly]
        public int TotalProduct()
        {
            var cart = Session["cart"] as CartModel;
            if (cart == null)
                return 0;
            return cart.TotalProduct;
        }
    }
}