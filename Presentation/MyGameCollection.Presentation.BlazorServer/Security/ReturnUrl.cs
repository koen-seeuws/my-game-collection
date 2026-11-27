namespace MyGameCollection.Presentation.BlazorServer.Security;

public static class ReturnUrl
{
    /// <summary>The given URL when it stays on this site, otherwise the home page (prevents open redirects).</summary>
    public static string Local(string? url) =>
        !string.IsNullOrEmpty(url) && url[0] == '/' && (url.Length == 1 || (url[1] != '/' && url[1] != '\\'))
            ? url
            : "/";
}
