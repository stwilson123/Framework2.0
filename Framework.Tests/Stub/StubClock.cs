﻿using System;
using Framework.Caching;
using Framework.Services;

namespace Framework.Tests.Stub
{
    public class StubClock : IClock {

        public StubClock()
            : this(new DateTime(2009, 10, 14, 12, 34, 56, DateTimeKind.Utc)) {
        }

        public StubClock(DateTime utcNow) {
            UtcNow = utcNow;
        }

        public DateTime UtcNow { get; private set; }

        public void Advance(TimeSpan span) {
            UtcNow = UtcNow.Add(span);
        }

        public DateTime FutureMoment(TimeSpan span) {
            return UtcNow.Add(span);
        }

        public IVolatileToken When(TimeSpan duration) {
            return new Clock.AbsoluteExpirationToken(this, duration);
        }

        public IVolatileToken WhenUtc(DateTime absoluteUtc) {
            return new Clock.AbsoluteExpirationToken(this, absoluteUtc);
        }
    }
}