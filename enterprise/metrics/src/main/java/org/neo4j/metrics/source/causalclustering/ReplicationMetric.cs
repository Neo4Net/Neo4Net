using System;

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

	using ReplicationMonitor = Org.Neo4j.causalclustering.core.replication.monitoring.ReplicationMonitor;

	public class ReplicationMetric : ReplicationMonitor
	{
		 private readonly AtomicLong _newReplication = new AtomicLong();
		 private readonly AtomicLong _attempts = new AtomicLong();
		 private readonly AtomicLong _success = new AtomicLong();
		 private readonly AtomicLong _fail = new AtomicLong();

		 public override void StartReplication()
		 {
			  _newReplication.AndIncrement;
		 }

		 public override void ReplicationAttempt()
		 {
			  _attempts.AndIncrement;
		 }

		 public override void SuccessfulReplication()
		 {
			  _success.AndIncrement;
		 }

		 public override void FailedReplication( Exception t )
		 {
			  _fail.AndIncrement;
		 }

		 public virtual long NewReplicationCount()
		 {
			  return _newReplication.get();
		 }

		 public virtual long AttemptCount()
		 {
			  return _attempts.get();
		 }

		 public virtual long SuccessCount()
		 {
			  return _success.get();
		 }

		 public virtual long FailCount()
		 {
			  return _fail.get();
		 }
	}

}