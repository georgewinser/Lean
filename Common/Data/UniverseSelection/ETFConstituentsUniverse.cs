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
            var guid = Guid.NewGuid().ToString();
            var universeTicker = _etfConstituentsUniverseIdentifier + '-' + guid;
            
            // The universe might get mapped, but the ID Symbol won't, which
            // will always be the universe ticker.
            if (compositeSymbol.ID.Symbol == universeTicker)
            {
                return compositeSymbol;
            }
            
            return new Symbol(
                SecurityIdentifier.GenerateConstituentIdentifier(
                    universeTicker,
                    compositeSymbol.SecurityType,
                    compositeSymbol.ID.Market),
                universeTicker,
                compositeSymbol);
        }
    }
}
