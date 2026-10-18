using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelContextProtocol.Server;
using Shared.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Shared.Models;

namespace Servers
{
    [McpServerToolType]
    public class InfoTool
    {
        public readonly RevitTcpClient _client;
        public InfoTool(RevitTcpClient client)
        {
            _client = client;
        }
        [McpServerTool]
        [Description("获取当前文档或激活视图中的类别统计信息，若为当前文档则传入的bool值为true，若传入激活视图则传入false，仅包含类别名称+类别Id、数量和总元素数，需遍历所有元素，有时较慢")]
        public async Task<string> GetCategoryInfo(bool allOrActiveView)
        {
            var request = new Shared.Models.TcpRequest
            {
                Action = ProtocolConstants.ActionGetCategoryInfo,
                RequestId = Guid.NewGuid().ToString(),
                Parameters = new Dictionary<string, string>
                {
                    { "scope", allOrActiveView.ToString() },
                },
            };
            Shared.Models.TcpResponse response = await _client.SendAsync(request);
            if (!response.Success)
                return "失败: " + response.Message;
            return System.Text.Json.JsonSerializer.Serialize(response.Data, Shared.Protocols.JsonConfig.Options);
        }
        
        [McpServerTool]
        [Description("获取项目的基本信息，包括项目名称、作者、路径、编号、地址、建筑名称、客户名称、发布日期、状态、组织名称、组织描述和楼层信息（标高为英尺）")]
        public async Task<string> GetProjectInfo()
        {
            var request = new Shared.Models.TcpRequest
            {
                Action = ProtocolConstants.ActionGetProjectInfo,
                RequestId = Guid.NewGuid().ToString(),
                Parameters = new Dictionary<string, string>
                {
                     
                },
            };
            Shared.Models.TcpResponse response = await _client.SendAsync(request);
            if (!response.Success)
                return "失败: " + response.Message;
            return System.Text.Json.JsonSerializer.Serialize(response.Data, Shared.Protocols.JsonConfig.Options);
        }
        [McpServerTool]
        [Description("获取模型的粗略的统计信息，包括总元素数、总族数、总视图数、总图纸数、总房间数、文件大小(MB)")]
        public async Task<string> GetModelStatistics()
        {
            var request = new Shared.Models.TcpRequest
            {
                Action = ProtocolConstants.ActionGetModelStatistics,
                RequestId = Guid.NewGuid().ToString(),
                Parameters = new Dictionary<string, string>
                {
                     
                },
            };
            Shared.Models.TcpResponse response = await _client.SendAsync(request);
            if (!response.Success)
                return "失败: " + response.Message;
            return System.Text.Json.JsonSerializer.Serialize(response.Data, Shared.Protocols.JsonConfig.Options);
        }
        [McpServerTool]
        [Description("快速获取当前文档中的楼层信息，包括楼层名称、楼层标高（英尺）和楼层 ID")]
        public async Task<string> GetLevelInfo()
        {
            var request = new Shared.Models.TcpRequest
            {
                Action = ProtocolConstants.ActionGetLevelInfo,
                RequestId = Guid.NewGuid().ToString(),
                Parameters = new Dictionary<string, string>
                {
                     
                },
            };
            Shared.Models.TcpResponse response = await _client.SendAsync(request);
            if (!response.Success)
                return "失败: " + response.Message;
            return System.Text.Json.JsonSerializer.Serialize(response.Data, Shared.Protocols.JsonConfig.Options);
        }
        [McpServerTool]
        [Description("快速获取当前文档中的视图信息，包括视图名称、视图类型、视图 ID 、是否为激活视图、是否为模板视图、视图族类型、比例、细节级别和是否有效")]
        public async Task<string> GetViewInfo()
        {
            var request = new Shared.Models.TcpRequest
            {
                Action = ProtocolConstants.ActionGetViewInfo,
                RequestId = Guid.NewGuid().ToString(),
                Parameters = new Dictionary<string, string>
                {
                     
                },
            };
            Shared.Models.TcpResponse response = await _client.SendAsync(request);
            if (!response.Success)
                return "失败: " + response.Message;
            return System.Text.Json.JsonSerializer.Serialize(response.Data, Shared.Protocols.JsonConfig.Options);
        }
        [McpServerTool]
        [Description("快速获取当前文档中的所有图纸的较详细信息，包括图号、名称、图纸上的视图信息、id、标题")]
        public async Task<string> GetViewSheetsInfo()
        {
            var request = new Shared.Models.TcpRequest
            {
                Action = ProtocolConstants.ActionGetViewSheetsInfo,
                RequestId = Guid.NewGuid().ToString(),
                Parameters = new Dictionary<string, string>
                {
                     
                },
            };
            Shared.Models.TcpResponse response = await _client.SendAsync(request);
            if (!response.Success)
                return "失败: " + response.Message;
            return System.Text.Json.JsonSerializer.Serialize(response.Data, Shared.Protocols.JsonConfig.Options);
        }
        [McpServerTool]
        [Description("快速获取当前文档中的所有族类型的较详细信息，包括族名、类型名、类型ID、类别、是否为族符号/系统族/内建族")]
        public async Task<string> GetProjectFamilyTypes()
        {
            var request = new Shared.Models.TcpRequest
            {
                Action = ProtocolConstants.ActionGetProjectFamilyTypes,
                RequestId = Guid.NewGuid().ToString(),
                Parameters = new Dictionary<string, string>
                {
                     
                },
            };
            Shared.Models.TcpResponse response = await _client.SendAsync(request);
            if (!response.Success)
                return "失败: " + response.Message;
            return System.Text.Json.JsonSerializer.Serialize(response.Data, Shared.Protocols.JsonConfig.Options);
        }
        [McpServerTool]
        [Description("根据类型ID获取指定族类型的所有类型参数信息,仅可传入类型ID，输出指定族类型的参数信息")]
        public async Task<string> GetFamilyTypeParameters_byFamilyTypeId(string Id)
        {
            var request = new Shared.Models.TcpRequest
            {
                Action = ProtocolConstants.ActionGetFamilyTypeParameters,
                RequestId = Guid.NewGuid().ToString(),
                Parameters = new Dictionary<string, string>
                {
                    {"IdType","familyTypeId" },
                    { "familyTypeId", Id }
                },
            };
            Shared.Models.TcpResponse response = await _client.SendAsync(request);
            if (!response.Success)
                return "失败: " + response.Message;
            return System.Text.Json.JsonSerializer.Serialize(response.Data, Shared.Protocols.JsonConfig.Options);
        }
        [McpServerTool]
        [Description("根据实例ID获取实例对应的族类型的所有类型参数信息,仅可传入实例ID，输出实例对应的族类型参数信息")]
        public async Task<string> GetFamilyTypeParameters_byInstanceId(string Id)
        {
            var request = new Shared.Models.TcpRequest
            {
                Action = ProtocolConstants.ActionGetFamilyTypeParameters,
                RequestId = Guid.NewGuid().ToString(),
                Parameters = new Dictionary<string, string>
                {
                    {"IdType","instanceId" },
                    { "instanceId", Id }
                },
            };
            Shared.Models.TcpResponse response = await _client.SendAsync(request);
            if (!response.Success)
                return "失败: " + response.Message;
            return System.Text.Json.JsonSerializer.Serialize(response.Data, Shared.Protocols.JsonConfig.Options);
        }
        [McpServerTool]
        [Description("根据实例ID获取指定实例的所有实例参数信息,仅可传入实例ID")]
        public async Task<string> GetElementInstanceParameters(string Id)
        {
            var request = new Shared.Models.TcpRequest
            {
                Action = ProtocolConstants.ActionGetElementInstanceParameters,
                RequestId = Guid.NewGuid().ToString(),
                Parameters = new Dictionary<string, string>
                {
                    { "instanceId", Id }
                },
            };
            Shared.Models.TcpResponse response = await _client.SendAsync(request);
            if (!response.Success)
                return "失败: " + response.Message;
            return System.Text.Json.JsonSerializer.Serialize(response.Data, Shared.Protocols.JsonConfig.Options);
        }
        [McpServerTool]
        [Description("根据类别ID获取指定类别的所有元素实例ID,若类别为null，则传入“0”（数字0，即使string类型的参数categoryId为“0”）查询类别为null的所有元素实例ID，返回所有该类别的元素的实例ID、名称、类别名称、levelId")]
        public async Task<string> GetElementIdsByCategoryId(string categoryId)
        {
            var request = new Shared.Models.TcpRequest
            {
                Action = ProtocolConstants.ActionGetElementIdsByCategoryId,
                RequestId = Guid.NewGuid().ToString(),
                Parameters = new Dictionary<string, string>
                {
                    { "categoryId", categoryId }
                },
            };
            Shared.Models.TcpResponse response = await _client.SendAsync(request);
            if (!response.Success)
                return "失败: " + response.Message;
            return System.Text.Json.JsonSerializer.Serialize(response.Data, Shared.Protocols.JsonConfig.Options);
        }
        [McpServerTool]
        [Description("辨别未知的元素ID，输入元素ID，输出元素ID所属的类型名")]
        public async Task<string> IdentifyUnknownElementId(string elementId)
        {
            var request = new Shared.Models.TcpRequest
            {
                Action = ProtocolConstants.ActionIdentifyUnknownElementId,
                RequestId = Guid.NewGuid().ToString(),
                Parameters = new Dictionary<string, string>
                {
                    { "elementId", elementId }
                },
            };
            Shared.Models.TcpResponse response = await _client.SendAsync(request);
            if (!response.Success)
                return "失败: " + response.Message;
            return System.Text.Json.JsonSerializer.Serialize(response.Data, Shared.Protocols.JsonConfig.Options);
        }
        [McpServerTool]
        [Description("获取指定元素的位置信息。输入元素ID，输出位置类型(Point/Curve/None)、位置点坐标、位置线/曲线端点、朝向向量以及包围盒(min/max/center/size)。不解析几何，不受开洞/剪切影响。")]
        public async Task<string> GetElementLocation(
                    string elementId,
                    bool includeBoundingBox = true,
                    bool includeDirection = true,
                    string? viewId = null)
        {
            var parameters = new Dictionary<string, string>
                {
                    { "elementId", elementId },
                    { "includeBoundingBox", includeBoundingBox.ToString() },
                    { "includeDirection", includeDirection.ToString() }
                };

            if (!string.IsNullOrWhiteSpace(viewId))
                parameters["viewId"] = viewId;

            var request = new Shared.Models.TcpRequest
            {
                Action = ProtocolConstants.ActionGetElementLocation,
                RequestId = Guid.NewGuid().ToString(),
                Parameters = parameters,
            };

            Shared.Models.TcpResponse response = await _client.SendAsync(request);
            if (!response.Success)
                return "失败: " + response.Message;

            return System.Text.Json.JsonSerializer.Serialize(
                response.Data,
                Shared.Protocols.JsonConfig.Options);
        }
        [McpServerTool]
        [Description("获取指定元素的完整几何信息（已应用开洞与剪切后的净几何）。" +
    "返回 Solid（体积/表面积/面/边）、Curve、Mesh、GeometryInstance 的详细信息；" +
    "每个面包含面积、法向量、材质ID、可选的生成来源元素ID（用于追溯洞口）与三角面片。" +
    "当某 Solid 非封闭导致体积/表面积不可计算时，该 Solid 不含 volume/surfaceArea 字段，" +
    "并标记 invalidSolid=true，同时 warnings 数组会记录降级原因。" +
    "detailLevel: Fine/Medium/Coarse；" +
    "useSymbolGeometry=true 返回族定义坐标系，false 返回项目坐标系（默认）；" +
    "maxFaces 限制返回面数上限，超限时 truncated=true。")]
        public async Task<string> GetElementGeometry(
    string elementId,
    string detailLevel = "Fine",
    bool includeNonVisible = false,
    bool useSymbolGeometry = false,
    bool includeTriangulation = false,
    bool includeGeneratingIds = false,
    int maxFaces = 2000)
        {
            var parameters = new Dictionary<string, string>
    {
        { "elementId", elementId },
        { "detailLevel", detailLevel },
        { "includeNonVisible", includeNonVisible.ToString() },
        { "useSymbolGeometry", useSymbolGeometry.ToString() },
        { "includeTriangulation", includeTriangulation.ToString() },
        { "includeGeneratingIds", includeGeneratingIds.ToString() },
        { "maxFaces", maxFaces.ToString() }
    };

            var request = new Shared.Models.TcpRequest
            {
                Action = ProtocolConstants.ActionGetElementGeometry,
                RequestId = Guid.NewGuid().ToString(),
                Parameters = parameters,
            };

            Shared.Models.TcpResponse response = await _client.SendAsync(request);
            if (!response.Success)
                return "失败: " + response.Message;

            return System.Text.Json.JsonSerializer.Serialize(
                response.Data,
                Shared.Protocols.JsonConfig.Options);
        }
        [McpServerTool]
        [Description("查询宿主元素（墙/楼板/屋顶/天花板等 HostObject）上的开洞与剪切。" +
    "返回门、窗、矩形/非矩形洞口(Opening)、体量剪切(VoidCut)的列表，" +
    "每项含 id/name/categoryName/hostId/symbolId；矩形洞口带 boundaryRect，非矩形带 boundaryCurves，" +
    "面积带 areaSource。注意：洞口(Opening)自身不是宿主，如需其几何请用 GetElementGeometry。")]
        public async Task<string> GetHostOpeningsAndCuts(
    [Description("宿主元素 ID（墙、楼板、屋顶、天花板等 HostObject）")]
    string hostElementId,

    [Description("是否包含矩形/非矩形洞口(Opening)，默认 true")]
    bool includeRectOpenings = true,

    [Description("是否包含门/窗族实例，默认 true")]
    bool includeDoorsWindows = true,

    [Description("是否包含体量剪切(VoidCut)，默认 true")]
    bool includeVoidCuts = true,

    [Description("是否估算面积（false 可提速），默认 true")]
    bool includeAreaEstimate = true,

    [Description("是否包含跨宿主插入物（真实宿主 ≠ 请求宿主），默认 false 过滤；" +
                 "开启后结果标记 isCrossHost:true 并附带 reportedHostId")]
    bool includeCrossHostInserts = false)
        {
            var parameters = new Dictionary<string, string>
    {
        { "hostElementId", hostElementId },
        { "includeRectOpenings", includeRectOpenings.ToString() },
        { "includeDoorsWindows", includeDoorsWindows.ToString() },
        { "includeVoidCuts", includeVoidCuts.ToString() },
        { "includeAreaEstimate", includeAreaEstimate.ToString() },
        { "includeCrossHostInserts", includeCrossHostInserts.ToString() }
    };

            var request = new Shared.Models.TcpRequest
            {
                Action = ProtocolConstants.ActionGetHostOpeningsAndCuts,
                RequestId = Guid.NewGuid().ToString(),
                Parameters = parameters,
            };

            Shared.Models.TcpResponse response = await _client.SendAsync(request);
            if (!response.Success)
                return "失败: " + response.Message;

            return System.Text.Json.JsonSerializer.Serialize(
                response.Data,
                Shared.Protocols.JsonConfig.Options);
        }
        [McpServerTool]
        [Description("查询元素或点所在的空间（房间/空间）。" +
    "mode=elementToRoom：批量查询元素归属（先走 FamilyInstance.Room/Space 快速路径，" +
    "否则用位置点+IsPointInRoom）；" +
    "mode=pointToRoom：批量查询点归属（遍历所有房间调用 IsPointInRoom）。" +
    "不返回几何。如需房间的体积/表面积，请改用 GetElementGeometry。")]
        public async Task<string> GetSpatialRelation(
    [Description("查询模式：elementToRoom | pointToRoom")]
    string mode,

    [Description("mode=elementToRoom 时必填：逗号分隔的元素 ID，如 \"123,456,789\"")]
    string? elementIds = null,

    [Description("mode=pointToRoom 时必填：分号分隔的点，每点逗号分隔 xyz，如 \"1,2,3;4,5,6\"")]
    string? points = null)
        {
            var parameters = new Dictionary<string, string>
    {
        { "mode", mode }
    };

            if (!string.IsNullOrWhiteSpace(elementIds))
                parameters["elementIds"] = elementIds;

            if (!string.IsNullOrWhiteSpace(points))
                parameters["points"] = points;

            var request = new Shared.Models.TcpRequest
            {
                Action = ProtocolConstants.ActionGetSpatialRelation,
                RequestId = Guid.NewGuid().ToString(),
                Parameters = parameters,
            };

            Shared.Models.TcpResponse response = await _client.SendAsync(request);
            if (!response.Success)
                return "失败: " + response.Message;

            return System.Text.Json.JsonSerializer.Serialize(
                response.Data,
                Shared.Protocols.JsonConfig.Options);
        }
    }
}