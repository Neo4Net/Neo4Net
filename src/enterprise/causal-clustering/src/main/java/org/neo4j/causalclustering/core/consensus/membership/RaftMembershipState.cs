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
namespace Neo4Net.causalclustering.core.consensus.membership
{

	using Neo4Net.causalclustering.core.state.storage;
	using MemberId = Neo4Net.causalclustering.identity.MemberId;
	using EndOfStreamException = Neo4Net.causalclustering.messaging.EndOfStreamException;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;
	using ReadableChannel = Neo4Net.Kernel.Api.StorageEngine.ReadableChannel;
	using WritableChannel = Neo4Net.Kernel.Api.StorageEngine.WritableChannel;

	/// <summary>
	/// Represents the current state of membership in RAFT and exposes operations
	/// for modifying the state. The valid states and transitions are represented
	/// by the following table:
	/// 
	/// state                           valid transitions
	/// 1: [ -        , -        ]        2 (append)
	/// 2: [ -        , appended ]      1,4 (commit or truncate)
	/// 3: [ committed, appended ]        4 (commit or truncate)
	/// 4: [ committed, -        ]        3 (append)
	/// 
	/// The transition from 3->4 is either because the appended entry became
	/// the new committed entry or because the appended entry was truncated.
	/// 
	/// A committed entry can never be truncated and there can only be a single
	/// outstanding appended entry which usually is committed shortly
	/// thereafter, but it might also be truncated.
	/// 
	/// Recovery must in-order replay all the log entries whose effects are not
	/// guaranteed to have been persisted. The handling of these events is
	/// idempotent so it is safe to replay entries which might have been
	/// applied already.
	/// 
	/// Note that commit updates occur separately from append/truncation in RAFT
	/// so it is possible to for example observe several membership entries in a row
	/// being appended on a particular member without an intermediate commit, even
	/// though this is not possible in the system as a whole because the leader which
	/// drives the membership change work will not spawn a new entry until it knows
	/// that the previous one has been appended with a quorum, i.e. committed. This
	/// is the reason why that this class is very lax when it comes to updating the
	/// state and not making hard assertions which on a superficial level might
	/// seem obvious. The consensus system as a whole and the membership change
	/// driving logic is relied upon for achieving the correct system level
	/// behaviour.
	/// </summary>
	public class RaftMembershipState : LifecycleAdapter
	{
		 private MembershipEntry _committed;
		 private MembershipEntry _appended;
		 private long _ordinal; // persistence ordinal must be increased each time we change committed or appended

		 public RaftMembershipState() : this(-1, null, null)
		 {
		 }

		 internal RaftMembershipState( long ordinal, MembershipEntry committed, MembershipEntry appended )
		 {
			  this._ordinal = ordinal;
			  this._committed = committed;
			  this._appended = appended;
		 }

		 public virtual bool Append( long logIndex, ISet<MemberId> members )
		 {
			  if ( _appended != null && logIndex <= _appended.logIndex() )
			  {
					return false;
			  }

			  if ( _committed != null && logIndex <= _committed.logIndex() )
			  {
					return false;
			  }

			  if ( _appended != null && ( _committed == null || _appended.logIndex() > _committed.logIndex() ) )
			  {
					/* This might seem counter-intuitive, but seeing two appended entries
					in a row must mean that the previous one got committed. So it must
					be recorded as having been committed or a subsequent truncation might
					erase the state. We also protect against going backwards in the
					committed state, as might happen during recovery. */

					_committed = _appended;
			  }

			  _ordinal++;
			  _appended = new MembershipEntry( logIndex, members );
			  return true;
		 }

		 public virtual bool Truncate( long fromIndex )
		 {
			  if ( _committed != null && fromIndex <= _committed.logIndex() )
			  {
					throw new System.InvalidOperationException( "Truncating committed entry" );
			  }

			  if ( _appended != null && fromIndex <= _appended.logIndex() )
			  {
					_ordinal++;
					_appended = null;
					return true;
			  }
			  return false;
		 }

		 public virtual bool Commit( long commitIndex )
		 {
			  if ( _appended != null && commitIndex >= _appended.logIndex() )
			  {
					_ordinal++;
					_committed = _appended;
					_appended = null;
					return true;
			  }
			  return false;
		 }

		 internal virtual bool UncommittedMemberChangeInLog()
		 {
			  return _appended != null;
		 }

		 internal virtual ISet<MemberId> Latest
		 {
			 get
			 {
   
				  return _appended != null ? _appended.members() : _committed != null ? _committed.members() : new HashSet<MemberId>();
			 }
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
			  RaftMembershipState that = ( RaftMembershipState ) o;
			  return _ordinal == that._ordinal && Objects.Equals( _committed, that._committed ) && Objects.Equals( _appended, that._appended );
		 }

		 public override int GetHashCode()
		 {
			  return Objects.hash( _committed, _appended, _ordinal );
		 }

		 public override string ToString()
		 {
			  return "RaftMembershipState{" +
						"committed=" + _committed +
						", appended=" + _appended +
						", ordinal=" + _ordinal +
						'}';
		 }

		 public virtual RaftMembershipState NewInstance()
		 {
			  return new RaftMembershipState( _ordinal, _committed, _appended );
		 }

		 public virtual MembershipEntry Committed()
		 {
			  return _committed;
		 }

		 public virtual long Ordinal
		 {
			 get
			 {
				  return _ordinal;
			 }
		 }

		 public class Marshal : SafeStateMarshal<RaftMembershipState>
		 {
			  internal MembershipEntry.Marshal EntryMarshal = new MembershipEntry.Marshal();

			  public override RaftMembershipState StartState()
			  {
					return new RaftMembershipState();
			  }

			  public override long Ordinal( RaftMembershipState state )
			  {
					return state._ordinal;
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void marshal(RaftMembershipState state, org.Neo4Net.Kernel.Api.StorageEngine.WritableChannel channel) throws java.io.IOException
//JAVA TO C# CONVERTER NOTE: Members cannot have the same name as their enclosing type:
			  public override void MarshalConflict( RaftMembershipState state, WritableChannel channel )
			  {
					channel.PutLong( state._ordinal );
					EntryMarshal.marshal( state._committed, channel );
					EntryMarshal.marshal( state._appended, channel );
			  }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public RaftMembershipState unmarshal0(org.Neo4Net.Kernel.Api.StorageEngine.ReadableChannel channel) throws java.io.IOException, org.Neo4Net.causalclustering.messaging.EndOfStreamException
			  public override RaftMembershipState Unmarshal0( ReadableChannel channel )
			  {
					long ordinal = channel.Long;
					MembershipEntry committed = EntryMarshal.unmarshal( channel );
					MembershipEntry appended = EntryMarshal.unmarshal( channel );
					return new RaftMembershipState( ordinal, committed, appended );
			  }
		 }
	}

}