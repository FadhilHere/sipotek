using System.ComponentModel.DataAnnotations;

namespace SIPOTEK.Models
{
	public class User
	{
		[Key]
		public int Id { get; set; }

		[Required]
		[MaxLength(50)]
		public string Username { get; set; } = string.Empty;

		[Required]
		[MaxLength(255)]
		public string Password { get; set; } = string.Empty;

		[Required]
		[MaxLength(100)]
		public string NamaLengkap { get; set; } = string.Empty;

		[Required]
		[MaxLength(20)]
		public string Role { get; set; } = "Apoteker";

		public bool IsActive { get; set; } = true;

		public DateTime CreatedAt { get; set; } = DateTime.Now;

		public DateTime? LastLogin { get; set; }

		public bool IsAdmin() => Role.Equals("Admin", StringComparison.OrdinalIgnoreCase);
		public bool IsApoteker() => Role.Equals("Apoteker", StringComparison.OrdinalIgnoreCase);
	}
	public static class UserRoles
	{
		public const string Admin = "Admin";
		public const string Apoteker = "Apoteker";

		public static List<string> GetAllRoles()
		{
			return new List<string> { Admin, Apoteker };
		}
	}
}