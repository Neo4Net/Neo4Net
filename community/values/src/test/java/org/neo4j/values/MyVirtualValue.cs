using System.Collections.Generic;

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
namespace Org.Neo4j.Values
{

	using VirtualValueGroup = Org.Neo4j.Values.@virtual.VirtualValueGroup;

	public class MyVirtualValue : VirtualValue
	{
		 private readonly int _hashCode;

		 internal MyVirtualValue( int hashCode )
		 {
			  this._hashCode = hashCode;
		 }

		 public override bool Equals( VirtualValue other )
		 {
			  return this == other;
		 }

		 public override VirtualValueGroup ValueGroup()
		 {
			  return null;
		 }

		 public override int CompareTo( VirtualValue other, IComparer<AnyValue> comparator )
		 {
			  return 0;
		 }

		 public override int ComputeHash()
		 {
			  return _hashCode;
		 }

		 public override T Map<T>( ValueMapper<T> mapper )
		 {
			  throw new System.NotSupportedException();
		 }

		 public override string TypeName
		 {
			 get
			 {
				  return "MyVirtualValue";
			 }
		 }

		 public override void WriteTo( AnyValueWriter writer )
		 {
		 }
	}

}