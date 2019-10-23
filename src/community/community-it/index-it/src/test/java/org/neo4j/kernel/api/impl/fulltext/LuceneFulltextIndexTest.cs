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
namespace Neo4Net.Kernel.Api.Impl.Fulltext
{
	using Test = org.junit.Test;

	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using IndexReference = Neo4Net.Kernel.Api.Internal.IndexReference;
	using SchemaDescriptor = Neo4Net.Kernel.Api.Internal.schema.SchemaDescriptor;
	using KernelTransactionImplementation = Neo4Net.Kernel.Impl.Api.KernelTransactionImplementation;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.StorageEngine.EntityType.NODE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.Kernel.Api.StorageEngine.EntityType.RELATIONSHIP;

	public class LuceneFulltextIndexTest : LuceneFulltextTestSupport
	{
		 private const string NODE_INDEX_NAME = "nodes";
		 private const string REL_INDEX_NAME = "rels";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFindNodeWithString() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFindNodeWithString()
		 {
			  IndexReference index;
			  using ( KernelTransactionImplementation tx = KernelTransaction )
			  {
					SchemaDescriptor descriptor = FulltextAdapter.schemaFor( NODE, new string[]{ Label.name() }, Settings, PROP );
					index = tx.SchemaWrite().indexCreate(descriptor, FulltextIndexProviderFactory.Descriptor.name(), NODE_INDEX_NAME);
					tx.Success();
			  }
			  Await( index );
			  long firstID;
			  long secondID;
			  using ( Transaction tx = Db.beginTx() )
			  {
					firstID = CreateNodeIndexableByPropertyValue( Label, "Hello. Hello again." );
					secondID = CreateNodeIndexableByPropertyValue( Label, "A zebroid (also zedonk, zorse, zebra mule, zonkey, and zebmule) is the offspring of any " + "cross between a zebra and any other equine: essentially, a zebra hybrid." );

					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					KernelTransaction ktx = KernelTransaction( tx );
					AssertQueryFindsIds( ktx, NODE_INDEX_NAME, "hello", firstID );
					AssertQueryFindsIds( ktx, NODE_INDEX_NAME, "zebra", secondID );
					AssertQueryFindsIds( ktx, NODE_INDEX_NAME, "zedonk", secondID );
					AssertQueryFindsIds( ktx, NODE_INDEX_NAME, "cross", secondID );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldRepresentPropertyChanges() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldRepresentPropertyChanges()
		 {
			  IndexReference index;
			  using ( KernelTransactionImplementation tx = KernelTransaction )
			  {
					SchemaDescriptor descriptor = FulltextAdapter.schemaFor( NODE, new string[]{ Label.name() }, Settings, PROP );
					index = tx.SchemaWrite().indexCreate(descriptor, FulltextIndexProviderFactory.Descriptor.name(), NODE_INDEX_NAME);
					tx.Success();
			  }
			  Await( index );

			  long firstID;
			  long secondID;
			  using ( Transaction tx = Db.beginTx() )
			  {
					firstID = CreateNodeIndexableByPropertyValue( Label, "Hello. Hello again." );
					secondID = CreateNodeIndexableByPropertyValue( Label, "A zebroid (also zedonk, zorse, zebra mule, zonkey, and zebmule) is the offspring of any " + "cross between a zebra and any other equine: essentially, a zebra hybrid." );

					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					SetNodeProp( firstID, "Finally! Potato!" );
					SetNodeProp( secondID, "This one is a potato farmer." );

					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					KernelTransaction ktx = KernelTransaction( tx );
					AssertQueryFindsNothing( ktx, NODE_INDEX_NAME, "hello" );
					AssertQueryFindsNothing( ktx, NODE_INDEX_NAME, "zebra" );
					AssertQueryFindsNothing( ktx, NODE_INDEX_NAME, "zedonk" );
					AssertQueryFindsNothing( ktx, NODE_INDEX_NAME, "cross" );
					AssertQueryFindsIds( ktx, NODE_INDEX_NAME, "finally", firstID );
					AssertQueryFindsIds( ktx, NODE_INDEX_NAME, "farmer", secondID );
					AssertQueryFindsIds( ktx, NODE_INDEX_NAME, "potato", firstID, secondID );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFindRemovedNodes() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFindRemovedNodes()
		 {
			  IndexReference index;
			  using ( KernelTransactionImplementation tx = KernelTransaction )
			  {
					SchemaDescriptor descriptor = FulltextAdapter.schemaFor( NODE, new string[]{ Label.name() }, Settings, PROP );
					index = tx.SchemaWrite().indexCreate(descriptor, FulltextIndexProviderFactory.Descriptor.name(), NODE_INDEX_NAME);
					tx.Success();
			  }
			  Await( index );

			  long firstID;
			  long secondID;
			  using ( Transaction tx = Db.beginTx() )
			  {
					firstID = CreateNodeIndexableByPropertyValue( Label, "Hello. Hello again." );
					secondID = CreateNodeIndexableByPropertyValue( Label, "A zebroid (also zedonk, zorse, zebra mule, zonkey, and zebmule) is the offspring of any " + "cross between a zebra and any other equine: essentially, a zebra hybrid." );

					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.getNodeById( firstID ).delete();
					Db.getNodeById( secondID ).delete();

					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					KernelTransaction ktx = KernelTransaction( tx );
					AssertQueryFindsNothing( ktx, NODE_INDEX_NAME, "hello" );
					AssertQueryFindsNothing( ktx, NODE_INDEX_NAME, "zebra" );
					AssertQueryFindsNothing( ktx, NODE_INDEX_NAME, "zedonk" );
					AssertQueryFindsNothing( ktx, NODE_INDEX_NAME, "cross" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotFindRemovedProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotFindRemovedProperties()
		 {
			  IndexReference index;
			  using ( KernelTransactionImplementation tx = KernelTransaction )
			  {
					SchemaDescriptor descriptor = FulltextAdapter.schemaFor( NODE, new string[]{ Label.name() }, Settings, "prop", "prop2" );
					index = tx.SchemaWrite().indexCreate(descriptor, FulltextIndexProviderFactory.Descriptor.name(), NODE_INDEX_NAME);
					tx.Success();
			  }
			  Await( index );
			  long firstID;
			  long secondID;
			  long thirdID;
			  using ( Transaction tx = Db.beginTx() )
			  {
					firstID = CreateNodeIndexableByPropertyValue( Label, "Hello. Hello again." );
					secondID = CreateNodeIndexableByPropertyValue( Label, "A zebroid (also zedonk, zorse, zebra mule, zonkey, and zebmule) is the offspring of any " + "cross between a zebra and any other equine: essentially, a zebra hybrid." );
					thirdID = CreateNodeIndexableByPropertyValue( Label, "Hello. Hello again." );

					SetNodeProp( firstID, "zebra" );
					SetNodeProp( secondID, "Hello. Hello again." );

					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					Node node = Db.getNodeById( firstID );
					Node node2 = Db.getNodeById( secondID );
					Node node3 = Db.getNodeById( thirdID );

					node.SetProperty( "prop", "tomtar" );
					node.SetProperty( "prop2", "tomtar" );

					node2.SetProperty( "prop", "tomtar" );
					node2.SetProperty( "prop2", "Hello" );

					node3.RemoveProperty( "prop" );

					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					KernelTransaction ktx = KernelTransaction( tx );
					AssertQueryFindsIds( ktx, NODE_INDEX_NAME, "hello", secondID );
					AssertQueryFindsNothing( ktx, NODE_INDEX_NAME, "zebra" );
					AssertQueryFindsNothing( ktx, NODE_INDEX_NAME, "zedonk" );
					AssertQueryFindsNothing( ktx, NODE_INDEX_NAME, "cross" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOnlyIndexIndexedProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOnlyIndexIndexedProperties()
		 {
			  IndexReference index;
			  using ( KernelTransactionImplementation tx = KernelTransaction )
			  {
					SchemaDescriptor descriptor = FulltextAdapter.schemaFor( NODE, new string[]{ Label.name() }, Settings, PROP );
					index = tx.SchemaWrite().indexCreate(descriptor, FulltextIndexProviderFactory.Descriptor.name(), NODE_INDEX_NAME);
					tx.Success();
			  }
			  Await( index );

			  long firstID;
			  using ( Transaction tx = Db.beginTx() )
			  {
					firstID = CreateNodeIndexableByPropertyValue( Label, "Hello. Hello again." );
					SetNodeProp( firstID, "prop2", "zebra" );

					Node node2 = Db.createNode( Label );
					node2.SetProperty( "prop2", "zebra" );
					node2.SetProperty( "prop3", "hello" );

					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					KernelTransaction ktx = KernelTransaction( tx );
					AssertQueryFindsIds( ktx, NODE_INDEX_NAME, "hello", firstID );
					AssertQueryFindsNothing( ktx, NODE_INDEX_NAME, "zebra" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldSearchAcrossMultipleProperties() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldSearchAcrossMultipleProperties()
		 {
			  IndexReference index;
			  using ( KernelTransactionImplementation tx = KernelTransaction )
			  {
					SchemaDescriptor descriptor = FulltextAdapter.schemaFor( NODE, new string[]{ Label.name() }, Settings, "prop", "prop2" );
					index = tx.SchemaWrite().indexCreate(descriptor, FulltextIndexProviderFactory.Descriptor.name(), NODE_INDEX_NAME);
					tx.Success();
			  }
			  Await( index );

			  long firstID;
			  long secondID;
			  long thirdID;
			  using ( Transaction tx = Db.beginTx() )
			  {
					firstID = CreateNodeIndexableByPropertyValue( Label, "Tomtar tomtar oftsat i tomteutstyrsel." );
					secondID = CreateNodeIndexableByPropertyValue( Label, "Olof och Hans" );
					SetNodeProp( secondID, "prop2", "och karl" );

					Node node3 = Db.createNode( Label );
					thirdID = node3.Id;
					node3.SetProperty( "prop2", "Tomtar som inte tomtar ser upp till tomtar som tomtar." );

					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					KernelTransaction ktx = KernelTransaction( tx );
					AssertQueryFindsIds( ktx, NODE_INDEX_NAME, "tomtar Karl", firstID, secondID, thirdID );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldOrderResultsBasedOnRelevance() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldOrderResultsBasedOnRelevance()
		 {
			  IndexReference index;
			  using ( KernelTransactionImplementation tx = KernelTransaction )
			  {
					SchemaDescriptor descriptor = FulltextAdapter.schemaFor( NODE, new string[]{ Label.name() }, Settings, "first", "last" );
					index = tx.SchemaWrite().indexCreate(descriptor, FulltextIndexProviderFactory.Descriptor.name(), NODE_INDEX_NAME);
					tx.Success();
			  }
			  Await( index );
			  long firstID;
			  long secondID;
			  long thirdID;
			  long fourthID;
			  using ( Transaction tx = Db.beginTx() )
			  {
					firstID = Db.createNode( Label ).Id;
					secondID = Db.createNode( Label ).Id;
					thirdID = Db.createNode( Label ).Id;
					fourthID = Db.createNode( Label ).Id;
					SetNodeProp( firstID, "first", "Full" );
					SetNodeProp( firstID, "last", "Hanks" );
					SetNodeProp( secondID, "first", "Tom" );
					SetNodeProp( secondID, "last", "Hunk" );
					SetNodeProp( thirdID, "first", "Tom" );
					SetNodeProp( thirdID, "last", "Hanks" );
					SetNodeProp( fourthID, "first", "Tom Hanks" );
					SetNodeProp( fourthID, "last", "Tom Hanks" );

					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					KernelTransaction ktx = KernelTransaction( tx );
					AssertQueryFindsIdsInOrder( ktx, NODE_INDEX_NAME, "Tom Hanks", fourthID, thirdID, firstID, secondID );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldDifferentiateNodesAndRelationships() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldDifferentiateNodesAndRelationships()
		 {
			  SchemaDescriptor nodes = FulltextAdapter.schemaFor( NODE, new string[]{ Label.name() }, Settings, PROP );
			  SchemaDescriptor rels = FulltextAdapter.schemaFor( RELATIONSHIP, new string[]{ Reltype.name() }, Settings, PROP );
			  IndexReference nodesIndex;
			  IndexReference relsIndex;
			  using ( KernelTransactionImplementation tx = KernelTransaction )
			  {
					nodesIndex = tx.SchemaWrite().indexCreate(nodes, FulltextIndexProviderFactory.Descriptor.name(), NODE_INDEX_NAME);
					relsIndex = tx.SchemaWrite().indexCreate(rels, FulltextIndexProviderFactory.Descriptor.name(), REL_INDEX_NAME);
					tx.Success();
			  }
			  Await( nodesIndex );
			  Await( relsIndex );
			  long firstNodeID;
			  long secondNodeID;
			  long firstRelID;
			  long secondRelID;
			  using ( Transaction tx = Db.beginTx() )
			  {
					firstNodeID = CreateNodeIndexableByPropertyValue( Label, "Hello. Hello again." );
					secondNodeID = CreateNodeIndexableByPropertyValue( Label, "A zebroid (also zedonk, zorse, zebra mule, zonkey, and zebmule) is the offspring of any " + "cross between a zebra and any other equine: essentially, a zebra hybrid." );
					firstRelID = CreateRelationshipIndexableByPropertyValue( firstNodeID, secondNodeID, "Hello. Hello again." );
					secondRelID = CreateRelationshipIndexableByPropertyValue( secondNodeID, firstNodeID, "And now, something completely different" );

					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					KernelTransaction ktx = KernelTransaction( tx );
					AssertQueryFindsIds( ktx, NODE_INDEX_NAME, "hello", firstNodeID );
					AssertQueryFindsIds( ktx, NODE_INDEX_NAME, "zebra", secondNodeID );
					AssertQueryFindsNothing( ktx, NODE_INDEX_NAME, "different" );

					AssertQueryFindsIds( ktx, REL_INDEX_NAME, "hello", firstRelID );
					AssertQueryFindsNothing( ktx, REL_INDEX_NAME, "zebra" );
					AssertQueryFindsIds( ktx, REL_INDEX_NAME, "different", secondRelID );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReturnNonMatches() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotReturnNonMatches()
		 {
			  SchemaDescriptor nodes = FulltextAdapter.schemaFor( NODE, new string[]{ Label.name() }, Settings, PROP );
			  SchemaDescriptor rels = FulltextAdapter.schemaFor( RELATIONSHIP, new string[]{ Reltype.name() }, Settings, PROP );
			  IndexReference nodesIndex;
			  IndexReference relsIndex;
			  using ( KernelTransactionImplementation tx = KernelTransaction )
			  {
					nodesIndex = tx.SchemaWrite().indexCreate(nodes, FulltextIndexProviderFactory.Descriptor.name(), NODE_INDEX_NAME);
					relsIndex = tx.SchemaWrite().indexCreate(rels, FulltextIndexProviderFactory.Descriptor.name(), REL_INDEX_NAME);
					tx.Success();
			  }
			  Await( nodesIndex );
			  Await( relsIndex );
			  using ( Transaction tx = Db.beginTx() )
			  {
					long firstNode = CreateNodeIndexableByPropertyValue( Label, "Hello. Hello again." );
					long secondNode = CreateNodeWithProperty( Label, "prop2", "A zebroid (also zedonk, zorse, zebra mule, zonkey, and zebmule) is the offspring of any " + "cross between a zebra and any other equine: essentially, a zebra hybrid." );
					CreateRelationshipIndexableByPropertyValue( firstNode, secondNode, "Hello. Hello again." );
					CreateRelationshipWithProperty( secondNode, firstNode, "prop2", "A zebroid (also zedonk, zorse, zebra mule, zonkey, and zebmule) is the offspring of any " + "cross between a zebra and any other equine: essentially, a zebra hybrid." );

					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					KernelTransaction ktx = KernelTransaction( tx );
					AssertQueryFindsNothing( ktx, NODE_INDEX_NAME, "zebra" );
					AssertQueryFindsNothing( ktx, REL_INDEX_NAME, "zebra" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldPopulateIndexWithExistingNodesAndRelationships() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldPopulateIndexWithExistingNodesAndRelationships()
		 {
			  long firstNodeID;
			  long secondNodeID;
			  long firstRelID;
			  long secondRelID;
			  using ( Transaction tx = Db.beginTx() )
			  {
					// skip a few rel ids, so the ones we work with are different from the node ids, just in case.
					Node node = Db.createNode();
					node.CreateRelationshipTo( node, Reltype );
					node.CreateRelationshipTo( node, Reltype );
					node.CreateRelationshipTo( node, Reltype );

					firstNodeID = CreateNodeIndexableByPropertyValue( Label, "Hello. Hello again." );
					secondNodeID = CreateNodeIndexableByPropertyValue( Label, "This string is slightly shorter than the zebra one" );
					firstRelID = CreateRelationshipIndexableByPropertyValue( firstNodeID, secondNodeID, "Goodbye" );
					secondRelID = CreateRelationshipIndexableByPropertyValue( secondNodeID, firstNodeID, "And now, something completely different" );

					tx.Success();
			  }

			  SchemaDescriptor nodes = FulltextAdapter.schemaFor( NODE, new string[]{ Label.name() }, Settings, PROP );
			  SchemaDescriptor rels = FulltextAdapter.schemaFor( RELATIONSHIP, new string[]{ Reltype.name() }, Settings, PROP );
			  IndexReference nodesIndex;
			  IndexReference relsIndex;
			  using ( KernelTransactionImplementation tx = KernelTransaction )
			  {
					nodesIndex = tx.schemaWrite().indexCreate(nodes, FulltextIndexProviderFactory.Descriptor.name(), NODE_INDEX_NAME);
					relsIndex = tx.schemaWrite().indexCreate(rels, FulltextIndexProviderFactory.Descriptor.name(), REL_INDEX_NAME);
					tx.Success();
			  }
			  Await( nodesIndex );
			  Await( relsIndex );
			  using ( Transaction tx = Db.beginTx() )
			  {
					KernelTransaction ktx = KernelTransaction( tx );
					AssertQueryFindsIds( ktx, NODE_INDEX_NAME, "hello", firstNodeID );
					AssertQueryFindsIds( ktx, NODE_INDEX_NAME, "string", secondNodeID );
					AssertQueryFindsNothing( ktx, NODE_INDEX_NAME, "goodbye" );
					AssertQueryFindsNothing( ktx, NODE_INDEX_NAME, "different" );

					AssertQueryFindsNothing( ktx, REL_INDEX_NAME, "hello" );
					AssertQueryFindsNothing( ktx, REL_INDEX_NAME, "string" );
					AssertQueryFindsIds( ktx, REL_INDEX_NAME, "goodbye", firstRelID );
					AssertQueryFindsIds( ktx, REL_INDEX_NAME, "different", secondRelID );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToUpdateAndQueryAfterIndexChange() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToUpdateAndQueryAfterIndexChange()
		 {
			  IndexReference index;
			  using ( KernelTransactionImplementation tx = KernelTransaction )
			  {
					SchemaDescriptor descriptor = FulltextAdapter.schemaFor( NODE, new string[]{ Label.name() }, Settings, PROP );
					index = tx.SchemaWrite().indexCreate(descriptor, FulltextIndexProviderFactory.Descriptor.name(), NODE_INDEX_NAME);
					tx.Success();
			  }
			  Await( index );

			  long firstID;
			  long secondID;
			  long thirdID;
			  long fourthID;
			  using ( Transaction tx = Db.beginTx() )
			  {
					firstID = CreateNodeIndexableByPropertyValue( Label, "thing" );

					secondID = Db.createNode( Label ).Id;
					SetNodeProp( secondID, "prop2", "zebra" );

					thirdID = CreateNodeIndexableByPropertyValue( Label, "zebra" );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					KernelTransaction ktx = KernelTransaction( tx );
					AssertQueryFindsIds( ktx, NODE_INDEX_NAME, "thing zebra", firstID, thirdID );
			  }

			  using ( KernelTransactionImplementation tx = KernelTransaction )
			  {
					SchemaDescriptor descriptor = FulltextAdapter.schemaFor( NODE, new string[]{ Label.name() }, Settings, "prop2" );
					tx.SchemaWrite().indexDrop(index);
					index = tx.SchemaWrite().indexCreate(descriptor, FulltextIndexProviderFactory.Descriptor.name(), NODE_INDEX_NAME);
					tx.Success();
			  }
			  Await( index );

			  using ( Transaction tx = Db.beginTx() )
			  {
					SetNodeProp( firstID, "prop2", "thing" );

					fourthID = Db.createNode( Label ).Id;
					SetNodeProp( fourthID, "prop2", "zebra" );
					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					KernelTransaction ktx = KernelTransaction( tx );
					AssertQueryFindsIds( ktx, NODE_INDEX_NAME, "thing zebra", firstID, secondID, fourthID );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToDropAndReadIndex() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToDropAndReadIndex()
		 {
			  SchemaDescriptor descriptor = FulltextAdapter.schemaFor( NODE, new string[]{ Label.name() }, Settings, PROP );
			  IndexReference index;
			  using ( KernelTransactionImplementation tx = KernelTransaction )
			  {
					index = tx.SchemaWrite().indexCreate(descriptor, FulltextIndexProviderFactory.Descriptor.name(), NODE_INDEX_NAME);
					tx.Success();
			  }
			  Await( index );

			  long firstID;
			  long secondID;

			  using ( Transaction tx = Db.beginTx() )
			  {
					firstID = CreateNodeIndexableByPropertyValue( Label, "thing" );

					secondID = CreateNodeIndexableByPropertyValue( Label, "zebra" );
					tx.Success();
			  }

			  using ( KernelTransactionImplementation tx = KernelTransaction )
			  {
					tx.SchemaWrite().indexDrop(index);
					tx.Success();
			  }
			  using ( KernelTransactionImplementation tx = KernelTransaction )
			  {
					index = tx.SchemaWrite().indexCreate(descriptor, FulltextIndexProviderFactory.Descriptor.name(), NODE_INDEX_NAME);
					tx.Success();
			  }
			  Await( index );

			  using ( Transaction tx = Db.beginTx() )
			  {
					KernelTransaction ktx = KernelTransaction( tx );
					AssertQueryFindsIds( ktx, NODE_INDEX_NAME, "thing zebra", firstID, secondID );
			  }
		 }
	}

}