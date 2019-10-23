using System;
using System.Collections.Generic;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Kernel.impl.proc
{

	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using ProcedureException = Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException;
	using DefaultParameterValue = Neo4Net.Kernel.Api.Internal.procs.DefaultParameterValue;
	using Neo4NetTypes = Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes;
	using AnyType = Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.AnyType;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using EmbeddedProxySPI = Neo4Net.Kernel.impl.core.EmbeddedProxySPI;
	using DefaultValueMapper = Neo4Net.Kernel.impl.util.DefaultValueMapper;
	using ValueUtils = Neo4Net.Kernel.impl.util.ValueUtils;
	using Name = Neo4Net.Procedure.Name;
	using AnyValue = Neo4Net.Values.AnyValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static bool.Parse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static double.Parse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Long.parseLong;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.DefaultParameterValue.ntBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.DefaultParameterValue.ntFloat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.DefaultParameterValue.ntInteger;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.DefaultParameterValue.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTAny;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTBoolean;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTByteArray;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTDate;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTDateTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTDuration;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTFloat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTInteger;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTLocalDateTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTLocalTime;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTNumber;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.NTTime;

	public class TypeMappers : DefaultValueMapper
	{
		 public abstract class TypeChecker
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly Neo4NetTypes.AnyType TypeConflict;
			  internal readonly Type JavaClass;

			  internal TypeChecker( Neo4NetTypes.AnyType type, Type javaClass )
			  {
					this.TypeConflict = type;
					this.JavaClass = javaClass;
			  }

			  public virtual Neo4NetTypes.AnyType Type()
			  {
					return TypeConflict;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public Object typeCheck(Object javaValue) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
			  public virtual object TypeCheck( object javaValue )
			  {
					if ( javaValue == null || JavaClass.IsInstanceOfType( javaValue ) )
					{
						 return javaValue;
					}
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureCallFailed, "Expected `%s` to be a `%s`, found `%s`.", javaValue, JavaClass.Name, javaValue.GetType() );
			  }

			  public virtual AnyValue ToValue( object obj )
			  {
					return ValueUtils.of( obj );
			  }
		 }

		 private readonly IDictionary<Type, DefaultValueConverter> _javaToNeo = new Dictionary<Type, DefaultValueConverter>();

		 /// <summary>
		 /// Used by testing.
		 /// </summary>
		 public TypeMappers() : this(null)
		 {
		 }

		 public TypeMappers( EmbeddedProxySPI proxySPI ) : base( proxySPI )
		 {
			  RegisterScalarsAndCollections();
		 }

		 /// <summary>
		 /// We don't have Node, Relationship, Property available down here - and don't strictly want to,
		 /// we want the procedures to be independent of which Graph API is being used (and we don't want
		 /// them to get tangled up with kernel code). So, we only register the "core" type system here,
		 /// scalars and collection types. Node, Relationship, Path and any other future graph types should
		 /// be registered from the outside in the same place APIs to work with those types is registered.
		 /// </summary>
		 private void RegisterScalarsAndCollections()
		 {
			  RegisterType( typeof( string ), _toString );
			  RegisterType( typeof( long ), _toInteger );
			  RegisterType( typeof( Long ), _toInteger );
			  RegisterType( typeof( double ), _toFloat );
			  RegisterType( typeof( Double ), _toFloat );
			  RegisterType( typeof( Number ), _toNumber );
			  RegisterType( typeof( bool ), _toBoolean );
			  RegisterType( typeof( Boolean ), _toBoolean );
			  RegisterType( typeof( System.Collections.IDictionary ), _toMap );
			  RegisterType( typeof( System.Collections.IList ), _toList );
			  RegisterType( typeof( object ), _toAny );
			  RegisterType( typeof( sbyte[] ), _toBytearray );
			  RegisterType( typeof( ZonedDateTime ), new DefaultValueConverter( NTDateTime, typeof( ZonedDateTime ) ) );
			  RegisterType( typeof( DateTime ), new DefaultValueConverter( NTLocalDateTime, typeof( DateTime ) ) );
			  RegisterType( typeof( LocalDate ), new DefaultValueConverter( NTDate, typeof( LocalDate ) ) );
			  RegisterType( typeof( OffsetTime ), new DefaultValueConverter( NTTime, typeof( OffsetTime ) ) );
			  RegisterType( typeof( LocalTime ), new DefaultValueConverter( NTLocalTime, typeof( LocalTime ) ) );
			  RegisterType( typeof( TemporalAmount ), new DefaultValueConverter( NTDuration, typeof( TemporalAmount ) ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.Neo4Net.Kernel.Api.Internal.procs.Neo4NetTypes.AnyType toNeo4NetType(Type type) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
		 public virtual Neo4NetTypes.AnyType ToNeo4NetType( Type type )
		 {
			  return ConverterFor( type ).TypeConflict;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public TypeChecker checkerFor(Type javaType) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
		 public virtual TypeChecker CheckerFor( Type javaType )
		 {
			  return ConverterFor( javaType );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: DefaultValueConverter converterFor(Type javaType) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
		 internal virtual DefaultValueConverter ConverterFor( Type javaType )
		 {
			  DefaultValueConverter converter = _javaToNeo[javaType];
			  if ( converter != null )
			  {
					return converter;
			  }

			  if ( javaType is ParameterizedType )
			  {
					ParameterizedType pt = ( ParameterizedType ) javaType;
					Type rawType = pt.RawType;

					if ( rawType == typeof( System.Collections.IList ) )
					{
						 Type type = pt.ActualTypeArguments[0];
						 return ToList( ConverterFor( type ), type );
					}
					else if ( rawType == typeof( System.Collections.IDictionary ) )
					{
						 Type type = pt.ActualTypeArguments[0];
						 if ( type != typeof( string ) )
						 {
							  throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureRegistrationFailed, "Maps are required to have `String` keys - but this map has `%s` keys.", type.TypeName );
						 }
						 return _toMap;
					}
			  }
			  throw JavaToNeoMappingError( javaType );
		 }

		 internal virtual void RegisterType( Type javaClass, DefaultValueConverter toNeo )
		 {
			  _javaToNeo[javaClass] = toNeo;
		 }

		 private static readonly DefaultValueConverter _toAny = new DefaultValueConverter( NTAny, typeof( object ) );
		 private static readonly DefaultValueConverter _toString = new DefaultValueConverter( NTString, typeof( string ), DefaultParameterValue.ntString );
		 private static readonly DefaultValueConverter _toInteger = new DefaultValueConverter( NTInteger, typeof( Long ), s => ntInteger( parseLong( s ) ) );
		 private static readonly DefaultValueConverter _toFloat = new DefaultValueConverter( NTFloat, typeof( Double ), s => ntFloat( parseDouble( s ) ) );
		 private static readonly DefaultValueConverter _toNumber = new DefaultValueConverter(NTNumber, typeof(Number), s =>
		 {
		  try
		  {
				return ntInteger( parseLong( s ) );
		  }
		  catch ( System.FormatException )
		  {
				return ntFloat( parseDouble( s ) );
		  }
		 });
		 private static readonly DefaultValueConverter _toBoolean = new DefaultValueConverter( NTBoolean, typeof( Boolean ), s => ntBoolean( parseBoolean( s ) ) );
		 private static readonly DefaultValueConverter _toMap = new DefaultValueConverter( NTMap, typeof( System.Collections.IDictionary ), new MapConverter() );
		 private static readonly DefaultValueConverter _toList = ToList( _toAny, typeof( object ) );
		 private readonly DefaultValueConverter _toBytearray = new DefaultValueConverter( NTByteArray, typeof( sbyte[] ), new ByteArrayConverter() );

		 private static DefaultValueConverter ToList( DefaultValueConverter inner, Type type )
		 {
			  return new DefaultValueConverter( NTList( inner.Type() ), typeof(System.Collections.IList), new ListConverter(type, inner.Type()) );
		 }

		 private ProcedureException JavaToNeoMappingError( Type cls )
		 {
			  IList<string> types = Iterables.asList( _javaToNeo.Keys ).Select( Type.getTypeName ).OrderBy( string.compareTo ).ToList();

			  return new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Statement.TypeError, "Don't know how to map `%s` to the Neo4Net Type System.%n" + "Please refer to to the documentation for full details.%n" + "For your reference, known types are: %s", cls.TypeName, types );
		 }

		 public sealed class DefaultValueConverter : TypeChecker
		 {
			  internal readonly System.Func<string, DefaultParameterValue> Parser;

			  public DefaultValueConverter( Neo4NetTypes.AnyType type, Type javaClass ) : this( type, javaClass, NullParser( javaClass, type ) )
			  {
			  }

			  internal DefaultValueConverter( Neo4NetTypes.AnyType type, Type javaClass, System.Func<string, DefaultParameterValue> parser ) : base( type, javaClass )
			  {
					this.Parser = parser;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public java.util.Optional<org.Neo4Net.Kernel.Api.Internal.procs.DefaultParameterValue> defaultValue(org.Neo4Net.procedure.Name parameter) throws org.Neo4Net.Kernel.Api.Internal.Exceptions.ProcedureException
			  public Optional<DefaultParameterValue> DefaultValue( Name parameter )
			  {
					string defaultValue = parameter.defaultValue();
					if ( defaultValue.Equals( Name.DEFAULT_VALUE ) )
					{
						 return null;
					}
					else
					{
						 try
						 {
							  return Parser.apply( defaultValue );
						 }
						 catch ( Exception )
						 {
							  throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureRegistrationFailed, "Default value `%s` could not be parsed as a %s", parameter.defaultValue(), JavaClass.Name );
						 }
					}
			  }

			  internal static System.Func<string, DefaultParameterValue> NullParser( Type javaType, Neo4NetTypes.AnyType neoType )
			  {
					return s =>
					{
					 if ( s.equalsIgnoreCase( "null" ) )
					 {
						  return nullValue( neoType );
					 }
					 else
					 {
						  throw new System.ArgumentException( string.Format( "A {0} can only have a `defaultValue = \"null\"", javaType.Name ) );
					 }
					};
			  }
		 }
	}

}