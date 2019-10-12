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
namespace Org.Neo4j.Kernel.Api.Index
{

	using SchemaDescriptor = Org.Neo4j.@internal.Kernel.Api.schema.SchemaDescriptor;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;

	public class TestNodePropertyAccessor : NodePropertyAccessor
	{
		 private readonly IDictionary<long, IDictionary<int, Value>> _nodePropertyMap;

		 internal TestNodePropertyAccessor( long nodeId, SchemaDescriptor schema, params Value[] values )
		 {
			  _nodePropertyMap = new Dictionary<long, IDictionary<int, Value>>();
			  AddNode( nodeId, schema, values );
		 }

		 public virtual void AddNode( long nodeId, SchemaDescriptor schema, params Value[] values )
		 {
			  IDictionary<int, Value> propertyMap = new Dictionary<int, Value>();
			  int[] propertyIds = Schema.PropertyIds;
			  for ( int i = 0; i < propertyIds.Length; i++ )
			  {
					propertyMap[propertyIds[i]] = values[i];
			  }
			  _nodePropertyMap[nodeId] = propertyMap;
		 }

		 public override Value GetNodePropertyValue( long nodeId, int propertyKeyId )
		 {
			  if ( _nodePropertyMap.ContainsKey( nodeId ) )
			  {
					Value value = _nodePropertyMap[nodeId][propertyKeyId];
					if ( value == null )
					{
						 return Values.NO_VALUE;
					}
					else
					{
						 return value;
					}
			  }
			  return Values.NO_VALUE;
		 }
	}

}