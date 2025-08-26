//using Microsoft.Extensions.Configuration;
//using MySqlX.XDevAPI;
//using Newtonsoft.Json;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Net.Http.Headers;
//using System.Security.Cryptography;
//using System.Text;
//using System.Text.RegularExpressions;
//using System.Threading.Tasks;
//using System.Web;
//using static System.Net.WebRequestMethods;

//namespace MangaShelf.UnitTests
//{
//    [TestClass]
//    public class MalApiTest
//    {
//        public string ClientId { get; set; }
//        public string ClientSecret { get; set; }

//        public MalApiTest()
//        {
//            var configuration = new ConfigurationBuilder()
//            .AddUserSecrets<MalApiTest>()
//            .Build();

//            ClientId = configuration.GetValue<string>("clientId");
//            ClientSecret = configuration.GetValue<string>("clientSecret");
//        }

//        [TestMethod]
//        public void Auth()
//        {
//            using (var client = new HttpClient())
//            {
//                client.BaseAddress = new Uri("https://api.myanimelist.net/v2/");
//                client.DefaultRequestHeaders.Add("X-MAL-CLIENT-ID", ClientId);

//                var builder = new UriBuilder(client.BaseAddress);
//                builder.Path = "/v2/anime";

//                var query = HttpUtility.ParseQueryString(builder.Query);
//                query["q"] = "Spy x Family";
//                query["limit"] = "3";
//                builder.Query = query.ToString();

//                var req = new HttpRequestMessage(HttpMethod.Get, builder.ToString());
//                Console.WriteLine(req.RequestUri);

//                var resp = client.Send(req);
//                var json = resp.Content.ReadAsStringAsync().Result;
//                Console.WriteLine(json);


//                var obj = JsonConvert.DeserializeObject<Rootobject>(json);

//                Assert.IsNotNull(obj);
//                Assert.AreEqual(3, obj.data.Length);
//            }












//            //var redirectUri = "http://localhost/oauth";
//            //var state = "AuthTest";

//            //var baseUri = new Uri("https://myanimelist.net/v1/oauth2/");


//            //using (var httpClient = new HttpClient())
//            //{
//            //    httpClient.BaseAddress = baseUri;
//            //    var contentType = new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded");
//            //    httpClient.DefaultRequestHeaders.Accept.Add(contentType);

//            //    var uri = $"https://myanimelist.net/v1/oauth2/authorize?response_type=code&client_id={clientId}&state={state}&code_challenge={Code.CodeChallenge}&code_challenge_method=plain&redirect_uri={redirectUri}";

//            //    var result = httpClient.GetAsync(uri).Result;
//            //    string resultContent = result.Content.ReadAsStringAsync().Result;

//            //    Console.WriteLine(resultContent);
//            //}

//            //using (var httpClient = new HttpClient())
//            //{
//            //    httpClient.BaseAddress = baseUri;
//            //    var contentType = new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded");
//            //    httpClient.DefaultRequestHeaders.Accept.Add(contentType);

//            //    var token = new AccessTokenSend()
//            //    {
//            //        client_id = clientId,
//            //        client_secret = clientSecret,
//            //        code = "???",
//            //        grant_type = "authorization_code",
//            //        redirect_url = redirectUri,
//            //        state = state,
//            //        code_verifier = Code.CodeVerifier
//            //    };

//            //    var json = JsonConvert.SerializeObject(token);
//            //    var data = new StringContent(json, Encoding.UTF8, "application/json");

//            //    var result = httpClient.PostAsync("https://myanimelist.net/v1/oauth2/token", data).Result;
//            //    string resultContent = result.Content.ReadAsStringAsync().Result;

//            //    Console.WriteLine(resultContent);
//            //}

//            //var uri = $"https://myanimelist.net/v1/oauth2/authorize?response_type=code&client_id={clientId}&state={state}&code_challenge={Code.CodeChallenge}&code_challenge_method=plain&redirect_uri={redirectUri}";

//            //Console.WriteLine(uri);

//            //HttpClient client = new HttpClient();
//            //var req = new HttpRequestMessage()
//            //{
//            //    Method = HttpMethod.Get,
//            //    RequestUri = new Uri(uri),

//            //};

//            //var resp = client.Send(req);
//            //Assert.IsNotNull(resp);
//            //Assert.AreEqual(HttpStatusCode.OK, resp.StatusCode);

//            //var code = resp.Content.ReadAsStringAsync().Result;

//            //var furi = $"https://myanimelist.net/v1/oauth2/token?client_id={clientId}&code={code}&code_verifier={Code.CodeVerifier}&grant_type=authorization_code&redirect_uri={redirectUri}";
//            //var final = new HttpRequestMessage()
//            //{
//            //    Method = HttpMethod.Post,
//            //    RequestUri = new Uri(furi),
//            //};

