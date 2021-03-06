﻿using System.Collections.Generic;

/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
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
namespace Org.Neo4j.Kernel.Api.Impl.Fulltext
{

	using IndexReference = Org.Neo4j.@internal.Kernel.Api.IndexReference;
	using IOUtils = Org.Neo4j.Io.IOUtils;
	using AuxiliaryTransactionState = Org.Neo4j.Kernel.api.txstate.auxiliary.AuxiliaryTransactionState;
	using KernelTransactionImplementation = Org.Neo4j.Kernel.Impl.Api.KernelTransactionImplementation;
	using Log = Org.Neo4j.Logging.Log;
	using StorageCommand = Org.Neo4j.Storageengine.Api.StorageCommand;

	/// <summary>
	/// The fulltext auxiliary transaction state manages the aggregate transaction state of <em>all</em> fulltext indexes in a transaction.
	/// <para>
	/// For the transaction state of the individual fulltext schema index, see the <seealso cref="FulltextIndexTransactionState"/> class.
	/// </para>
	/// </summary>
	internal class FulltextAuxiliaryTransactionState : AuxiliaryTransactionState, System.Func<IndexReference, FulltextIndexTransactionState>
	{
		 private readonly FulltextIndexProvider _provider;
		 private readonly Log _log;
		 private readonly IDictionary<IndexReference, FulltextIndexTransactionState> _indexStates;

		 internal FulltextAuxiliaryTransactionState( FulltextIndexProvider provider, Log log )
		 {
			  this._provider = provider;
			  this._log = log;
			  _indexStates = new Dictionary<IndexReference, FulltextIndexTransactionState>();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws Exception
		 public override void Close()
		 {
			  IOUtils.closeAll( _indexStates.Values );
		 }

		 public override bool HasChanges()
		 {
			  // We always return 'false' here, because we only use this transaction state for reading.
			  //Our index changes are already derived from the store commands, so we never have any commands of our own to extract.
			  return false;
		 }

		 public override void ExtractCommands( ICollection<StorageCommand> target )
		 {
			  // We never have any commands to extract, because this transaction state is only used for reading.
		 }

		 internal virtual FulltextIndexReader IndexReader( IndexReference indexReference, KernelTransactionImplementation kti )
		 {
			  FulltextIndexTransactionState state = _indexStates.computeIfAbsent( indexReference, this );
			  return state.GetIndexReader( kti );
		 }

		 public override FulltextIndexTransactionState Apply( IndexReference indexReference )
		 {
			  return new FulltextIndexTransactionState( _provider, _log, indexReference );
		 }
	}

}