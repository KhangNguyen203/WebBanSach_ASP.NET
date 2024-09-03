﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebBanSach.Models
{
    [Serializable]
    public class CartModels
    {
        private BookStoreEntities da = new BookStoreEntities();

        public int ProductID { get; set; }
        public String ProductName { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public double Total { get { return Price * Quantity; } }

        //public Cart(int productID)
        //{
        //    Product p = da.Products.FirstOrDefault(s => s.ProductID == productID);
        //    ProductID = p.ProductID;
        //    ProductName = p.ProductName;
        //    UnitPrice = p.UnitPrice;
        //    Quantity = 1;
        //}
    }
}