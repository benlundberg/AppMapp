using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MagicPattern.Core
{
    public class GeneratorService : IGeneratorConfig
    {
        public void GenerateClasses()
        {
            if (CodeWriter == null)
            {
                CodeWriter = new CSharpCodeWriter();
            }
            else
            {
                throw new InvalidOperationException("This instance of JsonClassGenerator has already been used. Please create a new instance.");
            }

            JObject[] examples;

            using (var sr = new StringReader(Example))
            {
                using (var reader = new JsonTextReader(sr))
                {
                    var json = JToken.ReadFrom(reader);
                    if (json is JArray)
                    {
                        examples = ((JArray)json).Cast<JObject>().ToArray();
                    }
                    else if (json is JObject)
                    {
                        examples = new[] { (JObject)json };
                    }
                    else
                    {
                        throw new Exception("Sample JSON must be either a JSON array, or a JSON object.");
                    }
                }
            }

            Types = new List<ClassInfo>();

            var rootType = new ClassInfo(this, examples[0])
            {
                UseMapping = true,
                UseMemoryChache = true,
                UseOfflineSupport = true,
                UseService = true,
                UseJsonProperty = this.PropertyAttribute == "JsonProperty",
                InternalVisibility = !AlwaysUsePublicValues
            };

            rootType.AssignName(MainClass);

            GenerateClass(examples, rootType);
        }

        private void GenerateClass(JObject[] examples, ClassInfo rootType)
        {
            var jsonFields = new Dictionary<string, ClassInfo>();
            var fieldExamples = new Dictionary<string, IList<object>>();

            var first = true;

            foreach (var obj in examples)
            {
                foreach (var prop in obj.Properties())
                {
                    var currentType = new ClassInfo(this, prop.Value);
                    var propName = prop.Name;

                    if (jsonFields.TryGetValue(propName, out ClassInfo fieldType))
                    {
                        var commonType = fieldType.GetCommonType(currentType);

                        jsonFields[propName] = commonType;
                    }
                    else
                    {
                        var commonType = currentType;

                        if (first)
                        {
                            commonType = commonType.MaybeMakeNullable(this);
                        }
                        else
                        {
                            commonType = commonType.GetCommonType(ClassInfo.GetNull(this));
                        }

                        jsonFields.Add(propName, commonType);
                        fieldExamples[propName] = new List<object>();
                    }

                    var fe = fieldExamples[propName];
                    var val = prop.Value;

                    if (val.Type == JTokenType.Null || val.Type == JTokenType.Undefined)
                    {
                        if (!fe.Contains(null))
                        {
                            fe.Insert(0, null);
                        }
                    }
                    else
                    {
                        var v = val.Type == JTokenType.Array || val.Type == JTokenType.Object ? val : val.Value<object>();
                        if (!fe.Any(x => v.Equals(x)))
                            fe.Add(v);
                    }
                }

                first = false;
            }

            foreach (var field in jsonFields)
            {
                var fieldType = field.Value;
                if (fieldType.Type == JsonTypeEnum.Object)
                {
                    var subexamples = new List<JObject>(examples.Length);
                    foreach (var obj in examples)
                    {
                        if (obj.TryGetValue(field.Key, out JToken value))
                        {
                            if (value.Type == JTokenType.Object)
                            {
                                subexamples.Add((JObject)value);
                            }
                        }
                    }

                    fieldType.AssignName(field.Key.CreateUniqueClassName());
                    if (!classNames.Contains(field.Key))
                    {
                        GenerateClass(subexamples.ToArray(), fieldType);

                        classNames.Add(field.Key);
                    }
                }

                if (fieldType.InternalType != null && fieldType.InternalType.Type == JsonTypeEnum.Object)
                {
                    var subexamples = new List<JObject>(examples.Length);
                    foreach (var obj in examples)
                    {
                        if (obj.TryGetValue(field.Key, out JToken value))
                        {
                            if (value.Type == JTokenType.Array)
                            {
                                foreach (var item in (JArray)value)
                                {
                                    if (!(item is JObject)) throw new NotSupportedException("Arrays of non-objects are not supported yet.");
                                    subexamples.Add((JObject)item);
                                }

                            }
                            else if (value.Type == JTokenType.Object)
                            {
                                foreach (var item in (JObject)value)
                                {
                                    if (!(item.Value is JObject)) throw new NotSupportedException("Arrays of non-objects are not supported yet.");

                                    subexamples.Add((JObject)item.Value);
                                }
                            }
                        }
                    }

                    field.Value.InternalType.AssignName(field.Key.CreateUniqueClassNameFromPlural());
                    GenerateClass(subexamples.ToArray(), field.Value.InternalType);
                }
            }


            rootType.Fields = jsonFields.Select(x => new FieldInfo
            {
                JsonMemberName = x.Key,
                MemberName = x.Key.ToTitleCase(),
                ClassInfo = x.Value,
                Examples = fieldExamples[x.Key]
            }).ToArray();

            Types.Add(rootType);
        }

        // JSON Example that's the source
        public string Example { get; set; }

        // Default namespace
        public string Namespace { get; set; }
        
        // Main class. Example is RootObjectDto
        public string MainClass { get; set; }

        // What kind of property attribrute to be used (JsonProperty)
        public string PropertyAttribute { get; set; }

        // If public is to be used
        public bool AlwaysUsePublicValues { get; set; }

        // If nullable values should be used
        public bool AlwaysUseNullableValues { get; set; }

        // The code writer to be used
        public ICodeWriter CodeWriter { get; set; }

        // All class types that can be recognized from the json
        public IList<ClassInfo> Types { get; private set; }

        // List to know if we have wrote a filed name 
        private readonly List<string> classNames = new List<string>();
    }
}
