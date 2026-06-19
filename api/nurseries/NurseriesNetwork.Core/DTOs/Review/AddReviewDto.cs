using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NurseriesNetwork.Core.DTOs.Review
{
    public record AddReviewDto(
     [Required]
    [Range(1, 5, ErrorMessage = "التقييم من 1 لـ 5")]
    int Rating,

     [Required(ErrorMessage = "الكومنت مطلوب")]
    [MaxLength(500)]
    string Comment
 );
}
