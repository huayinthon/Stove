﻿using Stove.Log;

using Xunit;

namespace Stove.NLog.Tests
{
    public class NLog_Tests : NLogTestBase
    {
        public NLog_Tests()
        {
            Building(builder => { }).Ok();
        }

        [Fact]
        public void should_work()
        {
            The<ILogger>().Debug("Log Me!");
            The<ILogger>().Warn("Log Me!");
            The<ILogger>().Error("Log Me!");
            The<ILogger>().Info("Log Me!");
        }
    }
}
