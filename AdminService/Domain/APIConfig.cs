using System.ComponentModel.DataAnnotations;

namespace AdminService.Domain
{
    public class APIConfig
    {
        [Key]
        public int APIID { get; set; }
        public string Name { get; set; }
        public string BaseURL { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }
}
