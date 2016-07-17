using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using Voron;

namespace VoronDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = new VoronDemoCommandLineOptions();
            if (Parser.Default.ParseArguments(args, options) == false)
            {
                var autoBuild = HelpText.AutoBuild(options);
                HelpText.DefaultParsingErrorsHandler(options, autoBuild);
                Console.WriteLine(autoBuild.ToString());
                return;
            }
            var vdso = StorageEnvironmentOptions.ForPath(Path.Combine(options.DataPath, "VoronDemo"));
            using (var voronDemo = new VoronDemo(vdso))
            {
                voronDemo.UserLoggedIn("shula", new DateTime(2016, 7, 11), true);
                voronDemo.UserLoggedIn("mosh",new DateTime(2016,7,14), true);
                voronDemo.UserLoggedIn("david", new DateTime(2016, 7, 14), true);
                voronDemo.UserLoggedIn("dov", new DateTime(2016, 7, 15), true);                
                voronDemo.UserLoggedIn("yudale", new DateTime(2016, 7, 17), false);
                var res = voronDemo.GetLastLogin("david");
                var entries = voronDemo.GetUsersWhoLogedInBetweenTimes(new DateTime(2016, 7, 14),
                    new DateTime(2016, 7, 15), 3);
                foreach (var e in entries)
                {
                    Console.WriteLine(e);
                }
            }
        }
    }
}
