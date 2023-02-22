using Microsoft.AspNetCore.Mvc;

namespace BroomStick.DataClasses
{
    public class APIResponse
    {
        public string? Status { get; set; } = null;
        public string? Message { get; set; } = null;
        public Dictionary<string, string>? Data { get; set; } = null;
        public int? Code { get; set; } = -1;

        private HttpResponseMessage? HttpResponseMessage { get; set; } = null;

        public void SetHttpResponse(HttpResponseMessage? response)
        {
            this.HttpResponseMessage = response;
        }

        public HttpResponseMessage? GetHttpResponse()
        {
            return this.HttpResponseMessage;
        }

        public APIResponse(string? status, string? message, Dictionary<string, string>? data, int code)
        {
            Status = status;
            Message = message;
            if(data == null)
            {
                Data = new();
            }
            else
            {
                Data = data;
            }
            Code = code;
        }

        public async Task<ObjectResult> GetObjectResult()
        {

            var result = new ObjectResult(this);
            if(HttpResponseMessage != null)
            {
                result = new ObjectResult(await HttpResponseMessage.Content.ReadAsStringAsync());
            }
            result.StatusCode = Code;

            return result;
        }
    }
}
