using System;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Neo4Net.Graphdb
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;

	using EnterpriseGraphDatabaseFactory = Neo4Net.Graphdb.factory.EnterpriseGraphDatabaseFactory;
	using Exceptions = Neo4Net.Helpers.Exceptions;
	using StandardConstraintSemantics = Neo4Net.Kernel.impl.constraints.StandardConstraintSemantics;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class StartupConstraintSemanticsTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory dir = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory Dir = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowOpeningADatabaseWithPECInCommunityEdition()
		 public virtual void ShouldNotAllowOpeningADatabaseWithPECInCommunityEdition()
		 {
			  AssertThatCommunityCannotStartOnEnterpriseOnlyConstraint( "CREATE CONSTRAINT ON (n:Draconian) ASSERT exists(n.required)", StandardConstraintSemantics.ERROR_MESSAGE_EXISTS );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldNotAllowOpeningADatabaseWithNodeKeyInCommunityEdition()
		 public virtual void ShouldNotAllowOpeningADatabaseWithNodeKeyInCommunityEdition()
		 {
			  AssertThatCommunityCannotStartOnEnterpriseOnlyConstraint( "CREATE CONSTRAINT ON (n:Draconian) ASSERT (n.required) IS NODE KEY", StandardConstraintSemantics.ERROR_MESSAGE_NODE_KEY );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAllowOpeningADatabaseWithUniqueConstraintInCommunityEdition()
		 public virtual void ShouldAllowOpeningADatabaseWithUniqueConstraintInCommunityEdition()
		 {
			  AssertThatCommunityCanStartOnNormalConstraint( "CREATE CONSTRAINT ON (n:Draconian) ASSERT (n.required) IS UNIQUE" );
		 }

		 private void AssertThatCommunityCanStartOnNormalConstraint( string constraintCreationQuery )
		 {
			  // given
			  GraphDatabaseService graphDb = ( new EnterpriseGraphDatabaseFactory() ).newEmbeddedDatabase(Dir.storeDir());
			  try
			  {
					graphDb.Execute( constraintCreationQuery );
			  }
			  finally
			  {
					graphDb.Shutdown();
			  }
			  graphDb = null;

			  // when
			  try
			  {
					graphDb = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(Dir.storeDir());
					// Should not get exception
			  }
			  finally
			  {
					if ( graphDb != null )
					{
						 graphDb.Shutdown();
					}
			  }
		 }

		 private void AssertThatCommunityCannotStartOnEnterpriseOnlyConstraint( string constraintCreationQuery, string errorMessage )
		 {
			  // given
			  GraphDatabaseService graphDb = ( new EnterpriseGraphDatabaseFactory() ).newEmbeddedDatabase(Dir.storeDir());
			  try
			  {
					graphDb.Execute( constraintCreationQuery );
			  }
			  finally
			  {
					graphDb.Shutdown();
			  }
			  graphDb = null;

			  // when
			  try
			  {
					graphDb = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(Dir.storeDir());
					fail( "should have failed to start!" );
			  }
			  // then
			  catch ( Exception e )
			  {
					Exception error = Exceptions.rootCause( e );
					assertThat( error, instanceOf( typeof( System.InvalidOperationException ) ) );
					assertEquals( errorMessage, error.Message );
			  }
			  finally
			  {
					if ( graphDb != null )
					{
						 graphDb.Shutdown();
					}
			  }
		 }
	}

}