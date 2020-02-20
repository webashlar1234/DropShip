using System.Web;
using System.Web.Optimization;

namespace DropshipPlatform
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/layoutBundles/jquery").Include(
                     "~/Scripts/jquery-3.3.1.min.js",
                    "~/Scripts/popper.min.js",
                    "~/Scripts/js/bootstrap.min.js",
                     "~/Scripts/jquery.dataTables.min.js",
                     "~/Scripts/dataTables.buttons.min.js",
                     "~/Scripts/dataTables.select.min.js",
                     "~/Scripts/dataTables.editor.min.js",
                     "~/Scripts/bootstrap-select.min.js",
                     "~/Scripts/toastr.min.js",
                     "~/Scripts/custom/site.js",
                     "~/Scripts/custom/globalJS.js",
                     "~/Scripts/custom/Error.js"));

            bundles.Add(new StyleBundle("~/layoutContent/css").Include(
                   "~/Content/bootstrap.min.css",
                   "~/Content/font-awesome.min.css",
                   "~/Content/style.css",
                   "~/Content/site.css",
                   "~/Content/responsive.css",
                   "~/Content/jquery.dataTables.min.css",
                   "~/Content/buttons.dataTables.min.css",
                   "~/Content/select.dataTables.min.css",
                   "~/Content/editor.dataTables.min.css",
                   "~/Content/bootstrap-select.min.css",
                   "~/Content/toastr.min.css",
                   "~/Content/custom/custom.css"
                   ));

        }
    }
}
