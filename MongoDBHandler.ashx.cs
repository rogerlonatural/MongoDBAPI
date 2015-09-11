using System;
using System.Collections;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Linq;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.Linq;
using System.Text.RegularExpressions;
using System.IO;

namespace MongoDBAPI
{
    /// <summary>
    /// Summary description for $codebehindclassname$
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    public class MongoDBHandler : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";


            string mongoDBConn = System.Configuration.ConfigurationManager.AppSettings["MongoDBConn"];
            string dbName = mongoDBConn.Substring(mongoDBConn.LastIndexOf('/') + 1);
            if (dbName.Contains('?'))
            {
                dbName = dbName.Substring(0, dbName.LastIndexOf('?'));
            }
            string collectionName = System.Configuration.ConfigurationManager.AppSettings["MongoDBCollection"];

            if ("POST".Equals(context.Request.HttpMethod))
            {
                try
                {

                    var client = new MongoClient(mongoDBConn);
                    var server = client.GetServer();
                    var dbSettings = server.CreateDatabaseSettings(dbName);
                    dbSettings.SlaveOk = true;
                    


                    var db = server.GetDatabase(dbSettings);
                    var collection = db.GetCollection(collectionName);


                    Stream instream = context.Request.InputStream;
                    BinaryReader br = new BinaryReader(instream, System.Text.Encoding.UTF8);
                    byte[] bytes = br.ReadBytes((int)instream.Length);
                    string jSONInput = System.Text.Encoding.UTF8.GetString(bytes);
                    var doc = MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(jSONInput);


                    var query = new QueryDocument(doc);
                    var result = collection.Find(query);

                    var settings = new MongoDB.Bson.IO.JsonWriterSettings();
                    settings.OutputMode = MongoDB.Bson.IO.JsonOutputMode.Strict;
                    string json = result.ToJson(settings);
                    json = "{\"result\":" + json + "}";
                    context.Response.Write(json);
                }
                catch (ExecutionTimeoutException ete)
                {
                    context.Response.Write(ete.Message);

                }
                catch (Exception ex)
                {
                    context.Response.Write(ex.Message);
                }

            }

        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}
