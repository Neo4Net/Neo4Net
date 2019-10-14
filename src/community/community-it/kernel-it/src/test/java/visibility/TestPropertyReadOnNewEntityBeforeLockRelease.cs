using System;
using System.Threading;

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
namespace Visibility
{
	using BeforeClass = org.junit.BeforeClass;
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;
	using RepeatRule = Neo4Net.Test.rule.RepeatRule;
	using Repeat = Neo4Net.Test.rule.RepeatRule.Repeat;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;

	public class TestPropertyReadOnNewEntityBeforeLockRelease
	{
		 private const string INDEX_NAME = "nodes";
		 private const int MAX_READER_DELAY_MS = 10;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static final org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public static readonly DatabaseRule Db = new ImpermanentDatabaseRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.RepeatRule repeat = new org.neo4j.test.rule.RepeatRule();
		 public readonly RepeatRule Repeat = new RepeatRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeClass public static void initializeIndex()
		 public static void InitializeIndex()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();
					Db.index().forNodes(INDEX_NAME).add(node, "foo", "bar");
					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test @Repeat(times = 100) public void shouldBeAbleToReadPropertiesFromNewNodeReturnedFromIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 [Repeat(times : 100)]
		 public virtual void ShouldBeAbleToReadPropertiesFromNewNodeReturnedFromIndex()
		 {
			  string propertyKey = System.Guid.randomUUID().ToString();
			  string propertyValue = System.Guid.randomUUID().ToString();
			  AtomicBoolean start = new AtomicBoolean( false );
			  int readerDelay = ThreadLocalRandom.current().Next(MAX_READER_DELAY_MS);

			  Writer writer = new Writer( Db, propertyKey, propertyValue, start );
			  Reader reader = new Reader( Db, propertyKey, propertyValue, start, readerDelay );

			  ExecutorService executor = Executors.newFixedThreadPool( 2 );
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> readResult;
			  Future<object> readResult;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.concurrent.Future<?> writeResult;
			  Future<object> writeResult;
			  try
			  {
					writeResult = executor.submit( writer );
					readResult = executor.submit( reader );

					start.set( true );
			  }
			  finally
			  {
					executor.shutdown();
					executor.awaitTermination( 20, TimeUnit.SECONDS );
			  }

			  assertNull( writeResult.get() );
			  assertNull( readResult.get() );
		 }

		 private class Writer : ThreadStart
		 {
			  internal readonly GraphDatabaseService Db;
			  internal readonly string PropertyKey;
			  internal readonly string PropertyValue;
			  internal readonly AtomicBoolean Start;

			  internal Writer( GraphDatabaseService db, string propertyKey, string propertyValue, AtomicBoolean start )
			  {
					this.Db = db;
					this.PropertyKey = propertyKey;
					this.PropertyValue = propertyValue;
					this.Start = start;
			  }

			  public override void Run()
			  {
					while ( !Start.get() )
					{
						 // spin
					}
					using ( Transaction tx = Db.beginTx() )
					{
						 Node node = Db.createNode();
						 node.SetProperty( PropertyKey, PropertyValue );
						 Db.index().forNodes(INDEX_NAME).add(node, PropertyKey, PropertyValue);
						 tx.Success();
					}
			  }
		 }

		 private class Reader : ThreadStart
		 {
			  internal readonly GraphDatabaseService Db;
			  internal readonly string PropertyKey;
			  internal readonly string PropertyValue;
			  internal readonly AtomicBoolean Start;
			  internal readonly int Delay;

			  internal Reader( GraphDatabaseService db, string propertyKey, string propertyValue, AtomicBoolean start, int delay )
			  {
					this.Db = db;
					this.PropertyKey = propertyKey;
					this.PropertyValue = propertyValue;
					this.Start = start;
					this.Delay = delay;
			  }

			  public override void Run()
			  {
					while ( !Start.get() )
					{
						 // spin
					}
					Sleep();
					using ( Transaction tx = Db.beginTx() )
					{
						 // it is acceptable to either see a node with correct property or not see it at all
						 Node node = Db.index().forNodes(INDEX_NAME).get(PropertyKey, PropertyValue).Single;
						 if ( node != null )
						 {
							  assertEquals( PropertyValue, node.GetProperty( PropertyKey ) );
						 }
						 tx.Success();
					}
			  }

			  internal virtual void Sleep()
			  {
					try
					{
						 Thread.Sleep( Delay );
					}
					catch ( InterruptedException e )
					{
						 Thread.CurrentThread.Interrupt();
						 throw new Exception( e );
					}
			  }
		 }
	}

}