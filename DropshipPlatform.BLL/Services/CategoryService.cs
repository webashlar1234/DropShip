using DropshipPlatform.BLL.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Top.Api;
using Top.Api.Request;
using Top.Api.Response;
using static Top.Api.Response.AliexpressCategoryRedefiningGetchildrenpostcategorybyidResponse;
using DropshipPlatform.DLL;

namespace DropshipPlatform.BLL.Services
{
    public class CategoryService
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        string appkey = System.Web.Configuration.WebConfigurationManager.AppSettings["AliExpress_appKey"].ToString();
        string secret = System.Web.Configuration.WebConfigurationManager.AppSettings["AliExpress_appSecreat"].ToString();
        string redirectURL = System.Web.Configuration.WebConfigurationManager.AppSettings["AliExpress_RedirectURL"].ToString();
        string url = System.Web.Configuration.WebConfigurationManager.AppSettings["AliExpress_URL"].ToString();

        public List<AliExpressCategory> getAliExpressCategory()
        {
            string sessionKey = SessionManager.GetAccessToken().access_token;
            ITopClient client = new DefaultTopClient(url, appkey, secret);
            List<AliExpressCategory> list = new List<AliExpressCategory>();

            list = getAliExpressChildCategory(client, sessionKey, 0, list);
            addAliExpressCategories(list);
            return list;
        }

        public List<AliExpressCategory> getAliExpressChildCategory(ITopClient client, string sessionKey, long? CategoryId, List<AliExpressCategory> list)
        {
            try
            {
                AliexpressCategoryRedefiningGetchildrenpostcategorybyidRequest req = new AliexpressCategoryRedefiningGetchildrenpostcategorybyidRequest();
                req.Param0 = CategoryId;
                AliexpressCategoryRedefiningGetchildrenpostcategorybyidResponse rsp = client.Execute(req);

                foreach (AeopPostCategoryDtoDomain category in rsp.Result.AeopPostCategoryList)
                {
                    
                    list.Add(convertToCategoryObj(category, CategoryId));
                    if (!category.Isleaf)
                    {
                        getAliExpressChildCategory(client, sessionKey, category.Id, list);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(ex.ToString());
            }
            return list;
        }

        public bool addAliExpressCategories(List<AliExpressCategory> categories)
        {
            bool result = true;

            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    foreach (AliExpressCategory category in categories)
                    {
                        datacontext.AliExpressCategories.Add(category);
                    }
                    datacontext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                result = false;
                logger.Info(ex.ToString());
            }

            return result;
        }

        public AliExpressCategory convertToCategoryObj(AeopPostCategoryDtoDomain obj, long? parentCategoryId)
        {
            AliExpressCategory category = new AliExpressCategory();
            category.AliExpressCategoryIsLeaf = obj.Isleaf;
            category.AliExpressCategoryLevel = (int)obj.Level;
            category.AliExpressCategoryID = (int)obj.Id;
            category.AliExpressCategoryName = JsonConvert.DeserializeObject<multiLanguage>(obj.Names).en;
            category.AliExpressParentCategoryID = (int)parentCategoryId;
            return category;
        }

        public List<AliExpressCategory> getlocalAliExpressCategories()
        {
            List<AliExpressCategory> list = new List<AliExpressCategory>();

            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    list = datacontext.AliExpressCategories.ToList();
                }
            }
            catch (Exception ex)
            {
                logger.Info(ex.ToString());
            }

            return list;
        }

        public List<Category> getCategories()
        {
            List<Category> list = new List<Category>();

            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    list = datacontext.Categories.ToList();
                }
            }
            catch (Exception ex)
            {
                logger.Info(ex.ToString());
            }

            return list;
        }

        public bool MapCategory(Category category)
        {
            bool result = true;

            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    Category obj = datacontext.Categories.Where(x => x.CategoryID == category.CategoryID).FirstOrDefault();
                    if (obj != null)
                    {
                        obj.AliExpressCategoryName = category.AliExpressCategoryName;
                        obj.AliExpressCategoryId = category.AliExpressCategoryId;
                        obj.ItemModifyBy = 1;
                        obj.ItemModifyWhen = DateTime.UtcNow;
                    }
                    datacontext.Entry(obj).State = System.Data.Entity.EntityState.Modified;
                    datacontext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                result = false;
                logger.Info(ex.ToString());
            }

            return result;
        }
    }
}
