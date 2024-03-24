using CuaHangSach.Models;
using System;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Web;

namespace CuaHangSach.Common
{
    public class Upload
    {
        private string filePath;
        private string fileName;
        private string saveLocation;
        private HttpPostedFileBase fileUpload;

        public Upload()
        {
            this.fileUpload = null;
            this.filePath = null;
            this.fileName = null;
            this.saveLocation = null;
        }

        public Upload(string saveLocation, string fileName)
        {
            this.fileUpload = null;
            this.filePath = null;
            this.fileName = fileName;
            this.saveLocation = saveLocation;
        }

        public string FilePath { get => filePath; set => filePath = value; }
        public string FileName { get => fileName; set => fileName = value; }
        public string SaveLocation { get => saveLocation; set => saveLocation = value; }
        public HttpPostedFileBase FileUpload { get => fileUpload; set => fileUpload = value; }

        public bool CheckExists()
        {
            if (System.IO.File.Exists(this.filePath))
                return true;
            return false;
        }

        public string GetFilePathAndExtension(HttpPostedFileBase file, int index = 1)
        {
            Dictionary<string, string> extDict = new Dictionary<string, string>();
            extDict.Add("image/jpeg", "jpg");
            extDict.Add("image/png", "png");
            extDict.Add("image/gif", "gif");
            string mime = file.ContentType;
            string ext = "invalid";
            if (extDict.ContainsKey(mime))
            {
                ext = $"{this.saveLocation}/{this.fileName}-{index}.{extDict[mime]}";
            }
            return ext;
        }

        public StatusAndResult<string> Complete()
        {
            this.filePath = GetFilePathAndExtension(fileUpload);
            if (this.filePath == "invalid")
            {
                return new StatusAndResult<string> { BoolStatus = false, StringStatus = "Tập tin không hợp lệ" };
            }
            else
            {
                if (CheckExists())
                    return new StatusAndResult<string> { BoolStatus = false, StringStatus = "Đã tồn tại" };
                fileUpload.SaveAs(this.filePath);
                return new StatusAndResult<string> { BoolStatus = true, StringStatus = "Tải lên thành công", Result = this.filePath };
            }
        }
    } 
}