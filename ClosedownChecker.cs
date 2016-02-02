using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Net;
using Linkhub;

namespace Closedown
{
    public class ClosedownChecker
    {
        private const string ServiceID = "CLOSEDOWN";
        private const String ServiceURL = "https://closedown.linkhub.co.kr";
        private const String APIVersion = "1.0";
        
        private Token token;
        private Authority _LinkhubAuth;
        private List<String> _Scopes = new List<string>();

        public ClosedownChecker(String LinkID, String SecretKey)
        {
            _LinkhubAuth = new Authority(LinkID, SecretKey);
            _Scopes.Add("170");
        }

        public Double GetBalance()
        {
            try
            {
                return _LinkhubAuth.getPartnerBalance(getSession_Token(), ServiceID);
            }
            catch (LinkhubException le)
            {
                throw new ClosedownException(le);
            }
        }

        public Single GetUnitCost()
        {
            try
            {

                UnitCostResponse response = httpget<UnitCostResponse>("/UnitCost");

                return response.unitCost;
            }
            catch (LinkhubException le)
            {
                throw new ClosedownException(le);
            }
        }

        public CorpState checkCorpNum(String CorpNum)
        {
            if (CorpNum == null || CorpNum == "")
            {
                throw new ClosedownException(-99999999, "조회할 사업자번호가 입력되지 않았습니다");
            }

            return httpget<CorpState>("/Check?CN=" + CorpNum);
        }

        public List<CorpState> checkCorpNums(List<String> CorpNumList)
        {
            if (CorpNumList == null || CorpNumList.Count == 0) throw new ClosedownException(-99999999, "조회할 사업자번호 목록이 입력되지 않았습니다.");

            String PostData = toJsonString(CorpNumList);

            return httppost<List<CorpState>>("/Check", PostData);
        }


        #region protected

        protected String toJsonString(Object graph)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(graph.GetType());
                ser.WriteObject(ms, graph);
                ms.Seek(0, SeekOrigin.Begin);
                return new StreamReader(ms).ReadToEnd();
            }
        }
        protected T fromJson<T>(Stream jsonStream)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            return (T)ser.ReadObject(jsonStream);
        }

        private String getSession_Token()
        {
            Token _token = null;

            if (token != null)
            {
                _token = token;
            }

            bool expired = true;
            if (_token != null)
            {
                DateTime expiration = DateTime.Parse(_token.expiration);
                DateTime now = DateTime.Parse(_LinkhubAuth.getTime());
                expired = expiration < now;
            }

            if (expired)
            {
                try
                {
                    _token = _LinkhubAuth.getToken(ServiceID, null, _Scopes);

                    token = _token;
                }
                catch (LinkhubException le)
                {
                    throw new ClosedownException(le);
                }
            }

            return _token.session_token;
        }

        protected T httpget<T>(String url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ServiceURL + url);

            String bearerToken = getSession_Token();
            request.Headers.Add("Authorization", "Bearer" + " " + bearerToken);

            request.Headers.Add("x-api-version", APIVersion);

            request.Headers.Add("Accept-Encoding", "gzip, deflate");

            request.AutomaticDecompression = DecompressionMethods.GZip;

            request.Method = "GET";

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream stReadData = response.GetResponseStream())
                    {
                        return fromJson<T>(stReadData);
                    }
                }
            }
            catch (Exception we)
            {
                if (we is WebException && ((WebException)we).Response != null)
                {
                    using (Stream stReadData = ((WebException)we).Response.GetResponseStream())
                    {
                        Response t = fromJson<Response>(stReadData);
                        throw new ClosedownException(t.code, t.message);
                    }
                }
                throw new ClosedownException(-99999999, we.Message);
            }
        }

        protected T httppost<T>(String url, String PostData)
        {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ServiceURL + url);

            request.ContentType = "application/json;";

            String bearerToken = getSession_Token();
            request.Headers.Add("Authorization", "Bearer" + " " + bearerToken);
            
            request.Headers.Add("x-api-version", APIVersion);
            request.Headers.Add("Accept-Encoding", "gzip,deflate");

            request.Method = "POST";
            request.AutomaticDecompression = DecompressionMethods.GZip;

            if (String.IsNullOrEmpty(PostData)) PostData = "";

            byte[] btPostDAta = Encoding.UTF8.GetBytes(PostData);

            request.ContentLength = btPostDAta.Length;

            request.GetRequestStream().Write(btPostDAta, 0, btPostDAta.Length);

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream stReadData = response.GetResponseStream())
                    {
                        return fromJson<T>(stReadData);
                    }
                }
            }
            catch (Exception we)
            {
                if (we is WebException && ((WebException)we).Response != null)
                {
                    using (Stream stReadData = ((WebException)we).Response.GetResponseStream())
                    {
                        Response t = fromJson<Response>(stReadData);
                        throw new ClosedownException(t.code, t.message);
                    }
                }
                throw new ClosedownException(-99999999, we.Message);
            }
        }


        #endregion

        [DataContract]
        public class UnitCostResponse
        {
            [DataMember]
            public Single unitCost;
        }
    }
}
