﻿using MongoDB.Bson;
using MongoDB.Driver;
using SE104_OnlineShopManagement.Models.ModelEntity;
using SE104_OnlineShopManagement.Network.Get_database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SE104_OnlineShopManagement.Network.Insert_database
{
    public class RegisterStocking
    {
        private StockInformation stock;
        private MongoClient mongoClient;
        private AppSession session;
        public RegisterStocking(StockInformation newstock, MongoClient client, AppSession ses)
        {
            this.stock = newstock;
            this.mongoClient = client;
            this.session = ses;
        }

        public async Task<string> register()
        {
            var database = mongoClient.GetDatabase(session.CurrnetUser.companyInformation);
            var collection = database.GetCollection<BsonDocument>("StockingInformation");
            var filtercheck = Builders<StockInformation>.Filter.Eq(x => x.displayID, stock.displayID) | Builders<StockInformation>.Filter.Eq(x => x.displayID, stock.ID);
            var task1 = new GetStocking(mongoClient, session, filtercheck).Get();
            var lscheck = await task1;
            Task.WaitAll(task1);
            if(string.IsNullOrEmpty(stock.ID) && string.IsNullOrEmpty(stock.displayID)){
                Console.WriteLine("Insert error");
                return null;
            }
            if (lscheck.Count > 0)
            {
                Console.WriteLine("Insert error");
                return null;
            }
            if (string.IsNullOrEmpty(stock.displayID)) { 
            BsonDocument newProductDoc = new BsonDocument
            {
                {"StockDay",stock.StockDay},
                {"UserID",stock.User},
                {"ProducerID", stock.producer },
                {"Total", stock.total },
                {"DisplayID",stock.ID},
            };
            await collection.InsertOneAsync(newProductDoc);
            Console.WriteLine("User Inserted into", session.CurrnetUser.companyInformation);
            return newProductDoc["_id"].ToString();
            }
            else
            {
                BsonDocument newProductDoc = new BsonDocument
            {
                {"StockDay",stock.StockDay},
                {"UserID",stock.User},
                {"ProducerID", stock.producer },
                {"Total", stock.total },
                {"DisplayID",stock.displayID},
            };
                await collection.InsertOneAsync(newProductDoc);
                Console.WriteLine("User Inserted into", session.CurrnetUser.companyInformation);
                return newProductDoc["_id"].ToString();
            }
        }
    }
}
