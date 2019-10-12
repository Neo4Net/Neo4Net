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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using Test = org.junit.Test;

	using StandardConfiguration = Neo4Net.Gis.Spatial.Index.curves.StandardConfiguration;
	using IndexCapability = Neo4Net.@internal.Kernel.Api.IndexCapability;
	using IndexNotApplicableKernelException = Neo4Net.@internal.Kernel.Api.exceptions.schema.IndexNotApplicableKernelException;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Neo4Net.Kernel.Api.Index;
	using TestIndexDescriptorFactory = Neo4Net.Kernel.api.schema.index.TestIndexDescriptorFactory;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ConfiguredSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.ConfiguredSpaceFillingCurveSettingsCache;
	using IndexDescriptor = Neo4Net.Storageengine.Api.schema.IndexDescriptor;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.index.@internal.gbptree.RecoveryCleanupWorkCollector.immediate;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.values.storable.CoordinateReferenceSystem.WGS84;

	public class SpatialIndexAccessorTest : NativeIndexAccessorTests<SpatialIndexKey, NativeIndexValue>
	{
		 private static readonly CoordinateReferenceSystem _crs = CoordinateReferenceSystem.WGS84;
		 private static readonly ConfiguredSpaceFillingCurveSettingsCache _configuredSettings = new ConfiguredSpaceFillingCurveSettingsCache( Config.defaults() );

		 private SpatialIndexFiles.SpatialFile _spatialFile;

		 internal override NativeIndexAccessor<SpatialIndexKey, NativeIndexValue> MakeAccessor()
		 {
			  _spatialFile = new SpatialIndexFiles.SpatialFile( CoordinateReferenceSystem.WGS84, _configuredSettings, base.IndexFile );
			  return new SpatialIndexAccessor.PartAccessor( pageCache, fs, _spatialFile.LayoutForNewIndex, immediate(), monitor, indexDescriptor, new StandardConfiguration(), false );
		 }

		 internal override IndexCapability IndexCapability()
		 {
			  return SpatialIndexProvider.Capability;
		 }

		 protected internal override ValueCreatorUtil<SpatialIndexKey, NativeIndexValue> CreateValueCreatorUtil()
		 {
			  return new SpatialValueCreatorUtil( TestIndexDescriptorFactory.forLabel( 42, 666 ).withId( 0 ), ValueCreatorUtil.FRACTION_DUPLICATE_NON_UNIQUE );
		 }

		 internal override IndexLayout<SpatialIndexKey, NativeIndexValue> CreateLayout()
		 {
			  return new SpatialLayout( _crs, _configuredSettings.forCRS( _crs ).curve() );
		 }

		 public override File IndexFile
		 {
			 get
			 {
				  return _spatialFile.indexFile;
			 }
		 }

		 public override void ShouldNotSeeFilteredEntries()
		 {
			  // This test doesn't make sense for spatial, since it needs a proper store for the values
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnMatchingEntriesForRangePredicateWithInclusiveStartAndInclusiveEnd() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnMatchingEntriesForRangePredicateWithInclusiveStartAndInclusiveEnd()
		 {
			  // given
			  IndexEntryUpdate<IndexDescriptor>[] updates = valueCreatorUtil.generateAddUpdatesFor( new Value[]{ Values.pointValue( WGS84, -90, -90 ), Values.pointValue( WGS84, -70, -70 ), Values.pointValue( WGS84, -50, -50 ), Values.pointValue( WGS84, 0, 0 ), Values.pointValue( WGS84, 50, 50 ), Values.pointValue( WGS84, 70, 70 ), Values.pointValue( WGS84, 90, 90 ) } );
			  ShouldReturnMatchingEntriesForRangePredicateWithInclusiveStartAndInclusiveEnd( updates );
		 }

		 public override void ShouldReturnMatchingEntriesForRangePredicateWithExclusiveStartAndExclusiveEnd()
		 {
			  // Exclusive is handled via a postfilter for spatial
		 }

		 public override void ShouldReturnMatchingEntriesForRangePredicateWithExclusiveStartAndInclusiveEnd()
		 {
			  // Exclusive is handled via a postfilter for spatial
		 }

		 public override void ShouldReturnMatchingEntriesForRangePredicateWithInclusiveStartAndExclusiveEnd()
		 {
			  // Exclusive is handled via a postfilter for spatial
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void mustHandleNestedQueries() throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException, org.neo4j.internal.kernel.api.exceptions.schema.IndexNotApplicableKernelException
		 public override void MustHandleNestedQueries()
		 {
			  // It ok to not use random values here because we are only testing nesting of queries
			  //noinspection unchecked
			  IndexEntryUpdate<IndexDescriptor>[] updates = valueCreatorUtil.generateAddUpdatesFor( new Value[]{ Values.pointValue( WGS84, -90, -90 ), Values.pointValue( WGS84, -70, -70 ), Values.pointValue( WGS84, -50, -50 ), Values.pointValue( WGS84, 0, 0 ), Values.pointValue( WGS84, 50, 50 ), Values.pointValue( WGS84, 70, 70 ), Values.pointValue( WGS84, 90, 90 ) } );
			  MustHandleNestedQueries( updates );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void mustHandleMultipleNestedQueries() throws Exception
		 public override void MustHandleMultipleNestedQueries()
		 {
			  // It ok to not use random values here because we are only testing nesting of queries
			  //noinspection unchecked
			  IndexEntryUpdate<IndexDescriptor>[] updates = valueCreatorUtil.generateAddUpdatesFor( new Value[]{ Values.pointValue( WGS84, -90, -90 ), Values.pointValue( WGS84, -70, -70 ), Values.pointValue( WGS84, -50, -50 ), Values.pointValue( WGS84, 0, 0 ), Values.pointValue( WGS84, 50, 50 ), Values.pointValue( WGS84, 70, 70 ), Values.pointValue( WGS84, 90, 90 ) } );
			  MustHandleMultipleNestedQueries( updates );
		 }

		 public override void ShouldReturnNoEntriesForRangePredicateOutsideAnyMatch()
		 {
			  // Accidental hits outside range is handled via a postfilter for spatial
		 }

		 public override void RespectIndexOrder()
		 { // Spatial is non-orderable so test does not make sense
		 }
	}

}