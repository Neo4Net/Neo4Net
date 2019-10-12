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
namespace Org.Neo4j.Kernel.impl.store
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Node = Org.Neo4j.Graphdb.Node;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using RecordStorageEngine = Org.Neo4j.Kernel.impl.storageengine.impl.recordstorage.RecordStorageEngine;
	using GraphDatabaseAPI = Org.Neo4j.Kernel.@internal.GraphDatabaseAPI;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Org.Neo4j.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.count;

	public class RelationshipGroupStoreIT
	{
		 private const int RELATIONSHIP_COUNT = 20;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.ImpermanentDatabaseRule().withSetting(org.neo4j.graphdb.factory.GraphDatabaseSettings.dense_node_threshold, "1");
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