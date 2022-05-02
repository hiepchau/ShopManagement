﻿using MongoDB.Bson;
using MongoDB.Driver;
using SE104_OnlineShopManagement.Models.ModelEntity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SE104_OnlineShopManagement.Services
{
    public class AutoCustomerIDGenerator : IDGenerator
    {
        public AutoCustomerIDGenerator(AppSession session, MongoClient client) : base(session, client)
        {
        }

        public async override Task<string> Generate()
        {
            string s = "";
            var database = _client.GetDatabase(_session.CurrnetUser.companyInformation);
            var collection = database.GetCollection<BsonDocument>("SavedID");
            var projection = Builders<BsonDocument>.Projection.Include("ID");
            var filter = Builders<BsonDocument>.Filter.Eq("_id", "SavedCustomer");
            var ls = await collection.Find(filter).Project(projection).ToListAsync();
            if (ls.Count < 1)
            {
                BsonDocument newdoc = new BsonDocument
                {
                    {"_id", "SavedCustomer" },
                    {"ID", "CS0" }
                };
                await collection.InsertOneAsync(newdoc);
                s = "CS0";
            }
            else if (ls.Count >= 1)
            {
                s = ls.First()["ID"].AsString;
                string tmp = s.Remove(0, 2);
                int index;
                if (int.TryParse(tmp, out index))
                {
                    tmp = "CS" + (index + 1).ToString();
                    s = tmp;
                    var collectioncheck = database.GetCollection<BsonDocument>("CustomerInformation");
                    var projectioncheck = Builders<BsonDocument>.Projection.Include("DisplayID");
                    var filtercheck = Builders<BsonDocument>.Filter.Eq("DisplayID", tmp);
                    var lscheck = await collectioncheck.Find(filtercheck).Project(projectioncheck).ToListAsync();
                    if (lscheck.Count > 0)
                    {
                        tmp = "CS" + (index + 1).ToString();
                        BsonDocument newdoc = new BsonDocument
                        {
                            {"_id", "SavedCustomer" },
                            {"ID", tmp }
                        };
                        await collection.ReplaceOneAsync(filter, newdoc);
                        return Guid.NewGuid().ToString();
                    }
                    else
                    {
                        BsonDocument newdoc = new BsonDocument
                {
                    {"_id", "SavedCustomer" },
                    {"ID", tmp }
                };
                        await collection.ReplaceOneAsync(filter, newdoc);
                    }
                }
                else
                {
                    return Guid.NewGuid().ToString();
                }
            }
            return s;
        }
    }
}
