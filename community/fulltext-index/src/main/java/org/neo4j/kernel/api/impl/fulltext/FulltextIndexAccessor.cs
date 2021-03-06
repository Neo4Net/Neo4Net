﻿using System.Collections.Generic;
using System.Threading;

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
	using Document = org.apache.lucene.document.Document;


	using Org.Neo4j.Helpers.Collection;
	using Org.Neo4j.Kernel.Api.Impl.Index;
	using IndexUpdater = Org.Neo4j.Kernel.Api.Index.IndexUpdater;
	using IndexUpdateMode = Org.Neo4j.Kernel.Impl.Api.index.IndexUpdateMode;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using Value = Org.Neo4j.Values.Storable.Value;
	using Values = Org.Neo4j.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.LuceneFulltextDocumentStructure.documentRepresentingProperties;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.fulltext.LuceneFulltextDocumentStructure.newTermForChangeOrRemove;

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

			  protected internal override void AddIdempotent( long entityId, Value[] values )
			  {
					try
					{
						 Document document = documentRepresentingProperties( entityId, outerInstance.Descriptor.propertyNames(), values );
						 outerInstance.Writer.updateDocument( newTermForChangeOrRemove( entityId ), document );
					}
					catch ( IOException e )
					{
						 throw new UncheckedIOException( e );
					}
			  }

			  public override void Add( long entityId, Value[] values )
			  {
					try
					{
						 Document document = documentRepresentingProperties( entityId, outerInstance.Descriptor.propertyNames(), values );
						 outerInstance.Writer.addDocument( document );
					}
					catch ( IOException e )
					{
						 throw new UncheckedIOException( e );
					}
			  }

			  protected internal override void Change( long entityId, Value[] values )
			  {
					try
					{
						 outerInstance.Writer.updateDocument( newTermForChangeOrRemove( entityId ), documentRepresentingProperties( entityId, outerInstance.Descriptor.propertyNames(), values ) );
					}
					catch ( IOException e )
					{
						 throw new UncheckedIOException( e );
					}
			  }

			  protected internal override void Remove( long entityId )
			  {
					try
					{
						 outerInstance.Writer.deleteDocuments( newTermForChangeOrRemove( entityId ) );
					}
					catch ( IOException e )
					{
						 throw new UncheckedIOException( e );
					}
			  }
		 }
	}

}