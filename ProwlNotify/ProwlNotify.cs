using System;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Win32;

namespace ProwlNotify
{
    class ProwlNotify
    {
        static string ProwlApiAdd = "https://api.prowlapp.com/publicapi/add";

        static string SafeDictionaryValue(Dictionary<string, string> d, string key)
        {
            if (d.ContainsKey(key))
                return d[key];
            return "";
        }

        static void Main(string[] args)
        {
            const string USAGE = "\nUsage: ProwlNotify [OPTIONS] [ARGS]\n" +
                "  ARGS (required):\n" +
                "   -a app          Application name\n" +
                "   -e event        Event (e.g. \"Build Complete\")\n" +
                "   -d description  A description, generally terse\n" +
                "\n" +
                "  OPTIONS:\n" +
                "   -k key          Prowl API key; if not specified, use registry key\n" +
                "   -p priority     -2, -1, 0, 1, 2 (very low, moderate, normal, high, emergency)\n" +
                "";

            if (args.Length % 2 != 0)
            {
                Console.Error.WriteLine("Invalid number of parameters");
                Console.Error.WriteLine(USAGE);
                return;
            }

            Dictionary<string, string> parameters = new Dictionary<string, string>();
            for (int i = 0; i < args.Length; i+= 2)
            {
                parameters.Add(args[i], args[i + 1]);
            }

            string app = SafeDictionaryValue(parameters, "-a");
            if (app.Length == 0)
            {
                Console.Error.WriteLine("Invalid app");
                Console.Error.WriteLine(USAGE);
                return;
            }
            string evt = SafeDictionaryValue(parameters, "-e");
            if (evt.Length == 0)
            {
                Console.Error.WriteLine("Invalid event");
                Console.Error.WriteLine(USAGE);
                return;
            }
            string desc = SafeDictionaryValue(parameters, "-d");
            if (desc.Length == 0)
            {
                Console.Error.WriteLine("Invalid description");
                Console.Error.WriteLine(USAGE);
                return;
            }

            string key = SafeDictionaryValue(parameters, "-k");
            if (key.Length == 0)
            {
                // check the registry
                try
                {
                    RegistryKey HKCU = Registry.CurrentUser;
                    RegistryKey ProwlNotifyKey = HKCU.CreateSubKey("SOFTWARE\\Bluetoo\\ProwlNotify");

                    key = (string)ProwlNotifyKey.GetValue("ProwlApiKey", "");

                    if (key.Length == 0)
                    {
                        Console.Error.WriteLine("Invalid ProwlApiKey in HKCU\\SOFTWARE\\Bluetoo\\ProwlNotify");
                        Console.Error.WriteLine(USAGE);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Caught exception accessing registry: {0}", ex.Message);
                    return;
                }
            }

            string priorityString = SafeDictionaryValue(parameters, "-p");
            int priority = 0;
            int.TryParse(priorityString, out priority);

            string content = string.Format("application={0}&event={1}&description={2}&priority={3}&apikey={4}",
                app, evt, desc, priority, key);
            byte[] data = new ASCIIEncoding().GetBytes(content);

            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(ProwlApiAdd);
                req.Method = "POST";
                req.ContentType = "application/x-www-form-urlencoded";
                req.ContentLength = data.Length;
                req.GetRequestStream().Write(data, 0, data.Length);

                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                StreamReader ss = new StreamReader(response.GetResponseStream());
                string xmlResponse = ss.ReadToEnd();
                /*
                  <?xml version="1.0" encoding="UTF-8"?>
                  <prowl>
                  <success code="200" remaining="999" resetdate="1320188676" />
                  </prowl>

                  <?xml version="1.0" encoding="UTF-8"?>
                  <prowl>
                    <error code="ERRORCODE">ERRORMESSAGE</error>
                  </prowl>
                */
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(xmlResponse);

                if (xml.GetElementsByTagName("error").Count > 0)
                {
                    Console.Error.WriteLine("Error Received: {0}", xmlResponse);
                }
                else if (xml.GetElementsByTagName("success").Count > 0)
                {
                    Console.WriteLine("Successfully posted notification");
                }
                else
                {
                    Console.Error.WriteLine("Unexpected response: {0}", xmlResponse);
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Console.Error.WriteLine(ex.StackTrace);
            }
        }
    }
}
