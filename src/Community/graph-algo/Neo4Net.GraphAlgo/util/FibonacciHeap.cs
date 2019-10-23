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
namespace Neo4Net.GraphAlgo.Utils
{

	/// <summary>
	/// At least a partial implementation of a Fibonacci heap (a priority heap).
	/// Almost all code is based on the chapter about Fibonacci heaps in the book
	/// "Introduction to Algorithms" by Cormen, Leiserson, Rivest and Stein (second
	/// edition, 2001). Amortized times for almost all operations are O(1).
	/// extractMin() runs in amortized time O(log n), which then a delete() based
	/// upon it also would. This Fibonacci heap can store any datatype, given by the
	/// KeyType parameter, all it needs is a comparator for that type. To achieve the
	/// stated running times, it is needed that this comparator can do comparisons in
	/// constant time (usually the case).
	/// @author Patrik Larsson </summary>
	/// @param <KeyType>
	///            The datatype to be stored in this heap. </param>
	public class FibonacciHeap<KeyType>
	{
		 /// <summary>
		 /// One entry in the fibonacci heap is stored as an instance of this class.
		 /// References to such entries are required for some operations, like
		 /// decreaseKey().
		 /// </summary>
		 public class FibonacciHeapNode
		 {
			 private readonly FibonacciHeap<KeyType> _outerInstance;

			  internal FibonacciHeapNode Left;
			  internal FibonacciHeapNode Right;
			  internal FibonacciHeapNode Parent;
			  internal FibonacciHeapNode Child;
			  internal bool Marked;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal KeyType KeyConflict;
			  internal int Degree;

			  public FibonacciHeapNode( FibonacciHeap<KeyType> outerInstance, KeyType key ) : base()
			  {
				  this._outerInstance = outerInstance;
					this.KeyConflict = key;
					Left = this;
					Right = this;
			  }

			  /// <returns> the key </returns>
			  public virtual KeyType Key
			  {
				  get
				  {
						return KeyConflict;
				  }
			  }
		 }

		 internal IComparer<KeyType> KeyComparator;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 internal FibonacciHeapNode MinimumConflict;
		 internal int NrNodes;

		 public FibonacciHeap( IComparer<KeyType> keyComparator ) : base()
		 {
			  this.KeyComparator = keyComparator;
		 }

		 /// <returns> True if the heap is empty. </returns>
		 public virtual bool Empty
		 {
			 get
			 {
				  return MinimumConflict == null;
			 }
		 }

		 /// <returns> The number of entries in this heap. </returns>
		 public virtual int Size()
		 {
			  return NrNodes;
		 }

		 /// <returns> The entry with the highest priority or null if the heap is empty. </returns>
		 public virtual FibonacciHeapNode Minimum
		 {
			 get
			 {
				  return MinimumConflict;
			 }
		 }

		 /// <summary>
		 /// Internal helper function for moving nodes into the root list
		 /// </summary>
		 protected internal virtual void InsertInRootList( FibonacciHeapNode fNode )
		 {
			  fNode.Parent = null;
			  fNode.Marked = false;
			  if ( MinimumConflict == null )
			  {
					MinimumConflict = fNode;
					MinimumConflict.right = MinimumConflict;
					MinimumConflict.left = MinimumConflict;
			  }
			  else
			  {
					// insert in root list
					fNode.Left = MinimumConflict.left;
					fNode.Right = MinimumConflict;
					fNode.Left.right = fNode;
					fNode.Right.left = fNode;
					if ( KeyComparator.Compare( fNode.KeyConflict, MinimumConflict.key ) < 0 )
					{
						 MinimumConflict = fNode;
					}
			  }
		 }

		 /// <summary>
		 /// Inserts a new value into the heap. </summary>
		 /// <param name="key">
		 ///            the value to be inserted. </param>
		 /// <returns> The entry made into the heap. </returns>
		 public virtual FibonacciHeapNode Insert( KeyType key )
		 {
			  FibonacciHeapNode node = new FibonacciHeapNode( this, key );
			  InsertInRootList( node );
			  ++NrNodes;
			  return node;
		 }

		 /// <summary>
		 /// Creates the union of two heaps by absorbing the other into this one.
		 /// Note: Destroys other
		 /// </summary>
		 public virtual void Union( FibonacciHeap<KeyType> other )
		 {
			  NrNodes += other.NrNodes;
			  if ( other.MinimumConflict == null )
			  {
					return;
			  }
			  if ( MinimumConflict == null )
			  {
					MinimumConflict = other.MinimumConflict;
					return;
			  }
			  // swap left nodes
			  FibonacciHeapNode otherLeft = other.MinimumConflict.left;
			  other.MinimumConflict.left = MinimumConflict.left;
			  MinimumConflict.left = otherLeft;
			  // update their right pointers
			  MinimumConflict.left.right = MinimumConflict;
			  other.MinimumConflict.left.right = other.MinimumConflict;
			  // get min
			  if ( KeyComparator.Compare( other.MinimumConflict.key, MinimumConflict.key ) < 0 )
			  {
					MinimumConflict = other.MinimumConflict;
			  }
		 }

