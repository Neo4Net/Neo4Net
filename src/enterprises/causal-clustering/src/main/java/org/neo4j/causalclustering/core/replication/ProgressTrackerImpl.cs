using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
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
namespace Neo4Net.causalclustering.core.replication
{

	using GlobalSession = Neo4Net.causalclustering.core.replication.session.GlobalSession;
	using LocalOperationId = Neo4Net.causalclustering.core.replication.session.LocalOperationId;
	using Result = Neo4Net.causalclustering.core.state.Result;

	public class ProgressTrackerImpl : ProgressTracker
	{
		 private readonly IDictionary<LocalOperationId, Progress> _tracker = new ConcurrentDictionary<LocalOperationId, Progress>();
		 private readonly GlobalSession _myGlobalSession;

		 public ProgressTrackerImpl( GlobalSession myGlobalSession )
		 {
			  this._myGlobalSession = myGlobalSession;
		 }

		 public override Progress Start( DistributedOperation operation )
		 {
			  Debug.Assert( operation.GlobalSession().Equals(_myGlobalSession) );

			  Progress progress = new Progress();
			  _tracker[operation.OperationId()] = progress;
			  return progress;
		 }

		 public override void TrackReplication( DistributedOperation operation )
		 {
			  if ( !operation.GlobalSession().Equals(_myGlobalSession) )
			  {
					return;
			  }

			  Progress progress = _tracker[operation.OperationId()];
			  if ( progress != null )
			  {
					progress.SetReplicated();
			  }
		 }

		 public override void TrackResult( DistributedOperation operation, Result result )
		 {
			  if ( !operation.GlobalSession().Equals(_myGlobalSession) )
			  {
					return;
			  }

			  Progress progress = _tracker.Remove( operation.OperationId() );

			  if ( progress != null )
			  {
					result.Apply( progress.FutureResult() );
			  }
		 }

		 public override void Abort( DistributedOperation operation )
		 {
			  _tracker.Remove( operation.OperationId() );
		 }

		 public override void TriggerReplicationEvent()
		 {
			  _tracker.forEach( ( ignored, progress ) => progress.triggerReplicationEvent() );
		 }

		 public override int InProgressCount()
		 {
			  return _tracker.Count;
		 }
	}

}