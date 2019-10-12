using System.Collections.Generic;

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
namespace Org.Neo4j.@internal.cypher.acceptance
{

	using GraphAlgoFactory = Org.Neo4j.Graphalgo.GraphAlgoFactory;
	using Org.Neo4j.Graphalgo;
	using WeightedPath = Org.Neo4j.Graphalgo.WeightedPath;
	using Direction = Org.Neo4j.Graphdb.Direction;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Label = Org.Neo4j.Graphdb.Label;
	using Node = Org.Neo4j.Graphdb.Node;
	using Path = Org.Neo4j.Graphdb.Path;
	using PathExpanders = Org.Neo4j.Graphdb.PathExpanders;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using RelationshipType = Org.Neo4j.Graphdb.RelationshipType;
	using Org.Neo4j.Graphdb;
	using Result = Org.Neo4j.Graphdb.Result;
	using Evaluation = Org.Neo4j.Graphdb.traversal.Evaluation;
	using Evaluator = Org.Neo4j.Graphdb.traversal.Evaluator;
	using Evaluators = Org.Neo4j.Graphdb.traversal.Evaluators;
	using TraversalDescription = Org.Neo4j.Graphdb.traversal.TraversalDescription;
	using Uniqueness = Org.Neo4j.Graphdb.traversal.Uniqueness;
	using Context = Org.Neo4j.Procedure.Context;
	using Description = Org.Neo4j.Procedure.Description;
	using Name = Org.Neo4j.Procedure.Name;
	using Procedure = Org.Neo4j.Procedure.Procedure;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.traversal.Evaluation.EXCLUDE_AND_CONTINUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.traversal.Evaluation.EXCLUDE_AND_PRUNE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.traversal.Evaluation.INCLUDE_AND_CONTINUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.procedure.Mode.WRITE;

