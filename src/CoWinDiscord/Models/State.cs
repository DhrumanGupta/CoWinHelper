using Newtonsoft.Json;

namespace CoWinDiscord.Models
{
    public class State
    {
        [JsonProperty("state_id")]
        public int Id { get; set; }
        
        [JsonProperty("state_name")]
        public string Name { get; set; }
    }

    public class StateData
    {
        public State[] States { get; set; }
    }
}