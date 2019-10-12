/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.metrics.source.causalclustering
{

	using PullRequestMonitor = Org.Neo4j.causalclustering.catchup.tx.PullRequestMonitor;

	internal class PullRequestMetric : PullRequestMonitor
	{
		 private AtomicLong _lastRequestedTxId = new AtomicLong( 0 );
		 private AtomicLong _lastReceivedTxId = new AtomicLong( 0 );
		 private LongAdder _events = new LongAdder();

		 public override void TxPullRequest( long txId )
		 {
			  _events.increment();
			  this._lastRequestedTxId.set( txId );
		 }

		 public override void TxPullResponse( long txId )
		 {
			  _lastReceivedTxId.set( txId );
		 }

		 public override long LastRequestedTxId()
		 {
			  return this._lastRequestedTxId.get();
		 }

		 public override long NumberOfRequests()
		 {
			  return _events.longValue();
		 }

		 public override long LastReceivedTxId()
		 {
			  return _lastReceivedTxId.get();
		 }
	}

}