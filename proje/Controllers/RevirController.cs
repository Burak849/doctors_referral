using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using proje.Models;



namespace proje.Controllers
{
    

    public class RevirController : Controller
    {
        private viziteEntities db = new viziteEntities();


        [HttpGet]
        [KartNoAuthorize]
        public ActionResult Index(string sortOrder, int? id,int? month, string sicilno, string isim, string soyisim, int? yakakodu, int page = 1, int pageSize = 10)
        {
            if (Session["KartNo"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            List<vizitealanlartablo> calisanlar = db.vizitealanlartabloes.ToList();

            if (calisanlar == null || !calisanlar.Any())
            {
                TempData["Error"] = "Çalışanlar listesi boş.";
            }




            if (id > 0)
            {
                calisanlar = calisanlar.Where(hasta => hasta.id == id).ToList();
            }
            // Filtreleme
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

            switch (sortOrder)
            {
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
                    calisanlar = calisanlar.OrderByDescending(hasta => hasta.sevk_saati).ToList();
                    break;
                default:
                    calisanlar = calisanlar.OrderBy(hasta => hasta.id).ToList();
                    break;
            }

            // Sayfalandırma
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
            }).ToList();

            // ViewBag ile sayfa bilgilerini gönder
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

        public JsonResult HastaBul(string sicilno)
        {
            var hasta = db.vizitealanlartabloes.Where(x => x.sicilno.ToString() == sicilno).FirstOrDefault();
            return new JsonResult { Data = hasta, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }



        public ActionResult DoktorKontrol(string kartno)
        {
            // KART NUMARASINI KONTROL EDIYOR
            var scanner = db.scanners.FirstOrDefault(s => s.kartno == kartno);
            if (scanner != null)
            {
                var doktor = db.doktortabloes.FirstOrDefault(a => a.doktor_sicilno == scanner.sicilno);
                if (doktor != null)
                {
                    Session["KartNo"] = kartno;
                    Session["Ad"] = doktor.doktor_ad; 
                    Session["Soyad"] = doktor.doktor_soyad; 
                    return RedirectToAction("Index", "Revir");
                }
                else
                {
                    
                    ViewBag.ErrorMessage = "Doktor bulunamadı.";
                    return View("Error");
                }
            }
            else
            {
                ViewBag.ErrorMessage = "Kart No bulunamadı.";
                return View("Error");
            }
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
                        tel = detay.tel,
                        e_mail = detay.e_mail,
                        departman = detay.departman,
                        amirleri = detay.amirleri
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            return Json(new { success = false, message = "Hasta bulunamadı." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetHastaDurum(int id)
        {
            
            var hasta = db.vizitealanlartabloes.FirstOrDefault(h => h.id == id);

            if (hasta != null)
            {
                return Json(new { success = true, durum = hasta.durum }, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return Json(new { success = false, message = "Hasta bulunamadı." }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public JsonResult DurumGuncelle(int id, string durum)
        {
            if (id <= 0)
            {
                return Json(new { success = false, message = "Geçersiz ID." });
            }

            if (durum == "1" || durum == "0")
            {
                int rowsAffected = db.Database.ExecuteSqlCommand("UPDATE dbo.vizitealanlartablo " +
                    "SET durum = @durum, sevk_saati = @sevk_saati " +
                    "WHERE id = @id", 
                    new SqlParameter("@durum", durum),
                    new SqlParameter("@id", id), 
                    new SqlParameter("@sevk_saati", (object)DateTime.Now ?? DBNull.Value));

                if (rowsAffected > 0)
                {
                    return Json(new { success = true, message = "Başarı ile güncellendi." });
                }
                else
                {
                    return Json(new { success = false, message = "Güncellenecek kayıt bulunamadı." });
                }
            }
            else
            {
                return Json(new { success = false, message = "Geçersiz durum." });
            }
        }



        public ActionResult Error()
        {
            return View();
        }

    }
}

