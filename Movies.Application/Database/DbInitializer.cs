using Dapper;

namespace Movies.Application.Database;

public class DbInitializer
{
    private readonly IDbConnectionFactory _connectionFactory;

    public DbInitializer(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task InitializeAsync()
    {
        using var connection = await _connectionFactory.CreateConnectionAsync();

        await connection.ExecuteAsync("""
              CREATE TABLE IF NOT EXISTS movies (
              id UUID PRIMARY KEY,
              title TEXT NOT NULL,
              slug TEXT NOT NULL,
              yearofrelease integer NOT NULL);
          """);

        await connection.ExecuteAsync("""
              CREATE UNIQUE INDEX CONCURRENTLY 
              IF NOT EXISTS movies_idx ON movies using btree(slug);
              """);

        await connection.ExecuteAsync("""
                                      CREATE TABLE IF NOT EXISTS genres (
                                          movieId UUID references movies (id),
                                          name TEXT NOT NULL);
                                      """);
    }
}