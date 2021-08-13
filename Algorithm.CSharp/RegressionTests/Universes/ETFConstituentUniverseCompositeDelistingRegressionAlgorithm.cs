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
using QuantConnect.Data.Market;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Interfaces;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// </summary>
    public class ETFConstituentUniverseCompositeDelistingRegressionAlgorithm : QCAlgorithm//, IRegressionAlgorithmDefinition
    {
        private Symbol _gdvd;
        private Symbol _aapl;
        private DateTime _delistingDate;
        
        /// <summary>
        /// Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        /// </summary>
        public override void Initialize()
        {
            SetStartDate(2020, 12, 1);
            SetEndDate(2021, 1, 31);
            SetCash(100000);
            
            UniverseSettings.Resolution = Resolution.Hour;
            
            _delistingDate = new DateTime(2021, 1, 20);

            _aapl = AddEquity("AAPL", Resolution.Hour).Symbol;
            _gdvd = AddEquity("GDVD", Resolution.Hour).Symbol;
            
            AddUniverse(new ETFConstituentsUniverse(_gdvd, UniverseSettings, FilterETFs));
        }

        private IEnumerable<Symbol> FilterETFs(IEnumerable<ETFConstituentData> constituents)
        {
            if (UtcTime > _delistingDate)
            {
                throw new Exception($"Performing constituent universe selection on {UtcTime:yyyy-MM-dd HH:mm:ss.fff} after composite ETF has been delisted");
            }

            return constituents.Select(x => x.Symbol);
        }

        /// <summary>
        /// OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.
        /// </summary>
        /// <param name="data">Slice object keyed by symbol containing the stock data</param>
        public override void OnData(Slice data)
        {
            if (UtcTime > _delistingDate && data.Keys.Any(x => x != _aapl))
            {
                throw new Exception($"Received unexpected slice in OnData(...) after universe was deselected");
            }
        }

        public override void OnSecuritiesChanged(SecurityChanges changes)
        {
            if (changes.AddedSecurities.Count != 0 && UtcTime > _delistingDate)
            {
                throw new Exception("New securities added after ETF constituents were delisted");
            }
        }

        public override void OnEndOfAlgorithm()
        {
            if (UniverseManager.Keys.Any(x => x.Underlying == _gdvd))
            {
                throw new Exception("ETF constituent universe was not removed from the algorithm after delisting");
            }
            if (Securities.Count > 2)
            {
                throw new Exception($"Expected less than 2 securities after algorithm ended, found {Securities.Count}");
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
