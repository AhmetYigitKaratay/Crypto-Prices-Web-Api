using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CryptoPricesWithWebApi.Data
{
   public class PairPrice
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Pairsymbol { get; set; }
        public decimal Price { get; set; }
        public DateTime Time { get; set; }
    }
}
