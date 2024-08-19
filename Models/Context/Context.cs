using Microsoft.EntityFrameworkCore;

public class Context : DbContext
{
    public DbSet<Users> Users_tbl { get; set; }
    public DbSet<Messages> Messages_tbl { get; set; }
    public DbSet<Reply> Reply_tbl { get; set; }
    public DbSet<Recivers> Recivers_tbl { get; set; }
    public DbSet<Files> Files_tbl { get; set; }
    public DbSet<smsToken> smsToken_tbl { get; set; }
    public DbSet<smsUser> sms_tbl { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("server=.\\SQL2019;database=DabirKhane;trusted_connection=true;MultipleActiveResultSets=True;TrustServerCertificate=True");
    }
}