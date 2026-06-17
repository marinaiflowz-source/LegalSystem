using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using LegalSystem.DTOs;
using LegalSystem.Enums;
using LegalSystem.Exceptions;
using LegalSystem.Helpers;
using Microsoft.Extensions.Options;
using System.Net;

namespace LegalSystem.Services
{
    public class FileStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;

        private readonly FileStorageConfiguration s3Configuration;

        public FileStorageService(IOptionsSnapshot<FileStorageConfiguration> s3ConfigurationOption)
        {
            s3Configuration = s3ConfigurationOption.Value;
            _s3Client = new AmazonS3Client(s3Configuration.AccessKey, s3Configuration.SecretKey, RegionEndpoint.USEast1);
            _bucketName = s3Configuration.MainBucket;
        }

        public async Task<string> UploadFileAsync(IFormFile file, bool isPublic)
        {
            MemoryStream fileStream = new();
            await file.CopyToAsync(fileStream);
            fileStream.Position = 0;

            var ext = Path.GetExtension(file.FileName);
            var fileNameWithExtension = $"{Guid.NewGuid().ToString()}{ext}";

            var fileName = Guid.NewGuid().ToString();
            PutObjectRequest request = new PutObjectRequest
            {
                Key = fileName,
                InputStream = fileStream,
                BucketName = _bucketName,
                ContentType = file.ContentType,

            };
            request.Metadata.Add("x-amz-meta-title", fileName);
            if (isPublic)
                request.CannedACL = S3CannedACL.PublicRead;

            //return (await _s3Client.PutObjectAsync(request)).HttpStatusCode == HttpStatusCode.OK;
            var res = await _s3Client.PutObjectAsync(request);
            if (res.HttpStatusCode != HttpStatusCode.OK)
                throw new CustomException(FailResponseStatus.Failed, res.HttpStatusCode.ToString());
            //return $"{s3Configuration.BaseUrl}/{s3Configuration.BucketName}/{fileName}";

            return fileName;
        }
        public async Task<string> UploadFileAsync(string fileExtension, Stream fileStream, bool isPublic)
        {
            var fileName = Guid.NewGuid().ToString();

            PutObjectRequest request = new PutObjectRequest
            {
                Key = fileName,
                InputStream = fileStream,
                BucketName = _bucketName,
                ContentType = FileHelper.GetMimeType(fileExtension),

            };
            request.Metadata.Add("x-amz-meta-title", fileName);
            if (isPublic)
                request.CannedACL = S3CannedACL.PublicRead;

            //return (await _s3Client.PutObjectAsync(request)).HttpStatusCode == HttpStatusCode.OK;
            var res = await _s3Client.PutObjectAsync(request);
            if (res.HttpStatusCode == HttpStatusCode.OK)
                throw new CustomException(FailResponseStatus.Failed, res.HttpStatusCode.ToString());
            //return $"{s3Configuration.BaseUrl}/{s3Configuration.BucketName}/{fileName}";

            return string.Empty;
        }
        public async Task<DownloadFileView?> DownloadFileAsync(string fileName)
        {
            try
            {
                MemoryStream stream = new MemoryStream();
                GetObjectRequest request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = fileName
                };
                using GetObjectResponse response = await _s3Client.GetObjectAsync(request);
                await response.ResponseStream.CopyToAsync(stream);


                string contentType = response.Headers.ContentType ?? "application/octet-stream";
                var result = new DownloadFileView()
                {
                    Stream = stream,
                    Name = fileName,
                    MimeType = contentType
                };
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
