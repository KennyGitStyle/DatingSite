using System;
using System.ComponentModel.DataAnnotations;

namespace DatingApp.API.Dtos
{
    public class UserForRegister
    {
        [Required(ErrorMessage="Username is required")]
        public string Username { get; set; }
        [Required(ErrorMessage="Password is required")]
        [StringLength(12, MinimumLength =8, ErrorMessage ="Please enter a valid password, this password must contain atleast 8 - 12 characters.")]
        //[RegularExpression(@"^((?=.*[a-z])(?=.*[A-Z])(?=.*\d)).+$")]
        [DataType(DataType.Password)]
        [Display(Name ="Password")]
        public string Password { get; set; }
    }
}