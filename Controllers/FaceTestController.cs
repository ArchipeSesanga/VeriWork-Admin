using Azure;
using Azure.AI.Vision.Face;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class FaceTestController : ControllerBase
{
    private readonly FaceClient _faceClient;

    public FaceTestController(IConfiguration config)
    {
        var endpoint = config["AzureFace:Endpoint"];
        var key = config["AzureFace:SubscriptionKey"];

        _faceClient = new FaceClient(new Uri(endpoint), new AzureKeyCredential(key));
    }

    [HttpGet("detect")]
    public async Task<IActionResult> TestFace([FromQuery] string imgUrl)
    {
        if (string.IsNullOrEmpty(imgUrl))
            return BadRequest("Image URL is required.");

        try
        {
            // ✅ Use BinaryData.FromString for URLs — this works for the new SDK
            var binaryData = BinaryData.FromObjectAsJson(new { url = imgUrl });

            var response = await _faceClient.DetectAsync(
                binaryData,
                detectionModel: FaceDetectionModel.Detection03,
                recognitionModel: FaceRecognitionModel.Recognition04,
                returnFaceId: true
            );

            var faces = response.Value;
            if (faces == null || faces.Count == 0)
                return Ok("❌ No face detected.");

            return Ok($"✅ {faces.Count} face(s) detected. FaceId: {faces[0].FaceId}");
        }
        catch (Exception ex)
        {
            return BadRequest($"Error detecting face: {ex.Message}");
        }
    }
}