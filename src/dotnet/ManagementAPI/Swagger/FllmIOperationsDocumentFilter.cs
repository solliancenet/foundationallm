using Asp.Versioning;
using DocumentFormat.OpenXml.Wordprocessing;
using FoundationaLLM.Common.Interfaces;
using FoundationaLLM.Common.Models.ResourceProviders;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json.Serialization;
using UglyToad.PdfPig.Graphics.Operations.SpecialGraphicsState;

namespace FoundationaLLM.Management.API.Swagger
{

    public class FllmIOperationsDocumentFilter : IDocumentFilter
    {
        IEnumerable<IResourceProviderService> _providerServer;
        ILogger _logger;

        public FllmIOperationsDocumentFilter(
            IEnumerable<IResourceProviderService> resourceProviderServices,
            ILogger<FllmIOperationsDocumentFilter> logger)
        {
            _providerServer = resourceProviderServices;
            _logger = logger;
        }

        public OpenApiSchema GetSchemaFromType(OpenApiDocument swaggerDoc, PropertyInfo prop)
        {
            string name = null;

            //get the custom attribute
            var attr = prop.GetCustomAttribute<JsonPropertyNameAttribute>();

            var attrIgnore = prop.GetCustomAttribute<JsonIgnoreAttribute>();

            if (attrIgnore != null && prop.Name != "Type")
                return null;

            OpenApiSchema s2 = new OpenApiSchema();

            if (attr != null)
                name = attr.Name;
            else
                name = prop.Name;

            switch (prop.PropertyType.Name)
            {
                case "String":
                    s2.Type = "string";
                    break;
                case "Decimal":
                case "Float":
                case "Single":
                    s2.Type = "number";
                    break;
                case "Integer":
                case "Int32":
                case "Int64":
                    s2.Type = "integer";
                    s2.Default = new OpenApiInteger(0);
                    break;
                case "Boolean":
                    s2.Type = "boolean";
                    break;
                case "Dictionary`2":
                    s2.Type = "object";
                    s2.AdditionalProperties = new OpenApiSchema { Type = "string" };
                    break;
                case "String[]":
                    s2.Type = "array";
                    s2.Items = new OpenApiSchema { Type = "string" };
                    break;
                case "Integer[]":
                case "Int32[]":
                case "Int64[]":
                    s2.Type = "array";
                    s2.Items = new OpenApiSchema { Type = "integer" };
                    break;
                case "List`1":
                    s2.Type = "array";
                    string fullname = prop.PropertyType.FullName;
                    string subTypeName = fullname.Replace("System.Collections.Generic.List`1[[", "").Replace("]]", "");
                    Type t = Type.GetType(subTypeName);

                    if (!subTypeName.StartsWith("System."))
                    {
                        ConvertType(swaggerDoc, t);

                        s2.Items = new OpenApiSchema { Reference = new OpenApiReference { Id = t.Name, Type = ReferenceType.Schema } };
                        //s2.AnyOf = new List<OpenApiSchema> { new OpenApiSchema { Type = "object", Reference = new OpenApiReference { Id = t.Name, Type = ReferenceType.Schema } } };
                    }
                    else
                    {
                        s2.Type = t.Name.ToLower();
                    }
                    break;
                default:
                    s2.Type = "object";

                    if (prop.PropertyType.FullName.StartsWith("FoundationaLLM"))
                    {
                        string name2 = prop.PropertyType.Name;

                        if (name2 == "KnowledgeManagementAgent")
                            Console.WriteLine();

                        OpenApiSchema objectS = ConvertType(swaggerDoc, prop.PropertyType);

                        //get the custom attribute
                        var attr2 = prop.PropertyType.GetCustomAttribute<JsonPropertyNameAttribute>();

                        if (attr2 != null)
                            name2 = attr2.Name;

                        if (!swaggerDoc.Components.Schemas.ContainsKey(name2))
                            swaggerDoc.Components.Schemas.Add(name2, objectS);

                        s2.Reference = new OpenApiReference { Id = name2, Type = ReferenceType.Schema };
                    }
                    break;
            }

            s2.Title = name;
            return s2;
        }

