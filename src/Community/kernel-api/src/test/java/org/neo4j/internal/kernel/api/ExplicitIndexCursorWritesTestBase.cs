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
namespace Neo4Net.@internal.Kernel.Api
{
	using Test = org.junit.Test;

	using Node = Neo4Net.Graphdb.Node;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Neo4Net.Graphdb.index;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @SuppressWarnings("Duplicates") public abstract class ExplicitIndexCursorWritesTestBase<G extends KernelAPIWriteTestSupport> extends KernelAPIWriteTestBase<G>
	public abstract class ExplicitIndexCursorWritesTestBase<G> : KernelAPIWriteTestBase<G> where G : KernelAPIWriteTestSupport
	{

		 private const string INDEX_NAME = "foo";
		 private const string KEY = "bar";
		 private const string VALUE = "this is it";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateExplicitNodeIndexEagerly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateExplicitNodeIndexEagerly()
		 {
			  // When
			  using ( Transaction tx = beginTransaction() )
			  {
					ExplicitIndexWrite indexWrite = tx.IndexWrite();
					Dictionary<string, string> config = new Dictionary<string, string>();
					config["type"] = "exact";
					config["provider"] = "lucene";
					indexWrite.NodeExplicitIndexCreate( INDEX_NAME, config );
					tx.Success();
			  }

			  // Then
			  using ( Neo4Net.Graphdb.Transaction ctx = graphDb.beginTx() )
			  {
					assertTrue( graphDb.index().existsForNodes(INDEX_NAME) );
					ctx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateExplicitNodeIndexLazily() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateExplicitNodeIndexLazily()
		 {
			  // When
			  using ( Transaction tx = beginTransaction() )
			  {
					ExplicitIndexWrite indexWrite = tx.IndexWrite();
					Dictionary<string, string> config = new Dictionary<string, string>();
					config["type"] = "exact";
					config["provider"] = "lucene";
					indexWrite.NodeExplicitIndexCreateLazily( INDEX_NAME, config );
					tx.Success();
			  }

			  // Then
			  using ( Neo4Net.Graphdb.Transaction ctx = graphDb.beginTx() )
			  {
					assertTrue( graphDb.index().existsForNodes(INDEX_NAME) );
					ctx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddNodeToExplicitIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAddNodeToExplicitIndex()
		 {
			  long nodeId;
			  using ( Transaction tx = beginTransaction() )
			  {
					nodeId = tx.DataWrite().nodeCreate();
					ExplicitIndexWrite indexWrite = tx.IndexWrite();
					indexWrite.NodeAddToExplicitIndex( INDEX_NAME, nodeId, KEY, VALUE );
					tx.Success();
			  }

			  // Then
			  using ( Neo4Net.Graphdb.Transaction ctx = graphDb.beginTx() )
			  {
					IndexHits<Node> hits = graphDb.index().forNodes(INDEX_NAME).get(KEY, VALUE);
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( hits.next().Id, equalTo(nodeId) );
					hits.Close();
					ctx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveNodeFromExplicitIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveNodeFromExplicitIndex()
		 {
			  // Given
			  long nodeId = AddNodeToExplicitIndex();

			  // When
			  using ( Transaction tx = beginTransaction() )
			  {
					ExplicitIndexWrite indexWrite = tx.IndexWrite();
					indexWrite.NodeRemoveFromExplicitIndex( INDEX_NAME, nodeId );
					tx.Success();
			  }

			  // Then
			  using ( Neo4Net.Graphdb.Transaction ctx = graphDb.beginTx() )
			  {
					IndexHits<Node> hits = graphDb.index().forNodes(INDEX_NAME).get(KEY, VALUE);
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( hits.hasNext() );
					hits.Close();
					ctx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleRemoveNodeFromExplicitIndexTwice() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleRemoveNodeFromExplicitIndexTwice()
		 {
			  // Given
			  long nodeId = AddNodeToExplicitIndex();

			  // When
			  using ( Transaction tx = beginTransaction() )
			  {
					ExplicitIndexWrite indexWrite = tx.IndexWrite();
					indexWrite.NodeRemoveFromExplicitIndex( INDEX_NAME, nodeId );
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					ExplicitIndexWrite indexWrite = tx.IndexWrite();
					indexWrite.NodeRemoveFromExplicitIndex( INDEX_NAME, nodeId );
					tx.Success();
			  }

			  // Then
			  using ( Neo4Net.Graphdb.Transaction ctx = graphDb.beginTx() )
			  {
					IndexHits<Node> hits = graphDb.index().forNodes(INDEX_NAME).get(KEY, VALUE);
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( hits.hasNext() );
					hits.Close();
					ctx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveNonExistingNodeFromExplicitIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveNonExistingNodeFromExplicitIndex()
		 {
			  // Given
			  long nodeId = AddNodeToExplicitIndex();

			  // When
			  using ( Transaction tx = beginTransaction() )
			  {
					ExplicitIndexWrite indexWrite = tx.IndexWrite();
					indexWrite.NodeRemoveFromExplicitIndex( INDEX_NAME, nodeId + 1 );
					tx.Success();
			  }

			  // Then
			  using ( Neo4Net.Graphdb.Transaction ctx = graphDb.beginTx() )
			  {
					IndexHits<Node> hits = graphDb.index().forNodes(INDEX_NAME).get(KEY, VALUE);
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( hits.next().Id, equalTo(nodeId) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( hits.hasNext() );
					hits.Close();
					ctx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateExplicitRelationshipIndexEagerly() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateExplicitRelationshipIndexEagerly()
		 {
			  // When
			  using ( Transaction tx = beginTransaction() )
			  {
					ExplicitIndexWrite indexWrite = tx.IndexWrite();
					Dictionary<string, string> config = new Dictionary<string, string>();
					config["type"] = "exact";
					config["provider"] = "lucene";
					indexWrite.RelationshipExplicitIndexCreate( INDEX_NAME, config );
					tx.Success();
			  }

			  // Then
			  using ( Neo4Net.Graphdb.Transaction ctx = graphDb.beginTx() )
			  {
					assertTrue( graphDb.index().existsForRelationships(INDEX_NAME) );
					ctx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateExplicitRelationshipIndexLazily() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateExplicitRelationshipIndexLazily()
		 {
			  // When
			  using ( Transaction tx = beginTransaction() )
			  {
					ExplicitIndexWrite indexWrite = tx.IndexWrite();
					Dictionary<string, string> config = new Dictionary<string, string>();
					config["type"] = "exact";
					config["provider"] = "lucene";
					indexWrite.RelationshipExplicitIndexCreateLazily( INDEX_NAME, config );
					tx.Success();
			  }

			  // Then
			  using ( Neo4Net.Graphdb.Transaction ctx = graphDb.beginTx() )
			  {
					assertTrue( graphDb.index().existsForRelationships(INDEX_NAME) );
					ctx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldCreateExplicitIndexTwice() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldCreateExplicitIndexTwice()
		 {
			  // Given
			  Dictionary<string, string> config = new Dictionary<string, string>();
			  config["type"] = "exact";
			  config["provider"] = "lucene";

			  using ( Transaction tx = beginTransaction() )
			  {
					ExplicitIndexWrite indexWrite = tx.IndexWrite();
					indexWrite.NodeExplicitIndexCreateLazily( INDEX_NAME, config );
					tx.Success();
			  }

			  // When
			  using ( Transaction tx = beginTransaction() )
			  {
					ExplicitIndexWrite indexWrite = tx.IndexWrite();
					indexWrite.NodeExplicitIndexCreateLazily( INDEX_NAME, config );
					tx.Success();
			  }

			  // Then
			  using ( Neo4Net.Graphdb.Transaction ctx = graphDb.beginTx() )
			  {
					assertTrue( graphDb.index().existsForNodes(INDEX_NAME) );
					ctx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAddRelationshipToExplicitIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAddRelationshipToExplicitIndex()
		 {
			  long relId;
			  using ( Neo4Net.Graphdb.Transaction ctx = graphDb.beginTx() )
			  {
					relId = graphDb.createNode().createRelationshipTo(graphDb.createNode(), RelationshipType.withName("R")).Id;
					ctx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					ExplicitIndexWrite indexWrite = tx.IndexWrite();
					indexWrite.RelationshipAddToExplicitIndex( INDEX_NAME, relId, KEY, VALUE );
					tx.Success();
			  }

			  // Then
			  using ( Neo4Net.Graphdb.Transaction ctx = graphDb.beginTx() )
			  {
					IndexHits<Relationship> hits = graphDb.index().forRelationships(INDEX_NAME).get(KEY, VALUE);
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( hits.next().Id, equalTo(relId) );
					hits.Close();
					ctx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveRelationshipFromExplicitIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveRelationshipFromExplicitIndex()
		 {
			  // Given
			  long relId = AddRelationshipToExplicitIndex();

			  // When
			  using ( Transaction tx = beginTransaction() )
			  {
					ExplicitIndexWrite indexWrite = tx.IndexWrite();
					indexWrite.RelationshipRemoveFromExplicitIndex( INDEX_NAME, relId );
					tx.Success();
			  }

			  // Then
			  using ( Neo4Net.Graphdb.Transaction ctx = graphDb.beginTx() )
			  {
					IndexHits<Node> hits = graphDb.index().forNodes(INDEX_NAME).get(KEY, VALUE);
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( hits.hasNext() );
					hits.Close();
					ctx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHandleRemoveRelationshipFromExplicitIndexTwice() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldHandleRemoveRelationshipFromExplicitIndexTwice()
		 {
			  // Given
			  long relId = AddRelationshipToExplicitIndex();

			  // When
			  using ( Transaction tx = beginTransaction() )
			  {
					ExplicitIndexWrite indexWrite = tx.IndexWrite();
					indexWrite.RelationshipRemoveFromExplicitIndex( INDEX_NAME, relId );
					tx.Success();
			  }

			  using ( Transaction tx = beginTransaction() )
			  {
					ExplicitIndexWrite indexWrite = tx.IndexWrite();
					indexWrite.RelationshipRemoveFromExplicitIndex( INDEX_NAME, relId );
					tx.Success();
			  }

			  // Then
			  using ( Neo4Net.Graphdb.Transaction ctx = graphDb.beginTx() )
			  {
					IndexHits<Relationship> hits = graphDb.index().forRelationships(INDEX_NAME).get(KEY, VALUE);
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( hits.hasNext() );
					hits.Close();
					ctx.Success();
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRemoveNonExistingRelationshipFromExplicitIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRemoveNonExistingRelationshipFromExplicitIndex()
		 {
			  // Given
			  long relId = AddRelationshipToExplicitIndex();

			  // When
			  using ( Transaction tx = beginTransaction() )
			  {
					ExplicitIndexWrite indexWrite = tx.IndexWrite();
					indexWrite.RelationshipRemoveFromExplicitIndex( INDEX_NAME, relId + 1 );
					tx.Success();
			  }

			  // Then
			  using ( Neo4Net.Graphdb.Transaction ctx = graphDb.beginTx() )
			  {
					IndexHits<Relationship> hits = graphDb.index().forRelationships(INDEX_NAME).get(KEY, VALUE);
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertThat( hits.next().Id, equalTo(relId) );
//JAVA TO C# CONVERTER TODO TASK: Java iterators are only converted within the context of 'while' and 'for' loops:
					assertFalse( hits.hasNext() );
					hits.Close();
					ctx.Success();
			  }
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private long addNodeToExplicitIndex() throws Exception
		 private long AddNodeToExplicitIndex()
		 {
			  long nodeId;
			  using ( Transaction tx = beginTransaction() )
			  {
					nodeId = tx.DataWrite().nodeCreate();
					ExplicitIndexWrite indexWrite = tx.IndexWrite();
					Dictionary<string, string> config = new Dictionary<string, string>();
					config["type"] = "exact";
					config["provider"] = "lucene";
					indexWrite.NodeExplicitIndexCreateLazily( INDEX_NAME, config );
					indexWrite.NodeAddToExplicitIndex( INDEX_NAME, nodeId, KEY, VALUE );
					tx.Success();
			  }
			  return nodeId;
		 }

		 private long AddRelationshipToExplicitIndex()
		 {
			  long relId;
			  using ( Neo4Net.Graphdb.Transaction ctx = graphDb.beginTx() )
			  {
					Relationship rel = graphDb.createNode().createRelationshipTo(graphDb.createNode(), RelationshipType.withName("R"));
					relId = rel.Id;
					graphDb.index().forRelationships(INDEX_NAME).add(rel, KEY, VALUE);
					ctx.Success();
			  }
			  return relId;
		 }
	}

}