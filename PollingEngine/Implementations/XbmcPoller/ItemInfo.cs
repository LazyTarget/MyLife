using System;

namespace XbmcPoller
{
    public class ItemInfo
    {
        public string Type { get; set; }

        public string Title { get; set; }

        public string EpisodeTitle { get; set; }

        public string Label { get; set; }

        public TimeSpan Duration { get; set; }
    }
}