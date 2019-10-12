using System;
using System.Collections.Generic;

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
namespace Neo4Net.cluster.protocol.atomicbroadcast.multipaxos
{


	/// <summary>
	/// Record of an individual Paxos instance, from a proposer perspective
	/// </summary>
	public class PaxosInstance
	{

		 internal enum State
		 {
			  Empty,
			  P1Pending,
			  P1Ready,
			  P2Pending,
			  Closed,
			  Delivered
		 }
		 internal PaxosInstanceStore Store;

		 internal InstanceId Id;
		 internal State State = State.Empty;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 internal long BallotConflict;
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
		 internal IList<URI> AcceptorsConflict;
		 internal IList<ProposerMessage.PromiseState> Promises = new List<ProposerMessage.PromiseState>();
		 internal IList<ProposerMessage.AcceptedState> Accepts = new List<ProposerMessage.AcceptedState>();
		 internal IList<ProposerMessage.RejectAcceptState> RejectedAccepts = new List<ProposerMessage.RejectAcceptState>();
		 internal object Value_1;
		 internal long Phase1Ballot;
		 internal object Value_2;
		 // This is true iff the acceptors did not already have a value for this instance
		 internal bool ClientValue;
		 internal string ConversationIdHeader;

		 public PaxosInstance( PaxosInstanceStore store, InstanceId instanceId )
		 {
			  this.Store = store;
			  this.Id = instanceId;
		 }

		 public virtual bool IsState( State s )
		 {
			  return State.Equals( s );
		 }

		 public virtual long Ballot
		 {
			 get
			 {
				  return BallotConflict;
			 }
		 }

		 public virtual void Propose( long ballot, IList<URI> acceptors )
		 {
			  this.State = State.P1Pending;
			  this.AcceptorsConflict = acceptors;
			  this.BallotConflict = ballot;
		 }

		 public virtual void Phase1Timeout( long ballot )
		 {
			  this.BallotConflict = ballot;
			  Promises.Clear();
		 }

		 public virtual void Promise( ProposerMessage.PromiseState promiseState )
		 {
			  Promises.Add( promiseState );
			  if ( promiseState.Value != null && promiseState.Ballot >= Phase1Ballot )
			  {
					Value_1 = promiseState.Value;
					Phase1Ballot = promiseState.Ballot;
			  }
		 }

		 public virtual bool IsPromised( int minimumQuorumSize )
		 {
			  return Promises.Count == minimumQuorumSize;
		 }

		 public virtual void Ready( object value, bool clientValue )
		 {
			  AssertNotNull( value );

			  State = State.P1Ready;
			  Promises.Clear();
			  Value_1 = null;
			  Phase1Ballot = 0;
			  Value_2 = value;
			  this.ClientValue = clientValue;
		 }

		 public virtual void Pending()
		 {
			  State = State.P2Pending;
		 }

		 public virtual void Phase2Timeout( long ballot )
		 {
			  State = State.P1Pending;
			  this.BallotConflict = ballot;
			  Promises.Clear();
			  Value_1 = null;
			  Phase1Ballot = 0;
		 }

		 public virtual void Accepted( ProposerMessage.AcceptedState acceptedState )
		 {
			  Accepts.Add( acceptedState );
		 }

		 public virtual void Rejected( ProposerMessage.RejectAcceptState rejectAcceptState )
		 {
			  RejectedAccepts.Add( rejectAcceptState );
		 }

		 public virtual bool IsAccepted( int minimumQuorumSize )
		 {
			  // If we have received enough responses to meet quorum and a majority
			  // are accepts, then the instance is considered accepted
			  return Accepts.Count + RejectedAccepts.Count == minimumQuorumSize && Accepts.Count > RejectedAccepts.Count;
		 }

		 public virtual void Closed( object value, string conversationIdHeader )
		 {
			  AssertNotNull( value );

			  Value_2 = value;
			  State = State.Closed;
			  Accepts.Clear();
			  RejectedAccepts.Clear();
			  AcceptorsConflict = null;
			  this.ConversationIdHeader = conversationIdHeader;
		 }

		 private void AssertNotNull( object value )
		 {
			  if ( value == null )
			  {
					throw new System.ArgumentException( "value null" );
			  }
		 }

		 public virtual void Delivered()
		 {
			  State = State.Delivered;
			  Store.delivered( Id );
		 }

		 public virtual IList<URI> Acceptors
		 {
			 get
			 {
				  return AcceptorsConflict;
			 }
		 }

		 public virtual PaxosInstance Snapshot( PaxosInstanceStore store )
		 {
			  PaxosInstance snap = new PaxosInstance( store, Id );

			  snap.State = State;
			  snap.BallotConflict = BallotConflict;
			  snap.AcceptorsConflict = AcceptorsConflict == null ? null : new List<URI>( AcceptorsConflict );
			  snap.Promises = Promises == null ? null : new List<ProposerMessage.PromiseState>( Promises );
			  snap.Accepts = Accepts == null ? null : new List<ProposerMessage.AcceptedState>( Accepts );
			  snap.RejectedAccepts = RejectedAccepts == null ? null : new List<ProposerMessage.RejectAcceptState>( RejectedAccepts );
			  snap.Value_1 = Value_1;
			  snap.Phase1Ballot = Phase1Ballot;
			  snap.Value_2 = Value_2;
			  snap.ClientValue = ClientValue;
			  snap.ConversationIdHeader = ConversationIdHeader;

			  return snap;
		 }

		 public override string ToString()
		 {
			  try
			  {
					object toStringValue1 = null;
					if ( Value_1 != null )
					{
						 if ( Value_1 is Payload )
						 {
							  toStringValue1 = ( new AtomicBroadcastSerializer() ).receive((Payload) Value_1).ToString();
						 }
						 else
						 {
							  toStringValue1 = Value_1.ToString();
						 }
					}

					object toStringValue2 = null;
					if ( Value_2 != null )
					{
						 if ( Value_2 is Payload )
						 {
							  toStringValue2 = ( new AtomicBroadcastSerializer() ).receive((Payload) Value_2).ToString();
						 }
						 else
						 {
							  toStringValue2 = Value_2.ToString();
						 }
					}

					return "[id:" + Id +
							 " state:" + State.name() +
							 " b:" + BallotConflict +
							 " v1:" + toStringValue1 +
							 " v2:" + toStringValue2 + "]";
			  }
			  catch ( Exception )
			  {
					return "";
			  }
		 }
	}

}