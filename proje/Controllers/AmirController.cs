using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using Microsoft.Ajax.Utilities;
using proje.Models;

namespace proje.Controllers
{
    public class HastaModel
    {
        public int id { get; set; }
        public string sicilno { get; set; }
        public string isim { get; set; }
        public string soyisim { get; set; }
        public DateTime? sevk_tarihi { get; set; }  
        public DateTime? sevk_saati { get; set; }    
        public int? yakakodu { get; set; }
        public int? durum { get; set; }
       
    }
    //VERİ SİLİNDİĞİNDE TÜM FİLTRELER SIFIRLANIYOR BUNU ENGELLEMELİYİM
    //GİRİŞ YAPABİLEN KİŞİ TÜM SAYFALAR ARASI GEZEBİLİYOR.
    public class KartNoAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            return httpContext.Session["KartNo"] != null;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            filterContext.Controller.TempData["ShowAlert"] = true;
            filterContext.Result = new RedirectResult("~/Home/Index");
        }
    }
    public class AmirController : Controller

    {
        private viziteEntities db = new viziteEntities();

        // GET: Amir
        [HttpGet]
        [KartNoAuthorize]
        public ActionResult Index(string sortOrder , int? id, int? month, string sicilno, string isim, string soyisim, int? yakakodu, int page = 1, int pageSize = 10)
        {
            if (Session["KartNo"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

           
           
            List<vizitealanlartablo> calisanlar = db.vizitealanlartabloes.ToList();
            List<departmanlar> departmanlar = db.departmanlars.ToList();
            List<kayitliamirtablo> kayitliAmirler = db.kayitliamirtabloes.ToList();


            if (calisanlar == null || !calisanlar.Any())
            {
                TempData["Error"] = "Çalışanlar listesi boş.";
            }
           

            //if (yakakodu.HasValue)
            //{
            //    calisanlar = calisanlar.Where(hasta => hasta.yakakodu == yakakodu.Value).ToList();
            //}



            if (id > 0) 
            {
                calisanlar = calisanlar.Where(hasta => hasta.id == id).ToList();
            }

            if (month.HasValue)
            {
                calisanlar = calisanlar.Where(hasta => hasta.sevk_tarihi.HasValue && hasta.sevk_tarihi.Value.Month == month.Value).ToList();
            }

            if (!string.IsNullOrWhiteSpace(sicilno))
            {
                calisanlar = calisanlar.Where(hasta => hasta.sicilno != null && hasta.sicilno.IndexOf(sicilno, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            if (!string.IsNullOrWhiteSpace(isim))
            {
                calisanlar = calisanlar.Where(hasta => hasta.isim != null && hasta.isim.IndexOf(isim, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            if (!string.IsNullOrWhiteSpace(soyisim))
            {
                calisanlar = calisanlar.Where(hasta => hasta.soyisim != null && hasta.soyisim.IndexOf(soyisim, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            if (yakakodu.HasValue)
            {
                calisanlar = calisanlar.Where(hasta => hasta.yakakodu.HasValue && hasta.yakakodu.Value == yakakodu.Value).ToList();
            }

            // Sıralama
            switch (sortOrder)
            {

                //ID ARTAN YAPILABİLİR ZATEN LİSTE AZALAN ŞEKLİYLE OLUŞTURULDU
                case "id_desc":
                    calisanlar = calisanlar.OrderByDescending(hasta => hasta.id).ToList();
                    break;
                case "sevk_tarihi":
                    calisanlar = calisanlar.OrderBy(hasta => hasta.sevk_tarihi).ToList();
                    break;
                case "sevk_tarihi_desc":
                    calisanlar = calisanlar.OrderByDescending(hasta => hasta.sevk_tarihi).ToList();
                    break;
                case "sevk_saati":
                    calisanlar = calisanlar.OrderBy(hasta => hasta.sevk_saati).ToList();
                    break;
                case "sevk_saati_desc":
                    calisanlar = calisanlar.OrderByDescending(hasta => hasta.sevk_tarihi).ToList();
                    break;
                default:
                    calisanlar = calisanlar.OrderBy(hasta => hasta.id).ToList();
                    break;
            }

            // Sayfalandırma kısmı
            var totalCount = calisanlar.Count();
            var calisanModelList = calisanlar
                .OrderByDescending(hasta => hasta.sevk_tarihi)
                .ThenByDescending(hasta => hasta.sevk_saati)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(hasta => new HastaModel
                {
                    id = hasta.id,
                    sicilno = hasta.sicilno,
                    isim = hasta.isim,
                    soyisim = hasta.soyisim,
                    sevk_tarihi = hasta.sevk_tarihi,
                    sevk_saati = hasta.sevk_saati,
                    yakakodu = hasta.yakakodu,
                    durum = hasta.durum,
              
                })
                .ToList();

            // ViewBag ile sayfa bilgilerini gönderir
            ViewBag.TotalCount = totalCount;
            ViewBag.CurrentPage = page;
            ViewBag.PageSize = pageSize;
            ViewBag.SelectedId = id;
            ViewBag.SelectedSicilno = sicilno;
            ViewBag.SelectedIsim = isim;
            ViewBag.SelectedSoyisim = soyisim;
            ViewBag.SelectedYakakodu = yakakodu;
            ViewBag.SelectedMonth = month;
            ViewBag.SelectedSortOrder = sortOrder;


            return View(calisanModelList);
        }


        //kartnosunu scannerdb'de buluyor
        [HttpPost]
        public ActionResult CheckKartNo(string kartno, int? departman)
        {
            var scanner = db.scanners.FirstOrDefault(s => s.kartno == kartno);
            if (scanner != null)
            {
                var hasta = db.temelcalisantabloes.FirstOrDefault(a => a.sicilno == scanner.sicilno);
                if (hasta != null)
                {
                    var amir = db.kayitliamirtabloes.FirstOrDefault(a => a.amir_sicilno == hasta.sicilno);
                    if (amir != null)
                    {
                        Session["KartNo"] = kartno;
                        Session["Ad"] = amir.amir_ad;
                        Session["Soyad"] = amir.amir_soyad;

                        // Yakakodunun amirinin karşısına çıkartıyoruz
                        int? selectedDepartman = hasta.yakakodu;

                        if (amir.departmanid == departman)
                        {
                            // Departmanı doğru şekilde almak için selectedDepartman ile sorgu yapılıyor
                            var departmani = db.departmanlars.FirstOrDefault(b => b.departmanid == selectedDepartman);
                            if (departmani != null)
                            {
                                Session["Departman"] = departmani.departmanad;
                                return RedirectToAction("Index", "Amir", new { yakakodu = hasta.yakakodu });
                            }
                            else
                            {
                                ViewBag.ErrorMessage = "Departman bulunamadı.";
                                return View("Error");
                            }
                        }
                        else
                        {
                            ViewBag.ErrorMessage = "Bu departmanda amirliğiniz yok.";
                            return View("Error");
                        }
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Amir bulnamadı.";
                        return View("Error");
                    }
                }
                else
                {
                    ViewBag.ErrorMessage = "Hasta bulunamadı.";
                    return View("Error");
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Kart No bulunamadı.";
                return View("Error");
            }
        }





        public JsonResult HastaBul(string sicilno)
        {
            var hasta = db.vizitealanlartabloes.Where(x => x.sicilno.ToString() == sicilno).FirstOrDefault();
            return new JsonResult { Data = hasta, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }
        [HttpPost]
        public JsonResult HastaEkle(string kartno)
        {
            var scannerr = db.scanners.FirstOrDefault(s => s.kartno == kartno);
            if (scannerr != null)
            {
                var hasta = db.temelcalisantabloes.FirstOrDefault(a => a.sicilno == scannerr.sicilno);
                if (hasta != null)
                {

                    try
                    {
                        //VİZİTE ALANLAR TALBODA KAYITLAR ID ÜZERİNE YAPILIYOR SİCİLNOYA KAYDEDİLİRSE ZAMAN HEP EN SON KAYDI ALIR
                        db.Database.ExecuteSqlCommand("INSERT INTO dbo.vizitealanlartablo (sicilno, isim, soyisim, yakakodu, sevk_tarihi, sevk_saati) " +
                       "VALUES (@sicilno, @isim, @soyisim, @yakakodu, @sevk_tarihi,@sevk_saati)",
                       new SqlParameter("@sicilno", hasta.sicilno),
                       new SqlParameter("@isim", hasta.isim),
                       new SqlParameter("@soyisim", hasta.soyisim),
                       new SqlParameter("@yakakodu", hasta.yakakodu),
                       new SqlParameter("@sevk_tarihi", (object)DateTime.Now ?? DBNull.Value),
                       new SqlParameter("@sevk_saati", (object)DateTime.Now ?? DBNull.Value));
                      


                        return Json(new { success = true, message = "Başarı ile Kaydedildi !!" });
                    }
                    catch (Exception ex)
                    {
                        return Json(new { success = false, message = "Kayıt sırasında bir hata oluştu: " + ex.Message });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Sicil no bulunamadı." });
                }
            }
            else
            {
                return Json(new { success = false, message = "Kart No bulunamadı." });
            }
        }

        [HttpPost]
        public ActionResult HastaSil(int id, int? yakakodu)
        {
            try
            {
                var user = db.vizitealanlartabloes.FirstOrDefault(a => a.id == id);
                if (user == null)
                {
                    TempData["Error"] = "Kullanıcı bulunamadı.";
                    return RedirectToAction("Index");
                }

                db.Database.ExecuteSqlCommand("DELETE FROM dbo.vizitealanlartablo WHERE id = @id",
                    new SqlParameter("@id", user.id));

                TempData["OK"] = "Kullanıcı başarıyla silindi.";

                //aynı yakakodunda kalmaya devam etsin yoksa silme işlemini yapınca tüm listeyi getiriyor
                if (yakakodu.HasValue)
                {
                    return RedirectToAction("Index", new { yakakodu = yakakodu });
                }
                else
                {
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Silme işlemi sırasında hata oluştu: " + ex.Message;
            }

            return RedirectToAction("Index", new { yakakodu });
        }



        public JsonResult HastaDetay(string sicilno)
        {
            var detay = db.temelcalisantabloes.FirstOrDefault(a => a.sicilno == sicilno);
            if (detay != null)
            {
                return Json(new
                {
                    success = true,
                    data = new
                    {
                        sicilno = detay.sicilno,
                        isim = detay.isim,
                        soyisim = detay.soyisim,
                        yakakodu = detay.yakakodu,
                        tel = detay.tel,
                        e_mail = detay.e_mail,
                        departman = detay.departman,
                        amirleri = detay.amirleri
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = "Hasta bulunamadı." }, JsonRequestBehavior.AllowGet);
        }




    }

    
}
