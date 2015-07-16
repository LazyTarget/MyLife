using System;
using System.Linq.Expressions;

namespace MyLife.Models
{
    public class ModifiedEvent : IEvent
    {
        private string _text;
        private string _description;
        private DateTime? _startTime;
        private DateTime? _endTime;
        private DateTime? _timeCreated;
        private string _imageUri;

        public ModifiedEvent()
        {
            
        }


        public string ID
        {
            get { return GetValue(x => x.ID, OriginalEvent); }
        }

        public string Text
        {
            get { return GetValue(x => x.Text, _text); }
            set { _text = value; }
        }

        public string Description
        {
            get { return GetValue(x => x.Description, _description); }
            set { _description = value; }
        }

        public DateTime StartTime
        {
            get
            {
                if (_startTime != null && _startTime.HasValue)
                    return _startTime.Value;
                return GetValue(x => x.StartTime, OriginalEvent);
            }
            set
            {
                if (value == DateTime.MinValue)
                    _startTime = null;
                else
                    _startTime = value;
            }
        }

        public DateTime EndTime
        {
            get
            {
                if (_endTime != null && _endTime.HasValue)
                    return _endTime.Value;
                return GetValue(x => x.EndTime, OriginalEvent);
            }
            set
            {
                if (value == DateTime.MinValue)
                    _endTime = null;
                else
                    _endTime = value;
            }
        }

        public DateTime TimeCreated
        {
            get
            {
                if (_timeCreated != null && _timeCreated.HasValue)
                    return _timeCreated.Value;
                return GetValue(x => x.TimeCreated, OriginalEvent);
            }
            set
            {
                if (value == DateTime.MinValue)
                    _timeCreated = null;
                else
                    _timeCreated = value;
            }
        }

        public string ImageUri
        {
            get { return GetValue(x => x.ImageUri, _imageUri); }
            set { _imageUri = value; }
        }

        public IEventSource Source { get; set; }


        public IEvent OriginalEvent { get; set; }




        private T GetValue<T>(Expression<Func<IEvent, T>> lambda, T currentValue)
            where T : class 
        {
            var val = currentValue;
            if (currentValue == default(T))
            {
                if (OriginalEvent != null)
                    val = GetValue(lambda, OriginalEvent);
            }
            return val;
        }
        
        private static T GetValue<T>(Expression<Func<IEvent, T>> lambda, IEvent obj)
        {
            var val = obj != null ? lambda.Compile().Invoke(obj) : default(T);
            return val;
        }
        
    }
}
