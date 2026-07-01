using Microsoft.EntityFrameworkCore;
using NurseriesNetwork.Core.DTOs.Review;
using NurseriesNetwork.Core.Entities;
using NurseriesNetwork.Core.Interfaces.Repositories;
using NurseriesNetwork.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Infrastructure.Repositories
{
    public class ReviewRepository : GenericRepository<Review>, IReviewRepository
    {
        public ReviewRepository(AppDbContext context) : base(context) { }

        public async Task<List<ReadReviewsDto>?> GetAllWithDetailsAsync(string currentUserId, int nurseryId)
        {
            return await _context.Reviews
                .Where(r => r.NurseryId == nurseryId)
                .OrderByDescending(r => r.ParentId == currentUserId)
                .Select(r => new ReadReviewsDto
                {
                    ParentName = r.Parent.FullName,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt
                })                
                .ToListAsync();
        }

        public async Task<ReadReviewsDto> GetWithDetailsAsync(string parentId)
        {
            var review = await _context.Reviews.Include(r=>r.Parent)
                .FirstOrDefaultAsync(r => r.ParentId == parentId);

            if(review == null)
                throw new Exception("Not Found");

            var reviewDto = new ReadReviewsDto
            {
                ParentName = review.Parent.FullName,
                Rating = review.Rating,
                Comment = review.Comment,
                CreatedAt = review.CreatedAt
            };
            return reviewDto;
        }
    }
}
