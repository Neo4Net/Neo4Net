using System.Collections.Generic;
using System.Threading;

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
	using Document = org.apache.lucene.document.Document;


	using Neo4Net.Collections.Helpers;
	using Neo4Net.Kernel.Api.Impl.Index;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using IndexUpdateMode = Neo4Net.Kernel.Impl.Api.index.IndexUpdateMode;
	using NodePropertyAccessor = Neo4Net.Kernel.Api.StorageEngine.NodePropertyAccessor;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.impl.fulltext.LuceneFulltextDocumentStructure.documentRepresentingProperties;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.impl.fulltext.LuceneFulltextDocumentStructure.newTermForChangeOrRemove;

	public class FulltextIndexAccessor : AbstractLuceneIndexAccessor<FulltextIndexReader, DatabaseFulltextIndex>
	{
		 private readonly IndexUpdateSink _indexUpdateSink;
		 private readonly new FulltextIndexDescriptor _descriptor;
		 private readonly ThreadStart _onClose;

		 public FulltextIndexAccessor( IndexUpdateSink indexUpdateSink, DatabaseFulltextIndex luceneIndex, FulltextIndexDescriptor descriptor, ThreadStart onClose ) : base( luceneIndex, descriptor )
		 {
			  this._indexUpdateSink = indexUpdateSink;
			  this._descriptor = descriptor;
			  this._onClose = onClose;
		 }

		 public virtual FulltextIndexDescriptor Descriptor
		 {
			 get
			 {
				  return _descriptor;
			 }
		 }

		 public override IndexUpdater GetIndexUpdater( IndexUpdateMode mode )
		 {
			  IndexUpdater indexUpdater = new FulltextIndexUpdater( this, mode.requiresIdempotency(), mode.requiresRefresh() );
			  if ( _descriptor.EventuallyConsistent )
			  {
					indexUpdater = new EventuallyConsistentIndexUpdater( LuceneIndex, indexUpdater, _indexUpdateSink );
			  }
			  return indexUpdater;
		 }

		 public override void Close()
		 {
			  try
			  {
					if ( _descriptor.EventuallyConsistent )
					{
						 _indexUpdateSink.awaitUpdateApplication();
					}
					base.Close();
			  }
			  finally
			  {
					_onClose.run();
			  }
		 }

		 public override BoundedIterable<long> NewAllEntriesReader()
		 {
			  return base.NewAllEntriesReader( LuceneFulltextDocumentStructure.getNodeId );
		 }

		 public override void VerifyDeferredConstraints( NodePropertyAccessor propertyAccessor )
		 {
			  //The fulltext index does not care about constraints.
		 }

		 public override IDictionary<string, Value> IndexConfig()
		 {
			  IDictionary<string, Value> map = new Dictionary<string, Value>();
			  map[FulltextIndexSettings.INDEX_CONFIG_ANALYZER] = Values.stringValue( _descriptor.analyzerName() );
			  map[FulltextIndexSettings.INDEX_CONFIG_EVENTUALLY_CONSISTENT] = Values.booleanValue( _descriptor.EventuallyConsistent );
			  return map;
		 }

		 public virtual TransactionStateLuceneIndexWriter TransactionStateIndexWriter
		 {
			 get
			 {
				  try
				  {
						return LuceneIndex.TransactionalIndexWriter;
				  }
				  catch ( IOException e )
				  {
						throw new UncheckedIOException( e );
				  }
			 }
		 }

		 public class FulltextIndexUpdater : AbstractLuceneIndexUpdater
		 {
			 private readonly FulltextIndexAccessor _outerInstance;

			  internal FulltextIndexUpdater( FulltextIndexAccessor outerInstance, bool idempotent, bool refresh ) : base( outerInstance, idempotent, refresh )
			  {
				  this._outerInstance = outerInstance;
			  }

			  protected internal override void AddIdempotent( long IEntityId, Value[] values )
			  {
					try
					{
						 Document document = documentRepresentingProperties( IEntityId, outerInstance.Descriptor.propertyNames(), values );
						 outerInstance.Writer.updateDocument( newTermForChangeOrRemove( IEntityId ), document );
					}
					catch ( IOException e )
					{
						 throw new UncheckedIOException( e );
					}
			  }

			  public override void Add( long IEntityId, Value[] values )
			  {
					try
					{
						 Document document = documentRepresentingProperties( IEntityId, outerInstance.Descriptor.propertyNames(), values );
						 outerInstance.Writer.addDocument( document );
					}
					catch ( IOException e )
					{
						 throw new UncheckedIOException( e );
					}
			  }

			  protected internal override void Change( long IEntityId, Value[] values )
			  {
					try
					{
						 outerInstance.Writer.updateDocument( newTermForChangeOrRemove( IEntityId ), documentRepresentingProperties( IEntityId, outerInstance.Descriptor.propertyNames(), values ) );
					}
					catch ( IOException e )
					{
						 throw new UncheckedIOException( e );
					}
			  }

			  protected internal override void Remove( long IEntityId )
			  {
					try
					{
						 outerInstance.Writer.deleteDocuments( newTermForChangeOrRemove( IEntityId ) );
					}
					catch ( IOException e )
					{
						 throw new UncheckedIOException( e );
					}
			  }
		 }
	}

}