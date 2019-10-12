/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.causalclustering.scenarios
{
	using Org.Neo4j.causalclustering.discovery;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;

	public class SampleData
	{
		 private static readonly Label _label = label( "ExampleNode" );
		 private const string PROPERTY_KEY = "prop";

		 private SampleData()
		 {
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public static void createSomeData(int items, org.neo4j.causalclustering.discovery.Cluster<?> cluster) throws Exception
		 public static void CreateSomeData<T1>( int items, Cluster<T1> cluster )
		 {
			  for ( int i = 0; i < items; i++ )
			  {
					cluster.CoreTx((db, tx) =>
					{
					 Node node = Db.createNode( _label );
					 node.setProperty( "foobar", "baz_bat" );
					 tx.success();
					});
			  }
		 }

		 internal static void CreateData( GraphDatabaseService db, int size )
		 {
			  for ( int i = 0; i < size; i++ )
			  {
					Node node1 = Db.createNode( _label );
					Node node2 = Db.createNode( _label );

					node1.SetProperty( PROPERTY_KEY, "svej" + i );
					node2.SetProperty( "tjabba", "tjena" );
					node1.SetProperty( "foobar", "baz_bat" );
					node2.SetProperty( "foobar", "baz_bat" );

					Relationship rel = node1.CreateRelationshipTo( node2, RelationshipType.withName( "halla" ) );
					rel.SetProperty( "this", "that" );
			  }
		 }

		 internal static void CreateSchema( GraphDatabaseService db )
		 {
			  Db.schema().constraintFor(_label).assertPropertyIsUnique(PROPERTY_KEY).create();
		 }
	}

}