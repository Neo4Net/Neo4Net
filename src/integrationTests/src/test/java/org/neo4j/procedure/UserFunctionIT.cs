using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Procedure
{
	using After = org.junit.After;
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using TemporaryFolder = org.junit.rules.TemporaryFolder;


	using Predicates = Neo4Net.Functions.Predicates;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Path = Neo4Net.GraphDb.Path;
	using QueryExecutionException = Neo4Net.GraphDb.QueryExecutionException;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Result = Neo4Net.GraphDb.Result;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using AuthorizationViolationException = Neo4Net.GraphDb.security.AuthorizationViolationException;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using Iterators = Neo4Net.Collections.Helpers.Iterators;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using AnonymousContext = Neo4Net.Kernel.api.security.AnonymousContext;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using JarBuilder = Neo4Net.Kernel.impl.proc.JarBuilder;
	using Procedures = Neo4Net.Kernel.impl.proc.Procedures;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using Log = Neo4Net.Logging.Log;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.startsWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsEqual.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterables.asList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.logging.AssertableLogProvider.inLog;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.procedure.Mode.WRITE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.procedure.StringMatcherIgnoresNewlines.containsStringIgnoreNewlines;

	public class UserFunctionIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.TemporaryFolder plugins = new org.junit.rules.TemporaryFolder();
		 public TemporaryFolder Plugins = new TemporaryFolder();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException exception = org.junit.rules.ExpectedException.none();
		 public ExpectedException Exception = ExpectedException.none();

		 private static IList<Exception> _exceptionsInFunction = Collections.synchronizedList( new List<Exception>() );
		 private IGraphDatabaseService _db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveNiceErrorMessageOnWrongStaticType()
		 public virtual void ShouldGiveNiceErrorMessageOnWrongStaticType()
		 {
			  //Expect
			  Exception.expect( typeof( QueryExecutionException ) );
			  Exception.expectMessage( "Type mismatch: expected Integer but was String (line 1, column 43 (offset: 42))" );

			  // When
			  using ( Transaction ignore = _db.beginTx() )
			  {
					//Make sure argument here is not auto parameterized away as that will drop all type information on the floor
					_db.execute( "RETURN org.Neo4Net.procedure.simpleArgument('42')" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveNiceErrorMessageWhenNoArguments()
		 public virtual void ShouldGiveNiceErrorMessageWhenNoArguments()
		 {
			  //Expect
			  Exception.expect( typeof( QueryExecutionException ) );
			  Exception.expectMessage( containsStringIgnoreNewlines( string.Format( "Function call does not provide the " + "required number of arguments: expected 1 got 0.%n%n" + "Function org.Neo4Net.procedure.simpleArgument has signature: " + "org.Neo4Net.procedure.simpleArgument(someValue :: INTEGER?) :: INTEGER?%n" + "meaning that it expects 1 argument of type INTEGER? (line 1, column 8 (offset: 7))" ) ) );
			  // When
			  using ( Transaction ignore = _db.beginTx() )
			  {
					_db.execute( "RETURN org.Neo4Net.procedure.simpleArgument()" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldShowDescriptionWhenMissingArguments()
		 public virtual void ShouldShowDescriptionWhenMissingArguments()
		 {
			  //Expect
			  Exception.expect( typeof( QueryExecutionException ) );
			  Exception.expectMessage( containsStringIgnoreNewlines( string.Format( "Function call does not provide the " + "required number of arguments: expected 1 got 0.%n%n" + "Function org.Neo4Net.procedure.nodeWithDescription has signature: " + "org.Neo4Net.procedure.nodeWithDescription(someValue :: NODE?) :: NODE?%n" + "meaning that it expects 1 argument of type NODE?%n" + "Description: This is a description (line 1, column 8 (offset: 7))" ) ) );
			  // When
			  using ( Transaction ignore = _db.beginTx() )
			  {
					_db.execute( "RETURN org.Neo4Net.procedure.nodeWithDescription()" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallDelegatingFunction()
		 public virtual void ShouldCallDelegatingFunction()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "RETURN org.Neo4Net.procedure.delegatingFunction({name}) AS someVal", map( "name", 43L ) );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.Next(), equalTo(map("someVal", 43L)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallRecursiveFunction()
		 public virtual void ShouldCallRecursiveFunction()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "RETURN org.Neo4Net.procedure.recursiveSum({order}) AS someVal", map( "order", 10L ) );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.Next(), equalTo(map("someVal", 55L)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallFunctionWithGenericArgument()
		 public virtual void ShouldCallFunctionWithGenericArgument()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "RETURN org.Neo4Net.procedure.genericArguments([ ['graphs'], ['are'], ['everywhere']], " + "[ [[1, 2, 3]], [[4, 5]]] ) AS someVal" );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.Next(), equalTo(map("someVal", 5L)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallFunctionWithMapArgument()
		 public virtual void ShouldCallFunctionWithMapArgument()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "RETURN org.Neo4Net.procedure.mapArgument({foo: 42, bar: 'hello'}) AS someVal" );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.Next(), equalTo(map("someVal", 2L)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallFunctionWithMapArgumentContainingNullFromParameter()
		 public virtual void ShouldCallFunctionWithMapArgumentContainingNullFromParameter()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "RETURN org.Neo4Net.procedure.mapArgument({foo: $p}) AS someVal", map( "p", null ) );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.Next(), equalTo(map("someVal", 1L)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallFunctionWithNull()
		 public virtual void ShouldCallFunctionWithNull()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "RETURN org.Neo4Net.procedure.mapArgument(null) AS someVal" );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.Next(), equalTo(map("someVal", 0L)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallFunctionWithNullFromParameter()
		 public virtual void ShouldCallFunctionWithNullFromParameter()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "RETURN org.Neo4Net.procedure.mapArgument($p) AS someVal", map( "p", null ) );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.Next(), equalTo(map("someVal", 0L)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallFunctionWithNodeReturn()
		 public virtual void ShouldCallFunctionWithNodeReturn()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					long nodeId = _db.createNode().Id;

					// When
					Result res = _db.execute( "RETURN org.Neo4Net.procedure.node({id}) AS node", map( "id", nodeId ) );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					Node node = ( Node ) res.Next()["node"];
					assertThat( node.Id, equalTo( nodeId ) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveHelpfulErrorOnMissingFunction()
		 public virtual void ShouldGiveHelpfulErrorOnMissingFunction()
		 {
			  // Expect
			  Exception.expect( typeof( QueryExecutionException ) );
			  Exception.expectMessage( string.Format( "Unknown function 'org.someFunctionThatDoesNotExist' (line 1, column 8 (offset: 7))" + "%n" + "\"RETURN org.someFunctionThatDoesNotExist()" ) );

			  // When
			  _db.execute( "RETURN org.someFunctionThatDoesNotExist()" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldGiveHelpfulErrorOnExceptionMidStream()
		 public virtual void ShouldGiveHelpfulErrorOnExceptionMidStream()
		 {
			  // Given
			  // run in tx to avoid having to wait for tx rollback on shutdown
			  using ( Transaction ignore = _db.beginTx() )
			  {
					Result result = _db.execute( "RETURN org.Neo4Net.procedure.throwsExceptionInStream()" );

					// Expect
					Exception.expect( typeof( QueryExecutionException ) );
					Exception.expectMessage( "Failed to invoke function `org.Neo4Net.procedure.throwsExceptionInStream`: Caused by: java.lang" + ".RuntimeException: Kaboom" );

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
					Exception.expectMessage( "Failed to invoke function `org.Neo4Net.procedure.indexOutOfBounds`: Caused by: java.lang" + ".ArrayIndexOutOfBoundsException" );
					// When
					_db.execute( "RETURN org.Neo4Net.procedure.indexOutOfBounds()" ).next();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallFunctionWithAccessToDB()
		 public virtual void ShouldCallFunctionWithAccessToDB()
		 {
			  // When
			  using ( Transaction tx = _db.beginTx() )
			  {
					_db.createNode( label( "Person" ) ).setProperty( "name", "Buddy Holly" );
					tx.Success();
			  }

			  // Then
			  using ( Transaction ignore = _db.beginTx() )
			  {
					Result res = _db.execute( "RETURN org.Neo4Net.procedure.listCoolPeopleInDatabase() AS cool" );

//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertEquals( res.Next()["cool"], singletonList("Buddy Holly") );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLogLikeThereIsNoTomorrow()
		 public virtual void ShouldLogLikeThereIsNoTomorrow()
		 {
			  // Given
			  AssertableLogProvider logProvider = new AssertableLogProvider();

			  _db.shutdown();
			  _db = ( new TestGraphDatabaseFactory() ).setInternalLogProvider(logProvider).setUserLogProvider(logProvider).newImpermanentDatabaseBuilder().setConfig(GraphDatabaseSettings.plugin_dir, Plugins.Root.AbsolutePath).setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newGraphDatabase();

			  // When
			  using ( Transaction ignore = _db.beginTx() )
			  {
					Result res = _db.execute( "RETURN org.Neo4Net.procedure.logAround()" );
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
//ORIGINAL LINE: @Test public void shouldDenyReadOnlyFunctionToPerformWrites()
		 public virtual void ShouldDenyReadOnlyFunctionToPerformWrites()
		 {
			  // Expect
			  Exception.expect( typeof( QueryExecutionException ) );
			  Exception.expectMessage( "Write operations are not allowed" );

			  // When
			  using ( Transaction ignore = _db.beginTx() )
			  {
					_db.execute( "RETURN org.Neo4Net.procedure.readOnlyTryingToWrite()" ).next();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotBeAbleToCallWriteProcedureThroughReadFunction()
		 public virtual void ShouldNotBeAbleToCallWriteProcedureThroughReadFunction()
		 {
			  // Expect
			  Exception.expect( typeof( QueryExecutionException ) );
			  Exception.expectMessage( "Write operations are not allowed" );

			  // When
			  using ( Transaction ignore = _db.beginTx() )
			  {
					_db.execute( "RETURN org.Neo4Net.procedure.readOnlyCallingWriteProcedure()" ).next();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDenyReadOnlyFunctionToPerformSchema()
		 public virtual void ShouldDenyReadOnlyFunctionToPerformSchema()
		 {
			  // Expect
			  Exception.expect( typeof( QueryExecutionException ) );
			  Exception.expectMessage( "Schema operations are not allowed" );

			  // Give
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					_db.execute( "RETURN org.Neo4Net.procedure.readOnlyTryingToWriteSchema()" ).next();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCoerceLongToDoubleAtRuntimeWhenCallingFunction()
		 public virtual void ShouldCoerceLongToDoubleAtRuntimeWhenCallingFunction()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "RETURN org.Neo4Net.procedure.squareDouble({value}) AS result", map( "value", 4L ) );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.Next(), equalTo(map("result", 16.0d)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCoerceListOfNumbersToDoublesAtRuntimeWhenCallingFunction()
		 public virtual void ShouldCoerceListOfNumbersToDoublesAtRuntimeWhenCallingFunction()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "RETURN org.Neo4Net.procedure.avgNumberList({param}) AS result", map( "param", Arrays.asList<Number>( 1L, 2L, 3L ) ) );

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
					Result res = _db.execute( "RETURN org.Neo4Net.procedure.avgDoubleList([{long}, {double}]) AS result", map( "long", 1L, "double", 2.0d ) );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.Next(), equalTo(map("result", 1.5d)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCoerceDoubleToLongAtRuntimeWhenCallingFunction()
		 public virtual void ShouldCoerceDoubleToLongAtRuntimeWhenCallingFunction()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "RETURN org.Neo4Net.procedure.squareLong({value}) as someVal", map( "value", 4L ) );

					// Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.Next(), equalTo(map("someVal", 16L)) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( res.HasNext() );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToPerformWritesOnNodesReturnedFromReadOnlyFunction()
		 public virtual void ShouldBeAbleToPerformWritesOnNodesReturnedFromReadOnlyFunction()
		 {
			  // When
			  using ( Transaction tx = _db.beginTx() )
			  {
					long nodeId = _db.createNode().Id;
					Node node = Iterators.single( _db.execute( "RETURN org.Neo4Net.procedure.node({id}) AS node", map( "id", nodeId ) ).columnAs( "node" ) );
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
			  Exception.expectMessage( "Failed to invoke function `org.Neo4Net.procedure.shutdown`: Caused by: java.lang" + ".UnsupportedOperationException" );

			  using ( Transaction ignore = _db.beginTx() )
			  {
					_db.execute( "RETURN org.Neo4Net.procedure.shutdown()" ).next();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToWriteAfterCallingReadOnlyFunction()
		 public virtual void ShouldBeAbleToWriteAfterCallingReadOnlyFunction()
		 {
			  using ( Transaction ignore = _db.beginTx() )
			  {
					_db.execute( "RETURN org.Neo4Net.procedure.simpleArgument(12)" ).close();
					_db.createNode();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPreserveSecurityContextWhenSpawningThreadsCreatingTransactionInFunctions() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPreserveSecurityContextWhenSpawningThreadsCreatingTransactionInFunctions()
		 {
			  // given
			  ThreadStart doIt = () =>
			  {
				Result result = _db.execute( "RETURN org.Neo4Net.procedure.unsupportedFunction()" );
				result.resultAsString();
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

			  // Then
			  Predicates.await( () => _exceptionsInFunction.Count >= numThreads, 5, TimeUnit.SECONDS );

			  foreach ( Exception exceptionInFunction in _exceptionsInFunction )
			  {
					assertThat( Exceptions.stringify( exceptionInFunction ), exceptionInFunction, instanceOf( typeof( AuthorizationViolationException ) ) );
					assertThat( Exceptions.stringify( exceptionInFunction ), exceptionInFunction.Message, startsWith( "Write operations are not allowed" ) );
			  }

			  Result result = _db.execute( "MATCH () RETURN count(*) as n" );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( result.HasNext(), equalTo(true) );
			  while ( result.MoveNext() )
			  {
					assertThat( result.Current.get( "n" ), equalTo( 0L ) );
			  }
			  result.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToUseFunctionCallWithPeriodicCommit() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToUseFunctionCallWithPeriodicCommit()
		 {
			  // GIVEN
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  string[] lines = IntStream.rangeClosed( 1, 100 ).boxed().map(i => Convert.ToString(i)).toArray(string[]::new);
			  string url = CreateCsvFile( lines );

			  //WHEN
			  Result result = _db.execute( "USING PERIODIC COMMIT 1 " + "LOAD CSV FROM '" + url + "' AS line " + "CREATE (n {prop: org.Neo4Net.procedure.simpleArgument(ToInt(line[0]))}) " + "RETURN n.prop" );
			  // THEN
			  for ( long i = 1; i <= 100L; i++ )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( result.Next()["n.prop"], equalTo(i) );
			  }

			  //Make sure all the lines has been properly commited to the database.
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  string[] dbContents = _db.execute( "MATCH (n) return n.prop" ).Select( m => Convert.ToString( ( long ) m.get( "n.prop" ) ) ).ToArray( string[]::new );
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
			  _db.execute( "USING PERIODIC COMMIT 1 " + "LOAD CSV FROM '" + url + "' AS line " + "WITH org.Neo4Net.procedure.simpleArgument(ToInt(line[0])) AS val " + "RETURN val" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallFunctionReturningPaths()
		 public virtual void ShouldCallFunctionReturningPaths()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					Node node1 = _db.createNode();
					Node node2 = _db.createNode();
					Relationship rel = node1.CreateRelationshipTo( node2, RelationshipType.withName( "KNOWS" ) );

					// When
					Result res = _db.execute( "RETURN org.Neo4Net.procedure.nodePaths({node}) AS path", map( "node", node1 ) );

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
//ORIGINAL LINE: @Test public void shouldBeAbleToUseUDFForLimit()
		 public virtual void ShouldBeAbleToUseUDFForLimit()
		 {
			  // Given
			  using ( Transaction ignore = _db.beginTx() )
			  {
					// When
					Result res = _db.execute( "UNWIND range(0, 100) AS r RETURN r LIMIT org.Neo4Net.procedure.squareLong(2)" );

					// Then
					IList<object> list = Iterators.asList( res ).Select( m => m.get( "r" ) ).ToList();
					assertThat( list, equalTo( Arrays.asList( 0L, 1L, 2L, 3L ) ) );
			  }
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
//ORIGINAL LINE: @Test public void shouldHandleAggregationFunctionInFunctionCall()
		 public virtual void ShouldHandleAggregationFunctionInFunctionCall()
		 {
			  using ( Transaction ignore = _db.beginTx() )
			  {
					_db.createNode( Label.label( "Person" ) );
					_db.createNode( Label.label( "Person" ) );
					assertEquals( _db.execute( "MATCH (n:Person) RETURN org.Neo4Net.procedure.nodeListArgument(collect(n)) AS someVal" ).next().get("someVal"), 2L );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleNullInList()
		 public virtual void ShouldHandleNullInList()
		 {
			  using ( Transaction ignore = _db.beginTx() )
			  {
					_db.createNode( Label.label( "Person" ) );
					assertEquals( _db.execute( "MATCH (n:Person) RETURN org.Neo4Net.procedure.nodeListArgument([n, null]) AS someVal" ).next().get("someVal"), 1L );
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
					Result res = _db.execute( "MATCH (n:Person) WITH collect(n) as persons RETURN org.Neo4Net.procedure.nodeListArgument(persons)" + " AS someVal" );

					// THEN
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( res.Next()["someVal"], equalTo(2L) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowReadFunctionInNoneTransaction()
		 public virtual void ShouldNotAllowReadFunctionInNoneTransaction()
		 {
			  // Expect
			  Exception.expect( typeof( AuthorizationViolationException ) );
			  Exception.expectMessage( "Read operations are not allowed" );

			  GraphDatabaseAPI gdapi = ( GraphDatabaseAPI ) _db;

			  // When
			  using ( Transaction tx = gdapi.BeginTransaction( KernelTransaction.Type.@explicit, AnonymousContext.none() ) )
			  {
					_db.execute( "RETURN org.Neo4Net.procedure.integrationTestMe()" ).next();
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallProcedureWithAllDefaultArgument()
		 public virtual void ShouldCallProcedureWithAllDefaultArgument()
		 {
			  //Given/When
			  Result res = _db.execute( "RETURN org.Neo4Net.procedure.defaultValues() AS result" );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( res.Next()["result"], equalTo("a string,42,3.14,true") );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( res.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleNullAsParameter()
		 public virtual void ShouldHandleNullAsParameter()
		 {
			  //Given/When
			  Result res = _db.execute( "RETURN org.Neo4Net.procedure.defaultValues($p) AS result", map( "p", null ) );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( res.Next()["result"], equalTo("null,42,3.14,true") );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( res.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallFunctionWithOneProvidedRestDefaultArgument()
		 public virtual void ShouldCallFunctionWithOneProvidedRestDefaultArgument()
		 {
			  //Given/When
			  Result res = _db.execute( "RETURN org.Neo4Net.procedure.defaultValues('another string') AS result" );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( res.Next()["result"], equalTo("another string,42,3.14,true") );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( res.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallFunctionWithTwoProvidedRestDefaultArgument()
		 public virtual void ShouldCallFunctionWithTwoProvidedRestDefaultArgument()
		 {
			  //Given/When
			  Result res = _db.execute( "RETURN org.Neo4Net.procedure.defaultValues('another string', 1337) AS result" );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( res.Next()["result"], equalTo("another string,1337,3.14,true") );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( res.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallFunctionWithThreeProvidedRestDefaultArgument()
		 public virtual void ShouldCallFunctionWithThreeProvidedRestDefaultArgument()
		 {
			  //Given/When
			  Result res = _db.execute( "RETURN org.Neo4Net.procedure.defaultValues('another string', 1337, 2.718281828) AS result" );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( res.Next()["result"], equalTo("another string,1337,2.72,true") );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( res.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallFunctionWithFourProvidedRestDefaultArgument()
		 public virtual void ShouldCallFunctionWithFourProvidedRestDefaultArgument()
		 {
			  //Given/When
			  Result res = _db.execute( "RETURN org.Neo4Net.procedure.defaultValues('another string', 1337, 2.718281828, false) AS result" );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( res.Next()["result"], equalTo("another string,1337,2.72,false") );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( res.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallFunctionReturningNull()
		 public virtual void ShouldCallFunctionReturningNull()
		 {
			  //Given/When
			  Result res = _db.execute( "RETURN org.Neo4Net.procedure.node(-1) AS result" );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( res.Next()["result"], equalTo(null) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( res.HasNext() );
		 }

		 /// <summary>
		 /// NOTE: this test tests user-defined functions added in this file <seealso cref="ClassWithFunctions"/>. These are not
		 /// built-in functions in any shape or form.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAllUserDefinedFunctions()
		 public virtual void ShouldListAllUserDefinedFunctions()
		 {
			  //Given/When
			  Result res = _db.execute( "CALL dbms.functions()" );

			  try
			  {
					  using ( StreamReader reader = new StreamReader( typeof( UserFunctionIT ).getResourceAsStream( "/misc/userDefinedFunctions" ) ) )
					  {
						string expected = reader.lines().collect(Collectors.joining(Environment.NewLine));
						string actual = res.ResultAsString();
						// Be aware that the text file "userDefinedFunctions" must end with two newlines
						assertThat( actual, equalTo( expected ) );
					  }
			  }
			  catch ( IOException )
			  {
					throw new Exception( "Failed to read userDefinedFunctions file." );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCallFunctionWithSameNameAsBuiltIn()
		 public virtual void ShouldCallFunctionWithSameNameAsBuiltIn()
		 {
			  //Given/When
			  Result res = _db.execute( "RETURN this.is.test.only.sum([1337, 2.718281828, 3.1415]) AS result" );

			  // Then
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertThat( res.Next()["result"], equalTo(1337 + 2.718281828 + 3.1415) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertFalse( res.HasNext() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setUp() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SetUp()
		 {
			  _exceptionsInFunction.Clear();
			  ( new JarBuilder() ).createJarFor(Plugins.newFile("myFunctions.jar"), typeof(ClassWithFunctions));
			  _db = ( new TestGraphDatabaseFactory() ).newImpermanentDatabaseBuilder().setConfig(GraphDatabaseSettings.plugin_dir, Plugins.Root.AbsolutePath).setConfig(GraphDatabaseSettings.record_id_batch_size, "1").setConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE).newGraphDatabase();

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

		 public class ClassWithFunctions
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.Neo4Net.graphdb.GraphDatabaseService db;
			  public IGraphDatabaseService Db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.Neo4Net.logging.Log log;
			  public Log Log;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public long integrationTestMe()
			  public virtual long IntegrationTestMe()
			  {
					return 1337L;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public long simpleArgument(@Name("someValue") long someValue)
			  public virtual long SimpleArgument( long someValue )
			  {
					return someValue;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String defaultValues(@Name(value = "string", defaultValue = "a string") String string, @Name(value = "integer", defaultValue = "42") long integer, @Name(value = "float", defaultValue = "3.14") double aFloat, @Name(value = "boolean", defaultValue = "true") boolean aBoolean)
			  public virtual string DefaultValues( string @string, long integer, double aFloat, bool aBoolean )
			  {
//JAVA TO C# CONVERTER TODO TASK: The following line has a Java format specifier which cannot be directly translated to .NET:
//ORIGINAL LINE: return String.format("%s,%d,%.2f,%b", string, integer, aFloat, aBoolean);
					return string.Format( "%s,%d,%.2f,%b", @string, integer, aFloat, aBoolean );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public long nodeListArgument(@Name("nodes") java.util.List<org.Neo4Net.graphdb.Node> nodes)
			  public virtual long NodeListArgument( IList<Node> nodes )
			  {
					long count = 0L;
					foreach ( Node node in nodes )
					{
						 if ( node != null )
						 {
							  count++;
						 }
					}
					return count;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public long delegatingFunction(@Name("someValue") long someValue)
			  public virtual long DelegatingFunction( long someValue )
			  {
					return ( long ) Db.execute( "RETURN org.Neo4Net.procedure.simpleArgument({name}) AS result", map( "name", someValue ) ).next().get("result");
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public long recursiveSum(@Name("someValue") long order)
			  public virtual long RecursiveSum( long order )
			  {
					if ( order == 0L )
					{
						 return 0L;
					}
					else
					{
						 long? prev = ( long? ) Db.execute( "RETURN org.Neo4Net.procedure.recursiveSum({order}) AS someVal", map( "order", order - 1 ) ).next().get("someVal");
						 return order + prev;
					}
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public long genericArguments(@Name("strings") java.util.List<java.util.List<String>> stringList, @Name("longs") java.util.List<java.util.List<java.util.List<long>>> longList)
			  public virtual long GenericArguments( IList<IList<string>> stringList, IList<IList<IList<long>>> longList )
			  {
					return stringList.Count + longList.Count;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public long mapArgument(@Name("map") java.util.Map<String,Object> map)
			  public virtual long MapArgument( IDictionary<string, object> map )
			  {
					if ( map == null )
					{
						 return 0;
					}
					return map.Count;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public org.Neo4Net.graphdb.Node node(@Name("id") long id)
			  public virtual Node Node( long id )
			  {
					if ( id < 0 )
					{
						 return null;
					}
					return Db.getNodeById( id );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public double squareDouble(@Name("someValue") double value)
			  public virtual double SquareDouble( double value )
			  {
					return value * value;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public double avgNumberList(@Name("someValue") java.util.List<Number> list)
			  public virtual double AvgNumberList( IList<Number> list )
			  {
					return list.Aggregate( ( l, r ) => l.doubleValue() + r.doubleValue() ).orElse(0.0d).doubleValue() / list.Count;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public double avgDoubleList(@Name("someValue") java.util.List<double> list)
			  public virtual double AvgDoubleList( IList<double> list )
			  {
					return list.Aggregate( ( l, r ) => l + r ).orElse( 0.0d ) / list.Count;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public long squareLong(@Name("someValue") long value)
			  public virtual long SquareLong( long value )
			  {
					return value * value;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public long throwsExceptionInStream()
			  public virtual long ThrowsExceptionInStream()
			  {
					throw new Exception( "Kaboom" );
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public long indexOutOfBounds()
			  public virtual long IndexOutOfBounds()
			  {
					int[] ints = new int[] { 1, 2, 3 };
					return ints[4];
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public java.util.List<String> listCoolPeopleInDatabase()
			  public virtual IList<string> ListCoolPeopleInDatabase()
			  {
					return Db.findNodes( label( "Person" ) ).map( node => ( string ) node.getProperty( "name" ) ).ToList();
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public long logAround()
			  public virtual long LogAround()
			  {
					Log.debug( "1" );
					Log.info( "2" );
					Log.warn( "3" );
					Log.error( "4" );
					return 1337L;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public org.Neo4Net.graphdb.Node readOnlyTryingToWrite()
			  public virtual Node ReadOnlyTryingToWrite()
			  {
					return Db.createNode();
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public org.Neo4Net.graphdb.Node readOnlyCallingWriteFunction()
			  public virtual Node ReadOnlyCallingWriteFunction()
			  {
					return ( Node ) Db.execute( "RETURN org.Neo4Net.procedure.writingFunction() AS node" ).next().get("node");
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public long readOnlyCallingWriteProcedure()
			  public virtual long ReadOnlyCallingWriteProcedure()
			  {
					Db.execute( "CALL org.Neo4Net.procedure.writingProcedure()" );
					return 1337L;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction("this.is.test.only.sum") public Number sum(@Name("numbers") java.util.List<Number> numbers)
			  [UserFunction("this.is.test.only.sum")]
			  public virtual Number Sum( IList<Number> numbers )
			  {
					return numbers.Select( Number.doubleValue ).Sum();
			  }

			  [Procedure(mode : WRITE)]
			  public virtual void WritingProcedure()
			  {
					Db.createNode();
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String shutdown()
			  public virtual string Shutdown()
			  {
					Db.shutdown();
					return "oh no!";
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String unsupportedFunction()
			  public virtual string UnsupportedFunction()
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
						  _exceptionsInFunction.Add( e );
					 }
					});

					return "why!?";
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public org.Neo4Net.graphdb.Path nodePaths(@Name("someValue") org.Neo4Net.graphdb.Node node)
			  public virtual Path NodePaths( Node node )
			  {
					return ( Path ) Db.execute( "WITH {node} AS node MATCH p=(node)-[*]->() RETURN p", map( "node", node ) ).next().getOrDefault("p", null);
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Description("This is a description") @UserFunction public org.Neo4Net.graphdb.Node nodeWithDescription(@Name("someValue") org.Neo4Net.graphdb.Node node)
			  [Description("This is a description")]
			  public virtual Node NodeWithDescription( Node node )
			  {
					return node;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @UserFunction public String readOnlyTryingToWriteSchema()
			  public virtual string ReadOnlyTryingToWriteSchema()
			  {
					Db.execute( "CREATE CONSTRAINT ON (book:Book) ASSERT book.isbn IS UNIQUE" );
					return "done";
			  }
		 }

		 private static readonly ScheduledExecutorService _jobs = Executors.newScheduledThreadPool( 5 );
	}

}