        public OpenApiSchema ConvertType(OpenApiDocument swaggerDoc, Type type)
        {
            OpenApiMediaType mediaType = new OpenApiMediaType();

            OpenApiSchema s = new OpenApiSchema();
            s.Title = type.Name;
            s.Type = "object";

            if (type.Name == "KnowledgeManagementAgent")
                Console.WriteLine();

            if (type.Name == "RoleDefinition")
                Console.WriteLine();

            //#check if its an enum
            if (type.IsEnum)
            {
                s.Type = "string";
                s.Enum = new List<IOpenApiAny>();

                foreach (var value in Enum.GetValues(type))
                {
                    s.Enum.Add(new OpenApiString(value.ToString()));
                }
            }
            else
            {
                PropertyInfo[] props = type.GetProperties();

                //find the type and add it first...
                foreach (PropertyInfo prop in props)
                {
                    OpenApiSchema s2 = GetSchemaFromType(swaggerDoc, prop);

                    if (s2 != null && s2.Title == "type")
                        s.Properties.Add(s2.Title, s2);
                }
                foreach (PropertyInfo prop in props)
                {
                    OpenApiSchema s2 = GetSchemaFromType(swaggerDoc, prop);

                    if (s2 == null || s2.Title == "type")
                        continue;

                    s.Properties.Add(s2.Title, s2);
                }
            }

            if (!swaggerDoc.Components.Schemas.ContainsKey(type.Name))
                swaggerDoc.Components.Schemas.Add(type.Name, s);
            else
                swaggerDoc.Components.Schemas[type.Name] = s;

            return s;
        }

        public void AddErrorResponses(OpenApiOperation op)
        {
            OpenApiResponse res404 = new OpenApiResponse
            {
                Description = "Not Found",
                Content = new Dictionary<string, OpenApiMediaType>
                    {
                        {
                            "application/json", new OpenApiMediaType
                            {
                                Schema = new OpenApiSchema
                                {
                                    Type = "string"
                                }
                            }
                        }
                    }
            };

            op.Responses.Add("404", res404);

            OpenApiResponse res400 = new OpenApiResponse
            {
                Description = "Failure",
                Content = new Dictionary<string, OpenApiMediaType>
                    {
                        {
                            "application/problem+json", new OpenApiMediaType
                            {
                                Schema = new OpenApiSchema
                                {
                                    Type = "object",
                                    Properties = new Dictionary<string, OpenApiSchema>
                                    {
                                        { "detail", new OpenApiSchema
                                            {
                                                Type = "string"
                                            } },
                                        { "status", new OpenApiSchema
                                            {
                                                Type = "string"
                                            } },
                                        { "title", new OpenApiSchema
                                            {
                                                Type = "string"
                                            } },
                                        { "type", new OpenApiSchema
                                            {
                                                Type = "string"
                                            } }
                                    }
                                }
                            }
                        }
                    }
            };

            op.Responses.Add("400", res400);
        }

        public List<Type> GetDerivedTypes(OpenApiDocument swaggerDoc, Type t)
        {
            //get all classes that use this base type
            var derived_types = new List<Type>();
            foreach (var domain_assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var assembly_types = domain_assembly.GetTypes()
                      .Where(type => type.IsSubclassOf(t) && !type.IsAbstract);

                    derived_types.AddRange(assembly_types);
                }
                catch (Exception ex)
                {

                }
            }

            foreach (Type type in derived_types)
            {
                OpenApiSchema temp = ConvertType(swaggerDoc, type);
            }

            return derived_types;
        }

