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
namespace Neo4Net.GraphDb.impl.traversal
{
	using BranchSelector = Neo4Net.GraphDb.traversal.BranchSelector;
	using SideSelector = Neo4Net.GraphDb.traversal.SideSelector;
	using TraversalBranch = Neo4Net.GraphDb.traversal.TraversalBranch;
	using TraversalContext = Neo4Net.GraphDb.traversal.TraversalContext;

	public abstract class AbstractSelectorOrderer<T> : SideSelector
	{
		public abstract TraversalBranch Next( TraversalContext metadata );
		 private static readonly BranchSelector _emptySelector = metadata => null;

		 private readonly BranchSelector[] _selectors;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") private final T[] states = (T[]) new Object[2];
		 private readonly T[] _states = ( T[] ) new object[2];
		 private int _selectorIndex;

		 public AbstractSelectorOrderer( BranchSelector startSelector, BranchSelector endSelector )
		 {
			  _selectors = new BranchSelector[] { startSelector, endSelector };
			  _states[0] = InitialState();
			  _states[1] = InitialState();
		 }

		 protected internal virtual T InitialState()
		 {
			  return default( T );
		 }

		 protected internal virtual T StateForCurrentSelector
		 {
			 set
			 {
				  _states[_selectorIndex] = value;
			 }
			 get
			 {
				  return _states[_selectorIndex];
			 }
		 }


		 protected internal virtual TraversalBranch NextBranchFromCurrentSelector( TraversalContext metadata, bool switchIfExhausted )
		 {
			  return NextBranchFromSelector( metadata, _selectors[_selectorIndex], switchIfExhausted );
		 }

		 protected internal virtual TraversalBranch NextBranchFromNextSelector( TraversalContext metadata, bool switchIfExhausted )
		 {
			  return NextBranchFromSelector( metadata, NextSelector(), switchIfExhausted );
		 }

		 private TraversalBranch NextBranchFromSelector( TraversalContext metadata, BranchSelector selector, bool switchIfExhausted )
		 {
			  TraversalBranch result = selector.Next( metadata );
			  if ( result == null )
			  {
					_selectors[_selectorIndex] = _emptySelector;
					if ( switchIfExhausted )
					{
						 result = NextSelector().next(metadata);
						 if ( result == null )
						 {
							  _selectors[_selectorIndex] = _emptySelector;
						 }
					}
			  }
			  return result;
		 }

		 protected internal virtual BranchSelector NextSelector()
		 {
			  _selectorIndex = ( _selectorIndex + 1 ) % 2;
			  return _selectors[_selectorIndex];
		 }

		 public override Direction CurrentSide()
		 {
			  return _selectorIndex == 0 ? Direction.OUTGOING : Direction.INCOMING;
		 }

	}

}