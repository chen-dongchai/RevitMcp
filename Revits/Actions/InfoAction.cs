using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.DB.DirectContext3D;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Revits.Actions
{
    public class GetCategoryInfoAction : IRevitAction
    {
        public TcpResponse Execute(UIApplication app, TcpRequest request)
        {
            Document doc = app.ActiveUIDocument.Document;

            string scope = request.Parameters["scope"];

            var categoryCount = new Dictionary<string, int>();
            if (scope == "True")
            {
                foreach (Element ele in new FilteredElementCollector(doc)
                                    .WhereElementIsNotElementType())
                {
                    string categoryName = ele.Category != null ? ele.Category.Name + $" ({ele.Category.Id.IntegerValue})"   : "无类别";

                    if (categoryCount.TryGetValue(categoryName, out int count))
                        categoryCount[categoryName] = count + 1;
                    else
                        categoryCount[categoryName] = 1;
                }
            }
            else if (scope == "False")
            {
                View view = doc.ActiveView;
                foreach (Element ele in new FilteredElementCollector(doc, view.Id)
                                    .WhereElementIsNotElementType())
                {
                    string categoryName = ele.Category != null ? ele.Category.Name + $" ({ele.Category.Id.IntegerValue})"   : "无类别";

                    if (categoryCount.TryGetValue(categoryName, out int count))
                        categoryCount[categoryName] = count + 1;
                    else
                        categoryCount[categoryName] = 1;
                }
            }
            // 按数量降序
            var sorted = categoryCount
                .OrderByDescending(kv => kv.Value)
                .ToDictionary(kv => kv.Key, kv => kv.Value);

            return new TcpResponse
            {
                RequestId = request.RequestId,
                Success = true,
                Message = $"统计完成，共 {categoryCount.Count} 个类别",
                Data = new
                {
                    categoryCount = sorted,                          // ← 真正的数据
                    totalElements = categoryCount.Values.Sum(),      // ← 附加信息
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                }
            };
        }
    }
    public class GetProjectInfoAction : IRevitAction
    {
        public TcpResponse Execute(UIApplication app, TcpRequest request)
        {
            Document doc = app.ActiveUIDocument.Document;
            ProjectInfo projectInfo = doc.ProjectInformation;
            var levels = new FilteredElementCollector(doc)
                    .OfClass(typeof(Level))
                    .Cast<Level>()
                    .OrderBy(l => l.Elevation)
                    .Select(l => new {name = l.Name, elevation = l.Elevation })
                    .ToList();
            Dictionary<String,double> levelDict = new Dictionary<string, double>();
            foreach (var level in levels)
            {
                levelDict[level.name] = level.elevation;
            }
            return new TcpResponse
            {
                RequestId = request.RequestId,
                Success = true,
                Message = "获取项目信息成功",
                Data = new
                {
                    projectName = doc.Title,
                    projectAuthor = projectInfo != null ? projectInfo.Author : "无作者",
                    projectPath = doc.PathName,
                    projectNumber = projectInfo != null ? projectInfo.Number : "无编号",
                    projectAddress = projectInfo != null ? projectInfo.Address : "无地址",
                    projectBuildingName = projectInfo != null ? projectInfo.BuildingName : "无建筑名称",
                    projectClientName = projectInfo != null ? projectInfo.ClientName : "无客户名称",
                    projectIssueDate = projectInfo != null ? projectInfo.IssueDate: "无发布日期",
                    projectStatus = projectInfo != null ? projectInfo.Status : "无状态",
                    projectOrganizationName = projectInfo != null ? projectInfo.OrganizationName : "无组织名称",
                    projectOrganizationDescription = projectInfo != null ? projectInfo.OrganizationDescription : "无组织描述",
                    levels = levelDict,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                }
            };
        }
    }
    public class GetModelStatistics : IRevitAction
    {
        public TcpResponse Execute(UIApplication app, TcpRequest request)
        {
            // 假设 doc 已经获取
            Document doc = app.ActiveUIDocument.Document;

            // 1. 实例元素总数（不包含元素类型）
            int totalElements = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .GetElementCount();

            // 2. 加载的族数量
            int totalFamilies = new FilteredElementCollector(doc)
                .OfClass(typeof(Family))
                .GetElementCount();

            // 3. 视图数量（排除图纸）
            int totalViews = new FilteredElementCollector(doc)
                .OfClass(typeof(View))
                .Cast<View>()
                .Count(v => !(v is ViewSheet));

            // 4. 图纸数量
            int totalSheets = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSheet))
                .GetElementCount();

            // 5. 房间数量
            int totalRooms = new FilteredElementCollector(doc)
                .WhereElementIsNotElementType()
                .OfClass(typeof(SpatialElement))
                .Cast<SpatialElement>()
                .OfType<Room>()          // 后处理筛选出 Room
                .Count();

            // 6. 文件大小（MB）
            double fileSizeMB = 0;
            if (!string.IsNullOrEmpty(doc.PathName) && File.Exists(doc.PathName))
            {
                var fileInfo = new FileInfo(doc.PathName);
                fileSizeMB = Math.Round(fileInfo.Length / 1024.0 / 1024.0, 2);
            }

            // 构造返回数据
            var result = new
            {
                totalElements,
                totalFamilies,
                totalViews,
                totalSheets,
                totalRooms,
                fileSizeMB
            };

            // 放入 TcpResponse
            return new TcpResponse
            {
                RequestId = request.RequestId,
                Success = true,
                Message = "模型统计完成",
                Data = result
            };
        }
    }
    public class GetLevelInfoAction : IRevitAction
    {
        public TcpResponse Execute(UIApplication app, TcpRequest request)
        {
            Document doc = app.ActiveUIDocument.Document;
            var levels = new FilteredElementCollector(doc)
                    .OfClass(typeof(Level))
                    .Cast<Level>()
                    .OrderBy(l => l.Elevation)
                    
                    .ToList();
            Dictionary<String,Dictionary<string, string>> levelDict = new Dictionary<string, Dictionary<string, string>>();
            foreach (var level in levels)
            {
                levelDict[level.Name] = new Dictionary<string, string>
                {
                    { "elevation", level.Elevation.ToString() },
                    { "id", level.Id.IntegerValue.ToString() }
                };
            }
            return new TcpResponse
            {
                RequestId = request.RequestId,
                Success = true,
                Message = "获取楼层信息成功",
                Data = new
                {
                    levels = levelDict,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                }
            };
        }
    }
    public class GetViewInfoAction : IRevitAction
    {
        public TcpResponse Execute(UIApplication app, TcpRequest request)
        {
            Document doc = app.ActiveUIDocument.Document;
            var views = new FilteredElementCollector(doc)
                    .OfClass(typeof(View))
                    .Cast<View>()
                    .OrderBy(v => v.Name)
                    .ToList();
            Dictionary<String, Dictionary<string, string>> viewDict = new Dictionary<string, Dictionary<string, string>>();
            foreach (var view in views)
            {
                viewDict[view.Name] = new Dictionary<string, string>
                {
                    { "id", view.Id.IntegerValue.ToString() },
                    { "type", view.ViewType.ToString() },
                    {"isTemplate", view.IsTemplate.ToString() },
                    {"viewFamilyType", (doc.GetElement(view.GetTypeId()) as ViewFamilyType)?.Name ?? "未知" },
                    {"scale", view.Scale.ToString() },
                    {"detailLevel", view.DetailLevel.ToString() },
                    //{"discipline", view.Discipline.ToString() ?? "未知" },
                    {"isValid", view.IsValidObject.ToString() }
                };
            }
            return new TcpResponse
            {
                RequestId = request.RequestId,
                Success = true,
                Message = "获取视图信息成功",
                Data = new
                {
                    views = viewDict,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                }
            };
        }
    }
    public class GetViewSheetsInfoAction : IRevitAction
    {
        public TcpResponse Execute(UIApplication app, TcpRequest request)
        {
            Document doc = app.ActiveUIDocument.Document;

            IEnumerable<ViewSheet> sheets = new FilteredElementCollector(doc)
                .OfClass(typeof(ViewSheet))
                .OfCategory(BuiltInCategory.OST_Sheets)
                .Cast<ViewSheet>();

            var sheetsDic = new Dictionary<string, Dictionary<string, object>>();

            foreach (var sheet in sheets)
            {
                var sheetInfo = new Dictionary<string, object>
            {
                { "id", sheet.Id.IntegerValue.ToString() },
                { "number", sheet.SheetNumber },
                { "title", sheet.Name },
                { "isPlaceholder", sheet.IsPlaceholder }
            };

                var views = new List<Dictionary<string, string>>();

                if (!sheet.IsPlaceholder)
                {
                    foreach (ElementId vpId in sheet.GetAllViewports())
                    {
                        var vp = doc.GetElement(vpId) as Viewport;
                        if (vp == null) continue;

                        var view = doc.GetElement(vp.ViewId) as View;
                        if (view == null) continue;

                        var vft = doc.GetElement(view.GetTypeId()) as ViewFamilyType;

                        views.Add(new Dictionary<string, string>
                    {
                        { "id", view.Id.IntegerValue.ToString() },
                        { "name", view.Name },
                        { "viewType", view.ViewType.ToString() },
                        { "viewFamily", vft?.ViewFamily.ToString() ?? "Unknown" },
                        { "viewFamilyTypeName", vft?.Name ?? "Unknown" }
                    });
                    }
                }

                sheetInfo["views"] = views;

                // 用 SheetNumber 作为 key，更稳定
                sheetsDic[sheet.SheetNumber] = sheetInfo;
            }

            return new TcpResponse
            {
                RequestId = request.RequestId,
                Success = true,
                Message = "获取图纸信息成功",
                Data = new
                {
                    sheets = sheetsDic,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                }
            };
        }
    }
    public class GetProjectFamilyTypesAction : IRevitAction
    {
        public TcpResponse Execute(UIApplication app, TcpRequest request)
        {
            Document doc = app.ActiveUIDocument.Document;
            var query = new FilteredElementCollector(doc)
                    .WhereElementIsElementType()
                    .OfType<ElementType>()
                    .Where(t => t.Category != null)
                    .Select(t => new Dictionary<string, object>
                {
                    { "id", t.Id.IntegerValue },
                    { "typeName", t.Name },
                    { "familyName", t.FamilyName },
                    { "category", t.Category?.Name },
                    { "isFamilySymbol", t is FamilySymbol },
                    { "isSystemFamily", !(t is FamilySymbol) },
                    { "isInPlace", (t as FamilySymbol)?.Family?.IsInPlace ?? false }
                }).ToList();
            //还应该补充其参数信息，便于ai分析，
            return new TcpResponse
            {
                RequestId = request.RequestId,
                Success = true,
                Message = "获取族类型信息成功",
                Data = new
                {
                    query,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                }
            };
        }
    }
    public class GetFamilyTypeParametersAction : IRevitAction
    {
        public TcpResponse Execute(UIApplication app, TcpRequest request)
        {
            Document doc = app.ActiveUIDocument.Document;
            int IdInt = int.Parse(request.Parameters[request.Parameters["IdType"]]);
            ElementId typeId = new ElementId(IdInt);
            if (request.Parameters["IdType"] == "instanceId")
            {
                Element instance = doc.GetElement(typeId);
                typeId = instance?.GetTypeId() ?? ElementId.InvalidElementId;
            }       
            ElementType type = doc.GetElement(typeId) as ElementType;
            if (type == null)
            {
                return new TcpResponse
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Message = $"未找到ID {IdInt}",
                    Data = null
                };
            }
            var parameters = type.Parameters
                .Cast<Parameter>()
                .Select(p => new Dictionary<string, object>
                {
                    { "name", p.Definition.Name },
                    { "id", p.Id.IntegerValue },
                    { "storageType", p.StorageType.ToString() },
                    { "value", GetParameterValue(p) }
                }).ToList();                                  //缺少单位的值
            return new TcpResponse
            {
                RequestId = request.RequestId,
                Success = true,
                Message = $"获取类型 ID {IdInt} 的参数信息成功",
                Data = new
                {
                    parameters,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                }
            };
        }
        private object GetParameterValue(Parameter p)
        {
            switch (p.StorageType)
            {
                case StorageType.Double:
                    return p.AsDouble();
                case StorageType.Integer:
                    return p.AsInteger();
                case StorageType.String:
                    return p.AsString();
                case StorageType.ElementId:
                    return p.AsElementId().IntegerValue;
                default:
                    return null;
            }
        }
    }
    public class GetElementInstanceParametersAction : IRevitAction
    {
        public TcpResponse Execute(UIApplication app, TcpRequest request)
        {
            Document doc = app.ActiveUIDocument.Document;
            int IdInt = int.Parse(request.Parameters["instanceId"]);
            ElementId instanceId = new ElementId(IdInt);
            Element instance = doc.GetElement(instanceId);
            if (instance == null)
            {
                return new TcpResponse
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Message = $"未找到ID {IdInt}",
                    Data = null
                };
            }
            var parameters = instance.Parameters
                .Cast<Parameter>()
                .Select(p => new Dictionary<string, object>
                {
                    { "name", p.Definition.Name },
                    { "id", p.Id.IntegerValue },
                    { "storageType", p.StorageType.ToString() },
                    { "value", GetParameterValue(p) }          //缺少单位的值？
                }).ToList();
            return new TcpResponse
            {
                RequestId = request.RequestId,
                Success = true,
                Message = $"获取实例 ID {IdInt} 的参数信息成功",
                Data = new
                {
                    parameters,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                }
            };
        }
        private object GetParameterValue(Parameter p)
        {
            switch (p.StorageType)
            {
                case StorageType.Double:
                    return p.AsDouble();
                case StorageType.Integer:
                    return p.AsInteger();
                case StorageType.String:
                    return p.AsString();
                case StorageType.ElementId:
                    return p.AsElementId().IntegerValue;
                default:
                    return null;
            }
        }
    }
    public class GetElementIdsByCategoryIdAction : IRevitAction
    {
        public TcpResponse Execute(UIApplication app, TcpRequest request)
        {
            Document doc = app.ActiveUIDocument.Document;
            int categoryIdInt = int.Parse(request.Parameters["categoryId"]);
            ElementId categoryId = new ElementId(categoryIdInt);
            var elements = new FilteredElementCollector(doc);
            var elementIds = new List<Dictionary<string, string>>();
            if (categoryIdInt !=0)
            {
                if(Category.GetCategory(doc, categoryId) == null)
                {
                    return new TcpResponse
                    {
                        RequestId = request.RequestId,
                        Success = false,
                        Message = $"{categoryIdInt}不是类别id",
                        Data = null
                    };
                }
                elementIds = elements
                    .WhereElementIsNotElementType()
                    .Where(e => e.Category != null && e.Category.Id == categoryId)
                    .Select(e => new Dictionary<string, string>
                    {
                        { "elementId", e.Id.IntegerValue.ToString() },
                        { "elementName", e.Name ?? "无名称" },
                        { "categoryName", e.Category.Name },
                        {"elementLevelId",e.LevelId?.IntegerValue.ToString() ?? "null" }
                    }).ToList();
            }
            else
            {
                elementIds = elements
                    .WhereElementIsNotElementType()
                    .Where(e => e.Category == null)
                    .Select(e => new Dictionary<string, string>
                    {
                        { "elementId", e.Id.IntegerValue.ToString() },
                        { "elementName", e.Name ?? "无名称" },
                        { "categoryName", "无类别" },
                        {"elementLevelId",e.LevelId?.IntegerValue.ToString() ?? "null" }
                    }).ToList();
            }
            return new TcpResponse
            {
                RequestId = request.RequestId,
                Success = true,
                Message = $"获取类别 ID {categoryIdInt} 的元素实例 ID 成功",
                Data = new
                {
                    elementIds,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                }
            };
        }
    }
    public class IdentifyUnknownElementIDAction : IRevitAction
    {
        public TcpResponse Execute(UIApplication app, TcpRequest request)
        {
            Document doc = app.ActiveUIDocument.Document;
            int elementIdInt = int.Parse(request.Parameters["elementId"]);
            ElementId elementId = new ElementId(elementIdInt);
            Element element = doc.GetElement(elementId);
            if (element == null)
            {
                return new TcpResponse
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Message = $"未找到ID {elementIdInt},ID无效",
                    Data = null
                };
            }
            string belong = "instance";
            if (element is SharedParameterElement) belong = "sharedParameterElement";
            else if (element is GlobalParameter) belong = "globalParameter";
            else if (element is ParameterElement) belong = "parameterElement";
            else if (element is RevitLinkType) belong = "revitLinkType";

            // 族与组类型（继承自 ElementType，必须在 ElementType 之前）
            else if (element is FamilySymbol) belong = "familySymbol";
            else if (element is GroupType) belong = "groupType";
            else if (element is AssemblyType) belong = "assemblyType";

            // 视图与标注类型（继承自 ElementType，必须在 ElementType 之前）
            else if (element is ViewFamilyType) belong = "viewFamilyType";
            else if (element is DimensionType) belong = "dimensionType";
            else if (element is TextNoteType) belong = "textNoteType";

            // 装配与参照（继承自 Element）
            else if (element is AssemblyInstance) belong = "assemblyInstance";
            else if (element is ReferencePlane) belong = "referencePlane";




            else if (element is ElementType) belong = "elementType";
            else if (Category.GetCategory(doc, elementId) != null) belong = "category";
            else if (element is ViewSheet) belong = "viewSheet";
            else if (element is View) belong = "view";
            else if (element is Level) belong = "level";
            else if (element is Grid) belong ="grid";
            else if (element is Family) belong = "family";
            else if (element is Material) belong = "material";
            else if (element is FillPatternElement) belong = "fillPatternElement";
            else if (element is LinePatternElement) belong = "linePatternElement";
            else if (element is GraphicsStyle) belong = "graphicsStyle";
            else if (element is ProjectInfo) belong = "projectInfo";
            else if (element is Phase) belong = "phase";
            else if (element is Group) belong = "group";
            else if (element is RevitLinkInstance) belong = "revitLinkInstance";
            else if (element is ImportInstance) belong ="importInstance";
            return new TcpResponse
                {
                    RequestId = request.RequestId,
                    Success = true,
                    Message = $"获取元素 ID {elementIdInt} 的信息成功",
                    Data = new
                    {
                        elementIdBelong = $"该ID背后对应的是{belong}",
                        timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                    }
                };
        }
    }
    public class GetElementLocationAction : IRevitAction
    {
        public TcpResponse Execute(UIApplication app, TcpRequest request)
        {
            Document doc = app.ActiveUIDocument.Document;

            // ── 解析参数 ──
            int elementIdInt = int.Parse(request.Parameters["elementId"]);
            bool includeBoundingBox = request.Parameters.ContainsKey("includeBoundingBox")
                ? bool.Parse(request.Parameters["includeBoundingBox"])
                : true;
            bool includeDirection = request.Parameters.ContainsKey("includeDirection")
                ? bool.Parse(request.Parameters["includeDirection"])
                : true;

            View view = null;
            if (request.Parameters.ContainsKey("viewId") &&
                !string.IsNullOrWhiteSpace(request.Parameters["viewId"]))
            {
                int viewIdInt = int.Parse(request.Parameters["viewId"]);
                view = doc.GetElement(new ElementId(viewIdInt)) as View;
            }

            // ── 获取元素 ──
            ElementId elementId = new ElementId(elementIdInt);
            Element element = doc.GetElement(elementId);
            if (element == null)
            {
                return new TcpResponse
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Message = $"未找到元素 ID {elementIdInt}",
                    Data = null
                };
            }

            // ── 位置信息 ──
            var result = new Dictionary<string, object>();
            result["elementId"] = elementIdInt.ToString();

            Location location = element.Location;
            if (location == null)
            {
                result["locationType"] = "None";
            }
            else if (location is LocationPoint locPoint)
            {
                result["locationType"] = "Point";
                result["point"] = XYZToDict(locPoint.Point);
                result["rotationDegrees"] = Math.Round(locPoint.Rotation * 180.0 / Math.PI, 6);
            }
            else if (location is LocationCurve locCurve)
            {
                Curve curve = locCurve.Curve;
                result["locationType"] = "Curve";
                result["curve"] = new Dictionary<string, object>
            {
                { "start", XYZToDict(curve.GetEndPoint(0)) },
                { "end", XYZToDict(curve.GetEndPoint(1)) },
                { "curveType", curve.GetType().Name },
                { "isBound", curve.IsBound },
                { "length", Math.Round(curve.Length, 6) }
            };

                if (includeDirection)
                {
                    XYZ direction = GetCurveDirection(curve);
                    if (direction != null)
                        result["direction"] = XYZToDict(direction);
                }
            }
            else
            {
                result["locationType"] = location.GetType().Name;
            }

            // ── 包围盒 ──
            if (includeBoundingBox)
            {
                BoundingBoxXYZ bbox = element.get_BoundingBox(view);
                if (bbox != null && bbox.Enabled)
                {
                    XYZ min = bbox.Min;
                    XYZ max = bbox.Max;
                    XYZ center = (min + max) * 0.5;
                    XYZ size = max - min;

                    result["boundingBox"] = new Dictionary<string, object>
                {
                    { "min", XYZToDict(min) },
                    { "max", XYZToDict(max) },
                    { "center", XYZToDict(center) },
                    { "size", XYZToDict(size) }
                };
                }
                else
                {
                    result["boundingBox"] = null;
                }
            }

            return new TcpResponse
            {
                RequestId = request.RequestId,
                Success = true,
                Message = $"获取元素 {elementIdInt} 的位置信息成功",
                Data = new
                {
                    location = result,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                }
            };
        }

        // ═══════════════════════════════════════════
        //  辅助方法
        // ═══════════════════════════════════════════

        /// <summary>将 XYZ 转为字典 {x, y, z}，单位：英尺，保留 6 位小数</summary>
        private static Dictionary<string, double> XYZToDict(XYZ p)
        {
            if (p == null) return null;
            return new Dictionary<string, double>
        {
            { "x", Math.Round(p.X, 6) },
            { "y", Math.Round(p.Y, 6) },
            { "z", Math.Round(p.Z, 6) }
        };
        }

        /// <summary>
        /// 获取曲线在起点处的单位切向量（朝向）。
        /// 优先使用 ComputeDerivatives，失败则用端点差值回退。
        /// </summary>
        private static XYZ GetCurveDirection(Curve curve)
        {
            try
            {
                Transform t = curve.ComputeDerivatives(0.0, true);
                if (t != null && !t.BasisX.IsZeroLength())
                    return t.BasisX.Normalize();
            }
            catch
            {
                // 部分曲线类型不支持，回退
            }

            try
            {
                XYZ start = curve.GetEndPoint(0);
                XYZ end = curve.GetEndPoint(1);
                XYZ dir = end - start;
                if (!dir.IsZeroLength())
                    return dir.Normalize();
            }
            catch { }

            return null;
        }
    }
    public class GetElementGeometryAction : IRevitAction
    {
        public TcpResponse Execute(UIApplication app, TcpRequest request)
        {
            Document doc = app.ActiveUIDocument.Document;

            // ── 参数解析 ──
            int elementIdInt = int.Parse(request.Parameters["elementId"]);
            string detailLevel = GetString(request, "detailLevel", "Fine");
            bool includeNonVisible = GetBool(request, "includeNonVisible", false);
            bool useSymbolGeometry = GetBool(request, "useSymbolGeometry", false);
            bool includeTriangulation = GetBool(request, "includeTriangulation", false);
            bool includeGeneratingIds = GetBool(request, "includeGeneratingIds", false);
            int maxFaces = GetInt(request, "maxFaces", 2000);

            // ── 获取元素 ──
            Element element = doc.GetElement(new ElementId(elementIdInt));
            if (element == null)
            {
                return new TcpResponse
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Message = $"未找到元素 ID {elementIdInt}",
                    Data = null
                };
            }

            // ── Options ──
            var options = new Options
            {
                DetailLevel = ParseDetailLevel(detailLevel),
                IncludeNonVisibleObjects = includeNonVisible,
                ComputeReferences = false
            };

            // ── 上下文 ──
            var ctx = new GeometryContext
            {
                Document = doc,
                HostElement = element,
                MaxFaces = maxFaces,
                IncludeTriangulation = includeTriangulation,
                IncludeGeneratingIds = includeGeneratingIds
            };

            // ── 获取几何 ──
            GeometryElement geomElem = null;
            try
            {
                geomElem = element.get_Geometry(options);
            }
            catch (Exception ex)
            {
                return new TcpResponse
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Message = $"元素 {elementIdInt} 几何获取失败: {ex.Message}",
                    Data = null
                };
            }

            if (geomElem == null)
            {
                return SuccessResponse(request, elementIdInt, ctx);
            }

            // ── 遍历 ──
            TraverseGeometry(geomElem, Transform.Identity, ctx, useSymbolGeometry);

            return SuccessResponse(request, elementIdInt, ctx);
        }

        // ═══════════════════════════════════════════
        //  递归遍历
        // ═══════════════════════════════════════════

        private static void TraverseGeometry(
            GeometryElement geomElem,
            Transform accumulatedTransform,
            GeometryContext ctx,
            bool useSymbolGeometry)
        {
            if (geomElem == null || ctx.Truncated) return;

            foreach (GeometryObject geomObj in geomElem)
            {
                if (ctx.Truncated) return;

                // ── GeometryInstance ──
                if (geomObj is GeometryInstance instance)
                {
                    try
                    {
                        ctx.Instances.Add(new Dictionary<string, object>
                    {
                        { "instanceId", instance.Id.ToString() },
                        { "symbolId", instance.Symbol?.Id.IntegerValue.ToString() ?? "null" },
                        { "transform", TransformToDict(
                            accumulatedTransform.Multiply(instance.Transform)) }
                    });
                    }
                    catch { }

                    GeometryElement symbolGeom = null;
                    try { symbolGeom = instance.GetSymbolGeometry(); }
                    catch (Exception ex)
                    {
                        ctx.Warnings.Add($"GetSymbolGeometry 失败(instanceId={instance.Id}): {ex.Message}");
                        continue;
                    }
                    if (symbolGeom == null) continue;

                    Transform nextTransform = useSymbolGeometry
                        ? Transform.Identity
                        : accumulatedTransform.Multiply(instance.Transform);

                    TraverseGeometry(symbolGeom, nextTransform, ctx, useSymbolGeometry);
                    continue;
                }

                // ── Solid ──
                if (geomObj is Solid solid)
                {
                    try
                    {
                        if (solid.Faces.Size == 0 && solid.Edges.Size == 0) continue;

                        Solid transformed = accumulatedTransform.IsIdentity
                            ? solid
                            : SolidUtils.CreateTransformed(solid, accumulatedTransform);

                        ProcessSolid(transformed, ctx);
                    }
                    catch (Exception ex)
                    {
                        // 单个 Solid 失败不影响整体
                        ctx.Warnings.Add($"Solid 处理失败: {ex.Message}");
                    }
                    continue;
                }

                // ── Curve ──
                if (geomObj is Curve curve)
                {
                    try
                    {
                        Curve transformed = accumulatedTransform.IsIdentity
                            ? curve
                            : curve.CreateTransformed(accumulatedTransform);

                        ctx.Curves.Add(CurveToDict(transformed));
                    }
                    catch (Exception ex)
                    {
                        ctx.Warnings.Add($"Curve 处理失败: {ex.Message}");
                    }
                    continue;
                }

                // ── Mesh ──
                if (geomObj is Mesh mesh)
                {
                    try
                    {
                        ctx.Meshes.Add(MeshToDict(mesh, accumulatedTransform));
                    }
                    catch (Exception ex)
                    {
                        ctx.Warnings.Add($"Mesh 处理失败: {ex.Message}");
                    }
                    continue;
                }

                // ── PolyLine ──
                if (geomObj is PolyLine polyLine)
                {
                    try
                    {
                        var pts = new List<Dictionary<string, double>>();
                        foreach (XYZ p in polyLine.GetCoordinates())
                            pts.Add(XYZToDict(accumulatedTransform.OfPoint(p)));

                        ctx.Curves.Add(new Dictionary<string, object>
                    {
                        { "type", "PolyLine" },
                        { "points", pts }
                    });
                    }
                    catch (Exception ex)
                    {
                        ctx.Warnings.Add($"PolyLine 处理失败: {ex.Message}");
                    }
                    continue;
                }
            }
        }

        // ═══════════════════════════════════════════
        //  Solid 处理（关键修正：Volume / SurfaceArea 容错）
        // ═══════════════════════════════════════════

        private static void ProcessSolid(Solid solid, GeometryContext ctx)
        {
            var faceList = new List<Dictionary<string, object>>();
            int faceCount = 0;

            // ── 面遍历 ──
            try
            {
                foreach (Face face in solid.Faces)
                {
                    if (ctx.TotalFaceCount + faceCount >= ctx.MaxFaces)
                    {
                        ctx.Truncated = true;
                        break;
                    }

                    var faceDict = new Dictionary<string, object>
                {
                    { "area", Math.Round(face.Area, 6) },
                    { "faceType", face.GetType().Name }
                };

                    // 法向量
                    try
                    {
                        BoundingBoxUV bbox = face.GetBoundingBox();
                        if (bbox != null)
                        {
                            double u = (bbox.Min.U + bbox.Max.U) * 0.5;
                            double v = (bbox.Min.V + bbox.Max.V) * 0.5;
                            XYZ normal = face.ComputeNormal(new UV(u, v));
                            if (normal != null && !normal.IsZeroLength())
                                faceDict["normal"] = XYZToDict(normal.Normalize());
                        }
                    }
                    catch { }

                    // 材质
                    try
                    {
                        ElementId matId = face.MaterialElementId;
                        if (matId != null && matId != ElementId.InvalidElementId)
                            faceDict["materialId"] = matId.IntegerValue.ToString();
                    }
                    catch { }

                    // 追溯洞口来源
                    if (ctx.IncludeGeneratingIds && ctx.HostElement != null)
                    {
                        try
                        {
                            ICollection<ElementId> genIds =
                                ctx.HostElement.GetGeneratingElementIds(face);

                            if (genIds != null && genIds.Count > 0)
                            {
                                var filtered = genIds
                                    .Where(id => id != ctx.HostElement.Id)
                                    .Select(id => id.IntegerValue.ToString())
                                    .ToList();

                                if (filtered.Count > 0)
                                    faceDict["generatingElementIds"] = filtered;
                            }
                        }
                        catch { }
                    }

                    // 三角化
                    if (ctx.IncludeTriangulation)
                    {
                        try
                        {
                            Mesh triMesh = face.Triangulate();
                            if (triMesh != null && triMesh.NumTriangles > 0)
                            {
                                var tris = new List<List<Dictionary<string, double>>>(
                                    triMesh.NumTriangles);
                                for (int i = 0; i < triMesh.NumTriangles; i++)
                                {
                                    MeshTriangle tri = triMesh.get_Triangle(i);
                                    tris.Add(new List<Dictionary<string, double>>
                                {
                                    XYZToDict(tri.get_Vertex(0)),
                                    XYZToDict(tri.get_Vertex(1)),
                                    XYZToDict(tri.get_Vertex(2))
                                });
                                }
                                faceDict["triangles"] = tris;
                            }
                        }
                        catch { }
                    }

                    faceList.Add(faceDict);
                    faceCount++;
                }
            }
            catch (Exception ex)
            {
                ctx.Warnings.Add($"Solid.Faces 遍历失败: {ex.Message}");
            }

            // ── Edge 数量 ──
            int edgeCount = 0;
            try { edgeCount = solid.Edges.Size; }
            catch (Exception ex)
            {
                ctx.Warnings.Add($"Solid.Edges 访问失败: {ex.Message}");
            }

            // ── Volume（关键修正：非封闭 Solid 会抛异常）──
            double? volume = null;
            try
            {
                volume = Math.Round(solid.Volume, 6);
                ctx.TotalVolume += volume.Value;
            }
            catch (Exception ex)
            {
                ctx.Warnings.Add($"Solid.Volume 计算失败（可能非封闭）: {ex.Message}");
            }

            // ── SurfaceArea ──
            double? surfaceArea = null;
            try
            {
                surfaceArea = Math.Round(solid.SurfaceArea, 6);
                ctx.TotalSurfaceArea += surfaceArea.Value;
            }
            catch (Exception ex)
            {
                ctx.Warnings.Add($"Solid.SurfaceArea 计算失败: {ex.Message}");
            }

            ctx.TotalFaceCount += faceCount;
            ctx.TotalEdgeCount += edgeCount;

            var solidDict = new Dictionary<string, object>
        {
            { "faceCount", faceCount },
            { "edgeCount", edgeCount },
            { "faces", faceList }
        };

            // 只在成功时写入 volume / surfaceArea，失败时字段缺失（AI 可据此判断）
            if (volume.HasValue) solidDict["volume"] = volume.Value;
            if (surfaceArea.HasValue) solidDict["surfaceArea"] = surfaceArea.Value;

            // 若两者都拿不到，标记该 Solid 可能是非封闭体
            if (!volume.HasValue && !surfaceArea.HasValue)
                solidDict["invalidSolid"] = true;

            ctx.Solids.Add(solidDict);
        }

        // ═══════════════════════════════════════════
        //  辅助方法
        // ═══════════════════════════════════════════

        private static string GetString(TcpRequest r, string k, string def)
            => r.Parameters.ContainsKey(k) && !string.IsNullOrWhiteSpace(r.Parameters[k])
                ? r.Parameters[k] : def;

        private static bool GetBool(TcpRequest r, string k, bool def)
            => r.Parameters.ContainsKey(k) ? bool.Parse(r.Parameters[k]) : def;

        private static int GetInt(TcpRequest r, string k, int def)
            => r.Parameters.ContainsKey(k) ? int.Parse(r.Parameters[k]) : def;

        private static ViewDetailLevel ParseDetailLevel(string s)
        {
            switch ((s ?? "fine").ToLowerInvariant())
            {
                case "coarse": return ViewDetailLevel.Coarse;
                case "medium": return ViewDetailLevel.Medium;
                default: return ViewDetailLevel.Fine;
            }
        }

        private static Dictionary<string, double> XYZToDict(XYZ p)
        {
            if (p == null) return null;
            return new Dictionary<string, double>
        {
            { "x", Math.Round(p.X, 6) },
            { "y", Math.Round(p.Y, 6) },
            { "z", Math.Round(p.Z, 6) }
        };
        }

        private static Dictionary<string, double> TransformToDict(Transform t)
        {
            return new Dictionary<string, double>
        {
            { "originX", Math.Round(t.Origin.X, 6) },
            { "originY", Math.Round(t.Origin.Y, 6) },
            { "originZ", Math.Round(t.Origin.Z, 6) },
            { "basisXx", Math.Round(t.BasisX.X, 6) },
            { "basisXy", Math.Round(t.BasisX.Y, 6) },
            { "basisXz", Math.Round(t.BasisX.Z, 6) },
            { "basisYx", Math.Round(t.BasisY.X, 6) },
            { "basisYy", Math.Round(t.BasisY.Y, 6) },
            { "basisYz", Math.Round(t.BasisY.Z, 6) },
            { "basisZx", Math.Round(t.BasisZ.X, 6) },
            { "basisZy", Math.Round(t.BasisZ.Y, 6) },
            { "basisZz", Math.Round(t.BasisZ.Z, 6) }
        };
        }

        private static Dictionary<string, object> CurveToDict(Curve curve)
        {
            return new Dictionary<string, object>
        {
            { "type", curve.GetType().Name },
            { "start", XYZToDict(curve.GetEndPoint(0)) },
            { "end", XYZToDict(curve.GetEndPoint(1)) },
            { "length", Math.Round(curve.Length, 6) },
            { "isBound", curve.IsBound }
        };
        }

        private static Dictionary<string, object> MeshToDict(Mesh mesh, Transform t)
        {
            var tris = new List<List<Dictionary<string, double>>>(mesh.NumTriangles);
            for (int i = 0; i < mesh.NumTriangles; i++)
            {
                MeshTriangle tri = mesh.get_Triangle(i);
                tris.Add(new List<Dictionary<string, double>>
            {
                XYZToDict(t.OfPoint(tri.get_Vertex(0))),
                XYZToDict(t.OfPoint(tri.get_Vertex(1))),
                XYZToDict(t.OfPoint(tri.get_Vertex(2)))
            });
            }
            return new Dictionary<string, object>
        {
            { "type", "Mesh" },
            { "triangleCount", mesh.NumTriangles },
            { "triangles", tris }
        };
        }

        private static TcpResponse SuccessResponse(
            TcpRequest req, int elementIdInt, GeometryContext ctx)
        {
            return new TcpResponse
            {
                RequestId = req.RequestId,
                Success = true,
                Message = $"获取元素 {elementIdInt} 的几何信息成功",
                Data = new
                {
                    geometry = new
                    {
                        elementId = elementIdInt.ToString(),
                        solids = ctx.Solids,
                        curves = ctx.Curves,
                        meshes = ctx.Meshes,
                        instances = ctx.Instances,
                        summary = new
                        {
                            solidCount = ctx.Solids.Count,
                            faceCount = ctx.TotalFaceCount,
                            edgeCount = ctx.TotalEdgeCount,
                            curveCount = ctx.Curves.Count,
                            meshCount = ctx.Meshes.Count,
                            instanceCount = ctx.Instances.Count,
                            totalVolume = Math.Round(ctx.TotalVolume, 6),
                            totalSurfaceArea = Math.Round(ctx.TotalSurfaceArea, 6),
                            truncated = ctx.Truncated,
                            warningCount = ctx.Warnings.Count
                        },
                        warnings = ctx.Warnings.Count > 0 ? ctx.Warnings : null
                    },
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                }
            };
        }
    }

    // ═══════════════════════════════════════════
    //  上下文
    // ═══════════════════════════════════════════

    internal class GeometryContext
    {
        public Document Document { get; set; }
        public Element HostElement { get; set; }
        public int MaxFaces { get; set; } = 2000;
        public bool IncludeTriangulation { get; set; }
        public bool IncludeGeneratingIds { get; set; }

        public List<Dictionary<string, object>> Solids { get; } = new List<Dictionary<string, object>>();
        public List<Dictionary<string, object>> Curves { get; } = new List<Dictionary<string, object>>();
        public List<Dictionary<string, object>> Meshes { get; } = new List<Dictionary<string, object>>();
        public List<Dictionary<string, object>> Instances { get; } = new List<Dictionary<string, object>>();
        public List<string> Warnings { get; } = new List<string>();

        public int TotalFaceCount { get; set; }
        public int TotalEdgeCount { get; set; }
        public double TotalVolume { get; set; }
        public double TotalSurfaceArea { get; set; }
        public bool Truncated { get; set; }
    }
    public class GetHostOpeningsAndCutsAction : IRevitAction
    {
        public TcpResponse Execute(UIApplication app, TcpRequest request)
        {
            Document doc = app.ActiveUIDocument.Document;

            // ── 参数解析 ──
            int hostId = int.Parse(request.Parameters["hostElementId"]);
            bool includeRectOpenings = GetBool(request, "includeRectOpenings", true);
            bool includeDoorsWindows = GetBool(request, "includeDoorsWindows", true);
            bool includeVoidCuts = GetBool(request, "includeVoidCuts", true);
            bool includeAreaEstimate = GetBool(request, "includeAreaEstimate", true);
            bool includeCrossHostInserts = GetBool(request, "includeCrossHostInserts", false);

            // ── 获取宿主元素 ──
            Element host = doc.GetElement(new ElementId(hostId));
            if (host == null)
            {
                return new TcpResponse
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Message = $"未找到宿主元素 ID {hostId}",
                    Data = null
                };
            }

            HostObject hostObj = host as HostObject;
            if (hostObj == null)
            {
                return new TcpResponse
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Message = $"元素 {hostId} 不是宿主元素（HostObject），无法查询开洞与剪切",
                    Data = null
                };
            }

            // ── 上下文 ──
            var ctx = new OpeningContext
            {
                Document = doc,
                HostElement = host,
                IncludeAreaEstimate = includeAreaEstimate
            };

            // ── 1. FindInserts（门、窗、矩形洞口、其他插入物）──
            IList<ElementId> insertIds = null;
            try
            {
                insertIds = hostObj.FindInserts(
                    addRectOpenings: true,
                    includeShadows: false,
                    includeEmbeddedWalls: false,
                    includeSharedEmbeddedInserts: false
                );
            }
            catch (Exception ex)
            {
                ctx.Warnings.Add($"FindInserts 失败: {ex.Message}");
            }

            if (insertIds != null)
            {
                foreach (ElementId insertId in insertIds)
                {
                    Element insertElem = null;
                    try { insertElem = doc.GetElement(insertId); }
                    catch { continue; }
                    if (insertElem == null) continue;

                    // ── 关键修复：核对真实宿主，过滤跨宿主串扰 ──
                    ElementId realHostId = GetReportedHostId(insertElem);
                    bool isCrossHost = realHostId != null && realHostId != host.Id;

                    if (isCrossHost && !includeCrossHostInserts)
                    {
                        ctx.CrossHostFilteredCount++;
                        continue;
                    }

                    Dictionary<string, object> info = null;
                    try { info = ClassifyInsert(insertElem, ctx); }
                    catch (Exception ex)
                    {
                        ctx.Warnings.Add($"插入物 {insertId.IntegerValue} 分类失败: {ex.Message}");
                    }
                    if (info == null) continue;

                    // 标记跨宿主项
                    if (isCrossHost)
                    {
                        info["isCrossHost"] = true;
                        info["reportedHostId"] = realHostId.IntegerValue.ToString();
                    }

                    string type = info.ContainsKey("type") ? info["type"] as string : "Unknown";

                    // 分类过滤
                    bool keep;
                    switch (type)
                    {
                        case "RectOpening":
                        case "NonRectOpening":
                        case "OtherInsert":
                            keep = includeRectOpenings;
                            break;
                        case "Door":
                        case "Window":
                        case "OtherFamilyInsert":
                            keep = includeDoorsWindows;
                            break;
                        default:
                            keep = true;
                            break;
                    }
                    if (!keep) continue;

                    ctx.Openings.Add(info);
                }
            }

            // ── 2. Void Cut（体量剪切）──
            if (includeVoidCuts)
            {
                try
                {
                    ICollection<ElementId> voidIds =
                        InstanceVoidCutUtils.GetCuttingVoidInstances(host);

                    if (voidIds != null)
                    {
                        foreach (ElementId vid in voidIds)
                        {
                            Element voidElem = null;
                            try { voidElem = doc.GetElement(vid); }
                            catch { continue; }
                            if (voidElem == null) continue;

                            Dictionary<string, object> voidInfo = null;
                            try { voidInfo = BuildVoidInfo(voidElem, ctx); }
                            catch (Exception ex)
                            {
                                ctx.Warnings.Add($"VoidCut {vid.IntegerValue} 处理失败: {ex.Message}");
                            }
                            if (voidInfo != null)
                                ctx.Openings.Add(voidInfo);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ctx.Warnings.Add($"InstanceVoidCutUtils.GetCuttingVoidInstances 失败: {ex.Message}");
                }
            }

            return SuccessResponse(request, hostId, host, ctx);
        }

        // ═══════════════════════════════════════════
        //  关键修复：获取插入物报告的真实宿主
        // ═══════════════════════════════════════════

        /// <summary>
        /// 返回插入物真实归属的宿主 ID：
        ///  - Opening → Opening.Host
        ///  - FamilyInstance → FamilyInstance.Host
        ///  - 其他 → null（无法判断，视为同宿主）
        /// </summary>
        private static ElementId GetReportedHostId(Element insertElem)
        {
            try
            {
                if (insertElem is Opening op)
                    return op.Host?.Id;

                if (insertElem is FamilyInstance fi)
                    return fi.Host?.Id;
            }
            catch { }
            return null;
        }

        // ═══════════════════════════════════════════
        //  插入物分类
        // ═══════════════════════════════════════════

        private static Dictionary<string, object> ClassifyInsert(
            Element elem, OpeningContext ctx)
        {
            if (elem is Opening opening)
                return BuildOpeningInfo(opening, ctx);

            if (elem is FamilyInstance fi)
                return BuildFamilyInstanceInfo(fi, ctx);

            var info = new Dictionary<string, object>
        {
            { "id", elem.Id.IntegerValue.ToString() },
            { "name", elem.Name ?? "无名称" },
            { "type", "OtherInsert" },
            { "categoryName", elem.Category?.Name ?? "无类别" }
        };

            if (ctx.IncludeAreaEstimate)
            {
                double? area = EstimateOpeningAreaFromGeometry(elem);
                if (area.HasValue)
                {
                    info["area"] = area.Value;
                    info["areaSource"] = "geometry";
                }
            }

            return info;
        }

        // ═══════════════════════════════════════════
        //  Opening 处理
        // ═══════════════════════════════════════════

        private static Dictionary<string, object> BuildOpeningInfo(
            Opening opening, OpeningContext ctx)
        {
            var info = new Dictionary<string, object>
        {
            { "id", opening.Id.IntegerValue.ToString() },
            { "name", opening.Name ?? "无名称" },
            { "categoryName", opening.Category?.Name ?? "洞口" },
            { "hostId", opening.Host?.Id.IntegerValue.ToString() ?? "null" },
            { "isRectBoundary", opening.IsRectBoundary }
        };

            if (opening.IsRectBoundary)
            {
                info["type"] = "RectOpening";

                IList<XYZ> rect = null;
                try { rect = opening.BoundaryRect; }
                catch (Exception ex)
                {
                    ctx.Warnings.Add($"Opening {opening.Id.IntegerValue} BoundaryRect 获取失败: {ex.Message}");
                }

                if (rect != null && rect.Count == 2)
                {
                    XYZ p1 = rect[0];
                    XYZ p2 = rect[1];

                    XYZ min = new XYZ(
                        Math.Min(p1.X, p2.X),
                        Math.Min(p1.Y, p2.Y),
                        Math.Min(p1.Z, p2.Z));
                    XYZ max = new XYZ(
                        Math.Max(p1.X, p2.X),
                        Math.Max(p1.Y, p2.Y),
                        Math.Max(p1.Z, p2.Z));

                    info["boundaryRect"] = new Dictionary<string, object>
                {
                    { "min", XYZToDict(min) },
                    { "max", XYZToDict(max) },
                    { "size", XYZToDict(max - min) }
                };

                    if (ctx.IncludeAreaEstimate)
                    {
                        info["area"] = EstimateAreaFromBBox(min, max);
                        info["areaSource"] = "boundingBox";
                    }
                }
            }
            else
            {
                info["type"] = "NonRectOpening";

                try
                {
                    CurveArray curves = opening.BoundaryCurves;
                    if (curves != null && curves.Size > 0)
                    {
                        var curveList = new List<Dictionary<string, object>>(curves.Size);
                        foreach (Curve c in curves)
                            curveList.Add(CurveToDict(c));
                        info["boundaryCurves"] = curveList;
                    }
                }
                catch (Exception ex)
                {
                    ctx.Warnings.Add($"Opening {opening.Id.IntegerValue} BoundaryCurves 获取失败: {ex.Message}");
                }

                if (ctx.IncludeAreaEstimate)
                {
                    double? area = EstimateOpeningAreaFromGeometry(opening);
                    if (area.HasValue)
                    {
                        info["area"] = area.Value;
                        info["areaSource"] = "geometry";
                    }
                }
            }

            return info;
        }

        // ═══════════════════════════════════════════
        //  FamilyInstance（门/窗/其他族实例）
        // ═══════════════════════════════════════════

        private static Dictionary<string, object> BuildFamilyInstanceInfo(
            FamilyInstance fi, OpeningContext ctx)
        {
            var info = new Dictionary<string, object>
        {
            { "id", fi.Id.IntegerValue.ToString() },
            { "name", fi.Name ?? "无名称" },
            { "categoryName", fi.Category?.Name ?? "无类别" },
            { "hostId", fi.Host?.Id.IntegerValue.ToString() ?? "null" },
            { "symbolId", fi.GetTypeId()?.IntegerValue.ToString() ?? "null" },
            { "symbolName", fi.Symbol?.Name ?? "无" }
        };

            // 按 BuiltInCategory 归类
            BuiltInCategory bic = BuiltInCategory.INVALID;
            try
            {
                if (fi.Category != null)
                    bic = (BuiltInCategory)fi.Category.Id.IntegerValue;
            }
            catch { }

            if (bic == BuiltInCategory.OST_Doors)
                info["type"] = "Door";
            else if (bic == BuiltInCategory.OST_Windows)
                info["type"] = "Window";
            else
                info["type"] = "OtherFamilyInsert";

            // 补充族名，让 AI 能识别天窗/老虎窗等特殊子类
            try
            {
                string familyName = fi.Symbol?.Family?.Name;
                if (!string.IsNullOrEmpty(familyName))
                    info["symbolFamilyName"] = familyName;
            }
            catch { }

            // 面积估算
            if (ctx.IncludeAreaEstimate)
            {
                double? w = GetParamAsDouble(fi, "宽度")
                         ?? GetParamAsDouble(fi, "Width")
                         ?? GetParamAsDouble(fi.Symbol, "宽度")
                         ?? GetParamAsDouble(fi.Symbol, "Width");

                double? h = GetParamAsDouble(fi, "高度")
                         ?? GetParamAsDouble(fi, "Height")
                         ?? GetParamAsDouble(fi.Symbol, "高度")
                         ?? GetParamAsDouble(fi.Symbol, "Height");

                if (w.HasValue && h.HasValue && w.Value > 0 && h.Value > 0)
                {
                    info["width"] = Math.Round(w.Value, 6);
                    info["height"] = Math.Round(h.Value, 6);
                    info["area"] = Math.Round(w.Value * h.Value, 6);
                    info["areaSource"] = "symbolParameters";
                }
                else
                {
                    double? area = EstimateOpeningAreaFromGeometry(fi);
                    if (area.HasValue)
                    {
                        info["area"] = area.Value;
                        info["areaSource"] = "geometry";
                    }
                }
            }

            return info;
        }

        // ═══════════════════════════════════════════
        //  Void Cut 处理
        // ═══════════════════════════════════════════

        private static Dictionary<string, object> BuildVoidInfo(
            Element voidElem, OpeningContext ctx)
        {
            var info = new Dictionary<string, object>
        {
            { "id", voidElem.Id.IntegerValue.ToString() },
            { "name", voidElem.Name ?? "无名称" },
            { "type", "VoidCut" },
            { "categoryName", voidElem.Category?.Name ?? "无类别" },
            { "symbolId", voidElem.GetTypeId()?.IntegerValue.ToString() ?? "null" }
        };

            if (voidElem is FamilyInstance vfi && vfi.Host != null)
                info["hostId"] = vfi.Host.Id.IntegerValue.ToString();

            if (ctx.IncludeAreaEstimate)
            {
                double? area = EstimateOpeningAreaFromGeometry(voidElem);
                if (area.HasValue)
                {
                    info["area"] = area.Value;
                    info["areaSource"] = "geometry";
                }
            }

            return info;
        }

        // ═══════════════════════════════════════════
        //  面积估算
        // ═══════════════════════════════════════════

        private static double EstimateAreaFromBBox(XYZ min, XYZ max)
        {
            double dx = max.X - min.X;
            double dy = max.Y - min.Y;
            double dz = max.Z - min.Z;

            var dims = new[] { dx, dy, dz };
            Array.Sort(dims);

            return Math.Round(dims[1] * dims[2], 6);
        }

        private static double? EstimateOpeningAreaFromGeometry(Element elem)
        {
            if (elem == null) return null;
            try
            {
                var options = new Options
                {
                    DetailLevel = ViewDetailLevel.Coarse,
                    ComputeReferences = false,
                    IncludeNonVisibleObjects = true
                };

                GeometryElement geomElem = elem.get_Geometry(options);
                if (geomElem == null) return null;

                double maxArea = 0.0;
                FindMaxPlanarFaceArea(geomElem, ref maxArea);

                return maxArea > 0 ? Math.Round(maxArea, 6) : (double?)null;
            }
            catch
            {
                return null;
            }
        }

        private static void FindMaxPlanarFaceArea(
            GeometryElement geomElem, ref double maxArea)
        {
            if (geomElem == null) return;

            foreach (GeometryObject obj in geomElem)
            {
                if (obj is Solid solid)
                {
                    if (solid.Faces.Size == 0) continue;
                    try
                    {
                        foreach (Face face in solid.Faces)
                        {
                            if (face is PlanarFace && face.Area > maxArea)
                                maxArea = face.Area;
                        }
                    }
                    catch { }
                }
                else if (obj is GeometryInstance instance)
                {
                    GeometryElement subGeom = null;
                    try { subGeom = instance.GetSymbolGeometry(); }
                    catch { continue; }

                    FindMaxPlanarFaceArea(subGeom, ref maxArea);
                }
            }
        }

        // ═══════════════════════════════════════════
        //  辅助方法
        // ═══════════════════════════════════════════

        private static double? GetParamAsDouble(Element elem, string paramName)
        {
            if (elem == null || string.IsNullOrEmpty(paramName)) return null;
            try
            {
                Parameter p = elem.LookupParameter(paramName);
                if (p == null || !p.HasValue) return null;
                if (p.StorageType != StorageType.Double) return null;
                return p.AsDouble();
            }
            catch { return null; }
        }

        private static bool GetBool(TcpRequest r, string k, bool def)
            => r.Parameters.ContainsKey(k) ? bool.Parse(r.Parameters[k]) : def;

        private static Dictionary<string, double> XYZToDict(XYZ p)
        {
            if (p == null) return null;
            return new Dictionary<string, double>
        {
            { "x", Math.Round(p.X, 6) },
            { "y", Math.Round(p.Y, 6) },
            { "z", Math.Round(p.Z, 6) }
        };
        }

        private static Dictionary<string, object> CurveToDict(Curve c)
        {
            if (c == null) return null;
            return new Dictionary<string, object>
        {
            { "type", c.GetType().Name },
            { "start", XYZToDict(c.GetEndPoint(0)) },
            { "end", XYZToDict(c.GetEndPoint(1)) },
            { "length", Math.Round(c.Length, 6) },
            { "isBound", c.IsBound }
        };
        }

        private static TcpResponse SuccessResponse(
            TcpRequest req, int hostId, Element host, OpeningContext ctx)
        {
            int rectCount = 0;
            int nonRectCount = 0;
            int doorCount = 0;
            int windowCount = 0;
            int voidCutCount = 0;
            int otherCount = 0;
            int crossHostCount = 0;
            double totalArea = 0.0;

            foreach (var o in ctx.Openings)
            {
                string t = o.ContainsKey("type") ? o["type"] as string : "Unknown";
                switch (t)
                {
                    case "RectOpening": rectCount++; break;
                    case "NonRectOpening": nonRectCount++; break;
                    case "Door": doorCount++; break;
                    case "Window": windowCount++; break;
                    case "VoidCut": voidCutCount++; break;
                    default: otherCount++; break;
                }

                if (o.ContainsKey("isCrossHost") && o["isCrossHost"] is bool b && b)
                    crossHostCount++;

                if (o.ContainsKey("area"))
                {
                    try { totalArea += Convert.ToDouble(o["area"]); }
                    catch { }
                }
            }

            return new TcpResponse
            {
                RequestId = req.RequestId,
                Success = true,
                Message = $"获取宿主元素 {hostId} 的开洞与剪切信息成功",
                Data = new
                {
                    openings = new
                    {
                        hostId = hostId.ToString(),
                        hostName = host.Name ?? "无名称",
                        hostCategory = host.Category?.Name ?? "无类别",
                        openingList = ctx.Openings,
                        summary = new
                        {
                            openingCount = ctx.Openings.Count,
                            rectOpeningCount = rectCount,
                            nonRectOpeningCount = nonRectCount,
                            doorCount = doorCount,
                            windowCount = windowCount,
                            voidCutCount = voidCutCount,
                            otherInsertCount = otherCount,
                            crossHostCount = crossHostCount,
                            crossHostFilteredCount = ctx.CrossHostFilteredCount,
                            totalArea = Math.Round(totalArea, 6),
                            warningCount = ctx.Warnings.Count
                        },
                        warnings = ctx.Warnings.Count > 0 ? ctx.Warnings : null
                    },
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                }
            };
        }
    }

    // ═══════════════════════════════════════════
    //  上下文
    // ═══════════════════════════════════════════

    internal class OpeningContext
    {
        public Document Document { get; set; }
        public Element HostElement { get; set; }
        public bool IncludeAreaEstimate { get; set; }

        public List<Dictionary<string, object>> Openings { get; } =
            new List<Dictionary<string, object>>();

        public List<string> Warnings { get; } = new List<string>();

        /// <summary>被过滤掉的跨宿主插入物数量</summary>
        public int CrossHostFilteredCount { get; set; }
    }
    public class GetSpatialRelationAction : IRevitAction
    {
        public TcpResponse Execute(UIApplication app, TcpRequest request)
        {
            Document doc = app.ActiveUIDocument.Document;

            // ── 参数解析 ──
            string mode = request.Parameters.ContainsKey("mode")
                ? request.Parameters["mode"] : "";

            if (mode != "elementToRoom" && mode != "pointToRoom")
            {
                return new TcpResponse
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Message = $"无效的 mode: \"{mode}\"，应为 elementToRoom 或 pointToRoom",
                    Data = null
                };
            }

            // ── 一次性收集所有房间和空间 ──
            // 关键修正：用 SpatialElement 收集，后处理筛出 Room / Space
            var ctx = new SpatialContext { Document = doc };
            try
            {
                List<SpatialElement> allSpatialElements = new FilteredElementCollector(doc)
                    .WhereElementIsNotElementType()
                    .OfClass(typeof(SpatialElement))
                    .Cast<SpatialElement>()
                    .ToList();

                foreach (SpatialElement se in allSpatialElements)
                {
                    if (se is Room room)
                    {
                        // 过滤未放置的房间（面积 > 0 表示已放置）
                        try
                        {
                            if (room.Area > 0)
                                ctx.Rooms.Add(room);
                        }
                        catch { }
                    }
                    else if (se is Space space)
                    {
                        ctx.Spaces.Add(space);
                    }
                }
            }
            catch (Exception ex)
            {
                return new TcpResponse
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Message = $"收集房间/空间失败: {ex.Message}",
                    Data = null
                };
            }

            if (mode == "elementToRoom")
                return HandleElementToRoom(request, ctx);
            else
                return HandlePointToRoom(request, ctx);
        }

        // ═══════════════════════════════════════════
        //  Mode 1: elementToRoom
        // ═══════════════════════════════════════════

        private TcpResponse HandleElementToRoom(TcpRequest request, SpatialContext ctx)
        {
            string idsStr = request.Parameters.ContainsKey("elementIds")
                ? request.Parameters["elementIds"] : null;

            if (string.IsNullOrWhiteSpace(idsStr))
            {
                return new TcpResponse
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Message = "mode=elementToRoom 时 elementIds 参数必填（逗号分隔的 ID 字符串，如 \"123,456\"）",
                    Data = null
                };
            }

            // ── 解析逗号分隔的 ID ──
            List<string> elementIds = new List<string>();
            foreach (string s in idsStr.Split(','))
            {
                string t = s.Trim();
                if (!string.IsNullOrEmpty(t))
                    elementIds.Add(t);
            }

            if (elementIds.Count == 0)
            {
                return new TcpResponse
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Message = "elementIds 解析后为空",
                    Data = null
                };
            }

            var results = new List<Dictionary<string, object>>();
            int foundCount = 0;
            int notFoundCount = 0;
            int noRoomCount = 0;

            foreach (string idStr in elementIds)
            {
                var item = new Dictionary<string, object> { { "elementId", idStr } };

                Element elem = null;
                if (int.TryParse(idStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int idInt))
                {
                    try { elem = ctx.Document.GetElement(new ElementId(idInt)); }
                    catch { }
                }

                if (elem == null)
                {
                    item["source"] = "elementNotFound";
                    item["roomId"] = null;
                    notFoundCount++;
                    results.Add(item);
                    continue;
                }

                try
                {
                    ResolveElementRoom(elem, ctx, item);
                }
                catch (Exception ex)
                {
                    item["source"] = "error";
                    item["error"] = ex.Message;
                    item["roomId"] = null;
                }

                if (item.ContainsKey("roomId") && item["roomId"] != null)
                    foundCount++;
                else
                    noRoomCount++;

                results.Add(item);
            }

            return new TcpResponse
            {
                RequestId = request.RequestId,
                Success = true,
                Message = $"查询 {elementIds.Count} 个元素的房间归属完成",
                Data = new
                {
                    mode = "elementToRoom",
                    results = results,
                    summary = new
                    {
                        total = elementIds.Count,
                        found = foundCount,
                        notFound = notFoundCount,
                        noRoom = noRoomCount
                    },
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                }
            };
        }

        // ═══════════════════════════════════════════
        //  Mode 2: pointToRoom
        // ═══════════════════════════════════════════

        private TcpResponse HandlePointToRoom(TcpRequest request, SpatialContext ctx)
        {
            string pointsStr = request.Parameters.ContainsKey("points")
                ? request.Parameters["points"] : null;

            if (string.IsNullOrWhiteSpace(pointsStr))
            {
                return new TcpResponse
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Message = "mode=pointToRoom 时 points 参数必填" +
                              "（分号分隔的点，每点逗号分隔 xyz，如 \"1,2,3;4,5,6\"）",
                    Data = null
                };
            }

            // ── 解析分号分隔、逗号分隔的点 ──
            var parsedPoints = new List<XYZ>();
            var parseErrors = new List<string>();

            int idx = 0;
            foreach (string pointStr in pointsStr.Split(';'))
            {
                idx++;
                string t = pointStr.Trim();
                if (string.IsNullOrEmpty(t)) continue;

                string[] parts = t.Split(',');
                if (parts.Length != 3)
                {
                    parseErrors.Add($"第 {idx} 个点 \"{t}\" 不是 3 个分量");
                    continue;
                }

                if (!double.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double x) ||
                    !double.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double y) ||
                    !double.TryParse(parts[2].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out double z))
                {
                    parseErrors.Add($"第 {idx} 个点 \"{t}\" 分量无法解析为数字");
                    continue;
                }

                parsedPoints.Add(new XYZ(x, y, z));
            }

            if (parsedPoints.Count == 0)
            {
                return new TcpResponse
                {
                    RequestId = request.RequestId,
                    Success = false,
                    Message = "points 解析后无有效点。" +
                              (parseErrors.Count > 0 ? " " + string.Join("; ", parseErrors) : ""),
                    Data = null
                };
            }

            var results = new List<Dictionary<string, object>>();
            int foundCount = 0;

            foreach (XYZ pt in parsedPoints)
            {
                var item = new Dictionary<string, object>
            {
                { "point", XYZToDict(pt) }
            };

                try
                {
                    SpatialElement found = FindSpatialElement(pt, ctx);
                    if (found != null)
                    {
                        item["source"] = "isPointInRoom";
                        FillRoomInfo(item, found);
                        foundCount++;
                    }
                    else
                    {
                        item["source"] = "isPointInRoom";
                        item["roomId"] = null;
                    }
                }
                catch (Exception ex)
                {
                    item["source"] = "error";
                    item["error"] = ex.Message;
                    item["roomId"] = null;
                }

                results.Add(item);
            }

            return new TcpResponse
            {
                RequestId = request.RequestId,
                Success = true,
                Message = $"查询 {parsedPoints.Count} 个点的房间归属完成",
                Data = new
                {
                    mode = "pointToRoom",
                    results = results,
                    summary = new
                    {
                        total = parsedPoints.Count,
                        found = foundCount,
                        notFound = parsedPoints.Count - foundCount,
                        parseErrorCount = parseErrors.Count
                    },
                    parseErrors = parseErrors.Count > 0 ? parseErrors : null,
                    timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
                }
            };
        }

        // ═══════════════════════════════════════════
        //  元素 → 房间
        // ═══════════════════════════════════════════

        private static void ResolveElementRoom(
            Element elem, SpatialContext ctx, Dictionary<string, object> item)
        {
            // 快速路径 1：FamilyInstance.Room / Space
            if (elem is FamilyInstance fi)
            {
                try
                {
                    Room r = fi.Room;
                    if (r != null)
                    {
                        item["source"] = "familyInstanceRoom";
                        FillRoomInfo(item, r);
                        return;
                    }
                }
                catch { }

                try
                {
                    Space s = fi.Space;
                    if (s != null)
                    {
                        item["source"] = "familyInstanceSpace";
                        FillRoomInfo(item, s);
                        return;
                    }
                }
                catch { }
            }

            // 兜底：位置点 + IsPointInRoom
            XYZ pt = GetLocationPoint(elem);
            if (pt == null)
            {
                item["source"] = "noLocation";
                item["roomId"] = null;
                return;
            }

            item["locationPoint"] = XYZToDict(pt);

            SpatialElement found = FindSpatialElement(pt, ctx);
            if (found != null)
            {
                item["source"] = "locationPoint";
                FillRoomInfo(item, found);
            }
            else
            {
                item["source"] = "locationPoint";
                item["roomId"] = null;
            }
        }

        // ═══════════════════════════════════════════
        //  点 → 房间
        // ═══════════════════════════════════════════

        private static SpatialElement FindSpatialElement(XYZ pt, SpatialContext ctx)
        {
            foreach (Room room in ctx.Rooms)
            {
                try
                {
                    if (room.IsPointInRoom(pt))
                        return room;
                }
                catch { }
            }

            foreach (Space space in ctx.Spaces)
            {
                try
                {
                    if (space.IsPointInSpace(pt))
                        return space;
                }
                catch { }
            }

            return null;
        }

        // ═══════════════════════════════════════════
        //  辅助方法
        // ═══════════════════════════════════════════

        private static XYZ GetLocationPoint(Element elem)
        {
            try
            {
                Location loc = elem.Location;
                if (loc is LocationPoint lp)
                    return lp.Point;

                if (loc is LocationCurve lc)
                {
                    Curve c = lc.Curve;
                    if (c != null)
                    {
                        try { return c.Evaluate(0.5, true); }
                        catch { }
                    }
                }
            }
            catch { }

            try
            {
                BoundingBoxXYZ bbox = elem.get_BoundingBox(null);
                if (bbox != null && bbox.Enabled)
                    return (bbox.Min + bbox.Max) * 0.5;
            }
            catch { }

            return null;
        }

        private static void FillRoomInfo(
            Dictionary<string, object> item, SpatialElement se)
        {
            item["roomId"] = se.Id.IntegerValue.ToString();

            try { item["roomName"] = se.Name ?? ""; }
            catch { item["roomName"] = ""; }

            try { item["roomNumber"] = se.Number ?? ""; }
            catch { item["roomNumber"] = ""; }

            if (se is Room room)
            {
                item["spatialType"] = "Room";
                try
                {
                    Level lvl = room.Level;
                    if (lvl != null)
                    {
                        item["roomLevelId"] = lvl.Id.IntegerValue.ToString();
                        item["roomLevelName"] = lvl.Name ?? "";
                    }
                }
                catch { }
            }
            else if (se is Space space)
            {
                item["spatialType"] = "Space";
                try
                {
                    Level lvl = space.Level;
                    if (lvl != null)
                    {
                        item["roomLevelId"] = lvl.Id.IntegerValue.ToString();
                        item["roomLevelName"] = lvl.Name ?? "";
                    }
                }
                catch { }
            }
        }

        private static Dictionary<string, double> XYZToDict(XYZ p)
        {
            if (p == null) return null;
            return new Dictionary<string, double>
        {
            { "x", Math.Round(p.X, 6) },
            { "y", Math.Round(p.Y, 6) },
            { "z", Math.Round(p.Z, 6) }
        };
        }
    }

    // ═══════════════════════════════════════════
    //  上下文
    // ═══════════════════════════════════════════

    internal class SpatialContext
    {
        public Document Document { get; set; }
        public List<Room> Rooms { get; set; } = new List<Room>();
        public List<Space> Spaces { get; set; } = new List<Space>();
    }
}