	public class TestProcedure
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public org.neo4j.graphdb.GraphDatabaseService db;
		 public GraphDatabaseService Db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure("org.neo4j.time") @Description("org.neo4j.time") public void time(@Name(value = "time") java.time.LocalTime statementTime)
		 [Procedure("org.neo4j.time"), Description("org.neo4j.time")]
		 public virtual void Time( LocalTime statementTime )
		 {
			  LocalTime realTime = LocalTime.now();
			  Duration duration = Duration.between( statementTime, realTime );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure("org.neo4j.aNodeWithLabel") @Description("org.neo4j.aNodeWithLabel") public java.util.stream.Stream<NodeResult> aNodeWithLabel(@Name(value = "label", defaultValue = "Dog") String label)
		 [Procedure("org.neo4j.aNodeWithLabel"), Description("org.neo4j.aNodeWithLabel")]
		 public virtual Stream<NodeResult> ANodeWithLabel( string label )
		 {
			  Result result = Db.execute( "MATCH (n:" + label + ") RETURN n LIMIT 1" );
			  return result.Select( row => new NodeResult( ( Node )row.get( "n" ) ) );
		 }

		 [Procedure("org.neo4j.stream123"), Description("org.neo4j.stream123")]
		 public virtual Stream<CountResult> Stream123()
		 {
			  return IntStream.of( 1, 2, 3 ).mapToObj( i => new CountResult( i, "count" + i ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure("org.neo4j.recurseN") @Description("org.neo4j.recurseN") public java.util.stream.Stream<NodeResult> recurseN(@Name("n") System.Nullable<long> n)
		 [Procedure("org.neo4j.recurseN"), Description("org.neo4j.recurseN")]
		 public virtual Stream<NodeResult> RecurseN( long? n )
		 {
			  Result result;
			  if ( n == 0 )
			  {
					result = Db.execute( "MATCH (n) RETURN n LIMIT 1" );
			  }
			  else
			  {
					result = Db.execute( "UNWIND [1] AS i CALL org.neo4j.recurseN(" + ( n - 1 ) + ") YIELD node RETURN node" );
			  }
			  return result.Select( row => new NodeResult( ( Node )row.get( "node" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure("org.neo4j.findNodesWithLabel") @Description("org.neo4j.findNodesWithLabel") public java.util.stream.Stream<NodeResult> findNodesWithLabel(@Name("label") String label)
		 [Procedure("org.neo4j.findNodesWithLabel"), Description("org.neo4j.findNodesWithLabel")]
		 public virtual Stream<NodeResult> FindNodesWithLabel( string label )
		 {
			  ResourceIterator<Node> nodes = Db.findNodes( Label.label( label ) );
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return nodes.Select( NodeResult::new );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure("org.neo4j.expandNode") @Description("org.neo4j.expandNode") public java.util.stream.Stream<NodeResult> expandNode(@Name("nodeId") System.Nullable<long> nodeId)
		 [Procedure("org.neo4j.expandNode"), Description("org.neo4j.expandNode")]
		 public virtual Stream<NodeResult> ExpandNode( long? nodeId )
		 {
			  Node node = Db.getNodeById( nodeId );
			  IList<Node> result = new List<Node>();
			  foreach ( Relationship r in node.Relationships )
			  {
					result.Add( r.GetOtherNode( node ) );
			  }

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return result.Select( NodeResult::new );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure(name = "org.neo4j.createNodeWithLoop", mode = WRITE) @Description("org.neo4j.createNodeWithLoop") public java.util.stream.Stream<NodeResult> createNodeWithLoop(@Name("nodeLabel") String label, @Name("relType") String relType)
		 [Procedure(name : "org.neo4j.createNodeWithLoop", mode : WRITE), Description("org.neo4j.createNodeWithLoop")]
		 public virtual Stream<NodeResult> CreateNodeWithLoop( string label, string relType )
		 {
			  Node node = Db.createNode( Label.label( label ) );
			  node.CreateRelationshipTo( node, RelationshipType.withName( relType ) );
			  return Stream.of( new NodeResult( node ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure(name = "org.neo4j.graphAlgosDijkstra") @Description("org.neo4j.graphAlgosDijkstra") public java.util.stream.Stream<NodeResult> graphAlgosDijkstra(@Name("startNode") org.neo4j.graphdb.Node start, @Name("endNode") org.neo4j.graphdb.Node end, @Name("relType") String relType, @Name("weightProperty") String weightProperty)
		 [Procedure(name : "org.neo4j.graphAlgosDijkstra"), Description("org.neo4j.graphAlgosDijkstra")]
		 public virtual Stream<NodeResult> GraphAlgosDijkstra( Node start, Node end, string relType, string weightProperty )
		 {
			  PathFinder<WeightedPath> pathFinder = GraphAlgoFactory.dijkstra( PathExpanders.forTypeAndDirection( RelationshipType.withName( relType ), Direction.BOTH ), weightProperty );

			  WeightedPath path = pathFinder.FindSinglePath( start, end );
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return StreamSupport.stream( path.Nodes().spliterator(), false ).map(NodeResult::new);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure(name = "org.neo4j.setProperty", mode = WRITE) public java.util.stream.Stream<NodeResult> setProperty(@Name("node") org.neo4j.graphdb.Node node, @Name("propertyKey") String propertyKeyName, @Name("value") String value)
		 [Procedure(name : "org.neo4j.setProperty", mode : WRITE)]
		 public virtual Stream<NodeResult> SetProperty( Node node, string propertyKeyName, string value )
		 {
			  if ( string.ReferenceEquals( value, null ) )
			  {
					node.RemoveProperty( propertyKeyName );
			  }
			  else
			  {
					node.SetProperty( propertyKeyName, value );
			  }
			  return Stream.of( new NodeResult( node ) );
		 }

		 public class NodeResult
		 {
			  public Node Node;

			  internal NodeResult( Node node )
			  {
					this.Node = node;
			  }
		 }

		 public class CountResult
		 {
			  public long Count;
			  public string Name;

			  internal CountResult( long count, string name )
			  {
					this.Count = count;
					this.Name = name;
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure("org.neo4j.movieTraversal") @Description("org.neo4j.movieTraversal") public java.util.stream.Stream<PathResult> movieTraversal(@Name("start") org.neo4j.graphdb.Node start)
		 [Procedure("org.neo4j.movieTraversal"), Description("org.neo4j.movieTraversal")]
		 public virtual Stream<PathResult> MovieTraversal( Node start )
		 {
			  TraversalDescription td = Db.traversalDescription().breadthFirst().relationships(RelationshipType.withName("ACTED_IN"), Direction.BOTH).relationships(RelationshipType.withName("PRODUCED"), Direction.BOTH).relationships(RelationshipType.withName("DIRECTED"), Direction.BOTH).evaluator(Evaluators.fromDepth(3)).evaluator(new LabelEvaluator("Western", 1, 3)).uniqueness(Uniqueness.NODE_GLOBAL);

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return td.Traverse( start ).Select( PathResult::new );
		 }

		 public class PathResult
		 {
			  public Path Path;

			  internal PathResult( Path path )
			  {
					this.Path = path;
			  }
		 }

		 public class LabelEvaluator : Evaluator
		 {
			  internal ISet<string> EndNodeLabels;
			  internal long Limit = -1;
			  internal long MinLevel = -1;
			  internal long ResultCount;

			  internal LabelEvaluator( string endNodeLabel, long limit, int minLevel )
			  {
					this.Limit = limit;
					this.MinLevel = minLevel;

					EndNodeLabels = Collections.singleton( endNodeLabel );
			  }

			  public override Evaluation Evaluate( Path path )
			  {
					int depth = path.Length();
					Node check = path.EndNode();

					if ( depth < MinLevel )
					{
						 return EXCLUDE_AND_CONTINUE;
					}

					if ( Limit != -1 && ResultCount >= Limit )
					{
						 return EXCLUDE_AND_PRUNE;
					}

					return LabelExists( check, EndNodeLabels ) ? CountIncludeAndContinue() : EXCLUDE_AND_CONTINUE;
			  }

			  internal virtual bool LabelExists( Node node, ISet<string> labels )
			  {
					if ( labels.Count == 0 )
					{
						 return false;
					}

					foreach ( Label lab in node.Labels )
					{
						 if ( labels.Contains( lab.Name() ) )
						 {
							  return true;
						 }
					}
					return false;
			  }

			  internal virtual Evaluation CountIncludeAndContinue()
			  {
					ResultCount++;
					return INCLUDE_AND_CONTINUE;
			  }
		 }
	}

}