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
namespace Org.Neo4j.@unsafe.Impl.Batchimport.staging
{

	using Org.Neo4j.Util.concurrent;

	internal class SendDownstream : Work<Downstream, SendDownstream>
	{
		 private readonly LongAdder _downstreamIdleTime;
		 private TicketedBatch _head;
		 private TicketedBatch _tail;

		 internal SendDownstream( long ticket, object batch, LongAdder downstreamIdleTime )
		 {
			  this._downstreamIdleTime = downstreamIdleTime;
			  TicketedBatch b = new TicketedBatch( ticket, batch );
			  _head = b;
			  _tail = b;
		 }

		 public override SendDownstream Combine( SendDownstream work )
		 {
			  _tail.next = work._head;
			  _tail = work._tail;
			  return this;
		 }

		 public override void Apply( Downstream downstream )
		 {
			  TicketedBatch next = _head;
			  do
			  {
					downstream.Queue( next );
					next = next.Next;
			  } while ( next != null );
			  _downstreamIdleTime.add( downstream.Send() );
		 }
	}

}