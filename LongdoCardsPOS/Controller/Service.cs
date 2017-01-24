using LongdoCardsPOS.Properties;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Script.Serialization;

namespace LongdoCardsPOS
{
    using Callback = Action<string, object>;

    class Service
    {
        public static void Login(string user, string pass, Callback action)
        {
            Request("main/login", new NameValueCollection
            {
                { "user", user },
                { "pass", pass.Md5() },
            }, action);
        }

        public static void Logout()
        {
            Request("main/logout", new NameValueCollection(), (error, data) => { });
        }

        public static void GetCards(Callback action)
        {
            Request("main/get_cards", new NameValueCollection(), action);
        }

        public static void GetRewards(Callback action)
        {
            Request("main/get_rewards", new NameValueCollection
            {
                { "card_id", Settings.Default.CardId },
            }, action);
        }

        public static void GetCustomers(Callback action)
        {
            Request("main/get_customers", new NameValueCollection
            {
                { "card_id", Settings.Default.CardId },
            }, action);
        }

        public static void GetCustomer(string ident, bool isPlastic, Callback action)
        {
            Request("main/get_customer_info", new NameValueCollection
            {
                { "card_id", Settings.Default.CardId },
                { isPlastic ? "plastic_ident" : "cuid", ident },
            }, action);
        }

        public static void NewCustomer(string serial, string barcode, User user, Callback action)
        {
            Request("main/subscribe_by_unregistered_card", new NameValueCollection
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
            Request("main/add_member", new NameValueCollection
            {
                { "card_id", Settings.Default.CardId },
                { user.Key, user.Id },
                { "mobile", user.Mobile },
                { "fname", user.Fname },
                { "lname", user.Lname },
                { "gender", user.Gender },
            }, action);
        }

        public static void SetCustomer(User user, Callback action)
        {
            Request("main/set_customer_profile", new NameValueCollection
            {
                { "card_id", Settings.Default.CardId },
                { user.Key, user.Id },
                { "mobile", user.Mobile },
                { "fname", user.Fname },
                { "lname", user.Lname },
                { "gender", user.Gender },
            }, action);
        }

        public static void CreateMemberTicket(string level, string remark, Callback action)
        {
            Request("main/create_member_ticket", new NameValueCollection
            {
                { "card_id", Settings.Default.CardId },
                { "level", level },
                { "remark", remark },
            }, action);
        }

        public static void AddPoint(User user, string point, Callback action)
        {
            Request("points/add_customer_point", new NameValueCollection
            {
                { "card_id", Settings.Default.CardId },
                { user.Key, user.Id },
                { "point", point },
                { "remark", "POS" },
            }, action);
        }

        public static void UsePoint(User user, Reward reward, Callback action)
        {
            Request("points/use_customer_point", new NameValueCollection
            {
                { "card_id", Settings.Default.CardId },
                { user.Key, user.Id },
                { "point", reward.Amount },
                { "pp_id", reward.Id },
            }, action);
        }

        public static void CreateTicket(string amount, string remark, Callback action)
        {
            Request("points/create_ticket", new NameValueCollection
                {
                { "card_id", Settings.Default.CardId },
                { "amount", amount },
                { "remark", remark },
            }, action);
        }

        private static void Request(string service, NameValueCollection values, Callback action)
        {
            values.Add("uuid", Settings.Default.Uuid);
            values.Add("token", Settings.Default.Token);
            Util.Log(service + "?" + String.Join("&", values.AllKeys.Select(k => k + "=" + values[k])));

            var client = new WebClient();
            client.UploadValuesCompleted += (s, e) => Response(e, action);
            client.UploadValuesAsync(new Uri(Config.Server + "/api/" + service), values);
        }

        private static void Response(UploadValuesCompletedEventArgs e, Callback action)
        {
            try
            {
                if (e.Error != null) throw e.Error;

                var result = Encoding.UTF8.GetString(e.Result);
                Util.Log("Complete: " + result);
                var json = new JavaScriptSerializer().DeserializeObject(result).ToDict();
                var error = json.String("code") == "200" ? null : json.String("msg");
                action(error, json.ContainsKey("data") ? json["data"] : null);
            }
            catch (Exception ex)
            {
                Util.Log(ex);
                action(ex.Message, null);
            }
        }
    }
}
