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
namespace Neo4Net.Kernel.impl.store
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Node = Neo4Net.GraphDb.Node;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using RecordStorageEngine = Neo4Net.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using GraphDatabaseAPI = Neo4Net.Kernel.Internal.GraphDatabaseAPI;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterables.count;

	public class RelationshipGroupStoreIT
	{
		 private const int RELATIONSHIP_COUNT = 20;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.DatabaseRule db = new Neo4Net.test.rule.ImpermanentDatabaseRule().withSetting(Neo4Net.graphdb.factory.GraphDatabaseSettings.dense_node_threshold, "1");
		 public readonly DatabaseRule Db = new ImpermanentDatabaseRule().withSetting(GraphDatabaseSettings.dense_node_threshold, "1");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateAllTheseRelationshipTypes()
		 public virtual void ShouldCreateAllTheseRelationshipTypes()
		 {
			  ShiftHighId( Db );

			  Node node;
			  using ( Transaction tx = Db.beginTx() )
			  {
					node = Db.createNode();
					for ( int i = 0; i < RELATIONSHIP_COUNT; i++ )
					{
						 node.CreateRelationshipTo( Db.createNode(), Type(i) );
					}
					tx.Success();
			  }

			  using ( Transaction ignored = Db.beginTx() )
			  {
					for ( int i = 0; i < RELATIONSHIP_COUNT; i++ )
					{
						 assertEquals( "Should be possible to get relationships of type with id in unsigned short range.", 1, count( node.GetRelationships( Type( i ) ) ) );
					}
			  }
		 }

		 private void ShiftHighId( GraphDatabaseAPI db )
		 {
			  RecordStorageEngine storageEngine = Db.DependencyResolver.resolveDependency( typeof( RecordStorageEngine ) );
			  NeoStores neoStores = storageEngine.TestAccessNeoStores();
			  neoStores.RelationshipTypeTokenStore.HighId = short.MaxValue - RELATIONSHIP_COUNT / 2;
		 }

		 private RelationshipType Type( int i )
		 {
			  return RelationshipType.withName( "TYPE_" + i );
		 }
	}

}