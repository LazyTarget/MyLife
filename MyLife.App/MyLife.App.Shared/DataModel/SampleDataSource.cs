using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Storage;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The data model defined by this file serves as a representative example of a strongly-typed
// model.  The property names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs. If using this model, you might improve app 
// responsiveness by initiating the data loading task in the code behind for App.xaml when the app 
// is first launched.
using CalendarCore.Outlook;
using MyLife.Channels.Calendar;
using MyLife.Channels.Toggl;

namespace MyLife.App.Data
{
    /// <summary>
    /// Generic item data model.
    /// </summary>
    public class SampleDataItem
    {
        public SampleDataItem(String uniqueId, String title, String subtitle, String imagePath, String description, String content)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Content = content;
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public string Content { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Generic group data model.
    /// </summary>
    public class SampleDataGroup
    {
        public SampleDataGroup(String uniqueId, String title, String subtitle, String imagePath, String description)
        {
            this.UniqueId = uniqueId;
            this.Title = title;
            this.Subtitle = subtitle;
            this.Description = description;
            this.ImagePath = imagePath;
            this.Items = new ObservableCollection<SampleDataItem>();
        }

        public string UniqueId { get; private set; }
        public string Title { get; private set; }
        public string Subtitle { get; private set; }
        public string Description { get; private set; }
        public string ImagePath { get; private set; }
        public ObservableCollection<SampleDataItem> Items { get; private set; }

        public override string ToString()
        {
            return this.Title;
        }
    }

    /// <summary>
    /// Creates a collection of groups and items with content read from a static json file.
    /// 
    /// SampleDataSource initializes with data read from a static json file included in the 
    /// project.  This provides sample data at both design-time and run-time.
    /// </summary>
    public sealed class SampleDataSource
    {
        private static SampleDataSource _sampleDataSource = new SampleDataSource();

        private ObservableCollection<SampleDataGroup> _groups = new ObservableCollection<SampleDataGroup>();
        public ObservableCollection<SampleDataGroup> Groups
        {
            get { return this._groups; }
        }

        public static async Task<IEnumerable<SampleDataGroup>> GetGroupsAsync()
        {
            await _sampleDataSource.GetSampleDataAsync();

            return _sampleDataSource.Groups;
        }

        public static async Task<SampleDataGroup> GetGroupAsync(string uniqueId)
        {
            await _sampleDataSource.GetSampleDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.Groups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static async Task<SampleDataItem> GetItemAsync(string uniqueId)
        {
            await _sampleDataSource.GetSampleDataAsync();
            // Simple linear search is acceptable for small data sets
            var matches = _sampleDataSource.Groups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }


        private async Task GetSampleDataAsync()
        {
            /* TESTING */

            var myLife = new Core.MyLife();

            var togglChannel = new TogglChannel("55e7ecf6095ea0d3d9d6af8aadc0fe00");
            myLife.AddChannel(togglChannel);

            var outlookClient = new OutlookClient();
            var accessToken = "EwCAAq1DBAAUGCCXc8wU/zFu9QnLdZXy+YnElFkAARaf0nPYTZgxz8qoYngiCALujrYCaZMjrLJif/HaalWM+OX+8bn10CvxjfTdTdl2kvj/9wo3nlADN4n0pqktHLo4/YzXIAy7pLQj4twdL+SBSc6PgohHkPNQRR9QdNxhLswsQMwBc1sa91DWW177IoiS7vjly5sMcT8YhDucUUHc7zwspV30fmqgzewTDCr4z7KwkwclTpf+ZGIhpqmQ18aiQGXBqy3w4Ohx/PstenGWfcvbAtjW8Rxob6NJDWe3zXcsM90bDQqd6WCjTLlYCjL0q2oMsj+KEE03RFl6d08bZF5eOrdD3/I+zRckLNe8V0n8oA4BQqAPbtQi6n1Q2YUDZgAACNyvuYVoqP3qUAEfct16JdAhGLPyEkJ2QPXjzDLi3Kig33w1xuPnF+wETblY91XygJu7SckKrRx/eRzRwVVAitxRiYnBLqMC8t9LGk+/zwz+pFNHPiNDWMGykbNKZNZYAjylelKYdJjxL9Gckmc3y7Z57Bzg9G9obV405O9nzzDQRsdh/qSWWjZn34hUlump+/JDnS21jMv9MX4S316cYr/UAL9nNR9Co/LKBdSK0yJJkNDe+BUiJDslc3+Hh3nmWp5+5VtqOIdfCEHEC+1yMjkcPgG2H4QPoNlaz/2lsrQ61AXvHKAcgmwzy48YMmcGIlHVxGUQnr4igJx1tPaH0TMO0GkFislG99SGUFP+PqX2LBo8HouIpIPleetOxEsZNhvJSU+i4K/UTllTDu9gsRh3XIaTCwYCdTkQ2ymVNQTRoX+isxVli4HYF8aO7H+8xjAhLxtnQQeieYxtAQ==";
            outlookClient.SetAccessToken(accessToken);
            var calendarChannel = new CalendarChannel(outlookClient);
            myLife.AddChannel(calendarChannel);


            var events = await myLife.GetEvents();





            if (this._groups.Count != 0)
                return;

            Uri dataUri = new Uri("ms-appx:///DataModel/SampleData.json");

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(dataUri);
            string jsonText = await FileIO.ReadTextAsync(file);
            JsonObject jsonObject = JsonObject.Parse(jsonText);
            JsonArray jsonArray = jsonObject["Groups"].GetArray();

            foreach (JsonValue groupValue in jsonArray)
            {
                JsonObject groupObject = groupValue.GetObject();
                SampleDataGroup group = new SampleDataGroup(groupObject["UniqueId"].GetString(),
                                                            groupObject["Title"].GetString(),
                                                            groupObject["Subtitle"].GetString(),
                                                            groupObject["ImagePath"].GetString(),
                                                            groupObject["Description"].GetString());

                foreach (JsonValue itemValue in groupObject["Items"].GetArray())
                {
                    JsonObject itemObject = itemValue.GetObject();
                    group.Items.Add(new SampleDataItem(itemObject["UniqueId"].GetString(),
                                                       itemObject["Title"].GetString(),
                                                       itemObject["Subtitle"].GetString(),
                                                       itemObject["ImagePath"].GetString(),
                                                       itemObject["Description"].GetString(),
                                                       itemObject["Content"].GetString()));
                }
                this.Groups.Add(group);
            }
        }
    }
}