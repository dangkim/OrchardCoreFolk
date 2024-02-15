using System.ComponentModel.DataAnnotations;

namespace OrchardCore.SongServices.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        public string UserIdentifier { get; set; }
    }
}
