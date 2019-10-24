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
namespace Neo4Net.Kernel.Impl.Api.integrationtest
{
	using Matcher = org.hamcrest.Matcher;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using Direction = Neo4Net.GraphDb.Direction;
	using Iterators = Neo4Net.Collections.Helpers.Iterators;
	using Transaction = Neo4Net.Kernel.Api.Internal.Transaction;
	using LoginContext = Neo4Net.Kernel.Api.Internal.security.LoginContext;
	using AnonymousContext = Neo4Net.Kernel.api.security.AnonymousContext;
	using Neo4Net.Test.rule.concurrent;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.AllOf.allOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Direction.BOTH;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Direction.INCOMING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Direction.OUTGOING;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.Internal.Transaction_Type.@implicit;

	public class RelationshipIT : KernelIntegrationTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.Neo4Net.test.rule.concurrent.OtherThreadRule<Object> otherThread = new org.Neo4Net.test.rule.concurrent.OtherThreadRule<>(10, java.util.concurrent.TimeUnit.SECONDS);
		 public OtherThreadRule<object> OtherThread = new OtherThreadRule<object>( 10, TimeUnit.SECONDS );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldListRelationshipsInCurrentAndSubsequentTx() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldListRelationshipsInCurrentAndSubsequentTx()
		 {
			  // given
			  Transaction transaction = NewTransaction( AnonymousContext.WriteToken() );
			  int relType1 = transaction.TokenWrite().relationshipTypeGetOrCreateForName("Type1");
			  int relType2 = transaction.TokenWrite().relationshipTypeGetOrCreateForName("Type2");

			  long refNode = transaction.DataWrite().nodeCreate();
			  long otherNode = transaction.DataWrite().nodeCreate();
			  long fromRefToOther1 = transaction.DataWrite().relationshipCreate(refNode, relType1, otherNode);
			  long fromRefToOther2 = transaction.DataWrite().relationshipCreate(refNode, relType2, otherNode);
			  long fromOtherToRef = transaction.DataWrite().relationshipCreate(otherNode, relType1, refNode);
			  long fromRefToRef = transaction.DataWrite().relationshipCreate(refNode, relType2, refNode);
			  long endNode = transaction.DataWrite().nodeCreate();
			  long fromRefToThird = transaction.DataWrite().relationshipCreate(refNode, relType2, endNode);

			  // when & then
			  AssertRels( NodeGetRelationships( transaction, refNode, BOTH ), fromRefToOther1, fromRefToOther2, fromRefToRef, fromRefToThird, fromOtherToRef );

			  AssertRels( NodeGetRelationships( transaction, refNode, BOTH, new int[]{ relType1 } ), fromRefToOther1, fromOtherToRef );

			  AssertRels( NodeGetRelationships( transaction, refNode, BOTH, new int[]{ relType1, relType2 } ), fromRefToOther1, fromRefToOther2, fromRefToRef, fromRefToThird, fromOtherToRef );

			  AssertRels( NodeGetRelationships( transaction, refNode, INCOMING ), fromOtherToRef );

			  AssertRels( NodeGetRelationships( transaction, refNode, INCOMING, new int[]{ relType1 } ) );

			  AssertRels( NodeGetRelationships( transaction, refNode, OUTGOING, new int[]{ relType1, relType2 } ), fromRefToOther1, fromRefToOther2, fromRefToThird, fromRefToRef );

			  // when
			  Commit();
			  transaction = NewTransaction();

			  // when & then
			  AssertRels( NodeGetRelationships( transaction, refNode, BOTH ), fromRefToOther1, fromRefToOther2, fromRefToRef, fromRefToThird, fromOtherToRef );

			  AssertRels( NodeGetRelationships( transaction, refNode, BOTH, new int[]{ relType1 } ), fromRefToOther1, fromOtherToRef );

			  AssertRels( NodeGetRelationships( transaction, refNode, BOTH, new int[]{ relType1, relType2 } ), fromRefToOther1, fromRefToOther2, fromRefToRef, fromRefToThird, fromOtherToRef );

			  AssertRels( NodeGetRelationships( transaction, refNode, INCOMING ), fromOtherToRef );

			  AssertRels( NodeGetRelationships( transaction, refNode, INCOMING, new int[]{ relType1 } ) );

			  AssertRels( NodeGetRelationships( transaction, refNode, OUTGOING, new int[]{ relType1, relType2 } ), fromRefToOther1, fromRefToOther2, fromRefToThird, fromRefToRef );
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldInterleaveModifiedRelationshipsWithExistingOnes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldInterleaveModifiedRelationshipsWithExistingOnes()
		 {
			  // given
			  long refNode;
			  long fromRefToOther1;
			  long fromRefToOther2;
			  int relType1;
			  int relType2;
			  {
					Transaction transaction = NewTransaction( AnonymousContext.WriteToken() );

					relType1 = transaction.TokenWrite().relationshipTypeGetOrCreateForName("Type1");
					relType2 = transaction.TokenWrite().relationshipTypeGetOrCreateForName("Type2");

					refNode = transaction.DataWrite().nodeCreate();
					long otherNode = transaction.DataWrite().nodeCreate();
					fromRefToOther1 = transaction.DataWrite().relationshipCreate(refNode, relType1, otherNode);
					fromRefToOther2 = transaction.DataWrite().relationshipCreate(refNode, relType2, otherNode);
					Commit();
			  }
			  {
					Transaction transaction = NewTransaction( AnonymousContext.WriteToken() );

					// When
					transaction.DataWrite().relationshipDelete(fromRefToOther1);
					long endNode = transaction.DataWrite().nodeCreate();
					long localTxRel = transaction.DataWrite().relationshipCreate(refNode, relType1, endNode);

					// Then
					AssertRels( NodeGetRelationships( transaction, refNode, BOTH ), fromRefToOther2, localTxRel );
					AssertRelsInSeparateTx( refNode, BOTH, fromRefToOther1, fromRefToOther2 );
					Commit();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReturnRelsWhenAskingForRelsWhereOnlySomeTypesExistInCurrentRel() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReturnRelsWhenAskingForRelsWhereOnlySomeTypesExistInCurrentRel()
		 {
			  Transaction transaction = NewTransaction( AnonymousContext.WriteToken() );

			  int relType1 = transaction.TokenWrite().relationshipTypeGetOrCreateForName("Type1");
			  int relType2 = transaction.TokenWrite().relationshipTypeGetOrCreateForName("Type2");

			  long refNode = transaction.DataWrite().nodeCreate();
			  long otherNode = transaction.DataWrite().nodeCreate();
			  long theRel = transaction.DataWrite().relationshipCreate(refNode, relType1, otherNode);

			  AssertRels( NodeGetRelationships( transaction, refNode, OUTGOING, new int[]{ relType2, relType1 } ), theRel );
			  Commit();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void askingForNonExistantReltypeOnDenseNodeShouldNotCorruptState() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AskingForNonExistantReltypeOnDenseNodeShouldNotCorruptState()
		 {
			  // Given a dense node with one type of rels
			  long[] rels = new long[200];
			  long refNode;
			  int relTypeTheNodeDoesUse;
			  int relTypeTheNodeDoesNotUse;
			  {
					Transaction transaction = NewTransaction( AnonymousContext.WriteToken() );

					relTypeTheNodeDoesUse = transaction.TokenWrite().relationshipTypeGetOrCreateForName("Type1");
					relTypeTheNodeDoesNotUse = transaction.TokenWrite().relationshipTypeGetOrCreateForName("Type2");

					refNode = transaction.DataWrite().nodeCreate();
					long otherNode = transaction.DataWrite().nodeCreate();

					for ( int i = 0; i < rels.Length; i++ )
					{
						 rels[i] = transaction.DataWrite().relationshipCreate(refNode, relTypeTheNodeDoesUse, otherNode);
					}
					Commit();
			  }
			  Transaction transaction = NewTransaction();

			  // When I've asked for rels that the node does not have
			  AssertRels( NodeGetRelationships( transaction, refNode, Direction.INCOMING, new int[]{ relTypeTheNodeDoesNotUse } ) );

			  // Then the node should still load the real rels
			  AssertRels( NodeGetRelationships( transaction, refNode, Direction.BOTH, new int[]{ relTypeTheNodeDoesUse } ), rels );
			  Commit();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void assertRelsInSeparateTx(final long refNode, final org.Neo4Net.graphdb.Direction both, final long... longs) throws InterruptedException, java.util.concurrent.ExecutionException, java.util.concurrent.TimeoutException
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
		 private void AssertRelsInSeparateTx( long refNode, Direction both, params long[] longs )
		 {
			  assertTrue(OtherThread.execute(state =>
			  {
				using ( Transaction ktx = Kernel.BeginTransaction( @implicit, LoginContext.AUTH_DISABLED ) )
				{
					 AssertRels( NodeGetRelationships( ktx, refNode, both ), longs );
				}
				return true;
			  }).get( 10, TimeUnit.SECONDS ));
		 }

		 private void AssertRels( IEnumerator<long> it, params long[] rels )
		 {
//JAVA TO C# CONVERTER TODO TASK: There is no .NET equivalent to the Java 'super' constraint:
//ORIGINAL LINE: java.util.List<org.hamcrest.Matcher<? super Iterable<long>>> all = new java.util.ArrayList<>(rels.length);
//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
			  IList<Matcher> all = new List<Matcher>( rels.Length );
			  foreach ( long element in rels )
			  {
					all.Add( hasItem( element ) );
			  }

			  IList<long> list = Iterators.asList( it );
			  assertThat( list, allOf( all ) );
		 }
	}

}