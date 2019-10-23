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

	public class PriorityMap<E, K, P>
	{
		 public interface IConverter<T, S>
		 {
			  T Convert( S source );
		 }

		 public sealed class Entry<E, P>
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly E EntityConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly P PriorityConflict;

			  internal Entry( E IEntity, P priority )
			  {
					this.EntityConflict = IEntity;
					this.PriorityConflict = priority;
			  }

			  internal Entry( Node<E, P> node ) : this( node.Head.entity, node.Head.priority )
			  {
			  }

			  public E IEntity
			  {
				  get
				  {
						return EntityConflict;
				  }
			  }

			  public P Priority
			  {
				  get
				  {
						return PriorityConflict;
				  }
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("rawtypes") private static final Converter SELF_KEY = source -> source;
		 private static readonly Converter _selfKey = source => source;
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static <K, P> PriorityMap<K, K, P> withSelfKey(java.util.Comparator<P> priority)
		 public static PriorityMap<K, K, P> WithSelfKey<K, P>( IComparer<P> priority )
		 {
			  return new PriorityMap<K, K, P>( _selfKey, priority, true );
		 }

		 private class NaturalPriority<P> : IComparer<P> where P : IComparable<P>
		 {
			  internal readonly bool Reversed;

			  internal NaturalPriority( bool reversed )
			  {
					this.Reversed = reversed;
			  }

			  public override int Compare( P o1, P o2 )
			  {
					return Reversed ? o2.compareTo( o1 ) : o1.compareTo( o2 );
			  }
		 }
		 public static PriorityMap<E, K, P> WithNaturalOrder<E, K, P>( Converter<K, E> key ) where P : IComparable<P>
		 {
			  return PriorityMap.WithNaturalOrder( key, false );
		 }
		 public static PriorityMap<E, K, P> WithNaturalOrder<E, K, P>( Converter<K, E> key, bool reversed ) where P : IComparable<P>
		 {
			  return WithNaturalOrder( key, reversed, true );
		 }
		 public static PriorityMap<E, K, P> WithNaturalOrder<E, K, P>( Converter<K, E> key, bool reversed, bool onlyKeepBestPriorities ) where P : IComparable<P>
		 {
			  IComparer<P> priority = new NaturalPriority<P>( reversed );
			  return new PriorityMap<E, K, P>( key, priority, onlyKeepBestPriorities );
		 }

		 public static PriorityMap<K, K, P> WithSelfKeyNaturalOrder<K, P>() where P : IComparable<P>
		 {
			  return PriorityMap.WithSelfKeyNaturalOrder( false );
		 }

		 public static PriorityMap<K, K, P> WithSelfKeyNaturalOrder<K, P>( bool reversed ) where P : IComparable<P>
		 {
			  return PriorityMap.WithSelfKeyNaturalOrder( reversed, true );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") public static <K, P extends Comparable<P>> PriorityMap<K, K, P> withSelfKeyNaturalOrder(boolean reversed, boolean onlyKeepBestPriorities)
		 public static PriorityMap<K, K, P> WithSelfKeyNaturalOrder<K, P>( bool reversed, bool onlyKeepBestPriorities ) where P : IComparable<P>
		 {
			  IComparer<P> priority = new NaturalPriority<P>( reversed );
			  return new PriorityMap<K, K, P>( _selfKey, priority, onlyKeepBestPriorities );
		 }

		 private readonly Converter<K, E> _keyFunction;
		 private readonly IComparer<P> _order;
		 private readonly bool _onlyKeepBestPriorities;

		 public PriorityMap( Converter<K, E> key, IComparer<P> priority, bool onlyKeepBestPriorities )
		 {
			  this._keyFunction = key;
			  this._order = priority;
			  this._onlyKeepBestPriorities = onlyKeepBestPriorities;
		 }

		 /// <summary>
		 /// Add an IEntity to the priority map. If the key for the {@code IEntity}
		 /// was already found in the priority map and the priority is the same
		 /// the IEntity will be added. If the priority is lower the existing entities
		 /// for that key will be discarded.
		 /// </summary>
		 /// <param name="entity"> the IEntity to add. </param>
		 /// <param name="priority"> the priority of the IEntity. </param>
		 /// <returns> whether or not the IEntity (with its priority) was added to the
		 /// priority map. Will return {@code false} iff the key for the IEntity
		 /// already exist and its priority is better than the given
		 /// {@code priority}. </returns>
		 public virtual bool Put( E IEntity, P priority )
		 {
			  K key = _keyFunction.convert( IEntity );
			  Node<E, P> node = _map[key];
			  bool result = false;
			  if ( node != null )
			  { // it already existed
					if ( _onlyKeepBestPriorities )
					{
						 if ( _order.Compare( priority, node.Head.priority ) == 0 )
						 { // ...with same priority => add as a candidate first in chain
							  node.Head = new Link<P>( IEntity, priority, node.Head );
							  result = true;
						 }
						 else if ( _order.Compare( priority, node.Head.priority ) < 0 )
						 { // ...with lower (better) priority => this new one replaces any existing
							  queue.remove( node );
							  PutNew( IEntity, priority, key );
							  result = true;
						 }
					}
					else
					{ // put in the appropriate place in the node linked list
						 if ( _order.Compare( priority, node.Head.priority ) < 0 )
						 { // ...first in chain and re-insert to queue
							  node.Head = new Link<P>( IEntity, priority, node.Head );
							  Reinsert( node );
							  result = true;
						 }
						 else
						 { // we couldn't add it first in chain, go look for the appropriate place
							  Link<E, P> link = node.Head;
							  Link<E, P> prev = link;
							  // skip the first one since we already compared head
							  link = link.Next;
							  while ( link != null )
							  {
									if ( _order.Compare( priority, link.Priority ) <= 0 )
									{ // here's our place, put it
										 // NODE ==> N ==> N ==> N
										 prev.Next = new Link<P>( IEntity, priority, link );
										 result = true;
										 break;
									}
									prev = link;
									link = link.Next;
							  }
							  if ( !result )
							  { // not added so append last in the chain
									prev.Next = new Link<P>( IEntity, priority, null );
									result = true;
							  }
						 }
					}
			  }
			  else
			  { // Didn't exist, just put
					PutNew( IEntity, priority, key );
					result = true;
			  }
			  return result;
		 }

		 private void PutNew( E IEntity, P priority, K key )
		 {
			  Node<E, P> node = new Node<E, P>( new Link<>( IEntity, priority, null ) );
			  _map[key] = node;
			  queue.add( node );
		 }

		 private void Reinsert( Node<E, P> node )
		 {
			  queue.remove( node );
			  queue.add( node );
		 }

		 /// <summary>
		 /// Get the priority for the IEntity with the specified key.
		 /// </summary>
		 /// <param name="key"> the key. </param>
		 /// <returns> the priority for the the IEntity with the specified key. </returns>
		 public virtual P Get( K key )
		 {
			  Node<E, P> node = _map[key];
			  return node != null ? node.Head.priority : default( P );
		 }

		 /// <summary>
		 /// Remove and return the entry with the highest priority.
		 /// </summary>
		 /// <returns> the entry with the highest priority. </returns>
		 public virtual Entry<E, P> Pop()
		 {
			  Node<E, P> node = queue.peek();
			  Entry<E, P> result = null;
			  if ( node == null )
			  {
					// Queue is empty
					return null;
			  }
			  else if ( node.Head.next == null )
			  {
					// There are no more entries attached to this key
					// Poll from queue and remove from map.
					node = queue.poll();
					_map.Remove( _keyFunction.convert( node.Head.entity ) );
					result = new Entry<E, P>( node );
			  }
			  else
			  {
					result = new Entry<E, P>( node );
					node.Head = node.Head.next;
					if ( _order.Compare( result.PriorityConflict, node.Head.priority ) == 0 )
					{
						 // Can leave at front of queue as priority is the same
						 // Do nothing
					}
					else
					{
						 // node needs to be reinserted into queue
						 Reinsert( node );
					}

			  }
			  return result;
		 }

		 public virtual Entry<E, P> Peek()
		 {
			  Node<E, P> node = queue.peek();
			  if ( node == null )
			  {
					return null;
			  }
			  return new Entry<E, P>( node );
		 }

		 // Naive implementation

		 private readonly IDictionary<K, Node<E, P>> _map = new Dictionary<K, Node<E, P>>();
		 private readonly PriorityQueue<Node<E, P>> queue = new PriorityQueue<Node<E, P>>( 11, new ComparatorAnonymousInnerClass() );

		 private class ComparatorAnonymousInnerClass : IComparer<Node<E, P>>
		 {
			 public int compare( Node<E, P> o1, Node<E, P> o2 )
			 {
				  return outerInstance.order.Compare( o1.Head.priority, o2.Head.priority );
			 }
		 }

		 private class Node<E, P>
		 {
			  internal Link<E, P> Head;

			  internal Node( Link<E, P> head )
			  {
					this.Head = head;
			  }
		 }

		 private class Link<E, P>
		 {
			  internal readonly E IEntity;
			  internal readonly P Priority;
			  internal Link<E, P> Next;

			  internal Link( E IEntity, P priority, Link<E, P> next )
			  {
					this.Entity = IEntity;
					this.Priority = priority;
					this.Next = next;
			  }
		 }
	}

}