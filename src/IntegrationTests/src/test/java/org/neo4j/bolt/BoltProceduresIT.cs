using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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
namespace Neo4Net.Bolt
{
	using RandomStringUtils = org.apache.commons.lang3.RandomStringUtils;
	using AfterClass = org.junit.AfterClass;
	using BeforeClass = org.junit.BeforeClass;
	using ClassRule = org.junit.ClassRule;
	using Test = org.junit.Test;


	using Driver = Neo4Net.driver.v1.Driver;
	using GraphDatabase = Neo4Net.driver.v1.GraphDatabase;
	using Record = Neo4Net.driver.v1.Record;
	using Session = Neo4Net.driver.v1.Session;
	using StatementResult = Neo4Net.driver.v1.StatementResult;
	using TransientException = Neo4Net.driver.v1.exceptions.TransientException;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Result = Neo4Net.Graphdb.Result;
	using Neo4jRule = Neo4Net.Harness.junit.Neo4jRule;
	using Iterables = Neo4Net.Helpers.Collection.Iterables;
	using Iterators = Neo4Net.Helpers.Collection.Iterators;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Status = Neo4Net.Kernel.Api.Exceptions.Status;
	using Settings = Neo4Net.Kernel.configuration.Settings;
	using OnlineBackupSettings = Neo4Net.Kernel.impl.enterprise.configuration.OnlineBackupSettings;
	using Context = Neo4Net.Procedure.Context;
	using Mode = Neo4Net.Procedure.Mode;
	using Procedure = Neo4Net.Procedure.Procedure;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class BoltProceduresIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final org.neo4j.harness.junit.Neo4jRule db = new org.neo4j.harness.junit.Neo4jRule().withProcedure(BoltTestProcedures.class).withConfig(org.neo4j.kernel.impl.enterprise.configuration.OnlineBackupSettings.online_backup_enabled, org.neo4j.kernel.configuration.Settings.FALSE);
		 public static readonly Neo4jRule Db = new Neo4jRule().withProcedure(typeof(BoltTestProcedures)).withConfig(OnlineBackupSettings.online_backup_enabled, Settings.FALSE);

		 private static Driver _driver;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void setUp() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public static void SetUp()
		 {
			  _driver = GraphDatabase.driver( Db.boltURI() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @AfterClass public static void tearDown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public static void TearDown()
		 {
			  if ( _driver != null )
			  {
					_driver.close();
			  }
		 }

		 /// <summary>
		 /// Test creates a situation where streaming of a node fails when accessing node labels/properties.
		 /// It fails because transaction is terminated. Bolt server should not send half-written message.
		 /// Driver should receive a regular FAILURE message saying that transaction has been terminated.
		 /// </summary>
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTransmitStreamingFailure()
		 public virtual void ShouldTransmitStreamingFailure()
		 {
			  using ( Session session = _driver.session() )
			  {
					IDictionary<string, object> @params = new Dictionary<string, object>();
					@params["name1"] = RandomLongString();
					@params["name2"] = RandomLongString();
					session.run( "CREATE (n1 :Person {name: $name1}), (n2 :Person {name: $name2}) RETURN n1, n2", @params ).consume();

					StatementResult result = session.run( "CALL test.readNodesReturnThemAndTerminateTheTransaction() YIELD node" );

					assertTrue( result.hasNext() );
					Record record = result.next();
					assertEquals( "Person", Iterables.single( record.get( 0 ).asNode().labels() ) );
					assertNotNull( record.get( 0 ).asNode().get("name") );

					try
					{
						 result.hasNext();
						 fail( "Exception expected" );
					}
					catch ( TransientException e )
					{
						 assertEquals( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Terminated.code().serialize(), e.code() );
					}
			  }
		 }

		 private static string RandomLongString()
		 {
			  return RandomStringUtils.randomAlphanumeric( 10_000 );
		 }

		 public class BoltTestProcedures
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.graphdb.GraphDatabaseService db;
			  public GraphDatabaseService Db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.kernel.api.KernelTransaction tx;
			  public KernelTransaction Tx;

			  [Procedure(name : "test.readNodesReturnThemAndTerminateTheTransaction", mode : Neo4Net.Procedure.Mode.READ)]
			  public virtual Stream<NodeResult> ReadNodesReturnThemAndTerminateTheTransaction()
			  {
					Result result = Db.execute( "MATCH (n) RETURN n" );

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					NodeResult[] results = result.Select( record => ( Node ) record.get( "n" ) ).Select( NodeResult::new ).ToArray( NodeResult[]::new );

					return Iterators.stream( new TransactionTerminatingIterator<>( Tx, results ) );
			  }
		 }

		 public class NodeResult
		 {
			  public Node Node;

			  internal NodeResult( Node node )
			  {
					this.Node = node;
			  }
		 }

		 /// <summary>
		 /// Returnes given elements, terminates the transaction before returning the very last one.
		 /// </summary>
		 /// @param <T> type of elements. </param>
		 private class TransactionTerminatingIterator<T> : IEnumerator<T>
		 {
			  internal readonly KernelTransaction Tx;
			  internal readonly LinkedList<T> Elements;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SafeVarargs private TransactionTerminatingIterator(org.neo4j.kernel.api.KernelTransaction tx, T... elements)
			  internal TransactionTerminatingIterator( KernelTransaction tx, params T[] elements )
			  {
					this.Tx = tx;
					this.Elements = new LinkedList<T>();
					Collections.addAll( this.Elements, elements );
			  }

			  public override bool HasNext()
			  {
					return Elements.Count > 0;
			  }

			  public override T Next()
			  {
					if ( Elements.Count == 1 )
					{
						 // terminate transaction before returning the last element
						 Tx.markForTermination( Neo4Net.Kernel.Api.Exceptions.Status_Transaction.Terminated );
					}
					T element = Elements.RemoveFirst();
					if ( element == default( T ) )
					{
						 throw new NoSuchElementException();
					}
					return element;
			  }
		 }
	}

}