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
namespace Neo4Net.Kernel.Api.Index
{
	using IndexQuery = Neo4Net.@internal.Kernel.Api.IndexQuery;
	using SchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.SchemaDescriptor;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

	public class IndexQueryHelper
	{
		 private IndexQueryHelper()
		 {
		 }

		 public static IndexQuery Exact( int propertyKeyId, object value )
		 {
			  return Exact( propertyKeyId, Values.of( value ) );
		 }

		 public static IndexQuery Exact( int propertyKeyId, Value value )
		 {
			  return IndexQuery.exact( propertyKeyId, value );
		 }

		 public static IndexEntryUpdate<SchemaDescriptor> Add( long nodeId, SchemaDescriptor schema, params object[] objects )
		 {
			  return IndexEntryUpdate.Add( nodeId, schema, ToValues( objects ) );
		 }

		 public static IndexEntryUpdate<SchemaDescriptor> Remove( long nodeId, SchemaDescriptor schema, params object[] objects )
		 {
			  return IndexEntryUpdate.Remove( nodeId, schema, ToValues( objects ) );
		 }

		 public static IndexEntryUpdate<SchemaDescriptor> Change( long nodeId, SchemaDescriptor schema, object o1, object o2 )
		 {
			  return IndexEntryUpdate.Change( nodeId, schema, Values.of( o1 ), Values.of( o2 ) );
		 }

		 public static IndexEntryUpdate<SchemaDescriptor> Change( long nodeId, SchemaDescriptor schema, object[] o1, object[] o2 )
		 {
			  return IndexEntryUpdate.Change( nodeId, schema, ToValues( o1 ), ToValues( o2 ) );
		 }

		 private static Value[] ToValues( object[] objects )
		 {
			  Value[] values = new Value[objects.Length];
			  for ( int i = 0; i < objects.Length; i++ )
			  {
					object @object = objects[i];
					values[i] = @object is Value ? ( Value )@object : Values.of( @object );
			  }
			  return values;
		 }
	}

}