using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NurseriesNetwork.Core.DTOs.Location;
using NurseriesNetwork.Core.DTOs.Nursery;
using NurseriesNetwork.Core.Interfaces.Repositories;
using NurseriesNetwork.Infrastructure.Services;
using NurseriesNetwork.Core.Entities;
using NurseriesNetwork.Core.DTOs.Review;

namespace NurseriesNetwork.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NurseriesController : ControllerBase
    {
        readonly IUnitOfWork _unitOfWork;

        public NurseriesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetNurseries()
        {
            var nurseries = await _unitOfWork.Nurseries.GetAllAsync();
            var nurseriesDto = nurseries.Select(n => new NurseryResponseDto
            {
                Id = n.Id,
                Name = n.Name,
                Description = n.Description,
                DailyPrice = n.DailyPrice,
                AvgRating = n.AvgRating,
                AgeRangeMin = n.AgeRangeMin,
                AgeRangeMax = n.AgeRangeMax,
                IsVerified = n.IsVerified,
                Address = n.Location.Address,
                City = n.Location.City,
                Latitude = n.Location.Latitude,
                Longitude = n.Location.Longitude,
                ImageUrls = n.Images.Select(i => i.ImageUrl).ToList()
            });
            return Ok(nurseriesDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNursery([FromBody] CreateNurseryDto nurseryDto)
        {
            var nursery = new Nursery
            {
                Name = nurseryDto.Name,
                Description = nurseryDto.Description,
                DailyPrice = nurseryDto.DailyPrice,
                AgeRangeMin = nurseryDto.AgeRangeMin,
                AgeRangeMax = nurseryDto.AgeRangeMax,
                Capacity = nurseryDto.Capacity,
                Location = new Location
                {
                    Address = nurseryDto.Address,
                    City = nurseryDto.City,
                    Latitude = nurseryDto.Latitude,
                    Longitude = nurseryDto.Longitude
                }
            };
            await _unitOfWork.Nurseries.AddAsync(nursery);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction(nameof(GetNurseries), new { id = nursery.Id }, nurseryDto);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNursery(int id, [FromBody] CreateNurseryDto nurseryDto)
        {
            var nursery = await _unitOfWork.Nurseries.GetByIdAsync(id);
            if (nursery == null)
            {
                return NotFound();
            }

            nursery.Name = nurseryDto.Name;
            nursery.Description = nurseryDto.Description;
            nursery.DailyPrice = nurseryDto.DailyPrice;
            nursery.AgeRangeMin = nurseryDto.AgeRangeMin;
            nursery.AgeRangeMax = nurseryDto.AgeRangeMax;
            nursery.Capacity = nurseryDto.Capacity;
            nursery.Location.Address = nurseryDto.Address;
            nursery.Location.City = nurseryDto.City;
            nursery.Location.Latitude = nurseryDto.Latitude;
            nursery.Location.Longitude = nurseryDto.Longitude;

            _unitOfWork.Nurseries.Update(nursery);
            await _unitOfWork.SaveChangesAsync();

            return Ok(nurseryDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNursery(int id)
        {
            var nursery = await _unitOfWork.Nurseries.GetByIdAsync(id);
            if (nursery == null)
            {
                return NotFound();
            }

            _unitOfWork.Nurseries.Delete(nursery);
            await _unitOfWork.SaveChangesAsync();

            return Ok();
        }

        /// <summary>
        /// Gets a specific nursery by ID.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("1/{id}")]
        public async Task<IActionResult> GetNursery(int id)
        {
            var nursery = await _unitOfWork.Nurseries.GetByIdAsync(id);
            if (nursery == null)
            {
                return NotFound();
            }
            var nurseryDto = new NurseryResponseDto
            {
                Id = nursery.Id,
                Name = nursery.Name,
                Description = nursery.Description,
                DailyPrice = nursery.DailyPrice,
                AvgRating = nursery.AvgRating,
                AgeRangeMin = nursery.AgeRangeMin,
                AgeRangeMax = nursery.AgeRangeMax,
                IsVerified = nursery.IsVerified,
                Address = nursery.Location.Address,
                City = nursery.Location.City,
                Latitude = nursery.Location.Latitude,
                Longitude = nursery.Location.Longitude,
                ImageUrls = nursery.Images.Select(i => i.ImageUrl).ToList()
            };
            return Ok(nurseryDto);
        }

        /// <summary>
        /// Gets a specific nursery by ID with reviews for Profile Page.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("2/{id}")]
        public async Task<IActionResult> GetNurseryWithReviews(int id)
        {
            var nursery = await _unitOfWork.Nurseries.GetWithReviewsAsync(id);
            if (nursery == null)
            {
                return NotFound();
            }
            var nurseryDto = new NurseryWithReviewsResponseDto
            {
                Id = nursery.Id,
                Name = nursery.Name,
                Description = nursery.Description,
                DailyPrice = nursery.DailyPrice,
                AvgRating = nursery.AvgRating,
                AgeRangeMin = nursery.AgeRangeMin,
                AgeRangeMax = nursery.AgeRangeMax,
                IsVerified = nursery.IsVerified,
                Address = nursery.Location.Address,
                City = nursery.Location.City,
                Latitude = nursery.Location.Latitude,
                Longitude = nursery.Location.Longitude,
                ImageUrls = nursery.Images.Select(i => i.ImageUrl).ToList(),
                Reviews = nursery.Reviews.Select(r => new ReviewResponseDto
                {
                    Rating = r.Rating,
                    Comment = r.Comment,
                    ParentId = r.ParentId,
                    ParentName = r.Parent.FullName
                }).ToList()
            };
            return Ok(nurseryDto);
        }
}
}
