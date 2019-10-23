using System;

/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Neo4Net.Kernel.impl.transaction.state.storeview
{

	using Neo4Net.Helpers.Collections;
	using LabelScanStore = Neo4Net.Kernel.api.labelscan.LabelScanStore;
	using NodeLabelUpdate = Neo4Net.Kernel.api.labelscan.NodeLabelUpdate;
	using IEntityUpdates = Neo4Net.Kernel.Impl.Api.index.EntityUpdates;
	using LockService = Neo4Net.Kernel.impl.locking.LockService;
	using StorageReader = Neo4Net.Kernel.Api.StorageEngine.StorageReader;

	/// <summary>
	/// Store scan view that will try to minimize amount of scanned nodes by using label scan store <seealso cref="LabelScanStore"/>
	/// as a source of known labeled node ids. </summary>
	/// @param <FAILURE> type of exception thrown on failure </param>
	public class LabelScanViewNodeStoreScan<FAILURE> : StoreViewNodeStoreScan<FAILURE> where FAILURE : Exception
	{
		 private readonly LabelScanStore _labelScanStore;

		 public LabelScanViewNodeStoreScan( StorageReader storageReader, LockService locks, LabelScanStore labelScanStore, Visitor<NodeLabelUpdate, FAILURE> labelUpdateVisitor, Visitor<EntityUpdates, FAILURE> propertyUpdatesVisitor, int[] labelIds, System.Func<int, bool> propertyKeyIdFilter ) : base( storageReader, locks, labelUpdateVisitor, propertyUpdatesVisitor, labelIds, propertyKeyIdFilter )
		 {
			  this._labelScanStore = labelScanStore;
		 }

		 public override IEntityIdIterator IEntityIdIterator
		 {
			 get
			 {
				  return new LabelScanViewIdIterator<>( _labelScanStore.newReader(), labelIds, IEntityCursor );
			 }
		 }
	}

}