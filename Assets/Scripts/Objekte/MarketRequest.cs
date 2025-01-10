using Unity.VisualScripting;

namespace Server.Data
{
    public class MarketRequest
    {
        public ulong player;
        public int amount;
        public string rec;

        public MarketRequest(ulong playerId, string rec, int amount)
        {
            this.player = playerId;
            this.amount = amount;
            this.rec = rec;
        }
    }
}
