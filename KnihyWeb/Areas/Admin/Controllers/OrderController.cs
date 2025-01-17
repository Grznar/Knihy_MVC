using Knihy.DataAccess.Repository.IRepository;
using Knihy.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace KnihyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            return View();
        }


                                    #region APICALLS

        [HttpGet]
        public IActionResult GetAll()
        {
            List<OrderHeader> objOrderHeadertList = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            return Json(new { data = objOrderHeadertList });
        }
       
        #endregion
    }
}
