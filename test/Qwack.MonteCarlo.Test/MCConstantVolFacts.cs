﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qwack.Paths;
using Qwack.Paths.Output;
using Qwack.Paths.Processes;
using Xunit;

namespace Qwack.MonteCarlo.Test
{
    public class MCConstantVolFacts
    {
        [Fact]
        public void PathsGenerated()
        {
            var engine = new PathEngine(2 << 8);
            engine.AddPathProcess(new Random.MersenneTwister.MersenneTwister64()
            {
                 UseNormalInverse = true
            });
            var asset = new ConstantVolSingleAsset
                (
                    startDate : DateTime.Now.Date,
                    expiry: DateTime.Now.Date.AddDays(50),
                    vol: 0.30,
                    spot: 1000,
                    drift: 0.00,
                    numberOfSteps: 25,
                    name: "TestAsset"
                );
            engine.AddPathProcess(asset);
            var asset2 = new ConstantVolSingleAsset
                (
                    startDate: DateTime.Now.Date,
                    expiry: DateTime.Now.Date.AddDays(50),
                    vol: 0.30,
                    spot: 500,
                    drift: 0.10,
                    numberOfSteps: 25,
                    name: "TestAsset"
                );
            engine.AddPathProcess(asset2);
            engine.SetupFeatures();
            engine.RunProcess();
            var output = new OutputPathsToImage(engine);

        }
    }
}