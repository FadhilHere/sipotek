using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIPOTEK.Models
{
    public class ObatKeluar
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ObatId { get; set; }

        public int JumlahKeluar { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalHarga { get; set; }

        public DateTime TglKeluar { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string? Pelanggan { get; set; }

        [MaxLength(50)]
        public string? NoTransaksi { get; set; }

        // Navigation property
        public Obat Obat { get; set; } = null!;
    }
}