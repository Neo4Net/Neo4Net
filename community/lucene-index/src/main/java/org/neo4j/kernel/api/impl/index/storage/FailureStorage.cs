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
namespace Org.Neo4j.Kernel.Api.Impl.Index.storage
{

	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using OpenMode = Org.Neo4j.Io.fs.OpenMode;
	using StoreChannel = Org.Neo4j.Io.fs.StoreChannel;
	using FolderLayout = Org.Neo4j.Kernel.Api.Impl.Index.storage.layout.FolderLayout;
	using UTF8 = Org.Neo4j.@string.UTF8;

	/// <summary>
	/// Helper class for storing a failure message that happens during an OutOfDisk situation in
	/// a pre-allocated file
	/// </summary>
	public class FailureStorage
	{
		 private const int MAX_FAILURE_SIZE = 16384;
		 public const string DEFAULT_FAILURE_FILE_NAME = "failure-message";

		 private readonly FileSystemAbstraction _fs;
		 private readonly FolderLayout _folderLayout;
		 private readonly string _failureFileName;

		 /// <param name="failureFileName"> name of failure files to be created </param>
		 /// <param name="folderLayout"> describing where failure files should be stored </param>
		 public FailureStorage( FileSystemAbstraction fs, FolderLayout folderLayout, string failureFileName )
		 {
			  this._fs = fs;
			  this._folderLayout = folderLayout;
			  this._failureFileName = failureFileName;
		 }

		 public FailureStorage( FileSystemAbstraction fs, FolderLayout folderLayout ) : this( fs, folderLayout, DEFAULT_FAILURE_FILE_NAME )
		 {
		 }

		 /// <summary>
		 /// Create/reserve an empty failure file for the given indexId.
		 /// 
		 /// This will overwrite any pre-existing failure file.
		 /// </summary>
		 /// <exception cref="IOException"> if the failure file could not be created </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void reserveForIndex() throws java.io.IOException
		 public virtual void ReserveForIndex()
		 {
			 lock ( this )
			 {
				  _fs.mkdirs( _folderLayout.IndexFolder );
				  File failureFile = failureFile();
				  using ( StoreChannel channel = _fs.create( failureFile ) )
				  {
						channel.WriteAll( ByteBuffer.wrap( new sbyte[MAX_FAILURE_SIZE] ) );
						channel.Force( true );
				  }
			 }
		 }

		 /// <summary>
		 /// Delete failure file for the given index id
		 /// 
		 /// </summary>
		 public virtual void ClearForIndex()
		 {
			 lock ( this )
			 {
				  _fs.deleteFile( FailureFile() );
			 }
		 }

		 /// <returns> the failure, if any. Otherwise {@code null} marking no failure. </returns>
		 public virtual string LoadIndexFailure()
		 {
			 lock ( this )
			 {
				  File failureFile = failureFile();
				  try
				  {
						if ( !_fs.fileExists( failureFile ) || !IsFailed( failureFile ) )
						{
							 return null;
						}
						return ReadFailure( failureFile );
				  }
				  catch ( IOException e )
				  {
						throw new Exception( e );
				  }
			 }
		 }

		 /// <summary>
		 /// Store failure in failure file for index with the given id
		 /// </summary>
		 /// <param name="failure"> message describing the failure that needs to be stored </param>
		 /// <exception cref="IOException"> if the failure could not be stored </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void storeIndexFailure(String failure) throws java.io.IOException
		 public virtual void StoreIndexFailure( string failure )
		 {
			 lock ( this )
			 {
				  File failureFile = failureFile();
				  using ( StoreChannel channel = _fs.open( failureFile, OpenMode.READ_WRITE ) )
				  {
						sbyte[] existingData = new sbyte[( int ) channel.size()];
						channel.ReadAll( ByteBuffer.wrap( existingData ) );
						channel.Position( LengthOf( existingData ) );
      
						sbyte[] data = UTF8.encode( failure );
						channel.WriteAll( ByteBuffer.wrap( data, 0, Math.Min( data.Length, MAX_FAILURE_SIZE ) ) );
      
						channel.Force( true );
						channel.close();
				  }
			 }
		 }

		 internal virtual File FailureFile()
		 {
			  File folder = _folderLayout.IndexFolder;
			  return new File( folder, _failureFileName );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String readFailure(java.io.File failureFile) throws java.io.IOException
		 private string ReadFailure( File failureFile )
		 {
			  using ( StoreChannel channel = _fs.open( failureFile, OpenMode.READ ) )
			  {
					sbyte[] data = new sbyte[( int ) channel.size()];
					channel.ReadAll( ByteBuffer.wrap( data ) );
					return UTF8.decode( WithoutZeros( data ) );
			  }
		 }

		 private static sbyte[] WithoutZeros( sbyte[] data )
		 {
			  sbyte[] result = new sbyte[LengthOf( data )];
			  Array.Copy( data, 0, result, 0, result.Length );
			  return result;
		 }

		 private static int LengthOf( sbyte[] data )
		 {
			  for ( int i = 0; i < data.Length; i++ )
			  {
					if ( 0 == data[i] )
					{
						 return i;
					}
			  }
			  return data.Length;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private boolean isFailed(java.io.File failureFile) throws java.io.IOException
		 private bool IsFailed( File failureFile )
		 {
			  using ( StoreChannel channel = _fs.open( failureFile, OpenMode.READ ) )
			  {
					sbyte[] data = new sbyte[( int ) channel.size()];
					channel.ReadAll( ByteBuffer.wrap( data ) );
					channel.close();
					return !AllZero( data );
			  }
		 }

		 private static bool AllZero( sbyte[] data )
		 {
			  foreach ( sbyte b in data )
			  {
					if ( b != 0 )
					{
						 return false;
					}
			  }
			  return true;
		 }
	}

}