﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trady.Core;
using Trady.Core.Helper;
using Trady.Strategy.Helper;
using Trady.Strategy.Rule;

namespace Trady.Strategy
{
    public class Portfolio
    {
        private IList<Equity> _equities;
        private IRule<EquityCandle> _buyRule;
        private IRule<EquityCandle> _sellRule;

        public Portfolio(IList<Equity> equities, IRule<EquityCandle> buyRule, IRule<EquityCandle> sellRule)
        {
            _equities = equities;
            _buyRule = buyRule;
            _sellRule = sellRule;
        }

        public async Task<PortfolioResult> RunAsync(decimal principal, decimal premium = 1.0m, DateTime? startTime = null, DateTime? endTime = null)
        {
            return await Task.Factory.StartNew(() =>
            {
                var principalForEachCandleTimeSeries = principal / _equities.Count;

                var portfolioResultList = new Dictionary<Equity, IDictionary<DateTime, decimal>>();

                foreach (var equity in _equities)
                {
                    decimal cash = principalForEachCandleTimeSeries;
                    decimal asset = 0;

                    var transactions = new Dictionary<DateTime, decimal>();

                    int startIndex = startTime.HasValue ? equity.FindFirstIndexOrDefault(c => c.DateTime >= startTime).Value : 0;
                    int endIndex = endTime.HasValue ? equity.FindLastIndexOrDefault(c => c.DateTime < endTime).Value : equity.Count - 1;
                    int? prevBuyIndex = null;

                    Console.WriteLine();
                    for (int i = startIndex; i <= endIndex; i++)
                    {
                        if (_buyRule.IsValid(equity.GetCandleAt(i)) && !prevBuyIndex.HasValue)
                        {
                            transactions.Add(equity[i].DateTime, -cash);
                            asset = cash - premium;
                            cash = 0;

                            prevBuyIndex = i;
                            Console.WriteLine("Buy on {0} @ {1:yyyy-MM-dd}", equity[i].Close, equity[i].DateTime);
                        }
                        else if (_sellRule.IsValid(equity.GetCandleAt(i)) && prevBuyIndex.HasValue)
                        {
                            decimal percent = (equity[i].Close - equity[prevBuyIndex.Value].Close) / equity[prevBuyIndex.Value].Close;

                            cash = asset * equity[i].Close / equity[prevBuyIndex.Value].Close - premium;
                            asset = 0;
                            transactions.Add(equity[i].DateTime, cash);

                            prevBuyIndex = null;
                            Console.WriteLine("Sell on {0} @ {1:yyyy-MM-dd}, P/L%: {2:0.##}%", equity[i].Close, equity[i].DateTime, percent * 100);
                            Console.WriteLine();
                        }
                    }
                    Console.WriteLine();

                    portfolioResultList.Add(equity, transactions);
                }

                return new PortfolioResult(principal, premium, portfolioResultList);
            }
            );
        }
    }
}
