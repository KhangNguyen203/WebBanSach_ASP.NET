using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebBanSach.Models;
using System.Transactions;

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

                        TempData["SuccessMessage"] = "Thanh toán thành công!";
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

    }
}