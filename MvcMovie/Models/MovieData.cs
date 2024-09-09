using Newtonsoft.Json;
using System.Collections.Generic;
using MvcMovie.Models; // Ensure this matches the namespace where Movie is defined

namespace MvcMovie.Models
{
    public class MovieData
    {
        [JsonProperty("search")]
        public List<Movie> Search { get; set; }
    }
}
