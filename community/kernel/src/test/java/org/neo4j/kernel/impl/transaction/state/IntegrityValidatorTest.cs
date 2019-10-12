using System;

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
namespace Org.Neo4j.Kernel.impl.transaction.state
{
	using Test = org.junit.Test;

	using ConstraintValidationException = Org.Neo4j.@internal.Kernel.Api.exceptions.schema.ConstraintValidationException;
	using UniquePropertyValueValidationException = Org.Neo4j.Kernel.Api.Exceptions.schema.UniquePropertyValueValidationException;
	using ConstraintDescriptorFactory = Org.Neo4j.Kernel.api.schema.constraints.ConstraintDescriptorFactory;
	using UniquenessConstraintDescriptor = Org.Neo4j.Kernel.api.schema.constraints.UniquenessConstraintDescriptor;
	using IndexingService = Org.Neo4j.Kernel.Impl.Api.index.IndexingService;
	using MetaDataStore = Org.Neo4j.Kernel.impl.store.MetaDataStore;
	using NeoStores = Org.Neo4j.Kernel.impl.store.NeoStores;
	using ConstraintRule = Org.Neo4j.Kernel.impl.store.record.ConstraintRule;
	using NodeRecord = Org.Neo4j.Kernel.impl.store.record.NodeRecord;

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