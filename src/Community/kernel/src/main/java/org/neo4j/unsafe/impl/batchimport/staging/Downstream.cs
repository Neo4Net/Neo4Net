using System.Collections.Generic;

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
namespace Neo4Net.@unsafe.Impl.Batchimport.staging
{

	internal class Downstream
	{
		 private static readonly IComparer<TicketedBatch> _ticketedBatchComparator = ( a, b ) => Long.compare( b.ticket, a.ticket );

		 private readonly Step<object> _downstream;
		 private readonly AtomicLong _doneBatches;
		 private readonly List<TicketedBatch> _batches;
		 private long _lastSendTicket = -1;

		 internal Downstream( Step<object> downstream, AtomicLong doneBatches )
		 {
			  this._downstream = downstream;
			  this._doneBatches = doneBatches;
			  _batches = new List<TicketedBatch>();
		 }

		 internal virtual long Send()
		 {
			  // Sort in reverse, so the elements we want to send first are at the end.
			  _batches.sort( _ticketedBatchComparator );
			  long idleTimeSum = 0;
			  long batchesDone = 0;

			  for ( int i = _batches.Count - 1; i >= 0 ; i-- )
			  {
					TicketedBatch batch = _batches[i];
					if ( batch.Ticket == _lastSendTicket + 1 )
					{
						 _batches.RemoveAt( i );
						 _lastSendTicket = batch.Ticket;
						 idleTimeSum += _downstream.receive( batch.Ticket, batch.Batch );
						 batchesDone++;
					}
					else
					{
						 break;
					}
			  }

			  _doneBatches.getAndAdd( batchesDone );
			  return idleTimeSum;
		 }

		 internal virtual void Queue( TicketedBatch batch )
		 {
			  // Check that this is not a marker to flush the downstream.
			  if ( batch.Ticket != -1 && batch.Batch != null )
			  {
					_batches.Add( batch );
			  }
		 }
	}

}