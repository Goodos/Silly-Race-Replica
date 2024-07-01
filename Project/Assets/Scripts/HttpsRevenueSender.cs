
using System;
using Proyecto26;
using UnityEngine;

namespace DefaultNamespace
{
    public static class HttpsRevenueSender
    {
        public static void SendRevenue(string url, PostData postData)
        {
            WWWForm form = new WWWForm();
            form.AddField("idfa", postData.idfa);
            form.AddField("bundle_id", postData.bundle_id);
            form.AddField("revenue", postData.revenue);
            form.AddField("ad_type", postData.ad_type);
            form.AddField("device", postData.device);
            form.AddField("countryCode", postData.countryCode);
            form.AddField("network", postData.network);
            
            
            RestClient.Post<ResponceData>(new RequestHelper {
                Uri = url,
                FormData = form,
            }).Then((response =>
            {
                Debug.LogError("Send revenue " + response.message+" "+response.status);
            })).Catch(err => 
                Debug.LogError("Error send revenue" + err.Message));
        }
    }
    [Serializable]
    public class PostData
    {
        public string idfa;
        public string bundle_id;
        public string revenue;
        public string ad_type;
        public string device;
        public string countryCode;
        public string network;
    }
    [Serializable]
    public class ResponceData
    {
        public string message;
        public string status;
        
    }
}