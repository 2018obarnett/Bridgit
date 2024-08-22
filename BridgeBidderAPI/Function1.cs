using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using BridgeBidding;
using bridgit;


namespace BridgeBidderAPI
{
    public class Function1
    {
        private readonly ILogger<Function1> _logger;

        public Function1(ILogger<Function1> logger)
        {
            _logger = logger;
        }

        [Function("BridgeBidder")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");


            string hand = req.Query["hand"];
            if (hand == "")
            {
                return new OkObjectResult("Please pass a hand");
            }

            var dealOption = hand;
            var vulnerableOption = Vulnerable.None;

            string bids = InterractiveApp.BidDealReturnState(Game.Parse(dealOption, vulnerableOption.ToString()));

            return new OkObjectResult(bids);
        }

        public string RunWithString(string deal)
        {
            var vulnerableOption = Vulnerable.None;

            string bids = InterractiveApp.BidDealReturnState(Game.Parse(deal, vulnerableOption.ToString()));

            return (bids);
        }
    }
}
