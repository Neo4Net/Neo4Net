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
namespace Neo4Net.Kernel.Impl.Index.Schema
{
	using RunWith = org.junit.runner.RunWith;
	using Parameterized = org.junit.runners.Parameterized;


	using StandardConfiguration = Neo4Net.Gis.Spatial.Index.curves.StandardConfiguration;
	using RecoveryCleanupWorkCollector = Neo4Net.Index.@internal.gbptree.RecoveryCleanupWorkCollector;
	using IndexCapability = Neo4Net.@internal.Kernel.Api.IndexCapability;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using IndexDirectoryStructure = Neo4Net.Kernel.Api.Index.IndexDirectoryStructure;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using Config = Neo4Net.Kernel.configuration.Config;
	using ConfiguredSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.ConfiguredSpaceFillingCurveSettingsCache;
	using IndexSpecificSpaceFillingCurveSettingsCache = Neo4Net.Kernel.Impl.Index.Schema.config.IndexSpecificSpaceFillingCurveSettingsCache;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;
	using RandomValues = Neo4Net.Values.Storable.RandomValues;
	using ValueGroup = Neo4Net.Values.Storable.ValueGroup;
	using ValueType = Neo4Net.Values.Storable.ValueType;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.schema.index.TestIndexDescriptorFactory.forLabel;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.ValueCreatorUtil.FRACTION_DUPLICATE_NON_UNIQUE;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class NativeIndexAccessorTest<KEY extends NativeIndexKey<KEY>, VALUE extends NativeIndexValue> extends NativeIndexAccessorTests<KEY,VALUE>
	public class NativeIndexAccessorTest<KEY, VALUE> : NativeIndexAccessorTests<KEY, VALUE> where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{index}: {0}") public static java.util.Collection<Object[]> data()
		 public static ICollection<object[]> Data()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return Arrays.asList(new object[][]
			  {
				  new object[] { "Number", NumberAccessorFactory(), RandomValues.typesOfGroup(ValueGroup.NUMBER), (IndexLayoutFactory) NumberLayoutNonUnique::new, NumberIndexProvider.Capability },
				  new object[] { "String", StringAccessorFactory(), RandomValues.typesOfGroup(ValueGroup.TEXT), (IndexLayoutFactory) StringLayout::new, StringIndexProvider.Capability },
				  new object[] { "Date", TemporalAccessorFactory( ValueGroup.DATE ), RandomValues.typesOfGroup( ValueGroup.DATE ), ( IndexLayoutFactory ) DateLayout::new, TemporalIndexProvider.Capability },
				  new object[] { "DateTime", TemporalAccessorFactory( ValueGroup.ZONED_DATE_TIME ), RandomValues.typesOfGroup( ValueGroup.ZONED_DATE_TIME ), ( IndexLayoutFactory ) ZonedDateTimeLayout::new, TemporalIndexProvider.Capability },
				  new object[] { "Duration", TemporalAccessorFactory( ValueGroup.DURATION ), RandomValues.typesOfGroup( ValueGroup.DURATION ), ( IndexLayoutFactory ) DurationLayout::new, TemporalIndexProvider.Capability },
				  new object[] { "LocalDateTime", TemporalAccessorFactory( ValueGroup.LOCAL_DATE_TIME ), RandomValues.typesOfGroup( ValueGroup.LOCAL_DATE_TIME ), ( IndexLayoutFactory ) LocalDateTimeLayout::new, TemporalIndexProvider.Capability },
				  new object[] { "LocalTime", TemporalAccessorFactory( ValueGroup.LOCAL_TIME ), RandomValues.typesOfGroup( ValueGroup.LOCAL_TIME ), ( IndexLayoutFactory ) LocalTimeLayout::new, TemporalIndexProvider.Capability },
				  new object[] { "LocalDateTime", TemporalAccessorFactory( ValueGroup.LOCAL_DATE_TIME ), RandomValues.typesOfGroup( ValueGroup.LOCAL_DATE_TIME ), ( IndexLayoutFactory ) LocalDateTimeLayout::new, TemporalIndexProvider.Capability },
				  new object[] { "Time", TemporalAccessorFactory( ValueGroup.ZONED_TIME ), RandomValues.typesOfGroup( ValueGroup.ZONED_TIME ), ( IndexLayoutFactory ) ZonedTimeLayout::new, TemporalIndexProvider.Capability },
				  new object[] { "Generic", GenericAccessorFactory(), ValueType.values(), (IndexLayoutFactory)() => new GenericLayout(1, _spaceFillingCurveSettings), GenericNativeIndexProvider.Capability }
			  });
		 }

		 private static readonly IndexSpecificSpaceFillingCurveSettingsCache _spaceFillingCurveSettings = new IndexSpecificSpaceFillingCurveSettingsCache( new ConfiguredSpaceFillingCurveSettingsCache( Config.defaults() ), Collections.emptyMap() );
		 private static readonly StandardConfiguration _configuration = new StandardConfiguration();

		 private readonly AccessorFactory<KEY, VALUE> _accessorFactory;
		 private readonly ValueType[] _supportedTypes;
		 private readonly IndexLayoutFactory<KEY, VALUE> _indexLayoutFactory;
		 private readonly IndexCapability _indexCapability;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unused") public NativeIndexAccessorTest(String name, AccessorFactory<KEY,VALUE> accessorFactory, org.neo4j.values.storable.ValueType[] supportedTypes, IndexLayoutFactory<KEY,VALUE> indexLayoutFactory, org.neo4j.internal.kernel.api.IndexCapability indexCapability)
		 public NativeIndexAccessorTest( string name, AccessorFactory<KEY, VALUE> accessorFactory, ValueType[] supportedTypes, IndexLayoutFactory<KEY, VALUE> indexLayoutFactory, IndexCapability indexCapability )
		 {
			  this._accessorFactory = accessorFactory;
			  this._supportedTypes = supportedTypes;
			  this._indexLayoutFactory = indexLayoutFactory;
			  this._indexCapability = indexCapability;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: NativeIndexAccessor<KEY,VALUE> makeAccessor() throws java.io.IOException
		 internal override NativeIndexAccessor<KEY, VALUE> MakeAccessor()
		 {
			  return _accessorFactory.create( pageCache, fs, IndexFile, layout, RecoveryCleanupWorkCollector.immediate(), monitor, indexDescriptor, indexDirectoryStructure, false );
		 }

		 internal override IndexCapability IndexCapability()
		 {
			  return _indexCapability;
		 }

		 internal override ValueCreatorUtil<KEY, VALUE> CreateValueCreatorUtil()
		 {
			  return new ValueCreatorUtil<KEY, VALUE>( forLabel( 42, 666 ).withId( 0 ), _supportedTypes, FRACTION_DUPLICATE_NON_UNIQUE );
		 }

		 internal override IndexLayout<KEY, VALUE> CreateLayout()
		 {
			  return _indexLayoutFactory.create();
		 }

		 /* Helpers */
		 private static AccessorFactory<NumberIndexKey, NativeIndexValue> NumberAccessorFactory()
		 {
			  return ( pageCache, fs, storeFile, layout, recoveryCleanupWorkCollector, monitor, descriptor, directory, readOnly ) => new NumberIndexAccessor( pageCache, fs, storeFile, layout, recoveryCleanupWorkCollector, monitor, descriptor, readOnly );
		 }

		 private static AccessorFactory<StringIndexKey, NativeIndexValue> StringAccessorFactory()
		 {
			  return ( pageCache, fs, storeFile, layout, recoveryCleanupWorkCollector, monitor, descriptor, directory, readOnly ) => new StringIndexAccessor( pageCache, fs, storeFile, layout, recoveryCleanupWorkCollector, monitor, descriptor, readOnly );
		 }

		 private static AccessorFactory<TK, NativeIndexValue> TemporalAccessorFactory<TK>( ValueGroup temporalValueGroup ) where TK : NativeIndexSingleValueKey<TK>
		 {
			  return ( pageCache, fs, storeFile, layout, cleanup, monitor, descriptor, directory, readOnly ) =>
			  {
				TemporalIndexFiles.FileLayout<TK> fileLayout = new TemporalIndexFiles.FileLayout<TK>( storeFile, layout, temporalValueGroup );
				return new TemporalIndexAccessor.PartAccessor<TK, NativeIndexValue>( pageCache, fs, fileLayout, cleanup, monitor, descriptor, readOnly );
			  };
		 }

		 private static AccessorFactory<GenericKey, NativeIndexValue> GenericAccessorFactory()
		 {
			  return ( pageCache, fs, storeFile, layout, cleanup, monitor, descriptor, directory, readOnly ) =>
			  {
				IndexDropAction dropAction = new FileSystemIndexDropAction( fs, directory );
				return new GenericNativeIndexAccessor( pageCache, fs, storeFile, layout, cleanup, monitor, descriptor, _spaceFillingCurveSettings, _configuration, dropAction, readOnly );
			  };
		 }

		 private interface AccessorFactory<KEY, VALUE> where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: NativeIndexAccessor<KEY,VALUE> create(org.neo4j.io.pagecache.PageCache pageCache, org.neo4j.io.fs.FileSystemAbstraction fs, java.io.File storeFile, IndexLayout<KEY,VALUE> layout, org.neo4j.index.internal.gbptree.RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, org.neo4j.kernel.api.index.IndexProvider.Monitor monitor, org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor, org.neo4j.kernel.api.index.IndexDirectoryStructure directory, boolean readOnly) throws java.io.IOException;
			  NativeIndexAccessor<KEY, VALUE> Create( PageCache pageCache, FileSystemAbstraction fs, File storeFile, IndexLayout<KEY, VALUE> layout, RecoveryCleanupWorkCollector recoveryCleanupWorkCollector, IndexProvider.Monitor monitor, StoreIndexDescriptor descriptor, IndexDirectoryStructure directory, bool readOnly );
		 }
	}

}