using System.ComponentModel.DataAnnotations;

namespace ProductManagemet.Models
{
    public class Party
    {
        [Key]
        public int PartyId { get; set; }
        public string PartyName { get; set; }
       
    }
}
    