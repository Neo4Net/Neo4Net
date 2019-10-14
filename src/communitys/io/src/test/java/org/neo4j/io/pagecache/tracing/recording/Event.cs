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
namespace Neo4Net.Io.pagecache.tracing.recording
{

	public abstract class Event
	{
		 public readonly PageSwapper Io;
		 public readonly long PageId;

		 public Event( PageSwapper io, long pageId )
		 {
			  this.Io = io;
			  this.PageId = pageId;
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

			  Event @event = ( Event ) o;

			  return PageId == @event.PageId && !( Io != null ?!Io.Equals( @event.Io ) : @event.Io != null );

		 }

		 public override int GetHashCode()
		 {
			  int result = Io != null ? Io.GetHashCode() : 0;
			  result = 31 * result + ( int )( PageId ^ ( ( long )( ( ulong )PageId >> 32 ) ) );
			  return result;
		 }

		 public override string ToString()
		 {
			  return string.Format( "{0}{{io={1}, pageId={2}}}", this.GetType().Name, Io, PageId );
		 }
	}

}