using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
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
    public class PhotosController : ControllerBase
    {
        private readonly IOptions<CloudinarySettings> _cloudConfig;
        private readonly IMapper _mapper;
        private readonly IDatingRepository _repo;
        private Cloudinary _cloud;

        public PhotosController(IDatingRepository repo, IMapper mapper, IOptions<CloudinarySettings> cloudConfig)
        {
            this._repo = repo;
            this._mapper = mapper;
            this._cloudConfig = cloudConfig;

            Account acc = new Account(
                _cloudConfig.Value.CloudName,
                _cloudConfig.Value.ApiKey,
                _cloudConfig.Value.ApiSecret
            );

            _cloud = new Cloudinary(acc);
        }

        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id) {
            var photo1 = await _repo.GetPhoto(id);
            var photo2 = _mapper.Map<PhotoForReturnDto>(photo1);
            return Ok(photo2);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, [FromForm]PhotoForCreationDto photo) {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var user = await _repo.GetUser(userId);
            var file = photo.File;
            var uploadResult = new ImageUploadResult();
            if (file.Length > 0) {
                using (var stream = file.OpenReadStream()) {
                    var uploadParms = new ImageUploadParams()
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation().Width(500).Height(500).Crop("fill").Gravity("face")
                    };
                    uploadResult = _cloud.Upload(uploadParms);
                }
            }

            photo.Url = uploadResult.Uri.ToString();
            photo.PublicId = uploadResult.PublicId;
            var photo2 = _mapper.Map<Photo>(photo);

            if (!user.Photos.Any(u => u.IsMain)) {
                photo2.IsMain = true;
            }

            user.Photos.Add(photo2);
            if (await _repo.SaveAll()) {
                var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo2);
                return CreatedAtRoute("GetPhoto", new { userId = userId, id = photo2.Id}, photoToReturn);  
            }

            return BadRequest("Photo not added");
        }

    }
}