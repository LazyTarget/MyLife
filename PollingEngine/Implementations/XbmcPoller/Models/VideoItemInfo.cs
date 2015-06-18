using System;
using System.Linq;
using Newtonsoft.Json;

namespace XbmcPoller
{
    public class VideoItemInfo : IComparable<VideoItemInfo>
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("showtitle")]
        public string Showtitle { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("plot")]
        public string Plot { get; set; }

        //[JsonProperty("runtime")]
        [JsonIgnore]
        public TimeSpan Runtime { get; set; }

        [JsonProperty("episode")]
        public int Episode { get; set; }

        [JsonProperty("season")]
        public int Season { get; set; }

        [JsonProperty("thumbnail")]
        public string Thumbnail { get; set; }


        public int CompareTo(VideoItemInfo other)
        {
            var c = CompareTo(this, other);
            return c;
        }

        public static int CompareTo(VideoItemInfo a, VideoItemInfo b)
        {
            int c;
            if (a == b)
                return 0;
            if (a == null)
                return 1;
            if (b == null)
                return -1;

            c = string.Compare(a.Type, b.Type);
            if (c != 0)
                return c;

            c = string.Compare(a.Title, b.Title);
            if (c != 0)
                return c;

            c = string.Compare(a.Showtitle, b.Showtitle);
            if (c != 0)
                return c;

            c = string.Compare(a.Label, b.Label);
            if (c != 0)
                return c;
            return 0;
        }


        public override string ToString()
        {
            var parts = new string[] {Title, Showtitle, Label};
            parts = parts.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

            var res = string.Join("/", parts);
            if (string.IsNullOrWhiteSpace(res))
                return base.ToString();

            res = string.Format("{0} [{1}]", res, Type);
            return res;
        }
    }
}