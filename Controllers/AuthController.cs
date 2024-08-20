using System.Security.Claims;
using Kavenegar;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


public class AuthController : Controller
{

    private readonly Context db;
    private readonly string salt = "S@lt?";

    private readonly IWebHostEnvironment _env;

    public AuthController(Context _db, IWebHostEnvironment env)
    {
        db = _db;
        _env = env;
    }

    [HttpGet]
    public IActionResult login()
    {
        if (User.Identity.IsAuthenticated)
        {
            return RedirectToAction("index", "Message");
        }
        if (db.Users_tbl.Count() == 0)
        {
            db.smsToken_tbl
                .Add(new smsToken
                {
                    Token = "3871353043697339486A70384F544A4A574C74612B51432F4C7A4B305076645457396F5267456F7A5A34383D"
                });
            db.SaveChanges();

            db.Users_tbl.Add(new Users
            {
                Username = "admin",
                Password = BCrypt.Net.BCrypt.HashPassword("admin" + salt + "admin"),
                FirstName = "admin",
                LastName = "admin",
                Phone = "admin",
                Addres = "admin",
                NatinalCode = "admin",
                PerconalCode = "admin",
                Profile = "admin",
                CreateDateTime = DateTime.UtcNow,
                isActive = true
            });
            db.SaveChanges();
        }
        return View();
    }

