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
namespace Neo4Net.@unsafe.Batchinsert.Internal
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;
	using RuleChain = org.junit.rules.RuleChain;

	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using StoreLayout = Neo4Net.Io.layout.StoreLayout;
	using PageCache = Neo4Net.Io.pagecache.PageCache;
	using MuninnPageCache = Neo4Net.Io.pagecache.impl.muninn.MuninnPageCache;
	using StoreLockException = Neo4Net.Kernel.StoreLockException;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using StoreLocker = Neo4Net.Kernel.Internal.locker.StoreLocker;
	using ReflectionUtil = Neo4Net.Test.ReflectionUtil;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Neo4Net.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.allOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.greaterThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.lessThan;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.MapUtil.stringMap;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.ByteUnit.kibiBytes;

	public class BatchInserterImplTest
	{
		private bool InstanceFieldsInitialized = false;

		public BatchInserterImplTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _testDirectory ).around( _expected ).around( _fileSystemRule );
		}

		 private readonly TestDirectory _testDirectory = TestDirectory.testDirectory();
		 private readonly ExpectedException _expected = ExpectedException.none();
		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(testDirectory).around(expected).around(fileSystemRule);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testHonorsPassedInParams() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestHonorsPassedInParams()
		 {
			  BatchInserter inserter = BatchInserters.inserter( _testDirectory.databaseDir(), _fileSystemRule.get(), stringMap(GraphDatabaseSettings.pagecache_memory.name(), "280K") );
			  NeoStores neoStores = ReflectionUtil.getPrivateField( inserter, "neoStores", typeof( NeoStores ) );
			  PageCache pageCache = ReflectionUtil.getPrivateField( neoStores, "pageCache", typeof( PageCache ) );
			  inserter.Shutdown();
			  long mappedMemoryTotalSize = MuninnPageCache.memoryRequiredForPages( pageCache.MaxCachedPages() );
			  assertThat( "memory mapped config is active", mappedMemoryTotalSize, @is( allOf( greaterThan( kibiBytes( 270 ) ), lessThan( kibiBytes( 290 ) ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testCreatesStoreLockFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestCreatesStoreLockFile()
		 {
			  // Given
			  DatabaseLayout databaseLayout = _testDirectory.databaseLayout();

			  // When
			  BatchInserter inserter = BatchInserters.inserter( databaseLayout.DatabaseDirectory(), _fileSystemRule.get() );

			  // Then
			  assertThat( databaseLayout.StoreLayout.storeLockFile().exists(), equalTo(true) );
			  inserter.Shutdown();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void testFailsOnExistingStoreLockFile() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void TestFailsOnExistingStoreLockFile()
		 {
			  // Given
			  StoreLayout storeLayout = _testDirectory.storeLayout();
			  using ( FileSystemAbstraction fileSystemAbstraction = new DefaultFileSystemAbstraction(), StoreLocker @lock = new StoreLocker(fileSystemAbstraction, storeLayout) )
			  {
					@lock.CheckLock();

					// Then
					_expected.expect( typeof( StoreLockException ) );
					_expected.expectMessage( "Unable to obtain lock on store lock file" );
					// When
					BatchInserters.inserter( storeLayout.DatabaseLayout( "any" ).databaseDirectory(), fileSystemAbstraction );
			  }
		 }
	}

}