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
namespace Org.Neo4j.Kernel.impl.storemigration
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using RuleChain = org.junit.rules.RuleChain;


	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using PageCache = Org.Neo4j.Io.pagecache.PageCache;
	using MetaDataStore = Org.Neo4j.Kernel.impl.store.MetaDataStore;
	using UTF8 = Org.Neo4j.@string.UTF8;
	using PageCacheRule = Org.Neo4j.Test.rule.PageCacheRule;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class StoreVersionCheckTest
	{
		private bool InstanceFieldsInitialized = false;

		public StoreVersionCheckTest()
		{
			if ( !InstanceFieldsInitialized )
			{
				InitializeInstanceFields();
				InstanceFieldsInitialized = true;
			}
		}

		private void InitializeInstanceFields()
		{
			RuleChain = RuleChain.outerRule( _directory ).around( _fileSystemRule ).around( _pageCacheRule );
		}

		 private readonly DefaultFileSystemRule _fileSystemRule = new DefaultFileSystemRule();
		 private readonly TestDirectory _directory = TestDirectory.testDirectory();
		 private readonly PageCacheRule _pageCacheRule = new PageCacheRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.junit.rules.RuleChain ruleChain = org.junit.rules.RuleChain.outerRule(directory).around(fileSystemRule).around(pageCacheRule);
		 public RuleChain RuleChain;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailIfFileDoesNotExist()
		 public virtual void ShouldFailIfFileDoesNotExist()
		 {
			  // given
			  File missingFile = new File( _directory.directory(), "missing-file" );
			  PageCache pageCache = _pageCacheRule.getPageCache( _fileSystemRule.get() );
			  StoreVersionCheck storeVersionCheck = new StoreVersionCheck( pageCache );

			  // when
			  StoreVersionCheck.Result result = storeVersionCheck.HasVersion( missingFile, "version" );

			  // then
			  assertFalse( result.Outcome.Successful );
			  assertEquals( StoreVersionCheck.Result.Outcome.MissingStoreFile, result.Outcome );
			  assertNull( result.ActualVersion );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportShortFileDoesNotHaveSpecifiedVersion() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportShortFileDoesNotHaveSpecifiedVersion()
		 {
			  // given
			  File shortFile = FileContaining( _fileSystemRule.get(), "nothing interesting" );
			  StoreVersionCheck storeVersionCheck = new StoreVersionCheck( _pageCacheRule.getPageCache( _fileSystemRule.get() ) );

			  // when
			  StoreVersionCheck.Result result = storeVersionCheck.HasVersion( shortFile, "version" );

			  // then
			  assertFalse( result.Outcome.Successful );
			  assertEquals( StoreVersionCheck.Result.Outcome.StoreVersionNotFound, result.Outcome );
			  assertNull( result.ActualVersion );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportFileWithIncorrectVersion() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportFileWithIncorrectVersion()
		 {
			  // given
			  File neoStore = EmptyFile( _fileSystemRule.get() );
			  long v1 = MetaDataStore.versionStringToLong( "V1" );
			  PageCache pageCache = _pageCacheRule.getPageCache( _fileSystemRule.get() );
			  MetaDataStore.setRecord( pageCache, neoStore, MetaDataStore.Position.STORE_VERSION, v1 );
			  StoreVersionCheck storeVersionCheck = new StoreVersionCheck( pageCache );

			  // when
			  StoreVersionCheck.Result result = storeVersionCheck.HasVersion( neoStore, "V2" );

			  // then
			  assertFalse( result.Outcome.Successful );
			  assertEquals( StoreVersionCheck.Result.Outcome.UnexpectedStoreVersion, result.Outcome );
			  assertEquals( "V1", result.ActualVersion );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportFileWithCorrectVersion() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportFileWithCorrectVersion()
		 {
			  // given
			  File neoStore = EmptyFile( _fileSystemRule.get() );
			  long v1 = MetaDataStore.versionStringToLong( "V1" );
			  PageCache pageCache = _pageCacheRule.getPageCache( _fileSystemRule.get() );
			  MetaDataStore.setRecord( pageCache, neoStore, MetaDataStore.Position.STORE_VERSION, v1 );
			  StoreVersionCheck storeVersionCheck = new StoreVersionCheck( pageCache );

			  // when
			  StoreVersionCheck.Result result = storeVersionCheck.HasVersion( neoStore, "V1" );

			  // then
			  assertTrue( result.Outcome.Successful );
			  assertEquals( StoreVersionCheck.Result.Outcome.Ok, result.Outcome );
			  assertNull( result.ActualVersion );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File emptyFile(org.neo4j.io.fs.FileSystemAbstraction fs) throws java.io.IOException
		 private File EmptyFile( FileSystemAbstraction fs )
		 {
			  File shortFile = _directory.file( "empty" );
			  fs.DeleteFile( shortFile );
			  fs.Create( shortFile ).close();
			  return shortFile;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File fileContaining(org.neo4j.io.fs.FileSystemAbstraction fs, String content) throws java.io.IOException
		 private File FileContaining( FileSystemAbstraction fs, string content )
		 {
			  File shortFile = _directory.file( "file" );
			  fs.DeleteFile( shortFile );
			  using ( Stream outputStream = fs.OpenAsOutputStream( shortFile, false ) )
			  {
					outputStream.Write( UTF8.encode( content ), 0, UTF8.encode( content ).Length );
					return shortFile;
			  }
		 }
	}

}