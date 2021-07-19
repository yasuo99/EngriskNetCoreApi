using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Pagination;
using Application.DTOs.Question;
using Application.DTOs.Word;
using Application.DTOs.Word.WordCategory;
using Application.Helper;
using Application.Services.Core.Abstraction;
using Application.Utilities;
using AutoMapper;
using Domain.Models;
using Domain.Models.Version2;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Z.EntityFramework.Plus;

namespace Application.Services.Core
{
    public class WordCategoryService : IWordCategoryService
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;
        private readonly IFileService _fileService;
        private WordCategory _wordCategory;
        public WordCategoryService(IMapper mapper, IFileService fileService, ApplicationDbContext context)
        {
            _mapper = mapper;
            _fileService = fileService;
            _context = context;
        }

        public async Task<bool> CheckConflictAsync(string name)
        {
            return await _context.WordCategories.AnyAsync(wc => wc.CategoryName == name);
        }

        public async Task<WordCategoryDTO> CreateCategoryAsync(WordCategoryCreateDTO wordCategoryCreateDTO)
        {
            var wordCategory = _mapper.Map<WordCategory>(wordCategoryCreateDTO);
            if (wordCategoryCreateDTO.Image != null)
            {
                wordCategory.CategoryImage = _fileService.UploadFile(wordCategoryCreateDTO.Image, SD.ImagePath);
            }
            _context.WordCategories.Add(wordCategory);
            if (await _context.SaveChangesAsync() > 0)
            {
                return _mapper.Map<WordCategoryDTO>(wordCategory);
            }
            return null;
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            _wordCategory ??= await _context.WordCategories.FirstOrDefaultAsync(wc => wc.Id == id);
            _context.Remove(_wordCategory);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistAsync(Guid id)
        {
            _wordCategory ??= await _context.WordCategories.Include(inc => inc.Tags).FirstOrDefaultAsync(wc => wc.Id == id);
            return _wordCategory != null;
        }

        public async Task<PaginateDTO<WordCategory>> GetAllAsync(PaginationDTO pagination, string search = null, bool learn = false, string tag = "all")
        {
            var wordCategories = await _context.WordCategories.Include(inc => inc.Words).Include(inc => inc.Tags).ThenInclude(inc => inc.CategoryTag).OrderByDescending(orderBy => orderBy.CreatedDate).AsNoTracking().ToListAsync();
            if(search != null){
                wordCategories = wordCategories.Where(wc => wc.CategoryName.ToLower().Contains(search.Trim().ToLower())).ToList();
            }
            if(learn){
                wordCategories = wordCategories.Where(wc => wc.Words.Count > 0).ToList();
            }
            if(tag != "all"){
                wordCategories = wordCategories.Where(wc => wc.Tags.Any(val => val.CategoryTag.Tag.ToLower().Equals(tag.ToLower()))).ToList();
            }
            var paginateWordCategories = PagingList<WordCategory>.OnCreate(wordCategories, pagination.CurrentPage, pagination.PageSize);
            return paginateWordCategories.CreatePaginate();
        }

        public async Task<List<WordCategoryDTO>> GetAllAsync()
        {
            var wordCategories = await _context.WordCategories.ToListAsync();
            return _mapper.Map<List<WordCategoryDTO>>(wordCategories);
        }

        public async Task<WordCategoryDTO> GetDetailAsync(Guid id)
        {
            var wordCategory = await _context.WordCategories.Where(predicate: wc => wc.Id == id).AsNoTracking()
            .FirstOrDefaultAsync();
            var wordCategoryDTO = _mapper.Map<WordCategoryDTO>(wordCategory);
            wordCategoryDTO.Vocabulary = _mapper.Map<List<WordDTO>>(await _context.Categories.Where(c => c.WordCategoryId == id).Include(inc => inc.Word).ThenInclude(inc => inc.Memories).Select(sel => sel.Word).ToListAsync());
            return wordCategoryDTO;
        }

        public async Task<List<QuestionDTO>> GetPracticeQuestion(Guid categoryId)
        {
            List<Question> questions = new List<Question>();
            var wordCategory = await GetDetailAsync(categoryId);
            // foreach(var word in wordCategory.Words){
            //     var questionsOfWord = await _context.WordQuestions.Where(wq => wq.WordId == word.Id).Include(inc => inc.Question).ThenInclude(inc => inc.Answers).Select(sl => sl.Question).AsNoTracking().ToListAsync();
            //     var question = questionsOfWord.GetOneRandomFromList();
            //     questions.Add(question);
            // }
            var returnQuestions = _mapper.Map<List<QuestionDTO>>(questions);
            return returnQuestions.Shuffle().ToList();
        }

        public async Task<WordCategoryDTO> UpdateCategoryAsync(Guid id, WordCategoryCreateDTO wordCategoryUpdateDTO)
        {
            _wordCategory ??= await _context.WordCategories.Include(inc => inc.Tags).FirstOrDefaultAsync(wc => wc.Id == id);
            _mapper.Map(wordCategoryUpdateDTO, _wordCategory);
            if (wordCategoryUpdateDTO.Image != null)
            {
                if (!string.IsNullOrEmpty(_wordCategory.CategoryImage))
                {
                    _fileService.DeleteFile(_wordCategory.CategoryImage);
                }
                _wordCategory.CategoryImage = _fileService.UploadFile(wordCategoryUpdateDTO.Image, SD.ImagePath);
            }
            if (await _context.SaveChangesAsync() > 0)
            {
                return _mapper.Map<WordCategoryDTO>(_wordCategory);
            }
            return null;
        }

        public async Task<WordCategoryDTO> UserGetDetailAsync(int accountId, Guid categoryId)
        {
            var wordCategory = await GetDetailAsync(categoryId);
            var returnWordCateogry = _mapper.Map<WordCategoryDTO>(wordCategory);
            foreach (var word in returnWordCateogry.Vocabulary)
            {
                word.Memory = await _context.AccountCardmems.Where(ac => ac.AccountId == accountId && ac.WordId == word.Id).Include(inc => inc.Memory).Select(sl => sl.Memory).AsNoTracking().FirstOrDefaultAsync();
                word.QuestionDTO = _mapper.Map<QuestionDTO>(await _context.WordQuestions.Where(wq => wq.WordId == word.Id).Include(inc => inc.Question).ThenInclude(inc => inc.Answers).Select(sel => sel.Question).FirstOrDefaultAsync());
            }
            return returnWordCateogry;
        }
    }
}