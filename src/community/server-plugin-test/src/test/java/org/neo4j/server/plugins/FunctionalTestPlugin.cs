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
namespace Neo4Net.Server.plugins
{

	using GraphAlgoFactory = Neo4Net.Graphalgo.GraphAlgoFactory;
	using Neo4Net.Graphalgo;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using NotFoundException = Neo4Net.GraphDb.NotFoundException;
	using Path = Neo4Net.GraphDb.Path;
	using PathExpanders = Neo4Net.GraphDb.PathExpanders;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Neo4Net.GraphDb;
	using Neo4Net.GraphDb;
	using Transaction = Neo4Net.GraphDb.Transaction;

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
//ORIGINAL LINE: @Name(GET_CONNECTED_NODES) @PluginTarget(org.Neo4Net.graphdb.Node.class) public Iterable<org.Neo4Net.graphdb.Node> getAllConnectedNodes(@Source Node start)
		 [Name(GET_CONNECTED_NODES), PluginTarget(Neo4Net.GraphDb.Node.class)]
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
//ORIGINAL LINE: @PluginTarget(org.Neo4Net.graphdb.Node.class) public Iterable<org.Neo4Net.graphdb.Relationship> getRelationshipsBetween(@Source final org.Neo4Net.graphdb.Node start, @Parameter(name = "other") final org.Neo4Net.graphdb.Node end)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 [PluginTarget(Neo4Net.GraphDb.Node.class)]
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
//ORIGINAL LINE: @PluginTarget(org.Neo4Net.graphdb.Node.class) public Iterable<org.Neo4Net.graphdb.Relationship> createRelationships(@Source Node start, @Parameter(name = "type") org.Neo4Net.graphdb.RelationshipType type, @Parameter(name = "nodes") Iterable<org.Neo4Net.graphdb.Node> nodes)
		 [PluginTarget(Neo4Net.GraphDb.Node.class)]
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
//ORIGINAL LINE: @PluginTarget(org.Neo4Net.graphdb.Node.class) public org.Neo4Net.graphdb.Node getThisNodeOrById(@Source Node start, @Parameter(name = "id", optional = true) System.Nullable<long> id)
		 [PluginTarget(Neo4Net.GraphDb.Node.class)]
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
//ORIGINAL LINE: @PluginTarget(org.Neo4Net.graphdb.GraphDatabaseService.class) public org.Neo4Net.graphdb.Node createNode(@Source IGraphDatabaseService db)
		 [PluginTarget(Neo4Net.GraphDb.GraphDatabaseService.class)]
		 public virtual Node CreateNode( IGraphDatabaseService db )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.createNode();

