using Npgsql;

const string connectionString = "postgresql://postgres:WSFapuScvaFjIdMpBrsmjsDtGlaUmywj@yamanote.proxy.rlwy.net:46495/railway";

var ddl = """
CREATE TABLE IF NOT EXISTS products (
    id SERIAL PRIMARY KEY,
    code VARCHAR(35) NOT NULL UNIQUE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
""";

await using var connection = new NpgsqlConnection(connectionString);
await connection.OpenAsync();

await using var command = new NpgsqlCommand(ddl, connection);
var rows = await command.ExecuteNonQueryAsync();

Console.WriteLine("Table ensured. Result: " + rows);
