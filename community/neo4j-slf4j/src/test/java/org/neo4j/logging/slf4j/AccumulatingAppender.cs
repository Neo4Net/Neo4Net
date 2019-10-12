using System.Collections.Generic;

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
namespace Org.Neo4j.Logging.slf4j
{
	using AppenderSkeleton = org.apache.log4j.AppenderSkeleton;
	using LoggingEvent = org.apache.log4j.spi.LoggingEvent;


	public class AccumulatingAppender : AppenderSkeleton
	{
		 private readonly LinkedList<LoggingEvent> _eventsList = new ConcurrentLinkedQueue<LoggingEvent>();

		 protected internal override void Append( LoggingEvent @event )
		 {
			  _eventsList.AddLast( @event );
		 }

		 public override void Close()
		 {
		 }

		 public override bool RequiresLayout()
		 {
			  return false;
		 }

		 internal virtual void ClearEventsList()
		 {
			  _eventsList.Clear();
		 }

		 internal virtual List<LoggingEvent> EventsList
		 {
			 get
			 {
				  return new List<LoggingEvent>( _eventsList );
			 }
		 }
	}

}