		 /// <summary>
		 /// This removes and returns the entry with the highest priority. </summary>
		 /// <returns> The value with the highest priority. </returns>
		 public virtual KeyType ExtractMin()
		 {
			  if ( MinimumConflict == null )
			  {
					return default( KeyType );
			  }
			  FibonacciHeapNode minNode = MinimumConflict;
			  // move all children to root list
			  if ( minNode.Child != null )
			  {
					FibonacciHeapNode child = minNode.Child;
					while ( minNode.Equals( child.Parent ) )
					{
						 FibonacciHeapNode nextChild = child.Right;
						 InsertInRootList( child );
						 child = nextChild;
					}
			  }
			  // remove minNode from root list
			  minNode.Left.right = minNode.Right;
			  minNode.Right.left = minNode.Left;
			  // update minimum
			  if ( minNode.Right.Equals( minNode ) )
			  {
					MinimumConflict = null;
			  }
			  else
			  {
					MinimumConflict = MinimumConflict.right;
					Consolidate();
			  }
			  --NrNodes;
			  return minNode.KeyConflict;
		 }

		 /// <summary>
		 /// Internal helper function.
		 /// </summary>
		 protected internal virtual void Consolidate()
		 {
			  // TODO: lower the size of this (log(n))
			  int arraySize = NrNodes + 1;
			  // arraySize = 2;
			  // for ( int a = nrNodes + 1; a < 0; a /= 2 )
			  // {
			  // arraySize++;
			  // }
			  // arraySize = (int) Math.log( (double) nrNodes )+1;
			  // FibonacciHeapNode[] A = (FibonacciHeapNode[]) new Object[arraySize];
			  // FibonacciHeapNode[] A = new FibonacciHeapNode[arraySize];
			  List<FibonacciHeapNode> nodes = new List<FibonacciHeapNode>( arraySize );
			  for ( int i = 0; i < arraySize; ++i )
			  {
					nodes.Add( null );
			  }
			  IList<FibonacciHeapNode> rootNodes = new LinkedList<FibonacciHeapNode>();
			  rootNodes.Add( MinimumConflict );
			  for ( FibonacciHeapNode n = MinimumConflict.right; !n.Equals( MinimumConflict ); n = n.Right )
			  {
					rootNodes.Add( n );
			  }
			  foreach ( FibonacciHeapNode node in rootNodes )
			  {
					// no longer a root node?
					if ( node.Parent != null )
					{
						 continue;
					}
					int d = node.Degree;
					while ( nodes[d] != null )
					{
						 FibonacciHeapNode y = nodes[d];
						 // swap?
						 if ( KeyComparator.Compare( node.KeyConflict, y.KeyConflict ) > 0 )
						 {
							  FibonacciHeapNode tmp = node;
							  node = y;
							  y = tmp;
						 }
						 Link( y, node );
						 nodes[d] = null;
						 ++d;
					}
					nodes[d] = node;
			  }
			  // throw away the root list
			  MinimumConflict = null;
			  // and rebuild it from A
			  foreach ( FibonacciHeapNode node in nodes )
			  {
					if ( node != null )
					{
						 InsertInRootList( node );
					}
			  }
		 }

		 /// <summary>
		 /// Internal helper function. Makes root node y a child of root node x.
		 /// </summary>
		 protected internal virtual void Link( FibonacciHeapNode y, FibonacciHeapNode x )
		 {
			  // remove y from root list
			  y.Left.right = y.Right;
			  y.Right.left = y.Left;
			  // make y a child of x
			  if ( x.Child == null ) // no previous children?
			  {
					y.Right = y;
					y.Left = y;
			  }
			  else
			  {
					y.Left = x.Child.left;
					y.Right = x.Child;
					y.Right.left = y;
					y.Left.right = y;
			  }
			  x.Child = y;
			  y.Parent = x;
			  // adjust degree and mark
			  x.Degree++;
			  y.Marked = false;
		 }

		 /// <summary>
		 /// Raises the priority for an entry. </summary>
		 /// <param name="node">
		 ///            The entry to receive a higher priority. </param>
		 /// <param name="newKey">
		 ///            The new value. </param>
		 public virtual void DecreaseKey( FibonacciHeapNode node, KeyType newKey )
		 {
			  if ( KeyComparator.Compare( newKey, node.KeyConflict ) > 0 )
			  {
					throw new Exception( "Trying to decrease to a greater key" );
			  }
			  node.KeyConflict = newKey;
			  FibonacciHeapNode parent = node.Parent;
			  if ( parent != null && KeyComparator.Compare( node.KeyConflict, parent.KeyConflict ) < 0 )
			  {
					Cut( node, parent );
					CascadingCut( parent );
			  }
			  if ( KeyComparator.Compare( node.KeyConflict, MinimumConflict.key ) < 0 )
			  {
					MinimumConflict = node;
			  }
		 }

		 /// <summary>
		 /// Internal helper function. This removes y's child x and moves x to the
		 /// root list.
		 /// </summary>
		 protected internal virtual void Cut( FibonacciHeapNode x, FibonacciHeapNode y )
		 {
			  // remove x from child list of y
			  x.Left.right = x.Right;
			  x.Right.left = x.Left;
			  if ( x.Right.Equals( x ) )
			  {
					y.Child = null;
			  }
			  else
			  {
					y.Child = x.Right;
			  }
			  y.Degree--;
			  // add x to root list
			  InsertInRootList( x );
		 }

		 /// <summary>
		 /// Internal helper function.
		 /// </summary>
		 protected internal virtual void CascadingCut( FibonacciHeapNode y )
		 {
			  FibonacciHeapNode parent = y.Parent;
			  if ( parent != null )
			  {
					if ( !parent.Marked )
					{
						 parent.Marked = true;
					}
					else
					{
						 Cut( y, parent );
						 CascadingCut( parent );
					}
			  }
		 }
	}

}