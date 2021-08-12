using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.DTOs.Pagination;
using Application.DTOs.Question;
using Application.DTOs.Quiz;
using Application.DTOs.Script;
using Application.DTOs.Section;
using Application.DTOs.Vocabulary;
using Application.DTOs.Word;
using Domain.Enums;
using Domain.Models;

namespace Application.Services.Core.Abstraction
{
    public interface ISectionService: IPublishService
    {
        Task<bool> CheckAnonymousLearnAsync(Guid id);
        Task<bool> CheckExistAsync(Guid sectionId);
        Task<bool> CheckProgressAsync(int accountId);
        Task<bool> CheckPreviousSectionDoneAsync(Guid id, int accountId);
        Task<PaginateDTO<SectionDTO>> GetManageSectionsAsync(PaginationDTO pagination, string search = null);
        Task<PaginateDTO<SectionDTO>> GetSectionsAsync(PaginationDTO pagination);
        Task<PaginateDTO<SectionDTO>> GetUserSectionsAndProgressAsync(PaginationDTO pagination, int accountId);
        Task<PaginateDTO<SectionDTO>> GetUserPublicSectionsAndProgressAsync(PaginationDTO pagination, int ownerId, int accountId);
        Task<List<SectionDTO>> GetFreeSectionsAsync();
        Task<bool> CreateSectionScriptAsync(Guid setionId, ScriptCreateDTO script);
        Task<SectionScriptDTO> GetSectionScriptAsync(Guid sectionId);

        //Script
        Task<bool> CheckSectionScriptExistAsync(Guid sectionId, Guid scriptId);

        Task<bool> CheckScriptTypeExistAsync(Guid sectionId, ScriptTypes type);
        Task<bool> CreateSectionScriptsAsync(Guid sectionId, SectionScriptCreateDTO scripts);
        Task<bool> CreateSectionScriptAsync(Guid sectionId, List<ScriptCreateDTO> scripts);
        Task<ScriptLearnDTO> ScriptLearnAsync(Guid id, int accountId);
        Task<ScriptLearnDTO> AnonymousScriptLearnAsync(Guid id);
        Task<ScriptLearnDTO> ScriptDoneAsync(Guid id, int accountId);

        Task CreateAccountSectionProgressAsync(Guid id, int accountId);
        Task<SectionLearningScript> AnonymousSectionLearnAsync();
        Task<SectionLearningScript> SectionPreviewAsync(Guid id);
        Task<SectionLearningScript> SectionLearn(Guid sectionId, int accountId, int quizQuestionConfig = 5, int listeningQuestionConfig = 5, int conversationConfig = 5);
        Task<VocabularyLearnDTO> VocabularyLearnAsync(Guid sectionId, int accountId);
        Task<SectionFinishUpDTO> SectionFinishUpAsync(Guid sectionId, int accountId, string action);
        Task<bool> CheckQuestionAnswerAsync(Guid quizId, Guid questionId, Guid answerId);
        Task<bool> DeleteSectionAsync(Guid id);
        Task DoneQuizAsync();
    }
}