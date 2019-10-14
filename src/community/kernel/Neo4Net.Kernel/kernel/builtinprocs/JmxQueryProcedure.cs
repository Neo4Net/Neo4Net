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
namespace Neo4Net.Kernel.builtinprocs
{

	using Neo4Net.Collections;
	using CollectorsUtil = Neo4Net.Helpers.Collections.CollectorsUtil;
	using ProcedureException = Neo4Net.Internal.Kernel.Api.exceptions.ProcedureException;
	using Neo4jTypes = Neo4Net.Internal.Kernel.Api.procs.Neo4jTypes;
	using QualifiedName = Neo4Net.Internal.Kernel.Api.procs.QualifiedName;
	using ResourceTracker = Neo4Net.Kernel.api.ResourceTracker;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using CallableProcedure = Neo4Net.Kernel.api.proc.CallableProcedure;
	using Context = Neo4Net.Kernel.api.proc.Context;
	using Mode = Neo4Net.Procedure.Mode;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Pair.pair;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.procs.ProcedureSignature.procedureSignature;

	public class JmxQueryProcedure : Neo4Net.Kernel.api.proc.CallableProcedure_BasicProcedure
	{
		 private readonly MBeanServer _jmxServer;

		 public JmxQueryProcedure( QualifiedName name, MBeanServer jmxServer ) : base( procedureSignature( name ).@in( "query", Neo4jTypes.NTString ).@out( "name", Neo4jTypes.NTString ).@out( "description", Neo4jTypes.NTString ).@out( "attributes", Neo4jTypes.NTMap ).mode( Mode.DBMS ).description( "Query JMX management data by domain and name. For instance, \"org.neo4j:*\"" ).build() )
		 {
			  this._jmxServer = jmxServer;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.collection.RawIterator<Object[], org.neo4j.internal.kernel.api.exceptions.ProcedureException> apply(org.neo4j.kernel.api.proc.Context ctx, Object[] input, org.neo4j.kernel.api.ResourceTracker resourceTracker) throws org.neo4j.internal.kernel.api.exceptions.ProcedureException
		 public override RawIterator<object[], ProcedureException> Apply( Context ctx, object[] input, ResourceTracker resourceTracker )
		 {
			  string query = input[0].ToString();
			  try
			  {
					// Find all beans that match the query name pattern
					IEnumerator<ObjectName> names = _jmxServer.queryNames( new ObjectName( query ), null ).GetEnumerator();

					// Then convert them to a Neo4j type system representation
					return RawIterator.from(() =>
					{
					 if ( !names.hasNext() )
					 {
						  return null;
					 }

					 ObjectName name = names.next();
					 try
					 {
						  MBeanInfo beanInfo = _jmxServer.getMBeanInfo( name );
						  return new object[]{ name.CanonicalName, beanInfo.Description, ToNeo4jValue( name, beanInfo.Attributes ) };
					 }
					 catch ( JMException e )
					 {
						  throw new ProcedureException( Status.General.UnknownError, e, "JMX error while accessing `%s`, please report this. Message was: %s", name, e.Message );
					 }
					});
			  }
			  catch ( MalformedObjectNameException )
			  {
					throw new ProcedureException( Neo4Net.Kernel.Api.Exceptions.Status_Procedure.ProcedureCallFailed, "'%s' is an invalid JMX name pattern. Valid queries should use" + "the syntax outlined in the javax.management.ObjectName API documentation." + "For instance, try 'org.neo4j:*' to find all JMX beans of the 'org.neo4j' " + "domain, or '*:*' to find every JMX bean.", query );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.Map<String,Object> toNeo4jValue(javax.management.ObjectName name, javax.management.MBeanAttributeInfo[] attributes) throws javax.management.JMException
		 private IDictionary<string, object> ToNeo4jValue( ObjectName name, MBeanAttributeInfo[] attributes )
		 {
			  Dictionary<string, object> @out = new Dictionary<string, object>();
			  foreach ( MBeanAttributeInfo attribute in attributes )
			  {
					if ( attribute.Readable )
					{
						 @out[attribute.Name] = ToNeo4jValue( name, attribute );
					}
			  }
			  return @out;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.Map<String,Object> toNeo4jValue(javax.management.ObjectName name, javax.management.MBeanAttributeInfo attribute) throws javax.management.JMException
		 private IDictionary<string, object> ToNeo4jValue( ObjectName name, MBeanAttributeInfo attribute )
		 {
			  object value;
			  try
			  {
					value = toNeo4jValue( _jmxServer.getAttribute( name, attribute.Name ) );
			  }
			  catch ( RuntimeMBeanException e )
			  {
					if ( e.InnerException != null && e.InnerException is System.NotSupportedException )
					{
						 // We include the name and description of this attribute still - but the value of it is
						 // unknown. We do this rather than rethrow the exception, because several MBeans built into
						 // the JVM will throw exception on attribute access depending on their runtime state, even
						 // if the attribute is marked as readable. Notably the GC beans do this.
						 value = null;
					}
					else
					{
						 throw e;
					}
			  }
			  return map( "description", attribute.Description, "value", value );
		 }

		 private object ToNeo4jValue( object attributeValue )
		 {
			  // These branches as per {@link javax.management.openmbean.OpenType#ALLOWED_CLASSNAMES_LIST}
			  if ( IsSimpleType( attributeValue ) )
			  {
					return attributeValue;
			  }
			  else if ( attributeValue.GetType().IsArray )
			  {
					if ( isSimpleType( attributeValue.GetType().GetElementType() ) )
					{
						 return attributeValue;
					}
					else
					{
						 return toNeo4jValue( ( object[] ) attributeValue );
					}
			  }
			  else if ( attributeValue is CompositeData )
			  {
					return ToNeo4jValue( ( CompositeData ) attributeValue );
			  }
			  else if ( attributeValue is ObjectName )
			  {
					return ( ( ObjectName ) attributeValue ).CanonicalName;
			  }
			  else if ( attributeValue is TabularData )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: return toNeo4jValue((java.util.Map<?,?>) attributeValue);
					return toNeo4jValue( ( IDictionary<object, ?> ) attributeValue );
			  }
			  else if ( attributeValue is DateTime )
			  {
					return ( ( DateTime ) attributeValue ).Ticks;
			  }
			  else
			  {
					// Don't convert objects that are not OpenType values
					return null;
			  }
		 }

		 private IDictionary<string, object> ToNeo4jValue<T1>( IDictionary<T1> attributeValue )
		 {
			  // Build a new map with the same keys, but each value passed
			  // through `toNeo4jValue`
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  return attributeValue.SetOfKeyValuePairs().Select(e => pair(e.Key.ToString(), toNeo4jValue(e.Value))).collect(CollectorsUtil.pairsToMap());
		 }

		 private IList<object> ToNeo4jValue( object[] array )
		 {
			  return java.util.array.Select( this.toNeo4jValue ).ToList();
		 }

		 private IDictionary<string, object> ToNeo4jValue( CompositeData composite )
		 {
			  Dictionary<string, object> properties = new Dictionary<string, object>();
			  foreach ( string key in composite.CompositeType.Keys )
			  {
					properties[key] = toNeo4jValue( composite.get( key ) );
			  }

			  return map( "description", composite.CompositeType.Description, "properties", properties );
		 }

		 private bool IsSimpleType( object value )
		 {
			  return value == null || isSimpleType( value.GetType() );
		 }

		 private bool IsSimpleType( Type cls )
		 {
			  return cls.IsAssignableFrom( typeof( string ) ) || cls.IsAssignableFrom( typeof( Number ) ) || cls.IsAssignableFrom( typeof( Boolean ) ) || cls.IsPrimitive;
		 }
	}

}