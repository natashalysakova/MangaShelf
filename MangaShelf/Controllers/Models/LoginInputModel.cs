using System.ComponentModel.DataAnnotations;
using MangaShelf.Localization.Resources;

namespace MangaShelf.Controllers.Models;

public sealed class LoginInputModel
    {
        [Required(ErrorMessageResourceName = "RequiredField", ErrorMessageResourceType = typeof(UserInterfaceResource))]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessageResourceName = "RequiredField", ErrorMessageResourceType = typeof(UserInterfaceResource))]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;
    }