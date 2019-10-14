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

	using RecoveryCleanupWorkCollector = Neo4Net.Index.Internal.gbptree.RecoveryCleanupWorkCollector;
	using InternalIndexState = Neo4Net.Internal.Kernel.Api.InternalIndexState;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using IndexDirectoryStructure = Neo4Net.Kernel.Api.Index.IndexDirectoryStructure;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using Config = Neo4Net.Kernel.configuration.Config;
	using CoordinateReferenceSystem = Neo4Net.Values.Storable.CoordinateReferenceSystem;
	using DateValue = Neo4Net.Values.Storable.DateValue;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.InternalIndexState.ONLINE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.Internal.kernel.api.InternalIndexState.POPULATING;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @RunWith(Parameterized.class) public class NativeIndexProviderTest extends NativeIndexProviderTests
	public class NativeIndexProviderTest : NativeIndexProviderTests
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameters(name = "{index} {0}") public static Object[][] data()
		 public static object[][] Data()
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  return new object[][]
			  {
				  new object[] { "Number", ( ProviderFactory ) NumberIndexProvider::new, POPULATING, Values.of( 1 ) },
				  new object[] { "String", ( ProviderFactory ) StringIndexProvider::new, POPULATING, Values.of( "string" ) },
				  new object[] { "Spatial", SpatialProviderFactory(), ONLINE, Values.pointValue(CoordinateReferenceSystem.WGS84, 0, 0) },
				  new object[] { "Temporal", ( ProviderFactory ) TemporalIndexProvider::new, ONLINE, DateValue.date( 1, 1, 1 ) },
				  new object[] { "Generic", GenericProviderFactory(), POPULATING, Values.of(1) }
			  };
		 }

		 private static ProviderFactory GenericProviderFactory()
		 {
			  return ( pageCache, fs, dir, monitor, collector, readOnly ) => new GenericNativeIndexProvider( dir, pageCache, fs, monitor, collector, readOnly, Config.defaults() );
		 }

		 private static ProviderFactory SpatialProviderFactory()
		 {
			  return ( pageCache, fs, dir, monitor, collector, readOnly ) => new SpatialIndexProvider( pageCache, fs, dir, monitor, collector, readOnly, Config.defaults() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter public String name;
		 public string Name;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(1) public ProviderFactory providerFactory;
		 public ProviderFactory ProviderFactory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(2) public org.neo4j.internal.kernel.api.InternalIndexState expectedStateOnNonExistingSubIndex;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public InternalIndexState ExpectedStateOnNonExistingSubIndexConflict;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Parameterized.Parameter(3) public org.neo4j.values.storable.Value someValue;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 public Value SomeValueConflict;

		 protected internal override InternalIndexState ExpectedStateOnNonExistingSubIndex()
		 {
			  return ExpectedStateOnNonExistingSubIndexConflict;
		 }

		 protected internal override Value SomeValue()
		 {
			  return SomeValueConflict;
		 }

		 internal override IndexProvider NewProvider( PageCache pageCache, FileSystemAbstraction fs, IndexDirectoryStructure.Factory dir, IndexProvider.Monitor monitor, RecoveryCleanupWorkCollector collector, bool readOnly )
		 {
			  return ProviderFactory.create( pageCache, fs, dir, monitor, collector, readOnly );
		 }

		 private delegate IndexProvider ProviderFactory( PageCache pageCache, FileSystemAbstraction fs, IndexDirectoryStructure.Factory dir, IndexProvider.Monitor monitor, RecoveryCleanupWorkCollector collector, bool readOnly );
	}

}