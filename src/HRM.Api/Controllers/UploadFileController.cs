using System.Net.Mime;
using HRM.Api.Filters;
using HRM.Api.Common.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Options;

namespace HRM.Api.Controllers;

[Route("UploadFile")]
[AllowAnonymousSmartEos]
public sealed class UploadFileController : ApiControllerBase
{
    private static readonly FileExtensionContentTypeProvider MimeProvider = new();
    private readonly HrmOptions _options;

    public UploadFileController(IOptions<HrmOptions> options)
    {
        _options = options.Value;
    }

    [HttpGet]
    [Route("Get")]
    public IActionResult Get(string? filename, string? path, bool viewonly = false)
    {
        var decodedFileName = dungchung.Base64Decode(filename ?? string.Empty);
        var decodedPath = dungchung.Base64Decode(path ?? string.Empty);

        if (string.IsNullOrEmpty(decodedFileName) || string.IsNullOrEmpty(decodedPath))
        {
            return NotFound();
        }

        if (!System.IO.File.Exists(decodedPath))
        {
            return NotFound();
        }

        var ext = Path.GetExtension(decodedPath).ToLowerInvariant();
        var isImage = ext is ".gif" or ".png" or ".jpg" or ".jpeg";

        if (ext == ".pdf" && viewonly)
        {
            Response.Headers.ContentDisposition = $"inline; filename=\"{decodedFileName}\"";
            return PhysicalFile(decodedPath, MediaTypeNames.Application.Pdf);
        }

        if (isImage && viewonly)
        {
            var fileNameGuid = Path.GetFileName(decodedPath);
            if (!MimeProvider.TryGetContentType(fileNameGuid, out var mimeType))
            {
                mimeType = MediaTypeNames.Application.Octet;
            }
            Response.Headers.ContentDisposition = $"inline; filename=\"{fileNameGuid}\"";
            return PhysicalFile(decodedPath, mimeType);
        }

        return PhysicalFile(decodedPath, MediaTypeNames.Application.Octet, decodedFileName);
    }

    [HttpPost]
    [Route("Post")]
    [RequestSizeLimit(100_000_000)]
    public async Task<IActionResult> Post(CancellationToken ct)
    {
        if (!Request.HasFormContentType)
        {
            return BadRequest("This request is not properly formatted");
        }

        var uploadFolderPath = _options.PathUploads;
        if (!Directory.Exists(uploadFolderPath))
        {
            Directory.CreateDirectory(uploadFolderPath);
        }

        var form = await Request.ReadFormAsync(ct);
        var results = new List<HDFileUpload>();
        var baseRequestUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}{Request.Path}";

        foreach (var file in form.Files)
        {
            var safeOriginalName = GetValidFileName(file.FileName.Trim('"'));
            var extension = Path.GetExtension(safeOriginalName);
            var storedFileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(uploadFolderPath, storedFileName);

            await using (var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
            {
                await file.CopyToAsync(fs, ct);
            }

            var info = new FileInfo(fullPath);
            var filenameBase64 = dungchung.Base64Encode(info.Name);
            var pathBase64 = dungchung.Base64Encode(Path.Combine(uploadFolderPath, info.Name));
            var url = $"{baseRequestUrl}?filename={filenameBase64}&path={pathBase64}";

            results.Add(new HDFileUpload(info.Name, file.FileName.Trim('"'), url, (info.Length / 1024).ToString()));
        }

        return Ok(results);
    }

    [HttpGet]
    [Route("")]
    [Route("~/api/UploadFile")]
    public IEnumerable<string> Get() => new[] { "value1", "value2" };

    private static string GetValidFileName(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath)) return string.Empty;
        var invalids = Path.GetInvalidFileNameChars();
        return string.Join("_", filePath.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
    }

    public sealed class HDFileUpload
    {
        public string Name { get; set; }
        public string NameLocal { get; set; }
        public string Url { get; set; }
        public string Size { get; set; }

        public HDFileUpload(string name, string nameLocal, string url, string size)
        {
            Name = name;
            NameLocal = nameLocal;
            Url = url;
            Size = size;
        }
    }
}
