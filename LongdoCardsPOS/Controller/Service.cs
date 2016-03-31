using LongdoCardsPOS.Properties;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace LongdoCardsPOS.Controller
{
    using Callback = Action<string, object>;

    class Service
    {
#if DEBUG
        public const string CARD_SERVER = "https://card-test.longdo.com/";
#else
        public const string CARD_SERVER = "https://card.longdo.com/";
#endif

        public static void Login(string user, string pass, Callback action)
        {
            Request("merchantjson/login", new NameValueCollection
            {
                { "user", user },
                { "pass", pass.Md5() },
            }, action);
        }

        public static void Logout()
        {
            Request("merchantjson/logout", new NameValueCollection(), (error, data) => { });
        }

        public static void GetCards(Callback action)
        {
            Request("merchantjson/get_cards", new NameValueCollection(), action);
        }

        public static void GetRewards(Callback action)
        {
            //merchantjson/get_rewards
            Request("clientjson/v2_get_rewards", new NameValueCollection
            {
                { "card_id", Settings.Default.CardId },
            }, action);
        }

        public static void GetCustomer(string barcode, string phone, Callback action)
        {
            Request("merchantjson/get_customer_info", new NameValueCollection
            {
                { "barcode", barcode },
                { "phone", phone },
                { "card_id", Settings.Default.CardId },
            }, action);
        }

        public static void UpdateCustomer(string cuid, string phone, Callback action)
        {
            Request("merchantjson/update_customer_info", new NameValueCollection
            {
                { "cuid", cuid },
                { "phone", phone },
            }, action);
        }

        public static void AddPoint(string cuid, string point, Callback action)
        {
            Request("merchantjson/add_customer_point", new NameValueCollection
            {
                { "card_id", Settings.Default.CardId },
                { "cuid", cuid },
                { "point", point },
            }, action);
        }

        public static void UsePoint(string cuid, string ppid, Callback action)
        {
            Request("merchantjson/add_customer_point", new NameValueCollection
            {
                { "card_id", Settings.Default.CardId },
                { "cuid", cuid },
                { "pp_id", ppid },
            }, action);
        }

        private static void Request(string service, NameValueCollection values, Callback action)
        {
            values.Add("uuid", Settings.Default.Uuid);
            values.Add("token", Settings.Default.Token);
            Util.Log(service + "?" + String.Join("&", values.AllKeys.Select(k => k + "=" + values[k])));

            var client = new WebClient();
            client.UploadValuesCompleted += (s, e) => Response(e, action);
            client.UploadValuesAsync(new Uri(CARD_SERVER + service), values);
        }

        private static void Response(UploadValuesCompletedEventArgs e, Callback action)
        {
            try
            {
                if (e.Error != null) throw e.Error;

                var result = Encoding.UTF8.GetString(e.Result);
                Util.Log("Complete: " + result);
                var json = new JavaScriptSerializer().DeserializeObject(result).ToDict();
                if (json.String("code") != "200") throw new Exception(json.String("msg"));

                action(null, json.ContainsKey("data") ? json["data"] : null);
            }
            catch (Exception ex)
            {
                Util.Log(ex);
                action(ex.Message, null);
            }
        }
    }
}
