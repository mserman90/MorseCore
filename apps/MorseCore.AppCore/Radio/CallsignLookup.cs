using System.Collections.Concurrent;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace MorseCore.AppCore.Radio;

public sealed record CallsignLookupResult(
    string CallSign,
    string? Name = null,
    string? Qth = null,
    string? Country = null,
    string? CountryId = null,
    string? GridSquare = null,
    double? Latitude = null,
    double? Longitude = null,
    bool IsOnlineMatch = false,
    string Source = "Offline"
);

public static partial class CallsignLookupService
{
    private static readonly ConcurrentDictionary<string, CallsignLookupResult> Cache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromSeconds(5) };

    public static async Task<CallsignLookupResult> LookupAsync(string callsign, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(callsign))
            throw new ArgumentException("Callsign cannot be empty.", nameof(callsign));

        string normalized = callsign.Trim().ToUpperInvariant();

        if (Cache.TryGetValue(normalized, out var cached))
            return cached;

        // Step 1: Perform fast offline validation & country extraction
        CallSignInfo info;
        try
        {
            info = CallSigns.Parse(normalized);
        }
        catch (ArgumentException)
        {
            return new CallsignLookupResult(normalized, Source: "Invalid");
        }

        string? countryName = info.CountryId switch
        {
            "TR" => "Turkey",
            "DE" => "Germany",
            "RU" => "Russian Federation",
            "US" => "United States",
            "GB" => "United Kingdom",
            "SA" => "Saudi Arabia",
            "AE" => "United Arab Emirates",
            _ => null
        };

        string? qth = null;
        if (info.CountryId == "TR" && info.Region != null)
        {
            string? area = CallSigns.TurkishArea(info.Region);
            if (area != null) qth = area.Replace("@EUROPE@", "Avrupa").Replace("@ASIA@", "Asya").Replace("@ISLANDS@", "Adalar");
        }

        var offlineResult = new CallsignLookupResult(
            CallSign: info.Normalized,
            Country: countryName,
            CountryId: info.CountryId,
            Qth: qth,
            IsOnlineMatch: false,
            Source: "Offline Catalog"
        );

        // Step 2: Query public HamQTH / open callsign directory if available
        try
        {
            string url = $"https://www.hamqth.com/dxcc.php?callsign={Uri.EscapeDataString(info.BaseCall)}";
            using var response = await HttpClient.GetAsync(url, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                string content = await response.Content.ReadAsStringAsync(cancellationToken);
                var onlineResult = ParseHamQthResponse(info.Normalized, content, offlineResult);
                if (onlineResult != null)
                {
                    Cache.TryAdd(normalized, onlineResult);
                    return onlineResult;
                }
            }
        }
        catch (Exception)
        {
            // Network failure: fallback to offline result gracefully
        }

        Cache.TryAdd(normalized, offlineResult);
        return offlineResult;
    }

    public static void ClearCache() => Cache.Clear();

    private static CallsignLookupResult? ParseHamQthResponse(string callsign, string responseXml, CallsignLookupResult fallback)
    {
        if (string.IsNullOrWhiteSpace(responseXml)) return null;

        string? name = ExtractTag(responseXml, "name");
        string? qth = ExtractTag(responseXml, "qth") ?? fallback.Qth;
        string? country = ExtractTag(responseXml, "country") ?? fallback.Country;
        string? grid = ExtractTag(responseXml, "grid");

        double? lat = null, lon = null;
        if (grid != null && grid.Length >= 4)
        {
            try
            {
                var point = Locator.Centre(grid);
                lat = point.Latitude;
                lon = point.Longitude;
            }
            catch (ArgumentException) { }
        }

        return fallback with
        {
            Name = name,
            Qth = qth,
            Country = country,
            GridSquare = grid ?? fallback.GridSquare,
            Latitude = lat ?? fallback.Latitude,
            Longitude = lon ?? fallback.Longitude,
            IsOnlineMatch = name != null || grid != null,
            Source = "HamQTH"
        };
    }

    private static string? ExtractTag(string xml, string tag)
    {
        var match = Regex.Match(xml, $"<{tag}>(.*?)</{tag}>", RegexOptions.IgnoreCase);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }
}
