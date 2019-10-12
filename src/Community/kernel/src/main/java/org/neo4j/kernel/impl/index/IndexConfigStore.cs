using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.index
{

	using Node = Neo4Net.Graphdb.Node;
	using PropertyContainer = Neo4Net.Graphdb.PropertyContainer;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using StoreChannel = Neo4Net.Io.fs.StoreChannel;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using IoPrimitiveUtils = Neo4Net.Kernel.impl.util.IoPrimitiveUtils;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;

	public class IndexConfigStore : LifecycleAdapter
	{
		 public const string INDEX_DB_FILE_NAME = "index.db";
		 private static readonly string _oldIndexDbFileName = INDEX_DB_FILE_NAME + ".old";
		 private static readonly string _tmpIndexDbFileName = INDEX_DB_FILE_NAME + ".tmp";

		 private static readonly sbyte[] _magic = new sbyte[] { ( sbyte )'n', ( sbyte )'e', ( sbyte )'o', ( sbyte )'4', ( sbyte )'j', ( sbyte )'-', ( sbyte )'i', ( sbyte )'n', ( sbyte )'d', ( sbyte )'e', ( sbyte )'x' };
		 private const int VERSION = 1;

		 private readonly File _file;
		 private readonly File _oldFile;
		 private readonly IDictionary<string, IDictionary<string, string>> _nodeConfig = new ConcurrentDictionary<string, IDictionary<string, string>>();
		 private readonly IDictionary<string, IDictionary<string, string>> _relConfig = new ConcurrentDictionary<string, IDictionary<string, string>>();
		 private readonly DatabaseLayout _dbDirectoryStructure;
		 private readonly FileSystemAbstraction _fileSystem;
		 private ByteBuffer _dontUseBuffer = ByteBuffer.allocate( 100 );

		 public IndexConfigStore( DatabaseLayout dbDirectoryStructure, FileSystemAbstraction fileSystem )
		 {
			  this._dbDirectoryStructure = dbDirectoryStructure;
			  this._fileSystem = fileSystem;
			  this._file = dbDirectoryStructure.File( INDEX_DB_FILE_NAME );
			  this._oldFile = dbDirectoryStructure.File( _oldIndexDbFileName );
		 }

		 private ByteBuffer Buffer( int size )
		 {
			  if ( _dontUseBuffer.capacity() < size )
			  {
					_dontUseBuffer = ByteBuffer.allocate( size * 2 );
			  }
			  return _dontUseBuffer;
		 }

		 private void Read()
		 {
			  File fileToReadFrom = _fileSystem.fileExists( _file ) ? _file : _oldFile;
			  if ( !_fileSystem.fileExists( fileToReadFrom ) )
			  {
					return;
			  }

			  StoreChannel channel = null;
			  try
			  {
					channel = _fileSystem.open( fileToReadFrom, OpenMode.READ );
					int? version = TryToReadVersion( channel );
					if ( version == null )
					{
						 Close( channel );
						 channel = _fileSystem.open( fileToReadFrom, OpenMode.READ );
						 // Legacy format, TODO
						 ReadMap( channel, _nodeConfig, null );
//JAVA TO C# CONVERTER TODO TASK: There is no .NET Dictionary equivalent to the Java 'putAll' method:
						 _relConfig.putAll( _nodeConfig );
					}
					else if ( version.Value < VERSION )
					{
						 // ...add version upgrade code here
						 throw new System.NotSupportedException( "" + version );
					}
					else
					{
						 ReadMap( channel, _nodeConfig, ReadNextInt( channel ) );
						 ReadMap( channel, _relConfig, ReadNextInt( channel ) );
					}
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
			  finally
			  {
					Close( channel );
			  }
		 }

		 public override void Init()
		 {
			  Read();
		 }

		 public override void Start()
		 {
			  // Refresh the read config
			  _nodeConfig.Clear();
			  _relConfig.Clear();
			  Read();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void readMap(org.neo4j.io.fs.StoreChannel channel, java.util.Map<String,java.util.Map<String,String>> map, System.Nullable<int> sizeOrTillEof) throws java.io.IOException
		 private void ReadMap( StoreChannel channel, IDictionary<string, IDictionary<string, string>> map, int? sizeOrTillEof )
		 {
			  for ( int i = 0; sizeOrTillEof == null || i < sizeOrTillEof.Value; i++ )
			  {
					string indexName = ReadNextString( channel );
					if ( string.ReferenceEquals( indexName, null ) )
					{
						 break;
					}
					int? propertyCount = ReadNextInt( channel );
					if ( propertyCount == null )
					{
						 break;
					}
					IDictionary<string, string> properties = new Dictionary<string, string>();
					for ( int p = 0; p < propertyCount.Value; p++ )
					{
						 string key = ReadNextString( channel );
						 if ( string.ReferenceEquals( key, null ) )
						 {
							  break;
						 }
						 string value = ReadNextString( channel );
						 if ( string.ReferenceEquals( value, null ) )
						 {
							  break;
						 }
						 properties[key] = value;
					}
					map[indexName] = properties;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private System.Nullable<int> tryToReadVersion(java.nio.channels.ReadableByteChannel channel) throws java.io.IOException
		 private int? TryToReadVersion( ReadableByteChannel channel )
		 {
			  sbyte[] array = IoPrimitiveUtils.readBytes( channel, new sbyte[_magic.Length] );
			  if ( !Arrays.Equals( _magic, array ) )
			  {
					return null;
			  }
			  return array != null ? ReadNextInt( channel ) : null;
		 }

		 private void Close( StoreChannel channel )
		 {
			  if ( channel != null )
			  {
					try
					{
						 channel.close();
					}
					catch ( IOException e )
					{
						 Console.WriteLine( e.ToString() );
						 Console.Write( e.StackTrace );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private System.Nullable<int> readNextInt(java.nio.channels.ReadableByteChannel channel) throws java.io.IOException
		 private int? ReadNextInt( ReadableByteChannel channel )
		 {
			  return IoPrimitiveUtils.readInt( channel, Buffer( 4 ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private String readNextString(java.nio.channels.ReadableByteChannel channel) throws java.io.IOException
		 private string ReadNextString( ReadableByteChannel channel )
		 {
			  return IoPrimitiveUtils.readLengthAndString( channel, Buffer( 100 ) );
		 }

		 public virtual bool Has( Type cls, string indexName )
		 {
			  return Map( cls ).ContainsKey( indexName );
		 }

		 public virtual IDictionary<string, string> Get( Type cls, string indexName )
		 {
			  return Map( cls )[indexName];
		 }

		 public virtual string[] GetNames( Type cls )
		 {
			  IDictionary<string, IDictionary<string, string>> indexMap = Map( cls );
			  return indexMap.Keys.toArray( new string[indexMap.Count] );
		 }

		 private IDictionary<string, IDictionary<string, string>> Map( Type cls )
		 {
			  if ( cls.Equals( typeof( Node ) ) )
			  {
					return _nodeConfig;
			  }
			  else if ( cls.Equals( typeof( Relationship ) ) )
			  {
					return _relConfig;
			  }
			  throw new System.ArgumentException( cls.ToString() );
		 }

		 // Synchronized since only one thread are allowed to write at any given time
		 public virtual void Remove( Type cls, string indexName )
		 {
			 lock ( this )
			 {
				  if ( Map( cls ).Remove( indexName ) == null )
				  {
						throw new Exception( "Index config for '" + indexName + "' not found" );
				  }
				  Write();
			 }
		 }

		 // Synchronized since only one thread are allowed to write at any given time
		 public virtual void Set( Type cls, string name, IDictionary<string, string> config )
		 {
			 lock ( this )
			 {
				  Map( cls )[name] = Collections.unmodifiableMap( config );
				  Write();
			 }
		 }

		 // Synchronized since only one thread are allowed to write at any given time
		 public virtual bool SetIfNecessary( Type cls, string name, IDictionary<string, string> config )
		 {
			 lock ( this )
			 {
				  IDictionary<string, IDictionary<string, string>> map = map( cls );
				  if ( map.ContainsKey( name ) )
				  {
						return false;
				  }
				  map[name] = Collections.unmodifiableMap( config );
				  Write();
				  return true;
			 }
		 }

		 private void Write()
		 {
			  // Write to a .tmp file
			  File tmpFile = _dbDirectoryStructure.file( _tmpIndexDbFileName );
			  Write( tmpFile );

			  // Make sure the .old file doesn't exist, then rename the current one to .old
			  _fileSystem.deleteFile( _oldFile );
			  try
			  {
					if ( _fileSystem.fileExists( _file ) )
					{
						 _fileSystem.renameFile( _file, _oldFile );
					}
			  }
			  catch ( IOException e )
			  {
					throw new Exception( "Couldn't rename " + _file + " -> " + _oldFile, e );
			  }

			  // Rename the .tmp file to the current name
			  try
			  {
					_fileSystem.renameFile( tmpFile, this._file );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( "Couldn't rename " + tmpFile + " -> " + _file, e );
			  }
			  _fileSystem.deleteFile( _oldFile );
		 }

		 private void Write( File file )
		 {
			  StoreChannel channel = null;
			  try
			  {

					channel = _fileSystem.open( file, OpenMode.READ_WRITE );
					channel.WriteAll( ByteBuffer.wrap( _magic ) );
					IoPrimitiveUtils.writeInt( channel, Buffer( 4 ), VERSION );
					WriteMap( channel, _nodeConfig );
					WriteMap( channel, _relConfig );
					channel.Force( false );
			  }
			  catch ( IOException e )
			  {
					throw new Exception( e );
			  }
			  finally
			  {
					Close( channel );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeMap(org.neo4j.io.fs.StoreChannel channel, java.util.Map<String, java.util.Map<String, String>> map) throws java.io.IOException
		 private void WriteMap( StoreChannel channel, IDictionary<string, IDictionary<string, string>> map )
		 {
			  IoPrimitiveUtils.writeInt( channel, Buffer( 4 ), map.Count );
			  foreach ( KeyValuePair<string, IDictionary<string, string>> entry in map.SetOfKeyValuePairs() )
			  {
					WriteString( channel, entry.Key );
					WriteInt( channel, entry.Value.size() );
					foreach ( KeyValuePair<string, string> propertyEntry in entry.Value.entrySet() )
					{
						 WriteString( channel, propertyEntry.Key );
						 WriteString( channel, propertyEntry.Value );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeInt(org.neo4j.io.fs.StoreChannel channel, int value) throws java.io.IOException
		 private void WriteInt( StoreChannel channel, int value )
		 {
			  IoPrimitiveUtils.writeInt( channel, Buffer( 4 ), value );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void writeString(org.neo4j.io.fs.StoreChannel channel, String value) throws java.io.IOException
		 private void WriteString( StoreChannel channel, string value )
		 {
			  IoPrimitiveUtils.writeLengthAndString( channel, Buffer( 200 ), value );
		 }
	}

}