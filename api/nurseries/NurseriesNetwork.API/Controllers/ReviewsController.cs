using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseriesNetwork.Core.DTOs.Review;
using NurseriesNetwork.Core.Entities;
using NurseriesNetwork.Core.Enums;
using NurseriesNetwork.Core.Interfaces.Repositories;
using System.Security.Claims;

namespace NurseriesNetwork.API.Controllers;

[ApiController]
[Route("api/nurseries/{nurseryId}/reviews")]
public class ReviewsController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public ReviewsController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    // ===========================
    // GET: api/nurseries/{nurseryId}/reviews
    // ===========================
    [HttpGet]
    public async Task<IActionResult> GetReviews(int nurseryId)
    {
        var nursery = await _uow.Nurseries.GetByIdAsync(nurseryId);
        if (nursery == null)
            return NotFound("الحضانة مش موجودة");

        var reviews = await _uow.Reviews
            .FindAsync(r => r.NurseryId == nurseryId);

        return Ok(reviews);
    }

    // ===========================
    // POST: api/nurseries/{nurseryId}/reviews
    // ===========================
    [HttpPost]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> AddReview(
        int nurseryId, AddReviewDto dto)
    {
        var parentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        // التحقق من الحضانة
        var nursery = await _uow.Nurseries.GetWithDetailsAsync(nurseryId);
        if (nursery == null)
            return NotFound("الحضانة مش موجودة");

        // التحقق إن Parent حجز في الحضانة دي
        var hasBooking = await _uow.Bookings.FindAsync(b =>
            b.ParentId == parentId &&
            b.NurseryId == nurseryId &&
            b.Status == BookingStatus.Confirmed);

        if (!hasBooking.Any())
            return BadRequest("مش هتقدر تعمل Review غير لو حجزت في الحضانة دي");

        // التحقق إنه مش عامل Review قبل كده
        var existingReview = await _uow.Reviews.FindAsync(r =>
            r.ParentId == parentId &&
            r.NurseryId == nurseryId);

        if (existingReview.Any())
            return BadRequest("عملت Review للحضانة دي قبل كده");

        // إضافة الـ Review
        var review = new Review
        {
            ParentId = parentId,
            NurseryId = nurseryId,
            Rating = dto.Rating,
            Comment = dto.Comment
        };

        await _uow.Reviews.AddAsync(review);

        // تحديث متوسط التقييم
        await UpdateAvgRatingAsync(nursery);

        await _uow.SaveChangesAsync();

        return Ok(review);
    }

    // ===========================
    // DELETE: api/nurseries/{nurseryId}/reviews/{reviewId}
    // ===========================
    [HttpDelete("{reviewId}")]
    [Authorize(Roles = "Parent")]
    public async Task<IActionResult> DeleteReview(
        int nurseryId, int reviewId)
    {
        var parentId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var review = await _uow.Reviews.GetByIdAsync(reviewId);
        if (review == null || review.NurseryId != nurseryId)
            return NotFound("الـ Review مش موجود");

        if (review.ParentId != parentId)
            return Forbid();

        _uow.Reviews.Delete(review);

        // تحديث متوسط التقييم
        var nursery = await _uow.Nurseries.GetWithDetailsAsync(nurseryId);
        if (nursery != null)
            await UpdateAvgRatingAsync(nursery);

        await _uow.SaveChangesAsync();

        return Ok("تم حذف الـ Review");
    }

    // ===========================
    // Helper
    // ===========================
    private async Task UpdateAvgRatingAsync(Nursery nursery)
    {
        var reviews = await _uow.Reviews
            .FindAsync(r => r.NurseryId == nursery.Id);

        nursery.AvgRating = reviews.Any()
            ? reviews.Average(r => r.Rating)
            : 0;

        _uow.Nurseries.Update(nursery);
    }
}