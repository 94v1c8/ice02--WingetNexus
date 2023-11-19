using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WingetNexus.Data;
using WingetNexus.Shared.Models;
using WingetNexus.Shared.Models.Dtos;

namespace WingetNexus.Controllers.v1
{
    [Route("api/v1/[controller]")]
    [ApiController]
    [ValidateAntiForgeryToken]
    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    public class PackagesController : ControllerBase
    {
        private readonly WingetNexusContext _context;
        private readonly ILogger<PackagesController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _env;

        public PackagesController(WingetNexusContext context, ILogger<PackagesController> logger, IConfiguration configuration, IHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _env = env;
        }

        /// <summary>
        /// Get all packages
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Package>>> Get()
        {
            var packages = _context.Packages
                .Include(p => p.Versions);

            return Ok(packages);
        }

        /// <summary>
        /// Get full package details by identifier
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("{id}")]
        //[Authorize]
        //[Authorize(Policy = "get:package")]
        public async Task<ActionResult<Package>> GetPackage(string id)
        {
            var package = await _context.Packages
                .Include(p => p.Versions)
                .Include("Versions.Installers")
                .Include("Versions.Installers.NestedInstallerFiles")
                .FirstAsync(p => p.Identifier == id);

            return Ok(package);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageForm"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<Package>> Post([FromBody] CreatePackageDto packageForm)
        {
            var hasValidationErrors = false;
            string validationErrors = "";

            ValidateCreateForm(packageForm, ref hasValidationErrors, ref validationErrors);

            if (hasValidationErrors)
            {
                return StatusCode(500, validationErrors);
            }

            //TODO: check for unicity
            var identifier = $"{packageForm.Publisher}.{packageForm.Name}";
            var package = new Package(identifier, packageForm.Name, packageForm.Publisher);

            var version = new PackageVersion(packageForm.Version, "en-US", identifier);
            var installer = new Installer(packageForm.Architecture, packageForm.InstallerType, packageForm.Filename, 
                packageForm.Checksum, "user");

            version.Installers = new List<Installer> { installer };
            package.Versions = new List<PackageVersion> { version };

            try
            {
                _context.Packages.Add(package);
                await _context.SaveChangesAsync();
                _logger.LogDebug("Commited to db");
            }
            catch (Exception e)
            {
                _logger.LogDebug($"Error committing to the database: {e}");
                return StatusCode(500, "Database error");
            }

            return Ok(package);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packageForm"></param>
        /// <param name="identifier"></param>
        /// <returns></returns>
        [HttpPut("{identifier}/infos")]
        public async Task<ActionResult<PackageVersion>> Put([FromBody] NewPackageDto packageForm, string identifier)
        {
            var hasValidationErrors = false;
            string validationErrors = "";
            Package package = null;

            //ValidateCreateForm(packageForm, ref hasValidationErrors, ref validationErrors);

            if (string.IsNullOrEmpty(identifier))
            {
                hasValidationErrors = true;
                validationErrors += "Package identifier is missing";
            }

            if (hasValidationErrors)
            {
                return StatusCode(500, validationErrors);
            }

            try
            {
                package = _context.Packages.FirstOrDefault(p => p.Identifier == identifier);
                if (package == null)
                {
                    return StatusCode(204, "Package not found");
                }

                package.Name = packageForm.Name;
                package.Publisher = packageForm.Publisher;

                await _context.SaveChangesAsync();
                _logger.LogDebug("Updated to db");
            }
            catch (Exception e)
            {
                _logger.LogDebug($"Error committing to the database: {e}");
                return StatusCode(500, "Database error");
            }

            return Ok(package);
        }

        /// <summary>
        /// Delete specified package with cascading delete
        /// </summary>
        /// <param name="packageID"></param>
        /// <returns></returns>
        [HttpDelete("{packageID}")]
        //[Authorize]
        public ActionResult DeletePackage(string packageID)
        {
            var package = _context.Packages.FirstOrDefault(p => p.Identifier == packageID);

            if (package == null)
            {
                return NotFound();
            }

            try
            {
                _context.Packages.Remove(package);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"ERROR: {ex.Message}");
            }

            return NoContent();
        }

        private void ValidateCreateForm(CreatePackageDto packageForm, ref bool hasValidationErrors, ref string validationErrors)
        {
            if (packageForm == null)
            {
                hasValidationErrors = true;
                validationErrors += "No package form data provided";
                return;
            }

            if (string.IsNullOrEmpty(packageForm.Name))
            {
                hasValidationErrors = true;
                validationErrors += "No package name provided";
            }

            if (string.IsNullOrEmpty(packageForm.Publisher))
            {
                hasValidationErrors = true;
                validationErrors += "No package publisher provided";
            }

            //try
            //{
            //    var cnt = _context.Packages.Count(p=>p.Identifier == packageForm.Identifier);
            //    if (cnt > 0)
            //    {
            //        hasValidationErrors = true;
            //        validationErrors += "Identifier (publisher.name) must be unique";
            //    }
            //}
            //catch (Exception e)
            //{
            //    _logger.LogDebug($"Error accessing to the database: {e}");
            //}

            if (hasValidationErrors)
            {
                throw new Exception(validationErrors);
            }
        }

    }
}
