using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CuaHangSach.Models
{
    public class ContributeIO
    {
        public long bookId { get; set; }

        [Display(Name = "Tên của sách")]
        public string bookName { get; set; }

        [Display(Name = "ID của tác giả")]
        public long authorId { get; set; }

        [Display(Name = "Vai trò của tác giả")]
        public string role { get; set; }
    }
}