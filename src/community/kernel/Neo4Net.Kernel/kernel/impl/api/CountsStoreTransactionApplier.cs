using System.Diagnostics;

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
namespace Neo4Net.Kernel.Impl.Api
{
	using CountsTracker = Neo4Net.Kernel.impl.store.counts.CountsTracker;
	using Command = Neo4Net.Kernel.impl.transaction.command.Command;
	using SchemaRuleCommand = Neo4Net.Kernel.impl.transaction.command.Command.SchemaRuleCommand;
	using TransactionApplicationMode = Neo4Net.Kernel.Api.StorageEngine.TransactionApplicationMode;

	public class CountsStoreTransactionApplier : TransactionApplier_Adapter
	{
		 private readonly TransactionApplicationMode _mode;
		 private readonly CountsTracker.Updater _countsUpdater;
		 private bool _haveUpdates;

		 public CountsStoreTransactionApplier( TransactionApplicationMode mode, CountsAccessor_Updater countsUpdater )
		 {
			  this._mode = mode;
			  this._countsUpdater = countsUpdater;
		 }

		 public override void Close()
		 {
			  Debug.Assert( _countsUpdater != null || _mode == TransactionApplicationMode.RECOVERY, "You must call begin first" );
			  CloseCountsUpdaterIfOpen();
		 }

		 private void CloseCountsUpdaterIfOpen()
		 {
			  if ( _countsUpdater != null )
			  { // CountsUpdater is null if we're in recovery and the counts store already has had this transaction applied.
					_countsUpdater.close();
			  }
		 }

		 public override bool VisitNodeCountsCommand( Command.NodeCountsCommand command )
		 {
			  Debug.Assert( _countsUpdater != null || _mode == TransactionApplicationMode.RECOVERY, "You must call begin first" );
			  _haveUpdates = true;
			  if ( _countsUpdater != null )
			  { // CountsUpdater is null if we're in recovery and the counts store already has had this transaction applied.
					_countsUpdater.incrementNodeCount( command.LabelId(), command.Delta() );
			  }
			  return false;
		 }

		 public override bool VisitRelationshipCountsCommand( Command.RelationshipCountsCommand command )
		 {
			  Debug.Assert( _countsUpdater != null || _mode == TransactionApplicationMode.RECOVERY, "You must call begin first" );
			  _haveUpdates = true;
			  if ( _countsUpdater != null )
			  { // CountsUpdater is null if we're in recovery and the counts store already has had this transaction applied.
					_countsUpdater.incrementRelationshipCount( command.StartLabelId(), command.TypeId(), command.EndLabelId(), command.Delta() );
			  }
			  return false;
		 }

		 public override bool VisitSchemaRuleCommand( Command.SchemaRuleCommand command )
		 {
			  // This shows that this transaction is a schema transaction, so it cannot have commands
			  // updating any counts anyway. Therefore the counts updater is closed right away.
			  // This also breaks an otherwise deadlocking scenario between check pointer, this applier
			  // and an index population thread wanting to apply index sampling to the counts store.
			  Debug.Assert( !_haveUpdates, "Assumed that a schema transaction wouldn't also contain data commands affecting " + );
						 "counts store, but was proven wrong with this transaction";
			  CloseCountsUpdaterIfOpen();
			  return false;
		 }
	}

}