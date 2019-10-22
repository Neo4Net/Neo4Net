using System;

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
namespace Neo4Net.Kernel.Internal.locker
{

	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using StoreLayout = Neo4Net.Io.layout.StoreLayout;

	/// <summary>
	/// The class takes a lock on store described by provided instance of <seealso cref="StoreLayout"/>. The lock is valid after a successful call to
	/// <seealso cref="checkLock()"/> until a call to <seealso cref="close()"/>.
	/// </summary>
	public class StoreLocker : System.IDisposable
	{
		 internal readonly FileSystemAbstraction FileSystemAbstraction;
		 internal readonly File StoreLockFile;

		 internal FileLock StoreLockFileLock;
		 private StoreChannel _storeLockFileChannel;

		 public StoreLocker( FileSystemAbstraction fileSystemAbstraction, StoreLayout storeLayout )
		 {
			  this.FileSystemAbstraction = fileSystemAbstraction;
			  StoreLockFile = storeLayout.StoreLockFile();
		 }

		 /// <summary>
		 /// Obtains lock on store file so that we can ensure the store is not shared between database instances
		 /// <para>
		 /// Creates store dir if necessary, creates store lock file if necessary
		 /// </para>
		 /// <para>
		 /// Please note that this lock is only valid for as long the <seealso cref="storeLockFileChannel"/> lives, so make sure the
		 /// lock cannot be garbage collected as long as the lock should be valid.
		 /// 
		 /// </para>
		 /// </summary>
		 /// <exception cref="StoreLockException"> if lock could not be acquired </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void checkLock() throws org.Neo4Net.kernel.StoreLockException
		 public virtual void CheckLock()
		 {
			  if ( HaveLockAlready() )
			  {
					return;
			  }

			  try
			  {
					if ( !FileSystemAbstraction.fileExists( StoreLockFile ) )
					{
						 FileSystemAbstraction.mkdirs( StoreLockFile.ParentFile );
					}
			  }
			  catch ( IOException e )
			  {
					string message = "Unable to create path for store dir: " + StoreLockFile.Parent;
					throw StoreLockException( message, e );
			  }

			  try
			  {
					if ( _storeLockFileChannel == null )
					{
						 _storeLockFileChannel = FileSystemAbstraction.open( StoreLockFile, OpenMode.READ_WRITE );
					}
					StoreLockFileLock = _storeLockFileChannel.tryLock();
					if ( StoreLockFileLock == null )
					{
						 string message = "Store and its lock file has been locked by another process: " + StoreLockFile;
						 throw StoreLockException( message, null );
					}
			  }
			  catch ( Exception e ) when ( e is OverlappingFileLockException || e is IOException )
			  {
					throw UnableToObtainLockException();
			  }
		 }

		 protected internal virtual bool HaveLockAlready()
		 {
			  return StoreLockFileLock != null && _storeLockFileChannel != null;
		 }

		 internal virtual StoreLockException UnableToObtainLockException()
		 {
			  string message = "Unable to obtain lock on store lock file: " + StoreLockFile;
			  return StoreLockException( message, null );
		 }

		 private static StoreLockException StoreLockException( string message, Exception e )
		 {
			  string help = "Please ensure no other process is using this database, and that the directory is writable " +
						 "(required even for read-only access)";
			  return new StoreLockException( message + ". " + help, e );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  if ( StoreLockFileLock != null )
			  {
					ReleaseLock();
			  }
			  if ( _storeLockFileChannel != null )
			  {
					ReleaseChannel();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void releaseChannel() throws java.io.IOException
		 private void ReleaseChannel()
		 {
			  _storeLockFileChannel.close();
			  _storeLockFileChannel = null;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: protected void releaseLock() throws java.io.IOException
		 protected internal virtual void ReleaseLock()
		 {
			  StoreLockFileLock.release();
			  StoreLockFileLock = null;
		 }
	}

}