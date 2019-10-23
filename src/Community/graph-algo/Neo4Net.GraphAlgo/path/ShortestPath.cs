using System;
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
namespace Neo4Net.GraphAlgo.Path
{
	using MutableBoolean = org.apache.commons.lang3.mutable.MutableBoolean;
	using MutableInt = org.apache.commons.lang3.mutable.MutableInt;
	using MutableIntObjectMap = org.eclipse.collections.api.map.primitive.MutableIntObjectMap;
	using IntObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.IntObjectHashMap;


	using Neo4Net.GraphAlgo;
	using PathImpl = Neo4Net.GraphAlgo.Utils.PathImpl;
	using Builder = Neo4Net.GraphAlgo.Utils.PathImpl.Builder;
	using Direction = Neo4Net.GraphDb.Direction;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Node = Neo4Net.GraphDb.Node;
	using Path = Neo4Net.GraphDb.Path;
	using Neo4Net.GraphDb;
	using IPropertyContainer = Neo4Net.GraphDb.PropertyContainer;
	using Relationship = Neo4Net.GraphDb.Relationship;
	using Neo4Net.GraphDb;
	using Neo4Net.GraphDb.Traversal;
	using TraversalMetadata = Neo4Net.GraphDb.Traversal.TraversalMetadata;
	using Neo4Net.Helpers.Collections;
	using Iterators = Neo4Net.Helpers.Collections.Iterators;
	using Neo4Net.Helpers.Collections;
	using Neo4Net.Helpers.Collections;
	using GraphDatabaseFacade = Neo4Net.Kernel.impl.factory.GraphDatabaseFacade;
	using Monitors = Neo4Net.Kernel.monitoring.Monitors;

	/// <summary>
	/// Find (all or one) simple shortest path(s) between two nodes. It starts
	/// from both ends and goes one relationship at the time, alternating side
	/// between each traversal. It does so to minimize the traversal overhead
	/// if one side has a very large amount of relationships, but the other one
	/// very few. It performs well however the graph is proportioned.
	/// 
	/// Relationships are traversed in the specified directions from the start node,
	/// but in the reverse direction ( <seealso cref="Direction.reverse()"/> ) from the
	/// end node. This doesn't affect <seealso cref="Direction.BOTH"/>.
	/// </summary>
	public class ShortestPath : PathFinder<Path>
	{
		 public readonly int Null = -1;
		 private readonly int _maxDepth;
		 private readonly int _maxResultCount;
		 private readonly PathExpander _expander;
		 private Metadata _lastMetadata;
		 private ShortestPathPredicate _predicate;
		 private DataMonitor _dataMonitor;

		 public interface IShortestPathPredicate
		 {
			  bool Test( Path path );
		 }

		 /// <summary>
		 /// Constructs a new shortest path algorithm. </summary>
		 /// <param name="maxDepth"> the maximum depth for the traversal. Returned paths
		 /// will never have a greater <seealso cref="Path.length()"/> than {@code maxDepth}. </param>
		 /// <param name="expander"> the <seealso cref="PathExpander"/> to use for deciding
		 /// which relationships to expand for each <seealso cref="Node"/>. </param>
		 public ShortestPath( int maxDepth, PathExpander expander ) : this( maxDepth, expander, int.MaxValue )
		 {
		 }

		 public ShortestPath( int maxDepth, PathExpander expander, ShortestPathPredicate predicate ) : this( maxDepth, expander )
		 {
			  this._predicate = predicate;
		 }

		 /// <summary>
		 /// Constructs a new shortest path algorithm. </summary>
		 /// <param name="maxDepth"> the maximum depth for the traversal. Returned paths
		 /// will never have a greater <seealso cref="Path.length()"/> than {@code maxDepth}. </param>
		 /// <param name="expander"> the <seealso cref="PathExpander"/> to use for deciding
		 /// which relationships to expand for each <seealso cref="Node"/>. </param>
		 /// <param name="maxResultCount"> the maximum number of hits to return. If this number
		 /// of hits are encountered the traversal will stop. </param>
		 public ShortestPath( int maxDepth, PathExpander expander, int maxResultCount )
		 {
			  this._maxDepth = maxDepth;
			  this._expander = expander;
			  this._maxResultCount = maxResultCount;
		 }

