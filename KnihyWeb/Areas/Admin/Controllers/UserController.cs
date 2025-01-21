using Knihy.DataAccess.Data;
using Knihy.DataAccess.Repository.IRepository;
using Knihy.Models;
using Knihy.Models.ViewModels;
using Knihy.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace KnihyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public UserController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db=db;
            _userManager = userManager;
            
        }
        public IActionResult Index()
        {
           
           
            return View();
        }

        public IActionResult RoleManager(string id)
        {
            var roleId = _db.UserRoles.FirstOrDefault(u => u.UserId == id).RoleId;
            if (roleId == null)
            {
                return View(NotFound());
            }
            ApplicationUserVM user = new ApplicationUserVM()
            {
                ActiveRole = roleId,
                ApplicationUser = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id),
                RoleList = _db.Roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name
                }),
                CompanyList = _db.Company.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name.ToString()
                }),

            };
            user.ApplicationUser.Role = _db.Roles.FirstOrDefault(u => u.Id == roleId).Name;
            return View(user);

        }
        [HttpPost]
        public IActionResult RoleManager(ApplicationUserVM user)
        {
            var roleId = _db.UserRoles.FirstOrDefault(u => u.UserId == user.ApplicationUser.Id).RoleId;
            string oldRole= _db.Roles.FirstOrDefault(u => u.Id == roleId).Name;
            if(!(user.ApplicationUser.Role==oldRole))
            {
                //role updated
                ApplicationUser userFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == user.ApplicationUser.Id);
                if(user.ApplicationUser.Id==SD.Role_Company)
                {
                    userFromDb.CompanyId = user.ApplicationUser.CompanyId;
                }
                if(oldRole==SD.Role_Company)
                {
                    userFromDb.CompanyId = null;
                }
                _db.SaveChanges();
                _userManager.RemoveFromRoleAsync(userFromDb, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(userFromDb, user.ApplicationUser.Role).GetAwaiter().GetResult();
            }
            return RedirectToAction(nameof(Index));

        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> objUserList = _db.ApplicationUsers.Include(u => u.Company).ToList();
            var userRoles = _db.UserRoles.ToList();
            var roles = _db.Roles.ToList();
            foreach (var user in objUserList)
            {
                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
              user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;
                
                if (user.Company == null)
                {
                    user.Company = new Company()
                    {
                        Name = ""
                    };
                }
            }
            return Json(new { data = objUserList });
        }
        [HttpPost]
        public IActionResult LockUnlock([FromBody]string id)
        {

            var objFromDb= _db.ApplicationUsers.FirstOrDefault(u=>u.Id==id);
            if (objFromDb==null)
            {
                return Json(new { success = true, message = "Error while locking, unlocking" });
            }
            if(objFromDb.LockoutEnd!=null && objFromDb.LockoutEnd>DateTime.Now)
            {
                //user locked
                objFromDb.LockoutEnd= DateTime.Now;

            }
            else
            {
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            _db.SaveChanges();
            return Json(new {success=true,message="Locked/unlocked Successfull"});
        }

        
        #endregion
    }
}
