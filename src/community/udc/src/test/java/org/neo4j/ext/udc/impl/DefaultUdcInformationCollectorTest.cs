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
namespace Neo4Net.Ext.Udc.impl
{
	using BeforeEach = org.junit.jupiter.api.BeforeEach;
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;
	using Mockito = org.mockito.Mockito;


	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using NeoStoreDataSource = Neo4Net.Kernel.NeoStoreDataSource;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Edition = Neo4Net.Kernel.impl.factory.Edition;
	using OperationalMode = Neo4Net.Kernel.impl.factory.OperationalMode;
	using Neo4Net.Kernel.impl.store.format;
	using IdGenerator = Neo4Net.Kernel.impl.store.id.IdGenerator;
	using IdGeneratorFactory = Neo4Net.Kernel.impl.store.id.IdGeneratorFactory;
	using IdRange = Neo4Net.Kernel.impl.store.id.IdRange;
	using IdType = Neo4Net.Kernel.impl.store.id.IdType;
	using DataSourceManager = Neo4Net.Kernel.impl.transaction.state.DataSourceManager;
	using Dependencies = Neo4Net.Kernel.impl.util.Dependencies;
	using IJobScheduler = Neo4Net.Scheduler.JobScheduler;
	using StoreFileMetadata = Neo4Net.Storageengine.Api.StoreFileMetadata;
	using StoreId = Neo4Net.Storageengine.Api.StoreId;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using UsageData = Neo4Net.Udc.UsageData;
	using UsageDataKeys = Neo4Net.Udc.UsageDataKeys;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNotNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.udc.UsageDataKeys.Features_Fields.bolt;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class DefaultUdcInformationCollectorTest
	internal class DefaultUdcInformationCollectorTest
	{
		private bool InstanceFieldsInitialized = false;

		public DefaultUdcInformationCollectorTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_collector = new DefaultUdcInformationCollector( Config.defaults(), _dataSourceManager, _usageData );
		}

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.Neo4Net.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;

		 private readonly UsageData _usageData = new UsageData( mock( typeof( IJobScheduler ) ) );

