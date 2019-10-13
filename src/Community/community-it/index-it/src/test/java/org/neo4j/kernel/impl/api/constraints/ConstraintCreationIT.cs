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
namespace Neo4Net.Kernel.Impl.Api.constraints
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using ConstraintViolationException = Neo4Net.Graphdb.ConstraintViolationException;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using IndexFolderLayout = Neo4Net.Kernel.Api.Impl.Index.storage.layout.IndexFolderLayout;
	using IndexProvider = Neo4Net.Kernel.Api.Index.IndexProvider;
	using IndexProviderMap = Neo4Net.Kernel.Impl.Api.index.IndexProviderMap;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.SchemaIndex.NATIVE20;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.default_schema_provider;

	public class ConstraintCreationIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.EmbeddedDatabaseRule db = new org.neo4j.test.rule.EmbeddedDatabaseRule().startLazily();
		 public EmbeddedDatabaseRule Db = new EmbeddedDatabaseRule().startLazily();

		 private static readonly Label _label = Label.label( "label1" );
		 private const long INDEX_ID = 1;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLeaveLuceneIndexFilesHangingAroundIfConstraintCreationFails()
		 public virtual void ShouldNotLeaveLuceneIndexFilesHangingAroundIfConstraintCreationFails()
		 {
			  // given
			  Db.withSetting( default_schema_provider, NATIVE20.providerName() ); // <-- includes Lucene sub-provider
			  AttemptAndFailConstraintCreation();

			  // then
			  IndexProvider indexProvider = Db.DependencyResolver.resolveDependency( typeof( IndexProviderMap ) ).DefaultProvider;
			  File indexDir = indexProvider.DirectoryStructure().directoryForIndex(INDEX_ID);

			  assertFalse( ( new IndexFolderLayout( indexDir ) ).IndexFolder.exists() );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotLeaveNativeIndexFilesHangingAroundIfConstraintCreationFails()
		 public virtual void ShouldNotLeaveNativeIndexFilesHangingAroundIfConstraintCreationFails()
		 {
			  // given
			  AttemptAndFailConstraintCreation();

			  // then
			  IndexProvider indexProvider = Db.DependencyResolver.resolveDependency( typeof( IndexProviderMap ) ).DefaultProvider;
			  File indexDir = indexProvider.DirectoryStructure().directoryForIndex(INDEX_ID);

			  assertFalse( indexDir.exists() );
		 }

		 private void AttemptAndFailConstraintCreation()
		 {
			  using ( Transaction tx = Db.beginTx() )
			  {
					for ( int i = 0; i < 2; i++ )
					{
						 Node node1 = Db.createNode( _label );
						 node1.SetProperty( "prop", true );
					}

					tx.Success();
			  }

			  // when
			  try
			  {
					  using ( Transaction tx = Db.beginTx() )
					  {
						Db.schema().constraintFor(_label).assertPropertyIsUnique("prop").create();
						fail( "Should have failed with ConstraintViolationException" );
						tx.Success();
					  }
			  }
			  catch ( ConstraintViolationException )
			  {
			  }

			  // then
			  using ( Transaction ignore = Db.beginTx() )
			  {
					assertEquals( 0, Iterables.count( Db.schema().Indexes ) );
			  }
		 }
	}

}