		 public override IEnumerable<Path> FindAllPaths( Node start, Node end )
		 {
			  return InternalPaths( start, end, false );
		 }

		 public override Path FindSinglePath( Node start, Node end )
		 {
			  IEnumerator<Path> paths = InternalPaths( start, end, true ).GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  return paths.hasNext() ? paths.next() : null;
		 }

		 private void ResolveMonitor( Node node )
		 {
			  if ( _dataMonitor == null )
			  {
					GraphDatabaseService service = node.GraphDatabase;
					if ( service is GraphDatabaseFacade )
					{
						 Monitors monitors = ( ( GraphDatabaseFacade ) service ).DependencyResolver.resolveDependency( typeof( Monitors ) );
						 _dataMonitor = monitors.NewMonitor( typeof( DataMonitor ) );
					}
			  }
		 }

		 private IEnumerable<Path> InternalPaths( Node start, Node end, bool stopAsap )
		 {
			  _lastMetadata = new Metadata();
			  if ( start.Equals( end ) )
			  {
					return FilterPaths( Collections.singletonList( PathImpl.singular( start ) ) );
			  }
			  Hits hits = new Hits();
			  ICollection<long> sharedVisitedRels = new HashSet<long>();
			  MutableInt sharedFrozenDepth = new MutableInt( Null ); // ShortestPathLengthSoFar
			  MutableBoolean sharedStop = new MutableBoolean();
			  MutableInt sharedCurrentDepth = new MutableInt( 0 );
			  using ( DirectionData startData = new DirectionData( this, start, sharedVisitedRels, sharedFrozenDepth, sharedStop, sharedCurrentDepth, _expander ), DirectionData endData = new DirectionData( this, end, sharedVisitedRels, sharedFrozenDepth, sharedStop, sharedCurrentDepth, _expander.reverse() ) )
			  {
					while ( startData.MoveNext() || endData.MoveNext() )
					{
						 GoOneStep( startData, endData, hits, startData, stopAsap );
						 GoOneStep( endData, startData, hits, startData, stopAsap );
					}
					ICollection<Hit> least = hits.Least();
					return least != null ? FilterPaths( HitsToPaths( least, start, end, stopAsap, _maxResultCount ) ) : Collections.emptyList();
			  }
		 }

		 public override TraversalMetadata Metadata()
		 {
			  return _lastMetadata;
		 }

		 // Few long-lived instances
		 private class Hit
		 {
			  internal readonly DirectionData Start;
			  internal readonly DirectionData End;
			  internal readonly Node ConnectingNode;

			  internal Hit( DirectionData start, DirectionData end, Node connectingNode )
			  {
					this.Start = start;
					this.End = end;
					this.ConnectingNode = connectingNode;
			  }

			  public override int GetHashCode()
			  {
					return ConnectingNode.GetHashCode();
			  }

			  public override bool Equals( object obj )
			  {
					if ( this == obj )
					{
						 return true;
					}
					if ( obj == null || this.GetType() != obj.GetType() )
					{
						 return false;
					}
					Hit o = ( Hit ) obj;
					return ConnectingNode.Equals( o.ConnectingNode );
			  }
		 }

