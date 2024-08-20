using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


public class UserController : Controller
{
    private readonly Context db;
    private readonly IWebHostEnvironment _env;


    public UserController(Context _db, IWebHostEnvironment env)
    {
        db = _db;
        _env = env;

    }

    [HttpGet]
    public IActionResult ProfileUser()
    {
        var UserId = Convert.ToInt32(User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value);


        Users Check = db.Users_tbl.Find(UserId);
        ViewBag.data = Check;

        var sentCheck = db.Users_tbl
            .Where(x=> x.Id == UserId)
            .Include(x=> x.sentMessage)
                .ThenInclude(x=>x.recivers)
                    .ThenInclude(x=>x.Reciver)
            .Include(x=> x.sentMessage)
                .ThenInclude(x=>x.SenderUser)
            .SelectMany(x=>x.sentMessage)
            .OrderByDescending(x=> x.Id)
            .ToList();

        ViewBag.dataSentCount = sentCheck.Count();
        sentCheck = sentCheck.Take(10).ToList();
            
        
        ViewBag.dataSent = sentCheck;

        var reciveCheck = db.Users_tbl
            .Where(x=> x.Id == UserId)
            .Include(x=> x.getMessage)
                .ThenInclude(x=> x.Message)
                    .ThenInclude(x=>x.SenderUser)
            .Include(x=>x.getMessage)
                .ThenInclude(x=> x.Message)
                    .ThenInclude(x=>x.recivers)
                        .ThenInclude(x=>x.Reciver)
            .SelectMany(x=>x.getMessage.Select(z=>z.Message))
            .OrderByDescending(x=>x.Id)
            .ToList();

        ViewBag.dataReciveCount = reciveCheck.Count();
        reciveCheck = reciveCheck.Take(10).ToList();

        ViewBag.dataRecive = reciveCheck;

        // var UserLogCheck = Log.AllUserLog(db, User);
        // ViewBag.dataUserLog = UserLogCheck;
        return View();
    }


    [HttpGet]
    public IActionResult UserSetting()
    {
        var Id = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        Users Check = db.Users_tbl.Find(Id);
        ViewBag.User = Check;
        return View("UserSetting");

    }

    [HttpPost]
    public async Task<IActionResult> UserSetting(string Username, string Password, string FirstName, string LastName, string Phone, string Addres, string NatinalCode, string PerconalCode, IFormFile Profile)
    {
        Users check = db.Users_tbl.Find(Convert.ToInt32(User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value));
        string PathSave = check.Profile;

        if (Profile != null)
        {
            string FileExtension = Path.GetExtension(Profile.FileName);
            var NewFileName = System.String.Concat(Guid.NewGuid().ToString(), FileExtension);
            var path = $"{_env.WebRootPath}\\uploads\\{NewFileName}";
            PathSave = $"\\uploads\\{NewFileName}";
            using (var stream = new FileStream(path, FileMode.Create))
            {
                await Profile.CopyToAsync(stream);
            }
        }

        check.FirstName = FirstName;
        check.LastName = LastName;
        check.Addres = Addres;
        check.PerconalCode = PerconalCode;
        check.Profile = PathSave;

        db.Users_tbl.Update(check);
        db.SaveChanges();

        HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        ClaimsIdentity Identity = new ClaimsIdentity(new[]
            {

                new Claim(ClaimTypes.Name,check.FirstName+" "+check.LastName),
                new Claim(ClaimTypes.NameIdentifier,check.Id.ToString()),
                new Claim("Profile",check.Profile),
                new Claim("Username", check.Username)

            }, CookieAuthenticationDefaults.AuthenticationScheme);


        var princpal = new ClaimsPrincipal(Identity);

        var properties = new AuthenticationProperties
        {
            ExpiresUtc = DateTime.UtcNow.AddMonths(1),
            IsPersistent = true
        };

        HttpContext.SignInAsync(princpal, properties);

        ViewBag.Result = "به روزرسانی اطلاعات با موفقیت انجام شد";

        return RedirectToAction("index", "home");
    }
}
//< option data - avatar = \"{item.Profile}\" value = \"{item.FirstName} {item.LastName}\" > {item.FirstName} {item.LastName} </ option >