﻿/*
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
namespace Org.Neo4j.Kernel.impl.store
{
	using Before = org.junit.Before;
	using ClassRule = org.junit.ClassRule;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using EphemeralFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using JumpingIdGeneratorFactory = Org.Neo4j.Kernel.impl.core.JumpingIdGeneratorFactory;
	using RecordFormatSelector = Org.Neo4j.Kernel.impl.store.format.RecordFormatSelector;
	using DynamicRecord = Org.Neo4j.Kernel.impl.store.record.DynamicRecord;
	using PropertyBlock = Org.Neo4j.Kernel.impl.store.record.PropertyBlock;
	using PropertyKeyTokenRecord = Org.Neo4j.Kernel.impl.store.record.PropertyKeyTokenRecord;
	using PropertyRecord = Org.Neo4j.Kernel.impl.store.record.PropertyRecord;
	using NullLogProvider = Org.Neo4j.Logging.NullLogProvider;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doAnswer;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.record.RecordLoad.FORCE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.rule.PageCacheRule.config;

	public class PropertyStoreTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ClassRule public static org.neo4j.test.rule.PageCacheRule pageCacheRule = new org.neo4j.test.rule.PageCacheRule(config().withInconsistentReads(false));
		 public static PageCacheRule PageCacheRule = new PageCacheRule( config().withInconsistentReads(false) );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.EphemeralFileSystemRule fsRule = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule FsRule = new EphemeralFileSystemRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
		 private EphemeralFileSystemAbstraction _fileSystemAbstraction;
		 private File _storeFile;
		 private File _idFile;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void setup()
		 public virtual void Setup()
		 {
			  _fileSystemAbstraction = FsRule.get();
			  _storeFile = TestDirectory.databaseLayout().propertyStore();
			  _idFile = TestDirectory.databaseLayout().idPropertyStore();

			  _fileSystemAbstraction.mkdir( _storeFile.ParentFile );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldWriteOutTheDynamicChainBeforeUpdatingThePropertyRecord()
		 public virtual void ShouldWriteOutTheDynamicChainBeforeUpdatingThePropertyRecord()
		 {
			  // given
			  PageCache pageCache = PageCacheRule.getPageCache( _fileSystemAbstraction );
			  Config config = Config.defaults( GraphDatabaseSettings.rebuild_idgenerators_fast, "true" );

			  DynamicStringStore stringPropertyStore = mock( typeof( DynamicStringStore ) );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final PropertyStore store = new PropertyStore(storeFile, idFile, config, new org.neo4j.kernel.impl.core.JumpingIdGeneratorFactory(1), pageCache, org.neo4j.logging.NullLogProvider.getInstance(), stringPropertyStore, mock(PropertyKeyTokenStore.class), mock(DynamicArrayStore.class), org.neo4j.kernel.impl.store.format.RecordFormatSelector.defaultFormat());
			  PropertyStore store = new PropertyStore( _storeFile, _idFile, config, new JumpingIdGeneratorFactory( 1 ), pageCache, NullLogProvider.Instance, stringPropertyStore, mock( typeof( PropertyKeyTokenStore ) ), mock( typeof( DynamicArrayStore ) ), RecordFormatSelector.defaultFormat() );
			  store.Initialise( true );

			  try
			  {
					store.MakeStoreOk();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long propertyRecordId = store.nextId();
					long propertyRecordId = store.NextId();

					PropertyRecord record = new PropertyRecord( propertyRecordId );
					record.InUse = true;

					DynamicRecord dynamicRecord = dynamicRecord();
					PropertyBlock propertyBlock = PropertyBlockWith( dynamicRecord );
					record.PropertyBlock = propertyBlock;

					doAnswer(invocation =>
					{
					 PropertyRecord recordBeforeWrite = store.GetRecord( propertyRecordId, store.NewRecord(), FORCE );
					 assertFalse( recordBeforeWrite.inUse() );
					 return null;
					}).when( stringPropertyStore ).updateRecord( dynamicRecord );

					// when
					store.UpdateRecord( record );

					// then verify that our mocked method above, with the assert, was actually called
					verify( stringPropertyStore ).updateRecord( dynamicRecord );
			  }
			  finally
			  {
					store.Close();
			  }
		 }

		 private DynamicRecord DynamicRecord()
		 {
			  DynamicRecord dynamicRecord = new DynamicRecord( 42 );
			  dynamicRecord.SetType( PropertyType.String.intValue() );
			  dynamicRecord.SetCreated();
			  return dynamicRecord;
		 }

		 private PropertyBlock PropertyBlockWith( DynamicRecord dynamicRecord )
		 {
			  PropertyBlock propertyBlock = new PropertyBlock();

			  PropertyKeyTokenRecord key = new PropertyKeyTokenRecord( 10 );
			  propertyBlock.SingleBlock = key.Id | ( ( ( long ) PropertyType.String.intValue() ) << 24 ) | (dynamicRecord.Id << 28);
			  propertyBlock.AddValueRecord( dynamicRecord );

			  return propertyBlock;
		 }
	}

}