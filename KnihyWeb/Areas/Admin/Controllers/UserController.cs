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
        
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UserController( UserManager<IdentityUser> userManager, 
            IUnitOfWork unitOfWork,RoleManager<IdentityRole> roleManager)
        {
            
            _userManager = userManager; 
            _unitOfWork = unitOfWork;
            _roleManager = roleManager;
        }
        public IActionResult Index()
        {
           
           
            return View();
        }

        public IActionResult RoleManager(string id)
        {
            //var roleId = _db.UserRoles.FirstOrDefault(u => u.UserId == id).RoleId;
          
            
           
            ApplicationUserVM user = new ApplicationUserVM()
            {
                
                //ApplicationUser = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id),
                ApplicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == id,includeProperties:"Company"),
                //RoleList = _db.Roles.Select(i => new SelectListItem
                //{
                //    Text = i.Name,
                //    Value = i.Name
                //}),
                
                RoleList = _roleManager.Roles.Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name
                }),
                CompanyList = _unitOfWork.Company.GetAll().Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Name.ToString()
                }),

            };
            user.ApplicationUser.Role = _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.Get(u => u.Id == id)).GetAwaiter().GetResult().FirstOrDefault();
            return View(user);

        }
        [HttpPost]
        public IActionResult RoleManager(ApplicationUserVM applicationUserVM)
        {
            ApplicationUser applicationUser = _unitOfWork.ApplicationUser.Get(u => u.Id == applicationUserVM.ApplicationUser.Id);
            string oldRole= _userManager.GetRolesAsync(_unitOfWork.ApplicationUser.Get(u => u.Id == applicationUserVM.ApplicationUser.Id)).GetAwaiter().GetResult().FirstOrDefault();
            if (!(applicationUserVM.ApplicationUser.Role == oldRole))
            {
                //role updated

                if (applicationUserVM.ApplicationUser.Role == SD.Role_Company)
                {
                    applicationUser.CompanyId = applicationUserVM.ApplicationUser.CompanyId;
                }
                 if (oldRole == SD.Role_Company)
                {
                    applicationUser.CompanyId = null;

                }
                _unitOfWork.ApplicationUser.Update(applicationUser);
                _unitOfWork.Save();

                _userManager.RemoveFromRoleAsync(applicationUser, oldRole).GetAwaiter().GetResult();
                _userManager.AddToRoleAsync(applicationUser, applicationUserVM.ApplicationUser.Role).GetAwaiter().GetResult();
            }
            else
            {
                if(oldRole==SD.Role_Company && applicationUser.CompanyId!=applicationUserVM.ApplicationUser.CompanyId)
                {
                    applicationUser.CompanyId=applicationUserVM.ApplicationUser.CompanyId;
                    _unitOfWork.ApplicationUser.Update(applicationUser);
                    _unitOfWork.Save();

                }
            }



            return RedirectToAction(nameof(Index));

        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> objUserList = _unitOfWork.ApplicationUser.GetAll(includeProperties:"Company").ToList();
            
            foreach (var user in objUserList)
            {
                
                user.Role = _userManager.GetRolesAsync(user).GetAwaiter().GetResult().FirstOrDefault();
                
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

            var objFromDb= _unitOfWork.ApplicationUser.Get(u=>u.Id==id);
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
            _unitOfWork.ApplicationUser.Update(objFromDb);
            _unitOfWork.Save();
            return Json(new { success = true, message = "Locked/unlocked Successfull" });
        }

        
        #endregion
    }
}
