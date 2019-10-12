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
namespace Org.Neo4j.Graphalgo.impl.path
{

	using Org.Neo4j.Graphalgo;
	using Org.Neo4j.Graphalgo;
	using Org.Neo4j.Graphalgo;
	using PathImpl = Org.Neo4j.Graphalgo.impl.util.PathImpl;
	using Org.Neo4j.Graphalgo.impl.util;
	using Org.Neo4j.Graphalgo.impl.util.PriorityMap;
	using WeightedPathImpl = Org.Neo4j.Graphalgo.impl.util.WeightedPathImpl;
	using Direction = Org.Neo4j.Graphdb.Direction;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Node = Org.Neo4j.Graphdb.Node;
	using Path = Org.Neo4j.Graphdb.Path;
	using Org.Neo4j.Graphdb;
	using PropertyContainer = Org.Neo4j.Graphdb.PropertyContainer;
	using Relationship = Org.Neo4j.Graphdb.Relationship;
	using Org.Neo4j.Graphdb.traversal;
	using TraversalMetadata = Org.Neo4j.Graphdb.traversal.TraversalMetadata;
	using Org.Neo4j.Helpers.Collection;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterables.option;

	public class AStar : PathFinder<WeightedPath>
	{
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final org.neo4j.graphdb.PathExpander<?> expander;
		 private readonly PathExpander<object> _expander;
		 private readonly CostEvaluator<double> _lengthEvaluator;
		 private readonly EstimateEvaluator<double> _estimateEvaluator;
		 private Metadata _lastMetadata;

		 public AStar<T1>( PathExpander<T1> expander, CostEvaluator<double> lengthEvaluator, EstimateEvaluator<double> estimateEvaluator )
		 {
			  this._expander = expander;
			  this._lengthEvaluator = lengthEvaluator;
			  this._estimateEvaluator = estimateEvaluator;
		 }

