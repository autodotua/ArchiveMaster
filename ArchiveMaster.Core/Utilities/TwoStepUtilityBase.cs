﻿using System;
using System.Threading;
using System.Threading.Tasks;
using ArchiveMaster.Configs;

namespace ArchiveMaster.Utilities
{
    public abstract class TwoStepUtilityBase<TConfig> : UtilityBase<TConfig> where TConfig : ConfigBase
    {
        public TwoStepUtilityBase(TConfig config, AppConfig appConfig) : base(config, appConfig)
        {
        }

        public abstract Task ExecuteAsync(CancellationToken token = default);

        public abstract Task InitializeAsync(CancellationToken token = default);
    }
}