using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

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
namespace Neo4Net.Values.Storable
{

	/// <summary>
	/// A tuple of n values.
	/// </summary>
	public class ValueTuple
	{
		 public static ValueTuple Of( params Value[] values )
		 {
			  Debug.Assert( values.Length > 0, "Empty ValueTuple is not allowed" );
			  Debug.Assert( NoNulls( values ) );
			  return new ValueTuple( values );
		 }

		 public static ValueTuple Of( params object[] objects )
		 {
			  Debug.Assert( objects.Length > 0, "Empty ValueTuple is not allowed" );
			  Debug.Assert( NoNulls( objects ) );
			  Value[] values = new Value[objects.Length];
			  for ( int i = 0; i < values.Length; i++ )
			  {
					values[i] = Values.Of( objects[i] );
			  }
			  return new ValueTuple( values );
		 }

		 private readonly Value[] _values;

		 protected internal ValueTuple( Value[] values )
		 {
			  this._values = values;
		 }

		 public virtual int Size()
		 {
			  return _values.Length;
		 }

		 public virtual Value ValueAt( int offset )
		 {
			  return _values[offset];
		 }

		 /// <summary>
		 /// WARNING: this method does not create a defensive copy. Do not modify the returned array.
		 /// </summary>
		 public virtual Value[] Values
		 {
			 get
			 {
				  return _values;
			 }
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

			  ValueTuple that = ( ValueTuple ) o;

			  if ( that._values.Length != _values.Length )
			  {
					return false;
			  }

			  for ( int i = 0; i < _values.Length; i++ )
			  {
					if ( !_values[i].Equals( that._values[i] ) )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public override int GetHashCode()
		 {
			  int result = 1;
			  foreach ( object value in _values )
			  {
					result = 31 * result + value.GetHashCode();
			  }
			  return result;
		 }

		 public virtual Value OnlyValue
		 {
			 get
			 {
				  Debug.Assert( _values.Length == 1, "Assumed single value tuple, but had " + _values.Length );
				  return _values[0];
			 }
		 }

		 public override string ToString()
		 {
			  StringBuilder sb = new StringBuilder();
			  string sep = "( ";
			  foreach ( Value value in _values )
			  {
					sb.Append( sep );
					sep = ", ";
					sb.Append( value );
			  }
			  sb.Append( " )" );
			  return sb.ToString();
		 }

		 private static bool NoNulls( object[] values )
		 {
			  foreach ( object v in values )
			  {
					if ( v == null )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 public static readonly IComparer<ValueTuple> Comparator = ( left, right ) =>
		 {
		  if ( left.values.length != right.values.length )
		  {
				throw new System.InvalidOperationException( "Comparing two ValueTuples of different lengths!" );
		  }

		  int compare = 0;
		  for ( int i = 0; i < left.values.length; i++ )
		  {
				compare = Values.Comparator.Compare( left.valueAt( i ), right.valueAt( i ) );
				if ( compare != 0 )
				{
					 return compare;
				}
		  }
		  return compare;
		 };
	}

}