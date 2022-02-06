using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CommunicateProvider;

namespace SampleService
{
    static public class Repository
    {
        static public Communicator ServerCommnicator { set; get; }
        static public List<Sample> samplesList { set; get; }
        static public void PrintSamples()
        {
            // 紀錄Sample中各屬性最大的長度，用來格式化format.
            Hashtable hs = new();

            // get the properts
            var props = typeof(Sample).GetProperties();
            int totalWidthAppend = 5;

            // 紀錄個欄位的長度
            foreach (var prop in props)
            {
                var valueLenColl = samplesList.Select(r => prop.GetValue(r).ToString().Length).ToList();
                valueLenColl.Add(prop.Name.Length); // 加入屬性名稱

                int maxLen = valueLenColl.Max();

                hs[prop.Name] = maxLen + totalWidthAppend;
            }

            // print divider
            var values = hs.Values;
            int dividerLen = 0;
            foreach (int v in values)
            {
                dividerLen += v;
            }
            dividerLen += (props.Count() - 1) + 2; // add number of "|"
            var divider = new string('-', dividerLen);
            Console.WriteLine(divider);

            // print title
            foreach (var prop in props)
            {
                // total width
                var tw = (int)hs[prop.Name];
                Console.Write("|");
                Console.Write(prop.Name.PadRight(tw));

            }
            Console.Write("|");
            Console.Write("\n"); // change line

            // print divider
            Console.WriteLine(divider);

            // print each sample
            foreach (var s in samplesList)
            {
                foreach (var prop in props)
                {
                    Console.Write("|");
                    var value = prop.GetValue(s).ToString();
                    // total width
                    var tw = (int)hs[prop.Name];
                    Console.Write(value.PadRight(tw));
                }
                Console.Write("|");
                Console.Write("\n"); // change line
            }

            // print divider
            Console.WriteLine(divider);
        }
    }
}