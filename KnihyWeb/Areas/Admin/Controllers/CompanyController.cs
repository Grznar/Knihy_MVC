﻿using Knihy.DataAccess.Data;
using Knihy.DataAccess.Repository.IRepository;
using Knihy.Models;
using Knihy.Models.ViewModels;
using Knihy.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace KnihyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class CompanyController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
       
        public CompanyController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
            
        }
        public IActionResult Index()
        {
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();
           
            return View(objCompanyList);
        }
        public IActionResult Upsert(int? id)
        {

            if (id == null || id == 0)
            {
                // create
                return View(new Company());
            }
            else
            {
                // update
                Company  obj= _unitOfWork.Company.Get(u => u.Id == id);
                return View(obj);
            }
        }
        [HttpPost]
        public IActionResult Upsert(Company companyObj)
        {


            if (ModelState.IsValid)
            {
               
                   

                if (companyObj.Id == 0)
                {
                    _unitOfWork.Company.Add(companyObj);

                }
                else
                {
                    _unitOfWork.Company.Update(companyObj);

                }

                _unitOfWork.Save();
                TempData["success"] = "Company created succesfully";
                return RedirectToAction("Index", "Company");
            }



            


            return View(companyObj);


        }





        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<Company> objCompanyList = _unitOfWork.Company.GetAll().ToList();
            return Json(new { data = objCompanyList });
        }
        [HttpDelete]
        public IActionResult Delete(int? id)
        {

            var companyToBeDeleted = _unitOfWork.Company.Get(u=>u.Id == id);
            if (companyToBeDeleted == null)
            {
                return Json(new { success=false,message = "Error while deleting"});
            }
            

            _unitOfWork.Company.Remove(companyToBeDeleted);
            _unitOfWork.Save();
           

            return Json(new { success = true, message = "Success while deleting" });
        }
        #endregion
    }
}