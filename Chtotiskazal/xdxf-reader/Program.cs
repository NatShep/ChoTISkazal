using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Text.Unicode;
using Dic.Logic.Dictionaries;

namespace xdxf_reader
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = "T:\\Dictionary\\eng_rus_full.json";
            var dictionary =  Tools.ReadFromFile(path);

            while (true)
            {
                var line = Console.ReadLine();
                var result = dictionary.GetOrNull(line);
                if (result == null)
                {
                    Console.WriteLine("Word not found");
                }
                else
                {
                    Console.WriteLine();

                    Console.Write(result.Origin);
                    if(result.Transcription!=null)
                        Console.Write($" [{result.Transcription}]");
                    Console.WriteLine();
                    foreach (var word in result.Translations.Take(10))
                    {
                        Console.WriteLine("\t"+ word);
                    }
                    Console.WriteLine();
                }
            }
        }
    }

   
}
