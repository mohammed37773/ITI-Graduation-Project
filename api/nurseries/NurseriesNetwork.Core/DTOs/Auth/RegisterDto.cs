using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NurseriesNetwork.Core.DTOs.Auth
{
    public record RegisterDto(
     [Required(ErrorMessage = "الاسم مطلوب")]
    [MaxLength(100)] string FullName,

     [Required(ErrorMessage = "الإيميل مطلوب")]
    [EmailAddress(ErrorMessage = "إيميل غير صحيح")] string Email,

     [Required(ErrorMessage = "الباسورد مطلوب")]
    [MinLength(6, ErrorMessage = "الباسورد أقل من 6 حروف")] string Password,

     string Role = "Parent"
 );
}
