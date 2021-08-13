/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Data;
using QuantConnect.Data.UniverseSelection;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// Basic template algorithm simply initializes the date range and cash. This is a skeleton
    /// framework you can use for designing an algorithm.
    /// </summary>
    /// <meta name="tag" content="using data" />
    /// <meta name="tag" content="using quantconnect" />
    /// <meta name="tag" content="trading and orders" />
    public class ETFConstituentUniverseMappedCompositeRegressionAlgorithm: QCAlgorithm//, IRegressionAlgorithmDefinition
    {
        private Symbol _aapl;
        private Symbol _qqq;
        private Dictionary<DateTime, int> _filterDateConstituentSymbolCount = new Dictionary<DateTime, int>();
        private Dictionary<DateTime, bool> _constituentDataEncountered = new Dictionary<DateTime, bool>();
        private HashSet<Symbol> _constituentSymbols = new HashSet<Symbol>();
        
        /// <summary>
        /// Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        /// </summary>
        public override void Initialize()
        {
            SetStartDate(2011, 1, 1);
            SetEndDate(2011, 4, 4);
            SetCash(100000);

            UniverseSettings.Resolution = Resolution.Hour;

            _aapl = QuantConnect.Symbol.Create("AAPL", SecurityType.Equity, Market.USA);
            _qqq = AddEquity("QQQ", Resolution.Hour).Symbol;
            AddUniverse(new ETFConstituentsUniverse(_qqq, UniverseSettings, FilterETFs));
        }

        private IEnumerable<Symbol> FilterETFs(IEnumerable<ETFConstituentData> constituents)
        {
            var constituentSymbols = constituents.Select(x => x.Symbol).ToHashSet();
            if (!constituentSymbols.Contains(_aapl))
            {
                throw new Exception("AAPL not found in QQQ constituents");
            }
            
            _filterDateConstituentSymbolCount[UtcTime] = constituentSymbols.Count;
            foreach (var symbol in constituentSymbols)
            {
                _constituentSymbols.Add(symbol);
            }
            
            return constituentSymbols;
        }

        /// <summary>
        /// OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.
        /// </summary>
        /// <param name="data">Slice object keyed by symbol containing the stock data</param>
        public override void OnData(Slice data)
        {
            if (!_constituentDataEncountered.ContainsKey(UtcTime.Date))
            {
                _constituentDataEncountered[UtcTime.Date] = false;
            }

            if (_constituentSymbols.Intersect(data.Keys).Any())
            {
                _constituentDataEncountered[UtcTime.Date] = true;
            }
        }

        public override void OnEndOfAlgorithm()
        {
            if (_filterDateConstituentSymbolCount.Count != 3)
            {
                throw new Exception($"ETF constituent filtering function was not called 3 times (actual: {_filterDateConstituentSymbolCount.Count}");
            }

            foreach (var kvp in _filterDateConstituentSymbolCount)
            {
                if (kvp.Value < 25)
                {
                    throw new Exception($"Expected 25 or more constituents in filter function on {kvp.Key:yyyy-MM-dd HH:mm:ss.fff}, found {kvp.Value}");
                }
            }

            if (!_constituentDataEncountered.Values.All(x => x))
            {
                throw new Exception("Received data in OnData(...) but it did not contain any constituent data on that day");
            }
        }

        /// <summary>
        /// This is used by the regression test system to indicate if the open source Lean repository has the required data to run this algorithm.
        /// </summary>
        public bool CanRunLocally { get; } = true;

        /// <summary>
        /// This is used by the regression test system to indicate which languages this algorithm is written in.
        /// </summary>
        public Language[] Languages { get; } = { Language.CSharp, Language.Python };

        /// <summary>
        /// This is used by the regression test system to indicate what the expected statistics are from running the algorithm
        /// </summary>
        public Dictionary<string, string> ExpectedStatistics => new Dictionary<string, string>
        {
            {"Total Trades", "1"},
            {"Average Win", "0%"},
            {"Average Loss", "0%"},
            {"Compounding Annual Return", "271.453%"},
            {"Drawdown", "2.200%"},
            {"Expectancy", "0"},
            {"Net Profit", "1.692%"},
            {"Sharpe Ratio", "8.888"},
            {"Probabilistic Sharpe Ratio", "67.609%"},
            {"Loss Rate", "0%"},
            {"Win Rate", "0%"},
            {"Profit-Loss Ratio", "0"},
            {"Alpha", "-0.005"},
            {"Beta", "0.996"},
            {"Annual Standard Deviation", "0.222"},
            {"Annual Variance", "0.049"},
            {"Information Ratio", "-14.565"},
            {"Tracking Error", "0.001"},
            {"Treynor Ratio", "1.978"},
            {"Total Fees", "$3.44"},
            {"Estimated Strategy Capacity", "$56000000.00"},
            {"Lowest Capacity Asset", "SPY R735QTJ8XC9X"},
            {"Fitness Score", "0.248"},
            {"Kelly Criterion Estimate", "0"},
            {"Kelly Criterion Probability Value", "0"},
            {"Sortino Ratio", "79228162514264337593543950335"},
            {"Return Over Maximum Drawdown", "93.728"},
            {"Portfolio Turnover", "0.248"},
            {"Total Insights Generated", "0"},
            {"Total Insights Closed", "0"},
            {"Total Insights Analysis Completed", "0"},
            {"Long Insight Count", "0"},
            {"Short Insight Count", "0"},
            {"Long/Short Ratio", "100%"},
            {"Estimated Monthly Alpha Value", "$0"},
            {"Total Accumulated Estimated Alpha Value", "$0"},
            {"Mean Population Estimated Insight Value", "$0"},
            {"Mean Population Direction", "0%"},
            {"Mean Population Magnitude", "0%"},
            {"Rolling Averaged Population Direction", "0%"},
            {"Rolling Averaged Population Magnitude", "0%"},
            {"OrderListHash", "9e4bfd2eb0b81ee5bc1b197a87ccedbe"}
        };
    }
}
