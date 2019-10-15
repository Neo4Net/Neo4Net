using System;
using System.Collections.Generic;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Procedure
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using TemporaryFolder = org.junit.rules.TemporaryFolder;


	using OnlineBackupSettings = Neo4Net.backup.OnlineBackupSettings;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Path = Neo4Net.Graphdb.Path;
	using QueryExecutionException = Neo4Net.Graphdb.QueryExecutionException;
	using QueryExecutionType = Neo4Net.Graphdb.QueryExecutionType;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Result = Neo4Net.Graphdb.Result;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using TransactionFailureException = Neo4Net.Graphdb.TransactionFailureException;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using AuthorizationViolationException = Neo4Net.Graphdb.security.AuthorizationViolationException;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using AnonymousContext = Neo4Net.Kernel.api.security.AnonymousContext;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using JarBuilder = Neo4Net.Kernel.impl.proc.JarBuilder;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using Log = Neo4Net.Logging.Log;
	using TestEnterpriseGraphDatabaseFactory = Neo4Net.Test.TestEnterpriseGraphDatabaseFactory;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsEqual.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsNull.nullValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.plugin_dir;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.procedure_unrestricted;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.logging.AssertableLogProvider.inLog;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.procedure.Mode.SCHEMA;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.procedure.Mode.WRITE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.procedure.StringMatcherIgnoresNewlines.containsStringIgnoreNewlines;

	public class ProcedureIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.TemporaryFolder plugins = new org.junit.rules.TemporaryFolder();
		 public TemporaryFolder Plugins = new TemporaryFolder();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

		 private GraphDatabaseService _db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  _exceptionsInProcedure.Clear();
			  ( new JarBuilder() ).createJarFor(Plugins.newFile("myProcedures.jar"), typeof(ClassWithProcedures));
			  ( new JarBuilder() ).createJarFor(Plugins.newFile("myFunctions.jar"), typeof(ClassWithFunctions));
			  _db = ( new TestEnterpriseGraphDatabaseFactory() ).newImpermanentDatabaseBuilder().setConfig(plugin_dir, Plugins.Root.AbsolutePath).setConfig(GraphDatabaseSettings.record_id_batch_size, "1").setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newGraphDatabase();
			  OnCloseCalled = new bool[2];
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @After public void tearDown()
		 public virtual void TearDown()
		 {
			  if ( this._db != null )
			  {
					this._db.shutdown();
			  }
		 }

		 public static bool[] OnCloseCalled;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallProcedureWithParameterMap()
		 public virtual void ShouldCallProcedureWithParameterMap()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "CALL org.neo4j.procedure.simpleArgument", map( "name", 42L ) );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.Next(), equalTo(map("someVal", 42L)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallProcedureWithDefaultArgument()
		 public virtual void ShouldCallProcedureWithDefaultArgument()
		 {
			  //Given/When
			  Result res = _db.execute( "CALL org.neo4j.procedure.simpleArgumentWithDefault" );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( res.Next(), equalTo(map("someVal", 42L)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( res.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallYieldProcedureWithDefaultArgument()
		 public virtual void ShouldCallYieldProcedureWithDefaultArgument()
		 {
			  // Given/When
			  Result res = _db.execute( "CALL org.neo4j.procedure.simpleArgumentWithDefault() YIELD someVal as n RETURN n + 1295 as val" );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( res.Next(), equalTo(map("val", 1337L)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( res.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallProcedureWithAllDefaultArgument()
		 public virtual void ShouldCallProcedureWithAllDefaultArgument()
		 {
			  //Given/When
			  Result res = _db.execute( "CALL org.neo4j.procedure.defaultValues" );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( res.Next(), equalTo(map("string", "a string", "integer", 42L, "aFloat", 3.14, "aBoolean", true)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( res.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallProcedureWithOneProvidedRestDefaultArgument()
		 public virtual void ShouldCallProcedureWithOneProvidedRestDefaultArgument()
		 {
			  //Given/When
			  Result res = _db.execute( "CALL org.neo4j.procedure.defaultValues('another string')" );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( res.Next(), equalTo(map("string", "another string", "integer", 42L, "aFloat", 3.14, "aBoolean", true)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( res.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallProcedureWithTwoProvidedRestDefaultArgument()
		 public virtual void ShouldCallProcedureWithTwoProvidedRestDefaultArgument()
		 {
			  //Given/When
			  Result res = _db.execute( "CALL org.neo4j.procedure.defaultValues('another string', 1337)" );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( res.Next(), equalTo(map("string", "another string", "integer", 1337L, "aFloat", 3.14, "aBoolean", true)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( res.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallProcedureWithThreeProvidedRestDefaultArgument()
		 public virtual void ShouldCallProcedureWithThreeProvidedRestDefaultArgument()
		 {
			  //Given/When
			  Result res = _db.execute( "CALL org.neo4j.procedure.defaultValues('another string', 1337, 2.718281828)" );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( res.Next(), equalTo(map("string", "another string", "integer", 1337L, "aFloat", 2.718281828, "aBoolean", true)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( res.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallProcedureWithFourProvidedRestDefaultArgument()
		 public virtual void ShouldCallProcedureWithFourProvidedRestDefaultArgument()
		 {
			  //Given/When
			  Result res = _db.execute( "CALL org.neo4j.procedure.defaultValues('another string', 1337, 2.718281828, false)" );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( res.Next(), equalTo(map("string", "another string", "integer", 1337L, "aFloat", 2.718281828, "aBoolean", false)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( res.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveNiceErrorMessageOnWrongStaticType()
		 public virtual void ShouldGiveNiceErrorMessageOnWrongStaticType()
		 {
			  //Expect
			  Exception.expect( typeof( QueryExecutionException ) );
			  Exception.expectMessage( "Type mismatch: expected Integer but was String (line 1, column 41 (offset: 40))" );

			  // When
			  using ( Transaction ignore = _db.beginTx() )
			  {
					//Make sure argument here is not auto parameterized away as that will drop all type information on the floor
					_db.execute( "CALL org.neo4j.procedure.simpleArgument('42')" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveNiceErrorMessageWhenNoArguments()
		 public virtual void ShouldGiveNiceErrorMessageWhenNoArguments()
		 {
			  //Expect
			  Exception.expect( typeof( QueryExecutionException ) );
			  Exception.expectMessage( containsStringIgnoreNewlines( string.Format( "Procedure call does not provide the required number of arguments: got 0 expected 1.%n%n" + "Procedure org.neo4j.procedure.simpleArgument has signature: " + "org.neo4j.procedure.simpleArgument(name :: INTEGER?) :: someVal :: INTEGER?%n" + "meaning that it expects 1 argument of type INTEGER?" ) ) );
			  // When
			  using ( Transaction ignore = _db.beginTx() )
			  {
					_db.execute( "CALL org.neo4j.procedure.simpleArgument()" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveNiceErrorWhenMissingArgumentsToVoidFunction()
		 public virtual void ShouldGiveNiceErrorWhenMissingArgumentsToVoidFunction()
		 {
			  //Expect
			  Exception.expect( typeof( QueryExecutionException ) );
			  Exception.expectMessage( containsStringIgnoreNewlines( string.Format( "Procedure call does not provide the required number of arguments: got 1 expected 3.%n%n" + "Procedure org.neo4j.procedure.sideEffectWithDefault has signature: org.neo4j.procedure" + ".sideEffectWithDefault(label :: STRING?, propertyKey :: STRING?, value  =  Zhang Wei :: STRING?) :: VOID%n" + "meaning that it expects 3 arguments of type STRING?, STRING?, STRING? (line 1, column 1 (offset: 0))" ) ) );
			  // When
			  using ( Transaction ignore = _db.beginTx() )
			  {
					_db.execute( "CALL org.neo4j.procedure.sideEffectWithDefault()" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldShowDescriptionWhenMissingArguments()
		 public virtual void ShouldShowDescriptionWhenMissingArguments()
		 {
			  //Expect
			  Exception.expect( typeof( QueryExecutionException ) );
			  Exception.expectMessage( containsStringIgnoreNewlines( string.Format( "Procedure call does not provide the required number of arguments: got 0 expected 1.%n%n" + "Procedure org.neo4j.procedure.nodeWithDescription has signature: " + "org.neo4j.procedure.nodeWithDescription(node :: NODE?) :: node :: NODE?%n" + "meaning that it expects 1 argument of type NODE?%n" + "Description: This is a description (line 1, column 1 (offset: 0))" ) ) );
			  // When
			  using ( Transaction ignore = _db.beginTx() )
			  {
					_db.execute( "CALL org.neo4j.procedure.nodeWithDescription()" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallDelegatingProcedure()
		 public virtual void ShouldCallDelegatingProcedure()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "CALL org.neo4j.procedure.delegatingProcedure", map( "name", 43L ) );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.Next(), equalTo(map("someVal", 43L)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallRecursiveProcedure()
		 public virtual void ShouldCallRecursiveProcedure()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "CALL org.neo4j.procedure.recursiveSum", map( "order", 10L ) );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.Next(), equalTo(map("someVal", 55L)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallProcedureWithGenericArgument()
		 public virtual void ShouldCallProcedureWithGenericArgument()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "CALL org.neo4j.procedure.genericArguments([ ['graphs'], ['are'], ['everywhere']], " + "[ [[1, 2, 3]], [[4, 5]]] )" );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.Next(), equalTo(map("someVal", 5L)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallProcedureWithMapArgument()
		 public virtual void ShouldCallProcedureWithMapArgument()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "CALL org.neo4j.procedure.mapArgument({foo: 42, bar: 'hello'})" );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.Next(), equalTo(map("someVal", 2L)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallProcedureWithMapArgumentDefaultingToNull()
		 public virtual void ShouldCallProcedureWithMapArgumentDefaultingToNull()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "CALL org.neo4j.procedure.mapWithNullDefault()" );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.Next(), equalTo(map("map", null)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallProcedureWithMapArgumentDefaultingToMap()
		 public virtual void ShouldCallProcedureWithMapArgumentDefaultingToMap()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "CALL org.neo4j.procedure.mapWithOtherDefault" );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.Next(), equalTo(map("map", map("default", true))) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallProcedureWithListWithDefault()
		 public virtual void ShouldCallProcedureWithListWithDefault()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "CALL org.neo4j.procedure.listWithDefault" );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.Next(), equalTo(map("list", asList(42L, 1337L))) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallProcedureWithGenericListWithDefault()
		 public virtual void ShouldCallProcedureWithGenericListWithDefault()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "CALL org.neo4j.procedure.genericListWithDefault" );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.Next(), equalTo(map("list", asList(42L, 1337L))) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallProcedureWithByteArrayWithParameter() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCallProcedureWithByteArrayWithParameter()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "CALL org.neo4j.procedure.incrBytes($param)", map( "param", new sbyte[]{ 4, 5, 6 } ) );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.ColumnAs( "bytes" ).next(), equalTo(new sbyte[]{ 5, 6, 7 }) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallProcedureWithByteArrayWithParameterAndYield() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCallProcedureWithByteArrayWithParameterAndYield()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "WITH $param AS b CALL org.neo4j.procedure.incrBytes(b) YIELD bytes RETURN bytes", map( "param", new sbyte[]{ 7, 8, 9 } ) );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.ColumnAs( "bytes" ).next(), equalTo(new sbyte[]{ 8, 9, 10 }) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallProcedureWithByteArrayWithParameterAndYieldAndParameterReuse() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCallProcedureWithByteArrayWithParameterAndYieldAndParameterReuse()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "WITH $param AS param CALL org.neo4j.procedure.incrBytes(param) YIELD bytes RETURN bytes, param", map( "param", new sbyte[]{ 10, 11, 12 } ) );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( res.HasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					IDictionary<string, object> results = res.Next();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
					assertThat( results["bytes"], equalTo( new sbyte[]{ 11, 12, 13 } ) );
					assertThat( results["param"], equalTo( new sbyte[]{ 10, 11, 12 } ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleCallWithCypherLiteralInByteArrayProcedure() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotBeAbleCallWithCypherLiteralInByteArrayProcedure()
		 {
			  //Expect
			  Exception.expect( typeof( QueryExecutionException ) );
			  Exception.expectMessage( "Cannot convert 1 to byte for input to procedure" );

			  // When
			  using ( Transaction ignore = _db.beginTx() )
			  {
					Result result = _db.execute( "CALL org.neo4j.procedure.incrBytes([1,2,3])" );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					result.Next();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallProcedureListWithNull() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCallProcedureListWithNull()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "CALL org.neo4j.procedure.genericListWithDefault(null)" );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.Next(), equalTo(map("list", null)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallProcedureListWithNullInList()
		 public virtual void ShouldCallProcedureListWithNullInList()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "CALL org.neo4j.procedure.genericListWithDefault([[42, null, 57]])" );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.Next(), equalTo(map("list", asList(42L, null, 57L))) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallProcedureWithNodeReturn()
		 public virtual void ShouldCallProcedureWithNodeReturn()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					long nodeId = _db.createNode().Id;

					// When
					Result res = _db.execute( "CALL org.neo4j.procedure.node({id})", map( "id", nodeId ) );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					Node node = ( Node ) res.Next()["node"];
					assertThat( node.Id, equalTo( nodeId ) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallProcedureReturningNull()
		 public virtual void ShouldCallProcedureReturningNull()
		 {
			  Result res = _db.execute( "CALL org.neo4j.procedure.node(-1)" );

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( res.Next()["node"], nullValue() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( res.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallYieldProcedureReturningNull()
		 public virtual void ShouldCallYieldProcedureReturningNull()
		 {
			  Result res = _db.execute( "CALL org.neo4j.procedure.node(-1) YIELD node as node RETURN node" );

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( res.Next()["node"], nullValue() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( res.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveHelpfulErrorOnMissingProcedure()
		 public virtual void ShouldGiveHelpfulErrorOnMissingProcedure()
		 {
			  // Expect
			  Exception.expect( typeof( QueryExecutionException ) );
			  Exception.expectMessage( "There is no procedure with the name `someProcedureThatDoesNotExist` " + "registered for this database instance. Please ensure you've spelled the " + "procedure name correctly and that the procedure is properly deployed." );

			  // When
			  _db.execute( "CALL someProcedureThatDoesNotExist" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveHelpfulErrorOnExceptionMidStream()
		 public virtual void ShouldGiveHelpfulErrorOnExceptionMidStream()
		 {
			  // Given
			  // run in tx to avoid having to wait for tx rollback on shutdown
			  using ( Transaction ignore = _db.beginTx() )
			  {
					Result result = _db.execute( "CALL org.neo4j.procedure.throwsExceptionInStream" );

					// Expect
					Exception.expect( typeof( QueryExecutionException ) );
					Exception.expectMessage( "Failed to invoke procedure `org.neo4j.procedure.throwsExceptionInStream`: " + "Caused by: java.lang.RuntimeException: Kaboom" );

					// When
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					result.Next();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldShowCauseOfError()
		 public virtual void ShouldShowCauseOfError()
		 {
			  // Given
			  // run in tx to avoid having to wait for tx rollback on shutdown
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// Expect
					Exception.expect( typeof( QueryExecutionException ) );
					Exception.expectMessage( "Failed to invoke procedure `org.neo4j.procedure.indexOutOfBounds`: Caused by: java.lang" + ".ArrayIndexOutOfBoundsException" );
					// When
					_db.execute( "CALL org.neo4j.procedure.indexOutOfBounds" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallProcedureWithAccessToDB()
		 public virtual void ShouldCallProcedureWithAccessToDB()
		 {
			  // When
			  using ( Transaction ignore = _db.beginTx() )
			  {
					_db.createNode( label( "Person" ) ).setProperty( "name", "Buddy Holly" );
			  }

			  // Then
			  using ( Transaction ignore = _db.beginTx() )
			  {
					Result res = _db.execute( "CALL org.neo4j.procedure.listCoolPeopleInDatabase" );

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogLikeThereIsNoTomorrow()
		 public virtual void ShouldLogLikeThereIsNoTomorrow()
		 {
			  // Given
			  AssertableLogProvider logProvider = new AssertableLogProvider();

			  _db.shutdown();
			  _db = ( new TestGraphDatabaseFactory() ).setInternalLogProvider(logProvider).setUserLogProvider(logProvider).newImpermanentDatabaseBuilder().setConfig(plugin_dir, Plugins.Root.AbsolutePath).setConfig(procedure_unrestricted, "org.neo4j.procedure.*").setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newGraphDatabase();

			  // When
			  using ( Transaction ignore = _db.beginTx() )
			  {
					Result res = _db.execute( "CALL org.neo4j.procedure.logAround()" );
					while ( res.MoveNext() )
					{
						res.Current;
					}
			  }

			  // Then
			  AssertableLogProvider.LogMatcherBuilder match = inLog( typeof( Procedures ) );
			  logProvider.AssertAtLeastOnce( match.Debug( "1" ), match.Info( "2" ), match.Warn( "3" ), match.Error( "4" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDenyReadOnlyProcedureToPerformWrites()
		 public virtual void ShouldDenyReadOnlyProcedureToPerformWrites()
		 {
			  // Expect
			  Exception.expect( typeof( QueryExecutionException ) );
			  Exception.expectMessage( "Write operations are not allowed" );

			  // When
			  using ( Transaction ignore = _db.beginTx() )
			  {
					_db.execute( "CALL org.neo4j.procedure.readOnlyTryingToWrite()" ).next();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowWriteProcedureToPerformWrites()
		 public virtual void ShouldAllowWriteProcedureToPerformWrites()
		 {
			  // When
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( "CALL org.neo4j.procedure.writingProcedure()" ).close();
					tx.Success();
			  }

			  // Then
			  using ( Transaction tx = _db.beginTx() )
			  {
					assertEquals( 1, _db.AllNodes.Count() );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readProceduresShouldPresentThemSelvesAsReadQueries()
		 public virtual void ReadProceduresShouldPresentThemSelvesAsReadQueries()
		 {
			  // When
			  using ( Transaction tx = _db.beginTx() )
			  {
					Result result = _db.execute( "EXPLAIN CALL org.neo4j.procedure.integrationTestMe()" );
					assertEquals( result.QueryExecutionType.queryType(), QueryExecutionType.QueryType.READ_ONLY );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void readProceduresWithYieldShouldPresentThemSelvesAsReadQueries()
		 public virtual void ReadProceduresWithYieldShouldPresentThemSelvesAsReadQueries()
		 {
			  // When
			  using ( Transaction tx = _db.beginTx() )
			  {
					Result result = _db.execute( "EXPLAIN CALL org.neo4j.procedure.integrationTestMe() YIELD someVal as v RETURN v" );
					assertEquals( result.QueryExecutionType.queryType(), QueryExecutionType.QueryType.READ_ONLY );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void writeProceduresShouldPresentThemSelvesAsWriteQueries()
		 public virtual void WriteProceduresShouldPresentThemSelvesAsWriteQueries()
		 {
			  // When
			  using ( Transaction tx = _db.beginTx() )
			  {
					Result result = _db.execute( "EXPLAIN CALL org.neo4j.procedure.createNode('n')" );
					assertEquals( result.QueryExecutionType.queryType(), QueryExecutionType.QueryType.READ_WRITE );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void writeProceduresWithYieldShouldPresentThemSelvesAsWriteQueries()
		 public virtual void WriteProceduresWithYieldShouldPresentThemSelvesAsWriteQueries()
		 {
			  // When
			  using ( Transaction tx = _db.beginTx() )
			  {
					Result result = _db.execute( "EXPLAIN CALL org.neo4j.procedure.createNode('n') YIELD node as n RETURN n.prop" );
					assertEquals( result.QueryExecutionType.queryType(), QueryExecutionType.QueryType.READ_WRITE );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToCallWriteProcedureThroughReadProcedure()
		 public virtual void ShouldNotBeAbleToCallWriteProcedureThroughReadProcedure()
		 {
			  // Expect
			  Exception.expect( typeof( QueryExecutionException ) );
			  Exception.expectMessage( "Write operations are not allowed" );

			  // When
			  using ( Transaction ignore = _db.beginTx() )
			  {
					_db.execute( "CALL org.neo4j.procedure.readOnlyCallingWriteProcedure" ).next();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToCallReadProcedureThroughWriteProcedureInWriteOnlyTransaction()
		 public virtual void ShouldNotBeAbleToCallReadProcedureThroughWriteProcedureInWriteOnlyTransaction()
		 {
			  // Expect
			  Exception.expect( typeof( QueryExecutionException ) );
			  Exception.expectMessage( "Read operations are not allowed" );

			  GraphDatabaseAPI gdapi = ( GraphDatabaseAPI ) _db;

			  // When
			  using ( Transaction tx = gdapi.BeginTransaction( KernelTransaction.Type.@explicit, AnonymousContext.writeOnly() ) )
			  {
					_db.execute( "CALL org.neo4j.procedure.writeProcedureCallingReadProcedure" ).next();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToCallWriteProcedureThroughWriteProcedure()
		 public virtual void ShouldBeAbleToCallWriteProcedureThroughWriteProcedure()
		 {
			  // When
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.execute( "CALL org.neo4j.procedure.writeProcedureCallingWriteProcedure()" ).close();
					tx.Success();
			  }

			  // Then
			  using ( Transaction tx = _db.beginTx() )
			  {
					assertEquals( 1, _db.AllNodes.Count() );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToCallSchemaProcedureThroughWriteProcedureInWriteTransaction()
		 public virtual void ShouldNotBeAbleToCallSchemaProcedureThroughWriteProcedureInWriteTransaction()
		 {
			  // Expect
			  Exception.expect( typeof( QueryExecutionException ) );
			  Exception.expectMessage( "Schema operations are not allowed" );

			  GraphDatabaseAPI gdapi = ( GraphDatabaseAPI ) _db;

			  // When
			  using ( Transaction tx = gdapi.BeginTransaction( KernelTransaction.Type.@explicit, AnonymousContext.write() ) )
			  {
					_db.execute( "CALL org.neo4j.procedure.writeProcedureCallingSchemaProcedure" ).next();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDenyReadOnlyProcedureToPerformSchema()
		 public virtual void ShouldDenyReadOnlyProcedureToPerformSchema()
		 {
			  // Expect
			  Exception.expect( typeof( QueryExecutionException ) );
			  Exception.expectMessage( "Schema operations are not allowed" );

			  // Give
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					_db.execute( "CALL org.neo4j.procedure.readOnlyTryingToWriteSchema" ).next();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDenyReadWriteProcedureToPerformSchema()
		 public virtual void ShouldDenyReadWriteProcedureToPerformSchema()
		 {
			  // Expect
			  Exception.expect( typeof( QueryExecutionException ) );
			  Exception.expectMessage( "Schema operations are not allowed for AUTH_DISABLED with FULL restricted to TOKEN_WRITE." );

			  // Give
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					_db.execute( "CALL org.neo4j.procedure.readWriteTryingToWriteSchema" ).next();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowSchemaProcedureToPerformSchema()
		 public virtual void ShouldAllowSchemaProcedureToPerformSchema()
		 {
			  // Give
			  using ( Transaction tx = _db.beginTx() )
			  {
					// When
					_db.execute( "CALL org.neo4j.procedure.schemaProcedure" );
					tx.Success();
			  }

			  // Then
			  using ( Transaction tx = _db.beginTx() )
			  {
					assertTrue( _db.schema().Constraints.GetEnumerator().hasNext() );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowSchemaCallReadOnly()
		 public virtual void ShouldAllowSchemaCallReadOnly()
		 {
			  // Given
			  long nodeId;
			  using ( Transaction tx = _db.beginTx() )
			  {
					nodeId = _db.createNode().Id;
					tx.Success();
			  }

			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "CALL org.neo4j.procedure.schemaCallReadProcedure({id})", map( "id", nodeId ) );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					Node node = ( Node ) res.Next()["node"];
					assertThat( node.Id, equalTo( nodeId ) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDenySchemaProcedureToPerformWrite()
		 public virtual void ShouldDenySchemaProcedureToPerformWrite()
		 {
			  // Expect
			  Exception.expect( typeof( QueryExecutionException ) );
			  Exception.expectMessage( "Cannot perform data updates in a transaction that has performed schema updates" );

			  // Give
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					_db.execute( "CALL org.neo4j.procedure.schemaTryingToWrite" ).next();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCoerceLongToDoubleAtRuntimeWhenCallingProcedure()
		 public virtual void ShouldCoerceLongToDoubleAtRuntimeWhenCallingProcedure()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "CALL org.neo4j.procedure.squareDouble", map( "value", 4L ) );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.Next(), equalTo(map("result", 16.0d)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCoerceListOfNumbersToDoublesAtRuntimeWhenCallingProcedure()
		 public virtual void ShouldCoerceListOfNumbersToDoublesAtRuntimeWhenCallingProcedure()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "CALL org.neo4j.procedure.avgNumberList({param})", map( "param", Arrays.asList<Number>( 1L, 2L, 3L ) ) );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.Next(), equalTo(map("result", 2.0d)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCoerceListOfMixedNumbers()
		 public virtual void ShouldCoerceListOfMixedNumbers()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "CALL org.neo4j.procedure.avgDoubleList([{long}, {double}])", map( "long", 1L, "double", 2.0d ) );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.Next(), equalTo(map("result", 1.5d)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCoerceDoubleToLongAtRuntimeWhenCallingProcedure()
		 public virtual void ShouldCoerceDoubleToLongAtRuntimeWhenCallingProcedure()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "CALL org.neo4j.procedure.squareLong", map( "value", 4L ) );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.Next(), equalTo(map("someVal", 16L)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToCallVoidProcedure()
		 public virtual void ShouldBeAbleToCallVoidProcedure()
		 {
			  using ( Transaction ignore = _db.beginTx() )
			  {
					_db.execute( "CALL org.neo4j.procedure.sideEffect('PONTUS')" );

					assertThat( _db.execute( "MATCH (n:PONTUS) RETURN count(n) AS c" ).next().get("c"), equalTo(1L) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToCallVoidProcedureWithDefaultValue()
		 public virtual void ShouldBeAbleToCallVoidProcedureWithDefaultValue()
		 {
			  using ( Transaction ignore = _db.beginTx() )
			  {
					_db.execute( "CALL org.neo4j.procedure.sideEffectWithDefault('Person','name')" );
					Result result = _db.execute( "MATCH (n:Person) RETURN n.name AS name" );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( result.Next()["name"], equalTo("Zhang Wei") );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( result.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToCallDelegatingVoidProcedure()
		 public virtual void ShouldBeAbleToCallDelegatingVoidProcedure()
		 {
			  using ( Transaction ignore = _db.beginTx() )
			  {
					_db.execute( "CALL org.neo4j.procedure.delegatingSideEffect('SUTNOP')" );

					assertThat( _db.execute( "MATCH (n:SUTNOP) RETURN count(n) AS c" ).next().get("c"), equalTo(1L) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToPerformWritesOnNodesReturnedFromReadOnlyProcedure()
		 public virtual void ShouldBeAbleToPerformWritesOnNodesReturnedFromReadOnlyProcedure()
		 {
			  // When
			  using ( Transaction tx = _db.beginTx() )
			  {
					long nodeId = _db.createNode().Id;
					Node node = Iterators.single( _db.execute( "CALL org.neo4j.procedure.node", map( "id", nodeId ) ).columnAs( "node" ) );
					node.SetProperty( "name", "Stefan" );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToShutdown()
		 public virtual void ShouldFailToShutdown()
		 {
			  // Expect
			  Exception.expect( typeof( QueryExecutionException ) );
			  Exception.expectMessage( "Failed to invoke procedure `org.neo4j.procedure.shutdown`: Caused by: java.lang" + ".UnsupportedOperationException" );

			  using ( Transaction ignore = _db.beginTx() )
			  {
					_db.execute( "CALL org.neo4j.procedure.shutdown()" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToWriteAfterCallingReadOnlyProcedure()
		 public virtual void ShouldBeAbleToWriteAfterCallingReadOnlyProcedure()
		 {
			  using ( Transaction ignore = _db.beginTx() )
			  {
					_db.execute( "CALL org.neo4j.procedure.simpleArgument(12)" ).close();
					_db.createNode();
			  }
		 }

		 private static IList<Exception> _exceptionsInProcedure = Collections.synchronizedList( new List<Exception>() );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToSpawnThreadsCreatingTransactionInProcedures() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToSpawnThreadsCreatingTransactionInProcedures()
		 {
			  // given
			  ThreadStart doIt = () =>
			  {
				Result result = _db.execute( "CALL org.neo4j.procedure.supportedProcedure()" );
				while ( result.hasNext() )
				{
					 result.next();
				}
				result.close();
			  };

			  int numThreads = 10;
			  Thread[] threads = new Thread[numThreads];
			  for ( int i = 0; i < numThreads; i++ )
			  {
					threads[i] = new Thread( doIt );
			  }

			  // when
			  for ( int i = 0; i < numThreads; i++ )
			  {
					threads[i].Start();
			  }

			  for ( int i = 0; i < numThreads; i++ )
			  {
					threads[i].Join();
			  }

			  Result result = _db.execute( "MATCH () RETURN count(*) as n" );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( result.HasNext(), equalTo(true) );
			  while ( result.MoveNext() )
			  {
					assertThat( result.Current.get( "n" ), equalTo( ( long ) numThreads ) );
			  }
			  result.Close();
			  assertThat( "Should be no exceptions in procedures", _exceptionsInProcedure.Count == 0, equalTo( true ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToUseCallYieldWithPeriodicCommit() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToUseCallYieldWithPeriodicCommit()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  string[] lines = IntStream.rangeClosed( 1, 100 ).boxed().map(i => Convert.ToString(i)).toArray(string[]::new);
			  string url = CreateCsvFile( lines );

			  //WHEN
			  Result result = _db.execute( "USING PERIODIC COMMIT 1 " + "LOAD CSV FROM '" + url + "' AS line " + "CALL org.neo4j.procedure.createNode(line[0]) YIELD node as n " + "RETURN n.prop" );
			  // THEN
			  for ( int i = 1; i <= 100; i++ )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( result.Next()["n.prop"], equalTo(Convert.ToString(i)) );
			  }
			  result.Close();

			  //Make sure all the lines has been properly commited to the database.
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  string[] dbContents = _db.execute( "MATCH (n) return n.prop" ).Select( m => ( string ) m.get( "n.prop" ) ).ToArray( string[]::new );
			  assertThat( dbContents, equalTo( lines ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfUsingPeriodicCommitWithReadOnlyQuery() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailIfUsingPeriodicCommitWithReadOnlyQuery()
		 {
			  // GIVEN
			  string url = CreateCsvFile( "13" );

			  // EXPECT
			  Exception.expect( typeof( QueryExecutionException ) );
			  Exception.expectMessage( "Cannot use periodic commit in a non-updating query (line 1, column 1 (offset: 0))" );

			  //WHEN
			  _db.execute( "USING PERIODIC COMMIT 1 " + "LOAD CSV FROM '" + url + "' AS line " + "CALL org.neo4j.procedure.simpleArgument(ToInt(line[0])) YIELD someVal as val " + "RETURN val" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToUseCallYieldWithLoadCsvAndSet() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToUseCallYieldWithLoadCsvAndSet()
		 {
			  // GIVEN
			  string url = CreateCsvFile( "foo" );

			  //WHEN
			  Result result = _db.execute( "LOAD CSV FROM '" + url + "' AS line " + "CALL org.neo4j.procedure.createNode(line[0]) YIELD node as n " + "SET n.p = 42 " + "RETURN n.p" );
			  // THEN
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( result.Next()["n.p"], equalTo(42L) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallProcedureReturningPaths()
		 public virtual void ShouldCallProcedureReturningPaths()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					Node node1 = _db.createNode();
					Node node2 = _db.createNode();
					Relationship rel = node1.CreateRelationshipTo( node2, RelationshipType.withName( "KNOWS" ) );

					// When
					Result res = _db.execute( "CALL org.neo4j.procedure.nodePaths({node}) YIELD path RETURN path", map( "node", node1 ) );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( res.HasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					IDictionary<string, object> value = res.Next();
					Path path = ( Path ) value["path"];
					assertThat( path.Length(), equalTo(1) );
					assertThat( path.StartNode(), equalTo(node1) );
					assertThat( asList( path.Relationships() ), equalTo(singletonList(rel)) );
					assertThat( path.EndNode(), equalTo(node2) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallStreamCloseWhenResultExhausted()
		 public virtual void ShouldCallStreamCloseWhenResultExhausted()
		 {
			  string query = "CALL org.neo4j.procedure.onCloseProcedure(0)";

			  Result res = _db.execute( query );

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( res.HasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  res.Next();

			  assertFalse( OnCloseCalled[0] );

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( res.HasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  res.Next();

			  assertTrue( OnCloseCalled[0] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallStreamCloseWhenResultFiltered()
		 public virtual void ShouldCallStreamCloseWhenResultFiltered()
		 {
			  // This query should return zero rows
			  string query = "CALL org.neo4j.procedure.onCloseProcedure(1) YIELD someVal WITH someVal WHERE someVal = 1337 RETURN someVal";

			  Result res = _db.execute( query );

			  assertFalse( OnCloseCalled[1] );

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( res.HasNext() );

			  assertTrue( OnCloseCalled[1] );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String createCsvFile(String... lines) throws java.io.IOException
		 private string CreateCsvFile( params string[] lines )
		 {
			  File file = Plugins.newFile();

			  using ( PrintWriter writer = FileUtils.newFilePrintWriter( file, StandardCharsets.UTF_8 ) )
			  {
					foreach ( string line in lines )
					{
						 writer.println( line );
					}
			  }

			  return file.toURI().toURL().ToString();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnNodeListTypedAsNodeList()
		 public virtual void ShouldReturnNodeListTypedAsNodeList()
		 {
			  // When
			  Result res = _db.execute( "CALL org.neo4j.procedure.nodeList() YIELD nodes RETURN extract( x IN nodes | id(x) ) as ids" );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( res.HasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: assertThat(((java.util.List<?>) res.next().get("ids")).size(), equalTo(2));
			  assertThat( ( ( IList<object> ) res.Next()["ids"] ).Count, equalTo(2) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( res.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveNiceErrorMessageWhenAggregationFunctionInProcedureCall()
		 public virtual void ShouldGiveNiceErrorMessageWhenAggregationFunctionInProcedureCall()
		 {
			  using ( Transaction ignore = _db.beginTx() )
			  {
					_db.createNode( Label.label( "Person" ) );
					_db.createNode( Label.label( "Person" ) );

					// Expect
					Exception.expect( typeof( QueryExecutionException ) );

					// When
					_db.execute( "MATCH (n:Person) CALL org.neo4j.procedure.nodeListArgument(collect(n)) YIELD someVal RETURN someVal" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWorkWhenUsingWithToProjectList()
		 public virtual void ShouldWorkWhenUsingWithToProjectList()
		 {
			  using ( Transaction ignore = _db.beginTx() )
			  {
					_db.createNode( Label.label( "Person" ) );
					_db.createNode( Label.label( "Person" ) );

					// When
					Result res = _db.execute( "MATCH (n:Person) WITH collect(n) as persons " + "CALL org.neo4j.procedure.nodeListArgument(persons) YIELD someVal RETURN someVal" );

					// THEN
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.Next()["someVal"], equalTo(2L) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowReadProcedureInNoneTransaction()
		 public virtual void ShouldNotAllowReadProcedureInNoneTransaction()
		 {
			  // Expect
			  Exception.expect( typeof( AuthorizationViolationException ) );
			  Exception.expectMessage( "Read operations are not allowed" );

			  GraphDatabaseAPI gdapi = ( GraphDatabaseAPI ) _db;

			  // When
			  using ( Transaction tx = gdapi.BeginTransaction( KernelTransaction.Type.@explicit, AnonymousContext.none() ) )
			  {
					_db.execute( "CALL org.neo4j.procedure.integrationTestMe()" );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowWriteProcedureInReadOnlyTransaction()
		 public virtual void ShouldNotAllowWriteProcedureInReadOnlyTransaction()
		 {
			  // Expect
			  Exception.expect( typeof( AuthorizationViolationException ) );
			  Exception.expectMessage( "Write operations are not allowed" );

			  GraphDatabaseAPI gdapi = ( GraphDatabaseAPI ) _db;

			  // When
			  using ( Transaction tx = gdapi.BeginTransaction( KernelTransaction.Type.@explicit, AnonymousContext.read() ) )
			  {
					_db.execute( "CALL org.neo4j.procedure.writingProcedure()" );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowSchemaWriteProcedureInWriteTransaction()
		 public virtual void ShouldNotAllowSchemaWriteProcedureInWriteTransaction()
		 {
			  // Expect
			  Exception.expect( typeof( AuthorizationViolationException ) );
			  Exception.expectMessage( "Schema operations are not allowed" );

			  GraphDatabaseAPI gdapi = ( GraphDatabaseAPI ) _db;

			  // When
			  using ( Transaction tx = gdapi.BeginTransaction( KernelTransaction.Type.@explicit, AnonymousContext.write() ) )
			  {
					_db.execute( "CALL org.neo4j.procedure.schemaProcedure()" );
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallProcedureWithDefaultNodeArgument()
		 public virtual void ShouldCallProcedureWithDefaultNodeArgument()
		 {
			  //Given/When
			  Result res = _db.execute( "CALL org.neo4j.procedure.nodeWithDefault" );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( res.Next(), equalTo(map("node", null)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( res.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIndicateDefaultValueWhenListingProcedures()
		 public virtual void ShouldIndicateDefaultValueWhenListingProcedures()
		 {
			  // Given/When
			  IList<IDictionary<string, object>> results = _db.execute( "CALL dbms.procedures()" ).Where( record => record.get( "name" ).Equals( "org.neo4j.procedure.nodeWithDefault" ) ).ToList();
			  // Then
			  assertFalse( "Expected to find test procedure", results.Count == 0 );
			  assertThat( results[0]["signature"], equalTo( "org.neo4j.procedure.nodeWithDefault(node = null :: NODE?) :: (node :: NODE?)" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldShowDescriptionWhenListingProcedures()
		 public virtual void ShouldShowDescriptionWhenListingProcedures()
		 {
			  // Given/When
			  IList<IDictionary<string, object>> results = _db.execute( "CALL dbms.procedures()" ).Where( record => record.get( "name" ).Equals( "org.neo4j.procedure.nodeWithDescription" ) ).ToList();
			  // Then
			  assertFalse( "Expected to find test procedure", results.Count == 0 );
			  assertThat( results[0]["description"], equalTo( "This is a description" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldShowModeWhenListingProcedures()
		 public virtual void ShouldShowModeWhenListingProcedures()
		 {
			  // Given/When
			  IList<IDictionary<string, object>> results = _db.execute( "CALL dbms.procedures()" ).Where( record => record.get( "name" ).Equals( "org.neo4j.procedure.nodeWithDescription" ) ).ToList();
			  // Then
			  assertFalse( "Expected to find test procedure", results.Count == 0 );
			  assertThat( results[0]["mode"], equalTo( "WRITE" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldIndicateDefaultValueWhenListingFunctions()
		 public virtual void ShouldIndicateDefaultValueWhenListingFunctions()
		 {
			  // Given/When
			  IList<IDictionary<string, object>> results = _db.execute( "CALL dbms.functions()" ).Where( record => record.get( "name" ).Equals( "org.neo4j.procedure.getNodeName" ) ).ToList();
			  // Then
			  assertFalse( "Expected to find test function", results.Count == 0 );
			  assertThat( results[0]["signature"], equalTo( "org.neo4j.procedure.getNodeName(node = null :: NODE?) :: (STRING?)" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldShowDescriptionWhenListingFunctions()
		 public virtual void ShouldShowDescriptionWhenListingFunctions()
		 {
			  // Given/When
			  IList<IDictionary<string, object>> results = _db.execute( "CALL dbms.functions()" ).Where( record => record.get( "name" ).Equals( "org.neo4j.procedure.functionWithDescription" ) ).ToList();
			  // Then
			  assertFalse( "Expected to find test function", results.Count == 0 );
			  assertThat( results[0]["description"], equalTo( "This is a description" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallFunctionWithByteArrayWithParameter() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCallFunctionWithByteArrayWithParameter()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "RETURN org.neo4j.procedure.decrBytes($param) AS bytes", map( "param", new sbyte[]{ 4, 5, 6 } ) );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.ColumnAs( "bytes" ).next(), equalTo(new sbyte[]{ 3, 4, 5 }) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallFuctionWithByteArrayWithBoundLiteral() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCallFuctionWithByteArrayWithBoundLiteral()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "WITH $param AS param RETURN org.neo4j.procedure.decrBytes(param) AS bytes, param", map( "param", new sbyte[]{ 10, 11, 12 } ) );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertTrue( res.HasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					IDictionary<string, object> results = res.Next();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
					assertThat( results["bytes"], equalTo( new sbyte[]{ 9, 10, 11 } ) );
					assertThat( results["param"], equalTo( new sbyte[]{ 10, 11, 12 } ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowNonByteValuesInImplicitByteArrayConversionWithUserDefinedFunction() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowNonByteValuesInImplicitByteArrayConversionWithUserDefinedFunction()
		 {
			  //Expect
			  Exception.expect( typeof( QueryExecutionException ) );
			  Exception.expectMessage( "Cannot convert 1 to byte for input to procedure" );

			  // When
			  using ( Transaction ignore = _db.beginTx() )
			  {
					//Make sure argument here is not auto parameterized away as that will drop all type information on the floor
					Result result = _db.execute( "RETURN org.neo4j.procedure.decrBytes([1,2,5]) AS bytes" );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					result.Next();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallAggregationFunctionWithByteArrays() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCallAggregationFunctionWithByteArrays()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					sbyte[][] data = new sbyte[3][];
					data[0] = new sbyte[]{ 1, 2, 3 };
					data[1] = new sbyte[]{ 3, 2, 1 };
					data[2] = new sbyte[]{ 1, 2, 1 };
					Result res = _db.execute( "UNWIND $data AS bytes RETURN org.neo4j.procedure.aggregateByteArrays(bytes) AS bytes", map( "data", data ) );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.ColumnAs( "bytes" ).next(), equalTo(new sbyte[]{ 5, 6, 5 }) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseGuardToDetectTransactionTermination() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUseGuardToDetectTransactionTermination()
		 {
			  Exception.expect( typeof( QueryExecutionException ) );
			  Exception.expectMessage( "The transaction has been terminated. Retry your operation in a new " + "transaction, and you should see a successful result. Explicitly terminated by the user. " );

			  // When
			  _db.execute( "CALL org.neo4j.procedure.guardMe" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMakeTransactionToFail()
		 public virtual void ShouldMakeTransactionToFail()
		 {
			  //When
			  using ( Transaction ignore = _db.beginTx() )
			  {
					_db.createNode( Label.label( "Person" ) );
			  }
			  Result result = _db.execute( "CALL org.neo4j.procedure.failingPersonCount" );
			  //Then
			  Exception.expect( typeof( TransactionFailureException ) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  result.Next();
		 }

		 public class Output
		 {
			  public long SomeVal = 1337;

			  public Output()
			  {
			  }

			  public Output( long someVal )
			  {
					this.SomeVal = someVal;
			  }
		 }

		 public class PrimitiveOutput
		 {
			  public string String;
			  public long Integer;
			  public double AFloat;
			  public bool ABoolean;

			  public PrimitiveOutput( string @string, long integer, double aFloat, bool aBoolean )
			  {
					this.String = @string;
					this.Integer = integer;
					this.AFloat = aFloat;
					this.ABoolean = aBoolean;
			  }
		 }

		 public class MapOutput
		 {
			  public IDictionary<string, object> Map;

			  public MapOutput( IDictionary<string, object> map )
			  {
					this.Map = map;
			  }
		 }

		 public class ListOutput
		 {
			  public IList<long> List;

			  public ListOutput( IList<long> list )
			  {
					this.List = list;
			  }
		 }

		 public class BytesOutput
		 {
			  public sbyte[] Bytes;

			  public BytesOutput( sbyte[] bytes )
			  {
					this.Bytes = bytes;
			  }
		 }

		 public class DoubleOutput
		 {
			  public double Result;

			  public DoubleOutput()
			  {
			  }

			  public DoubleOutput( double result )
			  {
					this.Result = result;
			  }
		 }

		 public class NodeOutput
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  public Node NodeConflict;

			  public NodeOutput()
			  {

			  }

			  public NodeOutput( Node node )
			  {
					this.NodeConflict = node;
			  }

			  internal virtual Node Node
			  {
				  set
				  {
						this.NodeConflict = value;
				  }
			  }
		 }

		 public class MyOutputRecord
		 {
			  public string Name;

			  public MyOutputRecord( string name )
			  {
					this.Name = name;
			  }
		 }

		 public class PathOutputRecord
		 {
			  public Path Path;

			  public PathOutputRecord( Path path )
			  {
					this.Path = path;
			  }
		 }

		 public class NodeListRecord
		 {
			  public IList<Node> Nodes;

			  public NodeListRecord( IList<Node> nodes )
			  {
					this.Nodes = nodes;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public static class ClassWithProcedures
		 public class ClassWithProcedures
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.graphdb.GraphDatabaseService db;
			  public GraphDatabaseService Db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.logging.Log log;
			  public Log Log;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public TerminationGuard guard;
			  public TerminationGuard Guard;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public ProcedureTransaction procedureTransaction;
			  public ProcedureTransaction ProcedureTransaction;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Output> guardMe()
			  public virtual Stream<Output> GuardMe()
			  {
					ProcedureTransaction.terminate();
					Guard.check();
					throw new System.InvalidOperationException( "Should never have executed this!" );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Output> integrationTestMe()
			  public virtual Stream<Output> IntegrationTestMe()
			  {
					return Stream.of( new Output() );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Output> failingPersonCount()
			  public virtual Stream<Output> FailingPersonCount()
			  {
					Result result = Db.execute( "MATCH (n:Person) RETURN count(n) as count" );
					ProcedureTransaction.failure();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					return Stream.of( new Output( ( long? ) result.Next()["count"].Value ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Output> simpleArgument(@Name("name") long someValue)
			  public virtual Stream<Output> SimpleArgument( long someValue )
			  {
					return Stream.of( new Output( someValue ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Output> simpleArgumentWithDefault(@Name(value = "name", defaultValue = "42") long someValue)
			  public virtual Stream<Output> SimpleArgumentWithDefault( long someValue )
			  {
					return Stream.of( new Output( someValue ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<PrimitiveOutput> defaultValues(@Name(value = "string", defaultValue = "a string") String string, @Name(value = "integer", defaultValue = "42") long integer, @Name(value = "float", defaultValue = "3.14") double aFloat, @Name(value = "boolean", defaultValue = "true") boolean aBoolean)
			  public virtual Stream<PrimitiveOutput> DefaultValues( string @string, long integer, double aFloat, bool aBoolean )
			  {
					return Stream.of( new PrimitiveOutput( @string, integer, aFloat, aBoolean ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Output> nodeListArgument(@Name("nodes") java.util.List<org.neo4j.graphdb.Node> nodes)
			  public virtual Stream<Output> NodeListArgument( IList<Node> nodes )
			  {
					return Stream.of( new Output( nodes.Count ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Output> delegatingProcedure(@Name("name") long someValue)
			  public virtual Stream<Output> DelegatingProcedure( long someValue )
			  {
					return Db.execute( "CALL org.neo4j.procedure.simpleArgument", map( "name", someValue ) ).Select( row => new Output( ( long? ) row.get( "someVal" ).Value ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Output> recursiveSum(@Name("order") long order)
			  public virtual Stream<Output> RecursiveSum( long order )
			  {
					if ( order == 0L )
					{
						 return Stream.of( new Output( 0L ) );
					}
					else
					{
						 long? prev = ( long? ) Db.execute( "CALL org.neo4j.procedure.recursiveSum", map( "order", order - 1 ) ).next().get("someVal");
						 return Stream.of( new Output( order + prev ) );
					}
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Output> genericArguments(@Name("stringList") java.util.List<java.util.List<String>> stringList, @Name("longList") java.util.List<java.util.List<java.util.List<long>>> longList)
			  public virtual Stream<Output> GenericArguments( IList<IList<string>> stringList, IList<IList<IList<long>>> longList )
			  {
					return Stream.of( new Output( stringList.Count + longList.Count ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Output> mapArgument(@Name("map") java.util.Map<String,Object> map)
			  public virtual Stream<Output> MapArgument( IDictionary<string, object> map )
			  {
					return Stream.of( new Output( map.Count ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<MapOutput> mapWithNullDefault(@Name(value = "map", defaultValue = "null") java.util.Map<String,Object> map)
			  public virtual Stream<MapOutput> MapWithNullDefault( IDictionary<string, object> map )
			  {
					return Stream.of( new MapOutput( map ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<MapOutput> mapWithOtherDefault(@Name(value = "map", defaultValue = "{default: true}") java.util.Map<String,Object> map)
			  public virtual Stream<MapOutput> MapWithOtherDefault( IDictionary<string, object> map )
			  {
					return Stream.of( new MapOutput( map ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<ListOutput> listWithDefault(@Name(value = "list", defaultValue = "[42, 1337]") java.util.List<long> list)
			  public virtual Stream<ListOutput> ListWithDefault( IList<long> list )
			  {
					return Stream.of( new ListOutput( list ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<ListOutput> genericListWithDefault(@Name(value = "list", defaultValue = "[[42, 1337]]") java.util.List<java.util.List<long>> list)
			  public virtual Stream<ListOutput> GenericListWithDefault( IList<IList<long>> list )
			  {
					return Stream.of( new ListOutput( list == null ? null : list[0] ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<BytesOutput> incrBytes(@Name(value = "bytes") byte[] bytes)
			  public virtual Stream<BytesOutput> IncrBytes( sbyte[] bytes )
			  {
					for ( int i = 0; i < bytes.Length; i++ )
					{
						 bytes[i] += 1;
					}
					return Stream.of( new BytesOutput( bytes ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<NodeOutput> node(@Name("id") long id)
			  public virtual Stream<NodeOutput> Node( long id )
			  {
					NodeOutput nodeOutput = new NodeOutput();
					if ( id < 0 )
					{
						 nodeOutput.Node = null;
					}
					else
					{
						 nodeOutput.Node = Db.getNodeById( id );
					}
					return Stream.of( nodeOutput );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<DoubleOutput> squareDouble(@Name("value") double value)
			  public virtual Stream<DoubleOutput> SquareDouble( double value )
			  {
					DoubleOutput output = new DoubleOutput( value * value );
					return Stream.of( output );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<DoubleOutput> avgNumberList(@Name("list") java.util.List<Number> list)
			  public virtual Stream<DoubleOutput> AvgNumberList( IList<Number> list )
			  {
					double sum = list.Aggregate( ( l, r ) => l.doubleValue() + r.doubleValue() ).orElse(0.0d).doubleValue();
					int count = list.Count;
					DoubleOutput output = new DoubleOutput( sum / count );
					return Stream.of( output );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<DoubleOutput> avgDoubleList(@Name("list") java.util.List<double> list)
			  public virtual Stream<DoubleOutput> AvgDoubleList( IList<double> list )
			  {
					double sum = list.Aggregate( ( l, r ) => l + r ).orElse( 0.0d );
					int count = list.Count;
					DoubleOutput output = new DoubleOutput( sum / count );
					return Stream.of( output );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Output> squareLong(@Name("value") long value)
			  public virtual Stream<Output> SquareLong( long value )
			  {
					Output output = new Output( value * value );
					return Stream.of( output );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Output> throwsExceptionInStream()
			  public virtual Stream<Output> ThrowsExceptionInStream()
			  {
					return Stream.generate(() =>
					{
					 throw new Exception( "Kaboom" );
					});
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Output> indexOutOfBounds()
			  public virtual Stream<Output> IndexOutOfBounds()
			  {
					int[] ints = new int[] { 1, 2, 3 };
					int foo = ints[4];
					return Stream.of( new Output() );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<MyOutputRecord> listCoolPeopleInDatabase()
			  public virtual Stream<MyOutputRecord> ListCoolPeopleInDatabase()
			  {
					return Db.findNodes( label( "Person" ) ).Select( n => new MyOutputRecord( ( string ) n.getProperty( "name" ) ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Output> logAround()
			  public virtual Stream<Output> LogAround()
			  {
					Log.debug( "1" );
					Log.info( "2" );
					Log.warn( "3" );
					Log.error( "4" );
					return Stream.empty();
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Output> readOnlyTryingToWrite()
			  public virtual Stream<Output> ReadOnlyTryingToWrite()
			  {
					Db.createNode();
					return Stream.empty();
			  }

			  [Procedure(mode : WRITE)]
			  public virtual Stream<Output> WritingProcedure()
			  {
					Db.createNode();
					return Stream.empty();
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure(mode = WRITE) public java.util.stream.Stream<NodeOutput> createNode(@Name("value") String value)
			  [Procedure(mode : WRITE)]
			  public virtual Stream<NodeOutput> CreateNode( string value )
			  {
					Node node = Db.createNode();
					node.SetProperty( "prop", value );
					NodeOutput @out = new NodeOutput();
					@out.Node = node;
					return Stream.of( @out );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<Output> readOnlyCallingWriteProcedure()
			  public virtual Stream<Output> ReadOnlyCallingWriteProcedure()
			  {
					return Db.execute( "CALL org.neo4j.procedure.writingProcedure" ).Select( row => new Output( 0 ) );
			  }

			  [Procedure(mode : WRITE)]
			  public virtual Stream<Output> WriteProcedureCallingWriteProcedure()
			  {
					return Db.execute( "CALL org.neo4j.procedure.writingProcedure" ).Select( row => new Output( 0 ) );
			  }

			  [Procedure(mode : WRITE)]
			  public virtual Stream<Output> WriteProcedureCallingReadProcedure()
			  {
					return Db.execute( "CALL org.neo4j.procedure.integrationTestMe" ).Select( row => new Output( 0 ) );
			  }

			  [Procedure(mode : WRITE)]
			  public virtual Stream<Output> WriteProcedureCallingSchemaProcedure()
			  {
					return Db.execute( "CALL org.neo4j.procedure.schemaProcedure" ).Select( row => new Output( 0 ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure(mode = WRITE) public void sideEffect(@Name("value") String value)
			  [Procedure(mode : WRITE)]
			  public virtual void SideEffect( string value )
			  {
					Db.createNode( Label.label( value ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure(mode = WRITE) public void sideEffectWithDefault(@Name("label") String label, @Name("propertyKey") String propertyKey, @Name(value = "value", defaultValue = "Zhang Wei") String value)
			  [Procedure(mode : WRITE)]
			  public virtual void SideEffectWithDefault( string label, string propertyKey, string value )
			  {
					Db.createNode( Label.label( label ) ).setProperty( propertyKey, value );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void shutdown()
			  public virtual void Shutdown()
			  {
					Db.shutdown();
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure(mode = WRITE) public void delegatingSideEffect(@Name("value") String value)
			  [Procedure(mode : WRITE)]
			  public virtual void DelegatingSideEffect( string value )
			  {
					Db.execute( "CALL org.neo4j.procedure.sideEffect", map( "value", value ) );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void supportedProcedure() throws java.util.concurrent.ExecutionException, InterruptedException
			  [Procedure(mode : WRITE)]
			  public virtual void SupportedProcedure()
			  {
					_jobs.submit(() =>
					{
					 try
					 {
						 using ( Transaction tx = Db.beginTx() )
						 {
							  Db.createNode();
							  tx.success();
						 }
					 }
					 catch ( Exception e )
					 {
						  _exceptionsInProcedure.Add( e );
					 }
					}).get();
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public java.util.stream.Stream<PathOutputRecord> nodePaths(@Name("node") org.neo4j.graphdb.Node node)
			  public virtual Stream<PathOutputRecord> NodePaths( Node node )
			  {
					return Db.execute( "WITH {node} AS node MATCH p=(node)-[*]->() RETURN p", map( "node", node ) ).Select( record => new PathOutputRecord( ( Path ) record.getOrDefault( "p", null ) ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure(mode = WRITE) public java.util.stream.Stream<NodeOutput> nodeWithDefault(@Name(value = "node", defaultValue = "null") org.neo4j.graphdb.Node node)
			  [Procedure(mode : WRITE)]
			  public virtual Stream<NodeOutput> NodeWithDefault( Node node )
			  {
					return Stream.of( new NodeOutput( node ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("This is a description") @Procedure(mode = WRITE) public java.util.stream.Stream<NodeOutput> nodeWithDescription(@Name("node") org.neo4j.graphdb.Node node)
			  [Description("This is a description"), Procedure(mode : WRITE)]
			  public virtual Stream<NodeOutput> NodeWithDescription( Node node )
			  {
					return Stream.of( new NodeOutput( node ) );
			  }

			  [Procedure(mode : WRITE)]
			  public virtual Stream<NodeListRecord> NodeList()
			  {
					IList<Node> nodesList = new List<Node>();
					nodesList.Add( Db.createNode() );
					nodesList.Add( Db.createNode() );

					return Stream.of( new NodeListRecord( nodesList ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure public void readOnlyTryingToWriteSchema()
			  public virtual void ReadOnlyTryingToWriteSchema()
			  {
					Db.execute( "CREATE CONSTRAINT ON (book:Book) ASSERT book.isbn IS UNIQUE" );
			  }

			  [Procedure(mode : WRITE)]
			  public virtual void ReadWriteTryingToWriteSchema()
			  {
					Db.execute( "CREATE CONSTRAINT ON (book:Book) ASSERT book.isbn IS UNIQUE" );
			  }

			  [Procedure(mode : SCHEMA)]
			  public virtual void SchemaProcedure()
			  {
					Db.execute( "CREATE CONSTRAINT ON (book:Book) ASSERT book.isbn IS UNIQUE" );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure(mode = SCHEMA) public java.util.stream.Stream<NodeOutput> schemaCallReadProcedure(@Name("id") long id)
			  [Procedure(mode : SCHEMA)]
			  public virtual Stream<NodeOutput> SchemaCallReadProcedure( long id )
			  {
					return Db.execute( "CALL org.neo4j.procedure.node(" + id + ")" ).Select(record =>
					{
					 NodeOutput n = new NodeOutput();
					 n.Node = ( Node ) record.get( "node" );
					 return n;
					});
			  }

			  [Procedure(mode : SCHEMA)]
			  public virtual void SchemaTryingToWrite()
			  {
					Db.execute( "CREATE CONSTRAINT ON (book:Book) ASSERT book.isbn IS UNIQUE" );
					Db.createNode();
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure(name = "org.neo4j.procedure.onCloseProcedure") public java.util.stream.Stream<Output> onCloseProcedure(@Name("index") long index)
			  [Procedure(name : "org.neo4j.procedure.onCloseProcedure")]
			  public virtual Stream<Output> OnCloseProcedure( long index )
			  {
					OnCloseCalled[( int ) index] = false;
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					return Stream.of( 1L, 2L ).map( Output::new ).onClose( () => OnCloseCalled[(int) index] = true );
			  }
		 }

		 public class ClassWithFunctions
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction() public String getNodeName(@Name(value = "node", defaultValue = "null") org.neo4j.graphdb.Node node)
			  [UserFunction()]
			  public virtual string GetNodeName( Node node )
			  {
					return "nodeName";
			  }

			  [Description("This is a description"), UserFunction()]
			  public virtual long FunctionWithDescription()
			  {
					return 0;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public byte[] decrBytes(@Name(value = "bytes") byte[] bytes)
			  public virtual sbyte[] DecrBytes( sbyte[] bytes )
			  {
					for ( int i = 0; i < bytes.Length; i++ )
					{
						 bytes[i] -= 1;
					}
					return bytes;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationFunction public ByteArrayAggregator aggregateByteArrays()
			  public virtual ByteArrayAggregator AggregateByteArrays()
			  {
					return new ByteArrayAggregator();
			  }

			  public class ByteArrayAggregator
			  {
					internal sbyte[] Aggregated;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationUpdate public void update(@Name("bytes") byte[] bytes)
					public virtual void Update( sbyte[] bytes )
					{
						 if ( Aggregated == null )
						 {
							  Aggregated = new sbyte[bytes.Length];
						 }
						 for ( int i = 0; i < Math.Min( bytes.Length, Aggregated.Length ); i++ )
						 {
							  Aggregated[i] += bytes[i];
						 }
					}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserAggregationResult public byte[] result()
					public virtual sbyte[] Result()
					{
						 return Aggregated == null ? new sbyte[0] : Aggregated;
					}
			  }
		 }

		 private static readonly ScheduledExecutorService _jobs = Executors.newScheduledThreadPool( 5 );
	}

}