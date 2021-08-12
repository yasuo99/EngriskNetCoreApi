using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Enums;
using Domain.Models.BaseModel.Generic;
using Domain.Models.Version2;
using Newtonsoft.Json;

namespace Domain.Models
{
    public class Word : AuditEntity<Guid>
    {
        public Word()
        {
            Groups = new HashSet<WordGroup>();
            Learned = new HashSet<WordLearnt>();
            Memories = new HashSet<Memory>();
            CardMems = new HashSet<AccountCardmem>();
            Questions = new HashSet<WordQuestion>();
            Synonyms = new HashSet<Word>();
            Families = new HashSet<Word>();
            Categories = new HashSet<Category>();
            Type = VocabularyType.Insert;
            PublishStatus = PublishStatus.UnPublished;
            Status = VocabularyStatus.Free;
        }
        public Guid? FamilyId { get; set; }
        public virtual Word Family { get; set; }
        public Guid? SynonymId { get; set; }
        public virtual Word Synonym { get; set; }
        public string WordImg { get; set; }
        public string Eng { get; set; }
        public string Spelling { get; set; }
        public string WordVoice { get; set; }
        public string Vie { get; set; }
        public WordClasses Class { get; set; }
        public VocabularyStatus Status { get; set; }
        public VocabularyType Type { get; set; }
        public PublishStatus PublishStatus { get; set; }
        public virtual ICollection<WordGroup> Groups { get; set; }
        public virtual ICollection<WordLearnt> Learned { get; set; }
        public virtual ICollection<Memory> Memories { get; set; }
        public virtual ICollection<AccountCardmem> CardMems { get; set; }
        [JsonIgnore]

        public virtual ICollection<WordQuestion> Questions { get; set; }
        public virtual ICollection<Word> Synonyms { get; set; }
        public virtual ICollection<Category> Categories { get; set; }
        public virtual ICollection<Example> Examples { get; set; }
        public virtual ICollection<ScriptWord> Scripts { get; set; }
        public virtual ICollection<Word> Families { get; set; }

    }
}