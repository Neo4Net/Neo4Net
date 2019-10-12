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
namespace Neo4Net.causalclustering
{

	using GlobalSession = Neo4Net.causalclustering.core.replication.session.GlobalSession;
	using GlobalSessionTrackerState = Neo4Net.causalclustering.core.replication.session.GlobalSessionTrackerState;
	using LocalOperationId = Neo4Net.causalclustering.core.replication.session.LocalOperationId;
	using Neo4Net.causalclustering.core.state.storage;

	public class SessionTracker
	{
		 private readonly StateStorage<GlobalSessionTrackerState> _sessionTrackerStorage;
		 private GlobalSessionTrackerState _sessionState;

		 public SessionTracker( StateStorage<GlobalSessionTrackerState> sessionTrackerStorage )
		 {
			  this._sessionTrackerStorage = sessionTrackerStorage;
		 }

		 public virtual void Start()
		 {
			  if ( _sessionState == null )
			  {
					_sessionState = _sessionTrackerStorage.InitialState;
			  }
		 }

		 public virtual long LastAppliedIndex
		 {
			 get
			 {
				  return _sessionState.logIndex();
			 }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void flush() throws java.io.IOException
		 public virtual void Flush()
		 {
			  _sessionTrackerStorage.persistStoreData( _sessionState );
		 }

		 public virtual GlobalSessionTrackerState Snapshot()
		 {
			  return _sessionState.newInstance();
		 }

		 public virtual void InstallSnapshot( GlobalSessionTrackerState sessionState )
		 {
			  this._sessionState = sessionState;
		 }

		 public virtual bool ValidateOperation( GlobalSession globalSession, LocalOperationId localOperationId )
		 {
			  return _sessionState.validateOperation( globalSession, localOperationId );
		 }

		 public virtual void Update( GlobalSession globalSession, LocalOperationId localOperationId, long logIndex )
		 {
			  _sessionState.update( globalSession, localOperationId, logIndex );
		 }

		 public virtual GlobalSessionTrackerState NewInstance()
		 {
			  return _sessionState.newInstance();
		 }
	}

}