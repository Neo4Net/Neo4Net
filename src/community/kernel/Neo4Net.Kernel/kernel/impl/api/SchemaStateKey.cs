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
namespace Neo4Net.Kernel.Impl.Api
{

	/// <summary>
	/// Recommended way to create keys for <seealso cref="SchemaState"/>, to guarantee control over equality uniqueness.
	/// </summary>
	public class SchemaStateKey
	{
		 private static AtomicLong _keyId = new AtomicLong();
		 public static SchemaStateKey NewKey()
		 {
			  return new SchemaStateKey( _keyId.AndIncrement );
		 }

		 public readonly long Id;

		 private SchemaStateKey( long id )
		 {
			  this.Id = id;
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
			  SchemaStateKey that = ( SchemaStateKey ) o;
			  return Id == that.Id;
		 }

		 public override int GetHashCode()
		 {
			  return Long.GetHashCode( Id );
		 }

		 public override string ToString()
		 {
			  return "SchemaStateKey(" + Id + ")";
		 }
	}

}