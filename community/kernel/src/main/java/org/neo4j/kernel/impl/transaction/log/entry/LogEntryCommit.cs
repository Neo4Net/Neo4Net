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
namespace Org.Neo4j.Kernel.impl.transaction.log.entry
{

	using Format = Org.Neo4j.Helpers.Format;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogEntryByteCodes.TX_COMMIT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogEntryVersion.CURRENT;

	public class LogEntryCommit : AbstractLogEntry
	{
		 private readonly long _txId;
		 private readonly long _timeWritten;
		 protected internal readonly string Name;

		 public LogEntryCommit( long txId, long timeWritten ) : this( CURRENT, txId, timeWritten )
		 {
		 }

		 public LogEntryCommit( LogEntryVersion version, long txId, long timeWritten ) : base( version, TX_COMMIT )
		 {
			  this._txId = txId;
			  this._timeWritten = timeWritten;
			  this.Name = "Commit";
		 }

		 public virtual long TxId
		 {
			 get
			 {
				  return _txId;
			 }
		 }

		 public virtual long TimeWritten
		 {
			 get
			 {
				  return _timeWritten;
			 }
		 }

		 public override string ToString()
		 {
			  return ToString( Format.DEFAULT_TIME_ZONE );
		 }

		 public override string ToString( TimeZone timeZone )
		 {
			  return Name + "[txId=" + TxId + ", " + Timestamp( TimeWritten, timeZone ) + "]";
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

			  LogEntryCommit commit = ( LogEntryCommit ) o;
			  return _timeWritten == commit._timeWritten && _txId == commit._txId && Name.Equals( commit.Name );
		 }

		 public override int GetHashCode()
		 {
			  int result = ( int )( _txId ^ ( ( long )( ( ulong )_txId >> 32 ) ) );
			  result = 31 * result + ( int )( _timeWritten ^ ( ( long )( ( ulong )_timeWritten >> 32 ) ) );
			  result = 31 * result + Name.GetHashCode();
			  return result;
		 }
	}

}