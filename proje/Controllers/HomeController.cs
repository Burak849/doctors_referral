using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace proje.Controllers
{
    public class HomeController : Controller
    {

        [HttpPost]
        public ActionResult Logout()
        {
            Session.Clear(); 
            return RedirectToAction("Index", "Home"); 
        }

        public ActionResult Index()
        {
            return View();
        }

    }
}