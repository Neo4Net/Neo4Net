using System.Collections.Generic;
using System.Text;

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
namespace Org.Neo4j.Kernel.impl.transaction.log
{

	using Org.Neo4j.Helpers.Collection;
	using StorageCommand = Org.Neo4j.Storageengine.Api.StorageCommand;

	public class PhysicalTransactionRepresentation : TransactionRepresentation
	{
		 private readonly ICollection<StorageCommand> _commands;
		 private sbyte[] _additionalHeader;
		 private int _masterId;
		 private int _authorId;
		 private long _timeStarted;
		 private long _latestCommittedTxWhenStarted;
		 private long _timeCommitted;

		 /// <summary>
		 /// This is a bit of a smell, it's used only for committing slave transactions on the master. Effectively, this
		 /// identifies the lock session used to guard this transaction. The master ensures that lock session is live before
		 /// committing, to guard against locks timing out. We may want to refactor this design later on.
		 /// </summary>
		 private int _lockSessionIdentifier;

		 public PhysicalTransactionRepresentation( ICollection<StorageCommand> commands )
		 {
			  this._commands = commands;
		 }

		 public virtual void SetHeader( sbyte[] additionalHeader, int masterId, int authorId, long timeStarted, long latestCommittedTxWhenStarted, long timeCommitted, int lockSession )
		 {
			  this._additionalHeader = additionalHeader;
			  this._masterId = masterId;
			  this._authorId = authorId;
			  this._timeStarted = timeStarted;
			  this._latestCommittedTxWhenStarted = latestCommittedTxWhenStarted;
			  this._timeCommitted = timeCommitted;
			  this._lockSessionIdentifier = lockSession;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public boolean accept(org.neo4j.helpers.collection.Visitor<org.neo4j.storageengine.api.StorageCommand,java.io.IOException> visitor) throws java.io.IOException
		 public override bool Accept( Visitor<StorageCommand, IOException> visitor )
		 {
			  foreach ( StorageCommand command in _commands )
			  {
					if ( visitor.Visit( command ) )
					{
						 return true;
					}
			  }
			  return false;
		 }

		 public override sbyte[] AdditionalHeader()
		 {
			  return _additionalHeader;
		 }

		 public virtual int MasterId
		 {
			 get
			 {
				  return _masterId;
			 }
		 }

		 public virtual int AuthorId
		 {
			 get
			 {
				  return _authorId;
			 }
		 }

		 public virtual long TimeStarted
		 {
			 get
			 {
				  return _timeStarted;
			 }
		 }

		 public virtual long LatestCommittedTxWhenStarted
		 {
			 get
			 {
				  return _latestCommittedTxWhenStarted;
			 }
		 }

		 public virtual long TimeCommitted
		 {
			 get
			 {
				  return _timeCommitted;
			 }
		 }

		 public virtual int LockSessionId
		 {
			 get
			 {
				  return _lockSessionIdentifier;
			 }
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }

			  PhysicalTransactionRepresentation that = ( PhysicalTransactionRepresentation ) o;
			  return _authorId == that._authorId && _latestCommittedTxWhenStarted == that._latestCommittedTxWhenStarted && _masterId == that._masterId && _timeStarted == that._timeStarted && Arrays.Equals( _additionalHeader, that._additionalHeader ) && _commands.Equals( that._commands );
		 }

		 public override int GetHashCode()
		 {
			  int result = _commands.GetHashCode();
			  result = 31 * result + ( _additionalHeader != null ? Arrays.GetHashCode( _additionalHeader ) : 0 );
			  result = 31 * result + _masterId;
			  result = 31 * result + _authorId;
			  result = 31 * result + ( int )( _timeStarted ^ ( ( long )( ( ulong )_timeStarted >> 32 ) ) );
			  result = 31 * result + ( int )( _latestCommittedTxWhenStarted ^ ( ( long )( ( ulong )_latestCommittedTxWhenStarted >> 32 ) ) );
			  return result;
		 }

		 public override string ToString()
		 {
			  StringBuilder builder = ( new StringBuilder( this.GetType().Name ) ).Append('[');
			  builder.Append( "masterId:" ).Append( _masterId ).Append( ',' );
			  builder.Append( "authorId:" ).Append( _authorId ).Append( ',' );
			  builder.Append( "timeStarted:" ).Append( _timeStarted ).Append( ',' );
			  builder.Append( "latestCommittedTxWhenStarted:" ).Append( _latestCommittedTxWhenStarted ).Append( ',' );
			  builder.Append( "timeCommitted:" ).Append( _timeCommitted ).Append( ',' );
			  builder.Append( "lockSession:" ).Append( _lockSessionIdentifier ).Append( ',' );
			  builder.Append( "additionalHeader:" ).Append( Arrays.ToString( _additionalHeader ) );
			  builder.Append( "commands.length:" ).Append( _commands.Count );
			  return builder.ToString();
		 }

		 public override IEnumerator<StorageCommand> Iterator()
		 {
			  return _commands.GetEnumerator();
		 }
	}

}