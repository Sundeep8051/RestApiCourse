using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Movies.Api.MappingConfig;
using Movies.Application.Repositories;
using Movies.Contracts.Requests;

namespace Movies.Api.Controllers;

[ApiController]
public class MoviesController(IMovieSqlRepository movieSqlRepository) : ControllerBase
{
    [HttpPost(ApiEndpoints.Movies.Create)]
    public async Task<IActionResult> Create([FromBody] CreateMovieRequest request)
    {
        var movie = request.MapToMovie();
        
        await movieSqlRepository.CreateAsync(movie);
        return CreatedAtAction(nameof(Get), new {idOrSlug = movie.Id},  movie.MapToMovieResponse());
    }

    [HttpGet(ApiEndpoints.Movies.Get)]
    public async Task<IActionResult> Get([FromRoute] string idOrSlug)
    {
        var movie = Guid.TryParse(idOrSlug, out var id) 
            ? await movieSqlRepository.GetByIdAsync(id)
            : await movieSqlRepository.GetBySlugAsync(idOrSlug);
        return Ok(movie?.MapToMovieResponse());
    }

    [HttpGet(ApiEndpoints.Movies.GetAll)]
    public async Task<IActionResult> GetAll()
    {
        var movies = await movieSqlRepository.GetAllAsync();
        return Ok(movies.MapToMoviesResponse());
    }

    [HttpPut(ApiEndpoints.Movies.Update)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateMovieRequest request)
    {
        var movie = request.MapToMovie(id);
        var result = await movieSqlRepository.UpdateAsync(movie);
        return result ? Ok(movie.MapToMovieResponse()) : NotFound();
    }

    [HttpDelete(ApiEndpoints.Movies.Delete)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var result = await movieSqlRepository.DeleteAsync(id);
        return result ? Ok() : NotFound();
    }
}