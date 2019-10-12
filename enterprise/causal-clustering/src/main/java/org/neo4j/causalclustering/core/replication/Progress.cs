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
namespace Org.Neo4j.causalclustering.core.replication
{

	/// <summary>
	/// The progress of a single replicated operation, from replication to result, and associated synchronization.
	/// </summary>
	public class Progress
	{
		 private readonly Semaphore _replicationSignal = new Semaphore( 0 );
		 private readonly CompletableFuture<object> _futureResult = new CompletableFuture<object>();

		 private volatile bool _isReplicated;

		 public virtual void TriggerReplicationEvent()
		 {
			  _replicationSignal.release();
		 }

		 public virtual void SetReplicated()
		 {
			  _isReplicated = true;
			  _replicationSignal.release();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void awaitReplication(long timeoutMillis) throws InterruptedException
		 public virtual void AwaitReplication( long timeoutMillis )
		 {
			  if ( !_isReplicated )
			  {
					_replicationSignal.tryAcquire( timeoutMillis, MILLISECONDS );
			  }
		 }

		 public virtual bool Replicated
		 {
			 get
			 {
				  return _isReplicated;
			 }
		 }

		 public virtual CompletableFuture<object> FutureResult()
		 {
			  return _futureResult;
		 }
	}

}