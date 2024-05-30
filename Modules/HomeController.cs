using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace FileTranferWebApp.Modules
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public JsonResult CheckDevice()
        {
            Process process = new Process();
            process.StartInfo.FileName = "adb";
            process.StartInfo.Arguments = "get-serialno";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            // Start the process
            process.Start();

            // Read the output
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // Display the serial number
            Console.WriteLine($"Device Serial Number: {output.Trim()}");
            return Json(output.Trim());
        }


        public JsonResult TransferFolderFromAndroid(string androidFolderPath)
        {
            // Initialize the process to execute adb pull command
            Process process = new Process();
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            process.StartInfo.FileName = "adb";
            process.StartInfo.Arguments = $"pull {androidFolderPath} {documentsPath}";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            try
            {
                // Start the process
                process.Start();

                // Read the output (if any)
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                // Check for errors
                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine($"Error: {error}");
                }
                else
                {
                    Console.WriteLine($"Folder {androidFolderPath} transferred successfully to {documentsPath}.");
                }
                return Json("Transfered Sucess");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return Json("Transfered Failed");

            }
        }


        public JsonResult DeleteFolderOnAndroid(string folderPath)
        {
            Process process = new Process();
            process.StartInfo.FileName = "adb";
            process.StartInfo.Arguments = $"shell rm -r {folderPath}";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            try
            {
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    Console.WriteLine($"Error: {error}");
                }
                else
                {
                    Console.WriteLine($"Folder {folderPath} deleted successfully.");
                }
                return Json("Deleted Success");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return Json("Deleted Failure");
            }


        }
      [HttpGet]
        public async Task<JsonResult> ListAndroidDirectories(string androidPath)
        {
            try
            {
                string command = $"shell ls -d {androidPath}*/";
                string result = await ExecuteAdbCommand(command);
                return Json(result);
            }
            catch (Exception ex)
            {
                //return $"Error: {ex.Message}";
                return Json(ex.Message);
            }
        }


        public JsonResult ListWindowsDirectories()
        {
            try
            {
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string path = documentsPath + "\\CardioPunChemical";
                var directories = Directory.GetDirectories(path);
                var directoryNames = directories.Select(dir => Path.GetFileName(dir));

                return Json(directoryNames);
            }
            catch (Exception ex)
            {
                return Json(ex.Message);
            }
        }

        public static async Task<string> ExecuteAdbCommand(string command)
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = "adb",
                Arguments = command,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var process = new Process { StartInfo = processStartInfo })
            {
                process.Start();
                string result = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();
                process.WaitForExit();

                if (!string.IsNullOrEmpty(error))
                {
                    throw new Exception(error);
                }

                return result;
            }
        }


        [HttpGet]
        public JsonResult CheckAdbSystem()
        {
            string adbPath = "adb"; // Assuming adb is in the system's PATH

            try
            {
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = adbPath,
                    Arguments = "version",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(processStartInfo))
                {
                    string output = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    if (process.ExitCode == 0)
                    {
                        return Json(new { success = true, message = output });
                    }
                    else
                    {
                        return Json(new { success = false, message = error });
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Exception occurred: " + ex.Message });
            }
        }
    }
}
