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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Neo4Net.Graphdb;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Neo4Net.Test.OtherThreadExecutor;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;
	using Neo4Net.Test.rule.concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class NestedIndexReadersIT
	{
		 private const int NODE_PER_ID = 3;
		 private const int IDS = 5;
		 private static readonly Label _label = Label.label( "Label" );
		 private const string KEY = "key";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.ImpermanentDatabaseRule db = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public readonly ImpermanentDatabaseRule Db = new ImpermanentDatabaseRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.concurrent.OtherThreadRule<Void> t2 = new org.neo4j.test.rule.concurrent.OtherThreadRule<>();
		 public readonly OtherThreadRule<Void> T2 = new OtherThreadRule<Void>();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadCorrectResultsFromMultipleNestedReaders()
		 public virtual void ShouldReadCorrectResultsFromMultipleNestedReaders()
		 {
			  // given
			  CreateIndex();
			  using ( Transaction tx = Db.beginTx() )
			  {
					for ( int i = 0; i < NODE_PER_ID; i++ )
					{
						 CreateRoundOfNodes();
					}
					tx.Success();
			  }

			  // when
			  using ( Transaction tx = Db.beginTx() )
			  {
					// opening all the index readers
					IList<ResourceIterator<Node>> iterators = new List<ResourceIterator<Node>>();
					for ( int id = 0; id < IDS; id++ )
					{
						 iterators.Add( Db.findNodes( _label, KEY, id ) );
					}

					// then iterating over them interleaved should yield all the expected results each
					for ( int i = 0; i < NODE_PER_ID; i++ )
					{
						 AssertRoundOfNodes( iterators );
					}

					foreach ( ResourceIterator<Node> reader in iterators )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 assertFalse( reader.hasNext() );
						 reader.Close();
					}

					tx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReadCorrectResultsFromMultipleNestedReadersWhenConcurrentWriteHappens() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReadCorrectResultsFromMultipleNestedReadersWhenConcurrentWriteHappens()
		 {
			  // given
			  CreateIndex();
			  using ( Transaction tx = Db.beginTx() )
			  {
					for ( int id = 0; id < IDS; id++ )
					{
						 for ( int i = 0; i < NODE_PER_ID; i++ )
						 {
							  Db.createNode( _label ).setProperty( KEY, id );
						 }
					}
					tx.Success();
			  }

			  // when
			  using ( Transaction tx = Db.beginTx() )
			  {
					// opening all the index readers
					IList<ResourceIterator<Node>> iterators = new List<ResourceIterator<Node>>();
					for ( int id = 0; id < IDS; id++ )
					{
						 iterators.Add( Db.findNodes( _label, KEY, id ) );
					}

					// then iterating over them interleaved should yield all the expected results each
					for ( int i = 0; i < NODE_PER_ID; i++ )
					{
						 AssertRoundOfNodes( iterators );

						 if ( i % 2 == 1 )
						 {
							  // will be triggered on i == 1
							  T2.execute( NodeCreator() ).get();
						 }
					}

					AssertRoundOfNodes( iterators );

					foreach ( ResourceIterator<Node> reader in iterators )
					{
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 assertFalse( reader.hasNext() );
						 reader.Close();
					}

					tx.Success();
			  }
		 }

		 private void CreateRoundOfNodes()
		 {
			  for ( int id = 0; id < IDS; id++ )
			  {
					Db.createNode( _label ).setProperty( KEY, id );
			  }
		 }

		 private void AssertRoundOfNodes( IList<ResourceIterator<Node>> iterators )
		 {
			  for ( int id = 0; id < IDS; id++ )
			  {
					AssertNode( iterators[id], id );
			  }
		 }

		 private WorkerCommand<Void, Void> NodeCreator()
		 {
			  return state =>
			  {
				using ( Transaction tx = Db.beginTx() )
				{
					 CreateRoundOfNodes();
					 tx.success();
				}
				return null;
			  };
		 }

		 private void AssertNode( ResourceIterator<Node> reader, int id )
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  assertTrue( reader.hasNext() );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  Node node = reader.next();
			  assertTrue( node.HasLabel( _label ) );
			  assertEquals( "Expected node " + node + " (returned by index reader) to have 'id' property w/ value " + id, id, node.GetProperty( KEY ) );
		 }

		 private void CreateIndex()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().indexFor(_label).on(KEY).create();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(10, SECONDS);
					tx.Success();
			  }
		 }
	}

}