using WorldClocks.Models;
using WorldClocks.Services;

namespace WorldClocks.Tests;

public class SettingsServiceTests
{
    [Fact]
    public void Load_ReturnsDefaultsAndFirstRun_WhenFileDoesNotExist()
    {
        var service = new SettingsService(TempFilePath());

        var (settings, isFirstRun) = service.Load();

        Assert.True(isFirstRun);
        Assert.Empty(settings.Clocks);
        Assert.True(settings.AutoStartEnabled);
    }

    [Fact]
    public void SaveThenLoad_RoundTripsAllFields()
    {
        var filePath = TempFilePath();
        try
        {
            var service = new SettingsService(filePath);
            var original = new AppSettings
            {
                AutoStartEnabled = false,
                Use24HourTime = true,
                Clocks =
                {
                    new ClockEntry { TimeZoneId = "Pacific Standard Time", Title = "Los Angeles", Subtitle = "Dave, Priya" },
                    new ClockEntry { TimeZoneId = "UTC" },
                },
            };

            service.Save(original);
            var (loaded, isFirstRun) = service.Load();

            Assert.False(isFirstRun);
            Assert.False(loaded.AutoStartEnabled);
            Assert.True(loaded.Use24HourTime);
            Assert.Equal(2, loaded.Clocks.Count);
            Assert.Equal("Los Angeles", loaded.Clocks[0].Title);
            Assert.Equal("Dave, Priya", loaded.Clocks[0].Subtitle);
            Assert.Equal("Pacific Standard Time", loaded.Clocks[0].TimeZoneId);
            Assert.Null(loaded.Clocks[1].Title);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    [Fact]
    public void Load_FallsBackToDefaults_WhenFileIsCorrupt()
    {
        var filePath = TempFilePath();
        try
        {
            File.WriteAllText(filePath, "{ this is not valid json ");
            var service = new SettingsService(filePath);

            var (settings, isFirstRun) = service.Load();

            Assert.True(isFirstRun);
            Assert.Empty(settings.Clocks);
        }
        finally
        {
            File.Delete(filePath);
        }
    }

    private static string TempFilePath() =>
        Path.Combine(Path.GetTempPath(), $"world-clocks-tests-{Guid.NewGuid():N}.json");
}
