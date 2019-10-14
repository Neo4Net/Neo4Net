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
namespace Neo4Net.Index.backup
{
	using FilenameUtils = org.apache.commons.io.FilenameUtils;
	using IndexFileNames = Org.Apache.Lucene.Index.IndexFileNames;
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using DependencyResolver = Neo4Net.Graphdb.DependencyResolver;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Neo4Net.Graphdb;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using IndexingService = Neo4Net.Kernel.Impl.Api.index.IndexingService;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Neo4Net.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasSize;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class IndexBackupIT
	{
		 private const string PROPERTY_PREFIX = "property";
		 private const int NUMBER_OF_INDEXES = 10;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.RandomRule randomRule = new org.neo4j.test.rule.RandomRule();
		 public RandomRule RandomRule = new RandomRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public org.neo4j.test.rule.EmbeddedDatabaseRule database = new org.neo4j.test.rule.EmbeddedDatabaseRule().startLazily();
		 public EmbeddedDatabaseRule Database = new EmbeddedDatabaseRule().startLazily();
		 private CheckPointer _checkPointer;
		 private IndexingService _indexingService;
		 private FileSystemAbstraction _fileSystem;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void concurrentLuceneIndexSnapshotUseDifferentSnapshots() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ConcurrentLuceneIndexSnapshotUseDifferentSnapshots()
		 {
			  Label label = Label.label( "testLabel" );
			  Database.withSetting( GraphDatabaseSettings.default_schema_provider, GraphDatabaseSettings.SchemaIndex.NATIVE20.providerName() );
			  PrepareDatabase( label );

			  ForceCheckpoint( _checkPointer );
			  ResourceIterator<File> firstCheckpointSnapshot = _indexingService.snapshotIndexFiles();
			  GenerateData( label );
			  RemoveOldNodes( LongStream.range( 1, 20 ) );
			  UpdateOldNodes( LongStream.range( 30, 40 ) );

			  ForceCheckpoint( _checkPointer );
			  ResourceIterator<File> secondCheckpointSnapshot = _indexingService.snapshotIndexFiles();

			  GenerateData( label );
			  RemoveOldNodes( LongStream.range( 50, 60 ) );
			  UpdateOldNodes( LongStream.range( 70, 80 ) );

			  ForceCheckpoint( _checkPointer );
			  ResourceIterator<File> thirdCheckpointSnapshot = _indexingService.snapshotIndexFiles();

			  ISet<string> firstSnapshotFileNames = GetFileNames( firstCheckpointSnapshot );
			  ISet<string> secondSnapshotFileNames = GetFileNames( secondCheckpointSnapshot );
			  ISet<string> thirdSnapshotFileNames = GetFileNames( thirdCheckpointSnapshot );

			  CompareSnapshotFiles( firstSnapshotFileNames, secondSnapshotFileNames, _fileSystem );
			  CompareSnapshotFiles( secondSnapshotFileNames, thirdSnapshotFileNames, _fileSystem );
			  CompareSnapshotFiles( thirdSnapshotFileNames, firstSnapshotFileNames, _fileSystem );

			  firstCheckpointSnapshot.Close();
			  secondCheckpointSnapshot.Close();
			  thirdCheckpointSnapshot.Close();
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void snapshotFilesDeletedWhenSnapshotReleased() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void SnapshotFilesDeletedWhenSnapshotReleased()
		 {
			  Label label = Label.label( "testLabel" );
			  PrepareDatabase( label );

			  ResourceIterator<File> firstCheckpointSnapshot = _indexingService.snapshotIndexFiles();
			  GenerateData( label );
			  ResourceIterator<File> secondCheckpointSnapshot = _indexingService.snapshotIndexFiles();
			  GenerateData( label );
			  ResourceIterator<File> thirdCheckpointSnapshot = _indexingService.snapshotIndexFiles();

			  ISet<string> firstSnapshotFileNames = GetFileNames( firstCheckpointSnapshot );
			  ISet<string> secondSnapshotFileNames = GetFileNames( secondCheckpointSnapshot );
			  ISet<string> thirdSnapshotFileNames = GetFileNames( thirdCheckpointSnapshot );

			  GenerateData( label );
			  ForceCheckpoint( _checkPointer );

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  assertTrue( firstSnapshotFileNames.Select( File::new ).All( _fileSystem.fileExists ) );
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  assertTrue( secondSnapshotFileNames.Select( File::new ).All( _fileSystem.fileExists ) );
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  assertTrue( thirdSnapshotFileNames.Select( File::new ).All( _fileSystem.fileExists ) );

			  firstCheckpointSnapshot.Close();
			  secondCheckpointSnapshot.Close();
			  thirdCheckpointSnapshot.Close();

			  GenerateData( label );
			  ForceCheckpoint( _checkPointer );

//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  assertFalse( firstSnapshotFileNames.Select( File::new ).Any( _fileSystem.fileExists ) );
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  assertFalse( secondSnapshotFileNames.Select( File::new ).Any( _fileSystem.fileExists ) );
//JAVA TO C# CONVERTER TODO TASK: Method reference constructor syntax is not converted by Java to C# Converter:
			  assertFalse( thirdSnapshotFileNames.Select( File::new ).Any( _fileSystem.fileExists ) );
		 }

		 private void CompareSnapshotFiles( ISet<string> firstSnapshotFileNames, ISet<string> secondSnapshotFileNames, FileSystemAbstraction fileSystem )
		 {
			  assertThat( format( "Should have %d modified index segment files. Snapshot segment files are: %s", NUMBER_OF_INDEXES, firstSnapshotFileNames ), firstSnapshotFileNames, hasSize( NUMBER_OF_INDEXES ) );
			  foreach ( string fileName in firstSnapshotFileNames )
			  {
					assertFalse( "Snapshot segments fileset should not have files from another snapshot set." + DescribeFileSets( firstSnapshotFileNames, secondSnapshotFileNames ), secondSnapshotFileNames.Contains( fileName ) );
					string path = FilenameUtils.getFullPath( fileName );
					assertTrue( "Snapshot should contain files for index in path: " + path + "." + DescribeFileSets( firstSnapshotFileNames, secondSnapshotFileNames ), secondSnapshotFileNames.Any( name => name.StartsWith( path ) ) );
					assertTrue( format( "Snapshot segment file '%s' should exist.", fileName ), fileSystem.FileExists( new File( fileName ) ) );
			  }
		 }

		 private void RemoveOldNodes( LongStream idRange )
		 {
			  using ( Transaction transaction = Database.beginTx() )
			  {
					idRange.mapToObj( id => Database.getNodeById( id ) ).forEach( Node.delete );
					transaction.Success();
			  }
		 }

		 private void UpdateOldNodes( LongStream idRange )
		 {
			  using ( Transaction transaction = Database.beginTx() )
			  {
					IList<Node> nodes = idRange.mapToObj( id => Database.getNodeById( id ) ).collect( Collectors.toList() );
					for ( int i = 0; i < NUMBER_OF_INDEXES; i++ )
					{
						 string propertyName = PROPERTY_PREFIX + i;
						 nodes.ForEach( node => node.setProperty( propertyName, RandomRule.nextLong() ) );
					}
					transaction.Success();
			  }
		 }

		 private string DescribeFileSets( ISet<string> firstFileSet, ISet<string> secondFileSet )
		 {
			  return "First snapshot files are: " + firstFileSet + Environment.NewLine +
						 "second snapshot files are: " + secondFileSet;
		 }

		 private ISet<string> GetFileNames( ResourceIterator<File> files )
		 {
//JAVA TO C# CONVERTER TODO TASK: Most Java stream collectors are not converted by Java to C# Converter:
			  return Files.Select( File.getAbsolutePath ).Where( this.segmentsFilePredicate ).collect( Collectors.toSet() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void forceCheckpoint(org.neo4j.kernel.impl.transaction.log.checkpoint.CheckPointer checkPointer) throws java.io.IOException
		 private void ForceCheckpoint( CheckPointer checkPointer )
		 {
			  checkPointer.ForceCheckPoint( new SimpleTriggerInfo( "testForcedCheckpoint" ) );
		 }

		 private void PrepareDatabase( Label label )
		 {
			  GenerateData( label );

			  using ( Transaction transaction = Database.beginTx() )
			  {
					for ( int i = 0; i < 10; i++ )
					{
						 Database.schema().indexFor(label).on(PROPERTY_PREFIX + i).create();
					}
					transaction.Success();
			  }

			  using ( Transaction ignored = Database.beginTx() )
			  {
					Database.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
			  }

			  _checkPointer = ResolveDependency( typeof( CheckPointer ) );
			  _indexingService = ResolveDependency( typeof( IndexingService ) );
			  _fileSystem = ResolveDependency( typeof( FileSystemAbstraction ) );
		 }

		 private void GenerateData( Label label )
		 {
			  for ( int i = 0; i < 100; i++ )
			  {
					TestNodeCreationTransaction( label, i );
			  }
		 }

		 private void TestNodeCreationTransaction( Label label, int i )
		 {
			  using ( Transaction transaction = Database.beginTx() )
			  {
					Node node = Database.createNode( label );
					node.SetProperty( "property" + i, i );
					transaction.Success();
			  }
		 }

		 private T ResolveDependency<T>( Type clazz )
		 {
				 clazz = typeof( T );
			  return DatabaseResolver.resolveDependency( clazz );
		 }

		 private DependencyResolver DatabaseResolver
		 {
			 get
			 {
				  return Database.DependencyResolver;
			 }
		 }

		 private bool SegmentsFilePredicate( string fileName )
		 {
			  return FilenameUtils.getName( fileName ).StartsWith( IndexFileNames.SEGMENTS );
		 }
	}

}