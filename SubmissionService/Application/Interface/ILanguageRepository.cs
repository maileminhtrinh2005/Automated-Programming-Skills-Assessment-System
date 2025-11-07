using SubmissionService.Application.DTOs;

namespace SubmissionService.Application.Interface
{
    public interface ILanguageRepository
    {
        public Task<List<LanguageDTO>?> GetLanguages();
        public Task<bool> AddLanguage(string name, int id);
        public Task<bool> DeleteLanguage(int id);
        public Task<bool> UpdateLanguage(string name, int id);


    }
}
