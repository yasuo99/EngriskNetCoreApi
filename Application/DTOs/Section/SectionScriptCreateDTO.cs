namespace Application.DTOs.Section
{
    public class SectionScriptCreateDTO
    {
        public ScriptCreateDTO Grammar { get; set; }
        public ScriptCreateDTO Vocabulary { get; set; }
        public ScriptCreateDTO ToeicVocabulary { get; set; }
        public ScriptCreateDTO Reading  { get; set; }
        public ScriptCreateDTO Listening { get; set; }
        public ScriptCreateDTO Conversation { get; set; }
        public ScriptCreateDTO Writing { get; set; }

    }
}