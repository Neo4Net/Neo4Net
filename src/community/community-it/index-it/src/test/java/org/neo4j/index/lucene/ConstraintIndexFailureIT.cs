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
namespace Neo4Net.Index.lucene
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using ConstraintViolationException = Neo4Net.GraphDb.ConstraintViolationException;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using FileUtils = Neo4Net.Io.fs.FileUtils;
	using UnableToValidateConstraintException = Neo4Net.Kernel.Api.Exceptions.schema.UnableToValidateConstraintException;
	using IndexDirectoryStructure = Neo4Net.Kernel.Api.Index.IndexDirectoryStructure;
	using FailingGenericNativeIndexProviderFactory = Neo4Net.Kernel.Impl.Index.Schema.FailingGenericNativeIndexProviderFactory;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using RandomRule = Neo4Net.Test.rule.RandomRule;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.allOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.graphdb.Label.label;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.FailingGenericNativeIndexProviderFactory.FailureType.INITIAL_STATE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.kernel.impl.index.schema.FailingGenericNativeIndexProviderFactory.INITIAL_STATE_FAILURE_MESSAGE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.Neo4Net.test.TestGraphDatabaseFactory.INDEX_PROVIDERS_FILTER;

	public class ConstraintIndexFailureIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.RandomRule random = new org.Neo4Net.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.TestDirectory directory = org.Neo4Net.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Directory = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldFailToValidateConstraintsIfUnderlyingIndexIsFailed() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldFailToValidateConstraintsIfUnderlyingIndexIsFailed()
		 {
			  // given a perfectly normal constraint
			  File dir = Directory.databaseDir();
			  IGraphDatabaseService db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(dir);
			  try
			  {
					  using ( Transaction tx = Db.beginTx() )
					  {
						Db.schema().constraintFor(label("Label1")).assertPropertyIsUnique("key1").create();
						tx.Success();
					  }
			  }
			  finally
			  {
					Db.shutdown();
			  }

			  // Remove the indexes offline and start up with an index provider which reports FAILED as initial state. An ordeal, I know right...
			  FileUtils.deleteRecursively( IndexDirectoryStructure.baseSchemaIndexFolder( dir ) );
			  db = ( new TestGraphDatabaseFactory() ).removeKernelExtensions(INDEX_PROVIDERS_FILTER).addKernelExtension(new FailingGenericNativeIndexProviderFactory(INITIAL_STATE)).newEmbeddedDatabase(dir);
			  // when
			  try
			  {
					  using ( Transaction tx = Db.beginTx() )
					  {
						Db.createNode( label( "Label1" ) ).setProperty( "key1", "value1" );
						fail( "expected exception" );
					  }
			  }
			  // then
			  catch ( ConstraintViolationException e )
			  {
					assertThat( e.InnerException, instanceOf( typeof( UnableToValidateConstraintException ) ) );
					assertThat( e.InnerException.InnerException.Message, allOf( containsString( "The index is in a failed state:" ), containsString( INITIAL_STATE_FAILURE_MESSAGE ) ) );
			  }
			  finally
			  {
					Db.shutdown();
			  }
		 }
	}

}