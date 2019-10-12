/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
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
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.causalclustering.catchup.storecopy
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;

	using StoreType = Org.Neo4j.Kernel.impl.store.StoreType;
	using Monitors = Org.Neo4j.Kernel.monitoring.Monitors;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class StreamToDiskTest
	{
		private bool InstanceFieldsInitialized = false;

		public StreamToDiskTest()
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

		 private static readonly sbyte[] _data = new sbyte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15 };

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
			  Monitors monitors = new Monitors();
			  StreamToDiskProvider writerProvider = new StreamToDiskProvider( _directory.databaseDir(), _fs, monitors );

			  // WHEN
			  foreach ( StoreType type in StoreType.values() )
			  {
					if ( type.RecordStore )
					{
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
						 File[] files = _directory.databaseLayout().file(type.DatabaseFile).toArray(File[]::new);
						 foreach ( File file in files )
						 {
							  WriteAndVerify( writerProvider, file );
						 }
					}
			  }
			  WriteAndVerify( writerProvider, _directory.databaseLayout().labelScanStore() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeAndVerify(StreamToDiskProvider writerProvider, java.io.File file) throws Exception
		 private void WriteAndVerify( StreamToDiskProvider writerProvider, File file )
		 {
			  using ( StoreFileStream acquire = writerProvider.Acquire( file.Name, 16 ) )
			  {
					acquire.Write( _data );
			  }
			  assertTrue( "Streamed file created.", _fs.fileExists( file ) );
			  assertEquals( _data.Length, _fs.getFileSize( file ) );
		 }
	}

}