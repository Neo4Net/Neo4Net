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
namespace Neo4Net.Kernel.impl.traversal
{

	using Direction = Neo4Net.GraphDb.Direction;
	using Node = Neo4Net.GraphDb.Node;
	using Path = Neo4Net.GraphDb.Path;
	using Resource = Neo4Net.GraphDb.Resource;
	using BidirectionalUniquenessFilter = Neo4Net.GraphDb.Traversal.BidirectionalUniquenessFilter;
	using BranchCollisionDetector = Neo4Net.GraphDb.Traversal.BranchCollisionDetector;
	using BranchSelector = Neo4Net.GraphDb.Traversal.BranchSelector;
	using Neo4Net.GraphDb.Traversal;
	using Evaluation = Neo4Net.GraphDb.Traversal.Evaluation;
	using Neo4Net.GraphDb.Traversal;
	using SideSelector = Neo4Net.GraphDb.Traversal.SideSelector;
	using SideSelectorPolicy = Neo4Net.GraphDb.Traversal.SideSelectorPolicy;
	using TraversalBranch = Neo4Net.GraphDb.Traversal.TraversalBranch;
	using TraversalContext = Neo4Net.GraphDb.Traversal.TraversalContext;
	using UniquenessFilter = Neo4Net.GraphDb.Traversal.UniquenessFilter;

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

		 internal BidirectionalTraverserIterator( Resource resource, MonoDirectionalTraversalDescription start, MonoDirectionalTraversalDescription end, SideSelectorPolicy sideSelector, Neo4Net.GraphDb.Traversal.BranchCollisionPolicy collisionPolicy, PathEvaluator collisionEvaluator, int maxDepth, IEnumerable<Node> startNodes, IEnumerable<Node> endNodes ) : base( resource )
		 {
			  this._sides[Direction.OUTGOING] = new Side( start );
			  this._sides[Direction.INCOMING] = new Side( end );
			  this._uniqueness = MakeSureStartAndEndHasSameUniqueness( start, end );

			  // A little chicken-and-egg problem. This happens when constructing the start/end
			  // selectors and they initially call Evaluate() and isUniqueFirst, where the selector is used.
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
//ORIGINAL LINE: private Neo4Net.graphdb.traversal.SideSelector fixedSide(final Neo4Net.graphdb.Direction direction)
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
					IEnumerable<Path> pathCollisions = _collisionDetector.Evaluate( result, _selector.currentSide() );
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
			  return CurrentSideDescription().Description.evaluator.Evaluate(branch, state);
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