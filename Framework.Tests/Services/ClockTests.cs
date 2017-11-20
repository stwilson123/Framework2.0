using System;
using System.Threading;
using Framework.Tests.Stub;
using Xunit;

namespace Framework.Tests.Services
{
    public class ClockTests {
        [Fact]
        public void StubClockShouldComeFromSystemUtcAndDoesNotComeFromSystemTime() {
            var clock = new StubClock();
            var before = DateTime.UtcNow;
            Thread.Sleep(2);
            var mark = clock.UtcNow;
            Thread.Sleep(2);
            var after = DateTime.UtcNow;

            Assert.Equal(mark.Kind, DateTimeKind.Utc);
            Assert.NotInRange(mark, before, after);
        }

        [Fact]
        public void StubClockCanBeManuallyAdvanced() {
            var clock = new StubClock();
            var before = clock.UtcNow;
            clock.Advance(TimeSpan.FromMilliseconds(2));
            var mark = clock.UtcNow;
            clock.Advance(TimeSpan.FromMilliseconds(2));
            var after = clock.UtcNow;

            Assert.Equal(mark.Kind,  DateTimeKind.Utc);
            Assert.InRange(mark,  before, after);
            Assert.Equal(after.Subtract(before),  TimeSpan.FromMilliseconds(4));
        }
    }
}