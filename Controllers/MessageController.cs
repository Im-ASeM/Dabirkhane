using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[Authorize]
public class MessageController : Controller
{
    private readonly Context db;
    private readonly IWebHostEnvironment _env;
    private readonly string salt = "S@lt?";

    public MessageController(Context _db, IWebHostEnvironment env)
    {
        db = _db;
        _env = env;
    }

    public IActionResult Index(int Id = 1)
    {
        var UserId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

        // var query = db.Messages_tbl
        // .Include(x => x.files)
        // .Include(x => x.recivers)
        //     .ThenInclude(x => x.Reciver)
        // .Include(x => x.SenderUser)
        // .Include(x => x.replies)
        // .Where(x => x.SenderUserId == UserId || x.recivers.Any(r => r.ReciverId == UserId));  //بهینه سازی ضعیف ، مرور زمان بهینه نیست با دیتا بیس بزرگ

        var query = db.Users_tbl.Where(x=>x.Id == UserId) // بهینه سازی فوق العاده ، سرعت بیشتر با دیستابیس های بزرگتر
            .Include(x=> x.sentMessage)
                .ThenInclude(x=>x.recivers)
                    .ThenInclude(x=> x.Reciver)
            .Include(x=> x.sentMessage)
                .ThenInclude(x=>x.SenderUser)
            .SelectMany(x=>x.sentMessage)
            .ToList();
            
        query.AddRange(
                db.Users_tbl.Where(x=>x.Id == UserId)
                .Include(x=>x.getMessage)
                    .ThenInclude(x=> x.Message)
                        .ThenInclude(x=> x.SenderUser)
                .Include(x=>x.getMessage)
                    .ThenInclude(x=> x.Message)
                        .ThenInclude(x=>x.recivers)
                            .ThenInclude(x=>x.Reciver)
                .SelectMany(x=>x.getMessage.Select(z=>z.Message))
                .ToList()
            );
            query.OrderByDescending(x=>x.Id);

        var DataCount = query.Count();

        var MessagesData = query.AsQueryable()
        .Skip((Id - 1) * 10)
        .Take(10)
        .ToList();

        ViewBag.Messages = MessagesData;
        ViewBag.DataPageCount = (int)Math.Ceiling((double)DataCount / 10);
        ViewBag.DataCount = DataCount;
        ViewBag.DataPage = Id;
        return View();
    }
    [HttpGet]
    public IActionResult NewMsg()
    {
        List<(string, int, string, string)> Result = new List<(string, int, string, string)>();

        var UserId = Convert.ToInt32(User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value);
        foreach (var item in db.Users_tbl.Where(x => x.Username != "admin" || x.Id != UserId)
        .ToList())
        {
            Result.Add(new($"../../../{item.Profile}", (int)item.Id, $"{item.FirstName} {item.LastName}", item.Username));
        }

        ViewBag.Contacts = Result;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> NewMsg(string Subject, string BodyText, List<int> ReciversId, List<int> CCsId, List<IFormFile>? Files)
    {
        List<(string, int, string, string)> Result = new List<(string, int, string, string)>();
        var SenderUserId = Convert.ToInt32(User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier).Value);

        foreach (var item in db.Users_tbl.Where(x => x.Username != "admin" || x.Id != SenderUserId)
        .ToList())
        {
            Result.Add(new($"../../../{item.Profile}", (int)item.Id, $"{item.FirstName} {item.LastName}", item.Username));
        }

        ViewBag.Contacts = Result;
        if (ReciversId == null)
        {
            ViewBag.NewError = "خطا ، حداقل یک کاربر باید در لیست دریافت کنندگان باشد ... (فایل های پیوستی را مجددا انتخاب نمایید)";
            ViewBag.ReciversId = ReciversId;
            ViewBag.CCsId = CCsId;
            ViewBag.Subject = Subject;
            ViewBag.BodyText = BodyText;

            return View();
        }
        else if (CCsId != null)
        {
            if (ReciversId.Any(x => CCsId.Contains(x)))
            {
                ViewBag.NewError = "خطا ، کاربر نمیتواند همزمان در لیست گیرنده و رونوشت باشد . لطفا مجددا تلاش کنید... (فایل های پیوستی را مجددا انتخاب نمایید)";
                ViewBag.ReciversId = ReciversId;
                ViewBag.CCsId = CCsId;
                ViewBag.Subject = Subject;
                ViewBag.BodyText = BodyText;

                return View();
            }
        }
        var random = new Random();
        var serialNumberCode = random.Next(100000, 999999);
        while (db.Messages_tbl.Any(x => x.SerialNumberCode == serialNumberCode))
        {
            serialNumberCode = random.Next(100000, 999999);
        }

        var form = PubDo.YM(DateTime.UtcNow);

        var serialNumber = form + serialNumberCode.ToString();

        Messages newMessage = new Messages
        {
            SerialNumber = serialNumber,
            SerialNumberCode = serialNumberCode,
            SenderUserId = SenderUserId,
            Subject = Subject,
            BodyText = BodyText,
            CreateDateTime = DateTime.UtcNow
        };

        db.Messages_tbl.Add(newMessage);
        db.SaveChanges();


        int messageId = Convert.ToInt32(newMessage.Id);

        foreach (var item in ReciversId)
        {
            db.Recivers_tbl.Add(new Recivers
            {
                ReciverId = item,
                MessageId = messageId,
                Type = "to",
                CreateDateTime = DateTime.UtcNow,
                isRead = false,
                Status = 1
            });
            db.SaveChanges();
        }
        if (CCsId != null)
        {
            foreach (var item in CCsId)
            {
                db.Recivers_tbl.Add(new Recivers
                {
                    ReciverId = item,
                    MessageId = messageId,
                    Type = "cc",
                    CreateDateTime = DateTime.UtcNow,
                    isRead = false,
                    Status = 1
                });
                db.SaveChanges();
            }
        }

        if (Files != null)
        {
            foreach (var item in Files)
            {

                string FileExtension = Path.GetExtension(item.FileName);
                var NewFileName = String.Concat(Guid.NewGuid().ToString(), FileExtension);
                var path = $"{_env.WebRootPath}\\uploads\\EmailFiles\\ID{messageId}-{NewFileName}";
                string PathSave = $"\\uploads\\EmailFiles\\ID{messageId}-{NewFileName}";
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await item.CopyToAsync(stream);
                }

                db.Files_tbl.Add(new Files
                {
                    FileName = item.FileName,
                    CreatorUserId = SenderUserId,
                    MessageId = messageId,
                    FilePath = PathSave,
                    FileType = FileExtension,
                    CreateDateTime = DateTime.UtcNow
                });
                db.SaveChanges();

            }
        }
        Log.NewMsgLog(db, SenderUserId, messageId, 1, true);
        return View();
    }
    public IActionResult test()
    {
        db.Users_tbl.Add(new Users
        {
            Username = "test1",
            Password = BCrypt.Net.BCrypt.HashPassword("test1" + salt + "test1"),
            FirstName = "test1",
            LastName = "test1",
            Phone = "test1",
            Addres = "test1",
            NatinalCode = "test1",
            PerconalCode = "test1",
            Profile = "test1",
            CreateDateTime = DateTime.UtcNow,
            isActive = true
        });
        db.SaveChanges();
        db.Users_tbl.Add(new Users
        {
            Username = "test2",
            Password = BCrypt.Net.BCrypt.HashPassword("test2" + salt + "test2"),
            FirstName = "test2",
            LastName = "test2",
            Phone = "test2",
            Addres = "test2",
            NatinalCode = "test2",
            PerconalCode = "test2",
            Profile = "test2",
            CreateDateTime = DateTime.UtcNow,
            isActive = true
        });
        db.SaveChanges();
        db.Users_tbl.Add(new Users
        {
            Username = "test3",
            Password = BCrypt.Net.BCrypt.HashPassword("test3" + salt + "test3"),
            FirstName = "test3",
            LastName = "test3",
            Phone = "test3",
            Addres = "test3",
            NatinalCode = "test3",
            PerconalCode = "test3",
            Profile = "test3",
            CreateDateTime = DateTime.UtcNow,
            isActive = true
        });
        db.SaveChanges();
        return Ok();
    }
}

