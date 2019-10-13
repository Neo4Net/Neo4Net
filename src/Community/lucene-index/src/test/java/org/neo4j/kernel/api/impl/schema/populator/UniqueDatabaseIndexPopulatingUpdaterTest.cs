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
namespace Neo4Net.Kernel.Api.Impl.Schema.populator
{
	using Test = org.junit.jupiter.api.Test;
	using ArgumentCaptor = org.mockito.ArgumentCaptor;


	using SchemaDescriptor = Neo4Net.@internal.Kernel.Api.schema.SchemaDescriptor;
	using IndexEntryConflictException = Neo4Net.Kernel.Api.Exceptions.index.IndexEntryConflictException;
	using LuceneIndexWriter = Neo4Net.Kernel.Api.Impl.Schema.writer.LuceneIndexWriter;
	using NodePropertyAccessor = Neo4Net.Storageengine.Api.NodePropertyAccessor;
	using SchemaDescriptorFactory = Neo4Net.Kernel.api.schema.SchemaDescriptorFactory;
	using UniqueIndexSampler = Neo4Net.Kernel.Impl.Api.index.sampling.UniqueIndexSampler;
	using IndexSample = Neo4Net.Storageengine.Api.schema.IndexSample;
	using Value = Neo4Net.Values.Storable.Value;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsInAnyOrder;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.any;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.ArgumentMatchers.eq;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verify;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.verifyZeroInteractions;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.LuceneTestUtil.documentRepresentingProperties;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.LuceneTestUtil.valueTupleList;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.impl.schema.LuceneDocumentStructure.newTermForChangeOrRemove;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexQueryHelper.add;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexQueryHelper.change;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.api.index.IndexQueryHelper.remove;

