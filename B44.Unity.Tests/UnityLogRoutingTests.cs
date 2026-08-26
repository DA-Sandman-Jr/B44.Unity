using B44.Common.Diagnostics;
using B44.Unity.Diagnostics;
using Xunit;

namespace B44.Unity.Tests;

/// <summary>
/// The severity rule, exercised without a Unity runtime. What this does not
/// cover is that <c>Debug.LogError</c> is actually reached — that is the
/// proving project's job, and no assertion here should be read as covering it.
/// </summary>
public sealed class UnityLogRoutingTests
{
    [Theory]
    [InlineData(LogSeverity.Debug, UnityLogChannel.Log)]
    [InlineData(LogSeverity.Info, UnityLogChannel.Log)]
    [InlineData(LogSeverity.Warning, UnityLogChannel.Warning)]
    [InlineData(LogSeverity.Error, UnityLogChannel.Error)]
    public void ChannelFor_MapsEachSeverityToItsChannel(LogSeverity severity, UnityLogChannel expected)
    {
        Assert.Equal(expected, UnityLogRouting.ChannelFor(severity));
    }

    [Fact]
    public void ChannelFor_TreatsAnUnknownHigherSeverityAsAnError()
    {
        // The reason the rule uses thresholds rather than equality. A severity
        // above Error must not silently fall back to the ordinary log channel,
        // which is what an exact-match implementation would do the day one is
        // added to B44.Common.
        const LogSeverity beyondError = (LogSeverity)(LogSeverity.Error + 1);

        Assert.Equal(UnityLogChannel.Error, UnityLogRouting.ChannelFor(beyondError));
    }
}
