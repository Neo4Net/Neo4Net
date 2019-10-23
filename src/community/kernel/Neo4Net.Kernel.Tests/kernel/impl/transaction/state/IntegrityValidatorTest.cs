using System;

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
namespace Neo4Net.Kernel.impl.transaction.state
{
	using Test = org.junit.Test;

	using ConstraintValidationException = Neo4Net.Kernel.Api.Internal.Exceptions.schema.ConstraintValidationException;
	using UniquePropertyValueValidationException = Neo4Net.Kernel.Api.Exceptions.schema.UniquePropertyValueValidationException;
	using ConstraintDescriptorFactory = Neo4Net.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using UniquenessConstraintDescriptor = Neo4Net.Kernel.api.schema.constraints.UniquenessConstraintDescriptor;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using MetaDataStore = Neo4Net.Kernel.impl.store.MetaDataStore;
	using NeoStores = Neo4Net.Kernel.impl.store.NeoStores;
	using ConstraintRule = Neo4Net.Kernel.Impl.Store.Records.ConstraintRule;
	using NodeRecord = Neo4Net.Kernel.Impl.Store.Records.NodeRecord;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.doThrow;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	public class IntegrityValidatorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldValidateUniquenessIndexes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldValidateUniquenessIndexes()
		 {
			  // Given
			  NeoStores store = mock( typeof( NeoStores ) );
			  IndexingService indexes = mock( typeof( IndexingService ) );
			  IntegrityValidator validator = new IntegrityValidator( store, indexes );
			  UniquenessConstraintDescriptor constraint = ConstraintDescriptorFactory.uniqueForLabel( 1, 1 );

			  doThrow( new UniquePropertyValueValidationException( constraint, ConstraintValidationException.Phase.VERIFICATION, new Exception() ) ).when(indexes).validateIndex(2L);

			  ConstraintRule record = ConstraintRule.constraintRule( 1L, constraint, 2L );

			  // When
			  try
			  {
					validator.ValidateSchemaRule( record );
					fail( "Should have thrown integrity error." );
			  }
			  catch ( Exception )
			  {
					// good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void deletingNodeWithRelationshipsIsNotAllowed()
		 public virtual void DeletingNodeWithRelationshipsIsNotAllowed()
		 {
			  // Given
			  NeoStores store = mock( typeof( NeoStores ) );
			  IndexingService indexes = mock( typeof( IndexingService ) );
			  IntegrityValidator validator = new IntegrityValidator( store, indexes );

			  NodeRecord record = new NodeRecord( 1L, false, 1L, -1L );
			  record.InUse = false;

			  // When
			  try
			  {
					validator.ValidateNodeRecord( record );
					fail( "Should have thrown integrity error." );
			  }
			  catch ( Exception )
			  {
					// good
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void transactionsStartedBeforeAConstraintWasCreatedAreDisallowed()
		 public virtual void TransactionsStartedBeforeAConstraintWasCreatedAreDisallowed()
		 {
			  // Given
			  NeoStores store = mock( typeof( NeoStores ) );
			  MetaDataStore metaDataStore = mock( typeof( MetaDataStore ) );
			  when( store.MetaDataStore ).thenReturn( metaDataStore );
			  IndexingService indexes = mock( typeof( IndexingService ) );
			  when( metaDataStore.LatestConstraintIntroducingTx ).thenReturn( 10L );
			  IntegrityValidator validator = new IntegrityValidator( store, indexes );

			  // When
			  try
			  {
					validator.ValidateTransactionStartKnowledge( 1 );
					fail( "Should have thrown integrity error." );
			  }
			  catch ( Exception )
			  {
					// good
			  }
		 }
	}

}