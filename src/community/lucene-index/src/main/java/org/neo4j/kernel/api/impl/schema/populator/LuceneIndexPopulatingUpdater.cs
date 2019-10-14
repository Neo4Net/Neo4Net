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
namespace Neo4Net.Kernel.Api.Impl.Schema.populator
{
	using Document = org.apache.lucene.document.Document;
	using Term = Org.Apache.Lucene.Index.Term;


	using LuceneIndexWriter = Neo4Net.Kernel.Api.Impl.Schema.writer.LuceneIndexWriter;
	using Neo4Net.Kernel.Api.Index;
	using IndexUpdater = Neo4Net.Kernel.Api.Index.IndexUpdater;
	using UpdateMode = Neo4Net.Kernel.Impl.Api.index.UpdateMode;

	/// <summary>
	/// An <seealso cref="IndexUpdater"/> used while index population is in progress. Takes special care of node property additions
	/// and changes applying them via <seealso cref="LuceneIndexWriter.updateDocument(Term, Document)"/> to make sure no duplicated
	/// documents are inserted.
	/// </summary>
	public abstract class LuceneIndexPopulatingUpdater : IndexUpdater
	{
		public abstract void Close();
		 private readonly LuceneIndexWriter _writer;

		 public LuceneIndexPopulatingUpdater( LuceneIndexWriter writer )
		 {
			  this._writer = writer;
		 }

		 public override void Process<T1>( IndexEntryUpdate<T1> update )
		 {
			  long nodeId = update.EntityId;

			  try
			  {
					switch ( update.UpdateMode() )
					{
					case ADDED:
						 Added( update );
						 _writer.updateDocument( LuceneDocumentStructure.newTermForChangeOrRemove( nodeId ), LuceneDocumentStructure.documentRepresentingProperties( nodeId, update.Values() ) );
						 break;
					case CHANGED:
						 Changed( update );
						 _writer.updateDocument( LuceneDocumentStructure.newTermForChangeOrRemove( nodeId ), LuceneDocumentStructure.documentRepresentingProperties( nodeId, update.Values() ) );
						 break;
					case REMOVED:
						 Removed( update );
						 _writer.deleteDocuments( LuceneDocumentStructure.newTermForChangeOrRemove( nodeId ) );
						 break;
					default:
						 throw new System.InvalidOperationException( "Unknown update mode " + Arrays.ToString( update.Values() ) );
					}
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 /// <summary>
		 /// Method is invoked when <seealso cref="IndexEntryUpdate"/> with <seealso cref="UpdateMode.ADDED"/> is processed.
		 /// </summary>
		 /// <param name="update"> the update being processed. </param>
		 protected internal abstract void added<T1>( IndexEntryUpdate<T1> update );

		 /// <summary>
		 /// Method is invoked when <seealso cref="IndexEntryUpdate"/> with <seealso cref="UpdateMode.CHANGED"/> is processed.
		 /// </summary>
		 /// <param name="update"> the update being processed. </param>
		 protected internal abstract void changed<T1>( IndexEntryUpdate<T1> update );

		 /// <summary>
		 /// Method is invoked when <seealso cref="IndexEntryUpdate"/> with <seealso cref="UpdateMode.REMOVED"/> is processed.
		 /// </summary>
		 /// <param name="update"> the update being processed. </param>
		 protected internal abstract void removed<T1>( IndexEntryUpdate<T1> update );
	}

}