using FluentAssertions;
using TRC.Application.Common;
using TRC.Application.Options;
using Xunit;

namespace TRC.Tests;

public class BookingRulesTests
{
    // Client sign-off: 20-min session + 10-min buffer, 4 slots, ending inside the 16:00-18:00 window.
    [Fact]
    public void Slots_match_the_client_approved_schedule()
    {
        var o = new BookingOptions();   // defaults == the signed-off rules
        var expected = new[]
        {
            ("16:00", "16:20"),
            ("16:30", "16:50"),
            ("17:00", "17:20"),
            ("17:30", "17:50"),
        };

        for (var i = 0; i < o.MaxBookingsPerDay; i++)
        {
            var start = o.WindowStartTime.AddMinutes(i * o.SlotStrideMinutes);
            var end = start.AddMinutes(o.SessionMinutes);

            start.ToString("HH:mm").Should().Be(expected[i].Item1);
            end.ToString("HH:mm").Should().Be(expected[i].Item2);
        }

        // The last session must finish on or before the window closes.
        var lastEnd = o.WindowStartTime
            .AddMinutes((o.MaxBookingsPerDay - 1) * o.SlotStrideMinutes)
            .AddMinutes(o.SessionMinutes);
        lastEnd.Should().BeOnOrBefore(o.WindowEndTime);
    }

    [Fact]
    public void Friday_is_the_only_closed_day()
    {
        var o = new BookingOptions();
        o.IsClosedOn(DayOfWeek.Friday).Should().BeTrue();
        o.IsClosedOn(DayOfWeek.Saturday).Should().BeFalse();
        o.IsClosedOn(DayOfWeek.Sunday).Should().BeFalse();
    }

    // A blocked number must not be able to slip back in by reformatting itself.
    [Theory]
    [InlineData("01799707090")]
    [InlineData("+8801799707090")]
    [InlineData("8801799707090")]
    [InlineData("+880 1799-707090")]
    [InlineData("00880 1799 707090")]
    public void Phone_numbers_collapse_to_one_canonical_form(string input)
    {
        PhoneNumber.Normalize(input).Should().Be("+8801799707090");
        PhoneNumber.IsPlausible(PhoneNumber.Normalize(input)).Should().BeTrue();
    }
}