		 private void GoOneStep( DirectionData directionData, DirectionData otherSide, Hits hits, DirectionData startSide, bool stopAsap )
		 {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  if ( !directionData.HasNext() )
			  {
					// We can not go any deeper from this direction. Possibly disconnected nodes.
					otherSide.FinishCurrentLayerThenStop = true;
					return;
			  }
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
			  Node nextNode = directionData.Next();
			  LevelData otherSideHit = otherSide.VisitedNodes[nextNode];
			  if ( otherSideHit != null )
			  {
					// This is a hit
					int depth = directionData.CurrentDepth + otherSideHit.Depth;

					if ( directionData.SharedFrozenDepth.intValue() == Null )
					{
						 directionData.SharedFrozenDepth.Value = depth;
					}
					if ( depth <= directionData.SharedFrozenDepth.intValue() )
					{
						 directionData.HaveFoundSomething = true;
						 if ( depth < directionData.SharedFrozenDepth.intValue() )
						 {
							  directionData.SharedFrozenDepth.Value = depth;
							  // TODO Is it really ok to just stop the other side here?
							  // I'm basing that decision on that it was the other side
							  // which found the deeper paths (correct assumption?)
							  otherSide.Stop = true;
						 }
						 // Add it to the list of hits
						 DirectionData startSideData = directionData == startSide ? directionData : otherSide;
						 DirectionData endSideData = directionData == startSide ? otherSide : directionData;
						 Hit hit = new Hit( startSideData, endSideData, nextNode );
						 Node start = startSide.StartNode;
						 Node end = ( startSide == directionData ) ? otherSide.StartNode : directionData.StartNode;
						 MonitorData( startSide, ( otherSide == startSide ) ? directionData : otherSide, nextNode );
						 // NOTE: Applying the filter-condition could give the wrong results with allShortestPaths,
						 // so only use it for singleShortestPath
						 if ( !stopAsap || FilterPaths( HitToPaths( hit, start, end, stopAsap ) ).Count > 0 )
						 {
							  if ( hits.Add( hit, depth ) >= _maxResultCount )
							  {
									directionData.Stop = true;
									otherSide.Stop = true;
									_lastMetadata.paths++;
							  }
							  else if ( stopAsap )
							  { // This side found a hit, but wait for the other side to complete its current depth
									// to see if it finds a shorter path. (i.e. stop this side and freeze the depth).
									// but only if the other side has not stopped, otherwise we might miss shorter paths
									if ( otherSide.Stop )
									{
										 return;
									}
									directionData.Stop = true;
							  }
						 }
						 else
						 {
							  directionData.HaveFoundSomething = false;
							  directionData.SharedFrozenDepth.Value = Null;
							  otherSide.Stop = false;
						 }
					}
			  }
		 }

		 private void MonitorData( DirectionData directionData, DirectionData otherSide, Node connectingNode )
		 {
			  ResolveMonitor( directionData.StartNode );
			  if ( _dataMonitor != null )
			  {
					_dataMonitor.monitorData( directionData.VisitedNodes, directionData.NextNodes, otherSide.VisitedNodes, otherSide.NextNodes, connectingNode );
			  }
		 }

		 private ICollection<Path> FilterPaths( ICollection<Path> paths )
		 {
			  if ( _predicate == null )
			  {
					return paths;
			  }
			  else
			  {
					ICollection<Path> filteredPaths = new List<Path>();
					foreach ( Path path in paths )
					{
						 if ( _predicate.test( path ) )
						 {
							  filteredPaths.Add( path );
						 }
					}
					return filteredPaths;
			  }
		 }

		 public interface DataMonitor
		 {
			  void MonitorData( IDictionary<Node, LevelData> theseVisitedNodes, ICollection<Node> theseNextNodes, IDictionary<Node, LevelData> thoseVisitedNodes, ICollection<Node> thoseNextNodes, Node connectingNode );
		 }

		 // Two long-lived instances
		 private class DirectionData : PrefetchingResourceIterator<Node>
		 {
			 private readonly ShortestPath _outerInstance;

			  internal bool FinishCurrentLayerThenStop;
			  internal readonly Node StartNode;
			  internal int CurrentDepth;
			  internal ResourceIterator<Relationship> NextRelationships;
			  internal readonly ICollection<Node> NextNodes = new List<Node>();
			  internal readonly IDictionary<Node, LevelData> VisitedNodes = new Dictionary<Node, LevelData>();
			  internal readonly ICollection<long> SharedVisitedRels;
			  internal readonly DirectionDataPath LastPath;
			  internal readonly MutableInt SharedFrozenDepth;
			  internal readonly MutableBoolean SharedStop;
			  internal readonly MutableInt SharedCurrentDepth;
			  internal bool HaveFoundSomething;
			  internal bool Stop;
			  internal readonly PathExpander Expander;

