using System;

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
namespace Org.Neo4j.Kernel.@internal
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using Mockito = org.mockito.Mockito;


	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using DelegatingFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.DelegatingFileSystemAbstraction;
	using DelegatingStoreChannel = Org.Neo4j.Graphdb.mockfs.DelegatingStoreChannel;
	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using OpenMode = Org.Neo4j.Io.fs.OpenMode;
	using StoreChannel = Org.Neo4j.Io.fs.StoreChannel;
	using StoreLayout = Org.Neo4j.Io.layout.StoreLayout;
	using StoreLocker = Org.Neo4j.Kernel.@internal.locker.StoreLocker;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using DefaultFileSystemRule = Org.Neo4j.Test.rule.fs.DefaultFileSystemRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.Is.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class StoreLockerTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory target = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Target = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.DefaultFileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.DefaultFileSystemRule();
		 public readonly DefaultFileSystemRule FileSystemRule = new DefaultFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldUseAlreadyOpenedFileChannel() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldUseAlreadyOpenedFileChannel()
		 {
			  StoreChannel channel = Mockito.mock( typeof( StoreChannel ) );
			  CustomChannelFileSystemAbstraction fileSystemAbstraction = new CustomChannelFileSystemAbstraction( this, FileSystemRule.get(), channel );
			  int numberOfCallesToOpen = 0;
			  try
			  {
					  using ( StoreLocker storeLocker = new StoreLocker( fileSystemAbstraction, Target.storeLayout() ) )
					  {
						try
						{
							 storeLocker.CheckLock();
							 fail();
						}
						catch ( StoreLockException )
						{
							 numberOfCallesToOpen = fileSystemAbstraction.NumberOfCallsToOpen;
      
							 // Try to grab lock a second time
							 storeLocker.CheckLock();
						}
					  }
			  }
			  catch ( StoreLockException )
			  {
					// expected
			  }

			  assertEquals( "Expect that number of open channels will remain the same for ", numberOfCallesToOpen, fileSystemAbstraction.NumberOfCallsToOpen );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowMultipleCallsToCheckLock() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAllowMultipleCallsToCheckLock()
		 {
			  using ( StoreLocker storeLocker = new StoreLocker( FileSystemRule.get(), Target.storeLayout() ) )
			  {
					storeLocker.CheckLock();
					storeLocker.CheckLock();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void keepLockWhenOtherTryToTakeLock() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void KeepLockWhenOtherTryToTakeLock()
		 {
			  StoreLayout storeLayout = Target.storeLayout();
			  DefaultFileSystemAbstraction fileSystemAbstraction = FileSystemRule.get();
			  StoreLocker storeLocker = new StoreLocker( fileSystemAbstraction, storeLayout );
			  storeLocker.CheckLock();

			  try
			  {
					  using ( StoreLocker storeLocker1 = new StoreLocker( fileSystemAbstraction, storeLayout ) )
					  {
						storeLocker1.CheckLock();
						fail();
					  }
			  }
			  catch ( StoreLockException )
			  {
					// Expected
			  }

			  // Initial locker should still have a valid lock
			  try
			  {
					  using ( StoreLocker storeLocker1 = new StoreLocker( fileSystemAbstraction, storeLayout ) )
					  {
						storeLocker1.CheckLock();
						fail();
					  }
			  }
			  catch ( StoreLockException )
			  {
					// Expected
			  }

			  storeLocker.Dispose();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldObtainLockWhenStoreFileNotLocked() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldObtainLockWhenStoreFileNotLocked()
		 {
			  FileSystemAbstraction fileSystemAbstraction = new DelegatingFileSystemAbstractionAnonymousInnerClass( this, FileSystemRule.get() );

			  try
			  {
					  using ( StoreLocker storeLocker = new StoreLocker( fileSystemAbstraction, Target.storeLayout() ) )
					  {
						storeLocker.CheckLock();
      
						// Ok
					  }
			  }
			  catch ( StoreLockException )
			  {
					fail();
			  }
		 }

		 private class DelegatingFileSystemAbstractionAnonymousInnerClass : DelegatingFileSystemAbstraction
		 {
			 private readonly StoreLockerTest _outerInstance;

			 public DelegatingFileSystemAbstractionAnonymousInnerClass( StoreLockerTest outerInstance, UnknownType get ) : base( get )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override bool fileExists( File file )
			 {
				  return true;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateStoreDirAndObtainLockWhenStoreDirDoesNotExist() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateStoreDirAndObtainLockWhenStoreDirDoesNotExist()
		 {
			  FileSystemAbstraction fileSystemAbstraction = new DelegatingFileSystemAbstractionAnonymousInnerClass2( this, FileSystemRule.get() );

			  using ( StoreLocker storeLocker = new StoreLocker( fileSystemAbstraction, Target.storeLayout() ) )
			  {
					storeLocker.CheckLock();
					// Ok
			  }
		 }

		 private class DelegatingFileSystemAbstractionAnonymousInnerClass2 : DelegatingFileSystemAbstraction
		 {
			 private readonly StoreLockerTest _outerInstance;

			 public DelegatingFileSystemAbstractionAnonymousInnerClass2( StoreLockerTest outerInstance, UnknownType get ) : base( get )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override bool fileExists( File file )
			 {
				  return false;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotObtainLockWhenStoreDirCannotBeCreated() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotObtainLockWhenStoreDirCannotBeCreated()
		 {
			  FileSystemAbstraction fileSystemAbstraction = new DelegatingFileSystemAbstractionAnonymousInnerClass3( this, FileSystemRule.get() );

			  StoreLayout storeLayout = Target.storeLayout();
			  try
			  {
					  using ( StoreLocker storeLocker = new StoreLocker( fileSystemAbstraction, storeLayout ) )
					  {
						storeLocker.CheckLock();
						fail();
					  }
			  }
			  catch ( StoreLockException e )
			  {
					string msg = format( "Unable to create path for store dir: %s. " + "Please ensure no other process is using this database, and that " + "the directory is writable (required even for read-only access)", storeLayout );
					assertThat( e.Message, @is( msg ) );
			  }
		 }

		 private class DelegatingFileSystemAbstractionAnonymousInnerClass3 : DelegatingFileSystemAbstraction
		 {
			 private readonly StoreLockerTest _outerInstance;

			 public DelegatingFileSystemAbstractionAnonymousInnerClass3( StoreLockerTest outerInstance, UnknownType get ) : base( get )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void mkdirs(java.io.File fileName) throws java.io.IOException
			 public override void mkdirs( File fileName )
			 {
				  throw new IOException( "store dir could not be created" );
			 }

			 public override bool fileExists( File file )
			 {
				  return false;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotObtainLockWhenUnableToOpenLockFile() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotObtainLockWhenUnableToOpenLockFile()
		 {
			  FileSystemAbstraction fileSystemAbstraction = new DelegatingFileSystemAbstractionAnonymousInnerClass4( this, FileSystemRule.get() );

			  StoreLayout storeLayout = Target.storeLayout();

			  try
			  {
					  using ( StoreLocker storeLocker = new StoreLocker( fileSystemAbstraction, storeLayout ) )
					  {
						storeLocker.CheckLock();
						fail();
					  }
			  }
			  catch ( StoreLockException e )
			  {
					string msg = format( "Unable to obtain lock on store lock file: %s. " + "Please ensure no other process is using this database, and that the " + "directory is writable (required even for read-only access)", storeLayout.StoreLockFile() );
					assertThat( e.Message, @is( msg ) );
			  }
		 }

		 private class DelegatingFileSystemAbstractionAnonymousInnerClass4 : DelegatingFileSystemAbstraction
		 {
			 private readonly StoreLockerTest _outerInstance;

			 public DelegatingFileSystemAbstractionAnonymousInnerClass4( StoreLockerTest outerInstance, UnknownType get ) : base( get )
			 {
				 this.outerInstance = outerInstance;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.fs.StoreChannel open(java.io.File fileName, org.neo4j.io.fs.OpenMode openMode) throws java.io.IOException
			 public override StoreChannel open( File fileName, OpenMode openMode )
			 {
				  throw new IOException( "cannot open lock file" );
			 }

			 public override bool fileExists( File file )
			 {
				  return false;
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotObtainLockWhenStoreAlreadyInUse() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotObtainLockWhenStoreAlreadyInUse()
		 {
			  FileSystemAbstraction fileSystemAbstraction = new DelegatingFileSystemAbstractionAnonymousInnerClass5( this, FileSystemRule.get() );

			  try
			  {
					  using ( StoreLocker storeLocker = new StoreLocker( fileSystemAbstraction, Target.storeLayout() ) )
					  {
						storeLocker.CheckLock();
						fail();
					  }
			  }
			  catch ( StoreLockException e )
			  {
					assertThat( e.Message, containsString( "Store and its lock file has been locked by another process" ) );
			  }
		 }

		 private class DelegatingFileSystemAbstractionAnonymousInnerClass5 : DelegatingFileSystemAbstraction
		 {
			 private readonly StoreLockerTest _outerInstance;

			 public DelegatingFileSystemAbstractionAnonymousInnerClass5( StoreLockerTest outerInstance, UnknownType get ) : base( get )
			 {
				 this.outerInstance = outerInstance;
			 }

			 public override bool fileExists( File file )
			 {
				  return false;
			 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public org.neo4j.io.fs.StoreChannel open(java.io.File fileName, org.neo4j.io.fs.OpenMode openMode) throws java.io.IOException
			 public override StoreChannel open( File fileName, OpenMode openMode )
			 {
				  return new DelegatingStoreChannelAnonymousInnerClass( this, base.open( fileName, openMode ) );
			 }

			 private class DelegatingStoreChannelAnonymousInnerClass : DelegatingStoreChannel
			 {
				 private readonly DelegatingFileSystemAbstractionAnonymousInnerClass5 _outerInstance;

				 public DelegatingStoreChannelAnonymousInnerClass( DelegatingFileSystemAbstractionAnonymousInnerClass5 outerInstance, UnknownType open ) : base( open )
				 {
					 this.outerInstance = outerInstance;
				 }

				 public override FileLock tryLock()
				 {
					  return null; // 'null' implies that the file has been externally locked
				 }
			 }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustPreventMultipleInstancesFromStartingOnSameStore()
		 public virtual void MustPreventMultipleInstancesFromStartingOnSameStore()
		 {
			  File storeDir = Target.storeDir();
			  GraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(storeDir);
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode();
					tx.Success();
			  }

			  try
			  {
					( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(storeDir);
					fail( "Should not be able to start up another db in the same dir" );
			  }
			  catch ( Exception )
			  {
					// Good
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }

		 private class CustomChannelFileSystemAbstraction : DelegatingFileSystemAbstraction
		 {
			 private readonly StoreLockerTest _outerInstance;

			  internal readonly StoreChannel Channel;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal int NumberOfCallsToOpenConflict;

			  internal CustomChannelFileSystemAbstraction( StoreLockerTest outerInstance, DefaultFileSystemAbstraction @delegate, StoreChannel channel ) : base( @delegate )
			  {
				  this._outerInstance = outerInstance;
					this.Channel = channel;
			  }

			  public override StoreChannel Open( File fileName, OpenMode openMode )
			  {
					NumberOfCallsToOpenConflict++;
					return Channel;
			  }

			  public virtual int NumberOfCallsToOpen
			  {
				  get
				  {
						return NumberOfCallsToOpenConflict;
				  }
			  }
		 }
	}

}