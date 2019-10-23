using System;
using System.Collections.Generic;

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
namespace Neo4Net.Dbms.CommandLine
{
	using BaseMatcher = org.hamcrest.BaseMatcher;
	using Description = org.hamcrest.Description;
	using Matcher = org.hamcrest.Matcher;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using IncorrectUsage = Neo4Net.CommandLine.Admin.IncorrectUsage;
	using NullOutsideWorld = Neo4Net.CommandLine.Admin.NullOutsideWorld;
	using IGraphDatabaseService = Neo4Net.GraphDb.GraphDatabaseService;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using Args = Neo4Net.Helpers.Args;
	using Config = Neo4Net.Kernel.configuration.Config;
	using Validators = Neo4Net.Kernel.impl.util.Validators;
	using TestGraphDatabaseFactory = Neo4Net.Test.TestGraphDatabaseFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;

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
//ORIGINAL LINE: @Rule public final org.Neo4Net.test.rule.TestDirectory testDir = org.Neo4Net.test.rule.TestDirectory.testDirectory();
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
			  additionalConfig[GraphDatabaseSettings.Neo4Net_home.name()] = homeDir.ToString();
			  additionalConfig[GraphDatabaseSettings.active_database.name()] = databaseName;
			  return Config.defaults( additionalConfig );
		 }

		 private File ProvideStoreDirectory()
		 {
			  IGraphDatabaseService db = null;
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