﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyLife.Core;
using MyLife.Models;
using OdbcWrapper;

namespace MyLife.Channels.Odbc
{
    public class OdbcChannel : IEventChannel
    {
        public static readonly Guid ChannelIdentifier = new Guid("60c69ee4-cc72-4f8b-b93c-8dcc250a8a80");

        private readonly OdbcClient _client;

        public OdbcChannel(string connectionString)
        {
            _client = new OdbcClient(connectionString);
            Settings = new OdbcChannelSettings();
        }

        public Guid Identifier { get { return ChannelIdentifier; } }

        public OdbcChannelSettings Settings { get; set; }


        public async Task<IEnumerable<IEvent>> GetEvents(EventRequest request)
        {
            var sql = Settings.GetEventsSql;
            if (string.IsNullOrEmpty(sql))
                return null;

            var dataTable = await _client.ExecuteReader(sql);
            
            var rows = dataTable.Rows.ToList();
            var dynamic = rows.Select(x => x.ToExpando()).ToList();
            var result = dynamic.Select(ModelConverter.ToEvent).ToList();
            return result;
        }
    }
}
