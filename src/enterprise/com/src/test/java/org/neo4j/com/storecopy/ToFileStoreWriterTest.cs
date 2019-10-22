/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.com.storecopy
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using StoreType = Neo4Net.Kernel.impl.store.StoreType;
	using PageCacheRule = Neo4Net.Test.rule.PageCacheRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class ToFileStoreWriterTest
	{
		private bool InstanceFieldsInitialized = false;

		public ToFileStoreWriterTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			_directory = TestDirectory.testDirectory( _fs );
			Rules = RuleChain.outerRule( _fs ).around( _directory ).around( _pageCacheRule );
		}

		 private readonly EphemeralFileSystemRule _fs = new EphemeralFileSystemRule();
		 private TestDirectory _directory;
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain rules = org.junit.rules.RuleChain.outerRule(fs).around(directory).around(pageCacheRule);
		 public RuleChain Rules;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldLetPageCacheHandleRecordStoresAndNativeLabelScanStoreFiles() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldLetPageCacheHandleRecordStoresAndNativeLabelScanStoreFiles()
		 {
			  // GIVEN
			  ToFileStoreWriter writer = new ToFileStoreWriter( _directory.databaseDir(), _fs, new StoreCopyClientMonitor_Adapter() );
			  ByteBuffer tempBuffer = ByteBuffer.allocate( 128 );

			  // WHEN
			  foreach ( StoreType type in StoreType.values() )
			  {
					if ( type.RecordStore )
					{
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
						 File[] files = _directory.databaseLayout().file(type.DatabaseFile).toArray(File[]::new);
						 foreach ( File file in files )
						 {
							  WriteAndVerify( writer, tempBuffer, file );
						 }
					}
			  }
			  WriteAndVerify( writer, tempBuffer, _directory.databaseLayout().labelScanStore() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void fullPathFileNamesUsedForMonitoringBackup() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FullPathFileNamesUsedForMonitoringBackup()
		 {
			  // given
			  AtomicBoolean wasActivated = new AtomicBoolean( false );
			  StoreCopyClientMonitor monitor = new StoreCopyClientMonitor_AdapterAnonymousInnerClass( this, wasActivated );

			  // and
			  ToFileStoreWriter writer = new ToFileStoreWriter( _directory.absolutePath(), _fs, monitor );
			  ByteBuffer tempBuffer = ByteBuffer.allocate( 128 );

			  // when
			  writer.Write( "expectedFileName", new DataProducer( 16 ), tempBuffer, true, 16 );

			  // then
			  assertTrue( wasActivated.get() );
		 }

		 private class StoreCopyClientMonitor_AdapterAnonymousInnerClass : StoreCopyClientMonitor_Adapter
		 {
			 private readonly ToFileStoreWriterTest _outerInstance;

			 private AtomicBoolean _wasActivated;

			 public StoreCopyClientMonitor_AdapterAnonymousInnerClass( ToFileStoreWriterTest outerInstance, AtomicBoolean wasActivated )
			 {
				 this.outerInstance = outerInstance;
				 this._wasActivated = wasActivated;
			 }

			 public override void startReceivingStoreFile( string file )
			 {
				  assertTrue( file.Contains( "expectedFileName" ) );
				  _wasActivated.set( true );
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeAndVerify(ToFileStoreWriter writer, ByteBuffer tempBuffer, java.io.File file) throws java.io.IOException
		 private void WriteAndVerify( ToFileStoreWriter writer, ByteBuffer tempBuffer, File file )
		 {
			  writer.Write( file.Name, new DataProducer( 16 ), tempBuffer, true, 16 );
			  assertTrue( "File created by writer should exist.", _fs.fileExists( file ) );
			  assertEquals( 16, _fs.getFileSize( file ) );
		 }
	}

}