	internal class UniqueDatabaseIndexPopulatingUpdaterTest
	{
		 private static readonly SchemaDescriptor _descriptor = SchemaDescriptorFactory.forLabel( 1, 42 );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void closeVerifiesUniquenessOfAddedValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CloseVerifiesUniquenessOfAddedValues()
		 {
			  SchemaIndex index = mock( typeof( SchemaIndex ) );
			  UniqueLuceneIndexPopulatingUpdater updater = NewUpdater( index );

			  updater.Process( add( 1, _descriptor, "foo" ) );
			  updater.Process( add( 1, _descriptor, "bar" ) );
			  updater.Process( add( 1, _descriptor, "baz" ) );

			  verifyZeroInteractions( index );

			  updater.Close();
			  VerifyVerifyUniqueness( index, _descriptor, "foo", "bar", "baz" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void closeVerifiesUniquenessOfChangedValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CloseVerifiesUniquenessOfChangedValues()
		 {
			  SchemaIndex index = mock( typeof( SchemaIndex ) );
			  UniqueLuceneIndexPopulatingUpdater updater = NewUpdater( index );

			  updater.Process( change( 1, _descriptor, "foo1", "foo2" ) );
			  updater.Process( change( 1, _descriptor, "bar1", "bar2" ) );
			  updater.Process( change( 1, _descriptor, "baz1", "baz2" ) );

			  verifyZeroInteractions( index );

			  updater.Close();

			  VerifyVerifyUniqueness( index, _descriptor, "foo2", "bar2", "baz2" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void closeVerifiesUniquenessOfAddedAndChangedValues() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void CloseVerifiesUniquenessOfAddedAndChangedValues()
		 {
			  SchemaIndex index = mock( typeof( SchemaIndex ) );
			  UniqueLuceneIndexPopulatingUpdater updater = NewUpdater( index );

			  updater.Process( add( 1, _descriptor, "added1" ) );
			  updater.Process( add( 2, _descriptor, "added2" ) );
			  updater.Process( change( 3, _descriptor, "before1", "after1" ) );
			  updater.Process( change( 4, _descriptor, "before2", "after2" ) );
			  updater.Process( remove( 5, _descriptor, "removed1" ) );

			  verifyZeroInteractions( index );

			  updater.Close();

			  VerifyVerifyUniqueness( index, _descriptor, "added1", "added2", "after1", "after2" );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void addedNodePropertiesIncludedInSample() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void AddedNodePropertiesIncludedInSample()
		 {
			  UniqueIndexSampler sampler = new UniqueIndexSampler();
			  UniqueLuceneIndexPopulatingUpdater updater = NewUpdater( sampler );

			  updater.Process( add( 1, _descriptor, "foo" ) );
			  updater.Process( add( 2, _descriptor, "bar" ) );
			  updater.Process( add( 3, _descriptor, "baz" ) );
			  updater.Process( add( 4, _descriptor, "qux" ) );

			  VerifySamplingResult( sampler, 4 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void changedNodePropertiesDoNotInfluenceSample() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ChangedNodePropertiesDoNotInfluenceSample()
		 {
			  UniqueIndexSampler sampler = new UniqueIndexSampler();
			  UniqueLuceneIndexPopulatingUpdater updater = NewUpdater( sampler );

			  updater.Process( change( 1, _descriptor, "before1", "after1" ) );
			  updater.Process( change( 2, _descriptor, "before2", "after2" ) );

			  VerifySamplingResult( sampler, 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void removedNodePropertyIncludedInSample() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void RemovedNodePropertyIncludedInSample()
		 {
			  long initialValue = 10;
			  UniqueIndexSampler sampler = new UniqueIndexSampler();
			  sampler.Increment( initialValue );

			  UniqueLuceneIndexPopulatingUpdater updater = NewUpdater( sampler );

			  updater.Process( remove( 1, _descriptor, "removed1" ) );
			  updater.Process( remove( 2, _descriptor, "removed2" ) );

			  VerifySamplingResult( sampler, initialValue - 2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void nodePropertyUpdatesIncludedInSample() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void NodePropertyUpdatesIncludedInSample()
		 {
			  UniqueIndexSampler sampler = new UniqueIndexSampler();
			  UniqueLuceneIndexPopulatingUpdater updater = NewUpdater( sampler );

			  updater.Process( add( 1, _descriptor, "foo" ) );
			  updater.Process( change( 1, _descriptor, "foo", "bar" ) );
			  updater.Process( add( 2, _descriptor, "baz" ) );
			  updater.Process( add( 3, _descriptor, "qux" ) );
			  updater.Process( remove( 4, _descriptor, "qux" ) );

			  VerifySamplingResult( sampler, 2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void additionsDeliveredToIndexWriter() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void AdditionsDeliveredToIndexWriter()
		 {
			  LuceneIndexWriter writer = mock( typeof( LuceneIndexWriter ) );
			  UniqueLuceneIndexPopulatingUpdater updater = NewUpdater( writer );

			  updater.Process( add( 1, _descriptor, "foo" ) );
			  updater.Process( add( 2, _descriptor, "bar" ) );
			  updater.Process( add( 3, _descriptor, "qux" ) );

			  verify( writer ).updateDocument( newTermForChangeOrRemove( 1 ), documentRepresentingProperties( ( long ) 1, "foo" ) );
			  verify( writer ).updateDocument( newTermForChangeOrRemove( 2 ), documentRepresentingProperties( ( long ) 2, "bar" ) );
			  verify( writer ).updateDocument( newTermForChangeOrRemove( 3 ), documentRepresentingProperties( ( long ) 3, "qux" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void changesDeliveredToIndexWriter() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ChangesDeliveredToIndexWriter()
		 {
			  LuceneIndexWriter writer = mock( typeof( LuceneIndexWriter ) );
			  UniqueLuceneIndexPopulatingUpdater updater = NewUpdater( writer );

			  updater.Process( change( 1, _descriptor, "before1", "after1" ) );
			  updater.Process( change( 2, _descriptor, "before2", "after2" ) );

			  verify( writer ).updateDocument( newTermForChangeOrRemove( 1 ), documentRepresentingProperties( ( long ) 1, "after1" ) );
			  verify( writer ).updateDocument( newTermForChangeOrRemove( 2 ), documentRepresentingProperties( ( long ) 2, "after2" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void removalsDeliveredToIndexWriter() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void RemovalsDeliveredToIndexWriter()
		 {
			  LuceneIndexWriter writer = mock( typeof( LuceneIndexWriter ) );
			  UniqueLuceneIndexPopulatingUpdater updater = NewUpdater( writer );

			  updater.Process( remove( 1, _descriptor, "foo" ) );
			  updater.Process( remove( 2, _descriptor, "bar" ) );
			  updater.Process( remove( 3, _descriptor, "baz" ) );

			  verify( writer ).deleteDocuments( newTermForChangeOrRemove( 1 ) );
			  verify( writer ).deleteDocuments( newTermForChangeOrRemove( 2 ) );
			  verify( writer ).deleteDocuments( newTermForChangeOrRemove( 3 ) );
		 }

		 private static void VerifySamplingResult( UniqueIndexSampler sampler, long expectedValue )
		 {
			  IndexSample sample = sampler.Result();

			  assertEquals( expectedValue, sample.IndexSize() );
			  assertEquals( expectedValue, sample.UniqueValues() );
			  assertEquals( expectedValue, sample.SampleSize() );
		 }

		 private static UniqueLuceneIndexPopulatingUpdater NewUpdater()
		 {
			  return NewUpdater( new UniqueIndexSampler() );
		 }

		 private static UniqueLuceneIndexPopulatingUpdater NewUpdater( SchemaIndex index )
		 {
			  return NewUpdater( index, mock( typeof( LuceneIndexWriter ) ), new UniqueIndexSampler() );
		 }

		 private static UniqueLuceneIndexPopulatingUpdater NewUpdater( LuceneIndexWriter writer )
		 {
			  return NewUpdater( mock( typeof( SchemaIndex ) ), writer, new UniqueIndexSampler() );
		 }

		 private static UniqueLuceneIndexPopulatingUpdater NewUpdater( UniqueIndexSampler sampler )
		 {
			  return NewUpdater( mock( typeof( SchemaIndex ) ), mock( typeof( LuceneIndexWriter ) ), sampler );
		 }

		 private static UniqueLuceneIndexPopulatingUpdater NewUpdater( SchemaIndex index, LuceneIndexWriter writer, UniqueIndexSampler sampler )
		 {
			  return new UniqueLuceneIndexPopulatingUpdater( writer, _descriptor.PropertyIds, index, mock( typeof( NodePropertyAccessor ) ), sampler );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void verifyVerifyUniqueness(org.neo4j.kernel.api.impl.schema.SchemaIndex index, org.neo4j.internal.kernel.api.schema.SchemaDescriptor descriptor, Object... values) throws java.io.IOException, org.neo4j.kernel.api.exceptions.index.IndexEntryConflictException
		 private void VerifyVerifyUniqueness( SchemaIndex index, SchemaDescriptor descriptor, params object[] values )
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("unchecked") org.mockito.ArgumentCaptor<java.util.List<org.neo4j.values.storable.Value[]>> captor = org.mockito.ArgumentCaptor.forClass(java.util.List.class);
			  ArgumentCaptor<IList<Value[]>> captor = ArgumentCaptor.forClass( typeof( System.Collections.IList ) );
			  verify( index ).verifyUniqueness( any(), eq(descriptor.PropertyIds), captor.capture() );

			  assertThat( captor.Value, containsInAnyOrder( valueTupleList( values ).toArray() ) );
		 }
	}

}