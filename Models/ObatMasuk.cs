using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SIPOTEK.Models
{
    public class ObatMasuk
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ObatId { get; set; }

        public int JumlahMasuk { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalHarga { get; set; }

        public DateTime TglKadaluarsaM { get; set; }

        public DateTime TglMasuk { get; set; } = DateTime.Now;

        [MaxLength(100)]
        public string? Supplier { get; set; }

        // Navigation property
        public Obat Obat { get; set; } = null!;
    }
}