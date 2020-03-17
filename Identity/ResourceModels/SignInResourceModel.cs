using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.ResourceModels
{
    public class SignInResourceModel
    {
        [Required(ErrorMessage = "Email alanı boş bırakılamaz")]
        [EmailAddress(ErrorMessage = "Not right format")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Şifre alanı boş bırakılamaz")]
        [MinLength(4, ErrorMessage = "Şifre minimum 4 karakter olmalıdır")]
        public string Password { get; set; }
    }
}
