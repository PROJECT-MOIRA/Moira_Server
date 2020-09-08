using Moira.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Threading.Tasks;

namespace Moira
{
    internal class Server
    {
        static void Main()
        {
            var server = new WebServiceHost(typeof(Services.MoiraService));
            server.AddServiceEndpoint(typeof(IService), new WebHttpBinding(), "");
            server.Open();
            Console.WriteLine("Moira Server Start");
            Console.WriteLine("If you want to exit this application, please push enter key.");
            Console.ReadLine();
            Console.WriteLine("Moira Server Stop");
        }
    }
}
