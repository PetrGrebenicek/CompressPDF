using System.Net.Http;
using System.Reflection;
using System.Text.Json;

namespace CompressPDF
{
    public class Update
    {
        public const string RepoOwner = "PetrGrebenicek";
        public const string RepoName = "CompressPDF";
        public const string DownloadLink = $"https://github.com/{RepoOwner}/{RepoName}/releases/latest";

        public static Version GetCurrentVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }

        public static async Task<Version> GetLatestReleaseVersion()
        {
            string url = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.UserAgent.TryParseAdd("request");
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                using (JsonDocument doc = JsonDocument.Parse(responseBody))
                {
                    string latestVersionString = doc.RootElement.GetProperty("tag_name").GetString().TrimStart('v');
                    return new Version(latestVersionString);
                }
            }
        }

        public static bool IsUpdateAvailable(Version current, Version latest)
        {
            return latest > current;
        }
    }
}