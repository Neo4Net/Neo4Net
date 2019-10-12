using System.Diagnostics;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
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

	using Neo4Net.causalclustering.core.state;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using FlushableChannel = Neo4Net.Kernel.impl.transaction.log.FlushableChannel;
	using PhysicalFlushableChannel = Neo4Net.Kernel.impl.transaction.log.PhysicalFlushableChannel;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using Log = Neo4Net.Logging.Log;
	using LogProvider = Neo4Net.Logging.LogProvider;

	public class DurableStateStorage<STATE> : LifecycleAdapter, StateStorage<STATE>
	{
		 private readonly StateRecoveryManager<STATE> _recoveryManager;
		 private readonly Log _log;
		 private STATE _initialState;
		 private readonly File _fileA;
		 private readonly File _fileB;
		 private readonly FileSystemAbstraction _fsa;
		 private readonly string _name;
		 private readonly StateMarshal<STATE> _marshal;
		 private readonly int _numberOfEntriesBeforeRotation;

		 private int _numberOfEntriesWrittenInActiveFile;
		 private File _currentStoreFile;

		 private PhysicalFlushableChannel _currentStoreChannel;

		 public DurableStateStorage( FileSystemAbstraction fsa, File baseDir, string name, StateMarshal<STATE> marshal, int numberOfEntriesBeforeRotation, LogProvider logProvider )
		 {
			  this._fsa = fsa;
			  this._name = name;
			  this._marshal = marshal;
			  this._numberOfEntriesBeforeRotation = numberOfEntriesBeforeRotation;
			  this._log = logProvider.getLog( this.GetType() );
			  this._recoveryManager = new StateRecoveryManager<STATE>( fsa, marshal );
			  File parent = StateDir( baseDir, name );
			  this._fileA = new File( parent, name + ".a" );
			  this._fileB = new File( parent, name + ".b" );
		 }

		 public virtual bool Exists()
		 {
			  return _fsa.fileExists( _fileA ) && _fsa.fileExists( _fileB );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void create() throws java.io.IOException
		 private void Create()
		 {
			  EnsureExists( _fileA );
			  EnsureExists( _fileB );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void ensureExists(java.io.File file) throws java.io.IOException
		 private void EnsureExists( File file )
		 {
			  if ( !_fsa.fileExists( file ) )
			  {
					_fsa.mkdirs( file.ParentFile );
					using ( FlushableChannel channel = new PhysicalFlushableChannel( _fsa.create( file ) ) )
					{
						 _marshal.marshal( _marshal.startState(), channel );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void recover() throws java.io.IOException
		 private void Recover()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.neo4j.causalclustering.core.state.StateRecoveryManager.RecoveryStatus<STATE> recoveryStatus = recoveryManager.recover(fileA, fileB);
			  StateRecoveryManager.RecoveryStatus<STATE> recoveryStatus = _recoveryManager.recover( _fileA, _fileB );

			  this._currentStoreFile = recoveryStatus.ActiveFile();
			  this._currentStoreChannel = ResetStoreFile( _currentStoreFile );
			  this._initialState = recoveryStatus.RecoveredState();

			  _log.info( "%s state restored, up to ordinal %d", _name, _marshal.ordinal( _initialState ) );
		 }

		 public virtual STATE InitialState
		 {
			 get
			 {
				  Debug.Assert( _initialState != default( STATE ) );
				  return _initialState;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void init() throws java.io.IOException
		 public override void Init()
		 {
			  Create();
			  Recover();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void shutdown() throws java.io.IOException
		 public override void Shutdown()
		 {
			 lock ( this )
			 {
				  _currentStoreChannel.Dispose();
				  _currentStoreChannel = null;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public synchronized void persistStoreData(STATE state) throws java.io.IOException
		 public override void PersistStoreData( STATE state )
		 {
			 lock ( this )
			 {
				  if ( _numberOfEntriesWrittenInActiveFile >= _numberOfEntriesBeforeRotation )
				  {
						SwitchStoreFile();
						_numberOfEntriesWrittenInActiveFile = 0;
				  }
      
				  _marshal.marshal( state, _currentStoreChannel );
				  _currentStoreChannel.prepareForFlush().flush();
      
				  _numberOfEntriesWrittenInActiveFile++;
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void switchStoreFile() throws java.io.IOException
		 internal virtual void SwitchStoreFile()
		 {
			  _currentStoreChannel.Dispose();

			  if ( _currentStoreFile.Equals( _fileA ) )
			  {
					_currentStoreChannel = ResetStoreFile( _fileB );
					_currentStoreFile = _fileB;
			  }
			  else
			  {
					_currentStoreChannel = ResetStoreFile( _fileA );
					_currentStoreFile = _fileA;
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.kernel.impl.transaction.log.PhysicalFlushableChannel resetStoreFile(java.io.File nextStore) throws java.io.IOException
		 private PhysicalFlushableChannel ResetStoreFile( File nextStore )
		 {
			  _fsa.truncate( nextStore, 0 );
			  return new PhysicalFlushableChannel( _fsa.open( nextStore, OpenMode.READ_WRITE ) );
		 }

		 internal static File StateDir( File baseDir, string name )
		 {
			  return new File( baseDir, name + "-state" );
		 }
	}

}