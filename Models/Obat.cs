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

        public DateTime TglKadaluarsa { get; set; }

        // Properti baru untuk gambar
        [MaxLength(500)]
        public string? GambarUrl { get; set; }

        [MaxLength(100)]
        public string? GambarFileName { get; set; }

        [MaxLength(500)]
        public string? Deskripsi { get; set; }

        [MaxLength(100)]
        public string? Produsen { get; set; }
    }
}