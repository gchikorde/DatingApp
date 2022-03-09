using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
     [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository userRepository;
        private readonly IMapper mapper;
        private readonly IPhotoService photoService;

        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService)
        {
            this.userRepository = userRepository;
            this.mapper = mapper;
            this.photoService = photoService;
        }

        [HttpGet]
      
        public async Task<ActionResult<List<MemberDto>>> GetUsers()
        {
            var users = await userRepository.GetMembersAsync();
            return Ok(users);
        }

        // api/user/3
        [HttpGet("{username}", Name ="GetUser")]
       
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await userRepository.GetMemberAsync(username);
        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var user = await userRepository.GetUserByUserNameAsync(User.GetUserName());
            mapper.Map(memberUpdateDto, user);
            userRepository.Update(user);
            if(await userRepository.SaveAllAsync()) return NoContent();
            
            return BadRequest("Failed to update the user profile.");
        }

        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await userRepository.GetUserByUserNameAsync(User.GetUserName());
            var result = photoService.AddPhotoAsync(file);
            if(result.Result.Error !=null ) return BadRequest(result.Result.Error.Message);
            var photo= new Photo{
                Url=result.Result.SecureUrl.AbsoluteUri,
                PublicId=result.Result.PublicId
            };
            if(user.Photos.Count==0)
            {
                photo.IsMain=true;
            } 
            user.Photos.Add(photo);
            if(await userRepository.SaveAllAsync())
            {
                return CreatedAtRoute("GetUser", new { username=user.UserName}, mapper.Map<PhotoDto>(photo));

            }
            return BadRequest("Problem adding photo.");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await userRepository.GetUserByUserNameAsync(User.GetUserName());

            var photo = user.Photos.FirstOrDefault(u=>u.Id==photoId);
            if(photo.IsMain) return BadRequest("This is already you main photo");

            var currentMain = user.Photos.FirstOrDefault(u=>u.IsMain);
            if(currentMain !=null) currentMain.IsMain=false;
            photo.IsMain=true;
            if(await userRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to set main photo.");
        }

         [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
         var user = await userRepository.GetUserByUserNameAsync(User.GetUserName());
         var photo= user.Photos.FirstOrDefault(x=>x.Id==photoId);
            if(photo==null) return NotFound();
            if(photo.IsMain) return BadRequest("You cannot delete your main photo.");
            if(photo.PublicId!=null)
            {
                var result =await photoService.DeletePhotoAsync(photo.PublicId);
                if(result.Error !=null) return BadRequest(result.Error.Message);
            }

            user.Photos.Remove(photo);
            if(await userRepository.SaveAllAsync()) return Ok();
            return BadRequest("Failed to delete the photo.");
            
        } 
    }

   
}