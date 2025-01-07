using Knihy.DataAccess.Data;
using Knihy.DataAccess.Repository.IRepository;
using Knihy.Models;
using Microsoft.AspNetCore.Mvc;

namespace KnihyWeb.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ICategoryRepository _categoryRepo;
        public CategoryController(ICategoryRepository db)
        {
            _categoryRepo = db;
        }
        public IActionResult Index()
        {
            List<Category> objCategoryList = _categoryRepo.GetAll().ToList();

            return View(objCategoryList);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public IActionResult Create(Category obj)
        {
            if (obj.Name == obj.DisplayOrder.ToString())
            {
                ModelState.AddModelError("name", "The Display order can not match the Name");
            }

            if (ModelState.IsValid)
            {
                _categoryRepo.Add(obj);
                _categoryRepo.Save();
                TempData["success"] = "Category created succesfully";
                return RedirectToAction("Index", "Category");
            }


            return View();

        }
        public IActionResult Edit(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category? catergoryFromDb = _categoryRepo.Get(u => u.Id == id);
            if (catergoryFromDb == null)
            {
                return NotFound();
            }

            return View(catergoryFromDb);
        }
        [HttpPost]
        public IActionResult Edit(Category obj)
        {


            if (ModelState.IsValid)
            {
                _categoryRepo.Update(obj);
                _categoryRepo.Save();
                TempData["success"] = "Category edited succesfully";
                return RedirectToAction("Index", "Category");
            }


            return View();

        }
        public IActionResult Delete(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            Category? catergoryFromDb = _categoryRepo.Get(u => u.Id == id); 
            if (catergoryFromDb == null)
            {
                return NotFound();
            }

            return View(catergoryFromDb);
        }
        [HttpPost, ActionName("Delete")]
        public IActionResult DeletePOST(int? id)
        {

            Category? obj = _categoryRepo.Get(u => u.Id == id);
            if (obj == null)
            {
                return NotFound();
            }
            _categoryRepo.Remove(obj);
            _categoryRepo.Save();
            TempData["success"] = "Category deleted succesfully";
            return RedirectToAction("Index", "Category");





        }


    }
}
