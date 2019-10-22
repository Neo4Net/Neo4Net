using System.Collections.Generic;
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
namespace Neo4Net.Kernel.Api.Impl.Fulltext
{
	using Document = org.apache.lucene.document.Document;


	using Neo4Net.Functions;
	using Neo4Net.Kernel.Api.Impl.Index;
	using Neo4Net.Kernel.Api.Impl.Schema.populator;
	using Neo4Net.Kernel.Api.Index;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using IndexSample = Neo4Net.Storageengine.Api.schema.IndexSample;
	using Value = Neo4Net.Values.Storable.Value;
	using Values = Neo4Net.Values.Storable.Values;

	public class FulltextIndexPopulator : LuceneIndexPopulator<DatabaseIndex<FulltextIndexReader>>
	{
		 private readonly FulltextIndexDescriptor _descriptor;
		 private readonly ThrowingAction<IOException> _descriptorCreateAction;

		 public FulltextIndexPopulator( FulltextIndexDescriptor descriptor, DatabaseIndex<FulltextIndexReader> luceneFulltext, ThrowingAction<IOException> descriptorCreateAction ) : base( luceneFulltext )
		 {
			  this._descriptor = descriptor;
			  this._descriptorCreateAction = descriptorCreateAction;
		 }

		 public override void Create()
		 {
			  base.Create();
			  try
			  {
					_descriptorCreateAction.apply();
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override void Add<T1>( ICollection<T1> updates ) where T1 : Neo4Net.Kernel.Api.Index.IndexEntryUpdate<T1>
		 {
			  try
			  {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.Neo4Net.kernel.api.index.IndexEntryUpdate<?> update : updates)
					foreach ( IndexEntryUpdate<object> update in updates )
					{
						 Writer.updateDocument( LuceneFulltextDocumentStructure.NewTermForChangeOrRemove( update.EntityId ), UpdateAsDocument( update ) );
					}
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override void VerifyDeferredConstraints( NodePropertyAccessor propertyAccessor )
		 {
			  //Fulltext index does not care about constraints.
		 }

		 public override IndexUpdater NewPopulatingUpdater( NodePropertyAccessor accessor )
		 {
			  return new PopulatingFulltextIndexUpdater( this );
		 }

		 public override void IncludeSample<T1>( IndexEntryUpdate<T1> update )
		 {
			  //Index sampling is not our thing, really.
		 }

		 public override IndexSample SampleResult()
		 {
			  return new IndexSample();
		 }

		 public override IDictionary<string, Value> IndexConfig()
		 {
			  IDictionary<string, Value> map = new Dictionary<string, Value>();
			  map[FulltextIndexSettings.INDEX_CONFIG_ANALYZER] = Values.stringValue( _descriptor.analyzerName() );
			  map[FulltextIndexSettings.INDEX_CONFIG_EVENTUALLY_CONSISTENT] = Values.booleanValue( _descriptor.EventuallyConsistent );
			  return map;
		 }

		 private Document UpdateAsDocument<T1>( IndexEntryUpdate<T1> update )
		 {
			  return LuceneFulltextDocumentStructure.DocumentRepresentingProperties( update.EntityId, _descriptor.propertyNames(), update.Values() );
		 }

		 private class PopulatingFulltextIndexUpdater : IndexUpdater
		 {
			 private readonly FulltextIndexPopulator _outerInstance;

			 public PopulatingFulltextIndexUpdater( FulltextIndexPopulator outerInstance )
			 {
				 this._outerInstance = outerInstance;
			 }

			  public override void Process<T1>( IndexEntryUpdate<T1> update )
			  {
					Debug.Assert( update.IndexKey().schema().Equals(outerInstance.descriptor.Schema()) );
					try
					{
						 switch ( update.UpdateMode() )
						 {
						 case ADDED:
							  long nodeId = update.EntityId;
							  outerInstance.LuceneIndex.IndexWriter.updateDocument( LuceneFulltextDocumentStructure.NewTermForChangeOrRemove( nodeId ), LuceneFulltextDocumentStructure.DocumentRepresentingProperties( nodeId, outerInstance.descriptor.PropertyNames(), update.Values() ) );

							 goto case CHANGED;
						 case CHANGED:
							  long nodeId1 = update.EntityId;
							  outerInstance.LuceneIndex.IndexWriter.updateDocument( LuceneFulltextDocumentStructure.NewTermForChangeOrRemove( nodeId1 ), LuceneFulltextDocumentStructure.DocumentRepresentingProperties( nodeId1, outerInstance.descriptor.PropertyNames(), update.Values() ) );
							  break;
						 case REMOVED:
							  outerInstance.LuceneIndex.IndexWriter.deleteDocuments( LuceneFulltextDocumentStructure.NewTermForChangeOrRemove( update.EntityId ) );
							  break;
						 default:
							  throw new System.NotSupportedException();
						 }
					}
					catch ( IOException e )
					{
						 throw new UncheckedIOException( e );
					}
			  }

			  public override void Close()
			  {
			  }
		 }
	}

}