using Movies.Application.Models;
using Movies.Contracts.Requests;
using Movies.Contracts.Responses;

namespace Movies.Api.MappingConfig;

public static class Mappers
{
    public static Movie MapToMovie(this CreateMovieRequest request)
    {
        var movie = new Movie()
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            YearOfRelease = request.YearOfRelease,
            Genres = request.Genres.ToList(),
        };
        return movie;
    }
    
    public static Movie MapToMovie(this UpdateMovieRequest request, Guid id)
    {
        var movie = new Movie()
        {
            Id = id,
            Title = request.Title,
            YearOfRelease = request.YearOfRelease,
            Genres = request.Genres.ToList(),
        };
        return movie;
    }

    public static MovieResponse MapToMovieResponse(this Movie movie)
    {
        var movieResponse = new MovieResponse
        {
            Id = movie.Id,
            Title = movie.Title,
            YearOfRelease = movie.YearOfRelease,
            Genres = movie.Genres,
        };
        return movieResponse;
    }

    public static MoviesResponse MapToMoviesResponse(this IEnumerable<Movie> movies)
    {
        return new MoviesResponse
        {
            Movies = movies.Select(MapToMovieResponse)
        };
    }
}