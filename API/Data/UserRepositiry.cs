using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
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

        public async Task<IEnumerable<MemberDto>> GetMembersAsync()
        {
           return await context.Users
           .ProjectTo<MemberDto>(mapper.ConfigurationProvider)
           .ToListAsync();
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