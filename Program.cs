using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using AngleSharp;
using AngleSharp.Dom;

class Program
{
    static bool IsAlbumLink(string href)
    {
        if (href == null) return false;
        return href.Contains("pfoto") || href.Contains("bonus") || href.Contains("kshf");
    }

    static HttpClient CreateHttpClientWithCookies()
    {
        var handler = new HttpClientHandler
        {
            UseCookies = true,
            CookieContainer = new CookieContainer(),
            AllowAutoRedirect = true,
        };

        return new HttpClient(handler);
    }
    static async Task Main()
    {
        List<string> albumLinks = new List<string>();
    
        using var client = CreateHttpClientWithCookies();

        // 2. Login POST request
        var loginUrl = "https://www.southern-charms3.com/auth.form";
        var loginData = new Dictionary<string, string>
        {
            { "uid", "jomannx17" },
            { "pwd", "20postmannx17" },
            { "rlm", "Bustytina's Private Page" },
            { "for", "https%3a%2f%2fwww%2esouthern%2dcharms3%2ecom%2fbustytina%2fprivate%2fmembers%2ehtm" }
        };

        var loginContent = new FormUrlEncodedContent(loginData);
        var loginResponse = await client.PostAsync(loginUrl, loginContent);

        Console.WriteLine("Login status: " + loginResponse.StatusCode);

        // 3. Fetch main page (authenticated)
        var mainUrl = "https://www.southern-charms3.com/bustytina/private/members.htm";
        var mainResponse = await client.GetAsync(mainUrl);
        var mainHtml = await mainResponse.Content.ReadAsStringAsync();

        Console.WriteLine("Main page loaded.");

        // 4. Parse HTML with AngleSharp
        var config = Configuration.Default;
        var context = BrowsingContext.New(config);
        var document = await context.OpenAsync(req => req.Content(mainHtml));

        // Example: extract all links with a specific CSS selector
        //var links = document.QuerySelectorAll("a.some-class");
        var links = document.QuerySelectorAll("a[href]");

        foreach (var link in links)
        {
            var href = link.GetAttribute("href");

            if (href == null)
                continue;

            if (!IsAlbumLink(href))
                continue;
            
            albumLinks.Add(href);

            Console.WriteLine(href);
        }

        Console.WriteLine($"Found {links.Length} links.");

        // 5. Perform subsequent requests based on parsed links
        foreach (var link in links)
        {
            var href = link.GetAttribute("href");
            if (href == null)
                continue;


            var absoluteUrl = new Uri(new Uri(mainUrl), href).ToString();
            Console.WriteLine("Fetching: " + absoluteUrl);

            var subResponse = await client.GetAsync(absoluteUrl);
            var subHtml = await subResponse.Content.ReadAsStringAsync();

            // Parse sub-page if needed
            var subDoc = await context.OpenAsync(req => req.Content(subHtml));

            // Example: extract a title
            var title = subDoc.QuerySelector("h1")?.TextContent?.Trim();
            Console.WriteLine("Title: " + title);
        }

        Console.WriteLine("Done.");
    }
}