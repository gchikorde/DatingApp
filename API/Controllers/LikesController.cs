using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LikesController : ControllerBase
    {
        private readonly IUserRepository userRepository;
        private readonly ILikesRepository likeRepository;

          public LikesController(IUserRepository userRepository, ILikesRepository likeRepository)
        {
            this.userRepository = userRepository;
            this.likeRepository = likeRepository;
        }

       
        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var sourceUserId = User.GetUserId();
            var likedUser= await userRepository.GetUserByUserNameAsync(username);
            var sourceUser = await likeRepository.GetUserWithLike(sourceUserId);
            
            if(likedUser == null) return NotFound();

            if(sourceUser.UserName == username) return BadRequest("You cannot like yourself.");

            var userLike = await likeRepository.GetUserLike(sourceUserId, likedUser.Id);

            if(userLike !=null) return BadRequest("You already liked this user.");

            userLike = new UserLike
            {
                SourceUserId=sourceUserId,
                LikedUserId = likedUser.Id
            };
            sourceUser.LikedUsers.Add(userLike);
            if(await userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to like the user");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users=  await likeRepository.GetUserLikes(likesParams);
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
            
            return Ok(users);
        }
       
    }
}