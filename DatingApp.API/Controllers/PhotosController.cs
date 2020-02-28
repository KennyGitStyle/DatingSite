using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using datingapp.api.Dtos;
using DatingApp.API.Data;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [Route("api/users/{userId}/photos")]
    [ApiController]
    public class PhotosController: ControllerBase 
    {
        private readonly IDatingRepository _datingRepo;
        private readonly IMapper _mapper;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;
        private Cloudinary _cloudinary;

        public PhotosController(IDatingRepository datingRepo, IMapper mapper, 
        IOptions<CloudinarySettings> cloudinaryConfig)
        {
            _datingRepo = datingRepo;
            _mapper = mapper;
            _cloudinaryConfig = cloudinaryConfig;

            Account acc = new Account(
                _cloudinaryConfig.Value.CloudName,
                _cloudinaryConfig.Value.ApiKey,
                _cloudinaryConfig.Value.ApiSecret
            );

            _cloudinary = new Cloudinary(acc);
        }

        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo = await _datingRepo.GetPhoto(id);

            var photoToReturn = _mapper.Map<PhotoForReturn>(photoFromRepo);

            return Ok(photoToReturn);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, 
        [FromForm]PhotoForCreation photoForCreation)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) Unauthorized();

            var userFromRepo = await _datingRepo.GetUser(userId);

            var file = photoForCreation.File;

            var uploadResults = new ImageUploadResult();

            if(file.Length > 0){
                using (var stream = file.OpenReadStream()){
                    var uploadParams = new ImageUploadParams(){
                        File = new FileDescription(file.Name, stream), 
                        Transformation = new Transformation()
                        .Width(500).Height(500).Crop("fill").Gravity("face")
                    };

                    uploadResults = _cloudinary.Upload(uploadParams);
                }
            }

            photoForCreation.Url = uploadResults.Uri.ToString();
            photoForCreation.PublicId = uploadResults.PublicId;
            
            var photo = _mapper.Map<Photo>(photoForCreation);

            if(!userFromRepo.Photos.Any(u => u.IsMain)){
                photo.IsMain = true;
            }

            userFromRepo.Photos.Add(photo);

            if(await _datingRepo.SaveAll())
            {
                var photoForReturn = _mapper.Map<PhotoForReturn>(photo);
                
                return CreatedAtRoute("GetPhoto", new { userId = userId, id = photo.Id}, photoForReturn);
            }

            return BadRequest("Could not add photo...");
        }

        [HttpPost("{id}/setMain")]
        public async Task<IActionResult> SetMainPhoto(int userId, int id)
        {
             if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) Unauthorized();

             var user = await _datingRepo.GetUser(userId);

             if(!user.Photos.Any(p => p.Id == id)) {
                 return Unauthorized();
             }

             var photoFromRepo = await _datingRepo.GetPhoto(id);

             if(photoFromRepo.IsMain) {
                 return BadRequest("This is already the main photo..");
             }

             var currentMainPhoto = await _datingRepo.GetMainPhotoForUser(userId);

             currentMainPhoto.IsMain = false;

             photoFromRepo.IsMain = true;

             if(await _datingRepo.SaveAll()){
                 return NoContent();
             }

             return BadRequest("Could not set photo to main");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id){
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value)) Unauthorized();

            var user = await _datingRepo.GetUser(userId);

             if(!user.Photos.Any(p => p.Id == id)) {
                 return Unauthorized();
             }

             var photoFromRepo = await _datingRepo.GetPhoto(id);

             if(photoFromRepo.IsMain) {
                 return BadRequest("You cannot delete your main photo..");
             }

             if(photoFromRepo.PublicId != null){
                var deleteParams = new DeletionParams(photoFromRepo.PublicId);

                var result = _cloudinary.Destroy(deleteParams);

                if(result.Result == "ok"){
                    _datingRepo.Delete(photoFromRepo);
                }
             }

             if(photoFromRepo.PublicId == null){
                 _datingRepo.Delete(photoFromRepo);
             }

             if(await _datingRepo.SaveAll()){
                 return Ok();
             }

             return BadRequest("Failed to delete photo");

        }

    }
}