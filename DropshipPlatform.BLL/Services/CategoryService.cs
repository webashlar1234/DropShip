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
using DropshipPlatform.Entity;

namespace DropshipPlatform.BLL.Services
{
    public class CategoryService
    {
        readonly log4net.ILog logger = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        public List<aliexpresscategory> getAliExpressCategory()
        {
            string sessionKey = SessionManager.GetAccessToken().access_token;
            ITopClient client = new DefaultTopClient(StaticValues.aliURL, StaticValues.aliAppkey, StaticValues.aliSecret);
            List<aliexpresscategory> list = new List<aliexpresscategory>();

            list = getAliExpressChildCategory(client, sessionKey, 0, list);
            addAliExpressCategories(list);
            return list;
        }

        public List<aliexpresscategory> getAliExpressChildCategory(ITopClient client, string sessionKey, long? CategoryId, List<aliexpresscategory> list)
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

        public bool addAliExpressCategories(List<aliexpresscategory> categories)
        {
            bool result = true;

            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    foreach (aliexpresscategory category in categories)
                    {
                        datacontext.aliexpresscategories.Add(category);
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

        public aliexpresscategory convertToCategoryObj(AeopPostCategoryDtoDomain obj, long? parentCategoryId)
        {
            aliexpresscategory category = new aliexpresscategory();
            category.AliExpressCategoryIsLeaf = obj.Isleaf;
            category.AliExpressCategoryLevel = (int)obj.Level;
            category.AliExpressCategoryID = (int)obj.Id;
            category.AliExpressCategoryName = JsonConvert.DeserializeObject<multiLanguage>(obj.Names).en;
            category.AliExpressParentCategoryID = (int)parentCategoryId;
            return category;
        }

        public List<aliexpresscategory> getlocalAliExpressCategories()
        {
            List<aliexpresscategory> list = new List<aliexpresscategory>();

            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    list = datacontext.aliexpresscategories.ToList();
                }
            }
            catch (Exception ex)
            {
                logger.Info(ex.ToString());
            }

            return list;
        }

        public List<category> getCategories()
        {
            List<category> list = new List<category>();

            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    list = datacontext.categories.ToList();
                }
            }
            catch (Exception ex)
            {
                logger.Info(ex.ToString());
            }

            return list;
        }
        public List<category> getCategoriesOnlyAvailableProd()
        {
            List<category> list = new List<category>();

            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    list = (from c in datacontext.categories
                            from p in datacontext.products.Where(x => x.CategoryID == c.CategoryID)
                            where p.ParentProductID == null && !string.IsNullOrEmpty(p.Cost) && p.IsActive == 1
                            select c).GroupBy(x => x.CategoryID).Select(g => g.FirstOrDefault()).ToList();
                }
            }
            catch (Exception ex)
            {
                logger.Info(ex.ToString());
            }

            return list;
        }

        public bool MapCategory(category category)
        {
            bool result = true;

            try
            {
                using (DropshipDataEntities datacontext = new DropshipDataEntities())
                {
                    category obj = datacontext.categories.Where(x => x.CategoryID == category.CategoryID).FirstOrDefault();
                    if (obj != null)
                    {
                        obj.AliExpresscategoryName = category.AliExpresscategoryName;
                        obj.AliExpressCategoryID = category.AliExpressCategoryID;
                        obj.ItemModifyBy = "1";
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
