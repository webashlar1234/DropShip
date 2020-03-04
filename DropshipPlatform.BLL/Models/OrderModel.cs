using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropshipPlatform.BLL.Models
{
    public class OrderModel
    {
        public AliexpressSolutionOrderGetResponse aliexpress_solution_order_get_response { get; set; }
    }

    public class OrderData
    {
        public int AliExpressOrderNumber { get; set; }
        public int AliExpressProductId { get; set; }
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
    }

    public class TotalProductAmount
    {
        public string currency_code { get; set; }
        public string amount { get; set; }
    }

    public class ProductUnitPrice
    {
        public string currency_code { get; set; }
        public string amount { get; set; }
    }

    public class LogisticsAmount
    {
        public string currency_code { get; set; }
        public string amount { get; set; }
    }

    public class OrderProductDto
    {
        public TotalProductAmount total_product_amount { get; set; }
        public string son_order_status { get; set; }
        public string sku_code { get; set; }
        public string show_status { get; set; }
        public string send_goods_time { get; set; }
        public string send_goods_operator { get; set; }
        public ProductUnitPrice product_unit_price { get; set; }
        public string product_unit { get; set; }
        public string product_standard { get; set; }
        public string product_snap_url { get; set; }
        public string product_name { get; set; }
        public string product_img_url { get; set; }
        public int product_id { get; set; }
        public int product_count { get; set; }
        public int order_id { get; set; }
        public bool money_back3x { get; set; }
        public string memo { get; set; }
        public string logistics_type { get; set; }
        public string logistics_service_name { get; set; }
        public LogisticsAmount logistics_amount { get; set; }
        public string issue_status { get; set; }
        public string issue_mode { get; set; }
        public int goods_prepare_time { get; set; }
        public string fund_status { get; set; }
        public string freight_commit_day { get; set; }
        public string escrow_fee_rate { get; set; }
        public string delivery_time { get; set; }
        public int child_id { get; set; }
        public bool can_submit_issue { get; set; }
        public string buyer_signer_last_name { get; set; }
        public string buyer_signer_first_name { get; set; }
        public string afflicate_fee_rate { get; set; }
    }

    public class ProductList
    {
        public List<OrderProductDto> order_product_dto { get; set; }
    }

    public class PayAmount
    {
        public string currency_code { get; set; }
        public string amount { get; set; }
    }

    public class LoanAmount
    {
        public string currency_code { get; set; }
        public string amount { get; set; }
    }

    public class EscrowFee
    {
        public string currency_code { get; set; }
        public string amount { get; set; }
    }

    public class OrderDto
    {
        public int timeout_left_time { get; set; }
        public string seller_signer_fullname { get; set; }
        public string seller_operator_login_id { get; set; }
        public string seller_login_id { get; set; }
        public ProductList product_list { get; set; }
        public bool phone { get; set; }
        public string payment_type { get; set; }
        public PayAmount pay_amount { get; set; }
        public string order_status { get; set; }
        public long order_id { get; set; }
        public string order_detail_url { get; set; }
        public string logistics_status { get; set; }
        public string logisitcs_escrow_fee_rate { get; set; }
        public LoanAmount loan_amount { get; set; }
        public string left_send_good_min { get; set; }
        public string left_send_good_hour { get; set; }
        public string left_send_good_day { get; set; }
        public string issue_status { get; set; }
        public bool has_request_loan { get; set; }
        public string gmt_update { get; set; }
        public string gmt_send_goods_time { get; set; }
        public string gmt_pay_time { get; set; }
        public string gmt_create { get; set; }
        public string fund_status { get; set; }
        public string frozen_status { get; set; }
        public int escrow_fee_rate { get; set; }
        public EscrowFee escrow_fee { get; set; }
        public string end_reason { get; set; }
        public string buyer_signer_fullname { get; set; }
        public string buyer_login_id { get; set; }
        public string biz_type { get; set; }
    }

    public class TargetList
    {
        public List<OrderDto> order_dto { get; set; }
    }

    public class ResultData
    {
        public string error_message { get; set; }
        public int total_count { get; set; }
        public List<TargetList> target_list { get; set; }
        public int page_size { get; set; }
        public string error_code { get; set; }
        public int current_page { get; set; }
        public int total_page { get; set; }
        public bool success { get; set; }
        public string time_stamp { get; set; }
    }

    public class AliexpressSolutionOrderGetResponse
    {
        public ResultData result { get; set; }
    }
}
