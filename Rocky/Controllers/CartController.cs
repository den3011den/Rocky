using Braintree;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Rocky_DataAccess.Data;
using Rocky_DataAccess.Repository;
using Rocky_Models;
using Rocky_Models.ViewModels;
using Rocky_Utility;
using Rocky_Utility.BrainTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Rocky.Controllers
{
    [Authorize]
    public class CartController : Controller
    {        
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IEmailSender _emailSender;

        private readonly IApplicationUserRepository _userRepo;
        private readonly IProductRepository _prodRepo;
        private readonly IInquiryHeaderRepository _InqHRepo;
        private readonly IInquiryDetailRepository _InqDRepo;

        private readonly IOrderHeaderRepository _orderHRepo;
        private readonly IOrderDetailRepository _orderDRepo;

        private readonly IBrainTreeGate _brain;

        [BindProperty]
        public ProductUserVM ProductUserVM { get; set; }

        public CartController(IWebHostEnvironment webHostEnvironment, IEmailSender emailSender,
            IApplicationUserRepository userRepo,
            IProductRepository prodRepo,
            IInquiryHeaderRepository InqHRepo,
            IInquiryDetailRepository InqDRepo,
            IOrderHeaderRepository orderHRepo,
            IOrderDetailRepository orderDRepo,
            IBrainTreeGate brain)
        {
            _userRepo = userRepo;
            _prodRepo = prodRepo;
            _InqHRepo = InqHRepo;
            _InqDRepo = InqDRepo;
            _webHostEnvironment = webHostEnvironment;
            _emailSender = emailSender;
            _orderHRepo = orderHRepo;
            _orderDRepo = orderDRepo;
            _brain = brain;

        }

        public IActionResult Index()
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if((HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart)!=null)
                && (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0))
            {
                // сессия существует
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            List<int> prodInCart = shoppingCartList.Select(i => i.ProductId).ToList();
            IEnumerable<Product> prodListTemp = _prodRepo.GetAll(u => prodInCart.Contains(u.Id));
            IList<Product> prodList = new List<Product>();

            foreach (var cartObj in shoppingCartList)
            {
                Product prodTemp = prodListTemp.FirstOrDefault(u => u.Id == cartObj.ProductId);
                prodTemp.TempSqFt = cartObj.SqFt;
                prodList.Add(prodTemp);
            }

            return View(prodList);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Index")]
        public IActionResult IndexPost(IEnumerable<Product> ProdList)
        {

            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            foreach (Product prod in ProdList)
            {
                shoppingCartList.Add(new ShoppingCart { ProductId = prod.Id, SqFt = prod.TempSqFt });
            }
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);

            return RedirectToAction(nameof(Summary));
        }

        public IActionResult Summary()
        {

            ApplicationUser applicationUser;

            if(User.IsInRole(WC.AdminRole))
            {
                if(HttpContext.Session.Get<int>(WC.SessionInquiryId)!=0)
                {
                    // корзина была загружена из запроса
                    InquiryHeader inquiryHeader = _InqHRepo.FirstOrDefault(u => u.Id == HttpContext.Session.Get<int>(WC.SessionInquiryId));
                    applicationUser = new ApplicationUser()
                    {
                        Email = inquiryHeader.Email,
                        FullName = inquiryHeader.Fullname,
                        PhoneNumber = inquiryHeader.PhoneNumber
                    };
                }
                else
                {
                    applicationUser = new ApplicationUser();
                }

                var gateway = _brain.GetGateway();
                var clientToken = gateway.ClientToken.Generate();
                ViewBag.ClientToken = clientToken;
            }
            else
            {
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                //var UserId = User.FindFirstValue(ClaimTypes.Name);
                applicationUser = _userRepo.FirstOrDefault(u => u.Id == claim.Value);
            }


             

            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if ((HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null)
                && (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0))
            {
                // сессия существует
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            List<int> prodInCart = shoppingCartList.Select(i => i.ProductId).ToList();
            IEnumerable<Product> prodList = _prodRepo.GetAll(u => prodInCart.Contains(u.Id));

            ProductUserVM = new ProductUserVM()
            {
                ApplicationUser = applicationUser,
                //ProductList = prodList.ToList()
            };

            foreach(var cartObj in shoppingCartList)
            {
                Product prodTemp = _prodRepo.FirstOrDefault(u => u.Id == cartObj.ProductId);
                prodTemp.TempSqFt = cartObj.SqFt;
                ProductUserVM.ProductList.Add(prodTemp);
            }

            return View(ProductUserVM);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(IFormCollection collection, ProductUserVM ProductUserVM)
        {

            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);

            if (User.IsInRole(WC.AdminRole))
            {
                //создание заказа
                //var orderTotal = 0.0;
                //foreach (Product prod in ProductUserVM.ProductList)
                //{
                //    orderTotal = orderTotal + (prod.Price * prod.TempSqFt);
                //}
                OrderHeader orderHeader = new OrderHeader()
                {
                     CreatedByUserId = claim.Value,
                     FinalOrderTotal = ProductUserVM.ProductList.Sum(x=>x.TempSqFt*x.Price),
                     City = ProductUserVM.ApplicationUser.City,
                     StreetAddress = ProductUserVM.ApplicationUser.StreetAddress,
                     State = ProductUserVM.ApplicationUser.State,
                     PostalCode = ProductUserVM.ApplicationUser.PostalCode,
                     FullName = ProductUserVM.ApplicationUser.FullName,
                     Email = ProductUserVM.ApplicationUser.Email,
                     PhoneNumber = ProductUserVM.ApplicationUser.PhoneNumber,
                     OrderDate = DateTime.Now,
                     OrderStatus = WC.StatusPending
                };
                _orderHRepo.Add(orderHeader);
                _orderHRepo.Save();

                foreach (var prod in ProductUserVM.ProductList)
                {

                    OrderDetail orderDetail = new OrderDetail()
                    {
                        OrderHeaderId = orderHeader.Id,
                        PricePerSqFt = prod.Price,
                        Sqft = prod.TempSqFt,
                        ProductId = prod.Id

                    };
                    _orderDRepo.Add(orderDetail);
                }

                _orderDRepo.Save();

                string nonceFromTheClient = collection["payment_method_nonce"];

                var request = new TransactionRequest
                {
                    Amount = Convert.ToDecimal(orderHeader.FinalOrderTotal),
                    PaymentMethodNonce = nonceFromTheClient,
                    OrderId = orderHeader.Id.ToString(),
                    Options = new TransactionOptionsRequest
                    {
                        SubmitForSettlement = true
                    }
                };

                var gateway = _brain.GetGateway();
                Result<Transaction> result = gateway.Transaction.Sale(request);

                if(result.Target.ProcessorResponseText=="Approved")
                {
                    orderHeader.TransactionId = result.Target.Id;
                    orderHeader.OrderStatus = WC.StatusApproved;
                }
                else
                {
                    orderHeader.OrderStatus = WC.StatusCancelled;
                }
                _orderHRepo.Save();

                return RedirectToAction(nameof(InquiryConfirmation), new { id = orderHeader.Id });

            }
            else
            {
                //создание запроса

                var PathToTemplate = _webHostEnvironment.WebRootPath + Path.DirectorySeparatorChar.ToString()
                    + "templates" + Path.DirectorySeparatorChar.ToString() + "Inquiry.html";

                var subject = "Новый запрос";
                string HtmlBody = "";
                using (StreamReader sr = System.IO.File.OpenText(PathToTemplate))
                {
                    HtmlBody = sr.ReadToEnd();
                }

                //Name: { 0}
                //Email: { 1}
                //Phone: { 2}

                //Products:{ 3}

                StringBuilder productListSB = new StringBuilder();
                foreach (var prod in ProductUserVM.ProductList)
                {
                    productListSB.Append($" - Наименование: {prod.Name} <span style='font-size:14px;> (ID: {prod.Id})</span><br />");
                }

                string messageBody = string.Format(HtmlBody,
                    ProductUserVM.ApplicationUser.FullName,
                    ProductUserVM.ApplicationUser.Email,
                    ProductUserVM.ApplicationUser.PhoneNumber,
                    productListSB.ToString());

                await _emailSender.SendEmailAsync(WC.EmailAdmin, subject, messageBody);

                InquiryHeader inquiryHeader = new InquiryHeader()
                {
                    ApplicationUserId = claim.Value,
                    Fullname = ProductUserVM.ApplicationUser.FullName,
                    Email = ProductUserVM.ApplicationUser.Email,
                    PhoneNumber = ProductUserVM.ApplicationUser.PhoneNumber,
                    InquiryDate = DateTime.Now
                };

                _InqHRepo.Add(inquiryHeader);
                _InqHRepo.Save();

                foreach (var prod in ProductUserVM.ProductList)
                {

                    InquiryDetail inquiryDetail = new InquiryDetail()
                    {
                        InquiryHeaderId = inquiryHeader.Id,
                        ProductId = prod.Id,

                    };
                    _InqDRepo.Add(inquiryDetail);
                }

                _InqDRepo.Save();

                TempData[WC.Success] = "Запрос успешно добавлен";

            }
            return RedirectToAction(nameof(InquiryConfirmation));
        }

        public IActionResult InquiryConfirmation(int id=0)
        {
            OrderHeader orderHeader = _orderHRepo.FirstOrDefault(u => u.Id == id);
            HttpContext.Session.Clear();
            return View(orderHeader);
        }


        public IActionResult Remove(int id)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            if ((HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart) != null)
                && (HttpContext.Session.Get<IEnumerable<ShoppingCart>>(WC.SessionCart).Count() > 0))
            {
                // сессия существует
                shoppingCartList = HttpContext.Session.Get<List<ShoppingCart>>(WC.SessionCart);
            }

            shoppingCartList.Remove(shoppingCartList.FirstOrDefault(u => u.ProductId == id));
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);

            TempData[WC.Success] = "Продукт удалён из корзины";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateCart(IEnumerable<Product> ProdList)
        {
            List<ShoppingCart> shoppingCartList = new List<ShoppingCart>();
            foreach(Product prod in ProdList)
            {
                shoppingCartList.Add(new ShoppingCart { ProductId = prod.Id, SqFt = prod.TempSqFt });
            }
            HttpContext.Session.Set(WC.SessionCart, shoppingCartList);
            return RedirectToAction(nameof(Index));
        }


        public IActionResult Clear()
        {
           
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
