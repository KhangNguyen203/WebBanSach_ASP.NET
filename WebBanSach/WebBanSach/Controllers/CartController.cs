﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebBanSach.Models;

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
    }
}