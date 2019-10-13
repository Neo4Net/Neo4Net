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
namespace Neo4Net.CommandLine.dbms
{

	using IOUtils = Neo4Net.Io.IOUtils;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using StoreLayout = Neo4Net.Io.layout.StoreLayout;
	using StoreLockException = Neo4Net.Kernel.StoreLockException;
	using GlobalStoreLocker = Neo4Net.Kernel.@internal.locker.GlobalStoreLocker;
	using StoreLocker = Neo4Net.Kernel.@internal.locker.StoreLocker;

	internal class StoreLockChecker : System.IDisposable
	{

		 private readonly FileSystemAbstraction _fileSystem;
		 private readonly StoreLocker _storeLocker;

		 private StoreLockChecker( FileSystemAbstraction fileSystem, StoreLayout storeLayout )
		 {
			  this._fileSystem = fileSystem;
			  this._storeLocker = new GlobalStoreLocker( fileSystem, storeLayout );
		 }

		 /// <summary>
		 /// Create store lock checker with lock on a provided store layout if it exists and writable </summary>
		 /// <param name="storeLayout"> store layout to check </param>
		 /// <returns> lock checker or empty closeable in case if path does not exists or is not writable </returns>
		 /// <exception cref="CannotWriteException">
		 /// </exception>
		 /// <seealso cref= StoreLocker </seealso>
		 /// <seealso cref= Files </seealso>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: static java.io.Closeable check(org.neo4j.io.layout.StoreLayout storeLayout) throws CannotWriteException
		 internal static System.IDisposable Check( StoreLayout storeLayout )
		 {
			  Path lockFile = storeLayout.StoreLockFile().toPath();
			  if ( Files.exists( lockFile ) )
			  {
					if ( Files.isWritable( lockFile ) )
					{
						 StoreLockChecker storeLocker = new StoreLockChecker( new DefaultFileSystemAbstraction(), storeLayout );
						 try
						 {
							  storeLocker.CheckLock();
							  return storeLocker;
						 }
						 catch ( StoreLockException le )
						 {
							  try
							  {
									storeLocker.Dispose();
							  }
							  catch ( IOException e )
							  {
									le.addSuppressed( e );
							  }
							  throw le;
						 }
					}
					else
					{
						 throw new CannotWriteException( lockFile );
					}
			  }
			  return () =>
			  {
			  };
		 }

		 private void CheckLock()
		 {
			  _storeLocker.checkLock();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  IOUtils.closeAll( _storeLocker, _fileSystem );
		 }
	}

}