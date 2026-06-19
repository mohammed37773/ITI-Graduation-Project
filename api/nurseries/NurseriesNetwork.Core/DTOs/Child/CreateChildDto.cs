using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NurseriesNetwork.Core.DTOs.Child
{
    public record CreateChildDto(
    [Required(ErrorMessage = "اسم الطفل مطلوب")]
    [MaxLength(100)]
    string FullName,

    [Required(ErrorMessage = "تاريخ الميلاد مطلوب")]
    DateOnly DateOfBirth,

    string? SpecialNeeds
);
}
