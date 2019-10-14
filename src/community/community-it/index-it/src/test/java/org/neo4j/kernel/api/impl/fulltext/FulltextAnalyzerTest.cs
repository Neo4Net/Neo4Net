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

	using Transaction = Neo4Net.Graphdb.Transaction;
	using IndexReference = Neo4Net.Internal.Kernel.Api.IndexReference;
	using SchemaRead = Neo4Net.Internal.Kernel.Api.SchemaRead;
	using SchemaWrite = Neo4Net.Internal.Kernel.Api.SchemaWrite;
	using SchemaDescriptor = Neo4Net.Internal.Kernel.Api.schema.SchemaDescriptor;
	using KernelTransactionImplementation = Neo4Net.Kernel.Impl.Api.KernelTransactionImplementation;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.storageengine.api.EntityType.NODE;

	public class FulltextAnalyzerTest : LuceneFulltextTestSupport
	{
		 public const string ENGLISH = "english";
		 public const string SWEDISH = "swedish";
		 public const string FOLDING = "standard-folding";

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToSpecifyEnglishAnalyzer() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToSpecifyEnglishAnalyzer()
		 {
			  ApplySetting( FulltextConfig.FulltextDefaultAnalyzer, ENGLISH );

			  SchemaDescriptor descriptor = FulltextAdapter.schemaFor( NODE, new string[]{ Label.name() }, Settings, PROP );
			  IndexReference nodes;
			  using ( KernelTransactionImplementation transaction = KernelTransaction )
			  {
					SchemaWrite schemaWrite = transaction.SchemaWrite();
					nodes = schemaWrite.IndexCreate( descriptor, FulltextIndexProviderFactory.Descriptor.name(), "nodes" );
					transaction.Success();
			  }
			  Await( nodes );

			  long id;
			  using ( Transaction tx = Db.beginTx() )
			  {
					CreateNodeIndexableByPropertyValue( Label, "Hello and hello again, in the end." );
					id = CreateNodeIndexableByPropertyValue( Label, "En apa och en tomte bodde i ett hus." );

					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					KernelTransaction ktx = KernelTransaction( tx );
					AssertQueryFindsNothing( ktx, "nodes", "and" );
					AssertQueryFindsNothing( ktx, "nodes", "in" );
					AssertQueryFindsNothing( ktx, "nodes", "the" );
					AssertQueryFindsIds( ktx, "nodes", "en", id );
					AssertQueryFindsIds( ktx, "nodes", "och", id );
					AssertQueryFindsIds( ktx, "nodes", "ett", id );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToSpecifySwedishAnalyzer() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToSpecifySwedishAnalyzer()
		 {
			  ApplySetting( FulltextConfig.FulltextDefaultAnalyzer, SWEDISH );
			  SchemaDescriptor descriptor = FulltextAdapter.schemaFor( NODE, new string[]{ Label.name() }, Settings, PROP );
			  IndexReference nodes;
			  using ( KernelTransactionImplementation transaction = KernelTransaction )
			  {
					SchemaWrite schemaWrite = transaction.SchemaWrite();
					nodes = schemaWrite.IndexCreate( descriptor, FulltextIndexProviderFactory.Descriptor.name(), "nodes" );
					transaction.Success();
			  }
			  Await( nodes );

			  long id;
			  using ( Transaction tx = Db.beginTx() )
			  {
					id = CreateNodeIndexableByPropertyValue( Label, "Hello and hello again, in the end." );
					CreateNodeIndexableByPropertyValue( Label, "En apa och en tomte bodde i ett hus." );

					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					KernelTransaction ktx = KernelTransaction( tx );
					AssertQueryFindsIds( ktx, "nodes", "and", id );
					AssertQueryFindsIds( ktx, "nodes", "in", id );
					AssertQueryFindsIds( ktx, "nodes", "the", id );
					AssertQueryFindsNothing( ktx, "nodes", "en" );
					AssertQueryFindsNothing( ktx, "nodes", "och" );
					AssertQueryFindsNothing( ktx, "nodes", "ett" );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeAbleToSpecifyFoldingAnalyzer() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldBeAbleToSpecifyFoldingAnalyzer()
		 {
			  ApplySetting( FulltextConfig.FulltextDefaultAnalyzer, FOLDING );
			  SchemaDescriptor descriptor = FulltextAdapter.schemaFor( NODE, new string[]{ Label.name() }, Settings, PROP );
			  IndexReference nodes;
			  using ( KernelTransactionImplementation transaction = KernelTransaction )
			  {
					SchemaWrite schemaWrite = transaction.SchemaWrite();
					nodes = schemaWrite.IndexCreate( descriptor, FulltextIndexProviderFactory.Descriptor.name(), "nodes" );
					transaction.Success();
			  }
			  Await( nodes );

			  long id;
			  using ( Transaction tx = Db.beginTx() )
			  {
					id = CreateNodeIndexableByPropertyValue( Label, "Příliš žluťoučký kůň úpěl ďábelské ódy." );

					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					KernelTransaction ktx = KernelTransaction( tx );
					AssertQueryFindsIds( ktx, "nodes", "prilis", id );
					AssertQueryFindsIds( ktx, "nodes", "zlutoucky", id );
					AssertQueryFindsIds( ktx, "nodes", "kun", id );
					AssertQueryFindsIds( ktx, "nodes", "upel", id );
					AssertQueryFindsIds( ktx, "nodes", "dabelske", id );
					AssertQueryFindsIds( ktx, "nodes", "ody", id );
			  }

		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotReindexNodesWhenDefaultAnalyzerIsChanged() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldNotReindexNodesWhenDefaultAnalyzerIsChanged()
		 {
			  long firstID;
			  long secondID;
			  ApplySetting( FulltextConfig.FulltextDefaultAnalyzer, ENGLISH );
			  SchemaDescriptor descriptor = FulltextAdapter.schemaFor( NODE, new string[]{ Label.name() }, Settings, PROP );
			  IndexReference nodes;
			  using ( KernelTransactionImplementation transaction = KernelTransaction )
			  {
					SchemaWrite schemaWrite = transaction.SchemaWrite();
					nodes = schemaWrite.IndexCreate( descriptor, FulltextIndexProviderFactory.Descriptor.name(), "nodes" );
					transaction.Success();
			  }
			  Await( nodes );

			  using ( Transaction tx = Db.beginTx() )
			  {
					firstID = CreateNodeIndexableByPropertyValue( Label, "Hello and hello again, in the end." );
					secondID = CreateNodeIndexableByPropertyValue( Label, "En apa och en tomte bodde i ett hus." );

					tx.Success();
			  }

			  using ( Transaction tx = Db.beginTx() )
			  {
					KernelTransaction ktx = KernelTransaction( tx );
					AssertQueryFindsNothing( ktx, "nodes", "and" );
					AssertQueryFindsNothing( ktx, "nodes", "in" );
					AssertQueryFindsNothing( ktx, "nodes", "the" );
					AssertQueryFindsIds( ktx, "nodes", "en", secondID );
					AssertQueryFindsIds( ktx, "nodes", "och", secondID );
					AssertQueryFindsIds( ktx, "nodes", "ett", secondID );
			  }

			  ApplySetting( FulltextConfig.FulltextDefaultAnalyzer, SWEDISH );
			  using ( KernelTransactionImplementation ktx = KernelTransaction )
			  {
					SchemaRead schemaRead = ktx.SchemaRead();
					Await( schemaRead.IndexGetForName( "nodes" ) );
					// These results should be exactly the same as before the configuration change and restart.
					AssertQueryFindsNothing( ktx, "nodes", "and" );
					AssertQueryFindsNothing( ktx, "nodes", "in" );
					AssertQueryFindsNothing( ktx, "nodes", "the" );
					AssertQueryFindsIds( ktx, "nodes", "en", secondID );
					AssertQueryFindsIds( ktx, "nodes", "och", secondID );
					AssertQueryFindsIds( ktx, "nodes", "ett", secondID );
			  }
		 }
	}

}