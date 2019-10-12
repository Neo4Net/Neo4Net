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
namespace Neo4Net.Kernel.stresstests.transaction.checkpoint.workload
{

	internal class SyncMonitor : Worker.Monitor
	{
		 private readonly AtomicBoolean _stopSignal = new AtomicBoolean();
		 private readonly AtomicLong _transactionCounter = new AtomicLong();
		 private readonly System.Threading.CountdownEvent _stopLatch;

		 internal SyncMonitor( int threads )
		 {
			  this._stopLatch = new System.Threading.CountdownEvent( threads );
		 }

		 public override void TransactionCompleted()
		 {
			  _transactionCounter.incrementAndGet();
		 }

		 public override bool Stop()
		 {
			  return _stopSignal.get();
		 }

		 public override void Done()
		 {
			  _stopLatch.Signal();
		 }

		 public virtual long Transactions()
		 {
			  return _transactionCounter.get();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void stopAndWaitWorkers() throws InterruptedException
		 public virtual void StopAndWaitWorkers()
		 {
			  _stopSignal.set( true );
			  _stopLatch.await();
		 }
	}

}