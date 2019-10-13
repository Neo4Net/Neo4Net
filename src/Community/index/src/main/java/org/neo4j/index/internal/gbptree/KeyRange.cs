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
namespace Neo4Net.Index.@internal.gbptree
{

	internal class KeyRange<KEY>
	{
		 private readonly int _level;
		 private readonly long _pageId;
		 private readonly IComparer<KEY> _comparator;
		 private readonly KEY _fromInclusive;
		 private readonly KEY _toExclusive;
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private final Layout<KEY,?> layout;
		 private readonly Layout<KEY, ?> _layout;
		 private readonly KeyRange<KEY> _superRange;

		 internal KeyRange<T1>( int level, long pageId, IComparer<KEY> comparator, KEY fromInclusive, KEY toExclusive, Layout<T1> layout, KeyRange<KEY> superRange )
		 {
			  this._level = level;
			  this._pageId = pageId;
			  this._comparator = comparator;
			  this._superRange = superRange;
			  this._fromInclusive = fromInclusive == default( KEY ) ? default( KEY ) : layout.CopyKey( fromInclusive, layout.NewKey() );
			  this._toExclusive = toExclusive == default( KEY ) ? default( KEY ) : layout.CopyKey( toExclusive, layout.NewKey() );
			  this._layout = layout;
		 }

		 internal virtual bool InRange( KEY key )
		 {
			  if ( _fromInclusive != default( KEY ) )
			  {
					if ( _toExclusive != default( KEY ) )
					{
						 return _comparator.Compare( key, _fromInclusive ) >= 0 && _comparator.Compare( key, _toExclusive ) < 0;
					}
					return _comparator.Compare( key, _fromInclusive ) >= 0;
			  }
			  return _toExclusive == default( KEY ) || _comparator.Compare( key, _toExclusive ) < 0;
		 }

		 internal virtual KeyRange<KEY> NewSubRange( int level, long pageId )
		 {
			  return new KeyRange<KEY>( level, pageId, _comparator, _fromInclusive, _toExclusive, _layout, this );
		 }

		 internal virtual bool HasPageIdInStack( long pageId )
		 {
			  if ( this._pageId == pageId )
			  {
					return true;
			  }
			  if ( _superRange != null )
			  {
					return _superRange.hasPageIdInStack( pageId );
			  }
			  return false;
		 }

		 internal virtual KeyRange<KEY> RestrictLeft( KEY left )
		 {
			  KEY newLeft;
			  if ( _fromInclusive == default( KEY ) )
			  {
					newLeft = left;
			  }
			  else if ( left == default( KEY ) )
			  {
					newLeft = _fromInclusive;
			  }
			  else if ( _comparator.Compare( _fromInclusive, left ) < 0 )
			  {
					newLeft = left;
			  }
			  else
			  {
					newLeft = _fromInclusive;
			  }
			  return new KeyRange<KEY>( _level, _pageId, _comparator, newLeft, _toExclusive, _layout, _superRange );
		 }

		 internal virtual KeyRange<KEY> RestrictRight( KEY right )
		 {
			  KEY newRight;
			  if ( _toExclusive == default( KEY ) )
			  {
					newRight = right;
			  }
			  else if ( right == default( KEY ) )
			  {
					newRight = _toExclusive;
			  }
			  else if ( _comparator.Compare( _toExclusive, right ) > 0 )
			  {
					newRight = right;
			  }
			  else
			  {
					newRight = _toExclusive;
			  }
			  return new KeyRange<KEY>( _level, _pageId, _comparator, _fromInclusive, newRight, _layout, _superRange );
		 }

		 public override string ToString()
		 {
			  return ( _superRange != null ? format( "%s%n", _superRange ) : "" ) + SingleLevel();
		 }

		 private string SingleLevel()
		 {
			  return "level: " + _level + " {" + _pageId + "} " + _fromInclusive + " ≤ key < " + _toExclusive;
		 }
	}

}