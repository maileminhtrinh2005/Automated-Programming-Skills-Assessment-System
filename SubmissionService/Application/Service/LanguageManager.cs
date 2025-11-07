using SubmissionService.Application.DTOs;
using SubmissionService.Application.Interface;

namespace SubmissionService.Application.Service
{
    public class LanguageManager
    {
        private readonly ILanguageRepository _language;
        public LanguageManager (ILanguageRepository language)
        {
            _language = language;
        }

        public async Task<List<LanguageDTO>?> GetLanguages()
        {
            return await _language.GetLanguages();
        }
        public async Task<bool> AddLanguage(string name, int id)
        {
            if (name== string.Empty || id < 0)
            {
                return false;
            }
            return await _language.AddLanguage(name, id);
        }
        
        public async Task<bool> DeleteLanguage(int id)
        {
            if (id < 0)
            {
                return false;
            }
            return await _language.DeleteLanguage(id);
        }
        public async Task<bool> UpdateLanguage(string name, int id)
        {
            if (name == string.Empty || id < 0)
            {
                return false;
            }
            return await _language.UpdateLanguage(name, id);
        }

    }
}
