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
    public class RegisterProducer
    {
        private ProducerInformation producer;
        private MongoClient mongoClient;
        private AppSession session;
        public RegisterProducer(ProducerInformation newcus, MongoClient client, AppSession ses)
        {
            this.producer = newcus;
            this.mongoClient = client;
            this.session = ses;
        }

        public async Task<string> register()
        {
            var database = mongoClient.GetDatabase(session.CurrnetUser.companyInformation);
            var collection = database.GetCollection<BsonDocument>("ProducerInformation");
            var filtercheck = Builders<ProducerInformation>.Filter.Eq(x=>x.displayID,producer.displayID) | Builders<ProducerInformation>.Filter.Eq(x => x.displayID, producer.ID);
            var task1 = new GetProducer(mongoClient, session, filtercheck).Get();
            var lscheck = await task1;
            Task.WaitAll(task1);
            if(string.IsNullOrEmpty(producer.ID) && string.IsNullOrEmpty(producer.displayID))
            {
                Console.WriteLine("Insert Error!");
                return null;
            }
            if (lscheck.Count > 0)
            {
                Console.WriteLine("Insert error");
                return null;
            }
            if (string.IsNullOrEmpty(producer.displayID)) { 
            BsonDocument newProductDoc = new BsonDocument{
                {"Name", producer.Name},
                {"Email", producer.Email },
                {"Phone", producer.PhoneNumber },
                {"DisplayID",producer.ID},
                {"Address",producer.Address }
            };
            await collection.InsertOneAsync(newProductDoc);
            Console.WriteLine("Object Inserted into " + session.CurrnetUser.companyInformation);
            return newProductDoc["_id"].ToString();
            }
            else
            {
                BsonDocument newProductDoc = new BsonDocument{
                {"Name", producer.Name},
                {"Email", producer.Email },
                {"Phone", producer.PhoneNumber },
                {"DisplayID",producer.displayID},
                {"Address", producer.Address },
                {"isActivated",true},
            };
                await collection.InsertOneAsync(newProductDoc);
                Console.WriteLine("Object Inserted into " + session.CurrnetUser.companyInformation);
                return newProductDoc["_id"].ToString();
            }
        }
    }
}
