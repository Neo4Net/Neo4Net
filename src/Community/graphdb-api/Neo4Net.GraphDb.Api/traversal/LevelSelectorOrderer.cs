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
namespace Neo4Net.GraphDb.Traversal
{

	using Neo4Net.GraphDb.impl.traversal;

	public class LevelSelectorOrderer : AbstractSelectorOrderer<LevelSelectorOrderer.Entry>
	{
		 private readonly bool _stopDescentOnResult;
		 private readonly TotalDepth _totalDepth = new TotalDepth();
		 private readonly int _maxDepth;

		 public LevelSelectorOrderer( BranchSelector startSelector, BranchSelector endSelector, bool stopDescentOnResult, int maxDepth ) : base( startSelector, endSelector )
		 {
			  _stopDescentOnResult = stopDescentOnResult;
			  _maxDepth = maxDepth;
		 }

		 protected internal override Entry InitialState()
		 {
			  return new Entry();
		 }

		 public override ITraversalBranch Next( TraversalContext metadata )
		 {
			  ITraversalBranch branch = NextBranchFromCurrentSelector( metadata, false );
			  Entry state = StateForCurrentSelector;
			  AtomicInteger previousDepth = state.Depth;
			  if ( branch != null && branch.Length== previousDepth.get() )
			  { // Same depth as previous branch returned from this side.
					return branch;
			  }

			  if ( branch != null )
			  {
					_totalDepth.set( CurrentSide(), branch.Length);
			  }
			  if ( ( _stopDescentOnResult && ( metadata.NumberOfPathsReturned > 0 ) ) || ( _totalDepth.get() > (_maxDepth + 1) ) )
			  {
					NextSelector();
					return null;
			  }

			  if ( branch != null )
			  {
					previousDepth.set( branch.Length);
					state.Branch = branch;
			  }
			  BranchSelector otherSelector = NextSelector();
			  Entry otherState = StateForCurrentSelector;
			  ITraversalBranch otherBranch = otherState.Branch;
			  if ( otherBranch != null )
			  {
					otherState.Branch = null;
					return otherBranch;
			  }

			  otherBranch = otherSelector.Next( metadata );
			  if ( otherBranch != null )
			  {
					return otherBranch;
			  }
			  else
			  {
					return branch;
			  }
		 }

		 internal class Entry
		 {
			  internal readonly AtomicInteger Depth = new AtomicInteger();
			  internal ITraversalBranch Branch;
		 }

		 private class TotalDepth
		 {
			  internal int Out;
			  internal int In;

			  internal virtual void Set( Direction side, int depth )
			  {
					switch ( side._innerEnumValue )
					{
					case Direction.InnerEnum.OUTGOING:
						 Out = depth;
						 break;
					case Direction.InnerEnum.INCOMING:
						 In = depth;
						 break;
					default:
						 break;
					}
			  }

			  internal virtual int Get()
			  {
					return Out + In;
			  }
		 }
	}

}