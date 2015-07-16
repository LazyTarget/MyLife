using System;
using System.Collections.Generic;
using System.Linq;
using MyLife.Models;
using SteamPoller.Models;

namespace MyLife.Channels.SteamPoller
{
    public class SteamReportFilterer
    {
        public IList<GamingSession> GetFilterSessions(IList<GamingSession> sessions, IList<SteamReportFilter> filters)
        {
            var result = new List<GamingSession>();
            foreach (var session in sessions)
            {
                var valid = true;
                try
                {
                    var groups = filters.GroupBy(x => x.GroupID);
                    foreach (var g in groups)
                    {
                        var v = ValidateReportFilterGroup(session, g.ToList());
                        if (!v)
                        {
                            valid = false;
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    valid = false;
                    System.Diagnostics.Debug.WriteLine("Error validating filter. Error: " + ex.Message);
                }

                if (valid)
                    result.Add(session);
            }
            return result;
        }

        private bool ValidateReportFilterGroup(GamingSession session, IList<SteamReportFilter> filterGroup)
        {
            var result = false;
            try
            {
                var results = filterGroup.Select(x =>
                {
                    var v = ValidateFilter(session, x);
                    return v;
                });

                var rule = filterGroup.Select(x => x.GroupRule).FirstOrDefault();
                switch (rule)
                {
                    case FilterGroupRule.AND:
                        result = results.All(x => x);
                        break;

                    case FilterGroupRule.OR:
                        result = results.Any(x => x);
                        break;

                    default:
                        throw new NotSupportedException();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error validating filter. Error: " + ex.Message);
            }
            return result;
        }

        private bool ValidateFilter(GamingSession session, SteamReportFilter filter)
        {
            var result = false;
            IComparable comparable = null;
            try
            {
                switch (filter.Attribute)
                {
                    case SteamReportAttribute.GameID:
                        comparable = session.GameID;
                        break;
                    case SteamReportAttribute.UserID:
                        comparable = session.UserID;
                        break;
                }

                if (comparable != null)
                {
                    result = CompareFilter(comparable, filter);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error validating filter. Error: " + ex.Message);
            }
            return result;
        }

        private bool CompareFilter(IComparable comparable, SteamReportFilter filter)
        {
            var res = false;
            var value = Convert.ChangeType(filter.Value, comparable.GetType());
            var c = comparable.CompareTo(value);
            switch (filter.Operator)
            {
                case FilterOperator.LessThan:
                    res = (c < 0);
                    break;
                case FilterOperator.LessOrEqualsThan:
                    res = (c <= 0);
                    break;
                case FilterOperator.Equals:
                    res = (c == 0);
                    break;
                case FilterOperator.GreaterOrEqualsThan:
                    res = (c >= 0);
                    break;
                case FilterOperator.GreaterThan:
                    res = (c > 0);
                    break;
            }
            return res;
        }

    }
}
