using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Server.plugins
{

	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;

	internal class PluginPointFactoryImpl : PluginPointFactory
	{

		 public override PluginPoint CreateFrom( ServerPlugin plugin, System.Reflection.MethodInfo method, Type discovery )
		 {
			  ResultConverter result = ResultConverter.Get( method.GenericReturnType );
			  Type[] types = method.GenericParameterTypes;
			  Annotation[][] annotations = method.ParameterAnnotations;
			  SourceExtractor sourceExtractor = null;
			  DataExtractor[] extractors = new DataExtractor[types.Length];
			  for ( int i = 0; i < types.Length; i++ )
			  {
					Description description = null;
					Parameter param = null;
					Source source = null;
					foreach ( Annotation annotation in annotations[i] )
					{
						 if ( annotation is Description )
						 {
							  description = ( Description ) annotation;
						 }
						 else if ( annotation is Parameter )
						 {
							  param = ( Parameter ) annotation;
						 }
						 else if ( annotation is Source )
						 {
							  source = ( Source ) annotation;
						 }
					}
					if ( param != null && source != null )
					{
						 throw new System.InvalidOperationException( string.Format( "Method parameter {0:D} of {1} cannot be retrieved as both Parameter and Source", Convert.ToInt32( i ), method ) );
					}
					else if ( source != null )
					{
						 if ( types[i] != discovery )
						 {
//JAVA TO C# CONVERTER WARNING: The .NET Type.FullName property will not always yield results identical to the Java Class.getName method:
							  throw new System.InvalidOperationException( "Source parameter type (" + types[i] + ") must equal the discovery type (" + discovery.FullName + ")." );
						 }
						 if ( sourceExtractor != null )
						 {
							  throw new System.InvalidOperationException( "Server Extension methods may have at most one Source parameter." );
						 }
						 extractors[i] = sourceExtractor = new SourceExtractor( source, description );
					}
					else if ( param != null )
					{
						 extractors[i] = ParameterExtractor( types[i], param, description );
					}
					else
					{
						 throw new System.InvalidOperationException( "Parameters of Server Extension methods must be annotated as either Source or Parameter." );
					}
			  }
			  return new PluginMethod( NameOf( method ), discovery, plugin, result, method, extractors, method.getAnnotation( typeof( Description ) ) );
		 }

		 private static ParameterExtractor ParameterExtractor( Type type, Parameter parameter, Description description )
		 {
			  if ( type is ParameterizedType )
			  {
					ParameterizedType paramType = ( ParameterizedType ) type;
					Type raw = ( Type ) paramType.RawType;
					Type componentType = paramType.ActualTypeArguments[0];
					Type component = null;
					if ( componentType is Type )
					{
						 component = ( Type ) componentType;
					}
					if ( typeof( ISet<object> ) == raw )
					{
						 TypeCaster caster = _types[component];
						 if ( caster != null )
						 {
							  return new ListParameterExtractorAnonymousInnerClass( caster, component, parameter, description );
						 }
					}
					else if ( typeof( System.Collections.IList ) == raw || typeof( System.Collections.ICollection ) == raw || typeof( System.Collections.IEnumerable ) == raw )
					{
						 TypeCaster caster = _types[component];
						 if ( caster != null )
						 {
							  return new ListParameterExtractorAnonymousInnerClass2( caster, component, parameter, description );
						 }
					}
			  }
			  else if ( type is Type )
			  {
					Type raw = ( Type ) type;
					if ( raw.IsArray )
					{
						 TypeCaster caster = _types[raw.GetElementType()];
						 if ( caster != null )
						 {
							  return new ListParameterExtractorAnonymousInnerClass3( caster, raw.GetElementType(), parameter, description );
						 }
					}
					else
					{
						 TypeCaster caster = _types[raw];
						 if ( caster != null )
						 {
							  return new ParameterExtractor( caster, raw, parameter, description );
						 }
					}
			  }
			  else if ( type is GenericArrayType )
			  {
					GenericArrayType array = ( GenericArrayType ) type;
					Type component = array.GenericComponentType;
					if ( component is Type )
					{
						 TypeCaster caster = _types[component];
						 if ( caster != null )
						 {
							  return new ListParameterExtractorAnonymousInnerClass4( caster, parameter, description );
						 }
					}
			  }
			  throw new System.InvalidOperationException( "Unsupported parameter type: " + type );
		 }

		 private class ListParameterExtractorAnonymousInnerClass : ListParameterExtractor
		 {
			 public ListParameterExtractorAnonymousInnerClass( Org.Neo4j.Server.plugins.TypeCaster caster, Type component, Parameter parameter, Description description ) : base( caster, component, parameter, description )
			 {
			 }

			 internal override object convert( object[] result )
			 {
				  return new HashSet<>( Arrays.asList( result ) );
			 }
		 }

		 private class ListParameterExtractorAnonymousInnerClass2 : ListParameterExtractor
		 {
			 public ListParameterExtractorAnonymousInnerClass2( Org.Neo4j.Server.plugins.TypeCaster caster, Type component, Parameter parameter, Description description ) : base( caster, component, parameter, description )
			 {
			 }

			 internal override object convert( object[] result )
			 {
				  return Arrays.asList( result );
			 }
		 }

		 private class ListParameterExtractorAnonymousInnerClass3 : ListParameterExtractor
		 {
			 public ListParameterExtractorAnonymousInnerClass3( Org.Neo4j.Server.plugins.TypeCaster caster, UnknownType getComponentType, Parameter parameter, Description description ) : base( caster, getComponentType, parameter, description )
			 {
			 }

			 internal override object convert( object[] result )
			 {
				  return result;
			 }
		 }

		 private class ListParameterExtractorAnonymousInnerClass4 : ListParameterExtractor
		 {
			 public ListParameterExtractorAnonymousInnerClass4( Org.Neo4j.Server.plugins.TypeCaster caster, Parameter parameter, Description description ) : base( caster, ( Type ) component, parameter, description )
			 {
			 }

			 internal override object convert( object[] result )
			 {
				  return result;
			 }
		 }

		 private static void Put( IDictionary<Type, TypeCaster> types, TypeCaster caster, params Type[] keys )
		 {
			  foreach ( Type key in keys )
			  {
					types[key] = caster;
			  }
		 }

		 private static readonly IDictionary<Type, TypeCaster> _types = new Dictionary<Type, TypeCaster>();
		 static PluginPointFactoryImpl()
		 {
			  Put( _types, new StringTypeCaster(), typeof(string) );
			  Put( _types, new ByteTypeCaster(), typeof(sbyte), typeof(Byte) );
			  Put( _types, new ShortTypeCaster(), typeof(short), typeof(Short) );
			  Put( _types, new IntegerTypeCaster(), typeof(int), typeof(Integer) );
			  Put( _types, new LongTypeCaster(), typeof(long), typeof(Long) );
			  Put( _types, new CharacterTypeCaster(), typeof(char), typeof(Character) );
			  Put( _types, new BooleanTypeCaster(), typeof(bool), typeof(Boolean) );
			  Put( _types, new FloatTypeCaster(), typeof(float), typeof(Float) );
			  Put( _types, new DoubleTypeCaster(), typeof(double), typeof(Double) );
			  Put( _types, new MapTypeCaster(), typeof(System.Collections.IDictionary) );
			  Put( _types, new NodeTypeCaster(), typeof(Node) );
			  Put( _types, new RelationshipTypeCaster(), typeof(Relationship) );
			  Put( _types, new RelationshipTypeTypeCaster(), typeof(RelationshipType) );
			  Put( _types, new UriTypeCaster(), typeof(URI) );
			  Put( _types, new URLTypeCaster(), typeof(URL) );
		 }

		 private static string NameOf( System.Reflection.MethodInfo method )
		 {
			  Name name = method.getAnnotation( typeof( Name ) );
			  if ( name != null )
			  {
					return name.value();
			  }
			  return method.Name;
		 }

	}

}