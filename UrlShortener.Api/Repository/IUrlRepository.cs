namespace UrlShortener.Api.Repository;

public interface IUrlRepository
{
    bool TryGetLongUrl(string code, out string longUrl);
}