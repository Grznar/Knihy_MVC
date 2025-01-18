using Knihy.DataAccess.Repository.IRepository;
using Knihy.Models;
using Knihy.Models.ViewModels;
using Knihy.Utility;
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
        public IActionResult Details(int orderId)
        {
            OrderVM orderVM = new()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u=>u.OrderHeaderId==orderId,includeProperties:"Product")
            };
            return View(orderVM);
        }

        #region APICALLS

        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> objOrderHeadertList = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();


            switch (status)
            {
                
                case "pending":
                    {
                        objOrderHeadertList = objOrderHeadertList.Where(u => u.PaymentStatus == SD.PaymentStatusDelayedPayment);
                        break;
                    }
                case "inprocess":
                    {
                        objOrderHeadertList = objOrderHeadertList.Where(u => u.PaymentStatus == SD.StatusInProcess);
                        break;
                    }
                case "completed":
                    {
                        objOrderHeadertList = objOrderHeadertList.Where(u => u.PaymentStatus == SD.StatusShipped);
                        break;
                    }
                case "approved":
                    {
                        objOrderHeadertList = objOrderHeadertList.Where(u => u.PaymentStatus == SD.StatusApproved);
                        break;
                    }
                default:
                    {
                       break;
                    }

            }
            return Json(new { data = objOrderHeadertList });
        }
       
        #endregion
    }
}
