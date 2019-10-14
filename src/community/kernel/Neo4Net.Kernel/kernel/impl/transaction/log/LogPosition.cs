using System;

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
namespace Neo4Net.Kernel.impl.transaction.log
{
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.impl.transaction.log.entry.LogHeader.LOG_HEADER_SIZE;

	public class LogPosition : IComparable<LogPosition>
	{
		 public static readonly LogPosition UNSPECIFIED = new LogPositionAnonymousInnerClass();

		 private class LogPositionAnonymousInnerClass : LogPosition
		 {
			 public LogPositionAnonymousInnerClass() : base(-1, -1)
			 {
			 }

			 public override long LogVersion
			 {
				 get
				 {
					  throw new System.NotSupportedException();
				 }
			 }

			 public override long ByteOffset
			 {
				 get
				 {
					  throw new System.NotSupportedException();
				 }
			 }

			 public override string ToString()
			 {
				  return "UNSPECIFIED";
			 }
		 }

		 public static LogPosition Start( long logVersion )
		 {
			  return new LogPosition( logVersion, LOG_HEADER_SIZE );
		 }

		 private readonly long _logVersion;
		 private readonly long _byteOffset;

		 public LogPosition( long logVersion, long byteOffset )
		 {
			  this._logVersion = logVersion;
			  this._byteOffset = byteOffset;
		 }

		 public virtual long LogVersion
		 {
			 get
			 {
				  return _logVersion;
			 }
		 }

		 public virtual long ByteOffset
		 {
			 get
			 {
				  return _byteOffset;
			 }
		 }

		 public override string ToString()
		 {
			  return "LogPosition{" +
						 "logVersion=" + _logVersion +
						 ", byteOffset=" + _byteOffset +
						 '}';
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

			  LogPosition that = ( LogPosition ) o;
			  return _byteOffset == that._byteOffset && _logVersion == that._logVersion;
		 }

		 public override int GetHashCode()
		 {
			  int result = ( int )( _logVersion ^ ( ( long )( ( ulong )_logVersion >> 32 ) ) );
			  result = 31 * result + ( int )( _byteOffset ^ ( ( long )( ( ulong )_byteOffset >> 32 ) ) );
			  return result;
		 }

		 public override int CompareTo( LogPosition o )
		 {
			  if ( _logVersion != o._logVersion )
			  {
					return Long.compare( _logVersion, o._logVersion );
			  }
			  return Long.compare( _byteOffset, o._byteOffset );
		 }
	}

}