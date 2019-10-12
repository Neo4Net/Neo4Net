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
namespace Org.Neo4j.Kernel.impl.index
{

	using FileSystemAbstraction = Org.Neo4j.Io.fs.FileSystemAbstraction;
	using OpenMode = Org.Neo4j.Io.fs.OpenMode;
	using StoreChannel = Org.Neo4j.Io.fs.StoreChannel;
	using NotCurrentStoreVersionException = Org.Neo4j.Kernel.impl.store.NotCurrentStoreVersionException;
	using UpgradeNotAllowedByConfigurationException = Org.Neo4j.Kernel.impl.storemigration.UpgradeNotAllowedByConfigurationException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.store.MetaDataStore.versionLongToString;

	public class IndexProviderStore
	{
		 private const int RECORD_SIZE = 8;
		 private const int RECORD_COUNT = 5;

		 private readonly long _creationTime;
		 private readonly long _randomIdentifier;
		 private volatile long _version;
		 private readonly long _indexVersion;

		 private readonly StoreChannel _fileChannel;
		 private readonly ByteBuffer _buf = ByteBuffer.allocate( RECORD_SIZE * RECORD_COUNT );
		 private volatile long _lastCommittedTx;
		 private readonly File _file;
		 private readonly Random _random;

		 public IndexProviderStore( File file, FileSystemAbstraction fileSystem, long expectedVersion, bool allowUpgrade )
		 {
			  this._file = file;
			  this._random = new Random( DateTimeHelper.CurrentUnixTimeMillis() );
			  StoreChannel channel = null;
			  bool success = false;
			  try
			  {
					// Create it if it doesn't exist
					if ( !fileSystem.FileExists( file ) || fileSystem.GetFileSize( file ) == 0 )
					{
						 Create( file, fileSystem, expectedVersion );
					}

					// Read all the records in the file
					channel = fileSystem.Open( file, OpenMode.READ_WRITE );
					long?[] records = ReadRecordsWithNullDefaults( channel, RECORD_COUNT, allowUpgrade );
					_creationTime = records[0].Value;
					_randomIdentifier = records[1].Value;
					_version = records[2].Value;
					_lastCommittedTx = records[3].Value;
					long? readIndexVersion = records[4];
					_fileChannel = channel;

					// Compare version and throw exception if there's a mismatch, also considering "allow upgrade"
					bool versionDiffers = CompareExpectedVersionWithStoreVersion( expectedVersion, allowUpgrade, readIndexVersion );

					// Here we know that either the version matches or we just upgraded to the expected version
					_indexVersion = expectedVersion;
					if ( versionDiffers )
					{
					// We have upgraded the version, let's write it
						 WriteOut();
					}
					success = true;
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
			  finally
			  {
					if ( !success && channel != null )
					{
						 try
						 {
							  channel.close();
						 }
						 catch ( IOException )
						 { // What to do?
						 }
					}
			  }
		 }

		 private bool CompareExpectedVersionWithStoreVersion( long expectedVersion, bool allowUpgrade, long? readIndexVersion )
		 {
			  bool versionDiffers = readIndexVersion == null || readIndexVersion.Value != expectedVersion;
			  if ( versionDiffers )
			  {
					// We can throw a more explicit exception if we see that we're trying to run
					// with an older version than the store is.
					if ( readIndexVersion != null && expectedVersion < readIndexVersion.Value )
					{
						 string expected = versionLongToString( expectedVersion );
						 string readVersion = versionLongToString( readIndexVersion );
						 throw new NotCurrentStoreVersionException( expected, readVersion, "Your index has been upgraded to " + readVersion + " and cannot run with an older version " + expected, false );
					}
					else if ( !allowUpgrade )
					{
						 // We try to run with a newer version than the store is but isn't allowed to upgrade.
						 throw new UpgradeNotAllowedByConfigurationException();
					}
			  }
			  return versionDiffers;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private System.Nullable<long>[] readRecordsWithNullDefaults(org.neo4j.io.fs.StoreChannel fileChannel, int count, boolean allowUpgrade) throws java.io.IOException
		 private long?[] ReadRecordsWithNullDefaults( StoreChannel fileChannel, int count, bool allowUpgrade )
		 {
			  _buf.clear();
			  int bytesRead = fileChannel.read( _buf );
			  int wholeRecordsRead = bytesRead / RECORD_SIZE;
			  if ( wholeRecordsRead < RECORD_COUNT && !allowUpgrade )
			  {
					throw new UpgradeNotAllowedByConfigurationException( "Index version (managed by " + _file + ") has changed and needs to be upgraded" );
			  }

			  _buf.flip();
			  long?[] result = new long?[count];
			  for ( int i = 0; i < wholeRecordsRead; i++ )
			  {
					result[i] = _buf.Long;
			  }
			  return result;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void create(java.io.File file, org.neo4j.io.fs.FileSystemAbstraction fileSystem, long indexVersion) throws java.io.IOException
		 private void Create( File file, FileSystemAbstraction fileSystem, long indexVersion )
		 {
			  if ( fileSystem.FileExists( file ) && fileSystem.GetFileSize( file ) > 0 )
			  {
					throw new System.ArgumentException( file + " already exist" );
			  }

			  using ( StoreChannel fileChannel = fileSystem.Open( file, OpenMode.READ_WRITE ) )
			  {
					Write( fileChannel, DateTimeHelper.CurrentUnixTimeMillis(), _random.nextLong(), 0, 1, indexVersion );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void write(org.neo4j.io.fs.StoreChannel channel, long time, long identifier, long version, long lastCommittedTxId, long indexVersion) throws java.io.IOException
		 private void Write( StoreChannel channel, long time, long identifier, long version, long lastCommittedTxId, long indexVersion )
		 {
			  _buf.clear();
			  _buf.putLong( time ).putLong( identifier ).putLong( version ).putLong( lastCommittedTxId ).putLong( indexVersion );
			  _buf.flip();

			  channel.WriteAll( _buf, 0 );
			  channel.Force( true );
		 }

		 public virtual File File
		 {
			 get
			 {
				  return _file;
			 }
		 }

		 public virtual long CreationTime
		 {
			 get
			 {
				  return _creationTime;
			 }
		 }

		 public virtual long Version
		 {
			 get
			 {
				  return _version;
			 }
			 set
			 {
				 lock ( this )
				 {
					  this._version = value;
					  WriteOut();
				 }
			 }
		 }

		 public virtual long IndexVersion
		 {
			 get
			 {
				  return _indexVersion;
			 }
		 }


		 public virtual long LastCommittedTx
		 {
			 set
			 {
				 lock ( this )
				 {
					  this._lastCommittedTx = value;
				 }
			 }
			 get
			 {
				  return this._lastCommittedTx;
			 }
		 }


		 private void WriteOut()
		 {
			  try
			  {
					Write( _fileChannel, _creationTime, _randomIdentifier, _version, _lastCommittedTx, _indexVersion );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }

		 public virtual void Close()
		 {
			  if ( !_fileChannel.Open )
			  {
					return;
			  }

			  WriteOut();
			  try
			  {
					_fileChannel.close();
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
		 }
	}

}