        public void ProcessAllowedType(OpenApiDocument swaggerDoc, IOpenApiAny version, string providerName, string resourceName, List<ResourceTypeAllowedTypes> allowedTypes, bool isAction)
        {
            OpenApiPathItem pathItem = new OpenApiPathItem();

            // Tags are for group the operations
            var openApiTags = new List<OpenApiTag> { new OpenApiTag { Name = providerName } };

            foreach (var allowedType in allowedTypes)
            {
                OpenApiOperation op = new OpenApiOperation();
                op.Tags = openApiTags;
                op.Responses = new OpenApiResponses();

                foreach (var returnType in allowedType.AllowedReturnTypes)
                {
                    //get all classes that use this base type
                    var derived_types = GetDerivedTypes(swaggerDoc, returnType);

                    OpenApiSchema returnTypeSchema = ConvertType(swaggerDoc, returnType);

                    Dictionary<string, OpenApiMediaType> item = new Dictionary<string, OpenApiMediaType>();

                    //these are arrays...I DUNNO...
                    if (allowedType.HttpMethod == "GET")
                    {
                        //create a new one...copy
                        OpenApiSchema s = new OpenApiSchema();
                        s.Type = "array";
                        s.Properties = null;
                        //s.AnyOf = new List<OpenApiSchema> { new OpenApiSchema { Type = "object", Reference = new OpenApiReference { Id = returnType.Name, Type = ReferenceType.Schema } } };
                        s.Items = new OpenApiSchema { Reference = new OpenApiReference { Id = returnType.Name, Type = ReferenceType.Schema } };
                        item.Add("application/json", new OpenApiMediaType { Schema = s });
                    }
                    else
                    {
                        item.Add("application/json", new OpenApiMediaType { Schema = new OpenApiSchema { Reference = new OpenApiReference { Id = returnTypeSchema.Title, Type = ReferenceType.Schema } } });
                    }

                    if ( derived_types.Count > 0)
                    {
                        //add all of them as options...
                        //create a new one...copy
                        OpenApiSchema s = new OpenApiSchema();
                        s.Type = "object";
                        s.Properties = null;

                        item = new Dictionary<string, OpenApiMediaType>();
                        List<OpenApiSchema> items = new List<OpenApiSchema>();

                        foreach (Type t in derived_types)
                        {
                            s.AllOf.Add(new OpenApiSchema { Reference = new OpenApiReference { Id = t.Name, Type = ReferenceType.Schema } });
                        }

                        item.Add("application/json", new OpenApiMediaType { Schema = s });
                    }

                    var res = new OpenApiResponse
                    {
                        Description = "Success",
                        Content = item
                    };

                    op.Responses.Add("200", res);
                };

                if (allowedType.AllowedReturnTypes.Count == 0)
                {
                    var res = new OpenApiResponse
                    {
                        Description = "Success"
                    };

                    op.Responses.Add("200", res);
                }

                //AddErrorResponses(op);

                foreach (var allowedBodyType in allowedType.AllowedBodyTypes)
                {
                    //get all classes that use this base type
                    var derived_types = GetDerivedTypes(swaggerDoc, allowedBodyType);

                    OpenApiSchema s = ConvertType(swaggerDoc, allowedBodyType);

                    op.RequestBody = new OpenApiRequestBody();

                    Dictionary<string, OpenApiMediaType> item = new Dictionary<string, OpenApiMediaType>();

                    item.Add("application/json", new OpenApiMediaType { Schema = new OpenApiSchema { Reference = new OpenApiReference { Id = allowedBodyType.Name, Type = ReferenceType.Schema } } });

                    if ( derived_types.Count > 0)
                    {
                        //add all of them as options...
                        //create a new one...copy
                        s = new OpenApiSchema();
                        s.Type = "object";
                        s.Properties = null;

                        item = new Dictionary<string, OpenApiMediaType>();
                        List<OpenApiSchema> items = new List<OpenApiSchema>();

                        foreach (Type t in derived_types)
                        {
                            s.AllOf.Add(new OpenApiSchema { Reference = new OpenApiReference { Id = t.Name, Type = ReferenceType.Schema } });
                        }

                        item.Add("application/json", new OpenApiMediaType { Schema = s });
                    }

                    op.RequestBody.Content = item;
                }

                OpenApiParameter param = new OpenApiParameter();
                param.Name = "instanceId";
                param.Required = true;
                param.In = ParameterLocation.Path;
                param.Schema = new OpenApiSchema { Type = "string" };
                op.Parameters.Add(param);

                param = new OpenApiParameter();
                param.Name = "name";
                param.Required = true;
                param.In = ParameterLocation.Path;
                param.Schema = new OpenApiSchema { Type = "string" };
                op.Parameters.Add(param);

                param = new OpenApiParameter();
                param.Name = "api-version";
                param.Description = "The requested API version";
                param.In = ParameterLocation.Query;
                param.Schema = new OpenApiSchema { Type = "string", Default = version };
                op.Parameters.Add(param);

                switch (allowedType.HttpMethod)
                {
                    case "GET":
                        op.OperationId = $"{resourceName}_Get".Replace("FoundationaLLM.", "");
                        pathItem.Operations.Add(OperationType.Get, op);
                        break;
                    case "POST":
                        op.OperationId = $"{resourceName}_Upsert".Replace("FoundationaLLM.", "");
                        pathItem.Operations.Add(OperationType.Post, op);
                        break;
                    case "PUT":
                        op.OperationId = $"{resourceName}_Upsert".Replace("FoundationaLLM.", "");
                        pathItem.Operations.Add(OperationType.Put, op);
                        break;
                    case "DELETE":
                        op.OperationId = $"{resourceName}_Delete".Replace("FoundationaLLM.", "");
                        pathItem.Operations.Add(OperationType.Delete, op);
                        break;
                }
            }

            if (pathItem.Operations.Count > 0 && !isAction)
                swaggerDoc.Paths.Add($"/instances/{{instanceId}}/providers/{providerName}/{resourceName}/{{name}}", pathItem);
        }

