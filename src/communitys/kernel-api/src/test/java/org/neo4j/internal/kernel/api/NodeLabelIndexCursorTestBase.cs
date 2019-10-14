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
namespace Neo4Net.@internal.Kernel.Api
{
	using MutableLongSet = org.eclipse.collections.api.set.primitive.MutableLongSet;
	using LongHashSet = org.eclipse.collections.impl.set.mutable.primitive.LongHashSet;
	using Test = org.junit.Test;

	using KernelException = Neo4Net.@internal.Kernel.Api.exceptions.KernelException;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.IndexReadAsserts.assertNodeCount;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@internal.kernel.api.IndexReadAsserts.assertNodes;

	public abstract class NodeLabelIndexCursorTestBase<G> : KernelAPIWriteTestBase<G> where G : KernelAPIWriteTestSupport
	{
		 private int _labelOne = 1;
		 private int _labelTwo = 2;
		 private int _labelThree = 3;
		 private int _labelFirst = 4;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindNodesByLabel() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindNodesByLabel()
		 {
			  // GIVEN
			  long toDelete;
			  using ( Transaction tx = beginTransaction() )
			  {
					CreateNode( tx.DataWrite(), _labelOne, _labelFirst );
					CreateNode( tx.DataWrite(), _labelTwo, _labelFirst );
					CreateNode( tx.DataWrite(), _labelThree, _labelFirst );
					toDelete = CreateNode( tx.DataWrite(), _labelOne );
					CreateNode( tx.DataWrite(), _labelTwo );
					CreateNode( tx.DataWrite(), _labelThree );
					CreateNode( tx.DataWrite(), _labelThree );
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					tx.DataWrite().nodeDelete(toDelete);
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					Read read = tx.DataRead();

					using ( NodeLabelIndexCursor cursor = tx.Cursors().allocateNodeLabelIndexCursor() )
					{
						 MutableLongSet uniqueIds = new LongHashSet();

						 // WHEN
						 read.NodeLabelScan( _labelOne, cursor );

						 // THEN
						 assertNodeCount( cursor, 1, uniqueIds );

						 // WHEN
						 read.NodeLabelScan( _labelTwo, cursor );

						 // THEN
						 assertNodeCount( cursor, 2, uniqueIds );

						 // WHEN
						 read.NodeLabelScan( _labelThree, cursor );

						 // THEN
						 assertNodeCount( cursor, 3, uniqueIds );

						 // WHEN
						 uniqueIds.clear();
						 read.NodeLabelScan( _labelFirst, cursor );

						 // THEN
						 assertNodeCount( cursor, 3, uniqueIds );
					}
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindNodesByLabelInTx() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindNodesByLabelInTx()
		 {
			  long inStore;
			  long deletedInTx;
			  long createdInTx;

			  using ( Transaction tx = beginTransaction() )
			  {
					inStore = CreateNode( tx.DataWrite(), _labelOne );
					CreateNode( tx.DataWrite(), _labelTwo );
					deletedInTx = CreateNode( tx.DataWrite(), _labelOne );
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					tx.DataWrite().nodeDelete(deletedInTx);
					createdInTx = CreateNode( tx.DataWrite(), _labelOne );

					CreateNode( tx.DataWrite(), _labelTwo );

					Read read = tx.DataRead();

					using ( NodeLabelIndexCursor cursor = tx.Cursors().allocateNodeLabelIndexCursor() )
					{
						 MutableLongSet uniqueIds = new LongHashSet();

						 // when
						 read.NodeLabelScan( _labelOne, cursor );

						 // then
						 assertNodes( cursor, uniqueIds, inStore, createdInTx );
					}
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long createNode(Write write, int... labels) throws org.neo4j.internal.kernel.api.exceptions.KernelException
		 private long CreateNode( Write write, params int[] labels )
		 {
			  long nodeId = write.NodeCreate();
			  foreach ( int label in labels )
			  {
					write.NodeAddLabel( nodeId, label );
			  }
			  return nodeId;
		 }
	}

}