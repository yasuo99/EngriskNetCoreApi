using System;

namespace Application.DTOs.Section
{
    public class ScriptLearnHistoryDTO
    {
        public Guid Id { get; set; }
        public bool IsCurrentPosition { get; set; }
        public string Type { get; set; }
        public bool IsDone { get; set; }
    }
}