        public void AddListPath(OpenApiDocument swaggerDoc, IOpenApiAny version, string providerName, string resourceName, List<ResourceTypeAllowedTypes> allowedTypes)
        {
            OpenApiPathItem pathItem = new OpenApiPathItem();

            var openApiTags = new List<OpenApiTag> { new OpenApiTag { Name = providerName } };

            OpenApiOperation op = new OpenApiOperation();
            op.Tags = openApiTags;
            op.Responses = new OpenApiResponses();
            op.OperationId = $"{resourceName}_List".Replace("FoundationaLLM.", "");

            OpenApiParameter param = new OpenApiParameter();
            param.Name = "instanceId";
            param.Required = true;
            param.In = ParameterLocation.Path;
            param.Schema = new OpenApiSchema { Type = "string" };
            op.Parameters.Add(param);

            param = new OpenApiParameter();
            param.Name = "api-version";
            param.Description = "The requested API version";
            param.In = ParameterLocation.Query;
            param.Schema = new OpenApiSchema { Type = "string", Default = version };
            op.Parameters.Add(param);

            foreach (var allowedType in allowedTypes)
            {
                if (allowedType.HttpMethod == "GET")
                {
                    Dictionary<string, OpenApiMediaType> item = new Dictionary<string, OpenApiMediaType>();
                    Type returnType = allowedType.AllowedReturnTypes[0];

                    //create a new one...copy
                    OpenApiSchema returnS = ConvertType(swaggerDoc, returnType);

                    OpenApiSchema s = new OpenApiSchema();
                    s.Type = "array";
                    s.Properties = null;
                    s.Items = new OpenApiSchema { Reference = new OpenApiReference { Id = returnType.Name, Type = ReferenceType.Schema } };
                    item.Add("application/json", new OpenApiMediaType { Schema = s });

                    var res = new OpenApiResponse
                    {
                        Description = "Success",
                        Content = item
                    };

                    op.Responses.Add("200", res);

                    pathItem.Operations.Add(OperationType.Get, op);

                    //Add the list operation
                    swaggerDoc.Paths.Add($"/instances/{{instanceId}}/providers/{providerName}/{resourceName}", pathItem);
                }
            }
        }

        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            IOpenApiAny version = new OpenApiString(swaggerDoc.Info.Version);

            foreach (IResourceProviderService resourceProviderService in _providerServer)
            {
                //get all methods...
                Type t = resourceProviderService.GetType();

                //resource types
                var resourceTypes = resourceProviderService.GetResourceTypes();

                foreach (var resourceType in resourceTypes)
                {
                    ResourceTypeDescriptor val = resourceType.Value;

                    ProcessAllowedType(swaggerDoc, version, resourceProviderService.Name, resourceType.Key, val.AllowedTypes, false);

                    foreach (var action in val.Actions)
                    {
                        OpenApiPathItem pathItem = new OpenApiPathItem();

                        ProcessAllowedType(swaggerDoc, version, resourceProviderService.Name, resourceType.Key, action.AllowedTypes, true);

                        swaggerDoc.Paths.Add($"/instances/{{instanceId}}/providers/{resourceProviderService.Name}/{resourceType.Key}/{action.Name}", pathItem);
                    }

                    AddListPath(swaggerDoc, version, resourceProviderService.Name, resourceType.Key, val.AllowedTypes);
                }
            }

            Console.WriteLine("Done");
        }
    }
}
