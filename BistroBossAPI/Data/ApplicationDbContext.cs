using BistroBossAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BistroBossAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<Uzytkownik>
    {
        public virtual DbSet<Uzytkownik> Uzytkownicy { get; set; }
        public virtual DbSet<Zamowienie> Zamowienia { get; set; }
        public virtual DbSet<ZamowienieProdukt> ZamowieniaProdukty { get; set; }
        public virtual DbSet<Kategoria> Kategorie { get; set; }
        public virtual DbSet<Dostawa> Dostawy { get; set; }
        public virtual DbSet<Opinia> Opinie { get; set; }
        public virtual DbSet<Koszyk> Koszyki { get; set; }
        public virtual DbSet<KoszykProdukt> KoszykProdukty { get; set; }
        public virtual DbSet<Produkt> Produkty { get; set; }
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Opinia>()
                .HasOne(o => o.Zamowienie)
                .WithOne()
                .HasForeignKey<Opinia>(o => o.ZamowienieId);
            
            modelBuilder.Entity<Zamowienie>()
                .HasOne(z => z.Opinia)
                .WithOne()
                .HasForeignKey<Zamowienie>(z => z.OpiniaId);

            modelBuilder.Entity<Koszyk>()
                .HasOne(k => k.Uzytkownik)
                .WithOne(u => u.Koszyk)
                .HasForeignKey<Koszyk>(k => k.UzytkownikId);

            modelBuilder.Entity<Kategoria>()
                .HasIndex(k => k.Nazwa)
                .IsUnique();

            modelBuilder.Entity<Uzytkownik>()
                    .HasMany(u => u.Zamowienia)
                    .WithOne(z => z.Uzytkownik)
                    .HasForeignKey(z => z.UzytkownikId)
                    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Uzytkownik>()
                .HasMany(u => u.Opinie)
                .WithOne(o => o.Uzytkownik)
                .HasForeignKey(o => o.UzytkownikId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Zamowienie>()
                .HasOne(z => z.Opinia)
                .WithOne(o => o.Zamowienie)
                .HasForeignKey<Zamowienie>(z => z.OpiniaId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Koszyk>()
                .HasOne(k => k.Uzytkownik)
                .WithOne(u => u.Koszyk)
                .HasForeignKey<Koszyk>(k => k.UzytkownikId)
                .OnDelete(DeleteBehavior.Cascade);

        }

    }
    }

