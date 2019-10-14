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
namespace Neo4Net.Kernel.impl.util
{
	using Log = Neo4Net.Logging.Log;

	public class DurationLogger : AutoCloseable
	{
		 private readonly Log _log;
		 private readonly string _tag;

		 private long _start;
		 private string _outcome = "Not finished";

		 public DurationLogger( Log log, string tag )
		 {
			  this._log = log;
			  this._tag = tag;
			  _start = DateTimeHelper.CurrentUnixTimeMillis();
			  log.Debug( format( "Started: %s", tag ) );
		 }

		 public virtual void MarkAsFinished()
		 {
			  _outcome = null;
		 }

		 public virtual void MarkAsAborted( string cause )
		 {
			  _outcome = format( "Aborted (cause: %s)", cause );
		 }

		 public override void Close()
		 {
			  long end = DateTimeHelper.CurrentUnixTimeMillis();
			  long duration = end - _start;
			  if ( string.ReferenceEquals( _outcome, null ) )
			  {
					_log.debug( format( "Finished: %s in %d ms", _tag, duration ) );
			  }
			  else
			  {
					_log.warn( format( "%s: %s in %d ms", _outcome, _tag, duration ) );
			  }
		 }
	}

}