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
namespace Org.Neo4j.Kernel.Impl.Index.Schema
{

	using StandardConfiguration = Org.Neo4j.Gis.Spatial.Index.curves.StandardConfiguration;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using IndexDirectoryStructure = Org.Neo4j.Kernel.Api.Index.IndexDirectoryStructure;
	using IndexProvider = Org.Neo4j.Kernel.Api.Index.IndexProvider;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using ConfiguredSpaceFillingCurveSettingsCache = Org.Neo4j.Kernel.Impl.Index.Schema.config.ConfiguredSpaceFillingCurveSettingsCache;
	using IndexSpecificSpaceFillingCurveSettingsCache = Org.Neo4j.Kernel.Impl.Index.Schema.config.IndexSpecificSpaceFillingCurveSettingsCache;
	using StoreIndexDescriptor = Org.Neo4j.Storageengine.Api.schema.StoreIndexDescriptor;
	using RandomValues = Org.Neo4j.Values.Storable.RandomValues;
	using ValueGroup = Org.Neo4j.Values.Storable.ValueGroup;
	using ValueType = Org.Neo4j.Values.Storable.ValueType;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.index.schema.ByteBufferFactory.heapBufferFactory;

	internal class NativeIndexPopulatorTestCases
	{
		 internal class TestCase<KEY, VALUE> where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
		 {
			  internal readonly string Name;
			  internal readonly PopulatorFactory<KEY, VALUE> PopulatorFactory;
			  internal readonly ValueType[] TypesOfGroup;
			  internal readonly IndexLayoutFactory<KEY, VALUE> IndexLayoutFactory;

			  internal TestCase( string name, PopulatorFactory<KEY, VALUE> populatorFactory, ValueType[] typesOfGroup, IndexLayoutFactory<KEY, VALUE> indexLayoutFactory )
			  {
					this.Name = name;
					this.PopulatorFactory = populatorFactory;
					this.TypesOfGroup = typesOfGroup;
					this.IndexLayoutFactory = indexLayoutFactory;
			  }

			  public override string ToString()
			  {
					return Name;
			  }
		 }

		 internal static ICollection<object[]> AllCases()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return Arrays.asList(new object[][]
			  {
				  new object[] { new TestCase<>( "Number", NumberPopulatorFactory(), RandomValues.typesOfGroup(ValueGroup.NUMBER), NumberLayoutNonUnique::new ) },
				  new object[] { new TestCase<>( "String", StringIndexPopulator::new, RandomValues.typesOfGroup( ValueGroup.TEXT ), StringLayout::new ) },
				  new object[] { new TestCase<>( "Date", TemporalPopulatorFactory( ValueGroup.DATE ), RandomValues.typesOfGroup( ValueGroup.DATE ), DateLayout::new ) },
				  new object[] { new TestCase<>( "DateTime", TemporalPopulatorFactory( ValueGroup.ZONED_DATE_TIME ), RandomValues.typesOfGroup( ValueGroup.ZONED_DATE_TIME ), ZonedDateTimeLayout::new ) },
				  new object[] { new TestCase<>( "Duration", TemporalPopulatorFactory( ValueGroup.DURATION ), RandomValues.typesOfGroup( ValueGroup.DURATION ), DurationLayout::new ) },
				  new object[] { new TestCase<>( "LocalDateTime", TemporalPopulatorFactory( ValueGroup.LOCAL_DATE_TIME ), RandomValues.typesOfGroup( ValueGroup.LOCAL_DATE_TIME ), LocalDateTimeLayout::new ) },
				  new object[] { new TestCase<>( "LocalTime", TemporalPopulatorFactory( ValueGroup.LOCAL_TIME ), RandomValues.typesOfGroup( ValueGroup.LOCAL_TIME ), LocalTimeLayout::new ) },
				  new object[] { new TestCase<>( "LocalDateTime", TemporalPopulatorFactory( ValueGroup.LOCAL_DATE_TIME ), RandomValues.typesOfGroup( ValueGroup.LOCAL_DATE_TIME ), LocalDateTimeLayout::new ) },
				  new object[] { new TestCase<>( "Time", TemporalPopulatorFactory( ValueGroup.ZONED_TIME ), RandomValues.typesOfGroup( ValueGroup.ZONED_TIME ), ZonedTimeLayout::new ) },
				  new object[] { new TestCase<>( "Generic", GenericPopulatorFactory(), ValueType.values(), () => new GenericLayout(1, _spaceFillingCurveSettings) ) },
				  new object[] { new TestCase<>( "Generic-BlockBased", GenericBlockBasedPopulatorFactory(), ValueType.values(), () => new GenericLayout(1, _spaceFillingCurveSettings) ) }
			  });
			  // { Spatial has it's own subclass because it need to override some of the test methods }
		 }

