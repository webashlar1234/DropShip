using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Top.Api.Response;

namespace DropshipPlatform.BLL.Models
{
    public class ResponseModel
    {
        public Object Data { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }


    #region AliExpress Responses 
    public class Result
    {
        public string schema { get; set; }
        public bool success { get; set; }
    }

    public class AliExpresssProductSchemaModel
    {
        public AliexpressSolutionProductSchemaGetResponse aliexpress_solution_product_schema_get_response { get; set; }
    }

    public class AliexpressSolutionFeedResponseModel
    {
        public string ItemContentId { get; set; }
        public string ItemExecutionResult { get; set; }
    }

    public class IntegrationRequestParameter
    {
        public string adminLoginId { get; set; }
        public string apiName { get; set; }
        public string appKey { get; set; }
        public string schemaPostRequest { get; set; }
    }

    public class IntegrationErrorCode
    {
        public string errorCode { get; set; }
        public string errorMessage { get; set; }
        public IntegrationRequestParameter integrationRequestParameter { get; set; }
    }

    public class ItemExecutionResultModel
    {
        public string errorCode { get; set; }
        public string errorMessage { get; set; }
        public IntegrationErrorCode integrationErrorCode { get; set; }
        public bool success { get; set; }
        public string traceId { get; set; }
    }
    #endregion

}
