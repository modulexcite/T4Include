﻿// ----------------------------------------------------------------------------------------------
// Copyright (c) Mårten Rånge.
// ----------------------------------------------------------------------------------------------
// This source code is subject to terms and conditions of the Microsoft Public License. A 
// copy of the license can be found in the License.html file at the root of this distribution. 
// If you cannot locate the  Microsoft Public License, please send an email to 
// dlr@microsoft.com. By using this source code in any fashion, you are agreeing to be bound 
//  by the terms of the Microsoft Public License.
// ----------------------------------------------------------------------------------------------
// You must not remove this notice, or any other, from this software.
// ----------------------------------------------------------------------------------------------

// ReSharper disable PartialTypeWithSinglePart
// ReSharper disable RedundantTypeArgumentsOfMethod



namespace Source.SQL
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Text;
    using System.Xml;

    sealed partial class TypeDefinition
    {
        sealed partial class SqlTypeInfo
        {
            public static readonly SqlTypeInfo Empty = new SqlTypeInfo (SqlDbType.Udt, typeof (object), -1, false, null);

            public readonly SqlDbType   DbType              ;
            public readonly Type        Type                ;
            public readonly int         ElementSize         ;
            public readonly bool        RequiresDimension   ;

            public readonly MethodInfo  GetterMethod        ;

            public readonly string      SqlName             ;
            public readonly string      CsName              ;

            public SqlTypeInfo(SqlDbType dbType, Type type, int elementSize, bool requiresDimension, MethodInfo getterMethod)
            {
                DbType              = dbType            ;
                Type                = type              ;
                ElementSize         = elementSize       ;
                RequiresDimension   = requiresDimension ;

                GetterMethod        = getterMethod      ;

                SqlName             = dbType.ToString().ToLowerInvariant();

                if (type.IsArray)
                {
                    CsName          = type.GetElementType().FullName + "[]";
                }
                else
                {
                    CsName          = type.FullName;
                }
            }
        }

        static void Add<T> (SqlDbType dbType, int elementSize, bool requiresDimension, Expression<Func<SqlDataReader, T>> getter)
        {
            var getterMethodInfo = ((MethodCallExpression)getter.Body).Method   ;
            s_typeInfoLookup.Add(dbType.ToString(), new SqlTypeInfo(dbType, typeof(T), elementSize, requiresDimension, getterMethodInfo));
        }

        readonly static Dictionary<string, SqlTypeInfo> s_typeInfoLookup = 
            new Dictionary<string, SqlTypeInfo> (StringComparer.OrdinalIgnoreCase);

        static TypeDefinition ()
        {
            Add<Int64>           (SqlDbType.BigInt           , getter:r => r.GetInt64(0)            , elementSize:8    , requiresDimension:false   );
            Add<Byte[]>          (SqlDbType.Binary           , getter:null                          , elementSize:1    , requiresDimension:true    );
            Add<Boolean>         (SqlDbType.Bit              , getter:r => r.GetBoolean(0)          , elementSize:1    , requiresDimension:false   );
            Add<String>          (SqlDbType.Char             , getter:r => r.GetString(0)           , elementSize:1    , requiresDimension:true    );
            Add<DateTime>        (SqlDbType.DateTime         , getter:r => r.GetDateTime(0)         , elementSize:8    , requiresDimension:false   );
            Add<Decimal>         (SqlDbType.Decimal          , getter:r => r.GetDecimal(0)          , elementSize:17   , requiresDimension:false   );
            Add<Double>          (SqlDbType.Float            , getter:r => r.GetDouble(0)           , elementSize:4    , requiresDimension:false   );
            Add<Byte[]>          (SqlDbType.Image            , getter:null                          , elementSize:16   , requiresDimension:false   );  // TODO: Check this
            Add<Int32>           (SqlDbType.Int              , getter:r => r.GetInt32(0)            , elementSize:4    , requiresDimension:false   );
            Add<Decimal>         (SqlDbType.Money            , getter:r => r.GetDecimal(0)          , elementSize:8    , requiresDimension:false   );
            Add<String>          (SqlDbType.NChar            , getter:r => r.GetString(0)           , elementSize:2    , requiresDimension:true    );
            Add<String>          (SqlDbType.NText            , getter:r => r.GetString(0)           , elementSize:2    , requiresDimension:false   );
            Add<String>          (SqlDbType.NVarChar         , getter:r => r.GetString(0)           , elementSize:2    , requiresDimension:true    );
            Add<Single>          (SqlDbType.Real             , getter:r => r.GetFloat(0)            , elementSize:4    , requiresDimension:false   );
            Add<Guid>            (SqlDbType.UniqueIdentifier , getter:r => r.GetGuid(0)             , elementSize:1    , requiresDimension:false   );
            Add<DateTime>        (SqlDbType.SmallDateTime    , getter:r => r.GetDateTime(0)         , elementSize:4    , requiresDimension:false   );
            Add<Int16>           (SqlDbType.SmallInt         , getter:r => r.GetInt16(0)            , elementSize:2    , requiresDimension:false   );
            Add<Decimal>         (SqlDbType.SmallMoney       , getter:r => r.GetDecimal(0)          , elementSize:4    , requiresDimension:false   );
            Add<String>          (SqlDbType.Text             , getter:r => r.GetString(0)           , elementSize:1    , requiresDimension:false   );
            Add<DateTime>        (SqlDbType.Timestamp        , getter:r => r.GetDateTime(0)         , elementSize:8    , requiresDimension:false   );
            Add<Byte>            (SqlDbType.TinyInt          , getter:r => r.GetByte(0)             , elementSize:1    , requiresDimension:false   );
            Add<Byte[]>          (SqlDbType.VarBinary        , getter:null                          , elementSize:1    , requiresDimension:true    );
            Add<String>          (SqlDbType.VarChar          , getter:r => r.GetString(0)           , elementSize:1    , requiresDimension:true    );
            Add<Byte[]>          (SqlDbType.Variant          , getter:null                          , elementSize: -1  , requiresDimension:false   );
            Add<XmlReader>       (SqlDbType.Xml              , getter:r => r.GetXmlReader(0)        , elementSize:-1   , requiresDimension:false   );  
            Add<object>          (SqlDbType.Udt              , getter:null                          , elementSize:-1   , requiresDimension:false   );  // TODO: Check this
            Add<object>          (SqlDbType.Structured       , getter:null                          , elementSize:-1   , requiresDimension:false   );  // TODO: Check this
            Add<DateTime>        (SqlDbType.Date             , getter:r => r.GetDateTime(0)         , elementSize:8    , requiresDimension:false   );
            Add<DateTime>        (SqlDbType.Time             , getter:r => r.GetDateTime(0)         , elementSize:5    , requiresDimension:false   );
            Add<DateTime>        (SqlDbType.DateTime2        , getter:r => r.GetDateTime(0)         , elementSize:8    , requiresDimension:false   );
            Add<DateTimeOffset>  (SqlDbType.DateTimeOffset   , getter:r => r.GetDateTimeOffset(0)   , elementSize:10   , requiresDimension:false   );
        }                                                                                                                                 

 
        public static readonly TypeDefinition Empty = new TypeDefinition (
            "empty" ,
            "void"  ,
            0       ,
            0       ,
            0       ,
            0       ,
            0       ,
            ""      ,
            true
            );


        public readonly string      Schema      ;
        public readonly string      Name        ;
        public readonly string      FullName    ;
        public readonly byte        SystemTypeId;
        public readonly int         UserTypeId  ;
        public readonly short       MaxLength   ;
        public readonly byte        Precision   ;
        public readonly byte        Scale       ;
        public readonly string      Collation   ;
        public readonly bool        IsNullable  ;

        readonly SqlTypeInfo        m_typeInfo  ;
        readonly string             m_asString  ;

        public TypeDefinition(
            string  schema          , 
            string  name            , 
            byte    systemTypeId    , 
            int     userTypeId      , 
            short   maxLength       , 
            byte    precision       , 
            byte    scale           , 
            string  collation       , 
            bool    isNullable
            )
        {
            Schema          = schema            ?? "";
            Name            = name              ?? "";
            FullName        = Schema + "." + Name;
            SystemTypeId    = systemTypeId      ;
            UserTypeId      = userTypeId        ;
            MaxLength       = maxLength         ;
            Precision       = precision         ;
            Scale           = scale             ;
            Collation       = collation         ?? "";
            IsNullable      = isNullable        ;

            SqlTypeInfo typeInfo;
            s_typeInfoLookup.TryGetValue (Name, out typeInfo) ;
            if (typeInfo == null)
            {
                typeInfo = SqlTypeInfo.Empty;    
            }

            m_typeInfo = typeInfo;

            m_asString      = "TD." + FullName  ;
        }

        public SqlDbType DbType
        {
            get {return m_typeInfo.DbType;}
        }

        public Type Type
        {
            get {return m_typeInfo.Type;}
        }

        public string DbTypeAsString (int? maxLength = null, bool? isNullable = null)
        {
            var sb = new StringBuilder(m_typeInfo.SqlName);

            if (m_typeInfo.RequiresDimension && maxLength != null)
            {
                sb.Append(m_typeInfo.SqlName);
                var elementSize = Math.Max(DbTypeElementSize(), 1);
                var value = maxLength.Value / elementSize;
                sb.Append('(');
                sb.Append(value.ToString(CultureInfo.InvariantCulture));
                sb.Append(')');
            }

            if (isNullable != null)
            {
                if (isNullable.Value)
                {
                    sb.Append(" NULL");                    
                }
                else
                {
                    sb.Append(" NOT NULL");                    
                }
            }

            return sb.ToString();
        }

        public int DbTypeElementSize ()
        {
            return m_typeInfo.ElementSize;
        }

        public string TypeAsCsString ()
        {
            return m_typeInfo.CsName;
        }

        public MethodInfo GetterMethod ()
        {
            return m_typeInfo.GetterMethod;
        }

        public override string ToString()
        {
            return m_asString;
        }
    }

    abstract partial class BaseTypedSubObject
    {
        public readonly string          Name        ;
        public readonly TypeDefinition  Type        ;
        public readonly int             Ordinal     ;
        public readonly short           MaxLength   ;
        public readonly byte            Precision   ;
        public readonly byte            Scale       ;

        protected BaseTypedSubObject(
            string          name        , 
            TypeDefinition  type        , 
            int             ordinal     , 
            short           maxLength   , 
            byte            precision   , 
            byte            scale        
            )
        {
            Name        = name      ?? "";
            Type        = type      ?? TypeDefinition.Empty;
            Ordinal     = ordinal   ;
            MaxLength   = maxLength ;
            Precision   = precision ;
            Scale       = scale     ;
        }
    }

    sealed partial class ColumnSubObject : BaseTypedSubObject
    {
        public readonly string          Collation   ;
        public readonly bool            IsNullable  ;
        public readonly bool            IsIdentity  ;
        public readonly bool            IsComputed  ;

        readonly string                 m_asString  ;

        public ColumnSubObject(
            string          name        , 
            TypeDefinition  type        , 
            int             ordinal     , 
            short           maxLength   , 
            byte            precision   , 
            byte            scale       , 
            string          collation   , 
            bool            isNullable  , 
            bool            isIdentity  , 
            bool            isComputed
            ) 
            : base(name, type, ordinal, maxLength, precision, scale)
        {
            Collation   = collation ?? "";
            IsNullable  = isNullable;
            IsIdentity  = isIdentity;
            IsComputed  = isComputed;

            m_asString  = "CSO." + Name ;
        }

        public override string ToString()
        {
            return m_asString;
        }
    }

    sealed partial class ParameterSubObject : BaseTypedSubObject
    {
        public readonly bool            IsOutput    ;

        readonly string                 m_asString  ;

        public ParameterSubObject(
            string          name        , 
            TypeDefinition  type        , 
            int             ordinal     , 
            short           maxLength   , 
            byte            precision   , 
            byte            scale       ,
            bool            isOutput
            ) 
            : base(name, type, ordinal, maxLength, precision, scale)
        {
            IsOutput    = isOutput  ;

            m_asString  = "PSO." + Name ;
        }

        public override string ToString()
        {
            return m_asString;
        }
    }

    sealed partial class SchemaObject
    {
        public enum SchemaObjectType
        {
            Unknown             ,
            StoredProcedure     ,
            Function            ,
            TableFunction       ,
            InlineTableFunction ,
            Table               ,
            View                ,
        }

        public readonly string                  Schema      ;
        public readonly string                  Name        ;
        public readonly string                  FullName    ;
        public readonly SchemaObjectType        Type        ;
        public readonly DateTime                CreateDate  ;
        public readonly DateTime                ModifyDate  ;

        public readonly ColumnSubObject[]       Columns     ;
        public readonly ParameterSubObject[]    Parameters  ;

        readonly string                         m_asString  ;


        public SchemaObject(
            string                  schema      , 
            string                  name        , 
            SchemaObjectType        type        , 
            DateTime                createDate  , 
            DateTime                modifyDate  ,
            ColumnSubObject[]       columns     ,
            ParameterSubObject[]    parameters  
            )
        {
            Schema          = schema        ?? "";
            Name            = name          ?? "";
            FullName        = Schema + "." + Name;
            Type            = type          ;
            CreateDate      = createDate    ;
            ModifyDate      = modifyDate    ;

            Columns         = columns       ?? new ColumnSubObject[0]       ;
            Parameters      = parameters    ?? new ParameterSubObject[0]    ;

            m_asString  = "SO." + FullName ;
        }

        public override string ToString()
        {
            return m_asString;
        }
    }

    sealed partial class Schema
    {
        // As SQL is generally case insensitive we ignore case when looking for objects
        static readonly StringComparer s_keyComparer = StringComparer.OrdinalIgnoreCase;

        readonly Dictionary<string, TypeDefinition> m_typeDefinitions   = new Dictionary<string, TypeDefinition> (s_keyComparer);
        readonly Dictionary<string, SchemaObject>   m_schemaObjects     = new Dictionary<string, SchemaObject> (s_keyComparer);

        public Schema (SqlConnection connection)
        {
            using (var command = connection.CreateCommand())
            {
                command.CommandType = CommandType.Text;
                command.CommandText = @"
SELECT
	s.name								[Schema]		,	-- 0
	t.name								Name			,	-- 1
	t.system_type_id					SystemTypeId	,	-- 2
	t.user_type_id						UserTypeId		,	-- 3
	t.max_length						[MaxLength]		,	-- 4
	t.[precision]						[Precision]		,	-- 5
	t.scale								Scale			,	-- 6
	ISNULL (t.collation_name	, '')	Collation		,	-- 7
	ISNULL (t.is_nullable		, 0)	IsNullable		 	-- 8
	FROM SYS.types t WITH(NOLOCK)
	INNER JOIN SYS.schemas s WITH(NOLOCK) ON t.schema_id = s.schema_id

SELECT 
	c.object_id							ObjectId	,	-- 0
	s.name								[TypeSchema],	-- 1
	t.name								[TypeName]	,	-- 2
	ISNULL (c.name		, '')			Name		,	-- 3
	c.column_id							Ordinal		,	-- 4
	c.max_length						[MaxLength]	,	-- 5
	c.[precision]						[Precision]	,	-- 6
	c.scale								Scale		,	-- 7
	ISNULL (c.collation_name	, '')	Collation	,	-- 8
	ISNULL (c.is_nullable		, 0)	IsNullable	,	-- 9
	c.is_identity						IsIdentity	,	-- 10
	c.is_computed						IsComputed		-- 11
	FROM SYS.objects o WITH(NOLOCK)
	INNER JOIN SYS.columns c WITH(NOLOCK) ON o.object_id = c.object_id
	INNER JOIN SYS.types t WITH(NOLOCK) ON c.user_type_id = t.user_type_id AND c.system_type_id = t.system_type_id
	INNER JOIN SYS.schemas s WITH(NOLOCK) ON t.schema_id = s.schema_id
	WHERE
		o.is_ms_shipped = 0

SELECT 
	p.object_id							ObjectId	,	-- 0
	s.name								[TypeSchema],	-- 1
	t.name								[TypeName]	,	-- 2
	ISNULL (p.name		, '')			Name		,	-- 3
	p.parameter_id						Ordinal		,	-- 4
	p.max_length						[MaxLength]	,	-- 5
	p.[precision]						[Precision]	,	-- 6
	p.scale								Scale		,	-- 7
	p.is_output							IsOutput		-- 8
	FROM SYS.objects o WITH(NOLOCK)
	INNER JOIN SYS.parameters p WITH(NOLOCK) ON o.object_id = p.object_id
	INNER JOIN SYS.types t WITH(NOLOCK) ON p.user_type_id = t.user_type_id AND p.system_type_id = t.system_type_id
	INNER JOIN SYS.schemas s WITH(NOLOCK) ON t.schema_id = s.schema_id
	WHERE
		o.is_ms_shipped = 0

SELECT
	o.object_id							ObjectId		,	-- 0
	s.name								[Schema]		,	-- 1
	o.name								Name			,	-- 2
	o.[type]							[Type]			,	-- 4
	o.create_date						CreateDate		,	-- 5
	o.modify_date						ModifyDate			-- 6
	FROM SYS.schemas s WITH(NOLOCK)
	INNER JOIN SYS.objects o WITH(NOLOCK) ON o.schema_id = s.schema_id
	WHERE
		o.is_ms_shipped = 0
		AND
		o.type IN ('P', 'TF', 'IF', 'F', 'U', 'V')
";

                var columnLookup    = new Dictionary<int, List<ColumnSubObject>> ();
                var parameterLookup = new Dictionary<int, List<ParameterSubObject>> ();

                using (var reader = command.ExecuteReader (CommandBehavior.SequentialAccess))
                {
                    while (reader.Read ())
                    {
                        var type = new TypeDefinition (
                            reader.GetString(0) , 
                            reader.GetString(1) , 
                            reader.GetByte(2)   , 
                            reader.GetInt32(3)  , 
                            reader.GetInt16(4)  , 
                            reader.GetByte(5)   , 
                            reader.GetByte(6)   , 
                            reader.GetString(7) , 
                            reader.GetBoolean(8) 
                            );
                        m_typeDefinitions[type.FullName] = type;
                    }

                    if (!reader.NextResult())
                    {
                        return;                            
                    }

                    while (reader.Read ())
                    {
                        var objectId = reader.GetInt32(0);
                        var fullName = reader.GetString(1) + "." + reader.GetString (2);
 
                        var type = FindTypeDefinition (fullName); 

                        var column = new ColumnSubObject (
                            reader.GetString(3)   ,
                            type                  ,
                            reader.GetInt32(4)    , 
                            reader.GetInt16(5)    , 
                            reader.GetByte(6)     , 
                            reader.GetByte(7)     , 
                            reader.GetString(8)   , 
                            reader.GetBoolean(9)  , 
                            reader.GetBoolean(10) , 
                            reader.GetBoolean(11) 
                            );

                        AddObject (columnLookup, objectId, column);
                    }

                    if (!reader.NextResult())
                    {
                        return;                            
                    }

                    while (reader.Read ())
                    {
                        var objectId = reader.GetInt32(0);
                        var fullName = reader.GetString(1) + "." + reader.GetString (2);
 
                        var type = FindTypeDefinition (fullName); 

                        var parameter = new ParameterSubObject (
                            reader.GetString(3)   ,
                            type                  ,
                            reader.GetInt32(4)    , 
                            reader.GetInt16(5)    , 
                            reader.GetByte(6)     , 
                            reader.GetByte(7)     , 
                            reader.GetBoolean(8)   
                            );

                        AddObject (parameterLookup, objectId, parameter);
                    }

                    if (!reader.NextResult())
                    {
                        return;                            
                    }

                    while (reader.Read ())
                    {
                        var objectId = reader.GetInt32(0);
                        var schema = reader.GetString(1);
                        var name = reader.GetString(2);
                        var fullName = schema + "." + name;
                        var schemaObjectType = ToSchemaType(reader.GetString(3));
                        
                        if (schemaObjectType == SchemaObject.SchemaObjectType.Unknown)
                        {
                            continue;
                        }

                        List<ColumnSubObject> columns;
                        List<ParameterSubObject> parameters;

                        columnLookup.TryGetValue(objectId, out columns);
                        parameterLookup.TryGetValue(objectId, out parameters);

                        var schemaObject = new SchemaObject (
                            schema                      ,
                            name                        ,
                            schemaObjectType            ,
                            reader.GetDateTime(4)       ,
                            reader.GetDateTime(5)       ,
                            NonNull(columns)
                                .Where(c => c != null)
                                .OrderBy(c => c.Ordinal)
                                .ToArray()
                                ,
                            NonNull(parameters)
                                .Where(p => p != null)
                                .OrderBy(p => p.Ordinal)
                                .ToArray()
                            );

                        m_schemaObjects[fullName] = schemaObject;
                    }

                }

                
            }

        }

        static IEnumerable<T> NonNull<T> (IEnumerable<T> list)
        {
            if (list == null)
            {
                return new T[0];
            }
            
            return list;
            
        }

        SchemaObject.SchemaObjectType ToSchemaType(string schemaType)
        {
            switch ((schemaType ?? "").Trim())
            {
                case "P":
                    return SchemaObject.SchemaObjectType.StoredProcedure;
                case "U":
                    return SchemaObject.SchemaObjectType.Table;
                case "TF":
                    return SchemaObject.SchemaObjectType.TableFunction;
                case "IF":
                    return SchemaObject.SchemaObjectType.InlineTableFunction;
                case "F":
                    return SchemaObject.SchemaObjectType.Function;
                case "V":
                    return SchemaObject.SchemaObjectType.View;
                default:
                    return SchemaObject.SchemaObjectType.Unknown;
            }
        }

        static void AddObject<T> (Dictionary<int, List<T>> dic, int key, T obj)
            where T : class
        {
            if (dic == null)
            {
                return;
            }

            if (obj == null)
            {
                return;
            }

            List<T> list;
            if (!dic.TryGetValue (key, out list))
            {
                list = new List<T> (16);
                dic[key] = list;
            }

            list.Add(obj);
        }

        public IEnumerable<TypeDefinition> TypeDefinitions
        {
            get
            {
                return m_typeDefinitions.Values;
            }
        }

        public TypeDefinition FindTypeDefinition (string fullName)
        {
            TypeDefinition typeDefinition;
            m_typeDefinitions.TryGetValue (fullName ?? "", out typeDefinition);
            return typeDefinition;
        }

        public IEnumerable<SchemaObject> SchemaObjects
        {
            get
            {
                return m_schemaObjects.Values;
            }
        }

        public SchemaObject FindSchemaObject (string fullName)
        {
            SchemaObject schemaObject;
            m_schemaObjects.TryGetValue (fullName ?? "", out schemaObject);
            return schemaObject;
        }
    
    }
}
