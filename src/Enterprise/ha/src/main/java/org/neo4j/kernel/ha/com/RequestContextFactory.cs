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
namespace Neo4Net.Kernel.ha.com
{

	using RequestContext = Neo4Net.com.RequestContext;
	using TransactionId = Neo4Net.Kernel.impl.store.TransactionId;
	using TransactionIdStore = Neo4Net.Kernel.impl.transaction.log.TransactionIdStore;
	using LifecycleAdapter = Neo4Net.Kernel.Lifecycle.LifecycleAdapter;

	public class RequestContextFactory : LifecycleAdapter
	{
		 private long _epoch;
		 private readonly int _serverId;
		 private readonly System.Func<TransactionIdStore> _txIdStoreSupplier;
		 private TransactionIdStore _txIdStore;

		 public const int DEFAULT_EVENT_IDENTIFIER = -1;

		 public RequestContextFactory( int serverId, System.Func<TransactionIdStore> txIdStoreSupplier )
		 {
			  this._txIdStoreSupplier = txIdStoreSupplier;
			  this._epoch = -1;
			  this._serverId = serverId;
		 }

		 public override void Start()
		 {
			  this._txIdStore = _txIdStoreSupplier.get();
		 }

		 public override void Stop()
		 {
			  this._txIdStore = null;
		 }

		 public virtual long Epoch
		 {
			 set
			 {
				  this._epoch = value;
			 }
		 }

		 public virtual RequestContext NewRequestContext( long epoch, int machineId, int eventIdentifier )
		 {
			  TransactionId lastTx = _txIdStore.LastCommittedTransaction;
			  // TODO beware, there's a race between getting tx id and checksum, and changes to last tx
			  // it must be fixed
			  return new RequestContext( epoch, machineId, eventIdentifier, lastTx.TransactionIdConflict(), lastTx.Checksum() );
		 }

		 public virtual RequestContext NewRequestContext( int eventIdentifier )
		 {
			  return NewRequestContext( _epoch, _serverId, eventIdentifier );
		 }

		 public virtual RequestContext NewRequestContext()
		 {
			  return NewRequestContext( DEFAULT_EVENT_IDENTIFIER );
		 }
	}

}