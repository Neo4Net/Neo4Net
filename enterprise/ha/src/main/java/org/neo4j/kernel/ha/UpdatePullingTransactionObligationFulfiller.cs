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
namespace Org.Neo4j.Kernel.ha
{

	using InstanceId = Org.Neo4j.cluster.InstanceId;
	using TransactionObligationFulfiller = Org.Neo4j.com.storecopy.TransactionObligationFulfiller;
	using HighAvailabilityMemberChangeEvent = Org.Neo4j.Kernel.ha.cluster.HighAvailabilityMemberChangeEvent;
	using HighAvailabilityMemberListener = Org.Neo4j.Kernel.ha.cluster.HighAvailabilityMemberListener;
	using HighAvailabilityMemberStateMachine = Org.Neo4j.Kernel.ha.cluster.HighAvailabilityMemberStateMachine;
	using TransactionIdStore = Org.Neo4j.Kernel.impl.transaction.log.TransactionIdStore;
	using LifecycleAdapter = Org.Neo4j.Kernel.Lifecycle.LifecycleAdapter;

	/// <summary>
	/// Fulfills transaction obligations by poking <seealso cref="UpdatePuller"/> and awaiting it to commit and apply
	/// the desired transactions.
	/// </summary>
	public class UpdatePullingTransactionObligationFulfiller : LifecycleAdapter, TransactionObligationFulfiller
	{
		 private readonly UpdatePuller _updatePuller;
		 private readonly RoleListener _listener;
		 private readonly HighAvailabilityMemberStateMachine _memberStateMachine;
		 private readonly System.Func<TransactionIdStore> _transactionIdStoreSupplier;

		 private volatile TransactionIdStore _transactionIdStore;

		 public UpdatePullingTransactionObligationFulfiller( UpdatePuller updatePuller, HighAvailabilityMemberStateMachine memberStateMachine, InstanceId serverId, System.Func<TransactionIdStore> transactionIdStoreSupplier )
		 {
			  this._updatePuller = updatePuller;
			  this._memberStateMachine = memberStateMachine;
			  this._transactionIdStoreSupplier = transactionIdStoreSupplier;
			  this._listener = new RoleListener( this, serverId );
		 }

		 /// <summary>
		 /// Triggers pulling of updates up until at least {@code toTxId} if no pulling is currently happening
		 /// and returns immediately.
		 /// </summary>
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void fulfill(final long toTxId) throws InterruptedException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 public override void Fulfill( long toTxId )
		 {
			  _updatePuller.pullUpdates((currentTicket, targetTicket) =>
			  {
				/*
				 * We need to await last *closed* transaction id, not last *committed* transaction id since
				 * right after leaving this method we might read records off of disk, and they had better
				 * be up to date, otherwise we read stale data.
				 */
				return _transactionIdStore != null && _transactionIdStore.LastClosedTransactionId >= toTxId;
			  }, true);
		 }

		 public override void Start()
		 {
			  _memberStateMachine.addHighAvailabilityMemberListener( _listener );
		 }

		 public override void Stop()
		 {
			  _memberStateMachine.removeHighAvailabilityMemberListener( _listener );
		 }

		 private class RoleListener : Org.Neo4j.Kernel.ha.cluster.HighAvailabilityMemberListener_Adapter
		 {
			 private readonly UpdatePullingTransactionObligationFulfiller _outerInstance;

			  internal readonly InstanceId MyInstanceId;

			  internal RoleListener( UpdatePullingTransactionObligationFulfiller outerInstance, InstanceId myInstanceId )
			  {
				  this._outerInstance = outerInstance;
					this.MyInstanceId = myInstanceId;
			  }

			  public override void SlaveIsAvailable( HighAvailabilityMemberChangeEvent @event )
			  {
					if ( @event.InstanceId.Equals( MyInstanceId ) )
					{
						 // I'm a slave, let the transactions stream in

						 // Pull out the transaction id store at this very moment, because we receive this event
						 // when joining a cluster or switching to a new master and there might have been a store copy
						 // just now where there has been a new transaction id store created.
						 outerInstance.transactionIdStore = outerInstance.transactionIdStoreSupplier.get();
					}
			  }

			  public override void InstanceStops( HighAvailabilityMemberChangeEvent @event )
			  {
					// clear state to avoid calling out of date objects
					outerInstance.transactionIdStore = null;
			  }
		 }
	}

}