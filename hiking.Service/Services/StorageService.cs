using System.Net.Http.Headers;
using hikingService.Options;

namespace hikingService.Services;

public class StorageService(HttpClient http, SupabaseOptions options)
{
    private readonly string _baseUrl = options.Url;
    private readonly string _key     = options.ServiceKey;

    public async Task<string> UploadAsync(string bucket, string path, Stream stream, string contentType)
    {
        var streamContent = new StreamContent(stream);
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        var request = new HttpRequestMessage(HttpMethod.Post,
            $"{_baseUrl}/storage/v1/object/{bucket}/{path}")
        {
            Content = streamContent,
        };
        request.Headers.Add("Authorization", $"Bearer {_key}");
        request.Headers.Add("x-upsert", "true");

        var res = await http.SendAsync(request);
        if (!res.IsSuccessStatusCode)
        {
            var body = await res.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Storage {(int)res.StatusCode}: {body}");
        }

        return $"{_baseUrl}/storage/v1/object/public/{bucket}/{path}";
    }
}