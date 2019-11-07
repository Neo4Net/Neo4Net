using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.Internal.cypher.acceptance
{

	using GraphAlgoFactory = Neo4Net.GraphAlgo.GraphAlgoFactory;
	using Neo4Net.GraphAlgo;
	using WeightedPath = Neo4Net.GraphAlgo.WeightedPath;
	using Direction = Neo4Net.GraphDb.Direction;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Path = Neo4Net.GraphDb.Path;
	using PathExpanders = Neo4Net.GraphDb.PathExpanders;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using RelationshipType = Neo4Net.GraphDb.RelationshipType;
	using Neo4Net.GraphDb;
	using Result = Neo4Net.GraphDb.Result;
	using Evaluation = Neo4Net.GraphDb.Traversal.Evaluation;
	using Evaluator = Neo4Net.GraphDb.Traversal.Evaluator;
	using Evaluators = Neo4Net.GraphDb.Traversal.Evaluators;
	using TraversalDescription = Neo4Net.GraphDb.Traversal.TraversalDescription;
	using Uniqueness = Neo4Net.GraphDb.Traversal.Uniqueness;
	using Context = Neo4Net.Procedure.Context;
	using Description = Neo4Net.Procedure.Description;
	using Name = Neo4Net.Procedure.Name;
	using Procedure = Neo4Net.Procedure.Procedure;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.traversal.Evaluation.EXCLUDE_AND_CONTINUE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.traversal.Evaluation.EXCLUDE_AND_PRUNE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.graphdb.traversal.Evaluation.IncludeAndContinue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.procedure.Mode.WRITE;

	public class TestProcedure
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Context public Neo4Net.graphdb.GraphDatabaseService db;
		 public IGraphDatabaseService Db;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure("Neo4Net.time") @Description("Neo4Net.time") public void time(@Name(value = "time") java.time.LocalTime statementTime)
		 [Procedure("Neo4Net.time"), Description("Neo4Net.time")]
		 public virtual void Time( LocalTime statementTime )
		 {
			  LocalTime realTime = LocalTime.now();
			  Duration duration = Duration.between( statementTime, realTime );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure("Neo4Net.aNodeWithLabel") @Description("Neo4Net.aNodeWithLabel") public java.util.stream.Stream<NodeResult> aNodeWithLabel(@Name(value = "label", defaultValue = "Dog") String label)
		 [Procedure("Neo4Net.aNodeWithLabel"), Description("Neo4Net.aNodeWithLabel")]
		 public virtual Stream<NodeResult> ANodeWithLabel( string label )
		 {
			  Result result = Db.execute( "MATCH (n:" + label + ") RETURN n LIMIT 1" );
			  return result.Select( row => new NodeResult( ( Node )row.get( "n" ) ) );
		 }

		 [Procedure("Neo4Net.stream123"), Description("Neo4Net.stream123")]
		 public virtual Stream<CountResult> Stream123()
		 {
			  return IntStream.of( 1, 2, 3 ).mapToObj( i => new CountResult( i, "count" + i ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure("Neo4Net.recurseN") @Description("Neo4Net.recurseN") public java.util.stream.Stream<NodeResult> recurseN(@Name("n") System.Nullable<long> n)
		 [Procedure("Neo4Net.recurseN"), Description("Neo4Net.recurseN")]
		 public virtual Stream<NodeResult> RecurseN( long? n )
		 {
			  Result result;
			  if ( n == 0 )
			  {
					result = Db.execute( "MATCH (n) RETURN n LIMIT 1" );
			  }
			  else
			  {
					result = Db.execute( "UNWIND [1] AS i CALL Neo4Net.recurseN(" + ( n - 1 ) + ") YIELD node RETURN node" );
			  }
			  return result.Select( row => new NodeResult( ( Node )row.get( "node" ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure("Neo4Net.findNodesWithLabel") @Description("Neo4Net.findNodesWithLabel") public java.util.stream.Stream<NodeResult> findNodesWithLabel(@Name("label") String label)
		 [Procedure("Neo4Net.findNodesWithLabel"), Description("Neo4Net.findNodesWithLabel")]
		 public virtual Stream<NodeResult> FindNodesWithLabel( string label )
		 {
			  IResourceIterator<Node> nodes = Db.findNodes( Label.label( label ) );
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return nodes.Select( NodeResult::new );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure("Neo4Net.expandNode") @Description("Neo4Net.expandNode") public java.util.stream.Stream<NodeResult> expandNode(@Name("nodeId") System.Nullable<long> nodeId)
		 [Procedure("Neo4Net.expandNode"), Description("Neo4Net.expandNode")]
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
//ORIGINAL LINE: @Procedure(name = "Neo4Net.createNodeWithLoop", mode = WRITE) @Description("Neo4Net.createNodeWithLoop") public java.util.stream.Stream<NodeResult> createNodeWithLoop(@Name("nodeLabel") String label, @Name("relType") String relType)
		 [Procedure(name : "Neo4Net.createNodeWithLoop", mode : WRITE), Description("Neo4Net.createNodeWithLoop")]
		 public virtual Stream<NodeResult> CreateNodeWithLoop( string label, string relType )
		 {
			  Node node = Db.createNode( Label.label( label ) );
			  node.CreateRelationshipTo( node, RelationshipType.withName( relType ) );
			  return Stream.of( new NodeResult( node ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure(name = "Neo4Net.graphAlgosDijkstra") @Description("Neo4Net.graphAlgosDijkstra") public java.util.stream.Stream<NodeResult> graphAlgosDijkstra(@Name("startNode") Neo4Net.graphdb.Node start, @Name("endNode") Neo4Net.graphdb.Node end, @Name("relType") String relType, @Name("weightProperty") String weightProperty)
		 [Procedure(name : "Neo4Net.graphAlgosDijkstra"), Description("Neo4Net.graphAlgosDijkstra")]
		 public virtual Stream<NodeResult> GraphAlgosDijkstra( Node start, Node end, string relType, string weightProperty )
		 {
			  PathFinder<WeightedPath> pathFinder = GraphAlgoFactory.dijkstra( PathExpanders.forTypeAndDirection( RelationshipType.withName( relType ), Direction.BOTH ), weightProperty );

			  WeightedPath path = pathFinder.FindSinglePath( start, end );
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return StreamSupport.stream( path.Nodes().spliterator(), false ).map(NodeResult::new);
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Procedure(name = "Neo4Net.setProperty", mode = WRITE) public java.util.stream.Stream<NodeResult> setProperty(@Name("node") Neo4Net.graphdb.Node node, @Name("propertyKey") String propertyKeyName, @Name("value") String value)
		 [Procedure(name : "Neo4Net.setProperty", mode : WRITE)]
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
//ORIGINAL LINE: @Procedure("Neo4Net.movieTraversal") @Description("Neo4Net.movieTraversal") public java.util.stream.Stream<PathResult> movieTraversal(@Name("start") Neo4Net.graphdb.Node start)
		 [Procedure("Neo4Net.movieTraversal"), Description("Neo4Net.movieTraversal")]
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
					return IncludeAndContinue;
			  }
		 }
	}

}