					tx.Success();
					return node;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.Neo4Net.graphdb.GraphDatabaseService.class) public org.Neo4Net.graphdb.Node methodWithIntParam(@Source IGraphDatabaseService db, @Parameter(name = "id", optional = false) int id)
		 [PluginTarget(Neo4Net.GraphDb.GraphDatabaseService.class)]
		 public virtual Node MethodWithIntParam( IGraphDatabaseService db, int id )
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.getNodeById( id );

					tx.Success();
					return node;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.Neo4Net.graphdb.Relationship.class) public Iterable<org.Neo4Net.graphdb.Node> methodOnRelationship(@Source Relationship rel)
		 [PluginTarget(Neo4Net.GraphDb.Relationship.class)]
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
//ORIGINAL LINE: @PluginTarget(org.Neo4Net.graphdb.GraphDatabaseService.class) public org.Neo4Net.graphdb.Node methodWithAllParams(@Source IGraphDatabaseService db, @Parameter(name = "id", optional = false) String a, @Parameter(name = "id2", optional = false) System.Nullable<sbyte> b, @Parameter(name = "id3", optional = false) System.Nullable<char> c, @Parameter(name = "id4", optional = false) System.Nullable<short> d, @Parameter(name = "id5", optional = false) System.Nullable<int> e, @Parameter(name = "id6", optional = false) System.Nullable<long> f, @Parameter(name = "id7", optional = false) System.Nullable<float> g, @Parameter(name = "id8", optional = false) System.Nullable<double> h, @Parameter(name = "id9", optional = false) System.Nullable<bool> i)
		 [PluginTarget(Neo4Net.GraphDb.GraphDatabaseService.class)]
		 public virtual Node MethodWithAllParams( IGraphDatabaseService db, string a, sbyte? b, char? c, short? d, int? e, long? f, float? g, double? h, bool? i )
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
//ORIGINAL LINE: @PluginTarget(org.Neo4Net.graphdb.GraphDatabaseService.class) public org.Neo4Net.graphdb.Node methodWithSet(@Source IGraphDatabaseService db, @Parameter(name = "strings", optional = false) java.util.Set<String> params)
		 [PluginTarget(Neo4Net.GraphDb.GraphDatabaseService.class)]
		 public virtual Node MethodWithSet( IGraphDatabaseService db, ISet<string> @params )
		 {
			  StringSet = @params;
			  return GetOrCreateANode( db );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.Neo4Net.graphdb.GraphDatabaseService.class) public org.Neo4Net.graphdb.Node methodWithList(@Source IGraphDatabaseService db, @Parameter(name = "strings", optional = false) java.util.List<String> params)
		 [PluginTarget(Neo4Net.GraphDb.GraphDatabaseService.class)]
		 public virtual Node MethodWithList( IGraphDatabaseService db, IList<string> @params )
		 {
			  StringList = @params;
			  return GetOrCreateANode( db );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.Neo4Net.graphdb.GraphDatabaseService.class) public org.Neo4Net.graphdb.Node methodWithListAndInt(@Source IGraphDatabaseService db, @Parameter(name = "strings", optional = false) java.util.List<String> params, @Parameter(name = "count", optional = false) int i)
		 [PluginTarget(Neo4Net.GraphDb.GraphDatabaseService.class)]
		 public virtual Node MethodWithListAndInt( IGraphDatabaseService db, IList<string> @params, int i )
		 {
			  StringList = @params;
			  _integer = i;
			  return GetOrCreateANode( db );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.Neo4Net.graphdb.GraphDatabaseService.class) public org.Neo4Net.graphdb.Node methodWithArray(@Source IGraphDatabaseService db, @Parameter(name = "strings", optional = false) String[] params)
		 [PluginTarget(Neo4Net.GraphDb.GraphDatabaseService.class)]
		 public virtual Node MethodWithArray( IGraphDatabaseService db, string[] @params )
		 {
			  StringArray = @params;
			  return GetOrCreateANode( db );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.Neo4Net.graphdb.GraphDatabaseService.class) public org.Neo4Net.graphdb.Node methodWithIntArray(@Source IGraphDatabaseService db, @Parameter(name = "ints", optional = false) int[] params)
		 [PluginTarget(Neo4Net.GraphDb.GraphDatabaseService.class)]
		 public virtual Node MethodWithIntArray( IGraphDatabaseService db, int[] @params )
		 {
			  IntArray = @params;
			  return GetOrCreateANode( db );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.Neo4Net.graphdb.GraphDatabaseService.class) public org.Neo4Net.graphdb.Node methodWithOptionalArray(@Source IGraphDatabaseService db, @Parameter(name = "ints", optional = true) int[] params)
		 [PluginTarget(Neo4Net.GraphDb.GraphDatabaseService.class)]
		 public virtual Node MethodWithOptionalArray( IGraphDatabaseService db, int[] @params )
		 {
			  IntArray = @params;
			  return GetOrCreateANode( db );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.Neo4Net.graphdb.Node.class) public org.Neo4Net.graphdb.Path pathToReference(@Source Node me)
		 [PluginTarget(Neo4Net.GraphDb.Node.class)]
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

		 private Node GetOrCreateANode( IGraphDatabaseService db )
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