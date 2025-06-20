using Microsoft.EntityFrameworkCore;
using SIPOTEK.Models;

namespace SIPOTEK.Data
{
    public class SipotekDbContext : DbContext
    {
        public SipotekDbContext(DbContextOptions<SipotekDbContext> options) : base(options)
        {
        }

        public DbSet<Obat> Obats { get; set; }
        public DbSet<ObatMasuk> ObatMasuks { get; set; }
        public DbSet<ObatKeluar> ObatKeluars { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure foreign key relationships
            modelBuilder.Entity<ObatMasuk>()
                .HasOne(om => om.Obat)
                .WithMany()
                .HasForeignKey(om => om.ObatId);

            modelBuilder.Entity<ObatKeluar>()
                .HasOne(ok => ok.Obat)
                .WithMany()
                .HasForeignKey(ok => ok.ObatId);

            // Seed data
            modelBuilder.Entity<Obat>().HasData(
                new Obat
                {
                    Id = 1,
                    NamaObat = "Paracetamol",
                    JenisObat = "Analgesik",
                    BentukObat = "Tablet",
                    Harga = 6000,
                    Stok = 20,
                    TglKadaluarsa = new DateTime(2026, 5, 15)
                },
                new Obat
                {
                    Id = 2,
                    NamaObat = "Amoxicillin",
                    JenisObat = "Antibiotik",
                    BentukObat = "Kapsul",
                    Harga = 8000,
                    Stok = 30,
                    TglKadaluarsa = new DateTime(2026, 6, 16)
                },
                new Obat
                {
                    Id = 3,
                    NamaObat = "OBH",
                    JenisObat = "Ekspektoran",
                    BentukObat = "Sirup",
                    Harga = 18000,
                    Stok = 40,
                    TglKadaluarsa = new DateTime(2026, 6, 17)
                }
            );
        }
    }
}