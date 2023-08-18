using FileListener.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System.ComponentModel.DataAnnotations;

namespace FileListener.Controllers
{
    public class FolderController : Controller
    {
        private readonly IFolderService _service;
        public FolderController(IFolderService folderService)
        {
            _service = folderService;
        }
        [HttpPost(nameof(AddFolder))]
        [SwaggerOperation(Summary = "Adds a folder for monitoring")]
        public async Task<IActionResult> AddFolder([Required] string folderName)
        {
            var result = await _service.AddFolderAsync(folderName);
            return result ? Ok(result) : BadRequest(result);
        }

        [HttpDelete(nameof(RemoveFolder))]
        [SwaggerOperation(Summary = "Remove a folder from monitoring")]
        public async Task<IActionResult> RemoveFolder([Required] string folderName)
        {
            var result = await _service.RemoveFolderAsync(folderName);
            return result ? Ok(result) : BadRequest(result);
        }

        [HttpGet(nameof(GetAllFolders))]
        [SwaggerOperation(Summary = "Get a list of all monitored folders")]
        public async Task<IActionResult> GetAllFolders()
        {
            var results = await _service.GetAllFoldersAsync();
            return results.Any() ? Ok(results) : BadRequest("No results");
        }
    }
}
