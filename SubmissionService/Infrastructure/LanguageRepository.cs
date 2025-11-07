using Microsoft.EntityFrameworkCore;
using SubmissionService.Application.DTOs;
using SubmissionService.Application.Interface;
using SubmissionService.Domain;
using SubmissionService.Infrastructure.Persistence;

namespace SubmissionService.Infrastructure
{
    public class LanguageRepository : ILanguageRepository
    {
        private readonly AppDbContext _context;
        public LanguageRepository (AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task<bool> AddLanguage(string name, int id)
        {
            if (name==string.Empty || id <= 0)
            {
                return false;
            }
            var language = new Language
            {
                LanguageId = id,
                LanguageName = name
            };
            await _context.Languages.AddAsync(language);
            await _context.SaveChangesAsync();
            return true;

        }

        public async Task<bool> DeleteLanguage(int id)
        {
            if (id < 0)
            {
                return false;

            }
            var language = await _context.Languages.FindAsync(id);
            if (language == null) return false;
            _context.Languages.Remove(language);
            await _context.SaveChangesAsync();


            return true;
        }

        public async Task<List<LanguageDTO>?> GetLanguages()
        {
            var languages = await _context.Languages.
                Select(l=> new LanguageDTO
                {
                    LanguageId= l.LanguageId,
                    LanguageName= l.LanguageName
                }).
                ToListAsync();
            if (languages.Count < 0)
            {
                return null;
            }
            return languages;
        }

        public async Task<bool> UpdateLanguage(string name, int id)
        {
            var language = await _context.Languages.FindAsync(id);
            if (language == null) return false;

            if (name != string.Empty) language.LanguageName = name;
            if (id > 0) language.LanguageId = id;
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
