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
namespace Neo4Net.@internal.Kernel.Api
{
	/// <summary>
	/// A token with its associated name.
	/// </summary>
	public sealed class NamedToken
	{
		 private readonly int _id;
		 private readonly string _name;

		 public NamedToken( string name, int id )
		 {
			  this._id = id;
			  this._name = name;
		 }

		 /// <summary>
		 /// Id of token
		 /// </summary>
		 /// <returns> the id of the token </returns>
		 public int Id()
		 {
			  return _id;
		 }

		 /// <summary>
		 /// The name associated with the token
		 /// </summary>
		 /// <returns> The name corresponding to the token </returns>
		 public string Name()
		 {
			  return _name;
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

			  NamedToken that = ( NamedToken ) o;

			  return _id == that._id && _name.Equals( that._name );
		 }

		 public override int GetHashCode()
		 {
			  int result = _id;
			  result = 31 * result + _name.GetHashCode();
			  return result;
		 }

		 public override string ToString()
		 {
			  return string.Format( "{0}[name:{1}, id:{2:D}]", this.GetType().Name, _name, _id );
		 }
	}

}