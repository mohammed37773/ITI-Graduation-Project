using NurseriesNetwork.Core.DTOs.Booking;
using NurseriesNetwork.Core.DTOs.Review;
using NurseriesNetwork.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace NurseriesNetwork.Core.Interfaces.Repositories
{
    public interface IReviewRepository : IGenericRepository<Review>
    {
        Task<List<ReadReviewsDto>?> GetAllWithDetailsAsync(string currentUserId, int nurseryId);
        Task<ReadReviewsDto> GetWithDetailsAsync(string parentId);
    }
}
