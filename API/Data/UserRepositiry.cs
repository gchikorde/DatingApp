using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class UserRepositiry : IUserRepository
    {
        private readonly DataContext context;
        private readonly IMapper mapper;

        public UserRepositiry(DataContext context, IMapper mapper)
        {
            this.context = context;
            this.mapper = mapper;
        }
        public async Task<IEnumerable<AppUser>> GetAllUsersAsync()
        {
            return await context.Users
            .Include(p=>p.Photos)
            .ToListAsync();
        }

        public async Task<MemberDto> GetMemberAsync(string uname)
        {
            return await context.Users
            .Where(x=>x.UserName==uname)
            .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
        //    return await context.Users
        //    .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
        //    .ToListAsync();

         var query= context.Users.AsQueryable();
                // .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
                // .AsNoTracking();
                query = query.Where(u=>u.UserName != userParams.CurrentUsername);
                query = query.Where(u=>u.Gender == userParams.Gender);

                var minDob= DateTime.Today.AddYears(-userParams.MaxAge -1);
                var maxDob= DateTime.Today.AddYears(-userParams.MinAge);

                query = query.Where(u=>u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);

                query = userParams.OrderBy switch
                {
                    "created" => query.OrderByDescending(u=>u.Created),
                    _ => query.OrderByDescending(u=>u.LastActive),
                };

            return await PagedList<MemberDto>.CreateAsync(query.ProjectTo<MemberDto>(mapper.
                ConfigurationProvider).AsNoTracking(),
                     userParams.PageNUmber, userParams.PageSize);
         
           
        }

        public async Task<AppUser> GetUserByIdAsync(int Id)
        {
            return await context.Users
            .FindAsync(Id);
        }

        public async Task<AppUser> GetUserByUserNameAsync(string uname)
        {
            return await context.Users
            .Include(p=>p.Photos)
            .SingleOrDefaultAsync(x=>x.UserName == uname);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await context.SaveChangesAsync() >0;
        }

        public void Update(AppUser user)
        {
           context.Entry(user).State = EntityState.Modified;
        }
    }
}