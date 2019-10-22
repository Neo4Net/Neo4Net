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
namespace Neo4Net.Kernel.impl.storageengine.impl.recordstorage
{
	using LongIterator = org.eclipse.collections.api.iterator.LongIterator;
	using Test = org.junit.Test;

	using PrimitiveLongCollections = Neo4Net.Collections.PrimitiveLongCollections;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using StorageNodeCursor = Neo4Net.Storageengine.Api.StorageNodeCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.eclipse.collections.impl.set.mutable.primitive.LongHashSet.newSetWith;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.Iterators.asSet;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.helpers.collection.MapUtil.map;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.Neo4NetMatchers.containsOnly;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.mockito.matcher.Neo4NetMatchers.getPropertyKeys;

	/// <summary>
	/// Test read access to committed label data.
	/// </summary>
	public class RecordStorageReaderLabelTest : RecordStorageReaderTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToListLabelsForNode()
		 public virtual void ShouldBeAbleToListLabelsForNode()
		 {
			  // GIVEN
			  long nodeId;
			  int labelId1;
			  int labelId2;
			  using ( Transaction tx = Db.beginTx() )
			  {
					nodeId = Db.createNode( Label1, Label2 ).Id;
					string labelName1 = Label1.name();
					string labelName2 = Label2.name();
					labelId1 = LabelId( Label.label( labelName1 ) );
					labelId2 = LabelId( Label.label( labelName2 ) );
					tx.Success();
			  }

			  // THEN
			  StorageNodeCursor nodeCursor = StorageReader.allocateNodeCursor();
			  nodeCursor.Single( nodeId );
			  assertTrue( nodeCursor.Next() );
			  assertEquals( newSetWith( labelId1, labelId2 ), newSetWith( nodeCursor.Labels() ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void labelsShouldNotLeakOutAsProperties()
		 public virtual void LabelsShouldNotLeakOutAsProperties()
		 {
			  // GIVEN
			  Node node = CreateLabeledNode( Db, map( "name", "Node" ), Label1 );

			  // WHEN THEN
			  assertThat( getPropertyKeys( Db, node ), containsOnly( "name" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnAllNodesWithLabel()
		 public virtual void ShouldReturnAllNodesWithLabel()
		 {
			  // GIVEN
			  Node node1 = CreateLabeledNode( Db, map( "name", "First", "age", 1L ), Label1 );
			  Node node2 = CreateLabeledNode( Db, map( "type", "Node", "count", 10 ), Label1, Label2 );
			  int labelId1 = LabelId( Label1 );
			  int labelId2 = LabelId( Label2 );

			  // WHEN
			  LongIterator nodesForLabel1 = StorageReader.nodesGetForLabel( labelId1 );
			  LongIterator nodesForLabel2 = StorageReader.nodesGetForLabel( labelId2 );

			  // THEN
			  assertEquals( asSet( node1.Id, node2.Id ), PrimitiveLongCollections.toSet( nodesForLabel1 ) );
			  assertEquals( asSet( node2.Id ), PrimitiveLongCollections.toSet( nodesForLabel2 ) );
		 }
	}

}