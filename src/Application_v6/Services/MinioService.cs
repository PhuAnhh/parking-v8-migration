// using Minio;
// using Minio.DataModel.Args;
// using Microsoft.Extensions.Configuration;
//
// namespace Application_v6.Services;
//
// public class MinioService
// {
//     private readonly MinioClient _minioClient;
//     private readonly string _bucket;
//
//     public MinioService(MinioClient minioClient, IConfiguration config)
//     {
//         _minioClient = minioClient;
//         _bucket = config["Minio:Bucket"] ?? "parking";
//     }
//     
//     public async Task UploadFileAsync(string objectKey, string filePath, string contentType, CancellationToken token)
//     {
//         bool found = await _minioClient.BucketExistsAsync(
//             new BucketExistsArgs().WithBucket(_bucket), token);
//
//         if (!found)
//         {
//             await _minioClient.MakeBucketAsync(
//                 new MakeBucketArgs().WithBucket(_bucket), token);
//         }
//
//         await _minioClient.PutObjectAsync(new PutObjectArgs()
//             .WithBucket(_bucket)
//             .WithObject(objectKey)
//             .WithFileName(filePath)
//             .WithContentType(contentType), token);
//     }
// }