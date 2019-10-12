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
namespace Org.Neo4j.Kernel.Api.Impl.Schema
{

	using Org.Neo4j.Helpers.Collection;
	using IndexEntryConflictException = Org.Neo4j.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using Org.Neo4j.Kernel.Api.Impl.Index;
	using IndexUpdater = Org.Neo4j.Kernel.Api.Index.IndexUpdater;
	using NodePropertyAccessor = Org.Neo4j.Storageengine.Api.NodePropertyAccessor;
	using LuceneIndexValueValidator = Org.Neo4j.Kernel.Impl.Api.LuceneIndexValueValidator;
	using IndexUpdateMode = Org.Neo4j.Kernel.Impl.Api.index.IndexUpdateMode;
	using IndexDescriptor = Org.Neo4j.Storageengine.Api.schema.IndexDescriptor;
	using IndexReader = Org.Neo4j.Storageengine.Api.schema.IndexReader;
	using Value = Org.Neo4j.Values.Storable.Value;

	public class LuceneIndexAccessor : AbstractLuceneIndexAccessor<IndexReader, SchemaIndex>
	{

		 public LuceneIndexAccessor( SchemaIndex luceneIndex, IndexDescriptor descriptor ) : base( luceneIndex, descriptor )
		 {
		 }

		 protected internal override IndexUpdater GetIndexUpdater( IndexUpdateMode mode )
		 {
			  return new LuceneSchemaIndexUpdater( this, mode.requiresIdempotency(), mode.requiresRefresh() );
		 }

		 public override BoundedIterable<long> NewAllEntriesReader()
		 {
			  return base.NewAllEntriesReader( LuceneDocumentStructure.getNodeId );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: public void verifyDeferredConstraints(org.neo4j.storageengine.api.NodePropertyAccessor nodePropertyAccessor) throws org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 public override void VerifyDeferredConstraints( NodePropertyAccessor nodePropertyAccessor )
		 {
			  try
			  {
					LuceneIndex.verifyUniqueness( nodePropertyAccessor, Descriptor.schema().PropertyIds );
			  }
			  catch ( IOException e )
			  {
					throw new UncheckedIOException( e );
			  }
		 }

		 public override void ValidateBeforeCommit( Value[] tuple )
		 {
			  // In Lucene all values in a tuple (composite index) will be placed in a separate field, so validate their fields individually.
			  foreach ( Value value in tuple )
			  {
					LuceneIndexValueValidator.INSTANCE.validate( value );
			  }
		 }

		 private class LuceneSchemaIndexUpdater : AbstractLuceneIndexUpdater
		 {
			 private readonly LuceneIndexAccessor _outerInstance;


			  protected internal LuceneSchemaIndexUpdater( LuceneIndexAccessor outerInstance, bool idempotent, bool refresh ) : base( outerInstance, idempotent, refresh )
			  {
				  this._outerInstance = outerInstance;
			  }

			  protected internal override void AddIdempotent( long nodeId, Value[] values )
			  {
					try
					{
						 outerInstance.Writer.updateDocument( LuceneDocumentStructure.NewTermForChangeOrRemove( nodeId ), LuceneDocumentStructure.DocumentRepresentingProperties( nodeId, values ) );
					}
					catch ( IOException e )
					{
						 throw new UncheckedIOException( e );
					}
			  }

			  protected internal override void Add( long nodeId, Value[] values )
			  {
					try
					{
						 outerInstance.Writer.addDocument( LuceneDocumentStructure.DocumentRepresentingProperties( nodeId, values ) );
					}
					catch ( IOException e )
					{
						 throw new UncheckedIOException( e );
					}
			  }

			  protected internal override void Change( long nodeId, Value[] values )
			  {
					try
					{
						 outerInstance.Writer.updateDocument( LuceneDocumentStructure.NewTermForChangeOrRemove( nodeId ), LuceneDocumentStructure.DocumentRepresentingProperties( nodeId, values ) );
					}
					catch ( IOException e )
					{
						 throw new UncheckedIOException( e );
					}
			  }

			  protected internal override void Remove( long nodeId )
			  {
					try
					{
						 outerInstance.Writer.deleteDocuments( LuceneDocumentStructure.NewTermForChangeOrRemove( nodeId ) );
					}
					catch ( IOException e )
					{
						 throw new UncheckedIOException( e );
					}
			  }
		 }
	}

}