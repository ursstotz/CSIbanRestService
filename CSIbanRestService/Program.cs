using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add OpenAPI/Swagger support
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Endpoint: Fix IBAN (calculate correct mod 97 check digits)
app.MapGet("/iban/fix", (string iban) =>
{
    string normalized = iban.Replace(" ", "").ToUpper();

    if (normalized.Length < 5)
    {
        return Results.BadRequest("Invalid IBAN format");
    }

    string countryCode = normalized.Substring(0, 2);
    string bban = normalized.Substring(4);
    string rearranged = bban + countryCode + "00";

    // Convert letters to numbers
    string numericIban = "";
    foreach (char c in rearranged)
    {
        if (char.IsLetter(c))
            numericIban += (c - 'A' + 10).ToString();
        else
            numericIban += c;
    }

    int checkDigits = 98 - Mod97(numericIban);
    string validIban = countryCode + checkDigits.ToString("D2") + bban;

    return Results.Ok(validIban);
});

// Endpoint: Version
app.MapGet("/version", () =>
{
    var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
    return Results.Ok(version);
});

app.Run();

// Helper function for mod 97 calculation
static int Mod97(string input)
{
    int checksum = 0;
    foreach (char c in input)
    {
        int digit = c - '0';
        checksum = (checksum * 10 + digit) % 97;
    }
    return checksum;
}