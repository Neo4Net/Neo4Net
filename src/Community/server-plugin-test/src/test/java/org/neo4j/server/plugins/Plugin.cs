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
namespace Neo4Net.Server.plugins
{

	using GraphAlgoFactory = Neo4Net.Graphalgo.GraphAlgoFactory;
	using Neo4Net.Graphalgo;
	using GraphDatabaseService = Neo4Net.Graphdb.GraphDatabaseService;
	using Node = Neo4Net.Graphdb.Node;
	using Path = Neo4Net.Graphdb.Path;
	using PathExpanders = Neo4Net.Graphdb.PathExpanders;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Neo4Net.Helpers.Collection;

	[Description("Here you can describe your plugin. It will show up in the description of the methods.")]
	public class Plugin : ServerPlugin
	{
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
		 [Name(GET_CONNECTED_NODES), PluginTarget(Neo4Net.Graphdb.Node.class)]
		 public virtual IEnumerable<Node> GetAllConnectedNodes( Node start )
		 {
			  List<Node> nodes = new List<Node>();

			  foreach ( Relationship rel in start.Relationships )
			  {
					nodes.Add( rel.GetOtherNode( start ) );
			  }

			  return nodes;
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.neo4j.graphdb.Node.class) public Iterable<org.neo4j.graphdb.Relationship> getRelationshipsBetween(@Source final org.neo4j.graphdb.Node start, @Parameter(name = "other") final org.neo4j.graphdb.Node end)
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 [PluginTarget(Neo4Net.Graphdb.Node.class)]
		 public virtual IEnumerable<Relationship> GetRelationshipsBetween( Node start, Node end )
		 {
			  return new FilteringIterable<Relationship>( start.Relationships, item => item.getOtherNode( start ).Equals( end ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.neo4j.graphdb.Node.class) public Iterable<org.neo4j.graphdb.Relationship> createRelationships(@Source Node start, @Parameter(name = "type") org.neo4j.graphdb.RelationshipType type, @Parameter(name = "nodes") Iterable<org.neo4j.graphdb.Node> nodes)
		 [PluginTarget(Neo4Net.Graphdb.Node.class)]
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
		 [PluginTarget(Neo4Net.Graphdb.Node.class)]
		 public virtual Node GetThisNodeOrById( Node start, long? id )
		 {
			  Optional = id;

			  if ( id == null )
			  {
					return start;
			  }

			  return start.GraphDatabase.getNodeById( id );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.neo4j.graphdb.GraphDatabaseService.class) public org.neo4j.graphdb.Node methodWithIntParam(@Source GraphDatabaseService db, @Parameter(name = "id", optional = false) int id)
		 [PluginTarget(Neo4Net.Graphdb.GraphDatabaseService.class)]
		 public virtual Node MethodWithIntParam( GraphDatabaseService db, int id )
		 {
			  return Db.getNodeById( id );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.neo4j.graphdb.Relationship.class) public Iterable<org.neo4j.graphdb.Node> methodOnRelationship(@Source Relationship rel)
		 [PluginTarget(Neo4Net.Graphdb.Relationship.class)]
		 public virtual IEnumerable<Node> MethodOnRelationship( Relationship rel )
		 {
			  return Arrays.asList( rel.Nodes );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.neo4j.graphdb.GraphDatabaseService.class) public org.neo4j.graphdb.Node methodWithAllParams(@Source GraphDatabaseService db, @Parameter(name = "id", optional = false) String a, @Parameter(name = "id2", optional = false) System.Nullable<sbyte> b, @Parameter(name = "id3", optional = false) System.Nullable<char> c, @Parameter(name = "id4", optional = false) System.Nullable<short> d, @Parameter(name = "id5", optional = false) System.Nullable<int> e, @Parameter(name = "id6", optional = false) System.Nullable<long> f, @Parameter(name = "id7", optional = false) System.Nullable<float> g, @Parameter(name = "id8", optional = false) System.Nullable<double> h, @Parameter(name = "id9", optional = false) System.Nullable<bool> i)
		 [PluginTarget(Neo4Net.Graphdb.GraphDatabaseService.class)]
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

			  return Db.createNode();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.neo4j.graphdb.GraphDatabaseService.class) public org.neo4j.graphdb.Node methodWithSet(@Source GraphDatabaseService db, @Parameter(name = "strings", optional = false) java.util.Set<String> params)
		 [PluginTarget(Neo4Net.Graphdb.GraphDatabaseService.class)]
		 public virtual Node MethodWithSet( GraphDatabaseService db, ISet<string> @params )
		 {
			  StringSet = @params;
			  return Db.createNode();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.neo4j.graphdb.GraphDatabaseService.class) public org.neo4j.graphdb.Node methodWithList(@Source GraphDatabaseService db, @Parameter(name = "strings", optional = false) java.util.List<String> params)
		 [PluginTarget(Neo4Net.Graphdb.GraphDatabaseService.class)]
		 public virtual Node MethodWithList( GraphDatabaseService db, IList<string> @params )
		 {
			  StringList = @params;
			  return Db.createNode();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.neo4j.graphdb.GraphDatabaseService.class) public org.neo4j.graphdb.Node methodWithArray(@Source GraphDatabaseService db, @Parameter(name = "strings", optional = false) String[] params)
		 [PluginTarget(Neo4Net.Graphdb.GraphDatabaseService.class)]
		 public virtual Node MethodWithArray( GraphDatabaseService db, string[] @params )
		 {
			  StringArray = @params;
			  return Db.createNode();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.neo4j.graphdb.GraphDatabaseService.class) public org.neo4j.graphdb.Node methodWithIntArray(@Source GraphDatabaseService db, @Parameter(name = "ints", optional = false) int[] params)
		 [PluginTarget(Neo4Net.Graphdb.GraphDatabaseService.class)]
		 public virtual Node MethodWithIntArray( GraphDatabaseService db, int[] @params )
		 {
			  IntArray = @params;
			  return Db.createNode();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.neo4j.graphdb.GraphDatabaseService.class) public org.neo4j.graphdb.Node methodWithOptionalArray(@Source GraphDatabaseService db, @Parameter(name = "ints", optional = true) int[] params)
		 [PluginTarget(Neo4Net.Graphdb.GraphDatabaseService.class)]
		 public virtual Node MethodWithOptionalArray( GraphDatabaseService db, int[] @params )
		 {
			  IntArray = @params;
			  return Db.createNode();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @PluginTarget(org.neo4j.graphdb.Node.class) public org.neo4j.graphdb.Path pathToReference(@Source Node me)
		 [PluginTarget(Neo4Net.Graphdb.Node.class)]
		 public virtual Path PathToReference( Node me )
		 {
			  PathFinder<Path> finder = GraphAlgoFactory.shortestPath( PathExpanders.allTypesAndDirections(), 6 );
			  return finder.FindSinglePath( me.GraphDatabase.createNode(), me );
		 }
	}

}