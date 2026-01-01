using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using AngleSharp;
using AngleSharp.Dom;
// <a href="../fotos1304/bu1304x002.jpg"><img src="../fotos1304/tn_bu1304x002.jpg" hspace="0" vspace="0" border="0" width="116" height="175"></a>
// <a href="bonus015/zbon15x001.jpg"><img src="bonus015/tn_zbon15x001.jpg" hspace="0" vspace="0" border="0" width="175" height="131"></a>

class Program
{
    static bool IsAlbumLink(string href)
    {
        //Console.WriteLine(href);
        if (href == null) return false;
        return href.Contains("pfoto") || href.Contains("bonus") || href.Contains("kshf");
    }

    static bool IsFotoLink(string href)
    {
        //Console.WriteLine(href);
        if (href == null) return false;
        return href.Contains("jpg") && (href.Contains("bonus") || href.Contains("fotos"));
    }

    static async Task DownloadAndSaveFoto(HttpClient client,  string href, string album, Config config)
    {
        var fotoUrl = config.MemberBaseUrl +href.Replace("../", "");

        //Console.WriteLine("Downloading: " + fotoUrl);

        var fotoResponse = await client.GetAsync(fotoUrl);
        var fotoBytes = await fotoResponse.Content.ReadAsByteArrayAsync();
        var fileName = href.Substring(href.LastIndexOf('/') + 1);
        // can the download path implemented as a "global" property of the main-class?

        var downloadPath = config.DownloadLocation + config.ModelName + "/" + album + "/";
        System.IO.Directory.CreateDirectory(downloadPath);
        fileName = downloadPath + fileName;
        await System.IO.File.WriteAllBytesAsync(fileName, fotoBytes);
        
        // Console.WriteLine("Saved: " + fileName);
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
    static List<string> albumLinks = new List<string>();
    static List<string> fotoLinks = new List<string>();
    static async Task Main()
    {

        var appConfig = new Config
        {
            BaseUrl = "https://www.southern-charms.com/",
            ModelName = "julimar",
            Username = "jomannx17",
            Password = "20postmannx17",
            Realm = "",
            RedirectUrl = "",
            //DownloadLocation = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/Downloads/AngelScraper/"
            DownloadLocation = "/Users/jmhl/.px/AngelScraper/"
        };
        
        appConfig.MemberBaseUrl = appConfig.BaseUrl + appConfig.ModelName + "/private/";
        appConfig.MemberMainUrl = appConfig.MemberBaseUrl + "members.htm";
        appConfig.loginUrl = appConfig.BaseUrl + "auth.form";

        using var client = CreateHttpClientWithCookies();        

        // get the main membership site that redirects to the login page if not authenticated. 
        // We need some form-data from the login page.
        var redirectResponse = await client.GetAsync(appConfig.MemberMainUrl);
        var redirectHtml = await redirectResponse.Content.ReadAsStringAsync();

        // Parse HTML with AngleSharp
        var config = Configuration.Default;
        var context = BrowsingContext.New(config);
        var loginForm = await context.OpenAsync(req => req.Content(redirectHtml));

        var inputs = loginForm.QuerySelectorAll("input");

        var formData = new Dictionary<string, string>();

        foreach (var input in inputs)
        {
            var name = input.GetAttribute("name");
            if (string.IsNullOrWhiteSpace(name))
                continue;

            var value = input.GetAttribute("value") ?? "";
            formData[name] = value;
        }
        formData["uid"] = appConfig.Username;
        formData["pwd"] = appConfig.Password;
        formData.Remove("rmb");
        formData.Remove("img");


        var loginContent = new FormUrlEncodedContent(formData);
        var loginResponse = await client.PostAsync(appConfig.loginUrl, loginContent);

        Console.WriteLine("Login status: " + loginResponse.StatusCode);

        // 3. Fetch main page (authenticated)
        var mainResponse = await client.GetAsync(appConfig.MemberMainUrl);
        var mainHtml = await mainResponse.Content.ReadAsStringAsync();

        // 4. Parse HTML with AngleSharp
        //var config = Configuration.Default;
        //var context = BrowsingContext.New(config);
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

        }

        foreach (var albumLink in albumLinks)
        {   
            var albumName = albumLink.Replace(".htm", "");

            var albumUrl = appConfig.MemberBaseUrl + albumLink;        
            var albumResponse = await client.GetAsync(albumUrl);
            var albumHtml = await albumResponse.Content.ReadAsStringAsync();

            // 4. Parse HTML with AngleSharp
            var album = await context.OpenAsync(req => req.Content(albumHtml));

            // Example: extract all links with a specific CSS selector
            //var links = document.QuerySelectorAll("a.some-class");
            var imageLinks = album.QuerySelectorAll("a[href]");

            foreach (var link in imageLinks)
            {
                var href = link.GetAttribute("href");

                if (href == null)
                    continue;

                if (!IsFotoLink(href))
                    continue;
                
                //fotoLinks.Add(href);
                //extract the name of the album to create a subfolder?  
                
                await DownloadAndSaveFoto(client, href, albumName, appConfig);   
            }          
        }

        Console.WriteLine("Done.");
    }
}