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
	using Term = Org.Apache.Lucene.Index.Term;
	using Test = org.junit.jupiter.api.Test;

	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;
	using LuceneIndexWriter = Neo4Net.Kernel.Api.Impl.Schema.writer.LuceneIndexWriter;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using DefaultNonUniqueIndexSampler = Neo4Net.Kernel.Impl.Api.index.sampling.DefaultNonUniqueIndexSampler;
	using NonUniqueIndexSampler = Neo4Net.Kernel.Impl.Api.index.sampling.NonUniqueIndexSampler;
	using IndexSample = Neo4Net.Storageengine.Api.schema.IndexSample;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasToString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.hamcrest.MockitoHamcrest.argThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.impl.LuceneTestUtil.documentRepresentingProperties;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.impl.schema.LuceneDocumentStructure.newTermForChangeOrRemove;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.index.IndexQueryHelper.add;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.index.IndexQueryHelper.change;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.api.index.IndexQueryHelper.remove;

	internal class NonUniqueDatabaseIndexPopulatingUpdaterTest
	{
		 private static readonly SchemaDescriptor _schemaDescriptor = SchemaDescriptorFactory.forLabel( 1, 42 );
		 private const int SAMPLING_BUFFER_SIZE_LIMIT = 100;
		 private static readonly SchemaDescriptor _compositeSchemaDescriptor = SchemaDescriptorFactory.forLabel( 1, 42, 43 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addedNodePropertiesIncludedInSample() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void AddedNodePropertiesIncludedInSample()
		 {
			  NonUniqueIndexSampler sampler = NewSampler();
			  NonUniqueLuceneIndexPopulatingUpdater updater = NewUpdater( sampler );

			  updater.Process( add( 1, _schemaDescriptor, "foo" ) );
			  updater.Process( add( 2, _schemaDescriptor, "bar" ) );
			  updater.Process( add( 3, _schemaDescriptor, "baz" ) );
			  updater.Process( add( 4, _schemaDescriptor, "bar" ) );

			  VerifySamplingResult( sampler, 4, 3, 4 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addedNodeCompositePropertiesIncludedInSample() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void AddedNodeCompositePropertiesIncludedInSample()
		 {
			  NonUniqueIndexSampler sampler = NewSampler();
			  NonUniqueLuceneIndexPopulatingUpdater updater = NewUpdater( sampler );
			  updater.Process( add( 1, _compositeSchemaDescriptor, "bit", "foo" ) );
			  updater.Process( add( 2, _compositeSchemaDescriptor, "bit", "bar" ) );
			  updater.Process( add( 3, _compositeSchemaDescriptor, "bit", "baz" ) );
			  updater.Process( add( 4, _compositeSchemaDescriptor, "bit", "bar" ) );

			  VerifySamplingResult( sampler, 4, 3, 4 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void changedNodePropertiesIncludedInSample() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ChangedNodePropertiesIncludedInSample()
		 {
			  NonUniqueIndexSampler sampler = NewSampler();
			  NonUniqueLuceneIndexPopulatingUpdater updater = NewUpdater( sampler );

			  updater.Process( add( 1, _schemaDescriptor, "initial1" ) );
			  updater.Process( add( 2, _schemaDescriptor, "initial2" ) );
			  updater.Process( add( 3, _schemaDescriptor, "new2" ) );

			  updater.Process( change( 1, _schemaDescriptor, "initial1", "new1" ) );
			  updater.Process( change( 1, _schemaDescriptor, "initial2", "new2" ) );

			  VerifySamplingResult( sampler, 3, 2, 3 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void changedNodeCompositePropertiesIncludedInSample() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ChangedNodeCompositePropertiesIncludedInSample()
		 {
			  NonUniqueIndexSampler sampler = NewSampler();
			  NonUniqueLuceneIndexPopulatingUpdater updater = NewUpdater( sampler );

			  updater.Process( add( 1, _compositeSchemaDescriptor, "bit", "initial1" ) );
			  updater.Process( add( 2, _compositeSchemaDescriptor, "bit", "initial2" ) );
			  updater.Process( add( 3, _compositeSchemaDescriptor, "bit", "new2" ) );

			  updater.Process( change( 1, _compositeSchemaDescriptor, new object[]{ "bit", "initial1" }, new object[]{ "bit", "new1" } ) );
			  updater.Process( change( 1, _compositeSchemaDescriptor, new object[]{ "bit", "initial2" }, new object[]{ "bit", "new2" } ) );

			  VerifySamplingResult( sampler, 3, 2, 3 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void removedNodePropertyIncludedInSample() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void RemovedNodePropertyIncludedInSample()
		 {
			  NonUniqueIndexSampler sampler = NewSampler();
			  NonUniqueLuceneIndexPopulatingUpdater updater = NewUpdater( sampler );

			  updater.Process( add( 1, _schemaDescriptor, "foo" ) );
			  updater.Process( add( 2, _schemaDescriptor, "bar" ) );
			  updater.Process( add( 3, _schemaDescriptor, "baz" ) );
			  updater.Process( add( 4, _schemaDescriptor, "qux" ) );

			  updater.Process( remove( 1, _schemaDescriptor, "foo" ) );
			  updater.Process( remove( 2, _schemaDescriptor, "bar" ) );
			  updater.Process( remove( 4, _schemaDescriptor, "qux" ) );

			  VerifySamplingResult( sampler, 1, 1, 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void removedNodeCompositePropertyIncludedInSample() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void RemovedNodeCompositePropertyIncludedInSample()
		 {
			  NonUniqueIndexSampler sampler = NewSampler();
			  NonUniqueLuceneIndexPopulatingUpdater updater = NewUpdater( sampler );

			  updater.Process( add( 1, _compositeSchemaDescriptor, "bit", "foo" ) );
			  updater.Process( add( 2, _compositeSchemaDescriptor, "bit", "bar" ) );
			  updater.Process( add( 3, _compositeSchemaDescriptor, "bit", "baz" ) );
			  updater.Process( add( 4, _compositeSchemaDescriptor, "bit", "qux" ) );

			  updater.Process( remove( 1, _compositeSchemaDescriptor, "bit", "foo" ) );
			  updater.Process( remove( 2, _compositeSchemaDescriptor, "bit", "bar" ) );
			  updater.Process( remove( 4, _compositeSchemaDescriptor, "bit", "qux" ) );

			  VerifySamplingResult( sampler, 1, 1, 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nodePropertyUpdatesIncludedInSample() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void NodePropertyUpdatesIncludedInSample()
		 {
			  NonUniqueIndexSampler sampler = NewSampler();
			  NonUniqueLuceneIndexPopulatingUpdater updater = NewUpdater( sampler );

			  updater.Process( add( 1, _schemaDescriptor, "foo" ) );
			  updater.Process( change( 1, _schemaDescriptor, "foo", "newFoo1" ) );

			  updater.Process( add( 2, _schemaDescriptor, "bar" ) );
			  updater.Process( remove( 2, _schemaDescriptor, "bar" ) );

			  updater.Process( change( 1, _schemaDescriptor, "newFoo1", "newFoo2" ) );

			  updater.Process( add( 42, _schemaDescriptor, "qux" ) );
			  updater.Process( add( 3, _schemaDescriptor, "bar" ) );
			  updater.Process( add( 4, _schemaDescriptor, "baz" ) );
			  updater.Process( add( 5, _schemaDescriptor, "bar" ) );
			  updater.Process( remove( 42, _schemaDescriptor, "qux" ) );

			  VerifySamplingResult( sampler, 4, 3, 4 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nodeCompositePropertyUpdatesIncludedInSample() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void NodeCompositePropertyUpdatesIncludedInSample()
		 {
			  NonUniqueIndexSampler sampler = NewSampler();
			  NonUniqueLuceneIndexPopulatingUpdater updater = NewUpdater( sampler );

			  updater.Process( add( 1, _compositeSchemaDescriptor, "bit", "foo" ) );
			  updater.Process( change( 1, _compositeSchemaDescriptor, new object[]{ "bit", "foo" }, new object[]{ "bit", "newFoo1" } ) );

			  updater.Process( add( 2, _compositeSchemaDescriptor, "bit", "bar" ) );
			  updater.Process( remove( 2, _compositeSchemaDescriptor, "bit", "bar" ) );

			  updater.Process( change( 1, _compositeSchemaDescriptor, new object[]{ "bit", "newFoo1" }, new object[]{ "bit", "newFoo2" } ) );

			  updater.Process( add( 42, _compositeSchemaDescriptor, "bit", "qux" ) );
			  updater.Process( add( 3, _compositeSchemaDescriptor, "bit", "bar" ) );
			  updater.Process( add( 4, _compositeSchemaDescriptor, "bit", "baz" ) );
			  updater.Process( add( 5, _compositeSchemaDescriptor, "bit", "bar" ) );
			  updater.Process( remove( 42, _compositeSchemaDescriptor, "bit", "qux" ) );

			  VerifySamplingResult( sampler, 4, 3, 4 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void additionsDeliveredToIndexWriter() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void AdditionsDeliveredToIndexWriter()
		 {
			  LuceneIndexWriter writer = mock( typeof( LuceneIndexWriter ) );
			  NonUniqueLuceneIndexPopulatingUpdater updater = NewUpdater( writer );

			  string expectedString1 = documentRepresentingProperties( 1, "foo" ).ToString();
			  string expectedString2 = documentRepresentingProperties( 2, "bar" ).ToString();
			  string expectedString3 = documentRepresentingProperties( 3, "qux" ).ToString();
			  string expectedString4 = documentRepresentingProperties( 4, "git", "bit" ).ToString();

			  updater.Process( add( 1, _schemaDescriptor, "foo" ) );
			  Verifydocument( writer, newTermForChangeOrRemove( 1 ), expectedString1 );

			  updater.Process( add( 2, _schemaDescriptor, "bar" ) );
			  Verifydocument( writer, newTermForChangeOrRemove( 2 ), expectedString2 );

			  updater.Process( add( 3, _schemaDescriptor, "qux" ) );
			  Verifydocument( writer, newTermForChangeOrRemove( 3 ), expectedString3 );

			  updater.Process( add( 4, _compositeSchemaDescriptor, "git", "bit" ) );
			  Verifydocument( writer, newTermForChangeOrRemove( 4 ), expectedString4 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void changesDeliveredToIndexWriter() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ChangesDeliveredToIndexWriter()
		 {
			  LuceneIndexWriter writer = mock( typeof( LuceneIndexWriter ) );
			  NonUniqueLuceneIndexPopulatingUpdater updater = NewUpdater( writer );

			  string expectedString1 = documentRepresentingProperties( 1, "after1" ).ToString();
			  string expectedString2 = documentRepresentingProperties( 2, "after2" ).ToString();
			  string expectedString3 = documentRepresentingProperties( 3, "bit", "after2" ).ToString();

			  updater.Process( change( 1, _schemaDescriptor, "before1", "after1" ) );
			  Verifydocument( writer, newTermForChangeOrRemove( 1 ), expectedString1 );

			  updater.Process( change( 2, _schemaDescriptor, "before2", "after2" ) );
			  Verifydocument( writer, newTermForChangeOrRemove( 2 ), expectedString2 );

			  updater.Process( change( 3, _compositeSchemaDescriptor, new object[]{ "bit", "before2" }, new object[]{ "bit", "after2" } ) );
			  Verifydocument( writer, newTermForChangeOrRemove( 3 ), expectedString3 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void removalsDeliveredToIndexWriter() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void RemovalsDeliveredToIndexWriter()
		 {
			  LuceneIndexWriter writer = mock( typeof( LuceneIndexWriter ) );
			  NonUniqueLuceneIndexPopulatingUpdater updater = NewUpdater( writer );

			  updater.Process( remove( 1, _schemaDescriptor, "foo" ) );
			  verify( writer ).deleteDocuments( newTermForChangeOrRemove( 1 ) );

			  updater.Process( remove( 2, _schemaDescriptor, "bar" ) );
			  verify( writer ).deleteDocuments( newTermForChangeOrRemove( 2 ) );

			  updater.Process( remove( 3, _schemaDescriptor, "baz" ) );
			  verify( writer ).deleteDocuments( newTermForChangeOrRemove( 3 ) );

			  updater.Process( remove( 4, _compositeSchemaDescriptor, "bit", "baz" ) );
			  verify( writer ).deleteDocuments( newTermForChangeOrRemove( 4 ) );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifydocument(org.Neo4Net.kernel.api.impl.schema.writer.LuceneIndexWriter writer, org.apache.lucene.index.Term eq, String documentString) throws java.io.IOException
		 private void Verifydocument( LuceneIndexWriter writer, Term eq, string documentString )
		 {
			  verify( writer ).updateDocument( eq( eq ), argThat( hasToString( documentString ) ) );
		 }

		 private static void VerifySamplingResult( NonUniqueIndexSampler sampler, long expectedIndexSize, long expectedUniqueValues, long expectedSampleSize )
		 {
			  IndexSample sample = sampler.Result();

			  assertEquals( expectedIndexSize, sample.IndexSize() );
			  assertEquals( expectedUniqueValues, sample.UniqueValues() );
			  assertEquals( expectedSampleSize, sample.SampleSize() );
		 }

		 private static NonUniqueLuceneIndexPopulatingUpdater NewUpdater( NonUniqueIndexSampler sampler )
		 {
			  return NewUpdater( mock( typeof( LuceneIndexWriter ) ), sampler );
		 }

		 private static NonUniqueLuceneIndexPopulatingUpdater NewUpdater( LuceneIndexWriter writer )
		 {
			  return NewUpdater( writer, NewSampler() );
		 }

		 private static NonUniqueLuceneIndexPopulatingUpdater NewUpdater( LuceneIndexWriter writer, NonUniqueIndexSampler sampler )
		 {
			  return new NonUniqueLuceneIndexPopulatingUpdater( writer, sampler );
		 }

		 private static NonUniqueIndexSampler NewSampler()
		 {
			  return new DefaultNonUniqueIndexSampler( SAMPLING_BUFFER_SIZE_LIMIT );
		 }
	}

}