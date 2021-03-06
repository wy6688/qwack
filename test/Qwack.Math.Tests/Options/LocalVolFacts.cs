using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Qwack.Options;
using Xunit;
using Qwack.Options.VolSurfaces;

namespace Qwack.Math.Tests.Options
{
    public class LocalVolFacts
    {
        [Fact]
        public void ConstLV()
        {
            var constVol = 0.32;
            var originDate = new DateTime(2017, 02, 21);
            var impliedSurface = new ConstantVolSurface(originDate, constVol);

            var strikes = new double[3][] {
                new double[] { 1, 2 },
                new double[] { 1, 2 },
                new double[] { 1, 2 }
            };

            var timesteps = Enumerable.Range(0, 3).Select(x => (double)x / 3.0).ToArray();
            Func<double, double> fwdCurve = (t => { return 1.5; });
            var localVarianceGrid = impliedSurface.ComputeLocalVarianceOnGrid(strikes, timesteps, fwdCurve);

            for (var t = 0; t < localVarianceGrid.Length; t++)
            {
                var expectedLocalVariance = constVol * constVol;
                for (var k = 0; k < localVarianceGrid[t].Length; k++)
                {
                    Assert.Equal(expectedLocalVariance, localVarianceGrid[t][k]);
                }
            }
        }
    }
}
