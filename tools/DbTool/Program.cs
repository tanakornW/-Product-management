using Npgsql;

var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
    ?? args.FirstOrDefault()
    ?? throw new InvalidOperationException("Please provide a PostgreSQL connection string via args[0] or DB_CONNECTION_STRING.");

const string createTableSql = """
CREATE TABLE IF NOT EXISTS "Products" (
    "Id" SERIAL PRIMARY KEY,
    "Code" VARCHAR(35) NOT NULL UNIQUE,
    "CreatedAt" TIMESTAMPTZ NOT NULL DEFAULT NOW()
);
CREATE INDEX IF NOT EXISTS "IX_Products_Code" ON "Products"("Code");
""";

await using var connection = new NpgsqlConnection(connectionString);
await connection.OpenAsync();

await using (var command = new NpgsqlCommand(createTableSql, connection))
{
    await command.ExecuteNonQueryAsync();
    Console.WriteLine("Products table ensured successfully.");
}
