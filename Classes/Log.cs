using System.Data.Common;

public static class Log
{
    public static void NewMsgLog(Context db , int userid , int status , bool isDone , int? MessageId = null){

        string description = "";
        var user = db.Users_tbl.Find(userid);
        var msg = db.Messages_tbl.Find(MessageId);

        switch(status){
            case 1:     // added Message
                description = isDone ? $"کاربر {user.FirstName} {user.LastName} ({user.Username}) در تاریخ {PubDo.persianDate(DateTime.UtcNow).Item1} {PubDo.persianDate(DateTime.UtcNow).Item2} پیام با شماره {msg.SerialNumber} را ارسال کرد." :  $"کاربر {user.FirstName} {user.LastName} ({user.Username}) در تاریخ {PubDo.persianDate(DateTime.UtcNow).Item1} {PubDo.persianDate(DateTime.UtcNow).Item2} قادر به ارسال پیام جدید نبود."; 
                break;
            case 2:     // read Message
                description = isDone ?  $"کاربر {user.FirstName} {user.LastName} ({user.Username}) در تاریخ {PubDo.persianDate(DateTime.UtcNow).Item1} {PubDo.persianDate(DateTime.UtcNow).Item2} پیام با شماره {msg.SerialNumber} را خواند." :  $"کاربر {user.FirstName} {user.LastName} ({user.Username}) در تاریخ {PubDo.persianDate(DateTime.UtcNow).Item1} {PubDo.persianDate(DateTime.UtcNow).Item2} پیام با شماره {msg.SerialNumber} را نتوانست بخواند";  
                break;
            case 3:     // read Reply
                description = isDone ?  $"کاربر {user.FirstName} {user.LastName} ({user.Username}) در تاریخ {PubDo.persianDate(DateTime.UtcNow).Item1} {PubDo.persianDate(DateTime.UtcNow).Item2} پاسخ پیام شماره {msg.SerialNumber} را خواند." :  $"کاربر {user.FirstName} {user.LastName} ({user.Username}) در تاریخ {PubDo.persianDate(DateTime.UtcNow).Item1} {PubDo.persianDate(DateTime.UtcNow).Item2} پاسخ پیام با شماره {msg.SerialNumber} را نتوانست بخواند";  
                break;
            case 4:     // add Reply
                description = isDone ?  $"کاربر {user.FirstName} {user.LastName} ({user.Username}) در تاریخ {PubDo.persianDate(DateTime.UtcNow).Item1} {PubDo.persianDate(DateTime.UtcNow).Item2} پاسخی برای پیام شماره {msg.SerialNumber} درج کرد." :  $"کاربر {user.FirstName} {user.LastName} ({user.Username}) در تاریخ {PubDo.persianDate(DateTime.UtcNow).Item1} {PubDo.persianDate(DateTime.UtcNow).Item2} نتوانست پاسخی برای پیام شماره {msg.SerialNumber} درج کند.";  
                break;
            case 5:     // empty
                break;
            case 6:     // empty
                break;
        }
        db.MsgLogs_tbl.Add(new MsgLog{
            CreateDateTime = DateTime.UtcNow,
            isDone = isDone,
            MsgId = msg != null ? msg.Id : null,
            Status = status,
            UserId = userid,
            Description = description
        });
        db.SaveChanges();
    }
}