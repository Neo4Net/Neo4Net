using System;
using System.Collections.Generic;

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
namespace Neo4Net.Kernel.impl.transaction.command
{

	using IndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.schema.IndexNotFoundKernelException;
	using IndexActivationFailedKernelException = Neo4Net.Kernel.Api.Exceptions.index.IndexActivationFailedKernelException;
	using IndexPopulationFailedKernelException = Neo4Net.Kernel.Api.Exceptions.index.IndexPopulationFailedKernelException;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;

	/// <summary>
	/// Delayed activation of indexes. At the point in time when a transaction that creates a uniqueness constraint
	/// commits it may be that some low-level locks are held on nodes/relationships, locks which prevents the backing index from being
	/// fully populated. Those locks are released after the appliers have been closed. This activator acts as a register for indexes
	/// that wants to be activated inside an applier, to be activated right after the low-level locks have been released for the batch
	/// of transactions currently applying.
	/// </summary>
	public class IndexActivator : AutoCloseable
	{
		 private readonly IndexingService _indexingService;
		 private ISet<long> _indexesToActivate;

		 public IndexActivator( IndexingService indexingService )
		 {
			  this._indexingService = indexingService;
		 }

		 /// <summary>
		 /// Activates any index that needs activation, i.e. have been added with <seealso cref="activateIndex(long)"/>.
		 /// </summary>
		 public override void Close()
		 {
			  if ( _indexesToActivate != null )
			  {
					foreach ( long indexId in _indexesToActivate )
					{
						 try
						 {
							  _indexingService.activateIndex( indexId );
						 }
						 catch ( Exception e ) when ( e is IndexNotFoundKernelException || e is IndexActivationFailedKernelException || e is IndexPopulationFailedKernelException )
						 {
							  throw new System.InvalidOperationException( "Unable to enable constraint, backing index is not online.", e );
						 }
					}
			  }
		 }

		 /// <summary>
		 /// Makes a note to activate index after batch of transaction have been applied, i.e. in <seealso cref="close()"/>. </summary>
		 /// <param name="indexId"> index id. </param>
		 internal virtual void ActivateIndex( long indexId )
		 {
			  if ( _indexesToActivate == null )
			  {
					_indexesToActivate = new HashSet<long>();
			  }
			  _indexesToActivate.Add( indexId );
		 }

		 /// <summary>
		 /// Called when an index is dropped, so that a previously noted index to activate is removed from this internal list. </summary>
		 /// <param name="indexId"> index id. </param>
		 internal virtual void IndexDropped( long indexId )
		 {
			  if ( _indexesToActivate != null )
			  {
					_indexesToActivate.remove( indexId );
			  }
		 }
	}

}