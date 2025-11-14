using backend.Models;

namespace backend.Data;

public static class ProductDataSeeder
{
    private static readonly string[] SampleCodes =
    [
        "ABCDE-FGHIJ-KLMNO-PQRST-UVWXY-Z1234",
        "1A2B3-C4D5E-F6G7H-I8J9K-L0M1N-O2P3Q",
        "ZXCVB-NMASD-FGHJK-LQWER-TYUIO-P1234",
    ];

    public static void Seed(ProductDbContext dbContext)
    {
        if (dbContext.Products.Any())
        {
            return;
        }

        var now = DateTime.UtcNow;
        var products = SampleCodes.Select(code => new Product
        {
            Code = code,
            CreatedAt = now,
        });

        dbContext.Products.AddRange(products);
        dbContext.SaveChanges();
    }
}

