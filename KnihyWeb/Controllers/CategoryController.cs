﻿using Microsoft.AspNetCore.Mvc;

namespace KnihyWeb.Controllers
{
    public class CategoryController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
