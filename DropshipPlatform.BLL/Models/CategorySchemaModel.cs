using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DropshipPlatform.BLL.Models
{

    public class CategoryId
    {
        public string title { get; set; }
        public string type { get; set; }
        public int @const { get; set; }
        public int @default { get; set; }
    }

    public class OneOf
    {
        public string @const { get; set; }
        public string title { get; set; }
    }

    public class Locale
    {
        public string title { get; set; }
        public string type { get; set; }
        public List<OneOf> oneOf { get; set; }
    }

    public class Title
    {
        public string title { get; set; }
        public string type { get; set; }
        public int maxLength { get; set; }
        public string description { get; set; }
    }

    public class Properties2
    {
        public Locale locale { get; set; }
        public Title title { get; set; }
    }

    public class Items
    {
        public string type { get; set; }
        public List<string> required { get; set; }
        public Properties2 properties { get; set; }
    }


    public class OneOf2
    {
        public string @const { get; set; }
        public string title { get; set; }
    }

    public class Locale2
    {
        public string title { get; set; }
        public string type { get; set; }
        public List<OneOf2> oneOf { get; set; }
    }

    public class OneOf4
    {
        public string @const { get; set; }
        public string title { get; set; }
    }

    public class Type
    {
        public string title { get; set; }
        public string type { get; set; }
        public List<OneOf4> oneOf { get; set; }
    }

    public class Content
    {
        public string type { get; set; }
        public string title { get; set; }
    }

    public class Properties5
    {
        public Content content { get; set; }
    }


    public class Properties4
    {
        public Type type { get; set; }
        public Html html { get; set; }
    }

    public class OneOf3
    {
        public string type { get; set; }
        public string title { get; set; }
        public List<string> required { get; set; }
        public Properties4 properties { get; set; }
    }

    public class Items3
    {
        public List<OneOf3> oneOf { get; set; }
    }

 

    public class Properties3
    {
        public Locale2 locale { get; set; }
        public ModuleList module_list { get; set; }
    }

    public class Items2
    {
        public string type { get; set; }
        public List<string> required { get; set; }
        public Properties3 properties { get; set; }
    }


    public class OneOf5
    {
        public string @const { get; set; }
        public string title { get; set; }
    }

    public class Locale3
    {
        public string title { get; set; }
        public string type { get; set; }
        public List<OneOf5> oneOf { get; set; }
    }

    public class OneOf6
    {
        public string @const { get; set; }
        public string title { get; set; }
    }

    public class ProductUnitsType
    {
        public string title { get; set; }
        public string type { get; set; }
        public List<OneOf6> oneOf { get; set; }
    }

    public class Items4
    {
        public string type { get; set; }
    }

    public class ImageUrlList
    {
        public string title { get; set; }
        public string type { get; set; }
        public Items4 items { get; set; }
        public int minItems { get; set; }
        public int maxItems { get; set; }
        public bool uniqueItems { get; set; }
        public string description { get; set; }
    }

    public class OneOf7
    {
        public string @const { get; set; }
        public string title { get; set; }
    }

    public class Value
    {
        public List<OneOf7> oneOf { get; set; }
        public string title { get; set; }
        public string type { get; set; }
    }

    public class Properties7
    {
        public Value value { get; set; }
    }


    public class Then
    {
        public List<string> required { get; set; }
    }

    public class Value2
    {
        public int @const { get; set; }
    }

    public class Properties8
    {
        public Value2 value { get; set; }
    }

    public class If
    {
        public Properties8 properties { get; set; }
    }

    public class CustomValue
    {
        public string title { get; set; }
        public string type { get; set; }
    }

    public class OneOf8
    {
        public string @const { get; set; }
        public string title { get; set; }
    }

    public class Value3
    {
        public List<OneOf8> oneOf { get; set; }
        public string title { get; set; }
        public string type { get; set; }
    }

    public class Properties9
    {
        public CustomValue customValue { get; set; }
        public Value3 value { get; set; }
    }

    public class ShirtsType
    {
        public Then then { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public If @if { get; set; }
        public List<string> required { get; set; }
        public Properties9 properties { get; set; }
    }

    public class OneOf9
    {
        public string @const { get; set; }
        public string title { get; set; }
    }

    public class Items5
    {
        public List<OneOf9> oneOf { get; set; }
        public string type { get; set; }
    }

    public class Value4
    {
        public bool uniqueItems { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public Items5 items { get; set; }
    }

    public class Properties10
    {
        public Value4 value { get; set; }
    }

    public class Material
    {
        public string title { get; set; }
        public string type { get; set; }
        public List<string> required { get; set; }
        public Properties10 properties { get; set; }
    }

    public class OneOf10
    {
        public string @const { get; set; }
        public string title { get; set; }
    }

    public class Value5
    {
        public List<OneOf10> oneOf { get; set; }
        public string title { get; set; }
        public string type { get; set; }
    }

    public class Properties11
    {
        public Value5 value { get; set; }
    }

    public class SleeveLengthCm
    {
        public string title { get; set; }
        public string type { get; set; }
        public List<string> required { get; set; }
        public Properties11 properties { get; set; }
    }

    public class OneOf11
    {
        public string @const { get; set; }
        public string title { get; set; }
    }

    public class Value6
    {
        public List<OneOf11> oneOf { get; set; }
        public string title { get; set; }
        public string type { get; set; }
    }

    public class Properties12
    {
        public Value6 value { get; set; }
    }

    public class Collar
    {
        public string title { get; set; }
        public string type { get; set; }
        public List<string> required { get; set; }
        public Properties12 properties { get; set; }
    }

    public class OneOf12
    {
        public string @const { get; set; }
        public string title { get; set; }
    }

    public class Value7
    {
        public List<OneOf12> oneOf { get; set; }
        public string title { get; set; }
        public string type { get; set; }
    }

    public class Properties13
    {
        public Value7 value { get; set; }
    }

    public class Style
    {
        public string title { get; set; }
        public string type { get; set; }
        public List<string> required { get; set; }
        public Properties13 properties { get; set; }
    }

    public class OneOf13
    {
        public string @const { get; set; }
        public string title { get; set; }
    }

    public class Value8
    {
        public List<OneOf13> oneOf { get; set; }
        public string title { get; set; }
        public string type { get; set; }
    }

    public class Properties14
    {
        public Value8 value { get; set; }
    }

    public class FabricType
    {
        public string title { get; set; }
        public string type { get; set; }
        public List<string> required { get; set; }
        public Properties14 properties { get; set; }
    }

    public class Value9
    {
        public string title { get; set; }
        public string type { get; set; }
    }

    public class Properties15
    {
        public Value9 value { get; set; }
    }

    public class ModelNumber
    {
        public string title { get; set; }
        public string type { get; set; }
        public List<string> required { get; set; }
        public Properties15 properties { get; set; }
    }

    public class OneOf14
    {
        public string @const { get; set; }
        public string title { get; set; }
    }

    public class Value10
    {
        public List<OneOf14> oneOf { get; set; }
        public string title { get; set; }
        public string type { get; set; }
    }

    public class Properties16
    {
        public Value10 value { get; set; }
    }

    public class SleeveStyle
    {
        public string title { get; set; }
        public string type { get; set; }
        public List<string> required { get; set; }
        public Properties16 properties { get; set; }
    }

    public class Then2
    {
        public List<string> required { get; set; }
    }

    public class Value11
    {
        public int @const { get; set; }
    }

    public class Properties17
    {
        public Value11 value { get; set; }
    }

    public class If2
    {
        public Properties17 properties { get; set; }
    }

    public class CustomValue2
    {
        public string title { get; set; }
        public string type { get; set; }
    }

    public class OneOf15
    {
        public string @const { get; set; }
        public string title { get; set; }
    }

    public class Value12
    {
        public List<OneOf15> oneOf { get; set; }
        public string title { get; set; }
        public string type { get; set; }
    }

    public class Properties18
    {
        public CustomValue2 customValue { get; set; }
        public Value12 value { get; set; }
    }

    public class PatternType
    {
        public Then2 then { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public If2 @if { get; set; }
        public List<string> required { get; set; }
        public Properties18 properties { get; set; }
    }

    public class OneOf16
    {
        public string @const { get; set; }
        public string title { get; set; }
    }

    public class Value13
    {
        public List<OneOf16> oneOf { get; set; }
        public string title { get; set; }
        public string type { get; set; }
    }

    public class Properties19
    {
        public Value13 value { get; set; }
    }

    public class ClosureType
    {
        public string title { get; set; }
        public string type { get; set; }
        public List<string> required { get; set; }
        public Properties19 properties { get; set; }
    }

    public class Then3
    {
        public List<string> required { get; set; }
    }

    public class Value14
    {
        public int @const { get; set; }
    }

    public class Properties20
    {
        public Value14 value { get; set; }
    }

    public class If3
    {
        public Properties20 properties { get; set; }
    }

    public class CustomValue3
    {
        public string title { get; set; }
        public string type { get; set; }
    }

    public class OneOf17
    {
        public string @const { get; set; }
        public string title { get; set; }
    }

    public class Value15
    {
        public List<OneOf17> oneOf { get; set; }
        public string title { get; set; }
        public string type { get; set; }
    }

    public class Properties21
    {
        public CustomValue3 customValue { get; set; }
        public Value15 value { get; set; }
    }

    public class PlaceOfOrigin
    {
        public Then3 then { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public If3 @if { get; set; }
        public List<string> required { get; set; }
        public Properties21 properties { get; set; }
    }

    public class Properties6
    {
        [JsonProperty("Brand Name")]
        public BrandName BrandName { get; set; }

        [JsonProperty("Material")]
        public Material material { get; set; }

        [JsonProperty("Sleeve Length(cm)")]
        public SleeveLengthCm SleeveLength { get; set; }

        [JsonProperty("Collar")]
        public Collar collar { get; set; }
    }

    public class AttributeName
    {
        public string title { get; set; }
        public string type { get; set; }
    }

    public class AttributeValue
    {
        public string title { get; set; }
        public string type { get; set; }
    }

    public class Properties22
    {
        public AttributeName attribute_name { get; set; }
        public AttributeValue attribute_value { get; set; }
    }

    public class Items6
    {
        public string type { get; set; }
        public List<string> required { get; set; }
        public Properties22 properties { get; set; }
    }

    public class UserDefinedAttributeList
    {
        public string title { get; set; }
        public string type { get; set; }
        public Items6 items { get; set; }
    }

    public class SkuCode
    {
        public string title { get; set; }
        public string type { get; set; }
    }

    public class Inventory
    {
        public string title { get; set; }
        public string type { get; set; }
    }

    public class Price
    {
        public string title { get; set; }
        public string type { get; set; }
    }

    public class DiscountPrice
    {
        public string title { get; set; }
        public string type { get; set; }
    }

    public class Alias
    {
        public string title { get; set; }
        public string type { get; set; }
    }

    public class OneOf18
    {
        public string @const { get; set; }
        public string title { get; set; }
    }

    public class Value16
    {
        public List<OneOf18> oneOf { get; set; }
        public string title { get; set; }
        public string type { get; set; }
    }

    public class Properties25
    {
        public Alias alias { get; set; }
        public Value16 value { get; set; }
    }

    public class Alias2
    {
        public string title { get; set; }
        public string type { get; set; }
    }

    public class SkuImageUrl
    {
        public string title { get; set; }
        public string type { get; set; }
    }

    public class OneOf19
    {
        public string @const { get; set; }
        public string title { get; set; }
    }

    public class Value17
    {
        public List<OneOf19> oneOf { get; set; }
        public string title { get; set; }
        public string type { get; set; }
    }

    public class Properties26
    {
        public Alias2 alias { get; set; }
        public SkuImageUrl sku_image_url { get; set; }
        public Value17 value { get; set; }
    }

    public class OneOf20
    {
        public string @const { get; set; }
        public string title { get; set; }
    }

    public class Value18
    {
        public List<OneOf20> oneOf { get; set; }
        public string title { get; set; }
        public string type { get; set; }
    }

    public class Properties27
    {
        public Value18 value { get; set; }
    }

    public class SaleByPack
    {
        public string title { get; set; }
        public string type { get; set; }
        public List<string> required { get; set; }
        public Properties27 properties { get; set; }
    }

    public class Properties24
    {
        public AliSize Size { get; set; }
        public AliColor Color { get; set; }
    }
    public class AliSize
    {
        public string title { get; set; }
        public string type { get; set; }
        public List<string> required { get; set; }
        public Properties25 properties { get; set; }
    }

    public class AliColor
    {
        public string title { get; set; }
        public string type { get; set; }
        public List<string> required { get; set; }
        public Properties26 properties { get; set; }
    }
    public class Properties23
    {
        public SkuCode sku_code { get; set; }
        public Inventory inventory { get; set; }
        public Price price { get; set; }
        public DiscountPrice discount_price { get; set; }
        public AliSkuAttributes sku_attributes { get; set; }
    }

    public class Items7
    {
        public string type { get; set; }
        public List<string> required { get; set; }
        public Properties23 properties { get; set; }
    }

    public class OneOf21
    {
        public string @const { get; set; }
        public string title { get; set; }
    }

    public class InventoryDeductionStrategy
    {
        public string title { get; set; }
        public string type { get; set; }
        public List<OneOf21> oneOf { get; set; }
        public string description { get; set; }
    }

    public class PackageWeight
    {
        public string title { get; set; }
        public string type { get; set; }
        public int maximum { get; set; }
        public string description { get; set; }
    }

    public class PackageLength
    {
        public string title { get; set; }
        public string type { get; set; }
        public int maximum { get; set; }
        public string description { get; set; }
    }

    public class PackageHeight
    {
        public string title { get; set; }
        public string type { get; set; }
        public int maximum { get; set; }
        public string description { get; set; }
    }

    public class PackageWidth
    {
        public string title { get; set; }
        public string type { get; set; }
        public int maximum { get; set; }
        public string description { get; set; }
    }

    public class ShippingPreparationTime
    {
        public string title { get; set; }
        public string type { get; set; }
        public int maximum { get; set; }
        public string description { get; set; }
    }

    public class OneOf22
    {
        public string @const { get; set; }
        public string title { get; set; }
    }

    public class ShippingTemplateId
    {
        public string title { get; set; }
        public string type { get; set; }
        public List<OneOf22> oneOf { get; set; }
    }

    public class OneOf23
    {
        public string @const { get; set; }
        public string title { get; set; }
    }

    public class ServiceTemplateId
    {
        public string title { get; set; }
        public string type { get; set; }
        public List<OneOf23> oneOf { get; set; }
    }

    public class ProductGroupId
    {
        public string title { get; set; }
        public string type { get; set; }
        public List<object> oneOf { get; set; }
    }

    public class SizeChartId
    {
        public string title { get; set; }
        public string type { get; set; }
        public List<object> oneOf { get; set; }
    }

    public class OneOf24
    {
        public string @const { get; set; }
        public string title { get; set; }
    }

    public class PriceType
    {
        public string title { get; set; }
        public string type { get; set; }
        public List<OneOf24> oneOf { get; set; }
    }

    public class OneOf25
    {
        public string @const { get; set; }
        public string title { get; set; }
    }

    public class ShipToCountry
    {
        public string title { get; set; }
        public string type { get; set; }
        public List<OneOf25> oneOf { get; set; }
    }

    public class SkuCode2
    {
        public string title { get; set; }
        public string type { get; set; }
    }

    public class Price2
    {
        public string title { get; set; }
        public string type { get; set; }
    }

    public class DiscountPrice2
    {
        public string title { get; set; }
        public string type { get; set; }
    }

    public class Properties30
    {
        public SkuCode2 sku_code { get; set; }
        public Price2 price { get; set; }
        public DiscountPrice2 discount_price { get; set; }
    }

    public class Items9
    {
        public string type { get; set; }
        public List<string> required { get; set; }
        public Properties30 properties { get; set; }
    }

    public class SkuPriceByCountryList
    {
        public string title { get; set; }
        public string type { get; set; }
        public int minItems { get; set; }
        public Items9 items { get; set; }
    }

    public class Properties29
    {
        public ShipToCountry ship_to_country { get; set; }
        public SkuPriceByCountryList sku_price_by_country_list { get; set; }
    }

    public class Items8
    {
        public string type { get; set; }
        public List<string> required { get; set; }
        public Properties29 properties { get; set; }
    }

    public class CountryPriceList
    {
        public string title { get; set; }
        public string type { get; set; }
        public int minItems { get; set; }
        public Items8 items { get; set; }
    }

    public class Properties28
    {
        public PriceType price_type { get; set; }
        public CountryPriceList country_price_list { get; set; }
    }

    public class MultiCountryPriceConfiguration
    {
        public string title { get; set; }
        public string type { get; set; }
        public string description { get; set; }
        public List<string> required { get; set; }
        public Properties28 properties { get; set; }
    }

    public class Properties
    {
        public CategoryId category_id { get; set; }
        public TitleMultiLanguageList title_multi_language_list { get; set; }
        public DescriptionMultiLanguageList description_multi_language_list { get; set; }
        public Locale3 locale { get; set; }
        public ProductUnitsType product_units_type { get; set; }
        public ImageUrlList image_url_list { get; set; }
        public CategoryAttributes category_attributes { get; set; }
        public UserDefinedAttributeList user_defined_attribute_list { get; set; }
        public AliSkuInfoList sku_info_list { get; set; }
        public InventoryDeductionStrategy inventory_deduction_strategy { get; set; }
        public PackageWeight package_weight { get; set; }
        public PackageLength package_length { get; set; }
        public PackageHeight package_height { get; set; }
        public PackageWidth package_width { get; set; }
        public ShippingPreparationTime shipping_preparation_time { get; set; }
        public ShippingTemplateId shipping_template_id { get; set; }
        public ServiceTemplateId service_template_id { get; set; }
        public ProductGroupId product_group_id { get; set; }
        public SizeChartId size_chart_id { get; set; }
        public MultiCountryPriceConfiguration multi_country_price_configuration { get; set; }
    }
    public class AliSkuInfoList
    {
        public string title { get; set; }
        public string type { get; set; }
        public int minItems { get; set; }
        public Items7 items { get; set; }
    }
    public class AliSkuAttributes
    {
        public string title { get; set; }
        public string type { get; set; }
        public List<object> required { get; set; }
        public Properties24 properties { get; set; }
    }

    public class CategorySchemaModel
    {
        //public string __invalid_name__$schema { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        public List<string> required { get; set; }
        public Properties properties { get; set; }
    }
}
