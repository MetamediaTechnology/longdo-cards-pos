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
    using Model;
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
            Request("merchantjson/get_rewards", new NameValueCollection
            {
                { "card_id", Settings.Default.CardId },
            }, action);
        }

        public static void GetCustomer(string plasticId, Callback action)
        {
            Request("merchantjson/get_customer_info", new NameValueCollection
            {
                { "card_id", Settings.Default.CardId },
                { "plastic_ident", plasticId },
            }, action);
        }

        public static void NewCustomer(string serial, string barcode, User user, Callback action)
        {
            Request("merchantjson/subscribe_by_unregistered_card", new NameValueCollection
            {
                { "card_id", Settings.Default.CardId },
                { "serial", serial },
                { "barcode", barcode },
                { "mobile", user.Mobile },
                { "fname", user.Fname },
                { "lname", user.Lname },
                { "gender", user.Gender },
            }, action);
        }

        public static void SubscribeCustomer(User user, Callback action)
        {
            Request("merchantjson/add_member", new NameValueCollection
            {
                { "card_id", Settings.Default.CardId },
                { user.Key, user.Id },
                { "mobile", user.Mobile },
            }, action);
        }

        public static void SetCustomer(User user, Callback action)
        {
            Request("merchantjson/set_plastic_profile", new NameValueCollection
            {
                { "card_id", Settings.Default.CardId },
                { user.Key, user.Id },
                { "mobile", user.Mobile },
                { "fname", user.Fname },
                { "lname", user.Lname },
                { "gender", user.Gender },
            }, action);
        }

        public static void AddPoint(User user, string point, Callback action)
        {
            Request("merchantjson/add_customer_point", new NameValueCollection
            {
                { "card_id", Settings.Default.CardId },
                { user.Key, user.Id },
                { "point", point },
                { "remark", "POS" },
            }, action);
        }

        public static void UsePoint(User user, Reward reward, Callback action)
        {
            Request("merchantjson/use_customer_point", new NameValueCollection
            {
                { "card_id", Settings.Default.CardId },
                { user.Key, user.Id },
                { "point", reward.Amount },
                { "pp_id", reward.Id },
            }, action);
        }

        public static void CreateTicket(string amount, Callback action)
        {
            Request("merchantjson/create_ticket", new NameValueCollection
            {
                { "card_id", Settings.Default.CardId },
                { "amount", amount },
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
