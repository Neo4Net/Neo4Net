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
namespace Org.Neo4j.Kernel.impl.traversal
{

	using Direction = Org.Neo4j.Graphdb.Direction;
	using Node = Org.Neo4j.Graphdb.Node;
	using Path = Org.Neo4j.Graphdb.Path;
	using Resource = Org.Neo4j.Graphdb.Resource;
	using BidirectionalUniquenessFilter = Org.Neo4j.Graphdb.traversal.BidirectionalUniquenessFilter;
	using BranchCollisionDetector = Org.Neo4j.Graphdb.traversal.BranchCollisionDetector;
	using BranchSelector = Org.Neo4j.Graphdb.traversal.BranchSelector;
	using Org.Neo4j.Graphdb.traversal;
	using Evaluation = Org.Neo4j.Graphdb.traversal.Evaluation;
	using Org.Neo4j.Graphdb.traversal;
	using SideSelector = Org.Neo4j.Graphdb.traversal.SideSelector;
	using SideSelectorPolicy = Org.Neo4j.Graphdb.traversal.SideSelectorPolicy;
	using TraversalBranch = Org.Neo4j.Graphdb.traversal.TraversalBranch;
	using TraversalContext = Org.Neo4j.Graphdb.traversal.TraversalContext;
	using UniquenessFilter = Org.Neo4j.Graphdb.traversal.UniquenessFilter;

	internal class BidirectionalTraverserIterator : AbstractTraverserIterator
	{
		 private readonly BranchCollisionDetector _collisionDetector;
		 private IEnumerator<Path> _foundPaths;
		 private SideSelector _selector;
		 private readonly IDictionary<Direction, Side> _sides = new Dictionary<Direction, Side>( typeof( Direction ) );
		 private readonly BidirectionalUniquenessFilter _uniqueness;

		 private class Side
		 {
			  internal readonly MonoDirectionalTraversalDescription Description;

			  internal Side( MonoDirectionalTraversalDescription description )
			  {
					this.Description = description;
			  }
		 }

		 internal BidirectionalTraverserIterator( Resource resource, MonoDirectionalTraversalDescription start, MonoDirectionalTraversalDescription end, SideSelectorPolicy sideSelector, Org.Neo4j.Graphdb.traversal.BranchCollisionPolicy collisionPolicy, PathEvaluator collisionEvaluator, int maxDepth, IEnumerable<Node> startNodes, IEnumerable<Node> endNodes ) : base( resource )
		 {
			  this._sides[Direction.OUTGOING] = new Side( start );
			  this._sides[Direction.INCOMING] = new Side( end );
			  this._uniqueness = MakeSureStartAndEndHasSameUniqueness( start, end );

			  // A little chicken-and-egg problem. This happens when constructing the start/end
			  // selectors and they initially call evaluate() and isUniqueFirst, where the selector is used.
			  // Solved this way for now, to have it return the start side to begin with.
			  this._selector = FixedSide( Direction.OUTGOING );
			  BranchSelector startSelector = start.BranchOrdering.create( new AsOneStartBranch( this, startNodes, start.InitialState, start.UniquenessConflict ), start.Expander );
			  this._selector = FixedSide( Direction.INCOMING );
			  BranchSelector endSelector = end.BranchOrdering.create( new AsOneStartBranch( this, endNodes, end.InitialState, start.UniquenessConflict ), end.Expander );

			  this._selector = sideSelector.Create( startSelector, endSelector, maxDepth );
			  this._collisionDetector = collisionPolicy.Create( collisionEvaluator, _uniqueness.checkFull );
		 }

		 private BidirectionalUniquenessFilter MakeSureStartAndEndHasSameUniqueness( MonoDirectionalTraversalDescription start, MonoDirectionalTraversalDescription end )
		 {
			  if ( !start.UniquenessConflict.Equals( end.UniquenessConflict ) )
			  {
					throw new System.ArgumentException( "Start and end uniqueness factories differ, they need to be the " + "same currently. Start side has " + start.UniquenessConflict + ", end side has " + end.UniquenessConflict );
			  }

			  bool parameterDiffers = start.UniquenessParameter == null || end.UniquenessParameter == null ? start.UniquenessParameter != end.UniquenessParameter :!start.UniquenessParameter.Equals( end.UniquenessParameter );
			  if ( parameterDiffers )
			  {
					throw new System.ArgumentException( "Start and end uniqueness parameters differ, they need to be the " + "same currently. Start side has " + start.UniquenessParameter + ", " + "end side has " + end.UniquenessParameter );
			  }

			  UniquenessFilter uniqueness = start.UniquenessConflict.create( start.UniquenessParameter );
			  if ( !( uniqueness is BidirectionalUniquenessFilter ) )
			  {
					throw new System.ArgumentException( "You must supply a BidirectionalUniquenessFilter, " + "not just a UniquenessFilter." );
			  }
			  return ( BidirectionalUniquenessFilter ) uniqueness;
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: private org.neo4j.graphdb.traversal.SideSelector fixedSide(final org.neo4j.graphdb.Direction direction)
		 private SideSelector FixedSide( Direction direction )
		 {
			  return new SideSelectorAnonymousInnerClass( this, direction );
		 }

		 private class SideSelectorAnonymousInnerClass : SideSelector
		 {
			 private readonly BidirectionalTraverserIterator _outerInstance;

			 private Direction _direction;

			 public SideSelectorAnonymousInnerClass( BidirectionalTraverserIterator outerInstance, Direction direction )
			 {
				 this.outerInstance = outerInstance;
				 this._direction = direction;
			 }

			 public TraversalBranch next( TraversalContext metadata )
			 {
				  throw new System.NotSupportedException();
			 }

			 public Direction currentSide()
			 {
				  return _direction;
			 }
		 }

		 protected internal override Path FetchNextOrNull()
		 {
			  if ( _foundPaths != null )
			  {
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					if ( _foundPaths.hasNext() )
					{
						 NumberOfPathsReturnedConflict++;
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 Path next = _foundPaths.next();
						 return next;
					}
					_foundPaths = null;
			  }

			  TraversalBranch result = null;
			  while ( true )
			  {
					result = _selector.next( this );
					if ( result == null )
					{
						 Close();
						 return null;
					}
					IEnumerable<Path> pathCollisions = _collisionDetector.evaluate( result, _selector.currentSide() );
					if ( pathCollisions != null )
					{
						 _foundPaths = pathCollisions.GetEnumerator();
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
						 if ( _foundPaths.hasNext() )
						 {
							  NumberOfPathsReturnedConflict++;
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
							  return _foundPaths.next();
						 }
					}
			  }
		 }

		 private Side CurrentSideDescription()
		 {
			  return _sides[_selector.currentSide()];
		 }

		 public override Evaluation Evaluate( TraversalBranch branch, BranchState state )
		 {
			  return CurrentSideDescription().Description.evaluator.evaluate(branch, state);
		 }

		 public override bool IsUniqueFirst( TraversalBranch branch )
		 {
			  return _uniqueness.checkFirst( branch );
		 }

		 public override bool IsUnique( TraversalBranch branch )
		 {
			  return _uniqueness.check( branch );
		 }
	}

}