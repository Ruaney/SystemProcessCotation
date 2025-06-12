using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemProcessCotation
{
    internal class CotationService : ICotationService
    {
        private readonly Random _random = new();
        private readonly Dictionary<string, double> _lastPrices = new();
        private double lastPrice = 0.0;

     
        public async Task<CotationResult> GetCotationAsync(string symbol)
        {
            var currentPrice = SimulatePriceMovement();

            var result = new CotationResult
            {
                Symbol = symbol,
                Price = currentPrice,
                Timestamp = DateTime.Now
            };
            Console.WriteLine($"Cotação {symbol}: R$ {currentPrice:F2} ({DateTime.Now:HH:mm:ss})");

            return await Task.FromResult(result);
        }


        private double SimulatePriceMovement()
        {
            Random random = new Random();
            double currentPrice = random.NextDouble() * (23.0 - 22.0) + 22.0;
            return currentPrice;
        }

    }
}
