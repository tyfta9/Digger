using Digger;
using Xunit;

namespace TestProgect
{
    public class GetFastestRouteTest
    {
        public static Cell Cell = new Cell { X = 0, Y = 0 };
        [Theory]
        [InlineData(Cell, CreatureMapCreator.CreateMap(@"
PTTGTT TST
TST  TSTTM
TTT TTSTTT
T TSTS TTT
T TTTGMSTS
T TMT M TS
TSTSTTMTTT
S TTST  TG
 TGST MTTT
 T  TMTTTT"))]
        public void Test(Cell cell, ICreature[,] map)
        {
            var monster = new Monster();
            var result = monster.GetFastestRoute();
        }
    }
}