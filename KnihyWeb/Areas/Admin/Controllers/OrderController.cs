using Knihy.DataAccess.Repository.IRepository;
using Knihy.Models;
using Knihy.Models.ViewModels;
using Knihy.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Security.Claims;

namespace KnihyWeb.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }
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
            OrderVM = new OrderVM()
            {
                OrderHeader = _unitOfWork.OrderHeader.Get(u => u.Id == orderId, includeProperties: "ApplicationUser"),
                OrderDetail = _unitOfWork.OrderDetail.GetAll(u => u.OrderHeaderId == orderId, includeProperties: "Product")
            };
            
            return View(OrderVM);
        }
        [HttpPost]
        [Authorize(Roles =(SD.Role_Admin)+" , "+(SD.Role_Employee))]
        public IActionResult UpdateOrderDetails()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeaderFromDb.Name=OrderVM.OrderHeader.Name;
            orderHeaderFromDb.PhoneNumber = OrderVM.OrderHeader.PhoneNumber;
            orderHeaderFromDb.StreetAddress = OrderVM.OrderHeader.StreetAddress;
            orderHeaderFromDb.City = OrderVM.OrderHeader.City;
            orderHeaderFromDb.PostalCode = OrderVM.OrderHeader.PostalCode;
            orderHeaderFromDb.State = OrderVM.OrderHeader.State;
           if(!string.IsNullOrEmpty(OrderVM.OrderHeader.Carrier))
            {
                orderHeaderFromDb.Carrier=OrderVM.OrderHeader.Carrier;
            }
            if (!string.IsNullOrEmpty(OrderVM.OrderHeader.TrackingNumber))
            {
                orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            }
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);
            _unitOfWork.Save();
            TempData["success"] = "Order header updated successfully";
            return RedirectToAction(nameof(Details), new {orderId=orderHeaderFromDb.Id});
        }
        [HttpPost]
        [Authorize(Roles = (SD.Role_Admin) + " , " + (SD.Role_Employee))]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeader.UpdateStatus(OrderVM.OrderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();
            TempData["success"] = "Order header updated successfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }

            public IActionResult ShipOrder()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
            orderHeaderFromDb.Carrier = OrderVM.OrderHeader.Carrier;
            orderHeaderFromDb.TrackingNumber = OrderVM.OrderHeader.TrackingNumber;
            orderHeaderFromDb.ShippingDate = DateTime.Now;
            orderHeaderFromDb.OrderStatus = SD.StatusShipped;
            if(orderHeaderFromDb.PaymentStatus==SD.PaymentStatusDelayedPayment)
            {
                orderHeaderFromDb.PaymentDue = DateOnly.FromDateTime(DateTime.Now.AddDays(30));
            }
            _unitOfWork.OrderHeader.Update(orderHeaderFromDb);

            
            _unitOfWork.Save();
            TempData["success"] = "Order shipped successfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }
        [HttpPost]
        [Authorize(Roles = (SD.Role_Admin) + " , " + (SD.Role_Employee))]
        public IActionResult CancelOrder()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeader.Get(u => u.Id == OrderVM.OrderHeader.Id);
            if(orderHeaderFromDb.PaymentStatus==SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason=RefundReasons.RequestedByCustomer,
                    PaymentIntent=orderHeaderFromDb.PaymentIntentId
                };
                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDb.Id,SD.StatusCancelled);
            }
            else
            {
                _unitOfWork.OrderHeader.UpdateStatus(orderHeaderFromDb.Id, SD.StatusCancelled);
            }
            _unitOfWork.Save();
            TempData["success"] = "Order cancelled successfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.OrderHeader.Id });
        }
        #region APICALLS

        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> objOrderHeadertList = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();

            if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
            {
                objOrderHeadertList = _unitOfWork.OrderHeader.GetAll(includeProperties: "ApplicationUser").ToList();
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;

                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                objOrderHeadertList = _unitOfWork.OrderHeader.GetAll(u => u.ApplicationUserId == userId, includeProperties: "ApplicationUser");
            }
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
