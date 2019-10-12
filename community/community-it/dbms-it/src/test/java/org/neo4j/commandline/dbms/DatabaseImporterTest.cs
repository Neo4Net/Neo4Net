using System;
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
namespace Org.Neo4j.Commandline.dbms
{
	using BaseMatcher = org.hamcrest.BaseMatcher;
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using IncorrectUsage = Org.Neo4j.Commandline.admin.IncorrectUsage;
	using NullOutsideWorld = Org.Neo4j.Commandline.admin.NullOutsideWorld;
	using GraphDatabaseService = Org.Neo4j.Graphdb.GraphDatabaseService;
	using Transaction = Org.Neo4j.Graphdb.Transaction;
	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Args = Org.Neo4j.Helpers.Args;
	using Config = Org.Neo4j.Kernel.configuration.Config;
	using Validators = Org.Neo4j.Kernel.impl.util.Validators;
	using TestGraphDatabaseFactory = Org.Neo4j.Test.TestGraphDatabaseFactory;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.core.IsNot.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;

	public class DatabaseImporterTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDir = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDir = TestDirectory.testDirectory();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void requiresFromArgument()
		 public virtual void RequiresFromArgument()
		 {
			  string[] arguments = new string[] { "--mode=database", "--database=bar" };

			  try
			  {
					new DatabaseImporter( Args.parse( arguments ), Config.defaults(), new NullOutsideWorld() );

					fail( "Should have thrown an exception." );
			  }
			  catch ( IncorrectUsage e )
			  {
					assertThat( e.Message, containsString( "from" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failIfSourceIsNotAStore()
		 public virtual void FailIfSourceIsNotAStore()
		 {
			  File from = TestDir.directory( "empty" );
			  string[] arguments = new string[] { "--mode=database", "--database=bar", "--from=" + from.AbsolutePath };

			  try
			  {
					new DatabaseImporter( Args.parse( arguments ), Config.defaults(), new NullOutsideWorld() );
					fail( "Should have thrown an exception." );
			  }
			  catch ( IncorrectUsage e )
			  {
					assertThat( e.Message, containsString( "does not contain a database" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void copiesDatabaseFromOldLocationToNewLocation() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void CopiesDatabaseFromOldLocationToNewLocation()
		 {
			  File home = TestDir.directory( "home" );

			  File from = ProvideStoreDirectory();
			  File destination = new File( new File( new File( home, "data" ), "databases" ), "bar" );

			  string[] arguments = new string[] { "--mode=database", "--database=bar", "--from=" + from.AbsolutePath };

			  DatabaseImporter importer = new DatabaseImporter( Args.parse( arguments ), GetConfigWith( home, "bar" ), new NullOutsideWorld() );
			  assertThat( destination, not( ExistingDatabase ) );
			  importer.DoImport();
			  assertThat( destination, ExistingDatabase );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void removesOldMessagesLog() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void RemovesOldMessagesLog()
		 {
			  File home = TestDir.directory();

			  File from = ProvideStoreDirectory();
			  File oldMessagesLog = new File( from, "messages.log" );

			  assertTrue( oldMessagesLog.createNewFile() );

			  File destination = TestDir.databaseDir();

			  string[] arguments = new string[] { "--mode=database", "--database=bar", "--from=" + from.AbsolutePath };
			  DatabaseImporter importer = new DatabaseImporter( Args.parse( arguments ), GetConfigWith( home, "bar" ), new NullOutsideWorld() );

			  File messagesLog = new File( destination, "messages.log" );
			  importer.DoImport();
			  assertFalse( messagesLog.exists() );
		 }

		 private static Config GetConfigWith( File homeDir, string databaseName )
		 {
			  Dictionary<string, string> additionalConfig = new Dictionary<string, string>();
			  additionalConfig[GraphDatabaseSettings.neo4j_home.name()] = homeDir.ToString();
			  additionalConfig[GraphDatabaseSettings.active_database.name()] = databaseName;
			  return Config.defaults( additionalConfig );
		 }

		 private File ProvideStoreDirectory()
		 {
			  GraphDatabaseService db = null;
			  File homeStoreDir = TestDir.databaseDir( "home" );
			  try
			  {
					db = ( new TestGraphDatabaseFactory() ).newEmbeddedDatabase(homeStoreDir);
					using ( Transaction transaction = Db.beginTx() )
					{
						 Db.createNode();
						 transaction.Success();
					}
			  }
			  finally
			  {
					if ( db != null )
					{
						 Db.shutdown();
					}
			  }

			  return homeStoreDir;
		 }

		 private static Matcher<File> ExistingDatabase
		 {
			 get
			 {
				  return new BaseMatcherAnonymousInnerClass();
			 }
		 }

		 private class BaseMatcherAnonymousInnerClass : BaseMatcher<File>
		 {
//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public boolean matches(final Object item)
			 public override bool matches( object item )
			 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.io.File store = (java.io.File) item;
				  File store = ( File ) item;
				  try
				  {
						Validators.CONTAINS_EXISTING_DATABASE.validate( store );
						return true;
				  }
				  catch ( Exception )
				  {
						return false;
				  }
			 }

			 public override void describeTo( Description description )
			 {
				  description.appendText( "an existing database." );
			 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public void describeMismatch(final Object item, final org.hamcrest.Description description)
			 public override void describeMismatch( object item, Description description )
			 {
				  description.appendValue( item ).appendText( " is not an existing database." );
			 }
		 }
	}

}