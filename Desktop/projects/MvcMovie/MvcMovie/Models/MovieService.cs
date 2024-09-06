using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MvcMovie.Models;

using MvcMovie.Data;

namespace MvcMovie.Services
{
    public class MovieService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<MovieService> _logger;
        private readonly MoviesDbContext _dbContext; // Use MoviesDbContext
        private readonly IWebHostEnvironment _hostingEnvironment;

        public MovieService(HttpClient httpClient, IConfiguration configuration, ILogger<MovieService> logger, MoviesDbContext dbContext, IWebHostEnvironment hostingEnvironment)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(_hostingEnvironment));
        }

        // Implement your methods
        public async Task ImportMoviesFromJsonFileAsync()
        {
            var filePath = _configuration["JsonFileSettings:FilePath"];
            
            

            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            {
                _logger.LogError("Invalid file path provided.");
                throw new ArgumentException("File path is invalid.", nameof(filePath));
            }

            try
            {
                var jsonData = await File.ReadAllTextAsync(filePath);
                var movieData = JsonConvert.DeserializeObject<MovieData>(jsonData);

                if (movieData == null || movieData.Search == null || !movieData.Search.Any())
                {
                    _logger.LogWarning("No movies found in the JSON file.");
                    return;
                }

                await _dbContext.Database.EnsureCreatedAsync();

                foreach (var movie in movieData.Search)
                {
                    if (!_dbContext.Movies.Any(m => m.ImdbId == movie.ImdbId))
                    {
                        _dbContext.Movies.Add(movie);
                    }
                }

                await _dbContext.SaveChangesAsync();
                _logger.LogInformation("Movies imported successfully from JSON file.");
            }
            catch (JsonSerializationException jsonEx)
            {
                _logger.LogError(jsonEx, "An error occurred while deserializing the JSON file.");
                throw;
            }
            catch (IOException ioEx)
            {
                _logger.LogError(ioEx, "An error occurred while reading the JSON file.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while importing movies from JSON file.");
                throw;
            }
        }




    }
}
