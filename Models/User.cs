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
		[MaxLength(20)]
		public string Role { get; set; } = string.Empty;
	}
}