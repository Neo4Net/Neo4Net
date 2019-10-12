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
	using Result = Org.Neo4j.causalclustering.core.state.Result;

	/// <summary>
	/// Keeps track of operations in progress. Operations move through two phases:
	///  - waiting for replication
	///  - waiting for result
	/// </summary>
	public interface ProgressTracker
	{
		 /// <summary>
		 /// Called to start tracking the progress of an operation.
		 /// </summary>
		 /// <param name="operation"> The operation to track.
		 /// </param>
		 /// <returns> A container for the progress. </returns>
		 Progress Start( DistributedOperation operation );

		 /// <summary>
		 /// Called when an operation has been replicated and is waiting
		 /// for the operation to be locally applied.
		 /// </summary>
		 /// <param name="operation"> The operation that has been replicated. </param>
		 void TrackReplication( DistributedOperation operation );

		 /// <summary>
		 /// Called when an operation has been applied and a result is
		 /// available.
		 /// </summary>
		 /// <param name="operation"> The operation that has been applied. </param>
		 /// <param name="result"> The result of the operation. </param>
		 void TrackResult( DistributedOperation operation, Result result );

		 /// <summary>
		 /// Called when an operation should be abnormally aborted
		 /// and removed from the tracker.
		 /// </summary>
		 /// <param name="operation"> The operation to be aborted. </param>
		 void Abort( DistributedOperation operation );

		 /// <summary>
		 /// Called when a significant event related to replication
		 /// has occurred (i.e. leader switch).
		 /// </summary>
		 void TriggerReplicationEvent();

		 /// <summary>
		 /// Returns a count of the current number of in-progress tracked operations.
		 /// </summary>
		 /// <returns> A count of currently tracked operations.. </returns>
		 int InProgressCount();
	}

}