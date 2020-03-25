using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace MagicPattern.Core
{
    public class ClassInfo : INotifyPropertyChanged
    {
        #region Constructors

        private ClassInfo(IGeneratorConfig generator)
        {
            this.generator = generator;

            UseJsonProperty = generator.PropertyAttribute == "JsonProperty";
        }

        public ClassInfo(IGeneratorConfig generator, JToken token)
            : this(generator)
        {
            UseJsonProperty = generator.PropertyAttribute == "JsonProperty";

            Type = GetFirstTypeEnum(token);

            if (Type == JsonTypeEnum.Array)
            {
                var array = (JArray)token;
                InternalType = GetCommonType(generator, array.ToArray());
            }
        }

        #endregion

        #region Methods

        internal static ClassInfo GetNull(IGeneratorConfig generator)
        {
            return new ClassInfo(generator, JsonTypeEnum.NullableSomething);
        }

        internal ClassInfo(IGeneratorConfig generator, JsonTypeEnum type) : this(generator)
        {
            this.Type = type;
        }

        public static ClassInfo GetCommonType(IGeneratorConfig generator, JToken[] tokens)
        {

            if (tokens.Length == 0) return new ClassInfo(generator, JsonTypeEnum.NonConstrained);

            var common = new ClassInfo(generator, tokens[0]).MaybeMakeNullable(generator);

            for (int i = 1; i < tokens.Length; i++)
            {
                var current = new ClassInfo(generator, tokens[i]);
                common = common.GetCommonType(current);
            }

            return common;

        }

        internal ClassInfo MaybeMakeNullable(IGeneratorConfig generator)
        {
            if (!generator.AlwaysUseNullableValues) return this;
            return this.GetCommonType(ClassInfo.GetNull(generator));
        }

        public void AssignName(string name)
        {
            AssignedName = name + "Dto";
        }

        public ClassInfo GetInnermostType()
        {
            if (Type != JsonTypeEnum.Array) throw new InvalidOperationException();
            if (InternalType.Type != JsonTypeEnum.Array) return InternalType;
            return InternalType.GetInnermostType();
        }

        public ClassInfo GetCommonType(ClassInfo type2)
        {
            var commonType = GetCommonTypeEnum(this.Type, type2.Type);

            if (commonType == JsonTypeEnum.Array)
            {
                if (type2.Type == JsonTypeEnum.NullableSomething) return this;
                if (this.Type == JsonTypeEnum.NullableSomething) return type2;
                var commonInternalType = InternalType.GetCommonType(type2.InternalType).MaybeMakeNullable(generator);
                if (commonInternalType != InternalType) return new ClassInfo(generator, JsonTypeEnum.Array) { InternalType = commonInternalType };
            }


            //if (commonType == JsonTypeEnum.Dictionary)
            //{
            //    var commonInternalType = InternalType.GetCommonType(type2.InternalType);
            //    if (commonInternalType != InternalType) return new ClassInfo(JsonTypeEnum.Dictionary) { InternalType = commonInternalType };
            //}


            if (this.Type == commonType) return this;
            return new ClassInfo(generator, commonType).MaybeMakeNullable(generator);
        }

        private static bool IsNull(JsonTypeEnum type)
        {
            return type == JsonTypeEnum.NullableSomething;
        }

        private JsonTypeEnum GetCommonTypeEnum(JsonTypeEnum type1, JsonTypeEnum type2)
        {
            if (type1 == JsonTypeEnum.NonConstrained) return type2;
            if (type2 == JsonTypeEnum.NonConstrained) return type1;

            switch (type1)
            {
                case JsonTypeEnum.Boolean:
                    if (IsNull(type2)) return JsonTypeEnum.NullableBoolean;
                    if (type2 == JsonTypeEnum.Boolean) return type1;
                    break;
                case JsonTypeEnum.NullableBoolean:
                    if (IsNull(type2)) return type1;
                    if (type2 == JsonTypeEnum.Boolean) return type1;
                    break;
                case JsonTypeEnum.Integer:
                    if (IsNull(type2)) return JsonTypeEnum.NullableInteger;
                    if (type2 == JsonTypeEnum.Float) return JsonTypeEnum.Float;
                    if (type2 == JsonTypeEnum.Long) return JsonTypeEnum.Long;
                    if (type2 == JsonTypeEnum.Integer) return type1;
                    break;
                case JsonTypeEnum.NullableInteger:
                    if (IsNull(type2)) return type1;
                    if (type2 == JsonTypeEnum.Float) return JsonTypeEnum.NullableFloat;
                    if (type2 == JsonTypeEnum.Long) return JsonTypeEnum.NullableLong;
                    if (type2 == JsonTypeEnum.Integer) return type1;
                    break;
                case JsonTypeEnum.Float:
                    if (IsNull(type2)) return JsonTypeEnum.NullableFloat;
                    if (type2 == JsonTypeEnum.Float) return type1;
                    if (type2 == JsonTypeEnum.Integer) return type1;
                    if (type2 == JsonTypeEnum.Long) return type1;
                    break;
                case JsonTypeEnum.NullableFloat:
                    if (IsNull(type2)) return type1;
                    if (type2 == JsonTypeEnum.Float) return type1;
                    if (type2 == JsonTypeEnum.Integer) return type1;
                    if (type2 == JsonTypeEnum.Long) return type1;
                    break;
                case JsonTypeEnum.Long:
                    if (IsNull(type2)) return JsonTypeEnum.NullableLong;
                    if (type2 == JsonTypeEnum.Float) return JsonTypeEnum.Float;
                    if (type2 == JsonTypeEnum.Integer) return type1;
                    break;
                case JsonTypeEnum.NullableLong:
                    if (IsNull(type2)) return type1;
                    if (type2 == JsonTypeEnum.Float) return JsonTypeEnum.NullableFloat;
                    if (type2 == JsonTypeEnum.Integer) return type1;
                    if (type2 == JsonTypeEnum.Long) return type1;
                    break;
                case JsonTypeEnum.Date:
                    if (IsNull(type2)) return JsonTypeEnum.NullableDate;
                    if (type2 == JsonTypeEnum.Date) return JsonTypeEnum.Date;
                    break;
                case JsonTypeEnum.NullableDate:
                    if (IsNull(type2)) return type1;
                    if (type2 == JsonTypeEnum.Date) return type1;
                    break;
                case JsonTypeEnum.NullableSomething:
                    if (IsNull(type2)) return type1;
                    if (type2 == JsonTypeEnum.String) return JsonTypeEnum.String;
                    if (type2 == JsonTypeEnum.Integer) return JsonTypeEnum.NullableInteger;
                    if (type2 == JsonTypeEnum.Float) return JsonTypeEnum.NullableFloat;
                    if (type2 == JsonTypeEnum.Long) return JsonTypeEnum.NullableLong;
                    if (type2 == JsonTypeEnum.Boolean) return JsonTypeEnum.NullableBoolean;
                    if (type2 == JsonTypeEnum.Date) return JsonTypeEnum.NullableDate;
                    if (type2 == JsonTypeEnum.Array) return JsonTypeEnum.Array;
                    if (type2 == JsonTypeEnum.Object) return JsonTypeEnum.Object;
                    break;
                case JsonTypeEnum.Object:
                    if (IsNull(type2)) return type1;
                    if (type2 == JsonTypeEnum.Object) return type1;
                    if (type2 == JsonTypeEnum.Dictionary) throw new ArgumentException();
                    break;
                case JsonTypeEnum.Dictionary:
                    throw new ArgumentException();
                //if (IsNull(type2)) return type1;
                //if (type2 == JsonTypeEnum.Object) return type1;
                //if (type2 == JsonTypeEnum.Dictionary) return type1;
                //  break;
                case JsonTypeEnum.Array:
                    if (IsNull(type2)) return type1;
                    if (type2 == JsonTypeEnum.Array) return type1;
                    break;
                case JsonTypeEnum.String:
                    if (IsNull(type2)) return type1;
                    if (type2 == JsonTypeEnum.String) return type1;
                    break;
            }

            return JsonTypeEnum.Anything;

        }

        private static JsonTypeEnum GetFirstTypeEnum(JToken token)
        {
            var type = token.Type;
            if (type == JTokenType.Integer)
            {
                if ((long)((JValue)token).Value < int.MaxValue) return JsonTypeEnum.Integer;
                else return JsonTypeEnum.Long;

            }
            switch (type)
            {
                case JTokenType.Array: return JsonTypeEnum.Array;
                case JTokenType.Boolean: return JsonTypeEnum.Boolean;
                case JTokenType.Float: return JsonTypeEnum.Float;
                case JTokenType.Null: return JsonTypeEnum.NullableSomething;
                case JTokenType.Undefined: return JsonTypeEnum.NullableSomething;
                case JTokenType.String: return JsonTypeEnum.String;
                case JTokenType.Object: return JsonTypeEnum.Object;
                case JTokenType.Date: return JsonTypeEnum.Date;

                default: return JsonTypeEnum.Anything;

            }
        }

        #endregion

        public JsonTypeEnum Type { get; set; }
        public ClassInfo InternalType { get; set; }
        public string AssignedName { get; set; }
        public string Documentation { get; set; }
        public IList<FieldInfo> Fields { get; set; }
        public bool InternalVisibility { get; set; }
        public RepositoryType RepositoryType => IsSQLiteRepository ? RepositoryType.Database : RepositoryType.Storage;
        
        public string TypeName => generator.CodeWriter.GetTypeName(this, generator);
        public string VisibilityName => InternalVisibility ? "internal" : "public";
        public bool IsStorageRepository { get; set; }
        public bool IsSQLiteRepository { get; set; } = true;
        public bool UseMapping { get; set; } = true;
        public bool UseService { get; set; }
        public bool UseOfflineSupport { get; set; }
        public bool UseMemoryChache { get; set; }
        public bool UseJsonProperty { get; set; }

        private readonly IGeneratorConfig generator;

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
