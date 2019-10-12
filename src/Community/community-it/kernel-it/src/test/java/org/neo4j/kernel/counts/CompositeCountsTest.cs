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
namespace Neo4Net.Kernel.counts
{
	using Before = org.junit.Before;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Relationship = Neo4Net.Graphdb.Relationship;
	using RelationshipType = Neo4Net.Graphdb.RelationshipType;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using TokenRead = Neo4Net.@internal.Kernel.Api.TokenRead;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using Statement = Neo4Net.Kernel.api.Statement;
	using StatementConstants = Neo4Net.Kernel.api.StatementConstants;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using ImpermanentDatabaseRule = Neo4Net.Test.rule.ImpermanentDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.RelationshipType.withName;

	public class CompositeCountsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule db = new org.neo4j.test.rule.ImpermanentDatabaseRule();
		 public readonly DatabaseRule Db = new ImpermanentDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldReportNumberOfRelationshipsFromNodesWithGivenLabel()
		 public virtual void ShouldReportNumberOfRelationshipsFromNodesWithGivenLabel()
		 {
			  // given
			  using ( Transaction tx = Db.beginTx() )
			  {
					Node foo = Db.createNode( label( "Foo" ) );
					Node fooBar = Db.createNode( label( "Foo" ), label( "Bar" ) );
					Node bar = Db.createNode( label( "Bar" ) );
					foo.CreateRelationshipTo( Db.createNode(), withName("ALPHA") );
					foo.CreateRelationshipTo( fooBar, withName( "BETA" ) );
					fooBar.CreateRelationshipTo( Db.createNode( label( "Bar" ) ), withName( "BETA" ) );
					fooBar.CreateRelationshipTo( Db.createNode(), withName("GAMMA") );
					bar.CreateRelationshipTo( Db.createNode( label( "Foo" ) ), withName( "GAMMA" ) );
					tx.Success();
			  }

			  // then
			  NumberOfRelationshipsMatching( label( "Foo" ), withName( "ALPHA" ), null ).shouldBe( 1 );
			  NumberOfRelationshipsMatching( label( "Foo" ), withName( "BETA" ), null ).shouldBe( 2 );
			  NumberOfRelationshipsMatching( label( "Foo" ), withName( "GAMMA" ), null ).shouldBe( 1 );
			  NumberOfRelationshipsMatching( null, withName( "ALPHA" ), label( "Foo" ) ).shouldBe( 0 );
			  NumberOfRelationshipsMatching( null, withName( "BETA" ), label( "Foo" ) ).shouldBe( 1 );
			  NumberOfRelationshipsMatching( null, withName( "GAMMA" ), label( "Foo" ) ).shouldBe( 1 );

			  NumberOfRelationshipsMatching( label( "Bar" ), withName( "ALPHA" ), null ).shouldBe( 0 );
			  NumberOfRelationshipsMatching( label( "Bar" ), withName( "BETA" ), null ).shouldBe( 1 );
			  NumberOfRelationshipsMatching( label( "Bar" ), withName( "GAMMA" ), null ).shouldBe( 2 );
			  NumberOfRelationshipsMatching( null, withName( "ALPHA" ), label( "Bar" ) ).shouldBe( 0 );
			  NumberOfRelationshipsMatching( null, withName( "BETA" ), label( "Bar" ) ).shouldBe( 2 );
			  NumberOfRelationshipsMatching( null, withName( "GAMMA" ), label( "Bar" ) ).shouldBe( 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMaintainCountsOnRelationshipCreate()
		 public virtual void ShouldMaintainCountsOnRelationshipCreate()
		 {
			  // given
			  Node foo;
			  Node bar;
			  using ( Transaction tx = Db.beginTx() )
			  {
					foo = Db.createNode( label( "Foo" ) );
					bar = Db.createNode( label( "Bar" ) );

					tx.Success();
			  }

			  // when
			  using ( Transaction tx = Db.beginTx() )
			  {
					foo.CreateRelationshipTo( bar, withName( "KNOWS" ) );

					tx.Success();
			  }

			  // then
			  NumberOfRelationshipsMatching( label( "Foo" ), withName( "KNOWS" ), null ).shouldBe( 1 );
			  NumberOfRelationshipsMatching( null, withName( "KNOWS" ), label( "Foo" ) ).shouldBe( 0 );
			  NumberOfRelationshipsMatching( null, withName( "KNOWS" ), label( "Bar" ) ).shouldBe( 1 );
			  NumberOfRelationshipsMatching( label( "Bar" ), withName( "KNOWS" ), null ).shouldBe( 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMaintainCountsOnRelationshipDelete()
		 public virtual void ShouldMaintainCountsOnRelationshipDelete()
		 {
			  // given
			  Relationship relationship;
			  using ( Transaction tx = Db.beginTx() )
			  {
					relationship = Db.createNode( label( "Foo" ) ).createRelationshipTo( Db.createNode( label( "Bar" ) ), withName( "KNOWS" ) );

					tx.Success();
			  }

			  // when
			  using ( Transaction tx = Db.beginTx() )
			  {
					relationship.Delete();

					tx.Success();
			  }

			  // then
			  NumberOfRelationshipsMatching( label( "Foo" ), withName( "KNOWS" ), null ).shouldBe( 0 );
			  NumberOfRelationshipsMatching( null, withName( "KNOWS" ), label( "Foo" ) ).shouldBe( 0 );
			  NumberOfRelationshipsMatching( null, withName( "KNOWS" ), label( "Bar" ) ).shouldBe( 0 );
			  NumberOfRelationshipsMatching( label( "Bar" ), withName( "KNOWS" ), null ).shouldBe( 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMaintainCountsOnLabelAdd()
		 public virtual void ShouldMaintainCountsOnLabelAdd()
		 {
			  // given
			  Node foo;
			  Node bar;
			  using ( Transaction tx = Db.beginTx() )
			  {
					foo = Db.createNode();
					bar = Db.createNode( label( "Bar" ) );
					foo.CreateRelationshipTo( bar, withName( "KNOWS" ) );

					tx.Success();
			  }

			  // when
			  using ( Transaction tx = Db.beginTx() )
			  {
					foo.AddLabel( label( "Foo" ) );

					tx.Success();
			  }

			  // then
			  NumberOfRelationshipsMatching( label( "Foo" ), withName( "KNOWS" ), null ).shouldBe( 1 );
			  NumberOfRelationshipsMatching( null, withName( "KNOWS" ), label( "Foo" ) ).shouldBe( 0 );
			  NumberOfRelationshipsMatching( null, withName( "KNOWS" ), label( "Bar" ) ).shouldBe( 1 );
			  NumberOfRelationshipsMatching( label( "Bar" ), withName( "KNOWS" ), null ).shouldBe( 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMaintainCountsOnLabelRemove()
		 public virtual void ShouldMaintainCountsOnLabelRemove()
		 {
			  // given
			  Node foo;
			  Node bar;
			  using ( Transaction tx = Db.beginTx() )
			  {
					foo = Db.createNode( label( "Foo" ) );
					bar = Db.createNode( label( "Bar" ) );
					foo.CreateRelationshipTo( bar, withName( "KNOWS" ) );

					tx.Success();
			  }

			  // when
			  using ( Transaction tx = Db.beginTx() )
			  {
					foo.RemoveLabel( label( "Foo" ) );

					tx.Success();
			  }

			  // then
			  NumberOfRelationshipsMatching( label( "Foo" ), withName( "KNOWS" ), null ).shouldBe( 0 );
			  NumberOfRelationshipsMatching( null, withName( "KNOWS" ), label( "Foo" ) ).shouldBe( 0 );
			  NumberOfRelationshipsMatching( null, withName( "KNOWS" ), label( "Bar" ) ).shouldBe( 1 );
			  NumberOfRelationshipsMatching( label( "Bar" ), withName( "KNOWS" ), null ).shouldBe( 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMaintainCountsOnLabelAddAndRelationshipCreate()
		 public virtual void ShouldMaintainCountsOnLabelAddAndRelationshipCreate()
		 {
			  // given
			  Node foo;
			  Node bar;
			  using ( Transaction tx = Db.beginTx() )
			  {
					foo = Db.createNode( label( "Foo" ) );
					bar = Db.createNode( label( "Bar" ) );
					foo.CreateRelationshipTo( bar, withName( "KNOWS" ) );

					tx.Success();
			  }

			  // when
			  using ( Transaction tx = Db.beginTx() )
			  {
					foo.AddLabel( label( "Bar" ) );
					foo.CreateRelationshipTo( Db.createNode( label( "Foo" ) ), withName( "KNOWS" ) );

					tx.Success();
			  }

			  // then
			  NumberOfRelationshipsMatching( label( "Foo" ), withName( "KNOWS" ), null ).shouldBe( 2 );
			  NumberOfRelationshipsMatching( null, withName( "KNOWS" ), label( "Foo" ) ).shouldBe( 1 );
			  NumberOfRelationshipsMatching( null, withName( "KNOWS" ), label( "Bar" ) ).shouldBe( 1 );
			  NumberOfRelationshipsMatching( label( "Bar" ), withName( "KNOWS" ), null ).shouldBe( 2 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMaintainCountsOnLabelRemoveAndRelationshipDelete()
		 public virtual void ShouldMaintainCountsOnLabelRemoveAndRelationshipDelete()
		 {
			  // given
			  Node foo;
			  Node bar;
			  Relationship rel;
			  using ( Transaction tx = Db.beginTx() )
			  {
					foo = Db.createNode( label( "Foo" ), label( "Bar" ) );
					bar = Db.createNode( label( "Bar" ) );
					foo.CreateRelationshipTo( bar, withName( "KNOWS" ) );
					rel = bar.CreateRelationshipTo( foo, withName( "KNOWS" ) );

					tx.Success();
			  }

			  // when
			  using ( Transaction tx = Db.beginTx() )
			  {
					foo.RemoveLabel( label( "Bar" ) );
					rel.Delete();

					tx.Success();
			  }

			  // then
			  NumberOfRelationshipsMatching( label( "Foo" ), withName( "KNOWS" ), null ).shouldBe( 1 );
			  NumberOfRelationshipsMatching( null, withName( "KNOWS" ), label( "Foo" ) ).shouldBe( 0 );
			  NumberOfRelationshipsMatching( null, withName( "KNOWS" ), label( "Bar" ) ).shouldBe( 1 );
			  NumberOfRelationshipsMatching( label( "Bar" ), withName( "KNOWS" ), null ).shouldBe( 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMaintainCountsOnLabelAddAndRelationshipDelete()
		 public virtual void ShouldMaintainCountsOnLabelAddAndRelationshipDelete()
		 {
			  // given
			  Node foo;
			  Node bar;
			  Relationship rel;
			  using ( Transaction tx = Db.beginTx() )
			  {
					foo = Db.createNode( label( "Foo" ) );
					bar = Db.createNode( label( "Bar" ) );
					foo.CreateRelationshipTo( bar, withName( "KNOWS" ) );
					rel = bar.CreateRelationshipTo( foo, withName( "KNOWS" ) );

					tx.Success();
			  }

			  // when
			  using ( Transaction tx = Db.beginTx() )
			  {
					foo.AddLabel( label( "Bar" ) );
					rel.Delete();

					tx.Success();
			  }

			  // then
			  NumberOfRelationshipsMatching( label( "Foo" ), withName( "KNOWS" ), null ).shouldBe( 1 );
			  NumberOfRelationshipsMatching( null, withName( "KNOWS" ), label( "Foo" ) ).shouldBe( 0 );
			  NumberOfRelationshipsMatching( null, withName( "KNOWS" ), label( "Bar" ) ).shouldBe( 1 );
			  NumberOfRelationshipsMatching( label( "Bar" ), withName( "KNOWS" ), null ).shouldBe( 1 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldMaintainCountsOnLabelRemoveAndRelationshipCreate()
		 public virtual void ShouldMaintainCountsOnLabelRemoveAndRelationshipCreate()
		 {
			  // given
			  Node foo;
			  Node bar;
			  using ( Transaction tx = Db.beginTx() )
			  {
					foo = Db.createNode( label( "Foo" ), label( "Bar" ) );
					bar = Db.createNode( label( "Bar" ) );
					foo.CreateRelationshipTo( bar, withName( "KNOWS" ) );

					tx.Success();
			  }

			  // when
			  using ( Transaction tx = Db.beginTx() )
			  {
					foo.RemoveLabel( label( "Bar" ) );
					foo.CreateRelationshipTo( Db.createNode( label( "Foo" ) ), withName( "KNOWS" ) );

					tx.Success();
			  }

			  // then
			  NumberOfRelationshipsMatching( label( "Foo" ), withName( "KNOWS" ), null ).shouldBe( 2 );
			  NumberOfRelationshipsMatching( null, withName( "KNOWS" ), label( "Foo" ) ).shouldBe( 1 );
			  NumberOfRelationshipsMatching( null, withName( "KNOWS" ), label( "Bar" ) ).shouldBe( 1 );
			  NumberOfRelationshipsMatching( label( "Bar" ), withName( "KNOWS" ), null ).shouldBe( 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotUpdateCountsIfCreatedRelationshipIsDeletedInSameTransaction()
		 public virtual void ShouldNotUpdateCountsIfCreatedRelationshipIsDeletedInSameTransaction()
		 {
			  // given
			  Node foo;
			  Node bar;
			  using ( Transaction tx = Db.beginTx() )
			  {
					foo = Db.createNode( label( "Foo" ) );
					bar = Db.createNode( label( "Bar" ) );

					tx.Success();
			  }

			  // when
			  using ( Transaction tx = Db.beginTx() )
			  {
					foo.CreateRelationshipTo( bar, withName( "KNOWS" ) ).delete();

					tx.Success();
			  }

			  // then
			  NumberOfRelationshipsMatching( label( "Foo" ), withName( "KNOWS" ), null ).shouldBe( 0 );
			  NumberOfRelationshipsMatching( label( "Bar" ), withName( "KNOWS" ), null ).shouldBe( 0 );
			  NumberOfRelationshipsMatching( null, withName( "KNOWS" ), label( "Foo" ) ).shouldBe( 0 );
			  NumberOfRelationshipsMatching( null, withName( "KNOWS" ), label( "Bar" ) ).shouldBe( 0 );
		 }

		 /// <summary>
		 /// Transactional version of <seealso cref="countsForRelationship(Label, RelationshipType, Label)"/>
		 /// </summary>
		 private MatchingRelationships NumberOfRelationshipsMatching( Label lhs, RelationshipType type, Label rhs )
		 {
			  using ( Transaction tx = Db.GraphDatabaseAPI.beginTx() )
			  {
					long nodeCount = CountsForRelationship( lhs, type, rhs );
					tx.Success();
					return new MatchingRelationships( string.Format( "({0})-{1}->({2})", lhs == null ? "" : ":" + lhs.Name(), type == null ? "" : "[:" + type.Name() + "]", rhs == null ? "" : ":" + rhs.Name() ), nodeCount );
			  }
		 }

		 private class MatchingRelationships
		 {
			  internal readonly string Message;
			  internal readonly long Count;

			  internal MatchingRelationships( string message, long count )
			  {
					this.Message = message;
					this.Count = count;
			  }

			  public virtual void ShouldBe( long expected )
			  {
					assertEquals( Message, expected, Count );
			  }
		 }

		 /// <param name="start"> the label of the start node of relationships to get the number of, or {@code null} for "any". </param>
		 /// <param name="type">  the type of the relationships to get the number of, or {@code null} for "any". </param>
		 /// <param name="end">   the label of the end node of relationships to get the number of, or {@code null} for "any". </param>
		 private long CountsForRelationship( Label start, RelationshipType type, Label end )
		 {
			  KernelTransaction transaction = _transactionSupplier.get();
			  using ( Statement ignore = transaction.AcquireStatement() )
			  {
					TokenRead tokenRead = transaction.TokenRead();
					int startId;
					int typeId;
					int endId;
					// start
					if ( start == null )
					{
						 startId = StatementConstants.ANY_LABEL;
					}
					else
					{
						 if ( Neo4Net.@internal.Kernel.Api.TokenRead_Fields.NO_TOKEN == ( startId = tokenRead.NodeLabel( start.Name() ) ) )
						 {
							  return 0;
						 }
					}
					// type
					if ( type == null )
					{
						 typeId = Neo4Net.@internal.Kernel.Api.TokenRead_Fields.NO_TOKEN;
					}
					else
					{
						 if ( Neo4Net.@internal.Kernel.Api.TokenRead_Fields.NO_TOKEN == ( typeId = tokenRead.RelationshipType( type.Name() ) ) )
						 {
							  return 0;
						 }
					}
					// end
					if ( end == null )
					{
						 endId = StatementConstants.ANY_LABEL;
					}
					else
					{
						 if ( Neo4Net.@internal.Kernel.Api.TokenRead_Fields.NO_TOKEN == ( endId = tokenRead.NodeLabel( end.Name() ) ) )
						 {
							  return 0;
						 }
					}
					return transaction.DataRead().countsForRelationship(startId, typeId, endId);
			  }
		 }

		 private System.Func<KernelTransaction> _transactionSupplier;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Before public void exposeGuts()
		 public virtual void ExposeGuts()
		 {
			  _transactionSupplier = () => Db.GraphDatabaseAPI.DependencyResolver.resolveDependency(typeof(ThreadToStatementContextBridge)).getKernelTransactionBoundToThisThread(true);
		 }
	}

}