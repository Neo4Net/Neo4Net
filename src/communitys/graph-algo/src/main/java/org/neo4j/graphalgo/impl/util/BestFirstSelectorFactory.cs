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
namespace Neo4Net.Graphalgo.impl.util
{

	using Neo4Net.Graphalgo.impl.util.PriorityMap;
	using Neo4Net.Graphalgo.impl.util.PriorityMap;
	using Node = Neo4Net.Graphdb.Node;
	using Path = Neo4Net.Graphdb.Path;
	using Neo4Net.Graphdb;
	using BranchOrderingPolicy = Neo4Net.Graphdb.traversal.BranchOrderingPolicy;
	using BranchSelector = Neo4Net.Graphdb.traversal.BranchSelector;
	using TraversalBranch = Neo4Net.Graphdb.traversal.TraversalBranch;
	using TraversalContext = Neo4Net.Graphdb.traversal.TraversalContext;

	public abstract class BestFirstSelectorFactory<P, D> : BranchOrderingPolicy where P : IComparable<P>
	{
		 private readonly PathInterest<P> _interest;

		 public BestFirstSelectorFactory( PathInterest<P> interest )
		 {
			  this._interest = interest;
		 }

		 public override BranchSelector Create( TraversalBranch startSource, PathExpander expander )
		 {
			  return new BestFirstSelector( this, startSource, StartData, expander );
		 }

		 protected internal abstract P StartData { get; }

		 private class Visit<P> : IComparable<P> where P : IComparable<P>
		 {
			  internal P Cost;
			  internal int VisitCount;

			  internal Visit( P cost )
			  {
					this.Cost = cost;
			  }

			  public override int CompareTo( P o )
			  {
					return Cost.compareTo( o );
			  }
		 }

		 public sealed class BestFirstSelector : BranchSelector
		 {
			 internal bool InstanceFieldsInitialized = false;

			 internal void InitializeInstanceFields()
			 {
				 Queue = new PriorityMap<TraversalBranch, Node, P>( Converter, outerInstance.interest.Comparator(), outerInstance.interest.StopAfterLowestCost() );
			 }

			 private readonly BestFirstSelectorFactory<P, D> _outerInstance;

			  internal PriorityMap<TraversalBranch, Node, P> Queue;
			  internal TraversalBranch Current;
			  internal P CurrentAggregatedValue;
			  internal readonly PathExpander Expander;
			  internal readonly IDictionary<long, Visit<P>> Visits = new Dictionary<long, Visit<P>>();

			  public BestFirstSelector( BestFirstSelectorFactory<P, D> outerInstance, TraversalBranch source, P startData, PathExpander expander )
			  {
				  this._outerInstance = outerInstance;

				  if ( !InstanceFieldsInitialized )
				  {
					  InitializeInstanceFields();
					  InstanceFieldsInitialized = true;
				  }
					this.Current = source;
					this.CurrentAggregatedValue = startData;
					this.Expander = expander;
			  }

			  public override TraversalBranch Next( TraversalContext metadata )
			  {
					// Exhaust current if not already exhausted
					while ( true )
					{
						 TraversalBranch next = Current.next( Expander, metadata );
						 if ( next == null )
						 {
							  break;
						 }

						 long endNodeId = next.EndNode().Id;
						 Visit<P> stay = Visits[endNodeId];

						 D cost = outerInstance.CalculateValue( next );
						 P newPriority = outerInstance.AddPriority( next, CurrentAggregatedValue, cost );

						 bool newStay = stay == null;
						 if ( newStay )
						 {
							  stay = new Visit<P>( newPriority );
							  Visits[endNodeId] = stay;
						 }
						 if ( newStay || !outerInstance.interest.CanBeRuledOut( stay.VisitCount, newPriority, stay.Cost ) )
						 {
							  if ( outerInstance.interest.Comparator().Compare(newPriority, stay.Cost) < 0 )
							  {
									stay.Cost = newPriority;
							  }
							  Queue.put( next, newPriority );
						 }
					}

					do
					{
						 // Pop the top from priorityMap
						 Entry<TraversalBranch, P> entry = Queue.pop();
						 if ( entry != null )
						 {
							  Current = entry.Entity;
							  Visit<P> stay = Visits[Current.endNode().Id];
							  stay.VisitCount++;
							  if ( outerInstance.interest.StillInteresting( stay.VisitCount ) )
							  {
									CurrentAggregatedValue = entry.Priority;
									return Current;
							  }
						 }
						 else
						 {
							  return null;
						 }
					} while ( true );
			  }
		 }

		 protected internal abstract P AddPriority( TraversalBranch source, P currentAggregatedValue, D value );

		 protected internal abstract D CalculateValue( TraversalBranch next );

//JAVA TO C# CONVERTER TODO TASK: Method reference arbitrary object instance method syntax is not converted by Java to C# Converter:
		 public static readonly Converter<Node, TraversalBranch> Converter = Path::endNode;
	}

}