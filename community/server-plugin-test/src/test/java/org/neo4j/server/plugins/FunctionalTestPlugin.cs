using System.Collections.Generic;

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
namespace Org.Neo4j.Server.plugins
{

	using GraphAlgoFactory = Org.Neo4j.Graphalgo.GraphAlgoFactory;
	using Org.Neo4j.Graphalgo;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using NotFoundException = Org.Neo4j.Graphdb.NotFoundException;
	using Path = Org.Neo4j.Graphdb.Path;
	using PathExpanders = Org.Neo4j.Graphdb.PathExpanders;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Org.Neo4j.Graphdb;
	using Org.Neo4j.Graphdb;
	using Transaction = Org.Neo4j.Graphdb.Transaction;

	[Description("Here you can describe your plugin. It will show up in the description of the methods.")]
	public class FunctionalTestPlugin : ServerPlugin
	{
		 public const string CREATE_NODE = "createNode";
		 public const string GET_CONNECTED_NODES = "connected_nodes";
		 internal static string _string;
		 internal static sbyte? _byte;
		 internal static char? _character;
		 internal static int? _integer;
		 internal static short? _short;
		 internal static long? _long;
		 internal static float? _float;
		 internal static double? _double;
		 internal static bool? _boolean;
		 internal static long? Optional;
		 internal static ISet<string> StringSet;
		 internal static IList<string> StringList;
		 internal static string[] StringArray;
		 public static int[] IntArray;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Name(GET_CONNECTED_NODES) @PluginTarget(org.neo4j.graphdb.Node.class) public Iterable<org.neo4j.graphdb.Node> getAllConnectedNodes(@Source Node start)
		 [Name(GET_CONNECTED_NODES), PluginTarget(Org.Neo4j.Graphdb.Node.class)]
		 public virtual IEnumerable<Node> GetAllConnectedNodes( Node start )
		 {
			  List<Node> nodes = new List<Node>();
			  using ( Transaction tx = start.GraphDatabase.beginTx() )
			  {
					foreach ( Relationship rel in start.Relationships )
					{
						 nodes.Add( rel.GetOtherNode( start ) );
					}

					tx.Success();
			  }
			  return nodes;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.neo4j.graphdb.Node.class) public Iterable<org.neo4j.graphdb.Relationship> getRelationshipsBetween(@Source final org.neo4j.graphdb.Node start, @Parameter(name = "other") final org.neo4j.graphdb.Node end)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 [PluginTarget(Org.Neo4j.Graphdb.Node.class)]
		 public virtual IEnumerable<Relationship> GetRelationshipsBetween( Node start, Node end )
		 {
			  IList<Relationship> result = new List<Relationship>();
			  using ( Transaction tx = start.GraphDatabase.beginTx() )
			  {
					foreach ( Relationship relationship in start.Relationships )
					{
						 if ( relationship.GetOtherNode( start ).Equals( end ) )
						 {
							  result.Add( relationship );
						 }
					}
					tx.Success();
			  }
			  return result;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.neo4j.graphdb.Node.class) public Iterable<org.neo4j.graphdb.Relationship> createRelationships(@Source Node start, @Parameter(name = "type") org.neo4j.graphdb.RelationshipType type, @Parameter(name = "nodes") Iterable<org.neo4j.graphdb.Node> nodes)
		 [PluginTarget(Org.Neo4j.Graphdb.Node.class)]
		 public virtual IEnumerable<Relationship> CreateRelationships( Node start, RelationshipType type, IEnumerable<Node> nodes )
		 {
			  IList<Relationship> result = new List<Relationship>();
			  using ( Transaction tx = start.GraphDatabase.beginTx() )
			  {
					foreach ( Node end in nodes )
					{
						 result.Add( start.createRelationshipTo( end, type ) );
					}
					tx.Success();
			  }
			  return result;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.neo4j.graphdb.Node.class) public org.neo4j.graphdb.Node getThisNodeOrById(@Source Node start, @Parameter(name = "id", optional = true) System.Nullable<long> id)
		 [PluginTarget(Org.Neo4j.Graphdb.Node.class)]
		 public virtual Node GetThisNodeOrById( Node start, long? id )
		 {
			  Optional = id;

			  if ( id == null )
			  {
					return start;
			  }

			  using ( Transaction tx = start.GraphDatabase.beginTx() )
			  {
					Node node = start.GraphDatabase.getNodeById( id );

					tx.Success();
					return node;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.neo4j.graphdb.GraphDatabaseService.class) public org.neo4j.graphdb.Node createNode(@Source GraphDatabaseService db)
		 [PluginTarget(Org.Neo4j.Graphdb.GraphDatabaseService.class)]
		 public virtual Node CreateNode( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();

					tx.Success();
					return node;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.neo4j.graphdb.GraphDatabaseService.class) public org.neo4j.graphdb.Node methodWithIntParam(@Source GraphDatabaseService db, @Parameter(name = "id", optional = false) int id)
		 [PluginTarget(Org.Neo4j.Graphdb.GraphDatabaseService.class)]
		 public virtual Node MethodWithIntParam( GraphDatabaseService db, int id )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.getNodeById( id );

					tx.Success();
					return node;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.neo4j.graphdb.Relationship.class) public Iterable<org.neo4j.graphdb.Node> methodOnRelationship(@Source Relationship rel)
		 [PluginTarget(Org.Neo4j.Graphdb.Relationship.class)]
		 public virtual IEnumerable<Node> MethodOnRelationship( Relationship rel )
		 {
			  using ( Transaction tx = rel.GraphDatabase.beginTx() )
			  {
					IList<Node> nodes = Arrays.asList( rel.Nodes );

					tx.Success();
					return nodes;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.neo4j.graphdb.GraphDatabaseService.class) public org.neo4j.graphdb.Node methodWithAllParams(@Source GraphDatabaseService db, @Parameter(name = "id", optional = false) String a, @Parameter(name = "id2", optional = false) System.Nullable<sbyte> b, @Parameter(name = "id3", optional = false) System.Nullable<char> c, @Parameter(name = "id4", optional = false) System.Nullable<short> d, @Parameter(name = "id5", optional = false) System.Nullable<int> e, @Parameter(name = "id6", optional = false) System.Nullable<long> f, @Parameter(name = "id7", optional = false) System.Nullable<float> g, @Parameter(name = "id8", optional = false) System.Nullable<double> h, @Parameter(name = "id9", optional = false) System.Nullable<bool> i)
		 [PluginTarget(Org.Neo4j.Graphdb.GraphDatabaseService.class)]
		 public virtual Node MethodWithAllParams( GraphDatabaseService db, string a, sbyte? b, char? c, short? d, int? e, long? f, float? g, double? h, bool? i )
		 {
			  _string = a;
			  _byte = b;
			  _character = c;
			  _short = d;
			  _integer = e;
			  _long = f;
			  _float = g;
			  _double = h;
			  _boolean = i;

			  return GetOrCreateANode( db );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.neo4j.graphdb.GraphDatabaseService.class) public org.neo4j.graphdb.Node methodWithSet(@Source GraphDatabaseService db, @Parameter(name = "strings", optional = false) java.util.Set<String> params)
		 [PluginTarget(Org.Neo4j.Graphdb.GraphDatabaseService.class)]
		 public virtual Node MethodWithSet( GraphDatabaseService db, ISet<string> @params )
		 {
			  StringSet = @params;
			  return GetOrCreateANode( db );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.neo4j.graphdb.GraphDatabaseService.class) public org.neo4j.graphdb.Node methodWithList(@Source GraphDatabaseService db, @Parameter(name = "strings", optional = false) java.util.List<String> params)
		 [PluginTarget(Org.Neo4j.Graphdb.GraphDatabaseService.class)]
		 public virtual Node MethodWithList( GraphDatabaseService db, IList<string> @params )
		 {
			  StringList = @params;
			  return GetOrCreateANode( db );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.neo4j.graphdb.GraphDatabaseService.class) public org.neo4j.graphdb.Node methodWithListAndInt(@Source GraphDatabaseService db, @Parameter(name = "strings", optional = false) java.util.List<String> params, @Parameter(name = "count", optional = false) int i)
		 [PluginTarget(Org.Neo4j.Graphdb.GraphDatabaseService.class)]
		 public virtual Node MethodWithListAndInt( GraphDatabaseService db, IList<string> @params, int i )
		 {
			  StringList = @params;
			  _integer = i;
			  return GetOrCreateANode( db );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.neo4j.graphdb.GraphDatabaseService.class) public org.neo4j.graphdb.Node methodWithArray(@Source GraphDatabaseService db, @Parameter(name = "strings", optional = false) String[] params)
		 [PluginTarget(Org.Neo4j.Graphdb.GraphDatabaseService.class)]
		 public virtual Node MethodWithArray( GraphDatabaseService db, string[] @params )
		 {
			  StringArray = @params;
			  return GetOrCreateANode( db );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.neo4j.graphdb.GraphDatabaseService.class) public org.neo4j.graphdb.Node methodWithIntArray(@Source GraphDatabaseService db, @Parameter(name = "ints", optional = false) int[] params)
		 [PluginTarget(Org.Neo4j.Graphdb.GraphDatabaseService.class)]
		 public virtual Node MethodWithIntArray( GraphDatabaseService db, int[] @params )
		 {
			  IntArray = @params;
			  return GetOrCreateANode( db );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.neo4j.graphdb.GraphDatabaseService.class) public org.neo4j.graphdb.Node methodWithOptionalArray(@Source GraphDatabaseService db, @Parameter(name = "ints", optional = true) int[] params)
		 [PluginTarget(Org.Neo4j.Graphdb.GraphDatabaseService.class)]
		 public virtual Node MethodWithOptionalArray( GraphDatabaseService db, int[] @params )
		 {
			  IntArray = @params;
			  return GetOrCreateANode( db );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.neo4j.graphdb.Node.class) public org.neo4j.graphdb.Path pathToReference(@Source Node me)
		 [PluginTarget(Org.Neo4j.Graphdb.Node.class)]
		 public virtual Path PathToReference( Node me )
		 {
			  PathFinder<Path> finder = GraphAlgoFactory.shortestPath( PathExpanders.allTypesAndDirections(), 6 );
			  using ( Transaction tx = me.GraphDatabase.beginTx() )
			  {
					Node other;
					if ( me.hasRelationship( RelationshipType.withName( "friend" ) ) )
					{
						 ResourceIterable<Relationship> relationships = ( ResourceIterable<Relationship> ) me.getRelationships( RelationshipType.withName( "friend" ) );
						 using ( ResourceIterator<Relationship> resourceIterator = relationships.GetEnumerator() )
						 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							  other = resourceIterator.next().getOtherNode(me);
						 }
					}
					else
					{
						 other = me.GraphDatabase.createNode();
					}
					Path path = finder.FindSinglePath( other, me );

					tx.Success();
					return path;
			  }
		 }

		 private Node GetOrCreateANode( GraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node;
					try
					{
						 node = Db.getNodeById( 0L );
					}
					catch ( NotFoundException )
					{
						 node = Db.createNode();
					}
					tx.Success();
					return node;
			  }
		 }

	}

}