			  internal DirectionData( ShortestPath outerInstance, Node startNode, ICollection<long> sharedVisitedRels, MutableInt sharedFrozenDepth, MutableBoolean sharedStop, MutableInt sharedCurrentDepth, PathExpander expander )
			  {
				  this._outerInstance = outerInstance;
					this.StartNode = startNode;
					this.VisitedNodes[startNode] = new LevelData( null, 0 );
					this.NextNodes.Add( startNode );
					this.SharedFrozenDepth = sharedFrozenDepth;
					this.SharedStop = sharedStop;
					this.SharedCurrentDepth = sharedCurrentDepth;
					this.Expander = expander;
					this.SharedVisitedRels = sharedVisitedRels;
					this.LastPath = new DirectionDataPath( startNode );
					if ( sharedCurrentDepth.intValue() < outerInstance.maxDepth )
					{
						 PrepareNextLevel();
					}
					else
					{
						 this.NextRelationships = Iterators.emptyResourceIterator();
					}
			  }

			  internal virtual void PrepareNextLevel()
			  {
					ICollection<Node> nodesToIterate = new List<Node>( this.NextNodes );
					this.NextNodes.Clear();
					this.LastPath.Length = CurrentDepth;
					CloseRelationshipsIterator();
					this.NextRelationships = new NestingResourceIteratorAnonymousInnerClass( this, nodesToIterate.GetEnumerator() );
					this.CurrentDepth++;
					this.SharedCurrentDepth.increment();
			  }

			  private class NestingResourceIteratorAnonymousInnerClass : NestingResourceIterator<Relationship, Node>
			  {
				  private readonly DirectionData _outerInstance;

				  public NestingResourceIteratorAnonymousInnerClass( DirectionData outerInstance, UnknownType iterator ) : base( iterator )
				  {
					  this.outerInstance = outerInstance;
				  }

				  protected internal override ResourceIterator<Relationship> createNestedIterator( Node node )
				  {
						_outerInstance.lastPath.EndNode = node;
						return Iterators.asResourceIterator( _outerInstance.expander.expand( _outerInstance.lastPath, BranchState.NO_STATE ).GetEnumerator() );
				  }
			  }

			  internal virtual void CloseRelationshipsIterator()
			  {
					if ( this.NextRelationships != null )
					{
						 this.NextRelationships.close();
					}
			  }

			  public override void Close()
			  {
					CloseRelationshipsIterator();
			  }

			  protected internal override Node FetchNextOrNull()
			  {
					while ( true )
					{
						 Relationship nextRel = FetchNextRelOrNull();
						 if ( nextRel == null )
						 {
							  return null;
						 }

						 Node result = nextRel.GetOtherNode( this.LastPath.endNode() );

						 if ( outerInstance.FilterNextLevelNodes( result ) != null )
						 {
							  outerInstance.lastMetadata.Rels++;

							  LevelData levelData = this.VisitedNodes[result];
							  if ( levelData == null )
							  {
									levelData = new LevelData( nextRel, this.CurrentDepth );
									this.VisitedNodes[result] = levelData;
									this.NextNodes.Add( result );
									return result;
							  }
							  else if ( this.CurrentDepth == levelData.Depth )
							  {
									levelData.AddRel( nextRel );
							  }
						 }
					}
			  }

			  internal virtual bool CanGoDeeper()
			  {
					return ( this.SharedFrozenDepth.intValue() == outerInstance.Null ) && (this.SharedCurrentDepth.intValue() < outerInstance.maxDepth) && !FinishCurrentLayerThenStop;
			  }

