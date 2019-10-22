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
namespace Neo4Net.Index.impl.lucene.@explicit
{
	using Document = org.apache.lucene.document.Document;
	using IndexWriter = Org.Apache.Lucene.Index.IndexWriter;
	using LongObjectMap = org.eclipse.collections.api.map.primitive.LongObjectMap;
	using MutableLongObjectMap = org.eclipse.collections.api.map.primitive.MutableLongObjectMap;
	using LongObjectHashMap = org.eclipse.collections.impl.map.mutable.primitive.LongObjectHashMap;


	using ExplicitIndexNotFoundKernelException = Neo4Net.Internal.Kernel.Api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException;
	using IndexCommand = Neo4Net.Kernel.impl.index.IndexCommand;

	/// <summary>
	/// This presents a context for each <seealso cref="IndexCommand"/> when they are
	/// committing its data.
	/// </summary>
	internal class CommitContext : System.IDisposable
	{
		 internal readonly LuceneDataSource DataSource;
		 internal readonly IndexIdentifier Identifier;
		 internal readonly IndexType IndexType;
		 internal readonly MutableLongObjectMap<DocumentContext> Documents = new LongObjectHashMap<DocumentContext>();
		 internal readonly bool Recovery;

		 internal IndexReference Searcher;
		 internal IndexWriter Writer;

		 internal CommitContext( LuceneDataSource dataSource, IndexIdentifier identifier, IndexType indexType, bool isRecovery )
		 {
			  this.DataSource = dataSource;
			  this.Identifier = identifier;
			  this.IndexType = indexType;
			  this.Recovery = isRecovery;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: void ensureWriterInstantiated() throws org.Neo4Net.internal.kernel.api.exceptions.explicitindex.ExplicitIndexNotFoundKernelException
		 internal virtual void EnsureWriterInstantiated()
		 {
			  if ( Searcher == null )
			  {
					Searcher = DataSource.getIndexSearcher( Identifier );
					Writer = Searcher.Writer;
			  }
		 }

		 internal virtual DocumentContext GetDocument( IEntityId IEntityId, bool allowCreate )
		 {
			  long id = IEntityId.Id();
			  DocumentContext context = Documents.get( id );
			  if ( context != null )
			  {
					return context;
			  }

			  Document document = LuceneDataSource.FindDocument( IndexType, Searcher.Searcher, id );
			  if ( document != null )
			  {
					context = new DocumentContext( document, true, id );
					Documents.put( id, context );
			  }
			  else if ( allowCreate )
			  {
					context = new DocumentContext( IndexType.NewDocument( IEntityId ), false, id );
					Documents.put( id, context );
			  }
			  return context;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void applyDocuments(org.apache.lucene.index.IndexWriter writer, IndexType type, org.eclipse.collections.api.map.primitive.LongObjectMap<DocumentContext> documents) throws java.io.IOException
		 private void ApplyDocuments( IndexWriter writer, IndexType type, LongObjectMap<DocumentContext> documents )
		 {
			  foreach ( DocumentContext context in documents )
			  {
					if ( context.Exists )
					{
						 if ( LuceneDataSource.DocumentIsEmpty( context.Document ) )
						 {
							  writer.deleteDocuments( type.IdTerm( context.EntityId ) );
						 }
						 else
						 {
							  writer.updateDocument( type.IdTerm( context.EntityId ), context.Document );
						 }
					}
					else
					{
						 writer.addDocument( context.Document );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void close() throws java.io.IOException
		 public override void Close()
		 {
			  ApplyDocuments( Writer, IndexType, Documents );
			  if ( Writer != null )
			  {
					DataSource.invalidateIndexSearcher( Identifier );
			  }
			  if ( Searcher != null )
			  {
					Searcher.close();
			  }
		 }

		 internal class DocumentContext
		 {
			  internal readonly Document Document;
			  internal readonly bool Exists;
			  internal readonly long IEntityId;

			  internal DocumentContext( Document document, bool exists, long IEntityId )
			  {
					this.Document = document;
					this.Exists = exists;
					this.EntityId = IEntityId;
			  }

			  public override string ToString()
			  {
					return "DocumentContext[document=" + Document + ", exists=" + Exists + ", IEntityId=" + IEntityId + "]";
			  }
		 }
	}

}