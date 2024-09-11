﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebBanSach.Models;
using System.Transactions;
using PayPal.Api;

namespace WebBanSach.Controllers
{
    public class CartController : Controller
    {
        private BookStoreEntities da = new BookStoreEntities();

        // GET: Admin/Cart
        public ActionResult Cart()
        {
            List<CartModels> cart = Session["cart"] as List<CartModels>;
            return View(cart);
        }

        // Add product to cart
        public RedirectToRouteResult AddToCart(int id)
        {
            if (Session["cart"] == null)
            {
                Session["cart"] = new List<CartModels>();
            }
            List<CartModels> cart = Session["cart"] as List<CartModels>;

            if (cart.FirstOrDefault(s => s.ProductID == id) == null)
            {
                Product p = da.Products.FirstOrDefault(s => s.ProductID == id);
                CartModels newCart = new CartModels();
                newCart.ProductID = id;
                newCart.ProductName = p.ProductName;
                newCart.Quantity = 1;
                newCart.Price = Convert.ToDouble(p.Price);
                cart.Add(newCart);
            }
            else
            {
                CartModels cartItem = cart.FirstOrDefault(s => s.ProductID == id);
                cartItem.Quantity++;
            }
            Session["cart"] = cart;

            return RedirectToAction("Detail", "Book", new { id = id });
        }

        // Update quantity product into cart
        public RedirectToRouteResult UpdateCart(int id, int txtSoLuong)
        {
            List<CartModels> cart = Session["cart"] as List<CartModels>;
            CartModels item = cart.FirstOrDefault(m => m.ProductID == id);
            if (item != null)
            {
                item.Quantity = txtSoLuong;
                Session["cart"] = cart;
            }
            return RedirectToAction("Cart");
        }

        // delete product into cart
        public RedirectToRouteResult DeleteCartItem(int id)
        {
            List<CartModels> cart = Session["cart"] as List<CartModels>;
            CartModels item = cart.FirstOrDefault(m => m.ProductID == id);
            if (item != null)
            {
                cart.Remove(item);
                Session["cart"] = cart;
            }
            return RedirectToAction("Cart");
        }

        //[HttpPost]
        public ActionResult Order()
        {
            List<CartModels> cart = Session["cart"] as List<CartModels>;
            string email = (string)Session["Email"];

            using (TransactionScope scope = new TransactionScope())
            {
                int userID = (int)Session["UserID"];
                try
                {
                    if (!cart.Any())
                    {
                        TempData["cartIsEmpty"] = "Chưa có đơn hàng";
                    }
                    else
                    {
                        Models.Order order = new Models.Order();
                        order.OrderDate = DateTime.Now;
                        order.UserID = userID;
                        order.Status = "Tiền mặt";

                        da.Orders.Add(order);
                        da.SaveChanges();

                        var idOrder = order.OrderID;

                        foreach (CartModels item in cart)
                        {
                            OrderDetail orderDetail = new OrderDetail();
                            orderDetail.OrderID = idOrder;
                            orderDetail.ProductID = item.ProductID;
                            orderDetail.UnitPrice = item.Price;
                            orderDetail.Quantity = item.Quantity;
                            da.OrderDetails.Add(orderDetail);
                            da.SaveChanges();
                        }

                        scope.Complete();
                        cart.Clear();

                        TempData["SuccessMessage"] = "Đặt hàng thành công!";
                        return RedirectToAction("Cart");

                    }
                    return RedirectToAction("Cart");

                }
                catch (Exception ex)
                {
                    scope.Dispose();
                    return View("Cart");
                }
            }
        }

        public ActionResult FailureView()
        {
            return View();
        }
        public ActionResult SuccessView()
        {
            return View();
        }