//            //var response = client.Send(final);

//            //Assert.IsNotNull(response);



//        }

//        public class AccessTokenSend
//        {
//            public string client_id { get; set; }
//            public string client_secret { get; set; }
//            public string code { get; set; }
//            public string grant_type { get; set; }
//            public string redirect_url { get; set; }
//            public string state { get; set; }
//            public string code_verifier { get; internal set; }
//        }

//        public class Code
//        {

//            public static string CodeVerifier;

//            public static string CodeChallenge;

//            public static void Init()
//            {
//                CodeVerifier = GenerateNonce();
//                CodeChallenge = GenerateCodeChallenge(CodeVerifier);
//            }

//            private static string GenerateNonce()
//            {
//                const string chars = "abcdefghijklmnopqrstuvwxyz123456789";
//                var random = new Random();
//                var nonce = new char[128];
//                for (int i = 0; i < nonce.Length; i++)
//                {
//                    nonce[i] = chars[random.Next(chars.Length)];
//                }

//                return new string(nonce);
//            }

//            private static string GenerateCodeChallenge(string codeVerifier)
//            {
//                using var sha256 = SHA256.Create();
//                var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(codeVerifier));
//                var b64Hash = Convert.ToBase64String(hash);
//                var code = Regex.Replace(b64Hash, "\\+", "-");
//                code = Regex.Replace(code, "\\/", "_");
//                code = Regex.Replace(code, "=+$", "");
//                return code;
//            }

//        }



//        public class Rootobject
//        {
//            public Datum[] data { get; set; }
//            public Paging paging { get; set; }
//        }

//        public class Paging
//        {
//            public string next { get; set; }
//        }

//        public class Datum
//        {
//            public Node node { get; set; }
//        }

//        public class Main_Picture
//        {
//            public string medium { get; set; }
//            public string large { get; set; }
//        }


//        public class Node
//        {
//            public int id { get; set; }
//            public string title { get; set; }
//            public Main_Picture main_picture { get; set; }
//            public Alternative_Titles alternative_titles { get; set; }
//            public string start_date { get; set; }
//            public string end_date { get; set; }
//            public string synopsis { get; set; }
//            public float mean { get; set; }
//            public int rank { get; set; }
//            public int popularity { get; set; }
//            public int num_list_users { get; set; }
//            public int num_scoring_users { get; set; }
//            public string nsfw { get; set; }
//            public DateTime created_at { get; set; }
//            public DateTime updated_at { get; set; }
//            public string media_type { get; set; }
//            public string status { get; set; }
//            public Genre[] genres { get; set; }
//            public My_List_Status my_list_status { get; set; }
//            public int num_episodes { get; set; }
//            public Start_Season start_season { get; set; }
//            public Broadcast broadcast { get; set; }
//            public string source { get; set; }
//            public int average_episode_duration { get; set; }
//            public string rating { get; set; }
//            public Picture[] pictures { get; set; }
//            public string background { get; set; }
//            public Related_Anime[] related_anime { get; set; }
//            public object[] related_manga { get; set; }
//            public Recommendation[] recommendations { get; set; }
//            public Studio[] studios { get; set; }
//            public Statistics statistics { get; set; }
//        }

//        public class Alternative_Titles
//        {
//            public string[] synonyms { get; set; }
//            public string en { get; set; }
//            public string ja { get; set; }
//        }

//        public class My_List_Status
//        {
//            public string status { get; set; }
//            public int score { get; set; }
//            public int num_episodes_watched { get; set; }
//            public bool is_rewatching { get; set; }
//            public DateTime updated_at { get; set; }
//        }

//        public class Start_Season
//        {
//            public int year { get; set; }
//            public string season { get; set; }
//        }

//        public class Broadcast
//        {
//            public string day_of_the_week { get; set; }
//            public string start_time { get; set; }
//        }

//        public class Statistics
//        {
//            public Status status { get; set; }
//            public int num_list_users { get; set; }
//        }

//        public class Status
//        {
//            public string watching { get; set; }
//            public string completed { get; set; }
//            public string on_hold { get; set; }
//            public string dropped { get; set; }
//            public string plan_to_watch { get; set; }
//        }

//        public class Genre
//        {
//            public int id { get; set; }
//            public string name { get; set; }
//        }

//        public class Picture
//        {
//            public string medium { get; set; }
//            public string large { get; set; }
//        }

//        public class Related_Anime
//        {
//            public Node node { get; set; }
//            public string relation_type { get; set; }
//            public string relation_type_formatted { get; set; }
//        }


//        public class Recommendation
//        {
//            public Node node { get; set; }
//            public int num_recommendations { get; set; }
//        }


//        public class Studio
//        {
//            public int id { get; set; }
//            public string name { get; set; }
//        }

//    }
//}
