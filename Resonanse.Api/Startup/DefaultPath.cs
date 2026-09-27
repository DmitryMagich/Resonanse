namespace Resonanse.Api.Startup;

public static class DefaultPaths
{
    public static string GetMusicFolder()
    {
        var myMusic = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);
        if (!string.IsNullOrEmpty(myMusic) && Directory.Exists(myMusic))
            return myMusic;

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, "Music");
    }

    public static string GetDefaultPeerName()
    {
        return $"{Environment.UserName}-desktop";
    }
}