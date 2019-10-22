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
namespace Neo4Net.Values
{

	using InvalidValuesArgumentException = Neo4Net.Values.utils.InvalidValuesArgumentException;
	using MapValue = Neo4Net.Values.@virtual.MapValue;

	public interface StructureBuilder<Input, Result>
	{
		 StructureBuilder<Input, Result> Add( string field, Input value );

		 Result Build();

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static <T> T build(final StructureBuilder<AnyValue, T> builder, org.Neo4Net.values.@virtual.MapValue map)
	//	 {
	//		  if (map.size() == 0)
	//		  {
	//				throw new InvalidValuesArgumentException("At least one temporal unit must be specified.");
	//		  }
	//		  map.@foreach(builder::add);
	//
	//		  return builder.build();
	//	 }

//JAVA TO C# CONVERTER TODO TASK: There is no equivalent in C# to Java static interface methods:
//		 static <T> T build(StructureBuilder<AnyValue, T> builder, Iterable<java.util.Map.Entry<String, AnyValue>> entries)
	//	 {
	//		  for (Map.Entry<String,AnyValue> entry : entries)
	//		  {
	//				builder.add(entry.getKey(), entry.getValue());
	//		  }
	//		  return builder.build();
	//	 }
	}

}