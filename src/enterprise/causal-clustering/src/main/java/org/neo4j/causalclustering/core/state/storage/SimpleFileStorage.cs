/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.causalclustering.core.state.storage
{

	using EndOfStreamException = Neo4Net.causalclustering.messaging.EndOfStreamException;
	using Neo4Net.causalclustering.messaging.marshalling;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using FlushableChannel = Neo4Net.Kernel.impl.transaction.log.FlushableChannel;
	using PhysicalFlushableChannel = Neo4Net.Kernel.impl.transaction.log.PhysicalFlushableChannel;
	using Neo4Net.Kernel.impl.transaction.log;
	using ReadableClosableChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosableChannel;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	public class SimpleFileStorage<T> : SimpleStorage<T>
	{
		 private readonly FileSystemAbstraction _fileSystem;
		 private readonly ChannelMarshal<T> _marshal;
		 private readonly File _file;
		 private Log _log;

		 public SimpleFileStorage( FileSystemAbstraction fileSystem, File directory, string name, ChannelMarshal<T> marshal, LogProvider logProvider )
		 {
			  this._fileSystem = fileSystem;
			  this._log = logProvider.getLog( this.GetType() );
			  this._file = new File( DurableStateStorage.StateDir( directory, name ), name );
			  this._marshal = marshal;
		 }

		 public override bool Exists()
		 {
			  return _fileSystem.fileExists( _file );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public T readState() throws java.io.IOException
		 public override T ReadState()
		 {
			  try
			  {
					  using ( ReadableClosableChannel channel = new ReadAheadChannel<>( _fileSystem.open( _file, OpenMode.READ ) ) )
					  {
						return _marshal.unmarshal( channel );
					  }
			  }
			  catch ( EndOfStreamException e )
			  {
					_log.error( "End of stream reached: " + _file );
					throw new IOException( e );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void writeState(T state) throws java.io.IOException
		 public override void WriteState( T state )
		 {
			  _fileSystem.mkdirs( _file.ParentFile );
			  _fileSystem.deleteFile( _file );

			  using ( FlushableChannel channel = new PhysicalFlushableChannel( _fileSystem.create( _file ) ) )
			  {
					_marshal.marshal( state, channel );
			  }
		 }
	}

}