        public ActionResult PaymentWithPaypal(string Cancel = null)
        {
            List<CartModels> cart = Session["cart"] as List<CartModels>;
            string email = (string)Session["Email"];

            APIContext apiContext = PaypalConfiguration.GetAPIContext();
            using (TransactionScope scope = new TransactionScope())
            {
                try
                {
                    if (!cart.Any())
                    {
                        TempData["cartIsEmpty"] = "Chưa có đơn hàng";
                    }
                    else
                    {
                        string payerId = Request.Params["PayerID"];
                        if (string.IsNullOrEmpty(payerId))
                        {
                            string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Cart/PaymentWithPayPal?";
                            var guid = Convert.ToString((new Random()).Next(100000));
                            var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid);

                            var links = createdPayment.links.GetEnumerator();
                            string paypalRedirectUrl = null;
                            while (links.MoveNext())
                            {
                                Links lnk = links.Current;
                                if (lnk.rel.ToLower().Trim().Equals("approval_url"))
                                {
                                    paypalRedirectUrl = lnk.href;
                                }
                            }
                            Session.Add(guid, createdPayment.id);

                            return Redirect(paypalRedirectUrl);
                        }
                        else
                        {
                            var guid = Request.Params["guid"];
                            var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);
                            int userID = (int)Session["UserID"];

                            if (executedPayment.state.ToLower() != "approved")
                            {
                                return View("FailureView");
                            }

                            Models.Order order = new Models.Order();
                            order.OrderDate = DateTime.Now;
                            order.Status = "Paypal";
                            order.UserID = userID;

                            da.Orders.Add(order);
                            da.SaveChanges();

                            var idOrder = order.OrderID;

                            foreach (CartModels item in cart)
                            {
                                OrderDetail orderDetail = new OrderDetail();
                                orderDetail.OrderID = idOrder;
                                orderDetail.ProductID = item.ProductID;
                                orderDetail.UnitPrice = item.Price;
                                orderDetail.Quantity = item.Quantity;
                                da.OrderDetails.Add(orderDetail);
                                da.SaveChanges();
                            }

                            scope.Complete();
                            cart.Clear();

                            TempData["SuccessMessage2"] = "Thanh toán thành công. Xin cảm ơn!";
                            return RedirectToAction("Cart");
                        }
                    }
                    return RedirectToAction("Cart");
                }
                catch (Exception ex)
                {
                    scope.Dispose();
                    return View("FailureView");
                }
            }
        }


        private PayPal.Api.Payment payment;
        private Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution()
            {
                payer_id = payerId
            };
            this.payment = new Payment()
            {
                id = paymentId
            };
            return this.payment.Execute(apiContext, paymentExecution);
        }
        private Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {
            List<CartModels> cart = Session["cart"] as List<CartModels>;
            double subtotal = cart.Sum(item => item.Price * item.Quantity);

            //create itemlist and add item objects to it  
            var itemList = new ItemList()
            {
                items = new List<Item>()
            };
            //Adding Item Details like name, currency, price etc
            foreach (var item in cart)
            {
                itemList.items.Add(new Item()
                {
                    name = item.ProductName,
                    currency = "USD",
                    price = item.Price.ToString(),
                    quantity = item.Quantity.ToString(),
                    sku = item.ProductID.ToString(),
                });
            }

            var payer = new Payer()
            {
                payment_method = "paypal"
            };
            // Configure Redirect Urls here with RedirectUrls object  
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };
            // Adding Tax, shipping and Subtotal details  
            var details = new Details()
            {
                tax = "0",
                shipping = "0",
                subtotal = subtotal.ToString()
            };
            //Final amount with details  
            var amount = new Amount()
            {
                currency = "USD",
                total = subtotal.ToString(), // Total must be equal to sum of tax, shipping and subtotal.  
                details = details
            };
            var transactionList = new List<PayPal.Api.Transaction>();
            // Adding description about the transaction  
            var paypalOrderId = DateTime.Now.Ticks;
            transactionList.Add(new PayPal.Api.Transaction()
            {
                description = $"Invoice #{paypalOrderId}",
                invoice_number = paypalOrderId.ToString(), //Generate an Invoice No    
                amount = amount,
                item_list = itemList
            });
            this.payment = new Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };
            // Create a payment using a APIContext  
            return this.payment.Create(apiContext);
        }

        private void ClearCart()
        {
            Session["cart"] = new List<CartModels>();
        }

    }
}