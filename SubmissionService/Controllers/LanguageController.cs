using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SubmissionService.Application.DTOs;
using SubmissionService.Application.Service;

namespace SubmissionService.Controllers
{
    [ApiController]
    [Route("api/language")]
    public class LanguageController : Controller
    {
        private readonly LanguageManager _languageManager;
        public LanguageController(LanguageManager languageManager)
        {
            _languageManager = languageManager;
        }

        [Authorize]
        [HttpGet("get-languages")]
        public async Task<IActionResult> Get()
        {
            var languages= await _languageManager.GetLanguages();
            if (languages == null) return NotFound();

            return  Ok(languages);
        }

        [Authorize(Roles ="Admin")]
        [HttpPost("add-language")]
        public async Task<IActionResult> Add([FromBody] Request l)
        {
            if (l == null)
            {
                return BadRequest();
            }
            if (! await _languageManager.AddLanguage(l.LanguageName, l.LanguageId))
            {
                return BadRequest();
            }
            return Ok();
        }

        [Authorize(Roles ="Admin")]
        [HttpDelete("delete-language/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id<0)
            {
                return BadRequest();
            }
            if (! await _languageManager.DeleteLanguage(id))
            {
                return BadRequest();
            }
            return Ok();
        }

        [Authorize(Roles ="Admin")]
        [HttpPut("update-language")]
        public async Task<IActionResult> Update([FromBody] Request language)
        {
            if (language == null)
            {
                return BadRequest();
            }
            bool success= await _languageManager.UpdateLanguage(language.LanguageName, language.LanguageId);
            if (!success) return BadRequest();
            return Ok();
        }

    }
}
