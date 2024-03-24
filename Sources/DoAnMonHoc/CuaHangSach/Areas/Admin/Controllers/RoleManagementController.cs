using CuaHangSach.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Permissions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using X.PagedList;

namespace CuaHangSach.Areas.Admin.Controllers
{
    [RouteArea("Admin", AreaPrefix = "administration")]
    [RoutePrefix("role-management")]
    [Authorize(Roles = "Super Admin")]
    public class RoleManagementController : Controller
    {
        UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(new ApplicationDbContext()));
        RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));

        [Route("users/{page?}")]
        public async Task<ActionResult> Users(int? page)
        {
            try
            {
                int pageNumber = (page == null || page < 1) ? 1 : page.Value;
                int pageSize = 2;
                int n = (pageNumber - 1) * pageSize;
                int totalItemCount = await userManager.Users.CountAsync();
                List<ApplicationUser> onePageOfData = await userManager.Users.OrderBy(p => p.UserName).Skip(n).Take(pageSize).ToListAsync();
                StaticPagedList<ApplicationUser> users = new StaticPagedList<ApplicationUser>(onePageOfData, pageNumber, pageSize, totalItemCount);
                ViewBag.Users = users;
                ViewBag.SiteName = "Danh sách người dùng và quyền hạn";

                if (Request.IsAjaxRequest())
                {
                    return PartialView("_UserListPartial");
                }
                return View();
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("user-details/{id}")]
        public async Task<ActionResult> Details(string id)
        {
            try
            {
                ApplicationUser user = await userManager.FindByIdAsync(id);
                List<string> roles = await userManager.GetRoles(user.Id).ToListAsync();
                ViewBag.Roles = roles;
                ViewBag.SiteName = "Chi tiết về thông tin người dùng";
                return View(user);
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("roles")]
        public async Task<ActionResult> Roles()
        {
            try
            {
                List<IdentityRole> identityRoles = await roleManager.Roles.ToListAsync();
                List<RoleModels> rolesName = new List<RoleModels>();
                foreach (IdentityRole identityRole in identityRoles)
                {
                    rolesName.Add(new RoleModels { ID = identityRole.Id, roleName = identityRole.Name });
                }
                ViewBag.SiteName = "Danh sách các vai trò";
                return View(rolesName);
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("create")]
        public ActionResult Create()
        {
            return RedirectToAction("SetRole");
        }

        [Route("set-role")]
        public ActionResult SetRole()
        {
            ViewBag.SiteName = "Đặt phân quyền mới cho trang";
            return View();
        }

        [Route("set-role")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetRole(RoleModels role)
        {
            try
            {
                ViewBag.SiteName = "Đặt phân quyền mới cho trang";
                if (ModelState.IsValid)
                {
                    IdentityRole checkRole = roleManager.FindByName(role.roleName);
                    if (checkRole != null)
                    {
                        ViewBag.Status = "Đã có vai trò này";
                        return View();
                    }
                    await roleManager.CreateAsync(new IdentityRole(role.roleName));

                    ViewBag.Status = "Thêm thành công vai trò";
                    return View();
                }
                ViewBag.Status = "Vai trò không hợp lệ";
                return View();
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("delete-role/{roleId}")]
        public ActionResult DeleteRole(string roleId)
        {
            try
            {
                IdentityRole identityRole = roleManager.FindById(roleId);
                if (identityRole == null)
                    return RedirectToAction("Roles");
                if (identityRole.Name != "Super Admin")
                {
                    roleManager.Delete(identityRole);
                    TempData["deleteRole"] = "Xóa thành công";
                    return RedirectToAction("Roles");
                }
                TempData["deleteRole"] = "Xóa thất bại";
                return RedirectToAction("Roles");
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("add-role-for-user/{userId}")]
        public ActionResult AddRoleForUser(string userId)
        {
            try
            {
                ApplicationUser user = userManager.FindById(userId);
                if (user == null)
                    return RedirectToAction("Users");
                ViewBag.RoleId = new SelectList(roleManager.Roles, "Id", "Name");
                ViewBag.SiteName = "Phân quyền cho người dùng";
                return View(user);
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }

        }

        [Route("add-role-for-user/{userId}")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddRoleForUser(string roleId, string Email)
        {
            try
            {
                ApplicationUser user = userManager.FindByEmail(Email);
                IdentityRole identityRole = await roleManager.FindByIdAsync(roleId);
                if (user == null || identityRole == null)
                {
                    TempData["addRoleForUser"] = "Không thể phân quyền cho người dùng này, kiểm tra lại dữ liệu đầu vào";
                    return RedirectToAction("AddRoleForUser", new { userId = user.Id });
                }
                List<string> checkRoles = await userManager.GetRoles(user.Id).ToListAsync();
                bool check = checkRoles.Any(r => r.Equals(identityRole.Name));
                if (check)
                {
                    TempData["addRoleForUser"] = "Người dùng đã được phân quyền này";
                    return RedirectToAction("AddRoleForUser", new { userId = user.Id });
                }
                userManager.AddToRole(user.Id, identityRole.Name);
                TempData["addRoleForUser"] = "Đã phân quyền thành công cho người dùng này";
                return RedirectToAction("AddRoleForUser", new { userId = user.Id });
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }

        [Route("delete-role-for-user/{userId}")]
        public async Task<ActionResult> DeleteRoleForUser(string userId)
        {
            try
            {
                ApplicationUser user = userManager.FindById(userId);
                if (user == null)
                    return RedirectToAction("Users");
                List<string> roles = await userManager.GetRoles(user.Id).ToListAsync();
                ViewBag.Roles = roles;
                ViewBag.SiteName = "Xóa phân quyền của người dùng";
                return View(user);
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }

        }

        [Route("delete-role-for-user/{userId}")]
        [HttpPost, ActionName("DeleteRoleForUser")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRoleForUserConfirmed(string role, string Email)
        {
            try
            {
                ApplicationUser user = userManager.FindByEmail(Email);
                userManager.RemoveFromRoleAsync(user.Id, role);
                TempData["deleteRoleForUser"] = "Đã xóa thành công vai trò ra khỏi người dùng này";
                return RedirectToAction("DeleteRoleForUser", new { userId = user.Id });
            }
            catch (Exception ex)
            {
                return View("_AdminError", model: $"Lỗi: {ex.Message}");
            }
        }
    }
}