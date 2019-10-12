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
namespace Org.Neo4j.@internal.Kernel.Api.schema
{
	public class IndexProviderDescriptor
	{
		 /// <summary>
		 /// Indicate that <seealso cref="IndexProviderDescriptor"/> has not yet been decided.
		 /// Specifically before transaction that create a new index has committed.
		 /// </summary>
		 public static readonly IndexProviderDescriptor Undecided = new IndexProviderDescriptor( "Undecided", "0" );

		 private readonly string _key;
		 private readonly string _version;

		 public IndexProviderDescriptor( string key, string version )
		 {
			  if ( string.ReferenceEquals( key, null ) )
			  {
					throw new System.ArgumentException( "null provider key prohibited" );
			  }
			  if ( key.Length == 0 )
			  {
					throw new System.ArgumentException( "empty provider key prohibited" );
			  }
			  if ( string.ReferenceEquals( version, null ) )
			  {
					throw new System.ArgumentException( "null provider version prohibited" );
			  }

			  this._key = key;
			  this._version = version;
		 }

		 public virtual string Key
		 {
			 get
			 {
				  return _key;
			 }
		 }

		 public virtual string Version
		 {
			 get
			 {
				  return _version;
			 }
		 }

		 /// <returns> a combination of <seealso cref="getKey()"/> and <seealso cref="getVersion()"/> with a '-' in between. </returns>
		 public virtual string Name()
		 {
			  return _key + "-" + _version;
		 }

		 public override int GetHashCode()
		 {
			  return ( 23 + _key.GetHashCode() ) ^ _version.GetHashCode();
		 }

		 public override bool Equals( object obj )
		 {
			  if ( obj is IndexProviderDescriptor )
			  {
					IndexProviderDescriptor otherDescriptor = ( IndexProviderDescriptor ) obj;
					return _key.Equals( otherDescriptor.Key ) && _version.Equals( otherDescriptor.Version );
			  }
			  return false;
		 }

		 public override string ToString()
		 {
			  return "{key=" + _key + ", version=" + _version + "}";
		 }
	}

}