			  internal virtual Relationship FetchNextRelOrNull()
			  {
					if ( this.Stop || this.SharedStop.booleanValue() )
					{
						 return null;
					}
					bool hasComeTooFarEmptyHanded = ( this.SharedFrozenDepth.intValue() != outerInstance.Null ) && (this.SharedCurrentDepth.intValue() > this.SharedFrozenDepth.intValue()) && !this.HaveFoundSomething;
					if ( hasComeTooFarEmptyHanded )
					{
						 return null;
					}
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( !this.NextRelationships.hasNext() )
					{
						 if ( CanGoDeeper() )
						 {
							  PrepareNextLevel();
						 }
					}
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					return this.NextRelationships.hasNext() ? this.NextRelationships.next() : null;
			  }
		 }

		 // Two long-lived instances
		 private class DirectionDataPath : Path
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly Node StartNodeConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal Node EndNodeConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal int LengthConflict;

			  internal DirectionDataPath( Node startNode )
			  {
					this.StartNodeConflict = startNode;
					this.EndNodeConflict = startNode;
					this.LengthConflict = 0;
			  }

			  internal virtual Node EndNode
			  {
				  set
				  {
						this.EndNodeConflict = value;
				  }
			  }

			  internal virtual int Length
			  {
				  set
				  {
						this.LengthConflict = value;
				  }
			  }

			  public override Node StartNode()
			  {
					return StartNodeConflict;
			  }

			  public override Node EndNode()
			  {
					return EndNodeConflict;
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
					return LengthConflict;
			  }

			  public override IEnumerator<PropertyContainer> Iterator()
			  {
					throw new System.NotSupportedException();
			  }
		 }

		 protected internal virtual Node FilterNextLevelNodes( Node nextNode )
		 {
			  // We need to be able to override this method from Cypher, so it must exist in this concrete class.
			  // And we also need it to do nothing but still work when not overridden.
			  return nextNode;
		 }

		 // Many long-lived instances
		 public class LevelData
		 {
			  internal long[] RelsToHere;
			  public readonly int Depth;

			  internal LevelData( Relationship relToHere, int depth )
			  {
					if ( relToHere != null )
					{
						 AddRel( relToHere );
					}
					this.Depth = depth;
			  }

			  internal virtual void AddRel( Relationship rel )
			  {
					long[] newRels = null;
					if ( RelsToHere == null )
					{
						 newRels = new long[1];
					}
					else
					{
						 newRels = new long[RelsToHere.Length + 1];
						 Array.Copy( RelsToHere, 0, newRels, 0, RelsToHere.Length );
					}
					newRels[newRels.Length - 1] = rel.Id;
					RelsToHere = newRels;
			  }
		 }

		 // One long lived instance
		 private class Hits
		 {
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  internal readonly MutableIntObjectMap<ICollection<Hit>> HitsConflict = new IntObjectHashMap<ICollection<Hit>>();
			  internal int LowestDepth;
			  internal int TotalHitCount;

			  internal virtual int Add( Hit hit, int atDepth )
			  {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
					ICollection<Hit> depthHits = HitsConflict.getIfAbsentPut( atDepth, HashSet<object>::new );
					if ( depthHits.Add( hit ) )
					{
						 TotalHitCount++;
					}
					if ( LowestDepth == 0 || atDepth < LowestDepth )
					{
						 LowestDepth = atDepth;
					}
					return TotalHitCount;
			  }

			  internal virtual ICollection<Hit> Least()
			  {
					return HitsConflict.get( LowestDepth );
			  }
		 }

		 // Methods for converting data representing paths to actual Path instances.
		 // It's rather tricky just because this algo stores as little info as possible
		 // required to build paths from hit information.
		 private class PathData
		 {
			  internal readonly LinkedList<Relationship> Rels;
			  internal readonly Node Node;

			  internal PathData( Node node, LinkedList<Relationship> rels )
			  {
					this.Rels = rels;
					this.Node = node;
			  }
		 }

