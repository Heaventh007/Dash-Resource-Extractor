using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DashResourceExtractor
{
    class Program {
        // Initialize our current directory variable.
        static readonly string CurrentDirectory = $"{AppDomain.CurrentDomain.BaseDirectory}";

        // Define our xextool image name.
        const string XexToolFile = "xextool.exe";

        static bool ExtractXamFile(string XamPath, string OutputDirectory) {
            Console.WriteLine("Extraction started.");

            // Allocate a new extraction process.
            Process ExtractionProcess = new Process() {
                // Allocate a new start info instance.
                StartInfo = new ProcessStartInfo() {
                    // Combine our current directory with the xextool file.
                    FileName = $"{CurrentDirectory}{XexToolFile}",

                    // Specify our arguments in xextool. -d extracts into a directory.
                    Arguments = $"-d \"{OutputDirectory}\" \"{XamPath}\"",

                    UseShellExecute = false, // This doesn't work with no window processes.
                    CreateNoWindow = true, // Don't create a window, we'll do it in the background.

                    // Set our working directory to our current directory.
                    WorkingDirectory = CurrentDirectory
                }
            };

            // Attempt to start our extraction process.
            if (ExtractionProcess.Start()) {
                // Wait for our extraction process to complete.
                ExtractionProcess.WaitForExit();
                return true; // Return true, hopefully it succeded.
            }

            // The extraction process failed to start.
            return false;
        }

        static void PrintFinished(string Error) {
            Console.WriteLine(Error); // Write our error message to the console window.
            Console.WriteLine("Press any key to exit."); // Tell the user we're waiting for an input.

            Console.ReadKey(); // Pause exiting unti a key is pressed.
        }

        static bool InitializeDirectories(string OutputDirectory, string TemporaryDirectory) {
            // Check if our output directory exists.
            if (!Directory.Exists(OutputDirectory))
                // Directory does not exist, attempt to create our output directory.
                Directory.CreateDirectory(OutputDirectory);

            // Check if our temporary directory exists.
            if (!Directory.Exists(TemporaryDirectory))
                // Directory does not exist, attempt to create our temporary directory.
                Directory.CreateDirectory(TemporaryDirectory);

            // Check if both, our temporary directory, and output directory exists.
            return Directory.Exists(TemporaryDirectory) && Directory.Exists(OutputDirectory);
        }

        static void Main(string[] args) {

            // Check if proper arguments have been passed in.
            if (args.Length < 1) {
                // Print out the proper usage.
                PrintFinished($"Usage:\n DashResourceExtractor.exe (XamFilePath)\n DashResourceExtractor.exe (XamFilePath) (OutDirectory)\n\nExample:\n DashResourceExtractor.exe {CurrentDirectory}xam.xex {CurrentDirectory}ExtractDir\\");
                return;
            }
            
            // Check if the xam file exists.
            if (!File.Exists(args[0])) {
                // Tell the user that the input xam file does not exist.
                PrintFinished($"The input file '{args[0]}' does not exist.");
                return;
            }

            // Check if xextool exists in the tool directory.
            if (!File.Exists($"{CurrentDirectory}{XexToolFile}")) {
                // Tell the user we've failed to find the xex tool file.
                PrintFinished($"xextool.exe does not exist in the tool's directory.");
                return;
            }

            // Define our output directory.
            string OutputDirectory = ((args.Length > 1) ? args[1] : $"{CurrentDirectory}ExtractDirectory");

            // Define our temporary directory.
            string TemporaryDirectory = $"{Path.GetTempPath()}ResourceExtractor";

            // Attempt to initialize our directories.
            if (!InitializeDirectories(OutputDirectory, TemporaryDirectory)) {
                // Tell the user one of the directories was not able to be created.
                PrintFinished($"Failed to initialize directories.");
                return;
            }

            // Attempt to extract the input xam file.
            if (ExtractXamFile(args[0], TemporaryDirectory)) {
                // Check if shrdres has been found.
                if (!File.Exists($"{TemporaryDirectory}\\shrdres")) {
                    // Delete the temporary directory we created.
                    Directory.Delete(TemporaryDirectory, true);

                    // Tell the user we've failed to extract xam.
                    PrintFinished($"Failed to extract file.");
                    return;
                }

                // Check if the file already exists.
                if (File.Exists($"{OutputDirectory}\\shrdres.xzp"))
                    // Delete the file, moving won't replace it.
                    File.Delete($"{OutputDirectory}\\shrdres.xzp");

                // Move our extracted shared resource to our output directory.
                //  (Yes, the one in the temporary file doesn't have an extension, that's not a mistake.)
                File.Move($"{TemporaryDirectory}\\shrdres", $"{OutputDirectory}\\shrdres.xzp");

                // Delete our temporary directory.
                Directory.Delete(TemporaryDirectory, true);

                // Tell the user we've successfully extracted, and moved the file to the output directory.
                PrintFinished("Extracted successfully.");
                return;
            }

            // Tell the user we've failed somewhere.
            PrintFinished("Failed to extract xam file.");
        }

    }
}
