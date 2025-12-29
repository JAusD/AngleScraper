# AngelScraper AI Coding Guidelines

## Project Overview
AngelScraper is a C# console application for web scraping authenticated websites. It uses HttpClient for HTTP requests with cookie-based session management and AngleSharp for HTML parsing and DOM manipulation.

## Architecture Patterns
- **Single-entry console app**: All logic resides in `Program.cs`. No separate classes or modules for simplicity.
- **Async-first**: All I/O operations use `async/await` pattern for non-blocking execution.
- **Session management**: Use `HttpClientHandler` with `CookieContainer` and `UseCookies = true` for maintaining authenticated sessions across requests.

## Key Implementation Patterns
- **Authentication**: Perform login via POST request using `FormUrlEncodedContent` with a `Dictionary<string, string>` of form fields.
  ```csharp
  var loginData = new Dictionary<string, string> { { "uid", "username" }, { "pwd", "password" } };
  var content = new FormUrlEncodedContent(loginData);
  await client.PostAsync(loginUrl, content);
  ```
- **HTML Parsing**: Use AngleSharp's `BrowsingContext` to parse HTML strings into DOM documents.
  ```csharp
  var context = BrowsingContext.New(Configuration.Default);
  var document = await context.OpenAsync(req => req.Content(htmlString));
  ```
- **URL Resolution**: Construct absolute URLs from relative links using `Uri` constructor.
  ```csharp
  var absoluteUrl = new Uri(new Uri(baseUrl), relativeHref).ToString();
  ```
- **Link Extraction**: Query DOM with CSS selectors to extract elements, e.g., `document.QuerySelectorAll("a[href]")`.

## Development Workflow
- **Build**: `dotnet build` in the project root.
- **Run**: `dotnet run` to execute the scraper.
- **Dependencies**: Managed via NuGet; AngleSharp is the primary external library for HTML parsing.

## Code Style Conventions
- **Target Framework**: .NET 10.0
- **Language Features**: Enable `ImplicitUsings` and `Nullable` in project file.
- **Error Handling**: Basic try-catch not implemented; rely on HttpClient's exception throwing for failed requests.
- **Output**: Use `Console.WriteLine` for logging progress and results.

## Integration Points
- **External Sites**: Designed for scraping sites requiring form-based authentication (e.g., southern-charms3.com).
- **No APIs**: Direct HTTP requests only; no REST APIs or databases involved.</content>
<parameter name="filePath">/Users/jmhl/dev/AngelScraper/.github/copilot-instructions.md