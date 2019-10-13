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
	using Before = org.junit.Before;
	using Test = org.junit.Test;


	using Neo4Net.Collections;
	using ProcedureException = Neo4Net.@internal.Kernel.Api.exceptions.ProcedureException;
	using ProcedureSignature = Neo4Net.@internal.Kernel.Api.procs.ProcedureSignature;
	using ResourceTracker = Neo4Net.Kernel.api.ResourceTracker;
	using StubResourceManager = Neo4Net.Kernel.api.StubResourceManager;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.contains;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;

	public class JmxQueryProcedureTest
	{

		 private MBeanServer _jmxServer;
		 private ObjectName _beanName;
		 private string _attributeName;
		 private readonly ResourceTracker _resourceTracker = new StubResourceManager();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleBasicMBean() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleBasicMBean()
		 {
			  // given
			  when( _jmxServer.getAttribute( _beanName, "name" ) ).thenReturn( "Hello, world!" );
			  JmxQueryProcedure procedure = new JmxQueryProcedure( ProcedureSignature.procedureName( "bob" ), _jmxServer );

			  // when
			  RawIterator<object[], ProcedureException> result = procedure.Apply( null, new object[]{ "*:*" }, _resourceTracker );

			  // then
			  assertThat( asList( result ), contains( equalTo( new object[]{ "org.neo4j:chevyMakesTheTruck=bobMcCoshMakesTheDifference", "This is a description", map( _attributeName, map( "description", "This is the attribute desc.", "value", "Hello, world!" ) ) } ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleMBeanThatThrowsOnGetAttribute() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleMBeanThatThrowsOnGetAttribute()
		 {
			  // given some JVM MBeans do not allow accessing their attributes, despite marking
			  // then as readable
			  when( _jmxServer.getAttribute( _beanName, "name" ) ).thenThrow( new RuntimeMBeanException( new System.NotSupportedException( "Haha, screw discoverable services!" ) ) );

			  JmxQueryProcedure procedure = new JmxQueryProcedure( ProcedureSignature.procedureName( "bob" ), _jmxServer );

			  // when
			  RawIterator<object[], ProcedureException> result = procedure.Apply( null, new object[]{ "*:*" }, _resourceTracker );

			  // then
			  assertThat( asList( result ), contains( equalTo( new object[]{ "org.neo4j:chevyMakesTheTruck=bobMcCoshMakesTheDifference", "This is a description", map( _attributeName, map( "description", "This is the attribute desc.", "value", null ) ) } ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleCompositeAttributes() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleCompositeAttributes()
		 {
			  // given
			  ObjectName beanName = new ObjectName( "org.neo4j:chevyMakesTheTruck=bobMcCoshMakesTheDifference" );
			  when( _jmxServer.queryNames( new ObjectName( "*:*" ), null ) ).thenReturn( asSet( beanName ) );
			  when( _jmxServer.getMBeanInfo( beanName ) ).thenReturn( new MBeanInfo( "org.neo4j.SomeMBean", "This is a description", new MBeanAttributeInfo[]{ new MBeanAttributeInfo( "name", "differenceMaker", "Who makes the difference?", true, false, false ) }, null, null, null ) );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: when(jmxServer.getAttribute(beanName, "name")).thenReturn(new javax.management.openmbean.CompositeDataSupport(new javax.management.openmbean.CompositeType("myComposite", "Composite description", new String[]{"key1", "key2"}, new String[]{"Can't be empty", "Also can't be empty"}, new javax.management.openmbean.OpenType<?>[]{javax.management.openmbean.SimpleType.STRING, javax.management.openmbean.SimpleType.INTEGER}), map("key1", "Hello", "key2", 123)));
			  when( _jmxServer.getAttribute( beanName, "name" ) ).thenReturn( new CompositeDataSupport( new CompositeType( "myComposite", "Composite description", new string[]{ "key1", "key2" }, new string[]{ "Can't be empty", "Also can't be empty" }, new OpenType<object>[]{ SimpleType.STRING, SimpleType.INTEGER } ), map( "key1", "Hello", "key2", 123 ) ) );

			  JmxQueryProcedure procedure = new JmxQueryProcedure( ProcedureSignature.procedureName( "bob" ), _jmxServer );

			  // when
			  RawIterator<object[], ProcedureException> result = procedure.Apply( null, new object[]{ "*:*" }, _resourceTracker );

			  // then
			  assertThat( asList( result ), contains( equalTo( new object[]{ "org.neo4j:chevyMakesTheTruck=bobMcCoshMakesTheDifference", "This is a description", map( _attributeName, map( "description", "Who makes the difference?", "value", map( "description", "Composite description", "properties", map( "key1", "Hello", "key2", 123 ) ) ) ) } ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldConvertAllStandardBeansWithoutError() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldConvertAllStandardBeansWithoutError()
		 {
			  // given
			  MBeanServer jmxServer = ManagementFactory.PlatformMBeanServer;

			  JmxQueryProcedure procedure = new JmxQueryProcedure( ProcedureSignature.procedureName( "bob" ), jmxServer );

			  // when
			  RawIterator<object[], ProcedureException> result = procedure.Apply( null, new object[]{ "*:*" }, _resourceTracker );

			  // then we verify that we respond with the expected number of beans without error
			  //      .. we don't assert more than this, this is more of a smoke test to ensure
			  //      that independent of platform, we never throw exceptions even when converting every
			  //      single MBean into Neo4j types, and we always get the correct number of MBeans out.
			  assertThat( asList( result ).size(), equalTo(jmxServer.MBeanCount) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void Setup()
		 {
			  _jmxServer = mock( typeof( MBeanServer ) );
			  _beanName = new ObjectName( "org.neo4j:chevyMakesTheTruck=bobMcCoshMakesTheDifference" );
			  _attributeName = "name";

			  when( _jmxServer.queryNames( new ObjectName( "*:*" ), null ) ).thenReturn( asSet( _beanName ) );
			  when( _jmxServer.getMBeanInfo( _beanName ) ).thenReturn( new MBeanInfo( "org.neo4j.SomeMBean", "This is a description", new MBeanAttributeInfo[]{ new MBeanAttributeInfo( _attributeName, "someType", "This is the attribute desc.", true, false, false ) }, null, null, null ) );
		 }
	}

}