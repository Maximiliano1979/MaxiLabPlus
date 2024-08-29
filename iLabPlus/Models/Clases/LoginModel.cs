using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;


namespace iLabPlus.Models
{
    public class LoginModel
    {
        [Required]
        public string Usuario { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public bool Recordar { get; set; }

        //[Display(Name = "Remember Me")]
        //public bool RememberMe { get; set; }
        //public string ReturnUrl { get; set; }

        //[Required]
        //[EmailAddress]
        //public string Email { get; set; }

        //[Required]
        //[DataType(DataType.Password)]
        //public string Password { get; set; }

        //[Display(Name = "Remember me?")]
        //public bool RememberMe { get; set; }



    }
}
