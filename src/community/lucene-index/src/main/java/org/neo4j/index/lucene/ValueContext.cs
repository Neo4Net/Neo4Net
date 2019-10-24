using System;

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
namespace Neo4Net.Index.lucene
{
	/// <summary>
	/// ValueContext allows you to give not just a value, but to give the value
	/// some context to live in. </summary>
	/// @deprecated This API will be removed in next major release. Please consider using schema indexes instead. 
	[Obsolete("This API will be removed in next major release. Please consider using schema indexes instead.")]
	public class ValueContext
	{
		 private readonly object _value;
		 private bool _indexNumeric;

		 [Obsolete]
		 public ValueContext( object value )
		 {
			  this._value = value;
		 }

		 /// <returns> the value object specified in the constructor. </returns>
		 [Obsolete]
		 public virtual object Value
		 {
			 get
			 {
				  return _value;
			 }
		 }

		 /// <summary>
		 /// Returns a ValueContext to be used with
		 /// <seealso cref="org.Neo4Net.GraphDb.Index.Index.add(org.Neo4Net.graphdb.PropertyContainer, string, object)"/>
		 /// </summary>
		 /// <returns> a numeric ValueContext </returns>
		 [Obsolete]
		 public virtual ValueContext IndexNumeric()
		 {
			  if ( !( this._value is Number ) )
			  {
					throw new System.InvalidOperationException( "Value should be a Number, is " + _value + " (" + _value.GetType() + ")" );
			  }
			  this._indexNumeric = true;
			  return this;
		 }

		 /// <summary>
		 /// Returns the string representation of the value given in the constructor,
		 /// or the unmodified value if <seealso cref="indexNumeric()"/> has been called.
		 /// </summary>
		 /// <returns> the, by the user, intended value to index. </returns>
		 [Obsolete]
		 public virtual object CorrectValue
		 {
			 get
			 {
				  return this._indexNumeric ? this._value : this._value.ToString();
			 }
		 }

		 [Obsolete]
		 public override string ToString()
		 {
			  return _value.ToString();
		 }

		 /// <summary>
		 /// Convenience method to add a numeric value to an index. </summary>
		 /// <param name="value"> The value to add </param>
		 /// <returns> A ValueContext that can be used with
		 /// <seealso cref="org.Neo4Net.GraphDb.Index.Index.add(org.Neo4Net.graphdb.PropertyContainer, string, object)"/> </returns>
		 [Obsolete]
		 public static ValueContext Numeric( Number value )
		 {
			  return ( new ValueContext( value ) ).IndexNumeric();
		 }
	}

}