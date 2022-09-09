using DotNext.Net.Cluster;
using Microsoft.AspNetCore.Connections;

namespace ClusterExample.Raft.Configuration
{
    public class VisualStudioConfiguration : IHostConfiguration
    {
        public bool IsColdStart => true;

        public Uri Url { get; private set; }

        public IEnumerable<Uri> Members { get; private set; }

        public Task InitializeAsync(string[] args)
        {
            var urls = GetUrls().ToDictionary(x => x.Scheme, StringComparer.InvariantCultureIgnoreCase);

            if (urls.TryGetValue("https", out var https))
            {
                Url = https;
            }
            else if (urls.TryGetValue("http", out var http))
            {
                Url = http;
            }
            else
            {
                throw new Exception("Please specify a http or https URL.");
            }

            Members = new[] { Url };

            return Task.CompletedTask;
        }

        private IEnumerable<Uri> GetUrls()
        {
            var urls = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");

            if (string.IsNullOrWhiteSpace(urls)) throw new Exception("Please specify a application URL by setting the ASPNETCORE_URLS environment variable.");

            foreach (var url in urls.Split(';'))
            {
                if (!Uri.TryCreate(url, UriKind.Absolute, out var result)) throw new Exception($"'{url}' is not a valid URL.");

                yield return result;
            }
        }
    }
}
