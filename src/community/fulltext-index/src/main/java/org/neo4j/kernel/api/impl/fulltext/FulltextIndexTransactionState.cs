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
namespace Neo4Net.Kernel.Api.Impl.Fulltext
{
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;


	using IndexReference = Neo4Net.Internal.Kernel.Api.IndexReference;
	using NodeCursor = Neo4Net.Internal.Kernel.Api.NodeCursor;
	using PropertyCursor = Neo4Net.Internal.Kernel.Api.PropertyCursor;
	using RelationshipScanCursor = Neo4Net.Internal.Kernel.Api.RelationshipScanCursor;
	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;
	using IOUtils = Neo4Net.Io.IOUtils;
	using TransactionState = Neo4Net.Kernel.api.txstate.TransactionState;
	using KernelTransactionImplementation = Neo4Net.Kernel.Impl.Api.KernelTransactionImplementation;
	using AllStoreHolder = Neo4Net.Kernel.Impl.Newapi.AllStoreHolder;
	using Log = Neo4Net.Logging.Log;
	using IEntityType = Neo4Net.Storageengine.Api.EntityType;
	using StoreIndexDescriptor = Neo4Net.Storageengine.Api.schema.StoreIndexDescriptor;

	/// <summary>
	/// Manages the transaction state of a specific individual fulltext index, in a given transaction.
	/// <para>
	/// This works by first querying the base index, then filtering out all results that are modified in this transaction, and then querying an in-memory Lucene
	/// index, where the transaction state is indexed. This all happens in the <seealso cref="TransactionStateFulltextIndexReader"/>.
	/// </para>
	/// <para>
	/// The transaction state is indexed prior to querying whenever we detect that the
	/// <seealso cref="KernelTransactionImplementation.getTransactionDataRevision() transaction data revision"/> has changed.
	/// </para>
	/// <para>
	/// The actual transaction state indexing is done by the <seealso cref="FulltextIndexTransactionStateVisitor"/>, which for the most part only looks at the ids, and then
	/// loads the modified entities up through the existing transaction state, via the <seealso cref="AllStoreHolder"/> API.
	/// </para>
	/// </summary>
	internal class FulltextIndexTransactionState : System.IDisposable
	{
		 private readonly FulltextIndexDescriptor _descriptor;
		 private readonly IList<IDisposable> _toCloseLater;
		 private readonly MutableLongSet _modifiedEntityIdsInThisTransaction;
		 private readonly TransactionStateLuceneIndexWriter _writer;
		 private readonly FulltextIndexTransactionStateVisitor _txStateVisitor;
		 private readonly bool _visitingNodes;
		 private long _lastUpdateRevision;
		 private FulltextIndexReader _currentReader;

		 internal FulltextIndexTransactionState( FulltextIndexProvider provider, Log log, IndexReference indexReference )
		 {
			  FulltextIndexAccessor accessor = provider.GetOpenOnlineAccessor( ( StoreIndexDescriptor ) indexReference );
			  log.Debug( "Acquired online fulltext schema index accessor, as base accessor for transaction state: %s", accessor );
			  _descriptor = accessor.Descriptor;
			  SchemaDescriptor schema = _descriptor.schema();
			  _toCloseLater = new List<IDisposable>();
			  _writer = accessor.TransactionStateIndexWriter;
			  _modifiedEntityIdsInThisTransaction = new LongHashSet();
			  _visitingNodes = Schema.entityType() == IEntityType.NODE;
			  _txStateVisitor = new FulltextIndexTransactionStateVisitor( _descriptor, _modifiedEntityIdsInThisTransaction, _writer );
		 }

		 internal virtual FulltextIndexReader GetIndexReader( KernelTransactionImplementation kti )
		 {
			  if ( _currentReader == null || _lastUpdateRevision != kti.TransactionDataRevision )
			  {
					if ( _currentReader != null )
					{
						 _toCloseLater.Add( _currentReader );
					}
					try
					{
						 UpdateReader( kti );
					}
					catch ( Exception e )
					{
						 _currentReader = null;
						 throw new Exception( "Failed to update the fulltext schema index transaction state.", e );
					}
			  }
			  return _currentReader;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void updateReader(org.Neo4Net.kernel.impl.api.KernelTransactionImplementation kti) throws Exception
		 private void UpdateReader( KernelTransactionImplementation kti )
		 {
			  _modifiedEntityIdsInThisTransaction.clear(); // Clear this so we don't filter out entities who have had their changes reversed since last time.
			  _writer.resetWriterState();
			  AllStoreHolder read = ( AllStoreHolder ) kti.DataRead();
			  TransactionState transactionState = kti.TxState();

			  using ( NodeCursor nodeCursor = _visitingNodes ? kti.Cursors().allocateNodeCursor() : null, RelationshipScanCursor relationshipCursor = _visitingNodes ? null : kti.Cursors().allocateRelationshipScanCursor(), PropertyCursor propertyCursor = kti.Cursors().allocatePropertyCursor() )
			  {
					transactionState.Accept( _txStateVisitor.init( read, nodeCursor, relationshipCursor, propertyCursor ) );
			  }
			  FulltextIndexReader baseReader = ( FulltextIndexReader ) read.IndexReader( _descriptor, false );
			  FulltextIndexReader nearRealTimeReader = _writer.NearRealTimeReader;
			  _currentReader = new TransactionStateFulltextIndexReader( baseReader, nearRealTimeReader, _modifiedEntityIdsInThisTransaction );
			  _lastUpdateRevision = kti.TransactionDataRevision;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  _toCloseLater.Add( _currentReader );
			  _toCloseLater.Add( _writer );
			  IOUtils.closeAll( _toCloseLater );
		 }
	}

}