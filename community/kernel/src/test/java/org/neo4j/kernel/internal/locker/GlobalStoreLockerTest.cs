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
namespace Org.Neo4j.Kernel.@internal.locker
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using StoreLayout = Org.Neo4j.Io.layout.StoreLayout;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;
	using Org.Neo4j.Test.rule.fs;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class GlobalStoreLockerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.FileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public readonly FileSystemRule FileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failToLockSameFolderAcrossIndependentLockers() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailToLockSameFolderAcrossIndependentLockers()
		 {
			  StoreLayout storeLayout = TestDirectory.storeLayout();
			  using ( GlobalStoreLocker storeLocker = new GlobalStoreLocker( FileSystemRule.get(), storeLayout ) )
			  {
					storeLocker.CheckLock();

					try
					{
							using ( GlobalStoreLocker locker = new GlobalStoreLocker( FileSystemRule.get(), storeLayout ) )
							{
							 locker.CheckLock();
							 fail( "directory should be locked" );
							}
					}
					catch ( StoreLockException )
					{
						 // expected
					}

					try
					{
							using ( GlobalStoreLocker locker = new GlobalStoreLocker( FileSystemRule.get(), storeLayout ) )
							{
							 locker.CheckLock();
							 fail( "directory should be locked" );
							}
					}
					catch ( StoreLockException )
					{
						 // expected
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allowToLockSameDirectoryIfItWasUnlocked() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AllowToLockSameDirectoryIfItWasUnlocked()
		 {
			  StoreLayout storeLayout = TestDirectory.storeLayout();
			  using ( GlobalStoreLocker storeLocker = new GlobalStoreLocker( FileSystemRule.get(), storeLayout ) )
			  {
					storeLocker.CheckLock();
			  }
			  using ( GlobalStoreLocker storeLocker = new GlobalStoreLocker( FileSystemRule.get(), storeLayout ) )
			  {
					storeLocker.CheckLock();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void allowMultipleCallstoActuallyStoreLocker() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AllowMultipleCallstoActuallyStoreLocker()
		 {
			  StoreLayout storeLayout = TestDirectory.storeLayout();
			  using ( GlobalStoreLocker storeLocker = new GlobalStoreLocker( FileSystemRule.get(), storeLayout ) )
			  {
					storeLocker.CheckLock();
					storeLocker.CheckLock();
					storeLocker.CheckLock();
					storeLocker.CheckLock();
					storeLocker.CheckLock();
			  }
		 }
	}

}