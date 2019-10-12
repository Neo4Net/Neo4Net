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
namespace Neo4Net.@internal.Kernel.Api
{
	using Test = org.junit.Test;

	using Values = Neo4Net.Values.Storable.Values;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertNull;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTrue;

	public abstract class TransactionStateTestBase<G> : KernelAPIWriteTestBase<G> where G : KernelAPIWriteTestSupport
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectNodeDeletedInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectNodeDeletedInTransaction()
		 {
			  // GIVEN
			  long deletedInTx, unaffected, addedInTx, addedAndRemovedInTx;
			  using ( Transaction tx = beginTransaction() )
			  {
					deletedInTx = tx.DataWrite().nodeCreate();
					unaffected = tx.DataWrite().nodeCreate();
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					// WHEN
					addedInTx = tx.DataWrite().nodeCreate();
					addedAndRemovedInTx = tx.DataWrite().nodeCreate();
					tx.DataWrite().nodeDelete(deletedInTx);
					tx.DataWrite().nodeDelete(addedAndRemovedInTx);

					// THEN
					assertFalse( tx.DataRead().nodeDeletedInTransaction(addedInTx) );
					assertFalse( tx.DataRead().nodeDeletedInTransaction(unaffected) );
					assertTrue( tx.DataRead().nodeDeletedInTransaction(addedAndRemovedInTx) );
					assertTrue( tx.DataRead().nodeDeletedInTransaction(deletedInTx) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDetectRelationshipDeletedInTransaction() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDetectRelationshipDeletedInTransaction()
		 {
			  // GIVEN
			  long node;
			  int relType;
			  long deletedInTx, unaffected, addedInTx, addedAndRemovedInTx;
			  using ( Transaction tx = beginTransaction() )
			  {
					node = tx.DataWrite().nodeCreate();
					relType = tx.TokenWrite().relationshipTypeCreateForName("REL_TYPE");
					deletedInTx = tx.DataWrite().relationshipCreate(node, relType, node);
					unaffected = tx.DataWrite().relationshipCreate(node, relType, node);
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					// WHEN
					addedInTx = tx.DataWrite().relationshipCreate(node, relType, node);
					addedAndRemovedInTx = tx.DataWrite().relationshipCreate(node, relType, node);
					tx.DataWrite().relationshipDelete(deletedInTx);
					tx.DataWrite().relationshipDelete(addedAndRemovedInTx);

					// THEN
					assertFalse( tx.DataRead().relationshipDeletedInTransaction(addedInTx) );
					assertFalse( tx.DataRead().relationshipDeletedInTransaction(unaffected) );
					assertTrue( tx.DataRead().relationshipDeletedInTransaction(addedAndRemovedInTx) );
					assertTrue( tx.DataRead().relationshipDeletedInTransaction(deletedInTx) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportInTransactionNodeProperty() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldReportInTransactionNodeProperty()
		 {
			  // GIVEN
			  long node;
			  int p1, p2, p3, p4, p5;
			  using ( Transaction tx = beginTransaction() )
			  {
					node = tx.DataWrite().nodeCreate();
					p1 = tx.TokenWrite().propertyKeyCreateForName("p1");
					p2 = tx.TokenWrite().propertyKeyCreateForName("p2");
					p3 = tx.TokenWrite().propertyKeyCreateForName("p3");
					p4 = tx.TokenWrite().propertyKeyCreateForName("p4");
					p5 = tx.TokenWrite().propertyKeyCreateForName("p5");
					tx.DataWrite().nodeSetProperty(node, p1, Values.of(1));
					tx.DataWrite().nodeSetProperty(node, p3, Values.of(3));
					tx.DataWrite().nodeSetProperty(node, p4, Values.of(4));
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					// WHEN
					tx.DataWrite().nodeSetProperty(node, p3, Values.of(13));
					tx.DataWrite().nodeRemoveProperty(node, p4);
					tx.DataWrite().nodeSetProperty(node, p5, Values.of(15));

					// THEN
					assertNull( "Unchanged existing property is null", tx.DataRead().nodePropertyChangeInTransactionOrNull(node, p1) );

					assertNull( "Unchanged missing property is null", tx.DataRead().nodePropertyChangeInTransactionOrNull(node, p2) );

					assertEquals( "Changed property is new value", Values.of( 13 ), tx.DataRead().nodePropertyChangeInTransactionOrNull(node, p3) );

					assertEquals( "Removed property is NO_VALUE", Values.NO_VALUE, tx.DataRead().nodePropertyChangeInTransactionOrNull(node, p4) );

					assertEquals( "Added property is new value", Values.of( 15 ), tx.DataRead().nodePropertyChangeInTransactionOrNull(node, p5) );
			  }
		 }
	}

}