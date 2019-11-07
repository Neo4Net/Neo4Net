using System.Collections.Generic;

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
namespace Neo4Net.Bolt.v1.runtime
{
	using Test = org.junit.Test;


	using ExecutionPlanDescription = Neo4Net.GraphDb.ExecutionPlanDescription;
	using Iterators = Neo4Net.Collections.Helpers.Iterators;
	using MapUtil = Neo4Net.Collections.Helpers.MapUtil;
	using ValueUtils = Neo4Net.Kernel.impl.util.ValueUtils;
	using DoubleValue = Neo4Net.Values.Storable.DoubleValue;
	using MapValue = Neo4Net.Values.@virtual.MapValue;
	using VirtualValues = Neo4Net.Values.@virtual.VirtualValues;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.longValue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.values.storable.Values.stringValue;

	public class ExecutionPlanConverterTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void profileStatisticConversion()
		 public virtual void ProfileStatisticConversion()
		 {
			  MapValue convertedMap = ExecutionPlanConverter.Convert( new TestExecutionPlanDescription( this, "description", ProfilerStatistics, Identifiers, Arguments ) );
			  assertEquals( convertedMap.Get( "operatorType" ), stringValue( "description" ) );
			  assertEquals( convertedMap.Get( "args" ), ValueUtils.asMapValue( Arguments ) );
			  assertEquals( convertedMap.Get( "identifiers" ), ValueUtils.asListValue( Identifiers ) );
			  assertEquals( convertedMap.Get( "children" ), VirtualValues.EMPTY_LIST );
			  assertEquals( convertedMap.Get( "rows" ), longValue( 1L ) );
			  assertEquals( convertedMap.Get( "dbHits" ), longValue( 2L ) );
			  assertEquals( convertedMap.Get( "pageCacheHits" ), longValue( 3L ) );
			  assertEquals( convertedMap.Get( "pageCacheMisses" ), longValue( 2L ) );
			  assertEquals( ( ( DoubleValue ) convertedMap.Get( "pageCacheHitRatio" ) ).doubleValue(), 3.0 / 5, 0.0001 );
			  assertEquals( convertedMap.Size(), 9 );
		 }

		 private IDictionary<string, object> Arguments
		 {
			 get
			 {
				  return MapUtil.map( "argKey", "argValue" );
			 }
		 }

		 private ISet<string> Identifiers
		 {
			 get
			 {
				  return Iterators.asSet( "identifier1", "identifier2" );
			 }
		 }

		 private TestProfilerStatistics ProfilerStatistics
		 {
			 get
			 {
				  return new TestProfilerStatistics( this, 1, 2, 3, 2 );
			 }
		 }

		 private class TestExecutionPlanDescription : ExecutionPlanDescription
		 {
			 private readonly ExecutionPlanConverterTest _outerInstance;


//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly string NameConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly Neo4Net.GraphDb.ExecutionPlanDescription_ProfilerStatistics ProfilerStatisticsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly ISet<string> IdentifiersConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly IDictionary<string, object> ArgumentsConflict;

			  internal TestExecutionPlanDescription( ExecutionPlanConverterTest outerInstance, string name, Neo4Net.GraphDb.ExecutionPlanDescription_ProfilerStatistics profilerStatistics, ISet<string> identifiers, IDictionary<string, object> arguments )
			  {
				  this._outerInstance = outerInstance;
					this.NameConflict = name;
					this.ProfilerStatisticsConflict = profilerStatistics;
					this.IdentifiersConflict = identifiers;
					this.ArgumentsConflict = arguments;
			  }

			  public virtual string Name
			  {
				  get
				  {
						return NameConflict;
				  }
			  }

			  public virtual IList<ExecutionPlanDescription> Children
			  {
				  get
				  {
						return Collections.emptyList();
				  }
			  }

			  public virtual IDictionary<string, object> Arguments
			  {
				  get
				  {
						return ArgumentsConflict;
				  }
			  }

			  public virtual ISet<string> Identifiers
			  {
				  get
				  {
						return IdentifiersConflict;
				  }
			  }

			  public override bool HasProfilerStatistics()
			  {
					return ProfilerStatisticsConflict != null;
			  }

			  public virtual Neo4Net.GraphDb.ExecutionPlanDescription_ProfilerStatistics ProfilerStatistics
			  {
				  get
				  {
						return ProfilerStatisticsConflict;
				  }
			  }
		 }

		 private class TestProfilerStatistics : Neo4Net.GraphDb.ExecutionPlanDescription_ProfilerStatistics
		 {
			 private readonly ExecutionPlanConverterTest _outerInstance;


//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly long RowsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly long DbHitsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly long PageCacheHitsConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly long PageCacheMissesConflict;

			  internal TestProfilerStatistics( ExecutionPlanConverterTest outerInstance, long rows, long dbHits, long pageCacheHits, long pageCacheMisses )
			  {
				  this._outerInstance = outerInstance;
					this.RowsConflict = rows;
					this.DbHitsConflict = dbHits;
					this.PageCacheHitsConflict = pageCacheHits;
					this.PageCacheMissesConflict = pageCacheMisses;
			  }

			  public virtual long Rows
			  {
				  get
				  {
						return RowsConflict;
				  }
			  }

			  public virtual long DbHits
			  {
				  get
				  {
						return DbHitsConflict;
				  }
			  }

			  public virtual long PageCacheHits
			  {
				  get
				  {
						return PageCacheHitsConflict;
				  }
			  }

			  public virtual long PageCacheMisses
			  {
				  get
				  {
						return PageCacheMissesConflict;
				  }
			  }
		 }
	}

}