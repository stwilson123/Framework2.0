﻿using System;

namespace Framework.Caching
{
    public interface IAsyncTokenProvider {
        IVolatileToken GetToken(Action<Action<IVolatileToken>> task);
    }
}