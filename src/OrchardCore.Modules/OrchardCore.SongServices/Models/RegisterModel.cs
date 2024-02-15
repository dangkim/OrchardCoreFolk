using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;

namespace OrchardCore.SongServices.Models
{
    public class RegisterModel
    {
        public string UserName { get; set; }

        public string Email { get; set; }

        public string TraderType { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordModel
    {
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }
        public string ResetToken { get; set; }
        public string UserId { get; set; }

    }
}
