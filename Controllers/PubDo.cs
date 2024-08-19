using System.Globalization;
using HtmlAgilityPack;

public class PubDo
{
    static public string texter(string htText)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(htText);
        return htmlDoc.DocumentNode.InnerText;
    }
    static public (string, string) persianDate(DateTime date)
    {
        PersianCalendar pc = new PersianCalendar();
        var LocalData = TimeZoneInfo.ConvertTimeFromUtc(date, TimeZoneInfo.Local);

        var year = pc.GetYear(LocalData).ToString().Length != 1 ? pc.GetYear(LocalData).ToString() : "0" + pc.GetYear(LocalData).ToString();
        var Month = pc.GetMonth(LocalData).ToString().Length != 1 ? pc.GetMonth(LocalData).ToString() : "0" + pc.GetMonth(LocalData).ToString();
        var day = pc.GetDayOfMonth(LocalData).ToString().Length != 1 ? pc.GetDayOfMonth(LocalData).ToString() : "0" + pc.GetDayOfMonth(LocalData).ToString();
        var hour = pc.GetHour(LocalData).ToString().Length != 1 ? pc.GetHour(LocalData).ToString() : "0" + pc.GetHour(LocalData).ToString();
        var min = pc.GetMinute(LocalData).ToString().Length != 1 ? pc.GetMinute(LocalData).ToString() : "0" + pc.GetMinute(LocalData).ToString();
        var second = pc.GetSecond(LocalData).ToString().Length != 1 ? pc.GetSecond(LocalData).ToString() : "0" + pc.GetSecond(LocalData).ToString();
        return new($"{year}/{Month}/{day}", $"{hour}:{min}:{second}");
    }

    static public string YM(DateTime date){

        PersianCalendar pc = new PersianCalendar();
        var LocalData = TimeZoneInfo.ConvertTimeFromUtc(date, TimeZoneInfo.Local);
        
        var year = pc.GetYear(LocalData).ToString().Length != 1 ? pc.GetYear(LocalData).ToString() : "0" + pc.GetYear(LocalData).ToString();
        var Month = pc.GetMonth(LocalData).ToString().Length != 1 ? pc.GetMonth(LocalData).ToString() : "0" + pc.GetMonth(LocalData).ToString();
        return $"{year}{Month}";
        
    }
}