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

	public class CheckPoint : AbstractLogEntry
	{
		 private readonly LogPosition _logPosition;

		 public CheckPoint( LogPosition logPosition ) : this( LogEntryVersion.CURRENT, logPosition )
		 {
		 }

		 public CheckPoint( LogEntryVersion version, LogPosition logPosition ) : base( version, LogEntryByteCodes.CheckPoint )
		 {
			  this._logPosition = logPosition;
		 }

		 public override T As<T>() where T : LogEntry
		 {
			  return ( T ) this;
		 }

		 public virtual LogPosition LogPosition
		 {
			 get
			 {
				  return _logPosition;
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

			  CheckPoint that = ( CheckPoint ) o;

			  return !( _logPosition != null ?!_logPosition.Equals( that._logPosition ) : that._logPosition != null );
		 }

		 public override int GetHashCode()
		 {
			  return _logPosition != null ? _logPosition.GetHashCode() : 0;
		 }

		 public override string ToString()
		 {
			  return "CheckPoint[" +
						"position=" + _logPosition +
						']';
		 }
	}

}