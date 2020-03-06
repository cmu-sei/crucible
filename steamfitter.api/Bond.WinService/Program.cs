/*
Crucible
Copyright 2020 Carnegie Mellon University.
NO WARRANTY. THIS CARNEGIE MELLON UNIVERSITY AND SOFTWARE ENGINEERING INSTITUTE MATERIAL IS FURNISHED ON AN "AS-IS" BASIS. CARNEGIE MELLON UNIVERSITY MAKES NO WARRANTIES OF ANY KIND, EITHER EXPRESSED OR IMPLIED, AS TO ANY MATTER INCLUDING, BUT NOT LIMITED TO, WARRANTY OF FITNESS FOR PURPOSE OR MERCHANTABILITY, EXCLUSIVITY, OR RESULTS OBTAINED FROM USE OF THE MATERIAL. CARNEGIE MELLON UNIVERSITY DOES NOT MAKE ANY WARRANTY OF ANY KIND WITH RESPECT TO FREEDOM FROM PATENT, TRADEMARK, OR COPYRIGHT INFRINGEMENT.
Released under a MIT (SEI)-style license, please see license.txt or contact permission@sei.cmu.edu for full terms.
[DISTRIBUTION STATEMENT A] This material has been approved for public release and unlimited distribution.  Please see Copyright notice for non-US Government use and distribution.
Carnegie Mellon® and CERT® are registered in the U.S. Patent and Trademark Office by Carnegie Mellon University.
DM20-0181
*/

using System;
using System.ServiceProcess;
using System.Threading;

namespace Bond.Service
{
    internal class Program
    {
        private static void Main(params string[] args)
        {
            var service = new BondService(args);

            if (!Environment.UserInteractive)
            {
                System.IO.Directory.SetCurrentDirectory(System.AppDomain.CurrentDomain.BaseDirectory);
                var servicesToRun = new ServiceBase[] { service };
                ServiceBase.Run(servicesToRun);
                return;
            }

            Console.WriteLine("Running as a Console Application");
            Console.WriteLine(" 1. Run Service");
            Console.WriteLine(" 2. Other Option");
            Console.WriteLine(" 3. Exit");
            Console.Write("Enter Option: ");

            var input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    service.Start(args);
                    Console.WriteLine("Running Service - Press Enter To Exit");
                    Console.ReadLine();
                    break;
                case "2":
                    break;
            }
            Console.WriteLine("Closing");
        }
    }

    public class BondService : ServiceBase
    {
        public BondService(string[] args)
        {

        }
        public void Start(string[] args) { OnStart(args); }
        protected override void OnStart(string[] args)
        {
            var t = new Thread(() => BondManager.Run(args));
            t.Start();
        }
        protected override void OnStop() { }
    }
}

