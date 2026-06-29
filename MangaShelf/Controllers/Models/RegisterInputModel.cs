using System.ComponentModel.DataAnnotations;
using MangaShelf.Localization.Resources;

namespace MangaShelf.Controllers.Models;

public sealed class RegisterInputModel
{
    [Required(ErrorMessageResourceName = "RequiredField", ErrorMessageResourceType = typeof(UserInterfaceResource))]
    [StringLength(100, ErrorMessageResourceName = "LengthError", ErrorMessageResourceType = typeof(UserInterfaceResource), MinimumLength = 2)]
    [Display(Name = "Username")]
    public string Username { get; set; } = "";

    [Required(ErrorMessageResourceName = "RequiredField", ErrorMessageResourceType = typeof(UserInterfaceResource))]
    [EmailAddress(ErrorMessageResourceName = "InvalidEmail", ErrorMessageResourceType = typeof(UserInterfaceResource))]
    [Display(Name = "Email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessageResourceName = "RequiredField", ErrorMessageResourceType = typeof(UserInterfaceResource))]
    [StringLength(100,  ErrorMessageResourceName = "LengthError", ErrorMessageResourceType = typeof(UserInterfaceResource), MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = "";

    [DataType(DataType.Password)]
    [Display(Name = "ConfirmPassword")]
    [Compare("Password", ErrorMessageResourceName = "PasswordsDoNotMatch", ErrorMessageResourceType = typeof(UserInterfaceResource))]
    public string ConfirmPassword { get; set; } = "";
}