		 private readonly DataSourceManager _dataSourceManager = new DataSourceManager( Config.defaults() );
		 private readonly NeoStoreDataSource _dataSource = mock( typeof( NeoStoreDataSource ) );
		 private DefaultUdcInformationCollector _collector;
		 private readonly DefaultFileSystemAbstraction _fileSystem = mock( typeof( DefaultFileSystemAbstraction ) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @BeforeEach void setUp()
		 internal virtual void SetUp()
		 {
			  Dependencies dependencies = new Dependencies();
			  dependencies.SatisfyDependencies( new StubIdGeneratorFactory() );
			  dependencies.SatisfyDependencies( _fileSystem );
			  when( _dataSource.DependencyResolver ).thenReturn( dependencies );
			  when( _dataSource.DatabaseLayout ).thenReturn( DatabaseLayout.of( new File( "database" ) ) );
			  when( _dataSource.StoreId ).thenReturn( StoreId.DEFAULT );

			  _dataSourceManager.start();
			  _dataSourceManager.register( _dataSource );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldIncludeTheMacAddress()
		 internal virtual void ShouldIncludeTheMacAddress()
		 {
			  assertNotNull( _collector.UdcParams[UdcConstants.MAC] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldIncludeTheNumberOfProcessors()
		 internal virtual void ShouldIncludeTheNumberOfProcessors()
		 {
			  assertNotNull( _collector.UdcParams[UdcConstants.NUM_PROCESSORS] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldIncludeTotalMemorySize()
		 internal virtual void ShouldIncludeTotalMemorySize()
		 {
			  assertNotNull( _collector.UdcParams[UdcConstants.TOTAL_MEMORY] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldIncludeHeapSize()
		 internal virtual void ShouldIncludeHeapSize()
		 {
			  assertNotNull( _collector.UdcParams[UdcConstants.HEAP_SIZE] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldIncludeNodeIdsInUse()
		 internal virtual void ShouldIncludeNodeIdsInUse()
		 {
			  assertEquals( "100", _collector.UdcParams[UdcConstants.NODE_IDS_IN_USE] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldIncludeRelationshipIdsInUse()
		 internal virtual void ShouldIncludeRelationshipIdsInUse()
		 {
			  assertEquals( "200", _collector.UdcParams[UdcConstants.RELATIONSHIP_IDS_IN_USE] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldIncludePropertyIdsInUse()
		 internal virtual void ShouldIncludePropertyIdsInUse()
		 {
			  assertEquals( "400", _collector.UdcParams[UdcConstants.PROPERTY_IDS_IN_USE] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldIncludeLabelIdsInUse()
		 internal virtual void ShouldIncludeLabelIdsInUse()
		 {
			  assertEquals( "300", _collector.UdcParams[UdcConstants.LABEL_IDS_IN_USE] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldIncludeVersionEditionAndMode()
		 internal virtual void ShouldIncludeVersionEditionAndMode()
		 {
			  // Given
			  _usageData.set( UsageDataKeys.version, "1.2.3" );
			  _usageData.set( UsageDataKeys.edition, Edition.enterprise );
			  _usageData.set( UsageDataKeys.operationalMode, OperationalMode.ha );

			  // When & Then
			  assertEquals( "1.2.3", _collector.UdcParams[UdcConstants.VERSION] );
			  assertEquals( "enterprise", _collector.UdcParams[UdcConstants.EDITION] );
			  assertEquals( "ha", _collector.UdcParams[UdcConstants.DATABASE_MODE] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldIncludeRecentClientNames()
		 internal virtual void ShouldIncludeRecentClientNames()
		 {
			  // Given
			  _usageData.get( UsageDataKeys.clientNames ).add( "SteveBrookClient/1.0" );
			  _usageData.get( UsageDataKeys.clientNames ).add( "MayorClient/1.0" );

			  // When & Then
			  string userAgents = _collector.UdcParams[UdcConstants.USER_AGENTS];
			  if ( !( userAgents.Equals( "SteveBrookClient/1.0,MayorClient/1.0" ) || userAgents.Equals( "MayorClient/1.0,SteveBrookClient/1.0" ) ) )
			  {
					fail( "Expected \"SteveBrookClient/1.0,MayorClient/1.0\" or \"MayorClient/1.0,SteveBrookClient/1.0\", " + "got \"" + userAgents + "\"" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldIncludePopularFeatures()
		 internal virtual void ShouldIncludePopularFeatures()
		 {
			  // Given
			  _usageData.get( UsageDataKeys.features ).flag( bolt );

			  // When & Then
			  assertEquals( "1000", _collector.UdcParams[UdcConstants.FEATURES] );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldReportStoreSizes() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldReportStoreSizes()
		 {
			  UdcInformationCollector collector = new DefaultUdcInformationCollector( Config.defaults(), _dataSourceManager, _usageData );

			  when( _fileSystem.getFileSize( Mockito.any() ) ).thenReturn(152L);
			  IDictionary<string, string> udcParams = collector.UdcParams;

			  assertThat( udcParams["storesize"], @is( "152" ) );
		 }

		 private static ISet<StoreFileMetadata> ToMeta( params File[] files )
		 {
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  return java.util.files.Select( file => new StoreFileMetadata( file, Neo4Net.Kernel.impl.store.format.RecordFormat_Fields.NO_RECORD_SIZE ) ).collect( Collectors.toCollection( HashSet<object>::new ) );
		 }

		 private class StubIdGeneratorFactory : IdGeneratorFactory
		 {
			  internal readonly IDictionary<IdType, long> IdsInUse = new Dictionary<IdType, long>();

			  internal StubIdGeneratorFactory()
			  {
					IdsInUse[IdType.NODE] = 100L;
					IdsInUse[IdType.RELATIONSHIP] = 200L;
					IdsInUse[IdType.LABEL_TOKEN] = 300L;
					IdsInUse[IdType.PROPERTY] = 400L;
			  }

			  public override IdGenerator Open( File filename, IdType idType, System.Func<long> highId, long maxId )
			  {
					return Open( filename, 0, idType, highId, maxId );
			  }

			  public override IdGenerator Open( File fileName, int grabSize, IdType idType, System.Func<long> highId, long maxId )
			  {
					return Get( idType );
			  }

			  public override void Create( File fileName, long highId, bool throwIfFileExists )
			  { // Ignore
			  }

			  public override IdGenerator Get( IdType idType )
			  {
					return new StubIdGenerator( IdsInUse[idType] );
			  }
		 }

		 private class StubIdGenerator : IdGenerator
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly long NumberOfIdsInUseConflict;

			  internal StubIdGenerator( long numberOfIdsInUse )
			  {
					this.NumberOfIdsInUseConflict = numberOfIdsInUse;
			  }

			  public override long NextId()
			  {
					throw new System.NotSupportedException( "Please implement" );
			  }

			  public override IdRange NextIdBatch( int size )
			  {
					throw new System.NotSupportedException( "Please implement" );
			  }

			  public virtual long HighId
			  {
				  set
				  {
						throw new System.NotSupportedException( "Please implement" );
				  }
				  get
				  {
						return 0;
				  }
			  }


			  public virtual long HighestPossibleIdInUse
			  {
				  get
				  {
						return 0;
				  }
			  }

			  public override void FreeId( long id )
			  { // Ignore
			  }

			  public override void Close()
			  { // Ignore
			  }

			  public virtual long NumberOfIdsInUse
			  {
				  get
				  {
						return NumberOfIdsInUseConflict;
				  }
			  }

			  public virtual long DefragCount
			  {
				  get
				  {
						return 0;
				  }
			  }

			  public override void Delete()
			  { // Ignore
			  }
		 }
	}

}