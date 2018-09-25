using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SortYAML
{
    class Program
    {

		// TODO clupo https://docs.microsoft.com/en-us/dotnet/framework/configure-apps/
        static void Main(string[] args)
        {
            List<string> entireFile = File.ReadAllLines(@"C:\Users\clupo\Source\Repos\SortYAML\SortYAML\swagger.yaml").ToList();
            // TODO clupo skip and take from config
            // TODO clupo file from config
            List<string> contentsRange = File.ReadAllLines(@"C:\Users\clupo\Source\Repos\SortYAML\SortYAML\swagger.yaml").Skip(17).Take(1354).ToList();

            var indexesOfLinesToSort = new List<int>();

            for (int i = 0; i < contentsRange.Count; i++)
            {
                // TODO clupo regex from config
                if (Regex.IsMatch(contentsRange.ElementAt(i), @"^  \S"))
                {
                    indexesOfLinesToSort.Add(i);
                }
            }

            var groupedLinesLists = new SortedList<string, List<string>>();

            for (int i = 0; i < indexesOfLinesToSort.Count; i++)
            {
                int currentIndex = indexesOfLinesToSort.ElementAt(i);

                if (i == indexesOfLinesToSort.Count - 1)
                {
                    groupedLinesLists.Add(
                        contentsRange.ElementAt(indexesOfLinesToSort.ElementAt(i)).TrimEnd(':'), // TODO clupo read : from config
                        contentsRange.GetRange(currentIndex, (contentsRange.Count - 1) - currentIndex)
                    );
                }
                else
                {
                    groupedLinesLists.Add(
                        contentsRange.ElementAt(indexesOfLinesToSort.ElementAt(i)).TrimEnd(':'),
                        contentsRange.GetRange(currentIndex, indexesOfLinesToSort.ElementAt(i + 1) - currentIndex)
                    );
                }

                // TODO clupo handle duplicate keys
            }

            var groupedLinesString = new List<string>();
            foreach (var groupedLinesList in groupedLinesLists)
            {
                groupedLinesString.Add(String.Join("\n", groupedLinesList.Value));
            }


            entireFile.RemoveRange(17, 1353); // TODO clupo the end is one less than the passed in value
            entireFile.InsertRange(17, groupedLinesString);

            File.WriteAllLines(@"C:\Users\clupo\Source\Repos\SortYAML\SortYAML\swagger-out.yaml", entireFile);
        }
    }
}

// TODO clupo make sure input and output files have the same amount of lines