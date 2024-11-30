using Dapper;
using Movies.Application.Database;
using Movies.Application.Models;

namespace Movies.Application.Repositories;

public class MovieSqlRepository(IDbConnectionFactory dbConnectionFactory) : IMovieSqlRepository
{
    public async Task<bool> CreateAsync(Movie movie)
    {
        var connection = await dbConnectionFactory.CreateConnectionAsync();

        using var transaction = connection.BeginTransaction();

        var result = await connection.ExecuteAsync(new CommandDefinition($"""
                  INSERT INTO movies (id, title, slug, yearofrelease)
                  VALUES (@Id, @Title, @Slug, @YearOfRelease)
                  """, movie));

        if (result <= 0) return false;
        foreach (var genre in movie.Genres)
        {
            await connection.ExecuteAsync(new CommandDefinition($"""
                                                                 INSERT INTO genres (movieId, name) 
                                                                 VALUES (@MovieId, @Name)
                                                                 """, new { MovieId = movie.Id, Name = genre }));
        }
        transaction.Commit();
        return true;
    }

    public async Task<IEnumerable<Movie>> GetAllAsync()
    {
        var connection = await dbConnectionFactory.CreateConnectionAsync();

        var result = await connection.QueryAsync(new CommandDefinition($"""
                                    SELECT m.*, string_agg(g.name, ',') as genres from movies m 
                                    left join genres g on m.id = g.movieid
                                    group by id
                                    """));

        return result.Select(x => new Movie
        {
            Id = x.id,
            Title = x.title,
            YearOfRelease = x.yearofrelease,
            Genres = Enumerable.ToList(x.genres.Split(','))
        });
    }

    public async Task<Movie?> GetByIdAsync(Guid id)
    {
        var connection = await dbConnectionFactory.CreateConnectionAsync();

        var result = await connection.QuerySingleOrDefaultAsync<Movie>(new CommandDefinition($"""
             SELECT * FROM movies WHERE id = @Id
             """, new {Id = id}));
        if (result == null) return null;

        var genres = await connection.QueryAsync<string>
            (new CommandDefinition($"""
                                    SELECT name FROM genres WHERE movieid = @Id
                                    """, new { Id = result.Id }));
        foreach (var genre in genres)
        {
            result.Genres.Add(genre);
        }            
        
        return result;
    }

    public async Task<Movie?> GetBySlugAsync(string slug)
    {
        var connection = await dbConnectionFactory.CreateConnectionAsync();

        var result = await connection.QuerySingleOrDefaultAsync<Movie>(new CommandDefinition($"""
             SELECT * FROM movies WHERE slug = @Slug
             """, new { Slug = slug }));
        
        if (result == null) return null;

        var genres = await connection.QueryAsync<string>(new CommandDefinition($"""
            SELECT name FROM genres WHERE movieId = @Id
            """, new { Id = result.Id }));

        foreach (var genre in genres)
        {
            result.Genres.Add(genre);
        }
        
        return result;
    }

    public async Task<bool> UpdateAsync(Movie movie)
    {
        var connection = await dbConnectionFactory.CreateConnectionAsync();
        if(!(await ExistAsync(movie.Id))) return false;
        
        using var transaction = connection.BeginTransaction();
        
        var result = await connection.ExecuteAsync(
            new CommandDefinition("DELETE FROM genres WHERE movieId = @Id", new { Id = movie.Id }));

        foreach (var genre in movie.Genres)
        {
            var genres = await connection.ExecuteAsync(new CommandDefinition(
            "INSERT INTO genres VALUES (@Id, @Name)", new { Id = movie.Id, Name = genre }));
        }

        var movieUpdates = await connection.ExecuteAsync(new CommandDefinition(
            "UPDATE movies SET title = @Title, slug = @Slug, yearofrelease = @YearOfRelease WHERE id = @Id",
            new {Id = movie.Id, Title = movie.Title, Slug = movie.Slug, YearOfRelease = movie.YearOfRelease}));
        
        transaction.Commit();
        return movieUpdates > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var connection = await dbConnectionFactory.CreateConnectionAsync();
        if(!(await ExistAsync(id))) return false;
        using var transaction = connection.BeginTransaction();
        
        await connection.ExecuteAsync(new CommandDefinition("DELETE FROM genres WHERE movieid = @Id", new { Id = id }));
        
        var result = await connection.ExecuteAsync(new CommandDefinition("DELETE FROM movies WHERE id = @Id", new { Id = id }));
        
        transaction.Commit();
        return result > 0;
    }

    public async Task<bool> ExistAsync(Guid id)
    {
        var connection = await dbConnectionFactory.CreateConnectionAsync();
        var result = await connection.ExecuteScalarAsync<bool>(new CommandDefinition($"""
                        SELECT COUNT(1) FROM movies WHERE id = @Id
                        """, new { Id = id }));

        return result;
    }
}