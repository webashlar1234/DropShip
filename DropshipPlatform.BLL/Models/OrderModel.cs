using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropshipPlatform.BLL.Models
{
    public class OrderModel
    {
        public ResultData Result { get; set; }
        public object ErrCode { get; set; }
        public object ErrMsg { get; set; }
        public object SubErrCode { get; set; }
        public object SubErrMsg { get; set; }
        public string RequestId { get; set; }
        public string Body { get; set; }
        public bool IsError { get; set; }
    }

    public class OrderData
    {
        public string AliExpressOrderNumber { get; set; }
        public string AliExpressProductId { get; set; }
        public string OrignalProductId { get; set; }
        public string OrignalProductLink { get; set; }
        public string ProductTitle { get; set; }
        public string OrderAmount { get; set; }
        public string DeleveryCountry { get; set; }
        public string ShippingWeight { get; set; }
        public string OrderStatus { get; set; }
        public string PaymentStatus { get; set; }
        public string SellerID { get; set; }
        public string SellerEmail { get; set; }
        public bool productExist { get; set; }
        public string LogisticType { get; set; }
        public string LogisticName { get; set; }
    }

    public class LogisticsAmount
    {
        public string Amount { get; set; }
        public string CurrencyCode { get; set; }
    }

    public class ProductUnitPrice
    {
        public string Amount { get; set; }
        public string CurrencyCode { get; set; }
    }

    public class TotalProductAmount
    {
        public string Amount { get; set; }
        public string CurrencyCode { get; set; }
    }

    public class ProductList
    {
        public object AfflicateFeeRate { get; set; }
        public object BuyerSignerFirstName { get; set; }
        public object BuyerSignerLastName { get; set; }
        public bool CanSubmitIssue { get; set; }
        public long ChildId { get; set; }
        public string DeliveryTime { get; set; }
        public object EscrowFeeRate { get; set; }
        public string FreightCommitDay { get; set; }
        public object FundStatus { get; set; }
        public int GoodsPrepareTime { get; set; }
        public object IssueMode { get; set; }
        public string IssueStatus { get; set; }
        public LogisticsAmount LogisticsAmount { get; set; }
        public string LogisticsServiceName { get; set; }
        public string LogisticsType { get; set; }
        public object Memo { get; set; }
        public bool MoneyBack3x { get; set; }
        public long OrderId { get; set; }
        public int ProductCount { get; set; }
        public long ProductId { get; set; }
        public string ProductImgUrl { get; set; }
        public string ProductName { get; set; }
        public string ProductSnapUrl { get; set; }
        public object ProductStandard { get; set; }
        public string ProductUnit { get; set; }
        public ProductUnitPrice ProductUnitPrice { get; set; }
        public string SendGoodsOperator { get; set; }
        public object SendGoodsTime { get; set; }
        public string ShowStatus { get; set; }
        public string SkuCode { get; set; }
        public string SonOrderStatus { get; set; }
        public TotalProductAmount TotalProductAmount { get; set; }
    }

    public class TargetList
    {
        public string BizType { get; set; }
        public string BuyerLoginId { get; set; }
        public string BuyerSignerFullname { get; set; }
        public string EndReason { get; set; }
        public object EscrowFee { get; set; }
        public int EscrowFeeRate { get; set; }
        public string FrozenStatus { get; set; }
        public string FundStatus { get; set; }
        public string GmtCreate { get; set; }
        public string GmtPayTime { get; set; }
        public object GmtSendGoodsTime { get; set; }
        public string GmtUpdate { get; set; }
        public bool HasRequestLoan { get; set; }
        public string IssueStatus { get; set; }
        public string LeftSendGoodDay { get; set; }
        public string LeftSendGoodHour { get; set; }
        public string LeftSendGoodMin { get; set; }
        public object LoanAmount { get; set; }
        public string LogisitcsEscrowFeeRate { get; set; }
        public object LogisticsStatus { get; set; }
        public object OrderDetailUrl { get; set; }
        public long OrderId { get; set; }
        public string OrderStatus { get; set; }
        public object PayAmount { get; set; }
        public string PaymentType { get; set; }
        public bool Phone { get; set; }
        public List<ProductList> ProductList { get; set; }
        public string SellerLoginId { get; set; }
        public string SellerOperatorLoginId { get; set; }
        public string SellerSignerFullname { get; set; }
        public long TimeoutLeftTime { get; set; }
    }

    public class ResultData
    {
        public int CurrentPage { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public int PageSize { get; set; }
        public bool Success { get; set; }
        public List<TargetList> TargetList { get; set; }
        public object TimeStamp { get; set; }
        public int TotalCount { get; set; }
        public int TotalPage { get; set; }
    }
}
