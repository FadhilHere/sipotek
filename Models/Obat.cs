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
    }
}