using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Protocols
{
    public static class ProtocolConstants
    {
        public const string DiscoveryDirName = "RevitMcp";
        public const string DiscoveryFileFormat = "revit-{0}.json";

        public const int MaxMessageSize = 10_000_000;
        public const int HeaderSize = 4;

        public const string ActionEcho = "echo";
        public const string ActionGetCategoryInfo = "get_category_info";
        
        public const string ActionGetProjectInfo = "get_project_info";
        public const string ActionGetModelStatistics = "get_model_statistics";
        public const string ActionGetLevelInfo = "get_level_info";
        public const string ActionGetViewInfo = "get_view_info";
        public const string ActionGetViewSheetsInfo = "get_view_sheets_info";
        public const string ActionGetProjectFamilyTypes = "get_project_family_types";
        public const string ActionGetFamilyTypeParameters = "get_family_type_parameters";
        public const string ActionGetElementInstanceParameters = "get_element_instance_parameters";
        public const string ActionGetElementIdsByCategoryId = "get_element_ids_by_categoryId";
        public const string ActionIdentifyUnknownElementId = "identify_unknown_elementid";
        public const string ActionGetElementLocation = "get_element_location";
        public const string ActionGetElementGeometry = "get_element_geometry";
        public const string ActionGetHostOpeningsAndCuts = "get_host_openings_and_cuts";
        public const string ActionGetSpatialRelation = "get_spatial_relation";
    }
}
//定义一些通信两方都需要用到和同一的固定值，比如消息头长度，最大消息长度，发现文件路径等。还有各种方法发对应名。