using System;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dtos
{
    public class UserForRegister
    {
        [Required(ErrorMessage="Username is required")]
        public string Username { get; set; }
        [Required(ErrorMessage="Password is required")]
        [StringLength(8, MinimumLength =12, ErrorMessage ="Please enter a valid password, this password must contain be 8 or 12 characters long")]
        [RegularExpression(@"^((?=.*[a-z])(?=.*[A-Z])(?=.*\d)).+$")]
        [DataType(DataType.Password)]
        [Display(Name ="Password")]
        public string Password { get; set; }
    }
}