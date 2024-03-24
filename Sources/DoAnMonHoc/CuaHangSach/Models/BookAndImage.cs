using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CuaHangSach.Models
{
    public class BookAndImage
    {
        public Book book { get; set; }
        public Image image { get; set; }

        public List<Image> images { get; set; }

        public static List<BookAndImage> GetBook(List<BookAndImage> bookAndImage)
        {
            List<BookAndImage> bookAndImages = new List<BookAndImage>();
            foreach (BookAndImage item in bookAndImage)
            {
                bool check = bookAndImages.Any(b => b.book.name == item.book.name);
                if(check == false)
                {
                    bookAndImages.Add(new BookAndImage
                    {
                        book = item.book,
                        images = bookAndImage.Where(b => b.book.name == item.book.name)
                            .Select(b => new Image
                            {
                                ID = b.image.ID,
                                name = b.image.name,
                                source = b.image.source
                            }).ToList()
                    });
                }
            }
            return bookAndImages;
        }
    }
}