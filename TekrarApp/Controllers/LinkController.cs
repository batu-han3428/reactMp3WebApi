using MediaToolkit;
using MediaToolkit.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text;
using VideoLibrary;
using System.IO;
using TekrarApp.Model;
using Microsoft.AspNetCore.Authorization;

namespace TekrarApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LinkController : ControllerBase
    {
        [Authorize(Roles = "Admin")]
        [HttpPost("ConvertLink")]
        public IActionResult ConvertLink(string url)
        {
            var source = @"C:\Users\bfindik\Desktop\TekrarAppMp3Folder\";
            var youtube = YouTube.Default;
            var vid = youtube.GetVideo(url);
            System.IO.File.WriteAllBytes(source + vid.FullName, vid.GetBytes());

            var inputFile = new MediaFile { Filename = source + vid.FullName };
            var outputFile = new MediaFile { Filename = $"{source + vid.FullName.Replace("mp4","mp3")}" };

            using (var engine = new Engine())
            {
                engine.GetMetadata(inputFile);

                engine.Convert(inputFile, outputFile);
            }

            byte[] File = System.IO.File.ReadAllBytes(outputFile.Filename);
            string Name = vid.FullName.Replace("mp4", "mp3");


            System.IO.File.Delete(inputFile.Filename);
            System.IO.File.Delete(outputFile.Filename);

            return Ok(
                    new Song()
                    {
                        File = File,
                        Name = Name
                    }
                );           
        }
    }
}