    [HttpPost]
    public IActionResult login(string Username, string Password)
    {
        Users check =
            db.Users_tbl.FirstOrDefault(x => x.Username == Username.ToLower());
        if (check == null)
        {
            ViewBag.Error = ("1اطلاعات وارد شده درست نیست");
        }
        else if (
            !BCrypt
                .Net
                .BCrypt
                .Verify(Password + salt + Username.ToLower(), check.Password)
        )
        {
            ViewBag.Error = ("اطلاعات وارد شده درست نیست");
        }
        else
        {
            ClaimsIdentity Identity =
                new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name,
                            check.FirstName + " " + check.LastName),
                        new Claim(ClaimTypes.NameIdentifier,
                            check.Id.ToString()),
                        new Claim("Profile", check.Profile),
                        new Claim("Username", check.Username)
                    },
                    CookieAuthenticationDefaults.AuthenticationScheme);

            var princpal = new ClaimsPrincipal(Identity);

            var properties =
                new AuthenticationProperties
                {
                    ExpiresUtc = DateTime.UtcNow.AddMonths(1),
                    IsPersistent = true
                };

            HttpContext.SignInAsync(princpal, properties);

            if (Username.ToLower() == "admin")
            {
                return RedirectToAction("ReportSeen", "home", new { area = "admin" });
            }

            return RedirectToAction("index", "Message");
        }
        return View();
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity.IsAuthenticated)
        {
            return RedirectToAction("index", "Message");
        }
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> RegisterAsync(string Username, string Password, string FirstName, string LastName, string Phone, string Addres, string NatinalCode, string PerconalCode, IFormFile Profile)
    {
        string PathSave;

        Users check =
            db
                .Users_tbl
                .FirstOrDefault(x =>
                    x.Username == Username ||
                    x.NatinalCode == NatinalCode ||
                    x.Phone == Phone);
        if (check != null)
        {
            if (check.Username == Username.ToLower())
            {
                ViewBag.Error = ("کاربر وارد شده تکراری است");
                return View();
            }
            else if (check.NatinalCode == NatinalCode)
            {
                ViewBag.Error = ("کد ملی وارد شده تکراری است");
                return View();
            }
            else if (check.Phone == Phone)
            {
                ViewBag.Error = ("شماره تلفن وارد شده  تکراری است");
                return View();
            }
            else
            {
                ViewBag.Error = "مشکلی پیش امده است ، با پشتیبانی تماس بگیرید";
                return View();
            }
        }
        else
        {
            string FileExtension = Path.GetExtension(Profile.FileName);
            var NewFileName =
                String.Concat(Guid.NewGuid().ToString(), FileExtension);
            var path = $"{_env.WebRootPath}\\uploads\\{NewFileName}";
            PathSave = $"\\uploads\\{NewFileName}";
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await Profile.CopyToAsync(stream);
            }

            var NewUser =
                new Users
                {
                    Username = Username.ToLower(),
                    Password =
                        BCrypt
                            .Net
                            .BCrypt
                            .HashPassword(Password +
                            salt +
                            Username.ToLower()),
                    FirstName = FirstName,
                    LastName = LastName,
                    Phone = Phone,
                    Addres = Addres,
                    NatinalCode = NatinalCode,
                    PerconalCode = PerconalCode,
                    Profile = PathSave,
                    CreateDateTime = DateTime.UtcNow,
                    isActive = true
                };
            db.Users_tbl.Add(NewUser);
            db.SaveChanges();


            ViewBag.Result = "ثبت نام با موفقیت انجام شد ";
            return View("login");
        }
    }

    [HttpGet]
    public IActionResult Forget()
    {
        if (User.Identity.IsAuthenticated)
        {
            return RedirectToAction("index", "Message");
        }
        return View();
    }

    [HttpPost]
    public IActionResult Forget(string Username, string NatinalCode)
    {
        Users check =
            db
                .Users_tbl
                .FirstOrDefault(x =>
                    x.Username == Username.ToLower() &&
                    x.NatinalCode == NatinalCode);
        if (check == null)
        {
            ViewBag.Error = "اطلاعات وارد شده نادرست میباشد";
            return View("Forget");
        }

        // sms check
        smsUser request = db.sms_tbl.FirstOrDefault(x => x.UserId == check.Id);

        if (request != null)
        {
            if (request.IsValid = false)
            {
                //return Ok("you Must Wait about 10 min");
                ViewBag.Error =
                    "تعداد تلاش شما برای ورود بیشتر از 5 میباشد . شما مجاز به ورود تا 10 دقیقه دیگر نیستید ، مجددا تلاش کنید";
                return View("Forget");
            }
            else if (DateTime.UtcNow.AddMinutes(-10) < request.CreateDateTime)
            {
                //return Ok("you Must Wait about 10 min");
                ViewBag.Error =
                    "شما باید 10 دقیقه برای دریافت کد جدید صبر کنید";
                return View("forget");
            }
            else
            {
                db.sms_tbl.Remove(request);
            }
        }
        Random random = new Random();
        smsUser newSms =
            new smsUser
            {
                TryCount = 0,
                SmsCode = random.Next(100000, 999999).ToString(),
                UserId = (int)check.Id,
                IsValid = true,
                CreateDateTime = DateTime.UtcNow
            };
        db.sms_tbl.Add(newSms);
        db.SaveChanges();

        SmsCode(newSms.SmsCode, check.Phone);
        ViewBag.smsPhone =
            check.Phone.Substring(check.Phone.Count() - 4) + "*****09";
        ViewBag.userId = check.Id;
        return View("Verify");
    }

    [HttpPost]
    public IActionResult Verify(int userid, string otp)
    {
        Users check = db.Users_tbl.Find(userid);
        if (check == null)
        {
            ViewBag.Error = "کاربر یافت نشد (محاله به این ارور بخوری)";
            return View("Forget");
        }

        //sms Check
        smsUser smsCheck = db.sms_tbl.FirstOrDefault(x => x.UserId == check.Id);
        if (smsCheck == null)
        {
            ViewBag.Error = "خطایی رخ داده است ، دوباره تلاش کنید";
            return View("Forget");
        }
        else if (DateTime.UtcNow.AddMinutes(-10) > smsCheck.CreateDateTime)
        {
            //Time Passed
            db.sms_tbl.Remove(smsCheck);
            db.SaveChanges();
            ViewBag.Error = "کد شما منقضی شده ، مجددا تلاش کنید";
            return View("Forget");
        }
        else if (smsCheck.IsValid == true)
        {
            if (otp == smsCheck.SmsCode)
            {
                //check.Password = BCrypt.Net.BCrypt.HashPassword(NewPassword + salt + Username.ToLower());
                //db.Users_tbl.Update(check);
                db.sms_tbl.Remove(smsCheck);
                db.SaveChanges();

                ClaimsIdentity Identity =
                    new ClaimsIdentity(new[]
                        {
                            new Claim(ClaimTypes.Name,
                                check.FirstName + " " + check.LastName),
                            new Claim(ClaimTypes.NameIdentifier,
                                check.Id.ToString()),
                            new Claim("Profile", check.Profile),
                            new Claim("Username", check.Username)
                        },
                        CookieAuthenticationDefaults.AuthenticationScheme);

                var princpal = new ClaimsPrincipal(Identity);

                var properties =
                    new AuthenticationProperties
                    {
                        ExpiresUtc = DateTime.UtcNow.AddMonths(1),
                        IsPersistent = true
                    };

                HttpContext.SignInAsync(princpal, properties);

                return RedirectToAction("resetPassword");
            }
            else
            {
                if (
                    smsCheck.TryCount > 3 // start from 0 -> 1,2,3,4 -> when 4 still can try 5 ! done
                )
                    smsCheck.IsValid = false;
                else
                    ++smsCheck.TryCount;
                db.sms_tbl.Update(smsCheck);
                db.SaveChanges();
                ViewBag.Error = "کد نامعتبر است ، دوباره تلاش کنید";
                ViewBag.smsPhone =
                    check.Phone.Substring(check.Phone.Count() - 4) + "*****09";
                ViewBag.userId = check.Id;
                return View("Verify");
            }
        }
        else
        {
            ViewBag.Error =
                "تعداد تلاش شما برای ورود بیشتر از 5 میباشد . شما مجاز به ورود تا 10 دقیقه دیگر نیستید ، مجددا تلاش کنید";
            return View("Forget");
        }
    }

    [HttpGet]
    [Authorize]
    public IActionResult resetPassword()
    {
        return View();
    }

    [HttpPost]
    [Authorize]
    public IActionResult resetPassword(string password)
    {
        var check =
            db
                .Users_tbl
                .Find(Convert
                    .ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier)));
        check.Password =
            BCrypt
                .Net
                .BCrypt
                .HashPassword(password + salt + check.Username.ToLower());
        db.SaveChanges();
        return RedirectToAction("index", "Message");
    }

    private void SmsCode(string Code, string Phone)
    {
        // real sms
        KavenegarApi SmsApi = new KavenegarApi(db.smsToken_tbl.Find(1).Token);
        SmsApi.VerifyLookup(Phone, Code, "demo");

        // price less
        //watch from sql server
    }

    public IActionResult NotAuthorized()
    {
        return View();
    }

    public IActionResult logout()
    {
        HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("login", "Auth");
    }
}
