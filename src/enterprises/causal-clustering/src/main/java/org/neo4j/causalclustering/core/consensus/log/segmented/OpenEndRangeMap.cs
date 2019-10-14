using System;
using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.causalclustering.core.consensus.log.segmented
{

	/// <summary>
	/// Keeps a map of sequential ranges to values, where the highest range is open (right-side unbounded).
	/// 
	/// Typical example:
	/// [ 0,  10] -> 1     (closed range)
	/// [11, 300] -> 2     (closed range)
	/// [301,   [ -> 3     (open range)
	/// 
	/// An added range always replaces a part of existing ranges, which could either be only a part of
	/// the open range or even the entire open range and parts of the closed ranges.
	/// </summary>
	/// @param <K> Type of keys which must be comparable. </param>
	/// @param <V> Type of values stored. </param>
	internal class OpenEndRangeMap<K, V> where K : IComparable<K>
	{
		 internal class ValueRange<K, V>
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly K LimitConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly V ValueConflict;

			  internal ValueRange( K limit, V value )
			  {
					this.LimitConflict = limit;
					this.ValueConflict = value;
			  }

			  internal virtual Optional<K> Limit()
			  {
					return Optional.ofNullable( LimitConflict );
			  }

			  internal virtual Optional<V> Value()
			  {
					return Optional.ofNullable( ValueConflict );
			  }

			  public override bool Equals( object o )
			  {
					if ( this == o )
					{
						 return true;
					}
					if ( o == null || this.GetType() != o.GetType() )
					{
						 return false;
					}
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: ValueRange<?,?> that = (ValueRange<?,?>) o;
					ValueRange<object, ?> that = ( ValueRange<object, ?> ) o;
					return Objects.Equals( LimitConflict, that.LimitConflict ) && Objects.Equals( ValueConflict, that.ValueConflict );
			  }

			  public override int GetHashCode()
			  {
					return Objects.hash( LimitConflict, ValueConflict );
			  }
		 }

		 private readonly SortedDictionary<K, V> _tree = new SortedDictionary<K, V>();

		 /* We optimize by keeping the open end range directly accessible. */
		 private K _endKey;
		 private V _endValue;

		 internal virtual ICollection<V> ReplaceFrom( K from, V value )
		 {
			  ICollection<V> removed = new List<V>();

			  IEnumerator<V> itr = _tree.tailMap( from ).values().GetEnumerator();
			  while ( itr.MoveNext() )
			  {
					removed.Add( itr.Current );
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
					itr.remove();
			  }

			  _tree[from] = value;

			  _endKey = from;
			  _endValue = value;

			  return removed;
		 }

		 internal virtual ValueRange<K, V> Lookup( K at )
		 {
			  if ( _endKey != default( K ) && _endKey.compareTo( at ) <= 0 )
			  {
					return new ValueRange<K, V>( null, _endValue );
			  }

			  KeyValuePair<K, V> entry = _tree.floorEntry( at );
			  return new ValueRange<K, V>( _tree.higherKey( at ), entry != null ? entry.Value : null );
		 }

		 public virtual V Last()
		 {
			  return _endValue;
		 }

		 public virtual ISet<KeyValuePair<K, V>> EntrySet()
		 {
			  return _tree.SetOfKeyValuePairs();
		 }

		 public virtual ICollection<V> Remove( K lessThan )
		 {
			  ICollection<V> removed = new List<V>();
			  K floor = _tree.floorKey( lessThan );

			  IEnumerator<V> itr = _tree.headMap( floor, false ).values().GetEnumerator();
			  while ( itr.MoveNext() )
			  {
					removed.Add( itr.Current );
//JAVA TO C# CONVERTER TODO TASK: .NET enumerators are read-only:
					itr.remove();
			  }

			  if ( _tree.Count == 0 )
			  {
					_endKey = default( K );
					_endValue = default( V );
			  }

			  return removed;
		 }
	}

}