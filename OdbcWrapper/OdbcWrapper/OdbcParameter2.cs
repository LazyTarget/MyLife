using System;

namespace OdbcWrapper
{
    public class OdbcParameter2
    {
        private object _value;
        
        
        public string Name { get; set; }

        public object Value
        {
            get { return _value; }
            set
            {
                _value = value;
                if (value is DateTime)
                {
                    var d = (DateTime) _value;
                    if (d == DateTime.MinValue)
                        _value = System.Data.SqlTypes.SqlDateTime.MinValue.Value;
                    else if (d == DateTime.MaxValue)
                        _value = System.Data.SqlTypes.SqlDateTime.MaxValue.Value;
                }
            }
        }

        public bool IsOutput { get; set; }

        public int? Size { get; set; }
        
    }
}
