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
namespace Neo4Net.Graphalgo.impl.shortestpath
{

	using Neo4Net.Graphalgo.impl.util;
	using Node = Neo4Net.Graphdb.Node;

	/// <summary>
	/// Implementation of <seealso cref="DijkstraPriorityQueue"/> using a <seealso cref="FibonacciHeap"/> </summary>
	/// @param <CostType>
	///            The datatype the path weights are represented by. </param>
	public class DijkstraPriorityQueueFibonacciImpl<CostType> : DijkstraPriorityQueue<CostType>
	{
		 /// <summary>
		 /// Data structure used for the internal priority heap
		 /// </summary>
		 protected internal class HeapObject
		 {
			 private readonly DijkstraPriorityQueueFibonacciImpl<CostType> _outerInstance;

//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal Node NodeConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal CostType CostConflict;

			  public HeapObject( DijkstraPriorityQueueFibonacciImpl<CostType> outerInstance, Node node, CostType cost )
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
//ORIGINAL LINE: final HeapObject other = (HeapObject) obj;
					HeapObject other = ( HeapObject ) obj;
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
					return NodeConflict == null ? 23 : 14 ^ NodeConflict.GetHashCode();
			  }
		 }

		 internal IDictionary<Node, FibonacciHeap<HeapObject>.FibonacciHeapNode> HeapNodes = new Dictionary<Node, FibonacciHeap<HeapObject>.FibonacciHeapNode>();
		 internal FibonacciHeap<HeapObject> Heap;

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public DijkstraPriorityQueueFibonacciImpl(final java.util.Comparator<CostType> costComparator)
		 public DijkstraPriorityQueueFibonacciImpl( IComparer<CostType> costComparator ) : base()
		 {
			  Heap = new FibonacciHeap<HeapObject>( ( IComparer<HeapObject> )( o1, o2 ) => costComparator.Compare( o1.Cost, o2.Cost ) );
		 }

		 public override void DecreaseValue( Node node, CostType newValue )
		 {
			  FibonacciHeap<HeapObject>.FibonacciHeapNode fNode = HeapNodes[node];
			  Heap.decreaseKey( fNode, new HeapObject( this, node, newValue ) );
		 }

		 public override Node ExtractMin()
		 {
			  HeapObject heapObject = Heap.extractMin();
			  if ( heapObject == null )
			  {
					return null;
			  }
			  return heapObject.Node;
		 }

		 public override void InsertValue( Node node, CostType value )
		 {
			  FibonacciHeap<HeapObject>.FibonacciHeapNode fNode = Heap.insert( new HeapObject( this, node, value ) );
			  HeapNodes[node] = fNode;
		 }

		 public virtual bool Empty
		 {
			 get
			 {
				  return Heap.Empty;
			 }
		 }

		 public override Node Peek()
		 {
			  FibonacciHeap<HeapObject>.FibonacciHeapNode fNode = Heap.Minimum;
			  if ( fNode == null )
			  {
					return null;
			  }
			  return fNode.Key.Node;
		 }
	}

}