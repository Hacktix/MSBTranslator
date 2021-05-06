using Apertium.Net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using text_msbt;

namespace btMSBT
{

    class Program
    {
        static List<string> translationIterations = new List<string>
        {
            "eng", "glg", "por", "cat", "ita", "spa", "oci", "spa", "eng"
        };
        static ApertiumClient dict = new ApertiumClient();

        static int THREAD_LIMIT = 5;

        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("No file specified. Please drag-and-drop an MSBT file onto the .exe.");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                return;
            }

            string path = args[0];
            if(!File.Exists(path))
            {
                Console.WriteLine("Couldn't find specified file. Please drag-and-drop an MSBT file onto the .exe.");
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
                return;
            }

            Stream fileStream = File.Open(path, FileMode.Open);
            MSBT msbt = new MSBT(fileStream);

            List <Thread> threads = new List<Thread>();
            int busyThreads = 0;

            if(msbt.HasLabels)
            {
                Console.WriteLine("Found " + msbt.LBL1.Labels.Count + " Labels. Translating...");
                int curLabel = 0;
                foreach(Label lb in msbt.LBL1.Labels)
                {
                    while(busyThreads >= THREAD_LIMIT)
                    {
                        busyThreads = 0;
                        foreach (Thread th in threads)
                            busyThreads += th.IsAlive ? 1 : 0;
                    }

                    Thread t = new Thread(() => {
                        string txt = lb.Text;
                        string translated = BadlyTranslate(txt);
                        lb.Text = translated;
                        Console.WriteLine("Translated Label '" + lb.Name + "' (" + (++curLabel) + " out of " + msbt.LBL1.Labels.Count + ")");
                    });
                    threads.Add(t);
                    t.Start();
                    busyThreads++;
                }
            }
            else if(msbt.HasIDs)
            {
                Console.WriteLine("IDs");
            }
            else
            {
                Console.WriteLine("Other");
            }

            while(busyThreads != 0)
            {
                busyThreads = 0;
                foreach (Thread th in threads)
                    busyThreads += th.IsAlive ? 1 : 0;
            }

            Console.WriteLine("Saving...");
            Stream outStream = File.Open(path, FileMode.OpenOrCreate);
            msbt.Save(outStream);
            Console.WriteLine("MSBT Saved. Press any key to exit.");
            Console.ReadKey();
        }

        static string BadlyTranslate(string input)
        {
            string text = input;
            string lastLang = translationIterations[0];
            for(int i = 1; i < translationIterations.Count; i++)
            {
                string nextLang = translationIterations[i];
                string result = Translate(text, lastLang, nextLang);
                if (result == null)
                    continue;

                lastLang = nextLang;
                text = result;
            }
            return text;
        }

        static string Translate(string input, string inLang, string outLang)
        {
            string output = "";
            string curWord = "";
            for (int i = 0; i < input.Length; i++)
            {
                if (Char.IsLetter(input[i]))
                    curWord += input[i];
                else
                {
                    if (curWord.Length > 0)
                    {
                        string word = TranslateWord(curWord, inLang, outLang);
                        if (word == null)
                            return null;
                        output += word;
                    }
                    output += input[i];
                    curWord = "";
                }
            }
            if (curWord.Length > 0)
            {
                string word = TranslateWord(curWord, inLang, outLang);
                if (word == null)
                    return null;
                output += word;
            }
            return output;
        }

        static string TranslateWord(string input, string inLang, string outLang)
        {
            try
            {
                var defs = dict.Translate(input, inLang, outLang).Replace("*", "");
                return defs;
            } catch(Exception e) {
                Console.WriteLine("WARNING: " + e.Message + " (Translating '" + input + "' from " + inLang + " to " + outLang + ".");
                return null;
            }
        }
    }
}
