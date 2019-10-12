using System.Collections.Generic;
using System.Diagnostics;

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
namespace Neo4Net.Kernel.Api.Impl.Schema.populator
{
	using Document = org.apache.lucene.document.Document;


	using IOUtils = Neo4Net.Io.IOUtils;
	using Neo4Net.Kernel.Api.Impl.Index;
	using LuceneIndexWriter = Neo4Net.Kernel.Api.Impl.Schema.writer.LuceneIndexWriter;
	using Neo4Net.Kernel.Api.Index;
	using IndexPopulator = Neo4Net.Kernel.Api.Index.IndexPopulator;

	/// <summary>
	/// An <seealso cref="IndexPopulator"/> used to create, populate and mark as online a Lucene schema index.
	/// </summary>
	public abstract class LuceneIndexPopulator<INDEX> : IndexPopulator
	{
		public abstract void PutAllNoOverwrite( IDictionary<string, Neo4Net.Values.Storable.Value> target, IDictionary<string, Neo4Net.Values.Storable.Value> source );
		public abstract IDictionary<string, Neo4Net.Values.Storable.Value> IndexConfig();
		public abstract void ScanCompleted( Neo4Net.Kernel.Impl.Api.index.PhaseTracker phaseTracker );
		public abstract Neo4Net.Storageengine.Api.schema.PopulationProgress Progress( Neo4Net.Storageengine.Api.schema.PopulationProgress scanProgress );
		public abstract Neo4Net.Storageengine.Api.schema.IndexSample SampleResult();
		public abstract void includeSample<T1>( IndexEntryUpdate<T1> update );
		public abstract IndexUpdater NewPopulatingUpdater( Neo4Net.Storageengine.Api.NodePropertyAccessor accessor );
		public abstract void VerifyDeferredConstraints( Neo4Net.Storageengine.Api.NodePropertyAccessor nodePropertyAccessor );
		 protected internal INDEX LuceneIndex;
		 protected internal LuceneIndexWriter Writer;

		 protected internal LuceneIndexPopulator( INDEX luceneIndex )
		 {
			  this.LuceneIndex = luceneIndex;
		 }

		 public override void Create()
		 {
			  try
			  {
					LuceneIndex.create();
					LuceneIndex.open();
					Writer = LuceneIndex.IndexWriter;
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override void Drop()
		 {
			  LuceneIndex.drop();
		 }

		 public override void Add<T1>( ICollection<T1> updates ) where T1 : Neo4Net.Kernel.Api.Index.IndexEntryUpdate<T1>
		 {
			  Debug.Assert( UpdatesForCorrectIndex( updates ) );

			  try
			  {
					// Lucene documents stored in a ThreadLocal and reused so we can't create an eager collection of documents here
					// That is why we create a lazy Iterator and then Iterable
					Writer.addDocuments( updates.Count, () => updates.Select(LuceneIndexPopulator.updateAsDocument).GetEnumerator() );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override void Close( bool populationCompletedSuccessfully )
		 {
			  try
			  {
					if ( populationCompletedSuccessfully )
					{
						 LuceneIndex.markAsOnline();
					}
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
			  finally
			  {
					IOUtils.closeAllSilently( LuceneIndex );
			  }
		 }

		 public override void MarkAsFailed( string failure )
		 {
			  try
			  {
					LuceneIndex.markAsFailed( failure );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 private bool UpdatesForCorrectIndex<T1>( ICollection<T1> updates ) where T1 : Neo4Net.Kernel.Api.Index.IndexEntryUpdate<T1>
		 {
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: for (org.neo4j.kernel.api.index.IndexEntryUpdate<?> update : updates)
			  foreach ( IndexEntryUpdate<object> update in updates )
			  {
					if ( !update.IndexKey().schema().Equals(LuceneIndex.Descriptor.schema()) )
					{
						 return false;
					}
			  }
			  return true;
		 }

		 private static Document UpdateAsDocument<T1>( IndexEntryUpdate<T1> update )
		 {
			  return LuceneDocumentStructure.documentRepresentingProperties( update.EntityId, update.Values() );
		 }
	}

}