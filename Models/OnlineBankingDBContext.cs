using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace OnlineBankingAPI.Models
{
    public partial class OnlineBankingDBContext : DbContext
    {
        public OnlineBankingDBContext(DbContextOptions<OnlineBankingDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<Bank> Banks { get; set; }
        public virtual DbSet<Guideline> Guidelines { get; set; }
        public virtual DbSet<Otp> Otps { get; set; }
        public virtual DbSet<SavingInfo> SavingInfos { get; set; }
        public virtual DbSet<SavingPackage> SavingPackages { get; set; }
        public virtual DbSet<Transaction> Transactions { get; set; }
        public virtual DbSet<TransferCommand> TransferCommands { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(e => e.AccountNumber)
                    .HasName("PK__Accounts__BE2ACD6E1D1CD999");

                entity.Property(e => e.AccountNumber)
                    .HasMaxLength(12)
                    .IsUnicode(false);

                entity.Property(e => e.BankId).HasColumnName("BankID");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.ExpiredAt).HasColumnType("datetime");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Bank)
                    .WithMany(p => p.Accounts)
                    .HasForeignKey(d => d.BankId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Accounts__BankID__29572725");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Accounts)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Accounts__UserID__286302EC");
            });

            modelBuilder.Entity<Bank>(entity =>
            {
                entity.ToTable("Bank");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.BankAddress)
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.BankName)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);

                entity.Property(e => e.Logo)
                    .IsRequired()
                    .HasMaxLength(250)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Guideline>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Content)
                    .IsRequired()
                    .HasColumnType("text");

                entity.Property(e => e.Title)
                    .IsRequired()
                    .HasMaxLength(1)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Otp>(entity =>
            {
                entity.ToTable("OTPs");

                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.AccountNumber)
                    .IsRequired()
                    .HasMaxLength(12)
                    .IsUnicode(false);

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.ExpiredAt).HasColumnType("datetime");

                entity.Property(e => e.Otp1)
                    .IsRequired()
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .HasColumnName("OTP");

                entity.HasOne(d => d.AccountNumberNavigation)
                    .WithMany(p => p.Otps)
                    .HasForeignKey(d => d.AccountNumber)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__OTPs__AccountNum__32E0915F");
            });

            modelBuilder.Entity<SavingInfo>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.AccountNumber)
                    .IsRequired()
                    .HasMaxLength(12)
                    .IsUnicode(false);

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.PackageId).HasColumnName("PackageID");

                entity.Property(e => e.SavingId)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("SavingID");

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.HasOne(d => d.AccountNumberNavigation)
                    .WithMany(p => p.SavingInfos)
                    .HasForeignKey(d => d.AccountNumber)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__SavingInf__Accou__398D8EEE");

                entity.HasOne(d => d.Package)
                    .WithMany(p => p.SavingInfos)
                    .HasForeignKey(d => d.PackageId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__SavingInf__Packa__3A81B327");
            });

            modelBuilder.Entity<SavingPackage>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.PackageName)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.AccountNumber)
                    .IsRequired()
                    .HasMaxLength(12)
                    .IsUnicode(false);

                entity.Property(e => e.CommandId)
                    .IsRequired()
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("CommandID");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.HasOne(d => d.Command)
                    .WithMany(p => p.Transactions)
                    .HasForeignKey(d => d.CommandId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Transacti__Comma__300424B4");
            });

            modelBuilder.Entity<TransferCommand>(entity =>
            {
                entity.Property(e => e.Id)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("ID");

                entity.Property(e => e.Content)
                    .HasMaxLength(1)
                    .IsUnicode(false);

                entity.Property(e => e.FromAccountNumber)
                    .HasMaxLength(12)
                    .IsUnicode(false);

                entity.Property(e => e.ToAccountNumber)
                    .HasMaxLength(12)
                    .IsUnicode(false);

                entity.HasOne(d => d.FromAccountNumberNavigation)
                    .WithMany(p => p.TransferCommandFromAccountNumberNavigations)
                    .HasForeignKey(d => d.FromAccountNumber)
                    .HasConstraintName("FK__TransferC__FromA__2C3393D0");

                entity.HasOne(d => d.ToAccountNumberNavigation)
                    .WithMany(p => p.TransferCommandToAccountNumberNavigations)
                    .HasForeignKey(d => d.ToAccountNumber)
                    .HasConstraintName("FK__TransferC__ToAcc__2D27B809");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(e => e.Id).HasColumnName("ID");

                entity.Property(e => e.Avatar)
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Email)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .IsRequired()
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.Phone)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);

                entity.Property(e => e.UserName)
                    .IsRequired()
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