		 private static readonly IndexSpecificSpaceFillingCurveSettingsCache _spaceFillingCurveSettings = new IndexSpecificSpaceFillingCurveSettingsCache( new ConfiguredSpaceFillingCurveSettingsCache( Config.defaults() ), new Dictionary<Org.Neo4j.Values.Storable.CoordinateReferenceSystem, SpaceFillingCurveSettings>() );
		 private static readonly StandardConfiguration _configuration = new StandardConfiguration();

		 private static PopulatorFactory<NumberIndexKey, NativeIndexValue> NumberPopulatorFactory()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return NumberIndexPopulator::new;
		 }

		 private static PopulatorFactory<TK, NativeIndexValue> TemporalPopulatorFactory<TK>( ValueGroup temporalValueGroup ) where TK : NativeIndexSingleValueKey<TK>
		 {
			  return ( pageCache, fs, storeFile, layout, monitor, descriptor ) =>
			  {
				TemporalIndexFiles.FileLayout<TK> fileLayout = new TemporalIndexFiles.FileLayout<TK>( storeFile, layout, temporalValueGroup );
				return new TemporalIndexPopulator.PartPopulator<TK, NativeIndexValue>( pageCache, fs, fileLayout, monitor, descriptor );
			  };
		 }

		 private static PopulatorFactory<GenericKey, NativeIndexValue> GenericPopulatorFactory()
		 {
			  return ( pageCache, fs, storeFile, layout, monitor, descriptor ) =>
			  {
				IndexDirectoryStructure directoryStructure = SimpleIndexDirectoryStructures.OnIndexFile( storeFile );
				IndexDropAction dropAction = new FileSystemIndexDropAction( fs, directoryStructure );
				return new GenericNativeIndexPopulator( pageCache, fs, storeFile, layout, monitor, descriptor, _spaceFillingCurveSettings, directoryStructure, _configuration, dropAction, false );
			  };
		 }

		 private static PopulatorFactory<GenericKey, NativeIndexValue> GenericBlockBasedPopulatorFactory()
		 {
			  return ( pageCache, fs, storeFile, layout, monitor, descriptor ) =>
			  {
				IndexDirectoryStructure directoryStructure = SimpleIndexDirectoryStructures.OnIndexFile( storeFile );
				IndexDropAction dropAction = new FileSystemIndexDropAction( fs, directoryStructure );
				return new GenericBlockBasedIndexPopulator( pageCache, fs, storeFile, layout, monitor, descriptor, _spaceFillingCurveSettings, directoryStructure, _configuration, dropAction, false, heapBufferFactory( 10 * 1024 ) );
			  };
		 }

		 public interface PopulatorFactory<KEY, VALUE> where KEY : NativeIndexKey<KEY> where VALUE : NativeIndexValue
		 {
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: NativeIndexPopulator<KEY,VALUE> create(org.neo4j.io.pagecache.PageCache pageCache, org.neo4j.io.fs.FileSystemAbstraction fs, java.io.File storeFile, IndexLayout<KEY,VALUE> layout, org.neo4j.kernel.api.index.IndexProvider.Monitor monitor, org.neo4j.storageengine.api.schema.StoreIndexDescriptor descriptor) throws java.io.IOException;
			  NativeIndexPopulator<KEY, VALUE> Create( PageCache pageCache, FileSystemAbstraction fs, File storeFile, IndexLayout<KEY, VALUE> layout, IndexProvider.Monitor monitor, StoreIndexDescriptor descriptor );
		 }
	}

}