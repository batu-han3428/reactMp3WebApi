using System.ComponentModel.DataAnnotations;

namespace TekrarApp.Model
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Surname { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        [MinLength(8)]
        public string Password { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefrestTokenEndDate { get; set; }
        public bool IsConfirmEmail { get; set; }
        public string? ConfirmEmailToken { get; set; }
        public ICollection<UserRole>? UserRoles { get; set; }
        
    }
}
