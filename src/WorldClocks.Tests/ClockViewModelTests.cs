using WorldClocks.ViewModels;

namespace WorldClocks.Tests;

/// <summary>
/// Exercises ClockViewModel.ComputeDisplay with synthetic UTC instants and custom, fixed-offset
/// TimeZoneInfo instances so results never depend on the host machine's clock or installed timezone data.
/// </summary>
public class ClockViewModelTests
{
    [Fact]
    public void ComputeDisplay_Uses24HourFormat_WhenRequested()
    {
        var utc = new DateTimeOffset(2026, 3, 15, 13, 5, 30, TimeSpan.Zero);
        var zone = TimeZoneInfo.CreateCustomTimeZone("Test/UTC+0", TimeSpan.Zero, "Test UTC", "Test UTC");

        var (timeText, dateBadge) = ClockViewModel.ComputeDisplay(zone, utc, utc.Date, use24HourTime: true);

        Assert.Equal("13:05:30", timeText);
        Assert.Null(dateBadge);
    }

    [Fact]
    public void ComputeDisplay_Uses12HourFormatWithAmPm_WhenRequested()
    {
        var utc = new DateTimeOffset(2026, 3, 15, 13, 5, 30, TimeSpan.Zero);
        var zone = TimeZoneInfo.CreateCustomTimeZone("Test/UTC+0", TimeSpan.Zero, "Test UTC", "Test UTC");

        var (timeText, _) = ClockViewModel.ComputeDisplay(zone, utc, utc.Date, use24HourTime: false);

        // PM designator is pulled from the current culture rather than hardcoded, so this stays
        // correct regardless of which locale the build/test machine is running under.
        var pmDesignator = System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.PMDesignator;
        Assert.Equal($"1:05:30 {pmDesignator}", timeText);
    }

    [Fact]
    public void ComputeDisplay_ShowsYesterdayBadge_WhenZoneIsBehindObserverAcrossMidnight()
    {
        // Observer's local date is the 16th; a zone far behind UTC is still on the 15th.
        var utc = new DateTimeOffset(2026, 3, 16, 3, 0, 0, TimeSpan.Zero);
        var observerLocalDate = new DateTime(2026, 3, 16);
        var behindZone = TimeZoneInfo.CreateCustomTimeZone("Test/UTC-10", TimeSpan.FromHours(-10), "Test Behind", "Test Behind");

        var (_, dateBadge) = ClockViewModel.ComputeDisplay(behindZone, utc, observerLocalDate, use24HourTime: true);

        Assert.Equal("Yesterday", dateBadge);
    }

    [Fact]
    public void ComputeDisplay_ShowsTomorrowBadge_WhenZoneIsAheadOfObserverAcrossMidnight()
    {
        // Observer's local date is the 15th; a zone far ahead of UTC has already rolled to the 16th.
        var utc = new DateTimeOffset(2026, 3, 15, 22, 0, 0, TimeSpan.Zero);
        var observerLocalDate = new DateTime(2026, 3, 15);
        var aheadZone = TimeZoneInfo.CreateCustomTimeZone("Test/UTC+10", TimeSpan.FromHours(10), "Test Ahead", "Test Ahead");

        var (_, dateBadge) = ClockViewModel.ComputeDisplay(aheadZone, utc, observerLocalDate, use24HourTime: true);

        Assert.Equal("Tomorrow", dateBadge);
    }

    [Fact]
    public void ComputeDisplay_ShowsNoBadge_WhenSameCalendarDate()
    {
        var utc = new DateTimeOffset(2026, 3, 15, 12, 0, 0, TimeSpan.Zero);
        var observerLocalDate = new DateTime(2026, 3, 15);
        var zone = TimeZoneInfo.CreateCustomTimeZone("Test/UTC+2", TimeSpan.FromHours(2), "Test", "Test");

        var (_, dateBadge) = ClockViewModel.ComputeDisplay(zone, utc, observerLocalDate, use24HourTime: true);

        Assert.Null(dateBadge);
    }
}
