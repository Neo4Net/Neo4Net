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
namespace Neo4Net.Index.impl.lucene.@explicit
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Node = Neo4Net.GraphDb.Node;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Neo4Net.GraphDb.Index;
	using ReadableRelationshipIndex = Neo4Net.GraphDb.Index.ReadableRelationshipIndex;
	using MyRelTypes = Neo4Net.Kernel.impl.MyRelTypes;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.Iterators.count;

	public class AutoIndexOperationsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.DatabaseRule db = new Neo4Net.test.rule.EmbeddedDatabaseRule().withSetting(Neo4Net.graphdb.factory.GraphDatabaseSettings.relationship_keys_indexable, "Type").withSetting(Neo4Net.graphdb.factory.GraphDatabaseSettings.relationship_auto_indexing, "true");
		 public readonly DatabaseRule Db = new EmbeddedDatabaseRule().withSetting(GraphDatabaseSettings.relationship_keys_indexable, "Type").withSetting(GraphDatabaseSettings.relationship_auto_indexing, "true");

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotSeeDeletedRelationshipWhenQueryingWithStartAndEndNode()
		 public virtual void ShouldNotSeeDeletedRelationshipWhenQueryingWithStartAndEndNode()
		 {
			  RelationshipType type = MyRelTypes.TEST;
			  long startId;
			  long endId;
			  Relationship rel;
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node start = Db.createNode();
					Node end = Db.createNode();
					startId = start.Id;
					endId = end.Id;
					rel = start.CreateRelationshipTo( end, type );
					rel.SetProperty( "Type", type.Name() );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					ReadableRelationshipIndex autoRelationshipIndex = Db.index().RelationshipAutoIndexer.AutoIndex;
					Node start = Db.getNodeById( startId );
					Node end = Db.getNodeById( endId );
					IndexHits<Relationship> hits = autoRelationshipIndex.Get( "Type", type.Name(), start, end );
					assertEquals( 1, count( hits ) );
					assertEquals( 1, hits.Size() );
					rel.Delete();
					autoRelationshipIndex = Db.index().RelationshipAutoIndexer.AutoIndex;
					hits = autoRelationshipIndex.Get( "Type", type.Name(), start, end );
					assertEquals( 0, count( hits ) );
					assertEquals( 0, hits.Size() );
					tx.Success();
			  }
		 }
	}

}