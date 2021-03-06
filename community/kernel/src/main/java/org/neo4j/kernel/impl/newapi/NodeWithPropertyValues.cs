﻿/*
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
namespace Org.Neo4j.Kernel.Impl.Newapi
{

	using Value = Org.Neo4j.Values.Storable.Value;

	/// <summary>
	/// A node together with properties. This class is needed to present changes in the transaction state to index operations
	/// that require knowing the affected property values as well.
	/// </summary>
	public class NodeWithPropertyValues
	{

		 private readonly long _nodeId;
		 private readonly Value[] _values;

		 internal NodeWithPropertyValues( long nodeId, Value[] values )
		 {
			  this._nodeId = nodeId;
			  this._values = values;
		 }

		 public virtual long NodeId
		 {
			 get
			 {
				  return _nodeId;
			 }
		 }

		 public virtual Value[] Values
		 {
			 get
			 {
				  return _values;
			 }
		 }

		 public override string ToString()
		 {
			  return "NodeWithPropertyValues{" + "nodeId=" + _nodeId + ", values=" + Arrays.ToString( _values ) + '}';
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
			  NodeWithPropertyValues that = ( NodeWithPropertyValues ) o;
			  return _nodeId == that._nodeId && Arrays.Equals( _values, that._values );
		 }

		 public override int GetHashCode()
		 {
			  int result = Objects.hash( _nodeId );
			  result = 31 * result + Arrays.GetHashCode( _values );
			  return result;
		 }
	}

}