using System;
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
namespace Neo4Net.Kernel.Api.Index
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using RuleChain = org.junit.rules.RuleChain;
	using RunWith = org.junit.runner.RunWith;
	using Suite = org.junit.runners.Suite;


	using Neo4Net.Functions;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using PhaseTracker = Neo4Net.Kernel.Impl.Api.index.PhaseTracker;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using PageCacheAndDependenciesRule = Neo4Net.Test.rule.PageCacheAndDependenciesRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;
	using ParameterizedSuiteRunner = Neo4Net.Test.runner.ParameterizedSuiteRunner;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using DateTimeValue = Neo4Net.Values.Storable.DateTimeValue;
	using DateValue = Neo4Net.Values.Storable.DateValue;
	using DurationValue = Neo4Net.Values.Storable.DurationValue;
	using LocalDateTimeValue = Neo4Net.Values.Storable.LocalDateTimeValue;
	using LocalTimeValue = Neo4Net.Values.Storable.LocalTimeValue;
	using RandomValues = Neo4Net.Values.Storable.RandomValues;
	using TimeValue = Neo4Net.Values.Storable.TimeValue;
	using Value = Neo4Net.Values.Storable.Value;
	using ValueType = Neo4Net.Values.Storable.ValueType;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(ParameterizedSuiteRunner.class) @Suite.SuiteClasses({ SimpleIndexPopulatorCompatibility.General.class, SimpleIndexPopulatorCompatibility.Unique.class, CompositeIndexPopulatorCompatibility.General.class, CompositeIndexPopulatorCompatibility.Unique.class, SimpleIndexAccessorCompatibility.General.class, SimpleIndexAccessorCompatibility.Unique.class, CompositeIndexAccessorCompatibility.General.class, CompositeIndexAccessorCompatibility.Unique.class, UniqueConstraintCompatibility.class, SimpleRandomizedIndexAccessorCompatibility.class, CompositeRandomizedIndexAccessorCompatibility.Exact.class, CompositeRandomizedIndexAccessorCompatibility.Range.class }) public abstract class IndexProviderCompatibilityTestSuite
	public abstract class IndexProviderCompatibilityTestSuite
	{
		 protected internal abstract IndexProvider CreateIndexProvider( PageCache pageCache, FileSystemAbstraction fs, File graphDbDir );

		 public abstract bool SupportsSpatial();

		 /// <summary>
		 /// Granular composite queries means queries against composite index that is made up of a mix of exact, range and exists queries.
		 /// For example: exact match on first column and range scan on seconds column.
		 /// See <seealso cref="org.neo4j.kernel.impl.index.schema.GenericNativeIndexProvider"/> for further details on supported combinations. </summary>
		 /// <returns> true if index provider have support granular composite queries. </returns>
		 public virtual bool SupportsGranularCompositeQueries()
		 {
			  return false;
		 }

		 public virtual bool SupportsBooleanRangeQueries()
		 {
			  return false;
		 };

		 public virtual bool SupportFullValuePrecisionForNumbers()
		 {
			  return true;
		 }

		 public virtual ValueType[] SupportedValueTypes()
		 {
			  if ( !SupportsSpatial() )
			  {
					return RandomValues.excluding( ValueType.CARTESIAN_POINT, ValueType.CARTESIAN_POINT_ARRAY, ValueType.CARTESIAN_POINT_3D, ValueType.CARTESIAN_POINT_3D_ARRAY, ValueType.GEOGRAPHIC_POINT, ValueType.GEOGRAPHIC_POINT_ARRAY, ValueType.GEOGRAPHIC_POINT_3D, ValueType.GEOGRAPHIC_POINT_3D_ARRAY );
			  }
			  return ValueType.values();
		 }

		 public virtual void ConsistencyCheck( IndexPopulator populator )
		 {
			  // no-op by default
		 }

		 public abstract class Compatibility
		 {
			  internal readonly PageCacheAndDependenciesRule PageCacheAndDependenciesRule;
			  internal readonly RandomRule Random;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.RuleChain ruleChain;
			  public RuleChain RuleChain;

			  protected internal File GraphDbDir;
			  protected internal FileSystemAbstraction Fs;
			  protected internal IndexProvider IndexProvider;
			  protected internal StoreIndexDescriptor Descriptor;
			  internal readonly IndexProviderCompatibilityTestSuite TestSuite;
			  internal readonly IList<NodeAndValue> ValueSet1;
			  internal readonly IList<NodeAndValue> ValueSet2;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
			  public virtual void Setup()
			  {
					Fs = PageCacheAndDependenciesRule.fileSystem();
					GraphDbDir = PageCacheAndDependenciesRule.directory().databaseDir();
					PageCache pageCache = PageCacheAndDependenciesRule.pageCache();
					IndexProvider = TestSuite.createIndexProvider( pageCache, Fs, GraphDbDir );
			  }

			  public Compatibility( IndexProviderCompatibilityTestSuite testSuite, IndexDescriptor descriptor )
			  {
					this.TestSuite = testSuite;
					this.Descriptor = descriptor.WithId( 17 );
					this.ValueSet1 = AllValues( testSuite.SupportsSpatial(), Arrays.asList(Values.of("string1"), Values.of(42), Values.of(true), Values.of(new char[]{ 'a', 'z' }), Values.of(new string[]{ "arrayString1", "arraysString2" }), Values.of(new sbyte[]{ (sbyte) 1, (sbyte) 12 }), Values.of(new short[]{ 314, 1337 }), Values.of(new int[]{ 3140, 13370 }), Values.of(new long[]{ 31400, 133700 }), Values.of(new bool[]{ true, true })), Arrays.asList(DateValue.epochDate(2), LocalTimeValue.localTime(100000), TimeValue.time(43_200_000_000_000L, ZoneOffset.UTC), TimeValue.time(43_201_000_000_000L, ZoneOffset.UTC), TimeValue.time(43_200_000_000_000L, ZoneOffset.of("+01:00")), TimeValue.time(46_800_000_000_000L, ZoneOffset.UTC), LocalDateTimeValue.localDateTime(2018, 3, 1, 13, 50, 42, 1337), DateTimeValue.datetime(2014, 3, 25, 12, 45, 13, 7474, "UTC"), DateTimeValue.datetime(2014, 3, 25, 12, 45, 13, 7474, "Europe/Stockholm"), DateTimeValue.datetime(2014, 3, 25, 12, 45, 13, 7474, "+05:00"), DateTimeValue.datetime(2015, 3, 25, 12, 45, 13, 7474, "+05:00"), DateTimeValue.datetime(2014, 4, 25, 12, 45, 13, 7474, "+05:00"), DateTimeValue.datetime(2014, 3, 26, 12, 45, 13, 7474, "+05:00"), DateTimeValue.datetime(2014, 3, 25, 13, 45, 13, 7474, "+05:00"), DateTimeValue.datetime(2014, 3, 25, 12, 46, 13, 7474, "+05:00"), DateTimeValue.datetime(2014, 3, 25, 12, 45, 14, 7474, "+05:00"), DateTimeValue.datetime(2014, 3, 25, 12, 45, 13, 7475, "+05:00"), DateTimeValue.datetime(2038, 1, 18, 9, 14, 7, 0, "-18:00"), DateTimeValue.datetime(10000, 100, ZoneOffset.ofTotalSeconds(3)), DateTimeValue.datetime(10000, 101, ZoneOffset.ofTotalSeconds(-3)), DurationValue.duration(10, 20, 30, 40), DurationValue.duration(11, 20, 30, 40), DurationValue.duration(10, 21, 30, 40), DurationValue.duration(10, 20, 31, 40), DurationValue.duration(10, 20, 30, 41), Values.dateTimeArray(new ZonedDateTime[]{ ZonedDateTime.of(2018, 10, 9, 8, 7, 6, 5, ZoneId.of("UTC")), ZonedDateTime.of(2017, 9, 8, 7, 6, 5, 4, ZoneId.of("UTC")) }), Values.localDateTimeArray(new DateTime[]{ new DateTime(2018, 10, 9, 8, 7, 6, 5), new DateTime(2018, 10, 9, 8, 7, 6, 5) }), Values.timeArray(new OffsetTime[]{ OffsetTime.of(20, 8, 7, 6, ZoneOffset.UTC), OffsetTime.of(20, 8, 7, 6, ZoneOffset.UTC) }), Values.dateArray(new LocalDate[]{ LocalDate.of(1, 12, 28), LocalDate.of(1, 12, 28) }), Values.localTimeArray(new LocalTime[]{ LocalTime.of(9, 28), LocalTime.of(9, 28) }), Values.durationArray(new DurationValue[]{ DurationValue.duration(12, 10, 10, 10), DurationValue.duration(12, 10, 10, 10) })), Arrays.asList(Values.pointValue(CoordinateReferenceSystem.Cartesian, 0, 0), Values.pointValue(CoordinateReferenceSystem.WGS84, 12.78, 56.7)) );

					this.ValueSet2 = AllValues( testSuite.SupportsSpatial(), Arrays.asList(Values.of("string2"), Values.of(1337), Values.of(false), Values.of(new char[]{ 'b', 'c' }), Values.of(new string[]{ "someString1", "someString2" }), Values.of(new sbyte[]{ (sbyte) 9, (sbyte) 9 }), Values.of(new short[]{ 99, 999 }), Values.of(new int[]{ 99999, 99999 }), Values.of(new long[]{ 999999, 999999 }), Values.of(new bool[]{ false, false })), Arrays.asList(DateValue.epochDate(42), LocalTimeValue.localTime(2000), TimeValue.time(100L, ZoneOffset.UTC), LocalDateTimeValue.localDateTime(2018, 2, 28, 11, 5, 1, 42), DateTimeValue.datetime(1999, 12, 31, 23, 59, 59, 123456789, "Europe/London"), DurationValue.duration(4, 3, 2, 1), Values.dateTimeArray(new ZonedDateTime[]{ ZonedDateTime.of(999, 10, 9, 8, 7, 6, 5, ZoneId.of("UTC")), ZonedDateTime.of(999, 9, 8, 7, 6, 5, 4, ZoneId.of("UTC")) }), Values.localDateTimeArray(new DateTime[]{ new DateTime(999, 10, 9, 8, 7, 6, 5), new DateTime(999, 10, 9, 8, 7, 6, 5) }), Values.timeArray(new OffsetTime[]{ OffsetTime.of(19, 8, 7, 6, ZoneOffset.UTC), OffsetTime.of(19, 8, 7, 6, ZoneOffset.UTC) }), Values.dateArray(new LocalDate[]{ LocalDate.of(999, 12, 28), LocalDate.of(999, 12, 28) }), Values.localTimeArray(new LocalTime[]{ LocalTime.of(19, 28), LocalTime.of(19, 28) }), Values.durationArray(new DurationValue[]{ DurationValue.duration(99, 10, 10, 10), DurationValue.duration(99, 10, 10, 10) })), Arrays.asList(Values.pointValue(CoordinateReferenceSystem.Cartesian, 90, 90), Values.pointValue(CoordinateReferenceSystem.WGS84, 9.21, 9.65)) );

					PageCacheAndDependenciesRule = ( new PageCacheAndDependenciesRule() ).with(new DefaultFileSystemRule()).with(testSuite.GetType());
					Random = new RandomRule();
					RuleChain = RuleChain.outerRule( PageCacheAndDependenciesRule ).around( Random );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void withPopulator(IndexPopulator populator, org.neo4j.function.ThrowingConsumer<IndexPopulator,Exception> runWithPopulator) throws Exception
			  internal virtual void WithPopulator( IndexPopulator populator, ThrowingConsumer<IndexPopulator, Exception> runWithPopulator )
			  {
					WithPopulator( populator, runWithPopulator, true );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void withPopulator(IndexPopulator populator, org.neo4j.function.ThrowingConsumer<IndexPopulator,Exception> runWithPopulator, boolean closeSuccessfully) throws Exception
			  internal virtual void WithPopulator( IndexPopulator populator, ThrowingConsumer<IndexPopulator, Exception> runWithPopulator, bool closeSuccessfully )
			  {
					try
					{
						 populator.Create();
						 runWithPopulator.Accept( populator );
						 if ( closeSuccessfully )
						 {
							  populator.ScanCompleted( Neo4Net.Kernel.Impl.Api.index.PhaseTracker_Fields.NullInstance );
							  TestSuite.consistencyCheck( populator );
						 }
					}
					finally
					{
						 populator.Close( closeSuccessfully );
					}
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<IndexEntryUpdate<?>> updates(java.util.List<NodeAndValue> values)
			  internal virtual IList<IndexEntryUpdate<object>> Updates( IList<NodeAndValue> values )
			  {
					return Updates( values, 0 );
			  }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<IndexEntryUpdate<?>> updates(java.util.List<NodeAndValue> values, long nodeIdOffset)
			  internal virtual IList<IndexEntryUpdate<object>> Updates( IList<NodeAndValue> values, long nodeIdOffset )
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: java.util.List<IndexEntryUpdate<?>> updates = new java.util.ArrayList<>();
					IList<IndexEntryUpdate<object>> updates = new List<IndexEntryUpdate<object>>();
					values.ForEach( entry => updates.Add( IndexEntryUpdate.Add( nodeIdOffset + entry.nodeId, Descriptor.schema(), entry.value ) ) );
					return updates;
			  }

			  internal static IList<NodeAndValue> AllValues( bool supportsSpatial, IList<Value> common, IList<Value> temporal, IList<Value> spatial )
			  {
					long nodeIds = 0;
					IList<NodeAndValue> result = new List<NodeAndValue>();
					foreach ( Value value in common )
					{
						 result.Add( new NodeAndValue( nodeIds++, value ) );
					}
					if ( supportsSpatial )
					{
						 foreach ( Value value in spatial )
						 {
							  result.Add( new NodeAndValue( nodeIds++, value ) );
						 }
					}
					foreach ( Value value in temporal )
					{
						 result.Add( new NodeAndValue( nodeIds++, value ) );
					}
					return result;
			  }

			  internal class NodeAndValue
			  {
					internal readonly long NodeId;
					internal readonly Value Value;

					internal NodeAndValue( long nodeId, Value value )
					{
						 this.NodeId = nodeId;
						 this.Value = value;
					}
			  }
		 }
	}

}