using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Qwack.Core.Basic;
using Qwack.Core.Cubes;
using Qwack.Core.Instruments;
using Qwack.Core.Models;
using Qwack.Models.Models;
using Qwack.Models.Risk.Mutators;
using Qwack.Utils.Parallel;

namespace Qwack.Models.Risk
{
    public class RiskLadder
    {
        public string AssetId { get; private set; }
        public MutationType ShiftType { get; private set; }
        public RiskMetric Metric { get; private set; }
        public double ShiftSize { get; private set; }
        public int NScenarios { get; private set; }
        public bool ReturnDifferential { get; private set; }

        public RiskLadder(string assetId, MutationType shiftType, RiskMetric metric, double shiftStepSize, int nScenarios, bool returnDifferential=true)
        {
            AssetId = assetId;
            ShiftType = shiftType;
            Metric = metric;
            ShiftSize = shiftStepSize;
            NScenarios = nScenarios;
            ReturnDifferential = returnDifferential;
        }

        public Dictionary<string,IAssetFxModel> GenerateScenarios(IAssetFxModel model)
        {
            var o = new Dictionary<string, IAssetFxModel>();

            var results = new KeyValuePair<string, IAssetFxModel>[NScenarios * 2 + 1];
            ParallelUtils.Instance.For(-NScenarios, NScenarios + 1, 1, (i) =>
            //for(var i=-NScenarios;i<=NScenarios;i++)
            {
                var thisShift = i * ShiftSize;
                var thisLabel = AssetId + "~" + thisShift;
                if (thisShift == 0)
                    results[i+NScenarios] = new KeyValuePair<string, IAssetFxModel>(thisLabel, model);
                else
                {
                    IAssetFxModel shifted;
                    switch (ShiftType)
                    {
                        case MutationType.FlatShift:
                            shifted = FlatShiftMutator.AssetCurveShift(AssetId, thisShift, model);
                            break;
                        default:
                            throw new Exception($"Unable to process shift type {ShiftType}");
                    }
                    results[i + NScenarios] = new KeyValuePair<string, IAssetFxModel>(thisLabel, shifted);
                }
            }).Wait();

            foreach (var kv in results)
                o.Add(kv.Key, kv.Value);

            return o;
        }

        public ICube Generate(IAssetFxModel model, Portfolio portfolio)
        {
            var o = new ResultCube();
            o.Initialize(new Dictionary<string, Type> { { "Scenario", typeof(string) } });

            var scenarios = GenerateScenarios(model);

            ICube baseRiskCube = null;
            if(ReturnDifferential)
            {
                baseRiskCube = GetRisk(model, portfolio);
            }

            var threadLock = new object();
            var results = new ICube[scenarios.Count];
            var scList = scenarios.ToList();

            ParallelUtils.Instance.For(0, scList.Count,1, i =>
            //foreach (var scenario in scenarios)
            {
                var scenario = scList[i];
                var result = GetRisk(scenario.Value, portfolio);

                if (ReturnDifferential)
                {
                    result = result.Difference(baseRiskCube);
                }

                results[i] = result;
            }).Wait();

            for (var i = 0; i < results.Length; i++)
            {
                o = (ResultCube)o.Merge(results[i], 
                    new Dictionary<string, object> { { "Scenario", scList[i].Key } }, null, true);
            }

            return o;
        }

        private ICube GetRisk(IAssetFxModel model, Portfolio portfolio)
        {
            switch (Metric)
            {
                case RiskMetric.AssetCurveDelta:
                    return portfolio.AssetDelta(model);
                default:
                    throw new Exception($"Unable to process risk metric {Metric}");

            }
        }
    }
}