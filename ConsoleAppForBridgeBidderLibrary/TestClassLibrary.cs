// See https://aka.ms/new-console-template for more information

using BridgeBidderLibrary;


var test = new GameFunctions();
var result = test.RunWithString("N:KT84.A42.J83.KQJ A652.T6.AK6.7653 J93.J875.T42.T82 Q7.KQ93.Q975.A94", verbose: true, newBidValues: [0,0,0,0,10.3,0,0,0,0,10.3,0,0,0,0,10.3,0,0,0,0,10.3]);

// var result = test.RunWithString("N:KT84.A42.J83.KQJ A652.T6.AK6.7653 J93.J875.T42.T82 Q7.KQ93.Q975.A94", verbose: true, newBidValues: [4,3,2,1,0,4,3,2,1,0,4,3,2,1,0,4,3,2,1,0]);



Console.WriteLine(result);
