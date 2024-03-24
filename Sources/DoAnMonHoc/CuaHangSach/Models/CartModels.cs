using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuaHangSach.Models
{
    public class CartItem
    {

        public Book book { get; set; }
        public int bookNumber { get; set; }

        public CartItem() { }
        public CartItem(Book book, int bookNumber)
        {
            this.book = book;
            this.bookNumber = bookNumber;
        }
    }

    public class CartModel
    {
        private List<CartItem> _List = new List<CartItem>();
        public List<CartItem> List => _List;

        public void Add(CartItem item)
        {
            var CartItem = _List.Find(p => p.book.ID == item.book.ID);
            if (CartItem == null)
                _List.Add(item);
            else
                CartItem.bookNumber += item.bookNumber;
        }
        public void Edit(int id, int bookNumber)
        {
            var edit = _List.Find(p => p.book.ID == id);
            edit.bookNumber = bookNumber;
        }
        public void Delete(int id)
        {
            var delete = _List.Find(p => p.book.ID == id);
            _List.Remove(delete);
        }
        public void DeleteAll()
        {
            _List.Clear();
        }

        public int TotalProduct
        {
            get { return _List.Count; }
        }
        public int TotalBook
        {
            get { return _List.Sum(p => p.bookNumber); }
        }
        public double TotalMoney
        {
            get
            {
                double kq = 0;
                kq = _List.Sum(p => p.bookNumber * p.book.price);
                return kq;
            }
        }
    }
}