﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using WingetNexus.Shared.Helpers;
using WingetNexus.Shared.Models.Dtos;

namespace WingetNexus.Controllers.v1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IHostEnvironment _env;
        private readonly ILogger<FilesController> _logger;
        private readonly IConfiguration _configuration;

        public FilesController(IHostEnvironment env,
                ILogger<FilesController> logger, 
                IConfiguration configuration
            )
        {
            _env = env;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var files = System.IO.Directory.GetFiles($"./upload/");
            return Ok(files);
        }

        [HttpGet("{filename}")]
        public IActionResult Get(string filename)
        {
            var path = _configuration["uploadPath"];

            if (string.IsNullOrEmpty(path))
            {
                path = Path.Combine(_env.ContentRootPath,
                                                   _env.EnvironmentName, "upload");
            }

            path = Path.Combine(path, filename);
            var file = System.IO.File.OpenRead(path);
            return File(file, "application/octet-stream");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
        public async Task<ActionResult<IList<UploadResult>>> PostFile(
        [FromForm] IEnumerable<IFormFile> files)
        {
            var maxAllowedFiles = 3;
            long maxFileSize = 1024 * 1024 * 128; // 128 Mo Max
            var filesProcessed = 0;
            var resourcePath = new Uri($"{Request.Scheme}://{Request.Host}/");
            List<UploadResult> uploadResults = new();

            foreach (var file in files)
            {
                var uploadResult = new UploadResult();
                string trustedFileNameForFileStorage;
                var untrustedFileName = file.FileName;
                uploadResult.FileName = untrustedFileName;
                var trustedFileNameForDisplay =
                    WebUtility.HtmlEncode(untrustedFileName);

                if (filesProcessed >= maxAllowedFiles)
                {
                    _logger.LogInformation("{FileName} not uploaded because the " +
                        "request exceeded the allowed {Count} of files (Err: 4)",
                        trustedFileNameForDisplay, maxAllowedFiles);
                    uploadResult.ErrorCode = 4;
                }
                else
                {
                    if (file.Length == 0)
                    {
                        _logger.LogInformation("{FileName} length is 0 (Err: 1)",
                            trustedFileNameForDisplay);
                        uploadResult.ErrorCode = 1;
                    }
                    else if (file.Length > maxFileSize)
                    {
                        _logger.LogInformation("{FileName} of {Length} bytes is " +
                            "larger than the limit of {Limit} bytes (Err: 2)",
                            trustedFileNameForDisplay, file.Length, maxFileSize);
                        uploadResult.ErrorCode = 2;
                    }
                    else
                    {
                        try
                        {
                            // keep real file name for now
                            // TODO: change to a name based on package metadata
                            //trustedFileNameForFileStorage = Path.GetRandomFileName();

                            var path = _configuration["uploadPath"];

                            if (string.IsNullOrEmpty(path))
                            {
                                path = Path.Combine(_env.ContentRootPath,
                                                    _env.EnvironmentName, "upload");
                            }

                            path = Path.Combine(path, untrustedFileName);

                            //await using FileStream fs = new(path, FileMode.Create);
                            //await file.CopyToAsync(fs);

                            using (var stream = new FileStream(path, FileMode.Create))
                            {
                                await file.CopyToAsync(stream);
                            }

                            _logger.LogInformation("{FileName} saved at {Path}",
                                trustedFileNameForDisplay, path);
                            uploadResult.Uploaded = true;
                            uploadResult.StoredFileName = untrustedFileName; //trustedFileNameForFileStorage;
                            uploadResult.Checksum = FileHelper.GetFileChecksum(path);

                        }
                        catch (IOException ex)
                        {
                            _logger.LogError("{FileName} error on upload (Err: 3): {Message}",
                                trustedFileNameForDisplay, ex.Message);
                            uploadResult.ErrorCode = 3;
                        }
                    }

                    filesProcessed++;
                }

                uploadResults.Add(uploadResult);
            }

            return new CreatedResult(resourcePath, uploadResults);
        }

    }
}
