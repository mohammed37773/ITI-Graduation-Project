using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NurseriesNetwork.Core.DTOs.Common;
using NurseriesNetwork.Core.DTOs.Nursery;
using NurseriesNetwork.Core.Entities;
using NurseriesNetwork.Core.Interfaces.Repositories;
using NurseriesNetwork.Core.Interfaces.Services;
using System.Security.Claims;

namespace NurseriesNetwork.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NurseriesController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IImageService _imageService;
    private readonly IAiService _aiService;

    public NurseriesController(
        IUnitOfWork uow,
        IImageService imageService,
        IAiService aiService)
    {
        _uow = uow;
        _imageService = imageService;
        _aiService = aiService;
    }

    // ===========================
    // GET: api/nurseries
    // ===========================
    [HttpGet]
    public async Task<IActionResult> GetAll(
    [FromQuery] NurseryFilterDto filter)
    {
        var nurseries = await _uow.Nurseries
            .FilterAsync(filter.MaxPrice, filter.MinRating, filter.City);

        var totalItems = nurseries.Count();
        var totalPages = (int)Math.Ceiling(
            totalItems / (double)filter.PageSize);

        var data = nurseries
            .Skip((filter.PageNumber - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(n => MapToResponse(n, null));

        return Ok(new PaginatedResponseDto<NurseryResponseDto>(
            Data: data,
            PageNumber: filter.PageNumber,
            PageSize: filter.PageSize,
            TotalItems: totalItems,
            TotalPages: totalPages,
            HasNextPage: filter.PageNumber < totalPages,
            HasPreviousPage: filter.PageNumber > 1
        ));
    }

        // ===========================
        // GET: api/nurseries/nearby
        // ===========================
        [HttpGet("nearby")]
    public async Task<IActionResult> GetNearby(
        [FromQuery] double lat,
        [FromQuery] double lng,
        [FromQuery] double radius = 10)
    {
        var nurseries = await _uow.Nurseries
            .GetNearbyAsync(lat, lng, radius);

        var result = nurseries.Select(n =>
            MapToResponse(n, CalculateDistance(lat, lng,
                n.Location?.Latitude ?? 0,
                n.Location?.Longitude ?? 0)));

        return Ok(result);
    }

    // ===========================
    // GET: api/nurseries/{id}
    // ===========================
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var nursery = await _uow.Nurseries.GetWithDetailsAsync(id);
        if (nursery == null)
            return NotFound("الحضانة مش موجودة");

        return Ok(MapToResponse(nursery, null));
    }

    // ===========================
    // POST: api/nurseries
    // ===========================
    [HttpPost]
    [Authorize(Roles = "NurseryAdmin")]
    public async Task<IActionResult> Create(CreateNurseryDto dto)
    {
        var nursery = new Nursery
        {
            Name = dto.Name,
            Description = dto.Description,
            DailyPrice = dto.DailyPrice,
            AgeRangeMin = dto.AgeRangeMin,
            AgeRangeMax = dto.AgeRangeMax,
            Capacity = dto.Capacity,
            Location = new Location
            {
                Address = dto.Address,
                City = dto.City,
                District = dto.District,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude
            }
        };

        await _uow.Nurseries.AddAsync(nursery);
        await _uow.SaveChangesAsync();

        // توليد الـ Embedding للـ RAG
        await _aiService.GenerateAndSaveEmbeddingAsync(nursery);

        return CreatedAtAction(nameof(GetById),
            new { id = nursery.Id },
            MapToResponse(nursery, null));
    }

    // ===========================
    // PUT: api/nurseries/{id}
    // ===========================
    [HttpPut("{id}")]
    [Authorize(Roles = "NurseryAdmin")]
    public async Task<IActionResult> Update(int id, CreateNurseryDto dto)
    {
        var nursery = await _uow.Nurseries.GetWithDetailsAsync(id);
        if (nursery == null)
            return NotFound("الحضانة مش موجودة");

        nursery.Name = dto.Name;
        nursery.Description = dto.Description;
        nursery.DailyPrice = dto.DailyPrice;
        nursery.AgeRangeMin = dto.AgeRangeMin;
        nursery.AgeRangeMax = dto.AgeRangeMax;
        nursery.Capacity = dto.Capacity;

        if (nursery.Location != null)
        {
            nursery.Location.Address = dto.Address;
            nursery.Location.City = dto.City;
            nursery.Location.District = dto.District;
            nursery.Location.Latitude = dto.Latitude;
            nursery.Location.Longitude = dto.Longitude;
        }

        _uow.Nurseries.Update(nursery);
        await _uow.SaveChangesAsync();

        // تحديث الـ Embedding
        await _aiService.GenerateAndSaveEmbeddingAsync(nursery);

        return Ok(MapToResponse(nursery, null));
    }

    // ===========================
    // DELETE: api/nurseries/{id}
    // ===========================
    [HttpDelete("{id}")]
    [Authorize(Roles = "NurseryAdmin")]
    public async Task<IActionResult> Delete(int id)
    {
        var nursery = await _uow.Nurseries.GetByIdAsync(id);
        if (nursery == null)
            return NotFound("الحضانة مش موجودة");

        _uow.Nurseries.Delete(nursery);
        await _uow.SaveChangesAsync();

        return Ok("تم حذف الحضانة");
    }

    // ===========================
    // POST: api/nurseries/{id}/images
    // ===========================
    [HttpPost("{id}/images")]
    [Authorize(Roles = "NurseryAdmin")]
    public async Task<IActionResult> UploadImage(
        int id, IFormFile image)
    {
        var nursery = await _uow.Nurseries.GetByIdAsync(id);
        if (nursery == null)
            return NotFound("الحضانة مش موجودة");

        if (image == null || image.Length == 0)
            return BadRequest("الصورة مطلوبة");

        var imageUrl = await _imageService.UploadImageAsync(
            image.OpenReadStream(), image.FileName);

        var nurseryImage = new NurseryImage
        {
            NurseryId = id,
            ImageUrl = imageUrl,
            IsMain = !nursery.Images.Any()
        };

        await _uow.NurseryImages.AddAsync(nurseryImage);
        await _uow.SaveChangesAsync();

        return Ok(new { imageUrl });
    }

    // ===========================
    // DELETE: api/nurseries/{id}/images/{imageId}
    // ===========================
    [HttpDelete("{id}/images/{imageId}")]
    [Authorize(Roles = "NurseryAdmin")]
    public async Task<IActionResult> DeleteImage(int id, int imageId)
    {
        var image = await _uow.NurseryImages.GetByIdAsync(imageId);
        if (image == null || image.NurseryId != id)
            return NotFound("الصورة مش موجودة");

        await _imageService.DeleteImageAsync(image.ImageUrl);
        _uow.NurseryImages.Delete(image);
        await _uow.SaveChangesAsync();

        return Ok("تم حذف الصورة");
    }

    // ===========================
    // Helper Methods
    // ===========================
    private static NurseryResponseDto MapToResponse(
        Nursery n, double? distance) => new(
            n.Id,
            n.Name,
            n.Description,
            n.DailyPrice,
            n.AgeRangeMin,
            n.AgeRangeMax,
            n.AvgRating,
            n.IsVerified,
            n.Location?.City ?? "",
            n.Location?.Address ?? "",
            n.Location?.Latitude ?? 0,
            n.Location?.Longitude ?? 0,
            distance,
            n.Images.Select(i => i.ImageUrl).ToList()
        );

    private static double CalculateDistance(
        double lat1, double lon1,
        double lat2, double lon2)
    {
        const double R = 6371;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(lat1 * Math.PI / 180) *
                Math.Cos(lat2 * Math.PI / 180) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return R * 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
    }
}