using System;
using System.Collections.Generic;
using System.Linq;
using MyLife.Models;

namespace MyLife.CoreClient
{
    public class Randomizer
    {
        private static readonly Random _random = new Random();


        public static readonly IList<string> EventTexts = new List<string>
        {
            "You played some games",
            "You where out running",
            "Brainstorm meeting",
        };
        
        public static readonly IList<string> EventDescTexts =
            new List<string>(
                @"Lorem ipsum dolor sit amet, consectetur adipiscing elit. Integer nec odio. Praesent libero. Sed cursus ante dapibus diam. Sed nisi. Nulla quis sem at nibh elementum imperdiet. Duis sagittis ipsum. Praesent mauris. Fusce nec tellus sed augue semper porta. Mauris massa. Vestibulum lacinia arcu eget nulla. 

                Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Curabitur sodales ligula in libero. Sed dignissim lacinia nunc. Curabitur tortor. Pellentesque nibh. Aenean quam. In scelerisque sem at dolor. Maecenas mattis. Sed convallis tristique sem. Proin ut ligula vel nunc egestas porttitor. Morbi lectus risus, iaculis vel, suscipit quis, luctus non, massa. Fusce ac turpis quis ligula lacinia aliquet. 

                Mauris ipsum. Nulla metus metus, ullamcorper vel, tincidunt sed, euismod in, nibh. Quisque volutpat condimentum velit. Class aptent taciti sociosqu ad litora torquent per conubia nostra, per inceptos himenaeos. Nam nec ante. Sed lacinia, urna non tincidunt mattis, tortor neque adipiscing diam, a cursus ipsum ante quis turpis. Nulla facilisi. Ut fringilla. Suspendisse potenti. Nunc feugiat mi a tellus consequat imperdiet. Vestibulum sapien. 

                Proin quam. Etiam ultrices. Suspendisse in justo eu magna luctus suscipit. Sed lectus. Integer euismod lacus luctus magna. Quisque cursus, metus vitae pharetra auctor, sem massa mattis sem, at interdum magna augue eget diam. Vestibulum ante ipsum primis in faucibus orci luctus et ultrices posuere cubilia Curae; Morbi lacinia molestie dui. Praesent blandit dolor. Sed non quam. In vel mi sit amet augue congue elementum. Morbi in ipsum sit amet pede facilisis laoreet. Donec lacus nunc, viverra nec, blandit vel, egestas et, augue. Vestibulum tincidunt malesuada tellus. Ut ultrices ultrices enim. 

                Curabitur sit amet mauris. Morbi in dui quis est pulvinar ullamcorper. Nulla facilisi. Integer lacinia sollicitudin massa. Cras metus. Sed aliquet risus a tortor. Integer id quam. Morbi mi. Quisque nisl felis, venenatis tristique, dignissim in, ultrices sit amet, augue. Proin sodales libero eget ante. Nulla quam. Aenean laoreet."

                .Split('.').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()));
                //.Split(' ').Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim().TrimEnd('.')));
        
        public static readonly IList<string> EventImgs = new List<string>
        {
            "ms-appx:///Assets/LightGray.png",
            "ms-appx:///Assets/MediumGray.png",
            "ms-appx:///Assets/DarkGray.png",
        };
        


        public DateTime DateTime(DateTime? start = null, DateTime? end = null)
        {
            if (!start.HasValue || !end.HasValue)
            {
                var sMins = _random.Next(60 * 24 * 14);
                var eMins = _random.Next(60 * 24 * 14);
                start = System.DateTime.Now.AddMinutes(sMins);
                end = System.DateTime.Now.AddMinutes(eMins);
            }

            DateTime val;
            var diff = end.Value.Subtract(start.Value);
            var maxMin = Math.Max(0, (int) diff.TotalMinutes);
            var rand = _random.Next(maxMin);
            val = start.Value.AddMinutes(rand);
            return val;
        }


        public T RandomItem<T>(IList<T> enumerable)
        {
            var index = _random.Next(enumerable.Count());
            var res = enumerable.ElementAt(index);
            return res;
        }
        


        public IEnumerable<IEvent> RandomEvents()
        {
            while (true)
            {
                var d1 = DateTime();
                var d2 = DateTime();
                var start = d1 <= d2 ? d1 : d2;
                var end = d1 > d2 ? d1 : d2;

                var evt = new Event
                {
                    ID = string.Format("random_{0}", _random.Next(Int32.MaxValue)),
                    StartTime = start,
                    EndTime = end,
                    Text = RandomItem(EventTexts),
                    Description = RandomItem(EventDescTexts),
                    ImageUri = RandomItem(EventImgs),
                };
                yield return evt;
            }
        }

    }
}
