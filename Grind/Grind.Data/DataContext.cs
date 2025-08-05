using Microsoft.EntityFrameworkCore;
using Grind.Core.Entities;
using Grind.Core.Enums;

namespace Grind.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        // DbSet עבור כל ישות ראשית
        public DbSet<Person> People { get; set; }
        public DbSet<Client> Clients { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<Address> Addresses { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<TrainerPayment> TrainerPayments { get; set; } // שם עקבי: TrainerPayments
        public DbSet<ClientClass> ClientClasses { get; set; }
        public DbSet<ClientPreferredTime> ClientPreferredTimes { get; set; }
        public DbSet<ClientPreferredClass> ClientPreferredClasses { get; set; }
        public DbSet<WaitingListEntry> WaitingListEntries { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // *** הגדרת אסטרטגיית מיפוי TPT (Table-Per-Type) ***
            modelBuilder.Entity<Client>().ToTable("Clients");
            modelBuilder.Entity<Trainer>().ToTable("Trainers");
            modelBuilder.Entity<Admin>().ToTable("Admins");
            modelBuilder.Entity<Person>().ToTable("People"); // טבלת הבסיס
            //------------------------------------------------------------
            //modelBuilder.Entity<Client>().HasBaseType<Person>();
            //modelBuilder.Entity<Trainer>().HasBaseType<Person>();
            //modelBuilder.Entity<Admin>().HasBaseType<Person>();
            //------------------------------------------------------------

            modelBuilder.Entity<Person>()
                .HasKey(p => p.Id);

            // ** אזהרות Decimal - הגדרת Precision ו-Scale **
            modelBuilder.Entity<Client>()
                .Property(c => c.BalanceDue)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<Client>()
                .Property(c => c.MonthlyPaymentAmount)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<Trainer>()
                .Property(t => t.HourlyRate)
                .HasColumnType("decimal(18, 2)");

            modelBuilder.Entity<TrainerPayment>()
                .Property(tp => tp.Amount)
                .HasColumnType("decimal(18, 2)");

            // ** הגדרת מפתחות ראשיים מורכבים לישויות עזר (Join Entities) **
            modelBuilder.Entity<ClientClass>()
                .HasKey(cc => new { cc.ClientId, cc.ClassId });

            modelBuilder.Entity<ClientPreferredTime>()
                .HasKey(cpt => new { cpt.ClientId, cpt.PreferredDay });

            modelBuilder.Entity<ClientPreferredClass>()
                .HasKey(cpc => new { cpc.ClientId, cpc.ClassCategory });

            modelBuilder.Entity<WaitingListEntry>()
                .HasKey(wle => new { wle.ClientId, wle.ClassId });

            // ** הגדרת קשרי גומלין ו-Foreign Keys עם טיפול ב-ON DELETE **

            // Person - Address (One-to-One)
            modelBuilder.Entity<Person>()
                .HasOne(p => p.Address)
                .WithOne()
                .HasForeignKey<Person>(p => p.AddressId)
                .IsRequired(false); // אם AddressId יכול להיות NULL

            // Client - Payment (One-to-Many)
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Client)
                .WithMany(c => c.Payments)
                .HasForeignKey(p => p.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Client - ClientPreferredTime (One-to-Many)
            modelBuilder.Entity<ClientPreferredTime>()
                .HasOne(cpt => cpt.Client)
                .WithMany(c => c.ClientPreferredTimes) // **תיקון כאן: שימוש בשם הקולקציה הנכון**
                .HasForeignKey(cpt => cpt.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Client - ClientPreferredClass (One-to-Many)
            modelBuilder.Entity<ClientPreferredClass>()
                .HasOne(cpc => cpc.Client)
                .WithMany(c => c.ClientPreferredClasses) // **תיקון כאן: שימוש בשם הקולקציה הנכון**
                .HasForeignKey(cpc => cpc.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Client - WaitingListEntry (One-to-Many)
            modelBuilder.Entity<WaitingListEntry>()
                .HasOne(wle => wle.Client)
                .WithMany(c => c.WaitingListEntries)
                .HasForeignKey(wle => wle.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Class - WaitingListEntry (One-to-Many)
            modelBuilder.Entity<WaitingListEntry>()
                .HasOne(wle => wle.Class)
                .WithMany(cl => cl.WaitingListEntries)
                .HasForeignKey(wle => wle.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            // Class - Trainer (One-to-Many)
            modelBuilder.Entity<Class>()
                .HasOne(c => c.Trainer)
                .WithMany(t => t.Classes)
                .HasForeignKey(c => c.TrainerId)
                .OnDelete(DeleteBehavior.Cascade);

            // ClientClass - Client (Many-to-Many Through Join Entity)
            modelBuilder.Entity<ClientClass>()
                .HasOne(cc => cc.Client)
                .WithMany(c => c.ClientClasses)
                .HasForeignKey(cc => cc.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // ClientClass - Class (Many-to-Many Through Join Entity)
            modelBuilder.Entity<ClientClass>()
                .HasOne(cc => cc.Class)
                .WithMany(cl => cl.ClientClasses)
                .HasForeignKey(cc => cc.ClassId)
                .OnDelete(DeleteBehavior.Restrict);

            // Trainer - TrainerPayment (One-to-Many)
            modelBuilder.Entity<TrainerPayment>()
                .HasOne(tp => tp.Trainer)
                .WithMany(t => t.TrainerPayments) // וודא שיש TrainerPayments בישות Trainer
                .HasForeignKey(tp => tp.TrainerId)
                .OnDelete(DeleteBehavior.Cascade);

            // אופציונלי: הגדרות עבור Enum Converters, אם יש צורך
            // לדוגמה, אם תרצה לשמור את ה-Enum כסטרינג במסד הנתונים במקום כאינטגר
            // modelBuilder.Entity<ClientPreferredTime>()
            //    .Property(cpt => cpt.PreferredDay)
            //    .HasConversion<string>();
        }
    }
}