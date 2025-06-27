using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIPOTEK.Models
{
    public class Obat
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string NamaObat { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? JenisObat { get; set; }

        [MaxLength(20)]
        public string? BentukObat { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Harga { get; set; }

        public int Stok { get; set; }

        // New property for minimum stock
        public int StokMinimum { get; set; } = 10;

        public DateTime TglKadaluarsa { get; set; }

        // Properti untuk gambar
        [MaxLength(500)]
        public string? GambarUrl { get; set; }

        [MaxLength(100)]
        public string? GambarFileName { get; set; }

        [MaxLength(500)]
        public string? Deskripsi { get; set; }

        [MaxLength(100)]
        public string? Produsen { get; set; }

        // Helper methods for stock status
        public StokStatus GetStokStatus()
        {
            if (Stok == 0) return StokStatus.Habis;
            if (Stok <= StokMinimum) return StokStatus.Kritis;
            if (Stok <= (StokMinimum * 1.5)) return StokStatus.Rendah;
            return StokStatus.Normal;
        }

        public bool IsStokRendah() => GetStokStatus() == StokStatus.Rendah || GetStokStatus() == StokStatus.Kritis;
    }

    public enum StokStatus
    {
        Normal,
        Rendah,
        Kritis,
        Habis
    }
}