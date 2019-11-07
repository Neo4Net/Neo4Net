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
namespace Neo4Net.Upgrade.Lucene
{
	using Description = org.hamcrest.Description;
	using Matchers = org.hamcrest.Matchers;
	using TypeSafeDiagnosingMatcher = org.hamcrest.TypeSafeDiagnosingMatcher;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;
	using ExpectedException = org.junit.rules.ExpectedException;


	using EmbeddedJarLoader = Neo4Net.Upgrade.Loader.EmbeddedJarLoader;
	using JarLoaderSupplier = Neo4Net.Upgrade.Loader.JarLoaderSupplier;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.upgrade.lucene.LuceneExplicitIndexUpgrader.NO_MONITOR;

	public class LuceneExplicitIndexUpgraderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.junit.rules.ExpectedException expectedException = org.junit.rules.ExpectedException.none();
		 public ExpectedException ExpectedException = ExpectedException.none();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void failOnFileMigration() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void FailOnFileMigration()
		 {
			  Path indexFolder = CreatePathForResource( "indexPretender.txt" );
			  ExpectedException.expect( typeof( System.ArgumentException ) );

			  new LuceneExplicitIndexUpgrader( indexFolder, NO_MONITOR );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void ignoreFoldersWithoutIndexes() throws java.net.URISyntaxException, ExplicitIndexMigrationException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void IgnoreFoldersWithoutIndexes()
		 {
			  Path indexFolder = CreatePathForResource( "notIndexFolder" );
			  TrackingLuceneExplicitIndexUpgrader indexUpgrader = new TrackingLuceneExplicitIndexUpgrader( indexFolder );
			  indexUpgrader.UpgradeIndexes();

			  assertTrue( indexUpgrader.MigratedIndexes.Count == 0 );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void migrateValidIndexes() throws java.net.URISyntaxException, ExplicitIndexMigrationException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MigrateValidIndexes()
		 {
			  Path indexFolder = CreatePathForResource( "indexFolder" );
			  TrackingLuceneExplicitIndexUpgrader indexUpgrader = new TrackingLuceneExplicitIndexUpgrader( indexFolder );
			  indexUpgrader.UpgradeIndexes();

			  assertThat( indexUpgrader.MigratedIndexes, Matchers.contains( "index1", "index2" ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void pointIncorrectIndexOnMigrationFailure() throws java.net.URISyntaxException, ExplicitIndexMigrationException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void PointIncorrectIndexOnMigrationFailure()
		 {
			  Path indexFolder = CreatePathForResource( "indexFolder" );
			  TrackingLuceneExplicitIndexUpgrader indexUpgrader = new TrackingLuceneExplicitIndexUpgrader( indexFolder, true );

			  ExpectedException.expect( typeof( ExplicitIndexMigrationException ) );
			  ExpectedException.expect( new ExplicitIndexMigrationExceptionBaseMatcher( "index1", "index2" ) );

			  indexUpgrader.UpgradeIndexes();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.nio.file.Path createPathForResource(String resourceName) throws java.net.URISyntaxException
		 private Path CreatePathForResource( string resourceName )
		 {
			  return Paths.get( this.GetType().ClassLoader.getResource(resourceName).toURI() );
		 }

		 private class ExplicitIndexMigrationExceptionBaseMatcher : TypeSafeDiagnosingMatcher<ExplicitIndexMigrationException>
		 {

			  internal readonly string[] FailedIndexNames;

			  internal ExplicitIndexMigrationExceptionBaseMatcher( params string[] failedIndexNames )
			  {
					this.FailedIndexNames = failedIndexNames;
			  }

			  public override void DescribeTo( Description description )
			  {
					description.appendText( "Failed index should be one of:" ).appendText( Arrays.ToString( FailedIndexNames ) );
			  }

			  protected internal override bool MatchesSafely( ExplicitIndexMigrationException item, Description mismatchDescription )
			  {
					string brokenIndexName = item.FailedIndexName;
					bool matched = Arrays.asList( FailedIndexNames ).contains( brokenIndexName );
					if ( !matched )
					{
						 mismatchDescription.appendText( "Failed index is: " ).appendText( brokenIndexName );
					}
					return matched;
			  }
		 }

		 private class TrackingLuceneExplicitIndexUpgrader : LuceneExplicitIndexUpgrader
		 {
//JAVA TO C# CONVERTER NOTE: Fields cannot have the same name as methods:
			  internal readonly ISet<string> MigratedIndexesConflict = new HashSet<string>();
			  internal readonly bool FailIndexUpgrade;

			  internal TrackingLuceneExplicitIndexUpgrader( Path indexRootPath ) : this( indexRootPath, false )
			  {
			  }

			  internal TrackingLuceneExplicitIndexUpgrader( Path indexRootPath, bool failIndexUpgrade ) : base( indexRootPath, NO_MONITOR )
			  {
					this.FailIndexUpgrade = failIndexUpgrade;
			  }

			  internal override IndexUpgraderWrapper CreateIndexUpgrader( string[] jars )
			  {
					return new IndexUpgraderWrapperStub( JarLoaderSupplier.of( jars ), MigratedIndexesConflict, FailIndexUpgrade );
			  }

			  public virtual ISet<string> MigratedIndexes
			  {
				  get
				  {
						return MigratedIndexesConflict;
				  }
			  }
		 }

		 private class IndexUpgraderWrapperStub : IndexUpgraderWrapper
		 {

			  internal readonly ISet<string> MigratedIndexes;
			  internal readonly bool FailIndexUpgrade;

			  internal IndexUpgraderWrapperStub( System.Func<EmbeddedJarLoader> jarLoaderSupplier, ISet<string> migratedIndexes, bool failIndexUpgrade ) : base( jarLoaderSupplier )
			  {
					this.MigratedIndexes = migratedIndexes;
					this.FailIndexUpgrade = failIndexUpgrade;
			  }

			  public override void UpgradeIndex( Path indexPath )
			  {
					if ( FailIndexUpgrade )
					{
						 throw new Exception( "Fail index migration: " + indexPath );
					}
					MigratedIndexes.Add( indexPath.FileName.ToString() );
			  }
		 }
	}

}