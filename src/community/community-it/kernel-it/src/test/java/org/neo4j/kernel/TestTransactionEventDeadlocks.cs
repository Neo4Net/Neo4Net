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
namespace Neo4Net.Kernel
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using TransactionData = Neo4Net.GraphDb.Events.TransactionData;
	using Neo4Net.GraphDb.Events;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.Neo4NetMatchers.hasProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.Neo4NetMatchers.inTx;

	public class TestTransactionEventDeadlocks
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.DatabaseRule database = new org.Neo4Net.test.rule.ImpermanentDatabaseRule();
		 public DatabaseRule Database = new ImpermanentDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canAvoidDeadlockThatWouldHappenIfTheRelationshipTypeCreationTransactionModifiedData()
		 public virtual void CanAvoidDeadlockThatWouldHappenIfTheRelationshipTypeCreationTransactionModifiedData()
		 {
			  IGraphDatabaseService graphdb = Database.GraphDatabaseAPI;
			  Node node = null;
			  using ( Transaction tx = graphdb.BeginTx() )
			  {
					node = graphdb.CreateNode();
					node.SetProperty( "counter", 0L );
					tx.Success();
			  }

			  graphdb.RegisterTransactionEventHandler( new RelationshipCounterTransactionEventHandler( node ) );

			  using ( Transaction tx = graphdb.BeginTx() )
			  {
					node.SetProperty( "state", "not broken yet" );
					node.CreateRelationshipTo( graphdb.CreateNode(), RelationshipType.withName("TEST") );
					node.RemoveProperty( "state" );
					tx.Success();
			  }

			  assertThat( node, inTx( graphdb, hasProperty( "counter" ).withValue( 1L ) ) );
		 }

		 private class RelationshipCounterTransactionEventHandler : TransactionEventHandler<Void>
		 {
			  internal readonly Node Node;

			  internal RelationshipCounterTransactionEventHandler( Node node )
			  {
					this.Node = node;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("boxing") @Override public Void beforeCommit(org.Neo4Net.graphdb.event.TransactionData data)
			  public override Void BeforeCommit( TransactionData data )
			  {
					if ( Iterables.count( data.CreatedRelationships() ) == 0 )
					{
						 return null;
					}

					Node.setProperty( "counter", ( ( long? ) Node.removeProperty( "counter" ) ) + 1 );
					return null;
			  }

			  public override void AfterCommit( TransactionData data, Void state )
			  {
					// nothing
			  }

			  public override void AfterRollback( TransactionData data, Void state )
			  {
					// nothing
			  }
		 }
	}

}