		 public override WeightedPath FindSinglePath( Node start, Node end )
		 {
			  _lastMetadata = new Metadata();
			  AStarIterator iterator = new AStarIterator( this, start, end );
			  while ( iterator.MoveNext() )
			  {
					Node node = iterator.Current;
					GraphDatabaseService graphDb = node.GraphDatabase;
					if ( node.Equals( end ) )
					{
						 // Hit, return path
						 double weight = iterator.VisitData[node.Id].wayLength;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.graphdb.Path path;
						 Path path;
						 if ( start.Id == end.Id )
						 {
							  // Nothing to iterate over
							  path = PathImpl.singular( start );
						 }
						 else
						 {
							  LinkedList<Relationship> rels = new LinkedList<Relationship>();
							  Relationship rel = graphDb.GetRelationshipById( iterator.VisitData[node.Id].cameFromRelationship );
							  while ( rel != null )
							  {
									rels.AddFirst( rel );
									node = rel.GetOtherNode( node );
									long nextRelId = iterator.VisitData[node.Id].cameFromRelationship;
									rel = nextRelId == -1 ? null : graphDb.GetRelationshipById( nextRelId );
							  }
							  path = ToPath( start, rels );
						 }
						 _lastMetadata.paths++;
						 return new WeightedPathImpl( weight, path );
					}
			  }
			  return null;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public Iterable<org.neo4j.graphalgo.WeightedPath> findAllPaths(final org.neo4j.graphdb.Node node, final org.neo4j.graphdb.Node end)
		 public override IEnumerable<WeightedPath> FindAllPaths( Node node, Node end )
		 {
			  return option( FindSinglePath( node, end ) );
		 }

		 public override TraversalMetadata Metadata()
		 {
			  return _lastMetadata;
		 }

		 private Path ToPath( Node start, LinkedList<Relationship> rels )
		 {
			  PathImpl.Builder builder = new PathImpl.Builder( start );
			  foreach ( Relationship rel in rels )
			  {
					builder = builder.Push( rel );
			  }
			  return builder.Build();
		 }

		 private class Visit
		 {
			  internal double WayLength; // accumulated cost to get here (g)
			  internal double Estimate; // heuristic estimate of cost to reach end (h)
			  internal long CameFromRelationship;
			  internal bool Visited;
			  internal bool Next;

			  internal Visit( long cameFromRelationship, double wayLength, double estimate )
			  {
					Update( cameFromRelationship, wayLength, estimate );
			  }

			  internal virtual void Update( long cameFromRelationship, double wayLength, double estimate )
			  {
					this.CameFromRelationship = cameFromRelationship;
					this.WayLength = wayLength;
					this.Estimate = estimate;
			  }

			  internal virtual double Fscore
			  {
				  get
				  {
						return WayLength + Estimate;
				  }
			  }
		 }

		 private class AStarIterator : PrefetchingIterator<Node>, Path
		 {
			 private readonly AStar _outerInstance;

			  internal readonly Node Start;
			  internal readonly Node End;
			  internal Node LastNode;
			  internal readonly PriorityMap<Node, Node, double> NextPrioritizedNodes = PriorityMap.withSelfKeyNaturalOrder();
			  internal readonly IDictionary<long, Visit> VisitData = new Dictionary<long, Visit>();

			  internal AStarIterator( AStar outerInstance, Node start, Node end )
			  {
				  this._outerInstance = outerInstance;
					this.Start = start;
					this.End = end;

					Visit visit = new Visit( -1, 0, outerInstance.estimateEvaluator.GetCost( start, end ) );
					AddNext( start, visit.Fscore, visit );
					this.VisitData[start.Id] = visit;
			  }

			  internal virtual void AddNext( Node node, double fscore, Visit visit )
			  {
					NextPrioritizedNodes.put( node, fscore );
					visit.Next = true;
			  }

			  internal virtual Node PopLowestScoreNode()
			  {
					PriorityMap.Entry<Node, double> top = NextPrioritizedNodes.pop();
					if ( top == null )
					{
						 return null;
					}

					Node node = top.Entity;
					Visit visit = VisitData[node.Id];
					visit.Visited = true;
					visit.Next = false;
					return node;
			  }

			  protected internal override Node FetchNextOrNull()
			  {
					if ( LastNode != null )
					{
						 Expand();
					}

					LastNode = PopLowestScoreNode();
					return LastNode;
			  }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private void expand()
			  internal virtual void Expand()
			  {
					IEnumerable<Relationship> expand = outerInstance.expander.Expand( this, BranchState.NO_STATE );
					foreach ( Relationship rel in expand )
					{
						 outerInstance.lastMetadata.Rels++;
						 Node node = rel.GetOtherNode( LastNode );
						 Visit visit = VisitData[node.Id];
						 if ( visit != null && visit.Visited )
						 {
							  continue;
						 }

						 Visit lastVisit = VisitData[LastNode.Id];
						 double tentativeGScore = lastVisit.WayLength + outerInstance.lengthEvaluator.GetCost( rel, Direction.OUTGOING );
						 double estimate = outerInstance.estimateEvaluator.GetCost( node, End );

						 if ( visit == null || !visit.Next || tentativeGScore < visit.WayLength )
						 {
							  if ( visit == null )
							  {
									visit = new Visit( rel.Id, tentativeGScore, estimate );
									VisitData[node.Id] = visit;
							  }
							  else
							  {
									visit.Update( rel.Id, tentativeGScore, estimate );
							  }
							  AddNext( node, estimate + tentativeGScore, visit );
						 }
					}
			  }

			  public override Node StartNode()
			  {
					return Start;
			  }

			  public override Node EndNode()
			  {
					return LastNode;
			  }

			  public override Relationship LastRelationship()
			  {
					throw new System.NotSupportedException();
			  }

			  public override IEnumerable<Relationship> Relationships()
			  {
					throw new System.NotSupportedException();
			  }

			  public override IEnumerable<Relationship> ReverseRelationships()
			  {
					throw new System.NotSupportedException();
			  }

			  public override IEnumerable<Node> Nodes()
			  {
					throw new System.NotSupportedException();
			  }

			  public override IEnumerable<Node> ReverseNodes()
			  {
					throw new System.NotSupportedException();
			  }

			  public override int Length()
			  {
					throw new System.NotSupportedException();
			  }

			  public override IEnumerator<PropertyContainer> Iterator()
			  {
					throw new System.NotSupportedException();
			  }
		 }

		 private class Metadata : TraversalMetadata
		 {
			  internal int Rels;
			  internal int Paths;

			  public virtual int NumberOfPathsReturned
			  {
				  get
				  {
						return Paths;
				  }
			  }

			  public virtual int NumberOfRelationshipsTraversed
			  {
				  get
				  {
						return Rels;
				  }
			  }
		 }
	}

}