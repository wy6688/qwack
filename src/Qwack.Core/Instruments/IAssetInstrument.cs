using System;
using System.Collections.Generic;
using System.Text;
using Qwack.Core.Basic;
using Qwack.Core.Models;

namespace Qwack.Core.Instruments
{
    public interface IAssetInstrument : IInstrument
    {
        string[] AssetIds { get; }
        string[] IrCurves { get; }
        Currency Currency { get; }
        
        Dictionary<string, List<DateTime>> PastFixingDates(DateTime valDate);
        FxConversionType FxType(IAssetFxModel model);
        string FxPair(IAssetFxModel model);

        IAssetInstrument Clone();
        IAssetInstrument SetStrike(double strike);

        Currency PaymentCurrency { get; }
    }
}
