using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Firebase1
{
    [Serializable]
    public class Mard
    {
        public Mard()
        { }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public int Age { get; set; }
    }
    class Program
    {

        static void Main(string[] args)
        {
            while (true)
            {
                Console.WriteLine("Mard");

                Console.Write($"Id: ");
                int Id = int.Parse(Console.ReadLine());
                Console.Write($"Name: ");
                string Name = Console.ReadLine();
                Console.Write($"Surname: ");
                string Surname = Console.ReadLine();
                Console.Write($"Age: ");
                string Age = Console.ReadLine();

                var mesage = new
                {
                    to = "AIzaSyCfDUqj-PhrVhxitjbP9-nU_fLspTA86nU",
                    data = new
                    {
                        Id = Id,
                        Name = Name,
                        Surname = Surname,
                        Age = Age
                    }
                };

                //var mesage = new Mard()
                //{
                //    Id = Id,
                //    Name = Name,
                //    Surname = Surname
                //};


                string json = JsonConvert.SerializeObject(mesage);

                byte[] byteArray = Encoding.UTF8.GetBytes(json);

                //string server_api_key = ConfigurationManager.AppSettings["SERVER_API_KEY"];
                //string sender_id = ConfigurationManager.AppSettings["SENDER_ID"];

                string server_api_key = "AIzaSyCfDUqj-PhrVhxitjbP9-nU_fLspTA86nU";
                string sender_id = "743451158652";

                try
                {
                    WebRequest webRequest = WebRequest.Create(@"https://fcm.googleapis.com/fcm/send");
                    webRequest.Method = "post";
                    webRequest.ContentType = "application/json";
                    webRequest.Headers.Add($"Authorization: key={server_api_key}");
                    webRequest.Headers.Add($"Sender: id={sender_id}");
                    webRequest.ContentLength = byteArray.Length;


                    using (Stream Requeststream = webRequest.GetRequestStream())
                    {
                        Requeststream.Write(byteArray, 0, byteArray.Length);
                    }

                    string ret = null;
                    using (WebResponse webResponse = webRequest.GetResponse())
                    {
                        using (Stream Responsestream = webResponse.GetResponseStream())
                        {
                            using (StreamReader reader = new StreamReader(Responsestream))
                            {
                                ret = reader.ReadToEnd();
                                Console.WriteLine(ret);
                            }
                        }
                    }
                    

                    Console.ReadKey();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }

    public static class Singleton<T> where T : class, new()
    {
        static Singleton()
        {
            // Здесь могут быть дополнительные условия на тип T
        }

        public static readonly T Instance = typeof(T).InvokeMember(typeof(T).Name,
                                                                    BindingFlags.CreateInstance |
                                                                    BindingFlags.Instance |
                                                                    BindingFlags.Public |
                                                                    BindingFlags.NonPublic,
                                                                    null, null, null) as T;
    }
}
