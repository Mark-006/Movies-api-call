using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MvcMovie.Models;
using Microsoft.AspNetCore.Authorization;


public class MoviesController : Controller
{
    private readonly ILogger<MoviesController> _logger;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly string _jsonFilePath;

    public MoviesController(ILogger<MoviesController> logger, IWebHostEnvironment hostingEnvironment)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        _jsonFilePath = Path.Combine(_hostingEnvironment.ContentRootPath, "JsonFile", "File.json");
    }

    private JArray LoadMoviesFromJson()
    {
        if (!System.IO.File.Exists(_jsonFilePath))
        {
            _logger.LogError("The JSON file was not found at the specified path: {Path}", _jsonFilePath);
            throw new FileNotFoundException("The JSON file was not found.", _jsonFilePath);
        }

        var json = System.IO.File.ReadAllText(_jsonFilePath);
        var jsonObject = JObject.Parse(json);
        return (JArray)jsonObject["search"];
    }

    private void SaveMoviesToJson(JArray movies)
    {
        var jsonObject = new JObject { ["search"] = movies };
        System.IO.File.WriteAllText(_jsonFilePath, jsonObject.ToString());
    }

    public IActionResult Index()
    {
        try
        {
            var movies = LoadMoviesFromJson().Select(j => new Movie
            {
                ImdbId = (string?)j["imdbid"] ?? string.Empty,
                Title = (string?)j["title"] ?? string.Empty,
                Year = (int?)j["year"] ?? 0,
                Score = (decimal?)j["score"] ?? 0,
                ScoreAverage = (decimal?)j["score_average"] ?? 0,
                Type = (string?)j["type"] ?? string.Empty,
                TmdbId = (int?)j["tmdbid"],
                TraktId = (int?)j["traktid"],
                MalId = (int?)j["malid"]
            }).ToList();

            return View(movies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching movies.");
            return View("Error");
        }
    }

    public IActionResult Details(string imdbid)
    {
        if (string.IsNullOrEmpty(imdbid))
        {
            _logger.LogWarning("Invalid IMDb ID provided for movie details.");
            return BadRequest("Invalid IMDb ID.");
        }

        var movies = LoadMoviesFromJson();
        var movie = movies.FirstOrDefault(j => (string?)j["imdbid"] == imdbid);

        if (movie == null)
        {
            _logger.LogWarning("Movie with IMDb ID {ImdbId} not found.", imdbid);
            return NotFound();
        }

        var movieModel = new Movie
        {
            ImdbId = (string?)movie["imdbid"],
            Title = (string?)movie["title"],
            Year = (int?)movie["year"] ?? 0,
            Score = (decimal?)movie["score"] ?? 0,
            ScoreAverage = (decimal?)movie["score_average"] ?? 0,
            Type = (string?)movie["type"],
            TmdbId = (int?)movie["tmdbid"],
            TraktId = (int?)movie["traktid"],
            MalId = (int?)movie["malid"]
        };

        return View(movieModel);
    }

    public IActionResult Create()
    {
        return View();
    }

    // POST: Movies/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create([Bind("ImdbId,Title,Year,Score,ScoreAverage,Type,TmdbId,TraktId,MalId")] Movie movie)
    {
        if (ModelState.IsValid) ;
        {
            try
            {
                var movies = LoadMoviesFromJson();

                // Check if a movie with the same IMDb ID already exists
                if (movies.Any(j => (string?)j["imdbid"] == movie.ImdbId))
                {
                    _logger.LogWarning("Movie with IMDb ID {ImdbId} already exists.", movie.ImdbId);
                    ModelState.AddModelError("", "Movie with this IMDb ID already exists.");
                    return View(movie);
                }

                // Create a new JObject for the new movie
                var newMovie = new JObject
                {
                    ["imdbid"] = movie.ImdbId,
                    ["title"] = movie.Title,
                    ["year"] = movie.Year,
                    ["score"] = movie.Score,
                    ["score_average"] = movie.ScoreAverage,
                    ["type"] = movie.Type,
                    ["tmdbid"] = movie.TmdbId,
                    ["traktid"] = movie.TraktId,
                    ["malid"] = movie.MalId
                };

                // Add the new movie to the JSON array and save
                
                var jsonArray = LoadMoviesFromJson();
                jsonArray.Add(newMovie);
                SaveMoviesToJson(jsonArray);

                // Redirect to the index page upon successful creation
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log the exception and add a model error
                _logger.LogError(ex, "An error occurred while creating the movie.");
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }
        }

        // Return the view with the model if validation fails or an exception occurs
        return View(movie);
    }

   
   

    public IActionResult Edit(string imdbid)
    {
        if (string.IsNullOrEmpty(imdbid))
        {
            _logger.LogWarning("Invalid IMDb ID provided for movie edit.");
            return BadRequest("Invalid IMDb ID.");
        }

        var movies = LoadMoviesFromJson();
        var movie = movies.FirstOrDefault(j => (string?)j["imdbid"] == imdbid);

        if (movie == null)
        {
            _logger.LogWarning("Movie with IMDb ID {ImdbId} not found for edit.", imdbid);
            return NotFound();
        }

        var movieModel = new Movie
        {
            ImdbId = (string?)movie["imdbid"] ?? string.Empty,
            Title = (string?)movie["title"] ?? string.Empty,
            Year = (int?)movie["year"] ?? 0,
            Score = (decimal?)movie["score"] ?? 0,
            ScoreAverage = (decimal?)movie["score_average"] ?? 0,
            Type = (string?)movie["type"] ?? string.Empty,
            TmdbId = (int?)movie["tmdbid"],
            TraktId = (int?)movie["traktid"],
            MalId = (int?)movie["malid"]
        };

        return View(movieModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Edit(string imdbid, [Bind("ImdbId,Title,Year,Score,ScoreAverage,Type,TmdbId,TraktId,MalId")] Movie movie)
    {
        if (imdbid != movie.ImdbId)
        {
            _logger.LogWarning("IMDb ID mismatch: URL IMDb ID {UrlImdbId} does not match the movie IMDb ID {ModelImdbId}.", imdbid, movie.ImdbId);
            return BadRequest("IMDb ID mismatch.");
        }

        if (ModelState.IsValid)
        {
            try
            {
                var movies = LoadMoviesFromJson();
                var existingMovie = movies.FirstOrDefault(j => (string?)j["imdbid"] == imdbid);

                if (existingMovie == null)
                {
                    _logger.LogWarning("Movie with IMDb ID {ImdbId} not found while updating.", imdbid);
                    return NotFound();
                }

                existingMovie["title"] = movie.Title;
                existingMovie["year"] = movie.Year;
                existingMovie["score"] = movie.Score;
                existingMovie["score_average"] = movie.ScoreAverage;
                existingMovie["type"] = movie.Type;
                existingMovie["tmdbid"] = movie.TmdbId;
                existingMovie["traktid"] = movie.TraktId;
                existingMovie["malid"] = movie.MalId;

                SaveMoviesToJson(movies);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the movie with IMDb ID {ImdbId}.", movie.ImdbId);
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }
        }

        return View(movie);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public IActionResult DeleteConfirmed(string imdbid)
    {
        if (string.IsNullOrEmpty(imdbid))
        {
            _logger.LogWarning("Invalid IMDb ID provided for movie deletion.");
            return BadRequest("Invalid IMDb ID.");
        }

        try
        {
            var movies = LoadMoviesFromJson();
            var movieToRemove = movies.FirstOrDefault(j => (string?)j["imdbid"] == imdbid);

            if (movieToRemove == null)
            {
                _logger.LogWarning("Attempted to delete a movie with IMDb ID {ImdbId} that does not exist.", imdbid);
                return NotFound();
            }

            movies.Remove(movieToRemove);
            SaveMoviesToJson(movies);

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting the movie with IMDb ID {ImdbId}.", imdbid);
            ModelState.AddModelError("", "Unable to delete movie. Try again, and if the problem persists, see your system administrator.");
        }

        return RedirectToAction(nameof(Index));
    }
}