		 private static ICollection<Path> HitsToPaths( ICollection<Hit> depthHits, Node start, Node end, bool stopAsap, int maxResultCount )
		 {
			  LinkedHashMap<string, Path> paths = new LinkedHashMap<string, Path>();
			  foreach ( Hit hit in depthHits )
			  {
					foreach ( Path path in HitToPaths( hit, start, end, stopAsap ) )
					{
						 paths.put( path.ToString(), path );
						 if ( paths.size() >= maxResultCount )
						 {
							  break;
						 }
					}
			  }
			  return paths.values();
		 }

		 private static ICollection<Path> HitToPaths( Hit hit, Node start, Node end, bool stopAsap )
		 {
			  ICollection<Path> paths = new List<Path>();
			  IEnumerable<LinkedList<Relationship>> startPaths = GetPaths( hit.ConnectingNode, hit.Start, stopAsap );
			  IEnumerable<LinkedList<Relationship>> endPaths = GetPaths( hit.ConnectingNode, hit.End, stopAsap );
			  foreach ( LinkedList<Relationship> startPath in startPaths )
			  {
					PathImpl.Builder startBuilder = ToBuilder( start, startPath );
					foreach ( LinkedList<Relationship> endPath in endPaths )
					{
						 PathImpl.Builder endBuilder = ToBuilder( end, endPath );
						 Path path = startBuilder.Build( endBuilder );
						 paths.Add( path );
					}
			  }
			  return paths;
		 }

		 private static IEnumerable<LinkedList<Relationship>> GetPaths( Node connectingNode, DirectionData data, bool stopAsap )
		 {
			  LevelData levelData = data.VisitedNodes[connectingNode];
			  if ( levelData.Depth == 0 )
			  {
					ICollection<LinkedList<Relationship>> result = new List<LinkedList<Relationship>>();
					result.Add( new LinkedList<>() );
					return result;
			  }
			  ICollection<PathData> set = new List<PathData>();
			  IGraphDatabaseService graphDb = data.StartNode.GraphDatabase;
			  foreach ( long rel in levelData.RelsToHere )
			  {
					set.Add( new PathData( connectingNode, new LinkedList<Relationship>( Arrays.asList( graphDb.GetRelationshipById( rel ) ) ) ) );
					if ( stopAsap )
					{
						 break;
					}
			  }
			  for ( int i = 0; i < levelData.Depth - 1; i++ )
			  {
					// One level
					ICollection<PathData> nextSet = new List<PathData>();
					foreach ( PathData entry in set )
					{
						 // One path...
						 Node otherNode = entry.Rels.First.Value.getOtherNode( entry.Node );
						 LevelData otherLevelData = data.VisitedNodes[otherNode];
						 int counter = 0;
						 foreach ( long rel in otherLevelData.RelsToHere )
						 {
							  // ...may split into several paths
							  LinkedList<Relationship> rels = ++counter == otherLevelData.RelsToHere.Length ? entry.Rels : new LinkedList<Relationship>( entry.Rels );
							  rels.AddFirst( graphDb.GetRelationshipById( rel ) );
							  nextSet.Add( new PathData( otherNode, rels ) );
							  if ( stopAsap )
							  {
									break;
							  }
						 }
					}
					set = nextSet;
			  }
			  return new IterableWrapperAnonymousInnerClass( set );
		 }

		 private class IterableWrapperAnonymousInnerClass : IterableWrapper<LinkedList<Relationship>, PathData>
		 {
			 public IterableWrapperAnonymousInnerClass( ICollection<PathData> set ) : base( set )
			 {
			 }

			 protected internal override LinkedList<Relationship> underlyingObjectToObject( PathData @object )
			 {
				  return @object.Rels;
			 }
		 }

		 private static PathImpl.Builder ToBuilder( Node startNode, LinkedList<Relationship> rels )
		 {
			  PathImpl.Builder builder = new PathImpl.Builder( startNode );
			  foreach ( Relationship rel in rels )
			  {
					builder = builder.Push( rel );
			  }
			  return builder;
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