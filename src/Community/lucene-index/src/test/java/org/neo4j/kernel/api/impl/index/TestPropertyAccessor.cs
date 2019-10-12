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
namespace Neo4Net.Kernel.Api.Impl.Index
{

	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using Value = Neo4Net.Values.Storable.Value;

	public class TestPropertyAccessor : NodePropertyAccessor
	{
		 private readonly IList<Value> _propertyValues;

		 public TestPropertyAccessor( IList<Value> propertyValues )
		 {
			  this._propertyValues = propertyValues;
		 }

		 public override Value GetNodePropertyValue( long nodeId, int propertyKeyId )
		 {
			  return _propertyValues[( int )nodeId];
		 }
	}

}