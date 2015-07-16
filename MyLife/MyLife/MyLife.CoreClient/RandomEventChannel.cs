using System.Linq;

namespace MyLife.CoreClient
{
    public class RandomEventChannel : TestEventChannel
    {
        private readonly Randomizer _randomizer;

        public RandomEventChannel()
            : this(new Randomizer())
        {
            
        }

        public RandomEventChannel(Randomizer randomizer)
        {
            _randomizer = randomizer;
        }

        public override string ChannelName
        {
            get { return "RandomEventChannel"; }
        }

        protected override void PopulateEvents()
        {
            base.PopulateEvents();

            var events = _randomizer.RandomEvents().Take(25);
            Events.AddRange(events);
        }
    }
}
