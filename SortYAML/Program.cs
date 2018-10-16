using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sort
{
    class Program
    {

        // TODO clupo mke it so the config file is a parameter
        // TODO clupo add an option to overwrite the input file
        // TODO clupo https://stackoverflow.com/questions/21171894/reading-external-configuration-file
        static void Main(string[] args)
        {
            string inputFile = "";
            int startLine = 0;
            int endLine = 0;
            string regex = "";
            bool overwriteInputFile = false;

            ValidateInput(args, out inputFile, out startLine, out endLine, out regex, out overwriteInputFile); // TODO clupo see if can do this without the out side effect
            List<string> entireFile = File.ReadAllLines(inputFile).ToList();
            bool isEndLineSameAsFileLineLength = endLine == entireFile.Count;

            List<string> contentsRange = File.ReadAllLines(inputFile).Skip(startLine).Take(endLine - startLine + 1).ToList();

            var indexesOfLinesToSort = new List<int>();

            // Find the indexes lines to sort
            for (int i = 0; i < contentsRange.Count; i++)
            {
                if (Regex.IsMatch(contentsRange.ElementAt(i), regex))
                {
                    indexesOfLinesToSort.Add(i);
                }
            }

            var groupedLinesLists = new SortedList<string, List<string>>();

            // Group all the text that's nested under each sorted line. Essentially a cold fold
            // The lines are sorted when they are added
            for (int i = 0; i < indexesOfLinesToSort.Count; i++)
            {
                int currentIndex = indexesOfLinesToSort.ElementAt(i);

                // TODO clupo read : from config
                var matchedSortedLine = contentsRange.ElementAt(indexesOfLinesToSort.ElementAt(i)).TrimEnd(':');
                if (!groupedLinesLists.ContainsKey(matchedSortedLine))
                {
                    // if at the last  index
                    if (i == indexesOfLinesToSort.Count - 1)
                    {
                        // be careful about the num of lines to get when the end line passed in by the user is the last line of the file
                        int countToGet = isEndLineSameAsFileLineLength
                            ? (contentsRange.Count) - currentIndex
                            : (contentsRange.Count - 1) - currentIndex;

                        groupedLinesLists.Add(
                            matchedSortedLine,
                            contentsRange.GetRange(currentIndex, countToGet)
                        );
                    }
                    else
                    {
                        groupedLinesLists.Add(
                            matchedSortedLine,
                            contentsRange.GetRange(currentIndex, indexesOfLinesToSort.ElementAt(i + 1) - currentIndex)
                        );
                    }
                }
                else
                {
                    Console.WriteLine($"This line {matchedSortedLine} is duplicated in the input file. The output will only contain the first of this line");
                }
            }

            // Join the grouped list of lines back together to prepare for inserting back to the original file
            var groupedLinesString = new List<string>();
            foreach (var groupedLinesList in groupedLinesLists)
            {
                groupedLinesString.Add(String.Join("\n", groupedLinesList.Value));
            }

            // Replace the original file with the sorted lines
            entireFile.RemoveRange(startLine, endLine - startLine);
            entireFile.InsertRange(startLine, groupedLinesString);

            string outputFilePath = overwriteInputFile ?
                                      inputFile
                                    : $"{Directory.GetParent(inputFile)}\\{Path.GetFileNameWithoutExtension(inputFile)}-out{Path.GetExtension(inputFile)}";

            File.WriteAllLines(outputFilePath, entireFile);

            CheckCharacterCount(inputFile, outputFilePath);
        }

        // check that the input and output files have the same amount of characters
        private static void CheckCharacterCount(string inputFile, string outputFile)
        {
            List<string> inStrings = File.ReadAllLines(inputFile).ToList();
            List<string> outStrings = File.ReadAllLines(outputFile).ToList();

            // dictionary key is a character and the value is the character count
            var inDictionary = new Dictionary<char, int>();
            var outDictionary = new Dictionary<char, int>();

            foreach (string line in inStrings)
            {
                foreach (char character in line)
                {
                    if (inDictionary.ContainsKey(character))
                    {
                        inDictionary[character] = inDictionary[character] + 1;
                    }
                    else
                    {
                        inDictionary.Add(character, 1);
                    }
                }
            }

            foreach (string line in outStrings)
            {
                foreach (char character in line)
                {
                    if (outDictionary.ContainsKey(character))
                    {
                        outDictionary[character] = outDictionary[character] + 1;
                    }
                    else
                    {
                        outDictionary.Add(character, 1);
                    }
                }
            }

            // do the comparison
            foreach (KeyValuePair<char, int> pair in inDictionary)
            {
                if (pair.Value != outDictionary[pair.Key])
                {
                    Console.WriteLine("the input and output files don't have the same amount of characters");
                }
            }
        }

        private static void ValidateInput(string[] args, out string inputFile, out int startLine, out int endLine, out string regex, out bool overwriteInputFile)
        {
            if (File.Exists(args[0]))
            {
                inputFile = args[0];
            }
            else
            {
                inputFile = "";
                Console.WriteLine("Input file doesn't exist");
                Environment.Exit(1);
            }

            if (int.TryParse(args[1], out startLine))
            {
                startLine--;
            }
            else
            {
                Console.WriteLine("The second argument for start line should be a number");
            }

            if (int.TryParse(args[2], out endLine))
            {
                int inputFileLength = File.ReadAllLines(inputFile).Length;
                if (endLine > inputFileLength)
                {
                    endLine = inputFileLength;
                }
            }
            else
            {
                Console.WriteLine("The third argument for end line should be a number");
            }

            if (args.Length >= 4)
            {
                regex = args[3];
            }
            else
            {
                regex = "";
                Console.WriteLine("The fourth argument for the regex pattern wasn't specified");
            }

            if (args.Length >= 5 && args[4] == "yes")
            {
                overwriteInputFile = true;
            }
            else
            {
                overwriteInputFile = false;
            }
        }

        // TODO clupo remove the statics
    }
}