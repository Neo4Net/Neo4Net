using System.Diagnostics;

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
namespace Neo4Net.causalclustering.core.state
{

	using Neo4Net.causalclustering.core.state.storage;
	using EndOfStreamException = Neo4Net.causalclustering.messaging.EndOfStreamException;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using OpenMode = Neo4Net.Io.fs.OpenMode;
	using Neo4Net.Kernel.impl.transaction.log;
	using ReadableClosableChannel = Neo4Net.Kernel.impl.transaction.log.ReadableClosableChannel;

	public class StateRecoveryManager<STATE>
	{
		 public class RecoveryStatus<STATE>
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly File ActiveFileConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly STATE RecoveredStateConflict;

			  internal RecoveryStatus( File activeFile, STATE recoveredState )
			  {
					this.ActiveFileConflict = activeFile;
					this.RecoveredStateConflict = recoveredState;
			  }

			  public virtual STATE RecoveredState()
			  {
					return RecoveredStateConflict;
			  }

			  public virtual File ActiveFile()
			  {
					return ActiveFileConflict;
			  }
		 }

		 protected internal readonly FileSystemAbstraction FileSystem;
		 private readonly StateMarshal<STATE> _marshal;

		 public StateRecoveryManager( FileSystemAbstraction fileSystem, StateMarshal<STATE> marshal )
		 {
			  this.FileSystem = fileSystem;
			  this._marshal = marshal;
		 }

		 /// <returns> RecoveryStatus containing the previously active and previously inactive files. The previously active
		 /// file contains the latest readable log index (though it may also contain some garbage) and the inactive file is
		 /// safe to become the new state holder. </returns>
		 /// <exception cref="IOException"> if any IO goes wrong. </exception>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public RecoveryStatus<STATE> recover(java.io.File fileA, java.io.File fileB) throws java.io.IOException
		 public virtual RecoveryStatus<STATE> Recover( File fileA, File fileB )
		 {
			  Debug.Assert( fileA != null && fileB != null );

			  STATE a = ReadLastEntryFrom( fileA );
			  STATE b = ReadLastEntryFrom( fileB );

			  if ( a == default( STATE ) && b == default( STATE ) )
			  {
					throw new System.InvalidOperationException( "no recoverable state" );
			  }

			  if ( a == default( STATE ) )
			  {
					return new RecoveryStatus<STATE>( fileA, b );
			  }
			  else if ( b == default( STATE ) )
			  {
					return new RecoveryStatus<STATE>( fileB, a );
			  }
			  else if ( _marshal.ordinal( a ) > _marshal.ordinal( b ) )
			  {
					return new RecoveryStatus<STATE>( fileB, a );
			  }
			  else
			  {
					return new RecoveryStatus<STATE>( fileA, b );
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private STATE readLastEntryFrom(java.io.File file) throws java.io.IOException
		 private STATE ReadLastEntryFrom( File file )
		 {
			  using ( ReadableClosableChannel channel = new ReadAheadChannel<>( FileSystem.open( file, OpenMode.READ ) ) )
			  {
					STATE result = default( STATE );
					STATE lastRead;

					try
					{
						 while ( ( lastRead = _marshal.unmarshal( channel ) ) != default( STATE ) )
						 {
							  result = lastRead;
						 }
					}
					catch ( EndOfStreamException )
					{
						 // ignore; just use previous complete entry
					}

					return result;
			  }
		 }
	}

}