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
namespace Neo4Net.Kernel.Impl.@event
{
	using Test = org.junit.Test;


	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using TransactionData = Neo4Net.Graphdb.@event.TransactionData;
	using Neo4Net.Graphdb.@event;
	using Neo4Net.Graphdb.index;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.single;

	public class TestTransactionEventsWithIndexes : TestTransactionEvents
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void nodeCanBeExplicitIndexedInBeforeCommit()
		 public virtual void NodeCanBeExplicitIndexedInBeforeCommit()
		 {
			  // Given we have an explicit index...
			  GraphDatabaseService db = DbRule.GraphDatabaseAPI;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.index.Index<org.neo4j.graphdb.Node> index;
			  Index<Node> index;
			  using ( Transaction tx = Db.beginTx() )
			  {
					index = Db.index().forNodes("index");
					tx.Success();
			  }

			  // ... and a transaction event handler that likes to add nodes to that index
			  Db.registerTransactionEventHandler( new TransactionEventHandlerAnonymousInnerClass( this, index ) );

			  // When we create a node...
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(10, TimeUnit.SECONDS);
					Node node = Db.createNode();
					node.SetProperty( "random", 42 );
					tx.Success();
			  }

			  // Then we should be able to look it up through the index.
			  using ( Transaction ignore = Db.beginTx() )
			  {
					Node node = single( index.get( "key", "value" ) );
					assertThat( node.GetProperty( "random" ), @is( 42 ) );
			  }
		 }

		 private class TransactionEventHandlerAnonymousInnerClass : TransactionEventHandler<object>
		 {
			 private readonly TestTransactionEventsWithIndexes _outerInstance;

			 private Index<Node> _index;

			 public TransactionEventHandlerAnonymousInnerClass( TestTransactionEventsWithIndexes outerInstance, Index<Node> index )
			 {
				 this.outerInstance = outerInstance;
				 this._index = index;
			 }

			 public object beforeCommit( TransactionData data )
			 {
				  IEnumerator<Node> nodes = data.CreatedNodes().GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
				  if ( nodes.hasNext() )
				  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						Node node = nodes.next();
						_index.add( node, "key", "value" );
				  }
				  return null;
			 }

			 public void afterCommit( TransactionData data, object state )
			 {
			 }

			 public void afterRollback( TransactionData data, object state )
			 {
			 }
		 }
	}

}