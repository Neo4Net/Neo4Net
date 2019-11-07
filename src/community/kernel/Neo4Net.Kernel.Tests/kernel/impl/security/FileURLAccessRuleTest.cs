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
namespace Neo4Net.Kernel.impl.security
{
	using Test = org.junit.Test;


	using GraphDatabaseSettings = Neo4Net.GraphDb.factory.GraphDatabaseSettings;
	using URLAccessValidationError = Neo4Net.GraphDb.security.URLAccessValidationError;
	using Config = Neo4Net.Kernel.configuration.Config;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.fail;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assume.assumeFalse;

	public class FileURLAccessRuleTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowWhenFileURLContainsAuthority() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowWhenFileURLContainsAuthority()
		 {
			  try
			  {
					URLAccessRules.FileAccess().validate(Config.defaults(), new URL("file://foo/bar/baz"));
					fail( "expected exception not thrown " );
			  }
			  catch ( URLAccessValidationError error )
			  {
					assertThat( error.Message, equalTo( "file URL may not contain an authority section (i.e. it should be 'file:///')" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowWhenFileURLContainsQuery() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowWhenFileURLContainsQuery()
		 {
			  try
			  {
					URLAccessRules.FileAccess().validate(Config.defaults(), new URL("file:///bar/baz?q=foo"));
					fail( "expected exception not thrown " );
			  }
			  catch ( URLAccessValidationError error )
			  {
					assertThat( error.Message, equalTo( "file URL may not contain a query component" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowWhenFileAccessIsDisabled() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowWhenFileAccessIsDisabled()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.net.URL url = new java.net.URL("file:///bar/baz.csv");
			  URL url = new URL( "file:///bar/baz.csv" );
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.kernel.configuration.Config config = Neo4Net.kernel.configuration.Config.defaults(Neo4Net.graphdb.factory.GraphDatabaseSettings.allow_file_urls, "false");
			  Config config = Config.defaults( GraphDatabaseSettings.allow_file_urls, "false" );
			  try
			  {
					URLAccessRules.FileAccess().validate(config, url);
					fail( "expected exception not thrown " );
			  }
			  catch ( URLAccessValidationError error )
			  {
					assertThat( error.Message, equalTo( "configuration property 'dbms.security.allow_csv_import_from_file_urls' is false" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldThrowWhenRelativePathIsOutsideImportDirectory() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldThrowWhenRelativePathIsOutsideImportDirectory()
		 {
			  assumeFalse( Paths.get( "/" ).relativize( Paths.get( "/../baz.csv" ) ).ToString().Equals("baz.csv") );
			  File importDir = ( new File( "/tmp/Neo4Nettest" ) ).AbsoluteFile;
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.kernel.configuration.Config config = Neo4Net.kernel.configuration.Config.defaults(Neo4Net.graphdb.factory.GraphDatabaseSettings.load_csv_file_url_root, importDir.toString());
			  Config config = Config.defaults( GraphDatabaseSettings.load_csv_file_url_root, importDir.ToString() );
			  try
			  {
					URLAccessRules.FileAccess().validate(config, new URL("file:///../baz.csv"));
					fail( "expected exception not thrown " );
			  }
			  catch ( URLAccessValidationError error )
			  {
					assertThat( error.Message, equalTo( "file URL points outside configured import directory" ) );
			  }
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldAdjustURLToWithinImportDirectory() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ShouldAdjustURLToWithinImportDirectory()
		 {
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final java.net.URL url = new java.io.File("/bar/baz.csv").toURI().toURL();
			  URL url = ( new File( "/bar/baz.csv" ) ).toURI().toURL();
//JAVA TO C# CONVERTER WARNING: The original Java variable was marked 'final':
//ORIGINAL LINE: final Neo4Net.kernel.configuration.Config config = Neo4Net.kernel.configuration.Config.defaults(Neo4Net.graphdb.factory.GraphDatabaseSettings.load_csv_file_url_root, "/var/lib/Neo4Net/import");
			  Config config = Config.defaults( GraphDatabaseSettings.load_csv_file_url_root, "/var/lib/Neo4Net/import" );
			  URL accessURL = URLAccessRules.FileAccess().validate(config, url);
			  URL expected = ( new File( "/var/lib/Neo4Net/import/bar/baz.csv" ) ).toURI().toURL();
			  assertEquals( expected, accessURL );
		 }
	}

}