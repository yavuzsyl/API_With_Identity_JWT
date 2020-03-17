using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.ResourceModels
{
    public class UserResourceModel
    {
        [Required(ErrorMessage = "Kullanıcı adı girilmesi zorunludur")]
        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        
        [EmailAddress(ErrorMessage = "Email adresi doğru formatta olmalı")]
        public string Email { get; set; }
        [Required(ErrorMessage ="Şifre zorunlu")]
        public string Password { get; set; }

        public DateTime? BirthDate { get; set; }
        public string Picture { get; set; }
        public string City { get; set; }
    }
}
