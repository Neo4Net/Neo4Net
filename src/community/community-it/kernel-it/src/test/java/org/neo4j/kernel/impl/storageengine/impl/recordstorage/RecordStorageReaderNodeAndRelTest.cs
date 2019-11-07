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
	using Test = org.junit.Test;

	using Transaction = Neo4Net.GraphDb.Transaction;
	using StorageNodeCursor = Neo4Net.Kernel.Api.StorageEngine.StorageNodeCursor;
	using StorageRelationshipScanCursor = Neo4Net.Kernel.Api.StorageEngine.StorageRelationshipScanCursor;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.RelationshipType.withName;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.MapUtil.map;

	/// <summary>
	/// Test reading committed node and relationships from disk.
	/// </summary>
	public class RecordStorageReaderNodeAndRelTest : RecordStorageReaderTestBase
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTellIfNodeExists()
		 public virtual void ShouldTellIfNodeExists()
		 {
			  // Given
			  long created = CreateLabeledNode( Db, map() ).Id;
			  long createdAndRemoved = CreateLabeledNode( Db, map() ).Id;
			  long neverExisted = createdAndRemoved + 99;

			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.getNodeById( createdAndRemoved ).delete();
					tx.Success();
			  }

			  // When & then
			  assertTrue( NodeExists( created ) );
			  assertFalse( NodeExists( createdAndRemoved ) );
			  assertFalse( NodeExists( neverExisted ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldTellIfRelExists()
		 public virtual void ShouldTellIfRelExists()
		 {
			  // Given
			  long node = CreateLabeledNode( Db, map() ).Id;
			  long created;
			  long createdAndRemoved;
			  long neverExisted;

			  using ( Transaction tx = Db.beginTx() )
			  {
					created = Db.createNode().createRelationshipTo(Db.createNode(), withName("Banana")).Id;
					createdAndRemoved = Db.createNode().createRelationshipTo(Db.createNode(), withName("Banana")).Id;
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.getRelationshipById( createdAndRemoved ).delete();
					tx.Success();
			  }

			  neverExisted = created + 99;

			  // When & then
			  assertTrue( RelationshipExists( node ) );
			  assertFalse( RelationshipExists( createdAndRemoved ) );
			  assertFalse( RelationshipExists( neverExisted ) );
		 }

		 private bool NodeExists( long id )
		 {
			  using ( StorageNodeCursor node = StorageReader.allocateNodeCursor() )
			  {
					node.Single( id );
					return node.Next();
			  }
		 }

		 private bool RelationshipExists( long id )
		 {
			  using ( StorageRelationshipScanCursor relationship = StorageReader.allocateRelationshipScanCursor() )
			  {
					relationship.Single( id );
					return relationship.Next();
			  }
		 }
	}

}