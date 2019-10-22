using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.core.replication.session
{

	using Neo4Net.causalclustering.messaging.marshalling;
	using EndOfStreamException = Neo4Net.causalclustering.messaging.EndOfStreamException;
	using Neo4Net.causalclustering.core.state.storage;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using ReadableChannel = Neo4Net.Storageengine.Api.ReadableChannel;
	using WritableChannel = Neo4Net.Storageengine.Api.WritableChannel;

	/// <summary>
	/// In memory implementation of <seealso cref="GlobalSessionTrackerState"/>.
	/// </summary>
	public class GlobalSessionTrackerState
	{
		 /// <summary>
		 /// Each owner can only have one local session tracker, identified by the unique global session ID.
		 /// </summary>
		 private IDictionary<MemberId, LocalSessionTracker> _sessionTrackers = new Dictionary<MemberId, LocalSessionTracker>();

		 private long _logIndex = -1L;

		 /// <summary>
		 /// Tracks the operation and returns true iff this operation should be allowed.
		 /// </summary>
		 public virtual bool ValidateOperation( GlobalSession globalSession, LocalOperationId localOperationId )
		 {
			  LocalSessionTracker existingSessionTracker = _sessionTrackers[globalSession.Owner()];
			  if ( IsNewSession( globalSession, existingSessionTracker ) )
			  {
					return IsFirstOperation( localOperationId );
			  }
			  else
			  {
					return existingSessionTracker.IsValidOperation( localOperationId );
			  }
		 }

		 public virtual void Update( GlobalSession globalSession, LocalOperationId localOperationId, long logIndex )
		 {
			  LocalSessionTracker localSessionTracker = ValidateGlobalSessionAndGetLocalSessionTracker( globalSession );
			  localSessionTracker.ValidateAndTrackOperation( localOperationId );
			  this._logIndex = logIndex;
		 }

		 private bool IsNewSession( GlobalSession globalSession, LocalSessionTracker existingSessionTracker )
		 {
			  return existingSessionTracker == null || !existingSessionTracker.GlobalSessionId.Equals( globalSession.SessionId() );
		 }

		 private bool IsFirstOperation( LocalOperationId id )
		 {
			  return id.SequenceNumber() == 0;
		 }

		 public virtual long LogIndex()
		 {
			  return _logIndex;
		 }

		 private LocalSessionTracker ValidateGlobalSessionAndGetLocalSessionTracker( GlobalSession globalSession )
		 {
			  LocalSessionTracker localSessionTracker = _sessionTrackers[globalSession.Owner()];

			  if ( localSessionTracker == null || !localSessionTracker.GlobalSessionId.Equals( globalSession.SessionId() ) )
			  {
					localSessionTracker = new LocalSessionTracker( globalSession.SessionId(), new Dictionary<long, long>() );
					_sessionTrackers[globalSession.Owner()] = localSessionTracker;
			  }

			  return localSessionTracker;
		 }

		 public virtual GlobalSessionTrackerState NewInstance()
		 {
			  GlobalSessionTrackerState copy = new GlobalSessionTrackerState();
			  copy._logIndex = _logIndex;
			  foreach ( KeyValuePair<MemberId, LocalSessionTracker> entry in _sessionTrackers.SetOfKeyValuePairs() )
			  {
					copy._sessionTrackers[entry.Key] = entry.Value.newInstance();
			  }
			  return copy;
		 }

		 public override bool Equals( object o )
		 {
			  if ( this == o )
			  {
					return true;
			  }
			  if ( o == null || this.GetType() != o.GetType() )
			  {
					return false;
			  }
			  GlobalSessionTrackerState that = ( GlobalSessionTrackerState ) o;
			  return _logIndex == that._logIndex && Objects.Equals( _sessionTrackers, that._sessionTrackers );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _sessionTrackers, _logIndex );
		 }

		 public override string ToString()
		 {
			  return string.Format( "GlobalSessionTrackerState{{sessionTrackers={0}, logIndex={1:D}}}", _sessionTrackers, _logIndex );
		 }

		 public class Marshal : SafeStateMarshal<GlobalSessionTrackerState>
		 {
			  internal readonly ChannelMarshal<MemberId> MemberMarshal;

			  public Marshal( ChannelMarshal<MemberId> marshal )
			  {
					this.MemberMarshal = marshal;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(GlobalSessionTrackerState target, org.Neo4Net.storageengine.api.WritableChannel channel) throws java.io.IOException
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  public override void MarshalConflict( GlobalSessionTrackerState target, WritableChannel channel )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<org.Neo4Net.causalclustering.identity.MemberId, LocalSessionTracker> sessionTrackers = target.sessionTrackers;
					IDictionary<MemberId, LocalSessionTracker> sessionTrackers = target.sessionTrackers;

					channel.PutLong( target._logIndex );
					channel.PutInt( sessionTrackers.Count );

					foreach ( KeyValuePair<MemberId, LocalSessionTracker> entry in sessionTrackers.SetOfKeyValuePairs() )
					{
						 MemberMarshal.marshal( entry.Key, channel );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LocalSessionTracker localSessionTracker = entry.getValue();
						 LocalSessionTracker localSessionTracker = entry.Value;

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.UUID uuid = localSessionTracker.globalSessionId;
						 System.Guid uuid = localSessionTracker.GlobalSessionId;
						 channel.PutLong( uuid.MostSignificantBits );
						 channel.PutLong( uuid.LeastSignificantBits );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<long, long> map = localSessionTracker.lastSequenceNumberPerSession;
						 IDictionary<long, long> map = localSessionTracker.LastSequenceNumberPerSession;

						 channel.PutInt( map.Count );

						 foreach ( KeyValuePair<long, long> sessionSequence in map.SetOfKeyValuePairs() )
						 {
							  channel.PutLong( sessionSequence.Key );
							  channel.PutLong( sessionSequence.Value );
						 }
					}
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public GlobalSessionTrackerState unmarshal0(org.Neo4Net.storageengine.api.ReadableChannel channel) throws java.io.IOException, org.Neo4Net.causalclustering.messaging.EndOfStreamException
			  public override GlobalSessionTrackerState Unmarshal0( ReadableChannel channel )
			  {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final long logIndex = channel.getLong();
					long logIndex = channel.Long;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int sessionTrackerSize = channel.getInt();
					int sessionTrackerSize = channel.Int;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<org.Neo4Net.causalclustering.identity.MemberId, LocalSessionTracker> sessionTrackers = new java.util.HashMap<>();
					IDictionary<MemberId, LocalSessionTracker> sessionTrackers = new Dictionary<MemberId, LocalSessionTracker>();

					for ( int i = 0; i < sessionTrackerSize; i++ )
					{
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final org.Neo4Net.causalclustering.identity.MemberId member = memberMarshal.unmarshal(channel);
						 MemberId member = MemberMarshal.unmarshal( channel );
						 if ( member == null )
						 {
							  throw new System.InvalidOperationException( "Null member" );
						 }

						 long mostSigBits = channel.Long;
						 long leastSigBits = channel.Long;
						 System.Guid globalSessionId = new System.Guid( mostSigBits, leastSigBits );

//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final int localSessionTrackerSize = channel.getInt();
						 int localSessionTrackerSize = channel.Int;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.util.Map<long, long> lastSequenceNumberPerSession = new java.util.HashMap<>();
						 IDictionary<long, long> lastSequenceNumberPerSession = new Dictionary<long, long>();
						 for ( int j = 0; j < localSessionTrackerSize; j++ )
						 {
							  long localSessionId = channel.Long;
							  long sequenceNumber = channel.Long;
							  lastSequenceNumberPerSession[localSessionId] = sequenceNumber;
						 }
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final LocalSessionTracker localSessionTracker = new LocalSessionTracker(globalSessionId, lastSequenceNumberPerSession);
						 LocalSessionTracker localSessionTracker = new LocalSessionTracker( globalSessionId, lastSequenceNumberPerSession );
						 sessionTrackers[member] = localSessionTracker;
					}
					GlobalSessionTrackerState result = new GlobalSessionTrackerState();
					result.sessionTrackers = sessionTrackers;
					result.logIndex = logIndex;
					return result;
			  }

			  public override GlobalSessionTrackerState StartState()
			  {
					return new GlobalSessionTrackerState();
			  }

			  public override long Ordinal( GlobalSessionTrackerState state )
			  {
					return state.LogIndex();
			  }
		 }

		 private class LocalSessionTracker
		 {
			  internal readonly System.Guid GlobalSessionId;
			  internal readonly IDictionary<long, long> LastSequenceNumberPerSession; // localSessionId -> lastSequenceNumber

			  internal LocalSessionTracker( System.Guid globalSessionId, IDictionary<long, long> lastSequenceNumberPerSession )
			  {
					this.GlobalSessionId = globalSessionId;
					this.LastSequenceNumberPerSession = lastSequenceNumberPerSession;
			  }

			  internal virtual bool ValidateAndTrackOperation( LocalOperationId operationId )
			  {
					if ( !IsValidOperation( operationId ) )
					{
						 return false;
					}

					LastSequenceNumberPerSession[operationId.LocalSessionId()] = operationId.SequenceNumber();
					return true;
			  }

			  /// <summary>
			  /// The sequence numbers under a single local session must come strictly in order and are only valid once only.
			  /// </summary>
			  internal virtual bool IsValidOperation( LocalOperationId operationId )
			  {
					long? lastSequenceNumber = LastSequenceNumberPerSession[operationId.LocalSessionId()];

					if ( lastSequenceNumber == null )
					{
						 if ( operationId.SequenceNumber() != 0 )
						 {
							  return false;
						 }
					}
					else if ( operationId.SequenceNumber() != lastSequenceNumber + 1 )
					{
						 return false;
					}

					return true;
			  }

			  public virtual LocalSessionTracker NewInstance()
			  {
					return new LocalSessionTracker( GlobalSessionId, new Dictionary<long, long>( LastSequenceNumberPerSession ) );
			  }

			  public override string ToString()
			  {
					return string.Format( "LocalSessionTracker{{globalSessionId={0}, lastSequenceNumberPerSession={1}}}", GlobalSessionId, LastSequenceNumberPerSession );
			  }
		 }
	}

}