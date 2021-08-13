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
using QuantConnect.Interfaces;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// Tests a custom filter function when creating an ETF constituents universe for SPY
    /// </summary>
    public class ETFConstituentUniverseFilterFunctionRegressionAlgorithm : QCAlgorithm//, IRegressionAlgorithmDefinition
    {
        private Symbol _aapl;
        private Symbol _spy;
        private bool _filtered;
        private bool _securitiesChanged;
        private bool _receivedData;
        
        /// <summary>
        /// Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        /// </summary>
        public override void Initialize()
        {
            SetStartDate(2020, 12, 1);
            SetEndDate(2021, 1, 31);
            SetCash(100000);

            UniverseSettings.Resolution = Resolution.Hour;
            
            _spy = AddEquity("SPY", Resolution.Daily).Symbol;
            _aapl = QuantConnect.Symbol.Create("AAPL", SecurityType.Equity, Market.USA);
            
            AddUniverse(new ETFConstituentsUniverse(_spy, UniverseSettings, FilterETFs));
        }

        /// <summary>
        /// Filters ETFs, performing some sanity checks
        /// </summary>
        /// <param name="constituents">Constituents of the ETF universe added above</param>
        /// <returns>Constituent Symbols to add to algorithm</returns>
        /// <exception cref="ArgumentException">Constituents collection was not structured as expected</exception>
        private IEnumerable<Symbol> FilterETFs(IEnumerable<ETFConstituentData> constituents)
        {
            var constituentsData = constituents.ToHashSet();
            var constituentsSymbols = constituentsData.Select(x => x.Symbol).ToList();
            if (constituentsData.Count == 0)
            {
                throw new ArgumentException($"Constituents collection is empty on {UtcTime:yyyy-MM-dd HH:mm:ss.fff}");
            }
            if (!constituentsSymbols.Contains(_aapl))
            {
                throw new ArgumentException("AAPL is not in the constituents data provided to the algorithm");
            }

            var aaplData = constituentsData.Single(x => x.Symbol == _aapl);
            if (aaplData.Weight == 0m)
            {
                throw new ArgumentException("AAPL weight is expected to be a non-zero value");
            }

            _filtered = true;
            return constituentsSymbols;
        }

        /// <summary>
        /// OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.
        /// </summary>
        /// <param name="data">Slice object keyed by symbol containing the stock data</param>
        public override void OnData(Slice data)
        {
            if (!_filtered && data.Bars.Count != 0 && data.Bars.ContainsKey(_aapl))
            {
                throw new Exception("AAPL TradeBar data added to algorithm before constituent universe selection took place");
            }
            if (data.Bars.Count != 0 && !data.Bars.ContainsKey(_aapl))
            {
                throw new Exception($"Expected AAPL TradeBar data in OnData on {UtcTime:yyyy-MM-dd HH:mm:ss}");
            }

            _receivedData = true;
        }

        /// <summary>
        /// Checks if new securities have been added to the algorithm after universe selection has occurred
        /// </summary>
        /// <param name="changes">Security changes</param>
        /// <exception cref="ArgumentException">Expected number of stocks were not added to the algorithm</exception>
        public override void OnSecuritiesChanged(SecurityChanges changes)
        {
            if (_filtered && !_securitiesChanged && changes.AddedSecurities.Count < 500)
            {
                throw new ArgumentException($"Added SPY S&P 500 ETF to algorithm, but less than 500 equities were loaded (added {changes.AddedSecurities.Count} securities)");
            }

            _securitiesChanged = true;
        }

        /// <summary>
        /// Ensures that all expected events were triggered by the end of the algorithm
        /// </summary>
        /// <exception cref="Exception">An expected event didn't happen</exception>
        public override void OnEndOfAlgorithm()
        {
            if (!_filtered)
            {
                throw new Exception("Universe selection was never triggered");
            }
            if (!_securitiesChanged)
            {
                throw new Exception("Security changes never propagated to the algorithm");
            }
            if (!_receivedData)
            {
                throw new Exception("Data was never loaded for the S&P 500 constituent AAPL");
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
