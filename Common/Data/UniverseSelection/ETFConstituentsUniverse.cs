using System;
using System.Collections.Generic;
using System.Linq;

namespace QuantConnect.Data.UniverseSelection
{
    public class ETFConstituentsUniverse : ConstituentsUniverse<ETFConstituentData>
    {
        private const string _etfConstituentsUniverseIdentifier = "qc-universe-etf-constituents";
        
        public ETFConstituentsUniverse(Symbol symbol, UniverseSettings universeSettings, Func<IEnumerable<ETFConstituentData>, IEnumerable<Symbol>> constituentsFilter = null)
            : base(CreateConstituentUniverseETFSymbol(symbol), universeSettings, constituentsFilter ?? (constituents => constituents.Select(c => c.Symbol)))
        {
        }

        private static Symbol CreateConstituentUniverseETFSymbol(Symbol compositeSymbol)
        {
            if (compositeSymbol.Value == _etfConstituentsUniverseIdentifier)
            {
                return compositeSymbol;
            }
            
            return new Symbol(
                SecurityIdentifier.GenerateConstituentIdentifier(
                    _etfConstituentsUniverseIdentifier,
                    compositeSymbol.SecurityType,
                    compositeSymbol.ID.Market),
                _etfConstituentsUniverseIdentifier,
                compositeSymbol);
        }
    }
}
