
using doc_web.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using System.Diagnostics;
using System.Security.Principal;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace doc_web.Controllers
{
    public class HomeController : Controller
    {
        //private IHostingEnvironment Environment;
        private readonly IWebHostEnvironment Environment;
        private readonly IConfiguration _configuration;
        public IActionResult Index1()
    {
        // Check if the authenticated user is in either "pki-sec" or "it-operation-sec" group
        WindowsIdentity windowsIdentity = HttpContext.User.Identity as WindowsIdentity;
        WindowsPrincipal windowsPrincipal = new WindowsPrincipal(windowsIdentity);

        if (windowsPrincipal.IsInRole("pki-sec") || windowsPrincipal.IsInRole("it-operation-sec"))
        {
            // Grant access to authorized users
            return View("AuthorizedView");
        }
        else
        {
            // Handle unauthorized access
            return Unauthorized();
        }
    }
        
        public HomeController(IWebHostEnvironment _environment, IConfiguration configuration)
        {
            Environment = _environment;
            _configuration = configuration;
        }
        [HttpPost]
        public IActionResult Search(string searchTerm)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                return RedirectToAction("Index");
            }
            //TODO:CHANGE CONFIG FILE
            var files = Directory.GetFiles(Path.Combine(Environment.WebRootPath, "Files"), "*", SearchOption.AllDirectories)
                .Where(file => Path.GetFileName(file).Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .Select(file => new FileModel { FileName = Path.GetFileName(file), IsFolder = false });

            var folders = Directory.GetDirectories(Path.Combine(Environment.WebRootPath, "Files"), "*", SearchOption.AllDirectories)
                .Where(folder => Path.GetFileName(folder).Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                .Select(folder => new FileModel { FileName = Path.GetFileName(folder), IsFolder = true });

            var searchResults = files.Concat(folders).ToList();

            return View("Index", searchResults);
        }


        public IActionResult DownloadFile(string path)
        {
            // Validate path to prevent directory traversal attacks
            string basePath = Path.Combine(Environment.WebRootPath, "Files");
            string fullPath = Path.Combine(basePath, path);

            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound();
            }

            byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);
            return File(fileBytes, "application/octet-stream", Path.GetFileName(path));
        }



        // private readonly ILogger<HomeController> _logger;

        //  public HomeController(ILogger<HomeController> logger)
        //  {
        //     _logger = logger;
        // }

        public IActionResult Index(string? searchTerm = null)
        {
            if (searchTerm != null)
            {
                // This is a search request, so return the search results
                return Search(searchTerm);
            }

            string basePath = Path.Combine(Environment.WebRootPath, "Files"); // Using CurrentDirectory instead of WebRootPath
            var files = Directory.GetFiles(basePath).Select(file => new FileModel { FileName = Path.GetFileName(file), IsFolder = false, FolderName = file });
            var folders = Directory.GetDirectories(basePath).Select(folder => new FileModel { FileName = Path.GetFileName(folder), IsFolder = true, FolderName = folder });

            var list = files.Concat(folders).ToList();

            return View(list);
        }

        public IActionResult Folder(string folderName)
        {

            string basePath = Path.Combine(Environment.WebRootPath, "Files", folderName);

            var files = Directory.GetFiles(basePath).Select(file => new FileModel
            {
                FileName = Path.GetFileName(file),
                IsFolder = false,
                FolderName = file
            });

            var folders = Directory.GetDirectories(basePath).Select(folder => new FileModel
            {
                FileName = Path.GetFileName(folder),
                IsFolder = true,
                FolderName = folder
            });

            var list = files.Concat(folders).ToList();



            return PartialView("_FolderContent", list);
        }

       

        public IActionResult ViewFile(string path)
        {
            // Construct the full file path based on the provided path
            string fullPath = Path.Combine(Environment.WebRootPath, "Files", path);

            // Check if the file exists
            if (!System.IO.File.Exists(fullPath))
            {
                return NotFound(); // Return a 404 Not Found response if the file doesn't exist
            }

            // Return the file for viewing
            // This example assumes the file can be directly viewed in the browser
            // You may need to adjust this logic based on your specific requirements
            return PhysicalFile(fullPath, "application/pdf"); // Change the MIME type as needed
        }
    


      
     /*   public IActionResult DownloadFile(string path)
        {

            Console.WriteLine("Path received: " + path); // Add debugging statement

            if (string.IsNullOrEmpty(path))
            {
                return NotFound();
            }
            // Construct the full path to the file
          string basePath = Path.Combine(Environment.WebRootPath, "Files");
            string fullPath = Path.Combine(basePath, path);

            // Validate if the file exists
         if (!System.IO.File.Exists(fullPath))
          {
                return NotFound();
           }

            // Read the file content into a byte array
          byte[] fileBytes = System.IO.File.ReadAllBytes(fullPath);

            // Determine the file's MIME type
           string contentType = GetContentType(path);

          // Return the file as a downloadable attachment
           return File(fileBytes, contentType, Path.GetFileName(path));
       }
     */

        // Helper method to determine the MIME type of the file based on its extension
        private string GetContentType(string path)
        {
            string contentType;
            string extension = Path.GetExtension(path).ToLowerInvariant();

            switch (extension)
            {
                case ".pdf":
                    contentType = "application/pdf";
                    break;
                case ".doc":
                    contentType = "application/msword";
                    break;
                case ".docx":
                    contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    break;
                case ".xlsx":
                    contentType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                    break;
                // Add more cases for other file types as needed
                default:
                    contentType = "application/octet-stream";
                    break;
            }

            return contentType;
        }

        // public IActionResult ViewFile(string path)
        // {
        // Construct the full file path based on the provided path
        //   string fullPath = Path.Combine(Environment.WebRootPath, "Files", path);

        // Check if the file exists
        //  if (!System.IO.File.Exists(fullPath))
        //  {
        //    return NotFound(); // Return a 404 Not Found response if the file doesn't exist
        //  }

        // Return the file for viewing
        // This example assumes the file can be directly viewed in the browser
        // You may need to adjust this logic based on your specific requirements
        // return PhysicalFile(fullPath, "application/pdf"); // Change the MIME type as needed
        //  }

 
        public IActionResult NavigateFolder(string folderName)
        {
            // Construct the full folder path based on the provided folderName
            string folderPath = Path.Combine(Environment.WebRootPath, "Files", folderName);

            // Get the list of files and folders inside the specified folder
            var files = Directory.GetFiles(folderPath).Select(file => new FileModel
            {
                FileName = Path.GetFileName(file),
                IsFolder = false
            });
            var folders = Directory.GetDirectories(folderPath).Select(folder => new FileModel
            {
                FileName = Path.GetFileName(folder),
                IsFolder = true
            });

            var list = files.Concat(folders).ToList();

            return View("Index", list); // Render the same Index view to display the contents of the folder
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
