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
namespace Neo4Net.Bolt.v1.runtime
{

	using ExecutionPlanDescription = Neo4Net.Graphdb.ExecutionPlanDescription;
	using ValueUtils = Neo4Net.Kernel.impl.util.ValueUtils;
	using AnyValue = Neo4Net.Values.AnyValue;
	using ListValue = Neo4Net.Values.@virtual.ListValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using MapValueBuilder = Neo4Net.Values.@virtual.MapValueBuilder;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.doubleValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.Values.stringValue;

	/// <summary>
	/// Takes execution plans and converts them to the subset of types used in the Neo4j type system </summary>
	internal class ExecutionPlanConverter
	{
		 private ExecutionPlanConverter()
		 {
		 }

		 public static MapValue Convert( ExecutionPlanDescription plan )
		 {
			  bool hasProfilerStatistics = plan.HasProfilerStatistics();
			  int size = hasProfilerStatistics ? 9 : 4;
			  MapValueBuilder @out = new MapValueBuilder( size );
			  @out.Add( "operatorType", stringValue( plan.Name ) );
			  @out.Add( "args", ValueUtils.asMapValue( plan.Arguments ) );
			  @out.Add( "identifiers", ValueUtils.asListValue( plan.Identifiers ) );
			  @out.Add( "children", Children( plan ) );
			  if ( hasProfilerStatistics )
			  {
					Neo4Net.Graphdb.ExecutionPlanDescription_ProfilerStatistics profile = plan.ProfilerStatistics;
					@out.Add( "dbHits", longValue( profile.DbHits ) );
					@out.Add( "pageCacheHits", longValue( profile.PageCacheHits ) );
					@out.Add( "pageCacheMisses", longValue( profile.PageCacheMisses ) );
					@out.Add( "pageCacheHitRatio", doubleValue( profile.PageCacheHitRatio ) );
					@out.Add( "rows", longValue( profile.Rows ) );
			  }
			  return @out.Build();
		 }

		 private static ListValue Children( ExecutionPlanDescription plan )
		 {
			  IList<AnyValue> children = new LinkedList<AnyValue>();
			  foreach ( ExecutionPlanDescription child in plan.Children )
			  {
					children.Add( Convert( child ) );
			  }
			  return VirtualValues.fromList( children );
		 }
	}

}