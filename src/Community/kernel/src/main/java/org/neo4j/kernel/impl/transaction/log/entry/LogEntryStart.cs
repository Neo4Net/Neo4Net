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
namespace Neo4Net.Kernel.impl.transaction.log.entry
{

	using Format = Neo4Net.Helpers.Format;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogEntryByteCodes.TX_START;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogEntryVersion.CURRENT;

	public class LogEntryStart : AbstractLogEntry
	{
		 private readonly int _masterId;
		 private readonly int _authorId;
		 private readonly long _timeWritten;
		 private readonly long _lastCommittedTxWhenTransactionStarted;
		 private readonly sbyte[] _additionalHeader;
		 private LogPosition _startPosition;

		 public LogEntryStart( int masterId, int authorId, long timeWritten, long lastCommittedTxWhenTransactionStarted, sbyte[] additionalHeader, LogPosition startPosition ) : this( CURRENT, masterId, authorId, timeWritten, lastCommittedTxWhenTransactionStarted, additionalHeader, startPosition )
		 {
		 }

		 public LogEntryStart( LogEntryVersion version, int masterId, int authorId, long timeWritten, long lastCommittedTxWhenTransactionStarted, sbyte[] additionalHeader, LogPosition startPosition ) : base( version, TX_START )
		 {
			  this._masterId = masterId;
			  this._authorId = authorId;
			  this._startPosition = startPosition;
			  this._timeWritten = timeWritten;
			  this._lastCommittedTxWhenTransactionStarted = lastCommittedTxWhenTransactionStarted;
			  this._additionalHeader = additionalHeader;
		 }

		 public virtual int MasterId
		 {
			 get
			 {
				  return _masterId;
			 }
		 }

		 public virtual int LocalId
		 {
			 get
			 {
				  return _authorId;
			 }
		 }

		 public virtual LogPosition StartPosition
		 {
			 get
			 {
				  return _startPosition;
			 }
		 }

		 public virtual long TimeWritten
		 {
			 get
			 {
				  return _timeWritten;
			 }
		 }

		 public virtual long LastCommittedTxWhenTransactionStarted
		 {
			 get
			 {
				  return _lastCommittedTxWhenTransactionStarted;
			 }
		 }

		 public virtual sbyte[] AdditionalHeader
		 {
			 get
			 {
				  return _additionalHeader;
			 }
		 }

		 /// <returns> combines necessary state to get a unique checksum to identify this transaction uniquely. </returns>
		 public static long Checksum( sbyte[] additionalHeader, int masterId, int authorId )
		 {
			  // [4 bits combined masterId/myId][4 bits xid hashcode, which combines time/randomness]
			  long lowBits = Arrays.GetHashCode( additionalHeader );
			  long highBits = masterId * 37 + authorId;
			  return ( highBits << 32 ) | ( lowBits & 0xFFFFFFFFL );
		 }

		 public static long Checksum( LogEntryStart entry )
		 {
			  return Checksum( entry._additionalHeader, entry._masterId, entry._authorId );
		 }

		 public virtual long Checksum()
		 {
			  return Checksum( this );
		 }

		 public override string ToString()
		 {
			  return ToString( Format.DEFAULT_TIME_ZONE );
		 }

		 public override string ToString( TimeZone timeZone )
		 {
			  return "Start[" +
						 "master=" + _masterId + "," +
						 "me=" + _authorId + "," +
						 "time=" + Timestamp( _timeWritten, timeZone ) + "," +
						 "lastCommittedTxWhenTransactionStarted=" + _lastCommittedTxWhenTransactionStarted + "," +
						 "additionalHeaderLength=" + ( _additionalHeader == null ? -1 : _additionalHeader.Length ) + "," +
						 ( _additionalHeader == null ? "" : Arrays.ToString( _additionalHeader ) ) + "," +
						 "position=" + _startPosition + "," +
						 "checksum=" + Checksum( this ) +
						 "]";
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Override @SuppressWarnings("unchecked") public <T extends LogEntry> T as()
		 public override T As<T>() where T : LogEntry
		 {
			  return ( T ) this;
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

			  LogEntryStart start = ( LogEntryStart ) o;

			  return _authorId == start._authorId && _lastCommittedTxWhenTransactionStarted == start._lastCommittedTxWhenTransactionStarted && _masterId == start._masterId && _timeWritten == start._timeWritten && Arrays.Equals( _additionalHeader, start._additionalHeader ) && _startPosition.Equals( start._startPosition );
		 }

		 public override int GetHashCode()
		 {
			  int result = _masterId;
			  result = 31 * result + _authorId;
			  result = 31 * result + ( int )( _timeWritten ^ ( ( long )( ( ulong )_timeWritten >> 32 ) ) );
			  result = 31 * result + ( int )( _lastCommittedTxWhenTransactionStarted ^ ( ( long )( ( ulong )_lastCommittedTxWhenTransactionStarted >> 32 ) ) );
			  result = 31 * result + ( _additionalHeader != null ? Arrays.GetHashCode( _additionalHeader ) : 0 );
			  result = 31 * result + _startPosition.GetHashCode();
			  return result;
		 }
	}

}