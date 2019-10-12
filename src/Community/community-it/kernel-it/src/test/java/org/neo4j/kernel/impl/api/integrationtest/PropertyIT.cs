using System.Collections.Generic;

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
namespace Neo4Net.Kernel.Impl.Api.integrationtest
{
	using Test = org.junit.Test;

	using NamedToken = Neo4Net.@internal.Kernel.Api.NamedToken;
	using Transaction = Neo4Net.@internal.Kernel.Api.Transaction;
	using Write = Neo4Net.@internal.Kernel.Api.Write;
	using EntityNotFoundException = Neo4Net.@internal.Kernel.Api.exceptions.EntityNotFoundException;
	using AnonymousContext = Neo4Net.Kernel.api.security.AnonymousContext;
	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsCollectionContaining.hasItems;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.Iterators.asCollection;

	public class PropertyIT : KernelIntegrationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListAllPropertyKeys() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListAllPropertyKeys()
		 {
			  // given
			  DbWithNoCache();

			  Transaction transaction = NewTransaction( AnonymousContext.writeToken() );
			  int prop1 = transaction.TokenWrite().propertyKeyGetOrCreateForName("prop1");
			  int prop2 = transaction.TokenWrite().propertyKeyGetOrCreateForName("prop2");

			  // when
			  IEnumerator<NamedToken> propIdsBeforeCommit = transaction.TokenRead().propertyKeyGetAllTokens();

			  // then
			  assertThat( asCollection( propIdsBeforeCommit ), hasItems( new NamedToken( "prop1", prop1 ), new NamedToken( "prop2", prop2 ) ) );

			  // when
			  Commit();
			  transaction = NewTransaction();
			  IEnumerator<NamedToken> propIdsAfterCommit = transaction.TokenRead().propertyKeyGetAllTokens();

			  // then
			  assertThat( asCollection( propIdsAfterCommit ), hasItems( new NamedToken( "prop1", prop1 ), new NamedToken( "prop2", prop2 ) ) );
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowModifyingPropertiesOnDeletedRelationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotAllowModifyingPropertiesOnDeletedRelationship()
		 {
			  // given
			  Transaction transaction = NewTransaction( AnonymousContext.writeToken() );
			  int prop1 = transaction.TokenWrite().propertyKeyGetOrCreateForName("prop1");
			  int type = transaction.TokenWrite().relationshipTypeGetOrCreateForName("RELATED");
			  long startNodeId = transaction.DataWrite().nodeCreate();
			  long endNodeId = transaction.DataWrite().nodeCreate();
			  long rel = transaction.DataWrite().relationshipCreate(startNodeId, type, endNodeId);

			  transaction.DataWrite().relationshipSetProperty(rel, prop1, Values.stringValue("As"));
			  transaction.DataWrite().relationshipDelete(rel);

			  // When
			  try
			  {
					transaction.DataWrite().relationshipRemoveProperty(rel, prop1);
					fail( "Should have failed." );
			  }
			  catch ( EntityNotFoundException e )
			  {
					assertThat( e.Message, equalTo( "Unable to load RELATIONSHIP with id " + rel + "." ) );
			  }
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToRemoveResetAndTwiceRemovePropertyOnRelationship() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToRemoveResetAndTwiceRemovePropertyOnRelationship()
		 {
			  // given
			  Transaction transaction = NewTransaction( AnonymousContext.writeToken() );
			  int prop = transaction.TokenWrite().propertyKeyGetOrCreateForName("foo");
			  int type = transaction.TokenWrite().relationshipTypeGetOrCreateForName("RELATED");

			  long startNodeId = transaction.DataWrite().nodeCreate();
			  long endNodeId = transaction.DataWrite().nodeCreate();
			  long rel = transaction.DataWrite().relationshipCreate(startNodeId, type, endNodeId);
			  transaction.DataWrite().relationshipSetProperty(rel, prop, Values.of("bar"));

			  Commit();

			  // when
			  Write write = DataWriteInNewTransaction();
			  write.RelationshipRemoveProperty( rel, prop );
			  write.RelationshipSetProperty( rel, prop, Values.of( "bar" ) );
			  write.RelationshipRemoveProperty( rel, prop );
			  write.RelationshipRemoveProperty( rel, prop );

			  Commit();

			  // then
			  transaction = NewTransaction();
			  assertThat( RelationshipGetProperty( transaction, rel, prop ), equalTo( Values.NO_VALUE ) );
			  Commit();
		 }
	}


}