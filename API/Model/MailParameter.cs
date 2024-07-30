using System.ComponentModel.DataAnnotations;

namespace API.Model
{
    public class MailParameter
    {
        [Key]
        public MailType Type {  get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
    public enum MailType
    {
        ShareLink,AValider,AccusedReception
    }
}
