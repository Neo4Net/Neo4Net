/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
	public class LogHeader
	{
		 public const int LOG_HEADER_SIZE = 16;

		 public readonly sbyte LogFormatVersion;
		 public readonly long LogVersion;
		 public readonly long LastCommittedTxId;

		 public LogHeader( sbyte logFormatVersion, long logVersion, long lastCommittedTxId )
		 {
			  this.LogFormatVersion = logFormatVersion;
			  this.LogVersion = logVersion;
			  this.LastCommittedTxId = lastCommittedTxId;
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

			  LogHeader logHeader = ( LogHeader ) o;
			  return LastCommittedTxId == logHeader.LastCommittedTxId && LogFormatVersion == logHeader.LogFormatVersion && LogVersion == logHeader.LogVersion;
		 }

		 public override int GetHashCode()
		 {
			  int result = ( int ) LogFormatVersion;
			  result = 31 * result + ( int )( LogVersion ^ ( ( long )( ( ulong )LogVersion >> 32 ) ) );
			  result = 31 * result + ( int )( LastCommittedTxId ^ ( ( long )( ( ulong )LastCommittedTxId >> 32 ) ) );
			  return result;
		 }

		 public override string ToString()
		 {
			  return "LogHeader{" +
						 "logFormatVersion=" + LogFormatVersion +
						 ", logVersion=" + LogVersion +
						 ", lastCommittedTxId=" + LastCommittedTxId +
						 '}';
		 }
	}

}