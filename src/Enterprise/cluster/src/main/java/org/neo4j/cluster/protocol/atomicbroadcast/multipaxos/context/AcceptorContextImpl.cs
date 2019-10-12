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
namespace Neo4Net.cluster.protocol.atomicbroadcast.multipaxos.context
{
	using Timeouts = Neo4Net.cluster.timeout.Timeouts;
	using LogProvider = Neo4Net.Logging.LogProvider;

	/// <summary>
	/// Context for the <seealso cref="AcceptorState"/> distributed state machine.
	/// <p/>
	/// This holds the store for Paxos instances, as seen from the acceptor role point of view in Paxos.
	/// </summary>
	internal class AcceptorContextImpl : AbstractContextImpl, AcceptorContext
	{
		 private readonly AcceptorInstanceStore _instanceStore;

		 internal AcceptorContextImpl( Neo4Net.cluster.InstanceId me, CommonContextState commonState, LogProvider logging, Timeouts timeouts, AcceptorInstanceStore instanceStore ) : base( me, commonState, logging, timeouts )
		 {
			  this._instanceStore = instanceStore;
		 }

		 public override AcceptorInstance GetAcceptorInstance( InstanceId instanceId )
		 {
			  return _instanceStore.getAcceptorInstance( instanceId );
		 }

		 public override void Promise( AcceptorInstance instance, long ballot )
		 {
			  _instanceStore.promise( instance, ballot );
		 }

		 public override void Accept( AcceptorInstance instance, object value )
		 {
			  _instanceStore.accept( instance, value );
		 }

		 public override void Leave()
		 {
			  _instanceStore.clear();
		 }

		 public virtual AcceptorContextImpl Snapshot( CommonContextState commonStateSnapshot, LogProvider logging, Timeouts timeouts, AcceptorInstanceStore instanceStore )
		 {
			  return new AcceptorContextImpl( Me, commonStateSnapshot, logging, timeouts, instanceStore );
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

			  AcceptorContextImpl that = ( AcceptorContextImpl ) o;

			  return _instanceStore != null ? _instanceStore.Equals( that._instanceStore ) : that._instanceStore == null;
		 }

		 public override int GetHashCode()
		 {
			  return _instanceStore != null ? _instanceStore.GetHashCode() : 0;
		 }
	}

}