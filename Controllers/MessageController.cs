using System.Security.Claims;
using Dabirkhane.Migrations;
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

        var query = db.Users_tbl.Where(x => x.Id == UserId) // بهینه سازی فوق العاده ، سرعت بیشتر با دیستابیس های بزرگتر
            .Include(x => x.sentMessage)
                .ThenInclude(x => x.recivers)
                    .ThenInclude(x => x.Reciver)
            .Include(x => x.sentMessage)
                .ThenInclude(x => x.SenderUser)
            .SelectMany(x => x.sentMessage)
            .Where(x => (x.SenderUserId == UserId && x.Status4Sender == 1) || x.recivers.Any(z => z.ReciverId == UserId && z.Status == 1))
            .ToList();

        query.AddRange(
                db.Users_tbl.Where(x => x.Id == UserId)
                .Include(x => x.getMessage)
                    .ThenInclude(x => x.Message)
                        .ThenInclude(x => x.SenderUser)
                .Include(x => x.getMessage)
                    .ThenInclude(x => x.Message)
                        .ThenInclude(x => x.recivers)
                            .ThenInclude(x => x.Reciver)
                .SelectMany(x => x.getMessage.Select(z => z.Message))
                .Where(x => (x.SenderUserId == UserId && x.Status4Sender == 1) || x.recivers.Any(z => z.ReciverId == UserId && z.Status == 1))
                .ToList()
            );
        query = query.OrderByDescending(x => x.Id).ToList();

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
            Status4Sender = 1,
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
        Log.NewMsgLog(db, SenderUserId, 1, true, messageId);
        return View();
    }

    public IActionResult Recives(int Id = 1)
    {
        var UserId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var query = db.Users_tbl
            .Where(x => x.Id == UserId)
            .Include(x => x.getMessage)
                .ThenInclude(x => x.Message)
                    .ThenInclude(x => x.SenderUser)
            .Include(x => x.getMessage)
                .ThenInclude(x => x.Message)
                    .ThenInclude(x => x.recivers)
                        .ThenInclude(x => x.Reciver)
            .SelectMany(x => x.getMessage.Select(z => z.Message))
            .Where(x => (x.SenderUserId == UserId && x.Status4Sender == 1) || x.recivers.Any(z => z.ReciverId == UserId && z.Status == 1))
            .OrderByDescending(x => x.Id)
            .ToList();

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

    public IActionResult Sents(int Id = 1)
    {

        var UserId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var query = db.Users_tbl.Where(x => x.Id == UserId) // بهینه سازی فوق العاده ، سرعت بیشتر با دیستابیس های بزرگتر
           .Include(x => x.sentMessage)
               .ThenInclude(x => x.recivers)
                   .ThenInclude(x => x.Reciver)
           .Include(x => x.sentMessage)
               .ThenInclude(x => x.SenderUser)
           .SelectMany(x => x.sentMessage)
            .Where(x => (x.SenderUserId == UserId && x.Status4Sender == 1) || x.recivers.Any(z => z.ReciverId == UserId && z.Status == 1))
           .OrderByDescending(x => x.Id)
           .ToList();

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

    public IActionResult RecycleBin(int Id = 1)
    {
        var UserId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));

        var query = db.Users_tbl.Where(x => x.Id == UserId) // بهینه سازی فوق العاده ، سرعت بیشتر با دیستابیس های بزرگتر
            .Include(x => x.sentMessage)
                .ThenInclude(x => x.recivers)
                    .ThenInclude(x => x.Reciver)
            .Include(x => x.sentMessage)
                .ThenInclude(x => x.SenderUser)
            .SelectMany(x => x.sentMessage)

            .ToList();

        query.AddRange(
                db.Users_tbl.Where(x => x.Id == UserId)
                .Include(x => x.getMessage)
                    .ThenInclude(x => x.Message)
                        .ThenInclude(x => x.SenderUser)
                .Include(x => x.getMessage)
                    .ThenInclude(x => x.Message)
                        .ThenInclude(x => x.recivers)
                            .ThenInclude(x => x.Reciver)
                .SelectMany(x => x.getMessage.Select(z => z.Message))
                .ToList()
            );
        query = query
            .Where(x => x.Status4Sender == 2 || x.recivers.Any(z => z.ReciverId == UserId && z.Status == 2))
            .OrderByDescending(x => x.Id)
            .ToList();

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

    public IActionResult trash(int Id, string page)
    {
        string route;
        switch (page)
        {
            case "index":
                route = "index";
                break;
            case "recives":
                route = "recives";
                break;
            case "sents":
                route = "sents";
                break;
            case "trash":
                route = "RecycleBin";
                break;
            default:
                return RedirectToAction("error");
        }

        var UserId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var check = db.Messages_tbl
            .Where(x => x.Id == Id)
            .Include(x => x.recivers)
            .First();
        if (check.SenderUserId == UserId)
        {
            if (check.Status4Sender == 3)
            {
                return RedirectToAction(route, "Message");
            }
            check.Status4Sender = 2;
        }
        else
        {
            if (check.recivers.FirstOrDefault(x => x.ReciverId == UserId).Status == 3)
            {
                return RedirectToAction(route, "Message");
            }
            check.recivers.FirstOrDefault(x => x.ReciverId == UserId).Status = 2;
        }
        db.Messages_tbl.Update(check);
        db.SaveChanges();

        return RedirectToAction(route, "Message");

    }

    public IActionResult delete(int Id)
    {
        var UserId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var check = db.Messages_tbl
            .Where(x => x.Id == Id)
            .Include(x => x.recivers)
            .First();
        if (check.SenderUserId == UserId)
        {
            check.Status4Sender = 3;
        }
        else
        {
            check.recivers.FirstOrDefault(x => x.ReciverId == UserId).Status = 3;
        }
        db.Messages_tbl.Update(check);
        db.SaveChanges();
        return RedirectToAction("RecycleBin");
    }

    public IActionResult restore(int Id)
    {
        var UserId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var check = db.Messages_tbl
            .Where(x => x.Id == Id)
            .Include(x => x.recivers)
            .First();
        if (check.SenderUserId == UserId)
        {
            if (check.Status4Sender == 3)
            {
                return RedirectToAction("RecycleBin");
            }

            check.Status4Sender = 1;
        }
        else
        {
            if (check.recivers.FirstOrDefault(x => x.ReciverId == UserId).Status == 3)
            {
                return RedirectToAction("RecycleBin");
            }

            check.recivers.FirstOrDefault(x => x.ReciverId == UserId).Status = 1;
        }
        db.Messages_tbl.Update(check);
        db.SaveChanges();
        return RedirectToAction("RecycleBin");
    }

    public IActionResult ReturnEmail(int id)
    {
        var userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var check = db.Messages_tbl
            .Where(x => x.Id == id)
            .Include(x => x.recivers)
                .ThenInclude(x => x.Reciver)
            .Include(x => x.SenderUser)
            .Include(x => x.files)
            .FirstOrDefault();

        if (check == null)
        {
            return RedirectToAction("Index");
        }
        else if (!(check.SenderUserId == userId || check.recivers.Any(x => x.ReciverId == userId)))
        {
            return RedirectToAction("index");
            //log false see
        }
        else
        {
            var msg = new
            {
                check.Id,
                files = check.files.Where(x=>x.ReplyId == null).ToList(),
                check.SerialNumber,
                check.Subject,
                check.BodyText,
                CreateDateTime = PubDo.persianDate((DateTime)check.CreateDateTime),
                check.SenderUser,
                ReciversUser = check.recivers,

            };
            ViewBag.msg = msg;

            if (check.recivers.Any(x => x.ReciverId == userId && x.isRead == false))
            {

                check.recivers.First(x => x.ReciverId == userId).isRead = true;
                db.Messages_tbl.Update(check);
                Log.NewMsgLog(db, userId, 2, true, check.Id);
                db.SaveChanges();
            }
            return View();
        }
    }

    public IActionResult ReturnEvant(int id, int page = 1)
    {
        var userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var check = db.Messages_tbl
            .Where(x => x.Id == id)
            .Include(x => x.logs)
                .ThenInclude(x => x.User)
            .Include(x => x.recivers)
            .FirstOrDefault();

        if (check == null)
        {
            return RedirectToAction("Index");
        }
        else if (!(check.SenderUserId == userId || check.recivers.Any(x => x.ReciverId == userId)))
        {
            return RedirectToAction("index");
            //log false see
        }
        else
        {
            var msg = new
            {
                check.Id,
                check.SerialNumber,
                logCount = (int)Math.Ceiling((double)check.logs.Count() / 10),
                logs = check.logs
                   .OrderByDescending(x => x.Id)
                   .Skip((page - 1) * 10)
                   .Take(10)
                   .ToList(),
                logPage = page
            };
            ViewBag.msg = msg;
            return View();
        }
    }

    
    [HttpPost]
    public async Task<IActionResult> AddReply(int MsgId , string Subject , string BodyText , List<IFormFile>? files){
        int userid = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var msgCheck = db.Messages_tbl
            .Include(x=>x.recivers)
            .FirstOrDefault(x=>x.Id == MsgId && x.recivers.Any(z=>z.ReciverId == userid));
            
        var check = new Reply{
            BodyText = BodyText,
            CreateDateTime = DateTime.UtcNow,
            isRead = false,
            MessageId = MsgId,
            ReciversId = msgCheck==null ? null : msgCheck.recivers.First(x=>x.ReciverId == userid).Id,
            Subject = Subject
        };
        db.Reply_tbl.Add(check);
        db.SaveChanges();

        if (files != null)
        {
            foreach (var item in files)
            {

                string FileExtension = Path.GetExtension(item.FileName);
                var NewFileName = String.Concat(Guid.NewGuid().ToString(), FileExtension);
                var path = $"{_env.WebRootPath}\\uploads\\EmailFiles\\ID{MsgId}_{check.Id}-{NewFileName}";
                string PathSave = $"\\uploads\\EmailFiles\\ID{MsgId}_{check.Id}-{NewFileName}";
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await item.CopyToAsync(stream);
                }

                db.Files_tbl.Add(new Files
                {
                    FileName = item.FileName,
                    CreatorUserId = userid,
                    MessageId = MsgId,
                    FilePath = PathSave,
                    FileType = FileExtension,
                    ReplyId = check.Id,
                    CreateDateTime = DateTime.UtcNow
                });
                db.SaveChanges();
            }
        }
        Log.NewMsgLog(db,userid,4,true,MsgId);
        return RedirectToAction("index");
    }

    public IActionResult ReturnReply(int id , int page = 1){
        var userId = Convert.ToInt32(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var check = db.Messages_tbl
            .Where(x => x.Id == id)
            .Include(x=>x.SenderUser)
            .Include(x=>x.recivers)
            .Include(x=>x.replies)
                .ThenInclude(x=>x.files)
            .Include(x=>x.replies)
                .ThenInclude(x=>x.Recivers)
                    .ThenInclude(x=>x.Reciver)
            .FirstOrDefault();

        if (check.replies == null)
        {
            return RedirectToAction("Index");
        }
        else if (!(check.SenderUserId == userId || check.recivers.Any(x => x.ReciverId == userId)))
        {
            Log.NewMsgLog(db,userId,3,false,check.Id);
            return RedirectToAction("index");
            //log false see
        }
        else
        {
            var Result = new{
                check.Id,
                check.SerialNumber,
                repliesCount = (int)Math.Ceiling((double)check.replies.Count() / 10),
                repliesPage = page,
                replies = check.replies.Select(x=>new{
                    x.Id,
                    sender = x.ReciversId == null ? check.SenderUser : x.Recivers.Reciver,
                    x.CreateDateTime,
                    x.BodyText,
                    x.files,
                    x.isRead,
                    x.Subject
                }).Skip((page-1)*10).Take(10).ToList()
            };
            foreach (var item in Result.replies)
            {
                if(item.isRead== false && item.sender.Id != userId){
                    db.Reply_tbl.Find(item.Id).isRead = true;
                    db.SaveChanges();
                    Log.NewMsgLog(db,userId,3,true,check.Id);
                }
            }
            ViewBag.msg = Result;

            return View();
        }
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
        return Ok("done");
    }

    public IActionResult test2()
    {
        Log.NewMsgLog(db, 1, 1, false);
        return Ok("done");
    }
}

