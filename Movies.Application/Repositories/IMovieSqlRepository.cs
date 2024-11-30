using Movies.Application.Models;

namespace Movies.Application.Repositories;

public interface IMovieSqlRepository
{
    Task<bool> CreateAsync(Movie movie);
    Task<IEnumerable<Movie>> GetAllAsync();
    Task<Movie?> GetByIdAsync(Guid id);
    Task<Movie?> GetBySlugAsync(string slug);
    Task<bool> UpdateAsync(Movie movie);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistAsync(Guid id);
}