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
namespace Neo4Net.Kernel
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using TransactionData = Neo4Net.Graphdb.@event.TransactionData;
	using Neo4Net.Graphdb.@event;
	using Iterables = Neo4Net.Helpers.Collection.Iterables;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.hasProperty;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.mockito.matcher.Neo4jMatchers.inTx;

	public class TestTransactionEventDeadlocks
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.DatabaseRule database = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public DatabaseRule Database = new ImpermanentDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void canAvoidDeadlockThatWouldHappenIfTheRelationshipTypeCreationTransactionModifiedData()
		 public virtual void CanAvoidDeadlockThatWouldHappenIfTheRelationshipTypeCreationTransactionModifiedData()
		 {
			  GraphDatabaseService graphdb = Database.GraphDatabaseAPI;
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
//ORIGINAL LINE: @SuppressWarnings("boxing") @Override public Void beforeCommit(org.neo4j.graphdb.event.TransactionData data)
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