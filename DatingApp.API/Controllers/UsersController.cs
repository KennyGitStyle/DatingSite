using System.Net;
using System.Security.Claims;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _datingRepo;
        private readonly IMapper _mapper;

        public UsersController(IDatingRepository datingRepo, IMapper mapper)
        {
            _datingRepo = datingRepo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers() {
            var users = await _datingRepo.GetUsers();
            var usersToReturn = _mapper.Map<IEnumerable<UserForList>>(users);
            return Ok(usersToReturn);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id){
            var user = await _datingRepo.GetUser(id);
            var userToReturn = _mapper.Map<UserForDetailed>(user);
            return Ok(userToReturn);
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdate userForUpdate){

            //var checkId = id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value) ? Unauthorized() : Ok();

            if (id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) Unauthorized();

            var userFromRepo = await _datingRepo.GetUser(id);
            
            _mapper.Map(userForUpdate, userFromRepo);

            if (await _datingRepo.SaveAll()) NoContent();

            throw new Exception($"Updating user {id} failed on save");
            
           
        }
    }
}