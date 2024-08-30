
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MvcMovie.Models;
using MvcMovie.Services;
using MvcMovie.Data;
using Microsoft.EntityFrameworkCore;

public class MoviesController : Controller
{
    private readonly MovieService _movieService;
    private readonly ILogger<MoviesController> _logger;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly MoviesDbContext _context;



    public MoviesController(MovieService movieService, ILogger<MoviesController> logger,
        IWebHostEnvironment hostingEnvironment, MoviesDbContext context)
    {
        _movieService = movieService ?? throw new ArgumentNullException(nameof(movieService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _hostingEnvironment = hostingEnvironment;
        _context = context;
    }

    // Loads movies from a JSON file and returns them as a JArray
    private JArray LoadMoviesFromJson()
    {
        //var jsonFilePath = "C:\\Users\\Mark.Kiniu\\Desktop\\projects\\MvcMovie\\MvcMovie\\JsonFile\\File.json";
        var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "JsonFile", "File.json");
        if (!System.IO.File.Exists(filePath))
        {
            _logger.LogError("The JSON file was not found at the specified path: {Path}", filePath);
            throw new FileNotFoundException("The JSON file was not found.", filePath);
        }

        var json = System.IO.File.ReadAllText(filePath);
        var jsonObject = JObject.Parse(json);
        return (JArray)jsonObject["search"];
    }

    // Displays movies loaded from the JSON file
    public IActionResult MoviesFromJson()
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

    // GET: Movies
    public IActionResult Index()
    {
        try
        {
            var movies = LoadMoviesFromJson().Select(j => new Movie
            {
                ImdbId = (string?)j["imdbid"],
                Title = (string?)j["title"],
                Year = (int?)j["year"] ?? 0,
                Score = (decimal?)j["score"] ?? 0,
                ScoreAverage = (decimal?)j["score_average"] ?? 0,
                Type = (string?)j["type"],
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
    //public async Task<IActionResult> AllMovies()
    //{
    //    return await LoadAndMergeMovies();
    //}

    public async Task<IActionResult> AllMovies()
    {
        try
        {
            // Fetch the list of movies from the database
            var movies = await _context.Movies.ToListAsync();

            // Load movies from the JSON file
            var jsonMovies = LoadMoviesFromJson().Select(j => new Movie
            {
                ImdbId = (string?)j["imdbid"] ?? string.Empty,
                Title = (string?)j["title"] ?? string.Empty,
                Year = (int?)j["year"] ?? 0,
                Score = (decimal?)j["score"] ?? 0,
                ScoreAverage = (decimal?)j["score_average"] ?? 0,
                Type = (string?)j["type"] ?? string.Empty,
                TmdbId = (int?)j["tmdbid"],
                TraktId = (int?)j["traktid"],
                MalId = (int?)j["malid"],
                IsSaved = false // Default to false
            }).ToList();

            // Iterate through the JSON movies to find matching movies in the database
            foreach (var jsonMovie in jsonMovies)
            {
                var matchingMovie = movies.FirstOrDefault(m => m.ImdbId == jsonMovie.ImdbId);

                if (matchingMovie != null)
                {
                    // If a matching movie is found, set IsSaved to true
                    matchingMovie.IsSaved = true;
                }
                else
                {
                    // If no matching movie is found, add the JSON movie to the list
                    movies.Add(jsonMovie);
                }
            }

            // Return the updated list of movies to the view
            return View(movies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching movies.");
            return View("Error");
        }
    }


    // GET: Movies/Details/5
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

    // GET: Movies/Edit/5
    [HttpGet]
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

    // POST: Movies/Edit/5
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

                // Update movie logic here - typically involves saving back to JSON file
                // Implement file update logic if needed

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

    // GET: Movies/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Movies/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create([Bind("ImdbId,Title,Year,Score,ScoreAverage,Type,TmdbId,TraktId,MalId")] Movie movie)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var movies = LoadMoviesFromJson();
                if (movies.Any(j => (string?)j["imdbid"] == movie.ImdbId))
                {
                    _logger.LogWarning("Movie with IMDb ID {ImdbId} already exists.", movie.ImdbId);
                    return BadRequest("Movie with this IMDb ID already exists.");
                }

                // Add movie logic here - typically involves appending to JSON file
                // Implement file update logic if needed

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the movie.");
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }
        }

        return View(movie);
    }

    // POST: Movies/Delete/5
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
            var movie = movies.FirstOrDefault(j => (string?)j["imdbid"] == imdbid);

            if (movie == null)
            {
                _logger.LogWarning("Attempted to delete a movie with IMDb ID {ImdbId} that does not exist.", imdbid);
                return NotFound();
            }

            // Delete movie logic here - typically involves removing from JSON file
            // Implement file update logic if needed

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting the movie with IMDb ID {ImdbId}.", imdbid);
            ModelState.AddModelError("", "Unable to delete the movie. Try again, and if the problem persists, see your system administrator.");
            return View();
        }
    }

    // POST: Movies/ImportFromJson
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ImportFromJson()
    {
        try
        {
            _movieService.ImportMoviesFromJsonFileAsync(); // Ensure this method is compatible with your new JSON handling logic
            _logger.LogInformation("Movies successfully imported from JSON file.");
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while importing movies from JSON file.");
            ModelState.AddModelError("", "Unable to import movies. Please check the log for more details.");
            return View("Error");
        }
    }
}
