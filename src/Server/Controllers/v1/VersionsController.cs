using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WingetNexus.Controllers.v1;
using WingetNexus.Data;
using WingetNexus.Shared.Models.Dtos;
using WingetNexus.Shared.Models;
using Microsoft.IdentityModel.Tokens;

namespace WingetNexus.Server.Controllers.v1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class VersionsController : ControllerBase
    {
        private readonly WingetNexusContext _context;
        private readonly ILogger<PackagesController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _env;

        public VersionsController(WingetNexusContext context, ILogger<PackagesController> logger, IConfiguration configuration, IHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _env = env;
        }

        /// <summary>
        /// Update a version based on it ID
        /// </summary>
        /// <param name="versionForm">values to update</param>
        /// <param name="versionId">ID of version to update</param>
        /// <returns></returns>
        [HttpPut("{versionId}")]
        public async Task<ActionResult<Package>> PutVersion([FromBody] VersionDto versionForm, int versionId)
        {
            var hasValidationErrors = false;
            string validationErrors = "";
            PackageVersion version = null;

            _logger.LogDebug($"Updating version {versionId}");

            //ValidateCreateForm(packageForm, ref hasValidationErrors, ref validationErrors);

            //if (string.IsNullOrEmpty(versionId))
            //{
            //    hasValidationErrors = true;
            //    validationErrors += "Version id identifier is missing";
            //}

            if (hasValidationErrors)
            {
                _logger.LogDebug($"Validation errors: {validationErrors}");
                return StatusCode(500, validationErrors);
            }

            try
            {
                version = _context.PackageVersions.FirstOrDefault(p => p.Id == versionId);
                if (version == null)
                {
                    _logger.LogDebug($"Version not found for id {versionId}");
                    return StatusCode(204, "Version not found");
                }

                if (!string.IsNullOrEmpty(versionForm.ShortDescription))
                {
                    version.ShortDescription = versionForm.ShortDescription;
                    _logger.LogDebug($"Updated ShortDescription to {versionForm.ShortDescription}");
                }
                if (!string.IsNullOrEmpty(versionForm.VersionCode))
                {
                    version.VersionCode = versionForm.VersionCode;
                    _logger.LogDebug($"Updated VersionCode to {versionForm.VersionCode}");
                }
               

                await _context.SaveChangesAsync();
                _logger.LogDebug("Updated to db");
            }
            catch (Exception e)
            {
                _logger.LogWarning($"Error committing to the database: {e}");
                return StatusCode(500, "Database error");
            }

            return Ok(version);
        }

    }
}
