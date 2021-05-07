using Newtonsoft.Json;

namespace CoWinDiscord.Models
{
    public class District
    {
        [JsonProperty("district_id")]
        public int Id { get; set; }
        
        [JsonProperty("district_name")]
        public string Name { get; set; }
    }

    public class DistrictData
    {
        [JsonProperty("districts")]
        public District[] Districts { get; set; }
        
        public int ttl { get; set; }
    }
}