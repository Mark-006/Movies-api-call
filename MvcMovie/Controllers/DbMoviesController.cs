using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MvcMovie.Data;
using MvcMovie.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;


public class DbMoviesController : Controller
{
    private readonly ILogger<DbMoviesController> _logger;
    private readonly MoviesDbContext _moviescontext;

    public DbMoviesController(ILogger<DbMoviesController> logger, MoviesDbContext moviescontext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _moviescontext = moviescontext ?? throw new ArgumentNullException(nameof(moviescontext));
    }

    // GET: DbMovies
    public async Task<IActionResult> Index()
    {
        try
        {
            var movies = await _moviescontext.Movies
                .Select(movie => new Movie
                {
                    ImdbId = movie.ImdbId,
                    Title = movie.Title,
                    Year = movie.Year ?? 0,  // Default to 0 if null
                    Score = movie.Score ?? 0, // Default to 0 if null
                    ScoreAverage = movie.ScoreAverage ?? 0, // Default to 0 if null
                    Type = movie.Type ?? "Unknown", // Default to "Unknown" if null
                    TmdbId = movie.TmdbId ?? 0, // Default to 0 if null
                    TraktId = movie.TraktId ?? 0, // Default to 0 if null
                    MalId = movie.MalId ?? 0 // Default to 0 if null
                })
                .ToListAsync();

            return View(movies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching movies. Check for null values in the database.");
            return View("Error");
        }
    }



    // GET: DbMovies/Details/5
    public async Task<IActionResult> Details(string imdbid)
    {
        if (string.IsNullOrEmpty(imdbid))
        {
            _logger.LogWarning("Invalid IMDb ID provided for movie details.");
            return BadRequest("Invalid IMDb ID.");
        }

        try
        {
            var movie = await _moviescontext.Movies
                .Where(m => m.ImdbId == imdbid)
                .SingleOrDefaultAsync();

            if (movie == null)
            {
                _logger.LogWarning("Movie with IMDb ID {ImdbId} not found.", imdbid);
                return NotFound();
            }

            // Ensure that the model properties are correctly initialized
            movie.Score = movie.Score ?? 0.0m;
            movie.ScoreAverage = movie.ScoreAverage ?? 0.0m;
            movie.Year = movie.Year ?? 0;
            movie.TmdbId = movie.TmdbId ?? 0;
            movie.TraktId = movie.TraktId ?? 0;
            movie.MalId = movie.MalId ?? 0;
            movie.Type = movie.Type ?? "Unknown";

            return View(movie);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching details for movie with IMDb ID {ImdbId}.", imdbid);
            return View("Error");
        }
    }






    // GET: DbMovies/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: DbMovies/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("ImdbId,Title,Year,Score,ScoreAverage,Type,TmdbId,TraktId,MalId")] Movie movie)
    {
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state while creating a movie.");
            return View(movie);
        } 

        try
        {
            if (await _moviescontext.Movies.AnyAsync(m => m.ImdbId == movie.ImdbId))
            {
                _logger.LogWarning("Movie with IMDb ID {ImdbId} already exists.", movie.ImdbId);
                ModelState.AddModelError("", "Movie with this IMDb ID already exists.");
                return View(movie);
            }

            // Ensure Score and ScoreAverage are not null
            movie.Score = movie.Score ?? 0.0m;
            movie.ScoreAverage = movie.ScoreAverage ?? 0.0m;

            _moviescontext.Movies.Add(movie);
            await _moviescontext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Movie created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while creating the movie.");
            ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            return View(movie);
        }
    }

    // GET: DbMovies/Edit/5
    public async Task<IActionResult> Edit(string imdbid)
    {
        if (string.IsNullOrEmpty(imdbid))
        {
            _logger.LogWarning("Invalid IMDb ID provided for movie edit.");
            return BadRequest("Invalid IMDb ID.");
        }

        try
        {
            var movie = await _moviescontext.Movies.FindAsync(imdbid);

            if (movie == null)
            {
                _logger.LogWarning("Movie with IMDb ID {ImdbId} not found for edit.", imdbid);
                return NotFound();
            }

            return View(movie);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while fetching movie for edit with IMDb ID {ImdbId}.", imdbid);
            return View("Error");
        }
    }

    // POST: DbMovies/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string imdbid, [Bind("ImdbId,Title,Year,Score,ScoreAverage,Type,TmdbId,TraktId,MalId")] Movie movie)
    {
        if (imdbid != movie.ImdbId)
        {
            _logger.LogWarning("IMDb ID mismatch: URL IMDb ID {UrlImdbId} does not match the movie IMDb ID {ModelImdbId}.", imdbid, movie.ImdbId);
            return BadRequest("IMDb ID mismatch.");
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state while editing a movie.");
            return View(movie);
        }

        try
        {
            var existingMovie = await _moviescontext.Movies.FindAsync(imdbid);

            if (existingMovie == null)
            {
                _logger.LogWarning("Movie with IMDb ID {ImdbId} not found while updating.", imdbid);
                return NotFound();
            }

            // Update properties, handling null values with default values
            existingMovie.Title = movie.Title ?? existingMovie.Title; // Keep existing title if null
            existingMovie.Year = movie.Year ?? 0; // Default to 0 if Year is null
            existingMovie.Score = movie.Score ?? 0.0m; // Default to 0.0m if Score is null
            existingMovie.ScoreAverage = movie.ScoreAverage ?? 0.0m; // Default to 0.0m if ScoreAverage is null
            existingMovie.Type = movie.Type ?? "Unknown"; // Default to "Unknown" if Type is null
            existingMovie.TmdbId = movie.TmdbId ?? 0; // Default to 0 if TmdbId is null
            existingMovie.TraktId = movie.TraktId ?? 0; // Default to 0 if TraktId is null
            existingMovie.MalId = movie.MalId ?? 0; // Default to 0 if MalId is null

            // Save changes
            _moviescontext.Movies.Update(existingMovie);
            await _moviescontext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Movie updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogError(ex, "A concurrency error occurred while updating the movie with IMDb ID {ImdbId}.", movie.ImdbId);
            ModelState.AddModelError("", "Unable to save changes due to a concurrency issue. Try again, and if the problem persists, see your system administrator.");
            return View(movie);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating the movie with IMDb ID {ImdbId}.", movie.ImdbId);
            ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            return View(movie);
        }
    }


    // POST: DbMovies/Delete/5
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
            var movie = await _moviescontext.Movies.FindAsync(imdbid);

            if (movie == null)
            {
                _logger.LogWarning("Attempted to delete a movie with IMDb ID {ImdbId} that does not exist.", imdbid);
                return NotFound();
            }

            _moviescontext.Movies.Remove(movie);
            await _moviescontext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Movie deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting the movie with IMDb ID {ImdbId}.", imdbid);
            ModelState.AddModelError("", "Unable to delete the movie. Try again, and if the problem persists, see your system administrator.");
            return View("Error");
        }
    }
}
