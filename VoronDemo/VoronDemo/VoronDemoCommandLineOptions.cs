using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;

namespace VoronDemo
{
    public class VoronDemoCommandLineOptions
    {
        [Option('d', "DataPath", HelpText = "The path for the voron file", Required = true)]
        public string DataPath { get; set; }
    }
}
