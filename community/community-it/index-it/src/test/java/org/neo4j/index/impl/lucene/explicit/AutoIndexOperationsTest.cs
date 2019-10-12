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
namespace Org.Neo4j.Index.impl.lucene.@explicit
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Org.Neo4j.Graphdb.index;
	using ReadableRelationshipIndex = Org.Neo4j.Graphdb.index.ReadableRelationshipIndex;
	using MyRelTypes = Org.Neo4j.Kernel.impl.MyRelTypes;
	using DatabaseRule = Org.Neo4j.Test.rule.DatabaseRule;
	using EmbeddedDatabaseRule = Org.Neo4j.Test.rule.EmbeddedDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.count;

	public class AutoIndexOperationsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.EmbeddedDatabaseRule().withSetting(org.neo4j.graphdb.factory.GraphDatabaseSettings.relationship_keys_indexable, "Type").withSetting(org.neo4j.graphdb.factory.GraphDatabaseSettings.relationship_auto_indexing, "true");
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