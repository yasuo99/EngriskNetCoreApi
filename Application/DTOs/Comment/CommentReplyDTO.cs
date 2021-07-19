using System.ComponentModel.DataAnnotations;

namespace Application.DTOs.Comment
{
    public class CommentReplyDTO
    {
        [Required]
        public string Content { get; set; }
    }
}