using PagedList;
using WebBanSach.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using WebBanSach.Models;

namespace QuanLiBanSach02.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {
            ViewBag.Category = da.Categories.ToList();
        }
        private BookStoreEntities da = new BookStoreEntities();

        // GET: Home
        public ActionResult Index(int? page, int? pageSize, int? categoryId, float? fromPrice, float? toPrice, string sortOrder)
        {
            if (page == null)
            {
                page = 1;
            }
            if (pageSize == null)
            {
                pageSize = 8;
            }

            List<Product> products = GetProductsByCategory(categoryId);

            if (fromPrice != null || toPrice != null)
            {
                if (fromPrice != null && toPrice != null)
                {
                    products = products.Where(p => p.Price > fromPrice && p.Price < toPrice).ToList();
                }
                else if (fromPrice != null && toPrice == null)
                {
                    products = products.Where(p => p.Price > fromPrice).ToList();
                }
                else if (fromPrice == null && toPrice != null)
                {
                    products = products.Where(p => p.Price < toPrice).ToList();
                }
            }

            ViewBag.CurrentSort = sortOrder;
            ViewBag.PriceSortParam = string.IsNullOrEmpty(sortOrder) ? "price_desc" : "";

            switch (sortOrder)
            {
                case "price_asc":
                    products = products.OrderBy(p => p.Price).ToList();
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price).ToList();
                    break;
                default:
                    // No sorting parameter specified, use default order
                    break;
            }

            return View(products.ToPagedList((int)page, (int)pageSize));
        }

        public ActionResult Search(String search = "")
        {
            if (search == "")
            {
                return RedirectToAction("Index");
            }
            else
            {
                List<Product> p = da.Products.Where(s => s.ProductName.Contains(search)).ToList();
                search = ViewBag.Search;
                return View(p);
            }
        }

        public List<Product> GetProductsByCategory(int? categoryId)
        {
            if (categoryId == null)
            {
                return da.Products.ToList();
            }
            else
            {
                return da.Products.Where(p => p.CategoryID == categoryId).ToList();
            }
        }
    }
}













//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Web;
//using System.Web.Mvc;

//namespace WebBanSach.Controllers
//{
//    public class HomeController : Controller
//    {
//        public ActionResult Index()
//        {
//            return View();
//        }

//        public ActionResult About()
//        {
//            ViewBag.Message = "Your application description page.";

//            return View();
//        }

//        public ActionResult Contact()
//        {
//            ViewBag.Message = "Your contact page.";

//            return View();
//        }
//    }
//}