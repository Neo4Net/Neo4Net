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
namespace Org.Neo4j.Graphalgo.impl.shortestpath
{

	using Node = Org.Neo4j.Graphdb.Node;

	/// <summary>
	/// Implementation of <seealso cref="DijkstraPriorityQueue"/> with just a normal java
	/// priority queue. </summary>
	/// @param <CostType>
	///            The datatype the path weights are represented by. </param>
	public class DijkstraPriorityQueueImpl<CostType> : DijkstraPriorityQueue<CostType>
	{
		 /// <summary>
		 /// Data structure used for the internal priority queue
		 /// </summary>
		 protected internal class pathObject
		 {
			 private readonly DijkstraPriorityQueueImpl<CostType> _outerInstance;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal Node NodeConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal CostType CostConflict;

			  public pathObject( DijkstraPriorityQueueImpl<CostType> outerInstance, Node node, CostType cost )
			  {
				  this._outerInstance = outerInstance;
					this.NodeConflict = node;
					this.CostConflict = cost;
			  }

			  public virtual CostType Cost
			  {
				  get
				  {
						return CostConflict;
				  }
			  }

			  public virtual Node Node
			  {
				  get
				  {
						return NodeConflict;
				  }
			  }

			  /*
			   * Equals is only defined from the stored node, so we can use it to find
			   * entries in the queue
			   */
			  public override bool Equals( object obj )
			  {
					if ( this == obj )
					{
						 return true;
					}
					if ( obj == null )
					{
						 return false;
					}
					if ( this.GetType() != obj.GetType() )
					{
						 return false;
					}
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final pathObject other = (pathObject) obj;
					pathObject other = ( pathObject ) obj;
					if ( NodeConflict == null )
					{
						 if ( other.NodeConflict != null )
						 {
							  return false;
						 }
					}
					else if ( !NodeConflict.Equals( other.NodeConflict ) )
					{
						 return false;
					}
					return true;
			  }

			  public override int GetHashCode()
			  {
					int result = NodeConflict != null ? NodeConflict.GetHashCode() : 0;
					result = 31 * result + ( CostConflict != default( CostType ) ? CostConflict.GetHashCode() : 0 );
					return result;
			  }
		 }

		 internal IComparer<CostType> CostComparator;
		 internal PriorityQueue<pathObject> Queue;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public DijkstraPriorityQueueImpl(final java.util.Comparator<CostType> costComparator)
		 public DijkstraPriorityQueueImpl( IComparer<CostType> costComparator ) : base()
		 {
			  this.CostComparator = costComparator;
			  Queue = new PriorityQueue<pathObject>( 11, ( o1, o2 ) => costComparator.Compare( o1.Cost, o2.Cost ) );
		 }

		 public override void InsertValue( Node node, CostType value )
		 {
			  Queue.add( new pathObject( this, node, value ) );
		 }

		 public override void DecreaseValue( Node node, CostType newValue )
		 {
			  pathObject po = new pathObject( this, node, newValue );
			  // Shake the queue
			  // remove() will remove the old pathObject
			  // BUT IT TAKES A LOT OF TIME FOR SOME REASON
			  // queue.remove( po );
			  Queue.add( po );
		 }

		 /// <summary>
		 /// Retrieve and remove
		 /// </summary>
		 public override Node ExtractMin()
		 {
			  pathObject po = Queue.poll();
			  if ( po == null )
			  {
					return null;
			  }
			  return po.Node;
		 }

		 /// <summary>
		 /// Retrieve without removing
		 /// </summary>
		 public override Node Peek()
		 {
			  pathObject po = Queue.peek();
			  if ( po == null )
			  {
					return null;
			  }
			  return po.Node;
		 }

		 public virtual bool Empty
		 {
			 get
			 {
				  return Queue.Empty;
			 }
		 }
	}

}