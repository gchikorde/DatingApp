using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext context;

        public LikesRepository(DataContext context)
        {
            this.context = context;
        }

        public async Task<UserLike> GetUserLike(int SourceUserId, int LikedUserId)
        {
            return await context.Likes.FindAsync(SourceUserId,LikedUserId);
        }

        public async Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams)
        {
            var users= context.Users.OrderBy(u=>u.UserName).AsQueryable();
            var likes = context.Likes.AsQueryable();
            if(likesParams.Predicate =="liked")
            {
                likes= likes.Where(like=>like.SourceUserId == likesParams.UserId);
                users= likes.Select(like=>like.LikedUser);
            }
            if(likesParams.Predicate=="likedBy")
            {
                 likes= likes.Where(like=>like.LikedUserId == likesParams.UserId);
                users= likes.Select(like=>like.SourceUser);
            }

            var likedUsers= users.Select(user=>new LikeDto{
                Username= user.UserName,
                KnownAs=user.KnownAs,
                Age= user.DateOfBirth.CalculateAge(),
                PhotoUrl=user.Photos.FirstOrDefault(p=>p.IsMain).Url,
                City= user.City,
                Id=user.Id
            });

            return await PagedList<LikeDto>.CreateAsync(likedUsers, likesParams.PageNUmber, likesParams.PageSize);
        }

        public async Task<AppUser> GetUserWithLike(int userId)
        {
            return await context.Users
            .Include(l=>l.LikedUsers)
            .FirstOrDefaultAsync(u=>u.Id==userId);
        }
    }
}