using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MvcMovie.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using MvcMovie.Data;

public class MoviesController : Controller
{
    private readonly ILogger<MoviesController> _logger;
    private readonly IWebHostEnvironment _hostingEnvironment;
    private readonly MoviesDbContext _moviescontext;
    private readonly string _jsonFilePath;

    public MoviesController(
        ILogger<MoviesController> logger,
        IWebHostEnvironment hostingEnvironment,
        MoviesDbContext moviescontext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        _moviescontext = moviescontext ?? throw new ArgumentNullException(nameof(moviescontext));
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

            // Retrieve existing movies from the database without tracking them
            var existingMovies = _moviescontext.Movies
                .AsNoTracking()
                .Where(m => movies.Select(j => j.ImdbId).Contains(m.ImdbId))
                .ToList();

            // Determine which movies are new
            var newMovies = movies
                .Where(m => !existingMovies.Any(e => e.ImdbId == m.ImdbId))
                .ToList();

            // Detach existing entities
            foreach (var entry in _moviescontext.ChangeTracker.Entries<Movie>().ToList())
            {
                if (existingMovies.Any(e => e.ImdbId == entry.Entity.ImdbId))
                {
                    _moviescontext.Entry(entry.Entity).State = EntityState.Detached;
                }
            }

            // Add new movies to the database
            foreach (var movie in newMovies)
            {
                // Check if the movie is already being tracked
                if (_moviescontext.ChangeTracker.Entries<Movie>().Any(e => e.Entity.ImdbId == movie.ImdbId))
                {
                    continue; // Skip adding this movie as it is already being tracked
                }

                _moviescontext.Movies.Add(movie);
            }

            _moviescontext.SaveChanges();

            return View(movies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching or saving movies.");
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

        var movie = _moviescontext.Movies.AsNoTracking().FirstOrDefault(m => m.ImdbId == imdbid);

        if (movie == null)
        {
            _logger.LogWarning("Movie with IMDb ID {ImdbId} not found.", imdbid);
            return NotFound();
        }

        return View(movie);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ImdbId,Title,Year,Score,ScoreAverage,Type,TmdbId,TraktId,MalId")] Movie movie)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Check if movie already exists
                var existingMovie = await _moviescontext.Movies
                    .AsNoTracking()
                    .FirstOrDefaultAsync(m => m.ImdbId == movie.ImdbId);

                if (existingMovie != null)
                {
                    _logger.LogWarning("Movie with IMDb ID {ImdbId} already exists.", movie.ImdbId);
                    ModelState.AddModelError("", "Movie with this IMDb ID already exists.");
                    return View(movie);
                }

                _moviescontext.Movies.Add(movie);
                await _moviescontext.SaveChangesAsync();

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

    public IActionResult Edit(string imdbid)
    {
        if (string.IsNullOrEmpty(imdbid))
        {
            _logger.LogWarning("Invalid IMDb ID provided for movie edit.");
            return BadRequest("Invalid IMDb ID.");
        }

        var movie = _moviescontext.Movies.AsNoTracking().FirstOrDefault(m => m.ImdbId == imdbid);

        if (movie == null)
        {
            _logger.LogWarning("Movie with IMDb ID {ImdbId} not found for edit.", imdbid);
            return NotFound();
        }

        return View(movie);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string imdbid, [Bind("ImdbId,Title,Year,Score,ScoreAverage,Type,TmdbId,TraktId,MalId")] Movie movie)
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
                var existingMovie = await _moviescontext.Movies
                    .FirstOrDefaultAsync(m => m.ImdbId == imdbid);

                if (existingMovie == null)
                {
                    _logger.LogWarning("Movie with IMDb ID {ImdbId} not found while updating.", imdbid);
                    return NotFound();
                }

                // Ensure that we are not tracking multiple instances
                _moviescontext.Entry(existingMovie).CurrentValues.SetValues(movie);
                await _moviescontext.SaveChangesAsync();

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
    public async Task<IActionResult> DeleteConfirmed(string imdbid)
    {
        if (string.IsNullOrEmpty(imdbid))
        {
            _logger.LogWarning("Invalid IMDb ID provided for movie deletion.");
            return BadRequest("Invalid IMDb ID.");
        }

        try
        {
            var movieToRemove = await _moviescontext.Movies.FindAsync(imdbid);

            if (movieToRemove == null)
            {
                _logger.LogWarning("Attempted to delete a movie with IMDb ID {ImdbId} that does not exist.", imdbid);
                return NotFound();
            }

            _moviescontext.Movies.Remove(movieToRemove);
            await _moviescontext.SaveChangesAsync();

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
