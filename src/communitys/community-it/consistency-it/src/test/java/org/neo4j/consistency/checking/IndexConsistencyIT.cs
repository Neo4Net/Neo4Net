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
namespace Neo4Net.Consistency.checking
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using ConsistencyCheckIncompleteException = Neo4Net.Consistency.checking.full.ConsistencyCheckIncompleteException;
	using Label = Neo4Net.Graphdb.Label;
	using Node = Neo4Net.Graphdb.Node;
	using Transaction = Neo4Net.Graphdb.Transaction;
	using Neo4Net.Helpers.Collections;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Neo4Net.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
	using DefaultIndexProviderMap = Neo4Net.Kernel.impl.transaction.state.DefaultIndexProviderMap;
	using AssertableLogProvider = Neo4Net.Logging.AssertableLogProvider;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.containsString;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.hasItem;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.progress.ProgressMonitorFactory.NONE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.io.fs.FileUtils.copyRecursively;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.TestLabels.LABEL_ONE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.TestLabels.LABEL_THREE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.test.TestLabels.LABEL_TWO;

	public class IndexConsistencyIT
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.EmbeddedDatabaseRule db = new org.neo4j.test.rule.EmbeddedDatabaseRule();
		 public readonly EmbeddedDatabaseRule Db = new EmbeddedDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.RandomRule random = new org.neo4j.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();

		 private readonly AssertableLogProvider _log = new AssertableLogProvider();
		 private static readonly Label[] _labels = new Label[]{ LABEL_ONE, LABEL_TWO, LABEL_THREE };
		 private const string PROPERTY_KEY = "numericProperty";
		 private const double DELETE_RATIO = 0.2;
		 private const double UPDATE_RATIO = 0.2;
		 private const int NODE_COUNT_BASELINE = 10;
		 private readonly FileFilter _sourceCopyFileFilter = file => file.Directory || file.Name.StartsWith( "index" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void reportNotCleanNativeIndex() throws java.io.IOException, org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReportNotCleanNativeIndex()
		 {
			  DatabaseLayout databaseLayout = Db.databaseLayout();
			  SomeData();
			  ResolveComponent( typeof( CheckPointer ) ).forceCheckPoint( new SimpleTriggerInfo( "forcedCheckpoint" ) );
			  File indexesCopy = databaseLayout.File( "indexesCopy" );
			  File indexSources = ResolveComponent( typeof( DefaultIndexProviderMap ) ).DefaultProvider.directoryStructure().rootDirectory();
			  copyRecursively( indexSources, indexesCopy, _sourceCopyFileFilter );

			  using ( Transaction tx = Db.beginTx() )
			  {
					CreateNewNode( new Label[]{ LABEL_ONE } );
					tx.Success();
			  }

			  Db.shutdownAndKeepStore();

			  copyRecursively( indexesCopy, indexSources );

			  ConsistencyCheckService.Result result = FullConsistencyCheck();
			  assertFalse( "Expected consistency check to fail", result.Successful );
			  assertThat( ReadReport( result ), hasItem( containsString( "WARN : Index was not properly shutdown and rebuild is required." ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void reportNotCleanNativeIndexWithCorrectData() throws java.io.IOException, org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReportNotCleanNativeIndexWithCorrectData()
		 {
			  DatabaseLayout databaseLayout = Db.databaseLayout();
			  SomeData();
			  ResolveComponent( typeof( CheckPointer ) ).forceCheckPoint( new SimpleTriggerInfo( "forcedCheckpoint" ) );
			  File indexesCopy = databaseLayout.File( "indexesCopy" );
			  File indexSources = ResolveComponent( typeof( DefaultIndexProviderMap ) ).DefaultProvider.directoryStructure().rootDirectory();
			  copyRecursively( indexSources, indexesCopy, _sourceCopyFileFilter );

			  Db.shutdownAndKeepStore();

			  copyRecursively( indexesCopy, indexSources );

			  ConsistencyCheckService.Result result = FullConsistencyCheck();
			  assertTrue( "Expected consistency check to fail", result.Successful );
			  assertThat( ReadReport( result ), hasItem( containsString( "WARN : Index was not properly shutdown and rebuild is required." ) ) );
		 }

		 private T ResolveComponent<T>( Type clazz )
		 {
				 clazz = typeof( T );
			  return Db.resolveDependency( clazz );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.List<String> readReport(org.neo4j.consistency.ConsistencyCheckService.Result result) throws java.io.IOException
		 private IList<string> ReadReport( ConsistencyCheckService.Result result )
		 {
			  return Files.readAllLines( result.ReportFile().toPath() );
		 }

		 internal virtual IList<Pair<long, Label[]>> SomeData()
		 {
			  return SomeData( 50 );
		 }

		 internal virtual IList<Pair<long, Label[]>> SomeData( int numberOfModifications )
		 {
			  IList<Pair<long, Label[]>> existingNodes;
			  existingNodes = new List<Pair<long, Label[]>>();
			  using ( Transaction tx = Db.beginTx() )
			  {
					RandomModifications( existingNodes, numberOfModifications );
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().indexFor(LABEL_ONE).on(PROPERTY_KEY).create();
					tx.Success();
			  }
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.schema().awaitIndexesOnline(1, TimeUnit.MINUTES);
					tx.Success();
			  }
			  return existingNodes;
		 }

		 private IList<Pair<long, Label[]>> RandomModifications( IList<Pair<long, Label[]>> existingNodes, int numberOfModifications )
		 {
			  for ( int i = 0; i < numberOfModifications; i++ )
			  {
					double selectModification = Random.NextDouble();
					if ( existingNodes.Count < NODE_COUNT_BASELINE || selectModification >= DELETE_RATIO + UPDATE_RATIO )
					{
						 CreateNewNode( existingNodes );
					}
					else if ( selectModification < DELETE_RATIO )
					{
						 DeleteExistingNode( existingNodes );
					}
					else
					{
						 ModifyLabelsOnExistingNode( existingNodes );
					}
			  }
			  return existingNodes;
		 }

		 private void CreateNewNode( IList<Pair<long, Label[]>> existingNodes )
		 {
			  Label[] labels = RandomLabels();
			  Node node = CreateNewNode( labels );
			  existingNodes.Add( Pair.of( node.Id, labels ) );
		 }

		 private Node CreateNewNode( Label[] labels )
		 {
			  Node node = Db.createNode( labels );
			  node.SetProperty( PROPERTY_KEY, Random.Next() );
			  return node;
		 }

		 private void ModifyLabelsOnExistingNode( IList<Pair<long, Label[]>> existingNodes )
		 {
			  int targetIndex = Random.Next( existingNodes.Count );
			  Pair<long, Label[]> existingPair = existingNodes[targetIndex];
			  long nodeId = existingPair.First();
			  Node node = Db.getNodeById( nodeId );
			  node.Labels.forEach( node.removeLabel );
			  Label[] newLabels = RandomLabels();
			  foreach ( Label label in newLabels )
			  {
					node.AddLabel( label );
			  }
			  existingNodes.RemoveAt( targetIndex );
			  existingNodes.Add( Pair.of( nodeId, newLabels ) );
		 }

		 private void DeleteExistingNode( IList<Pair<long, Label[]>> existingNodes )
		 {
			  int targetIndex = Random.Next( existingNodes.Count );
			  Pair<long, Label[]> existingPair = existingNodes[targetIndex];
			  Node node = Db.getNodeById( existingPair.First() );
			  node.Delete();
			  existingNodes.RemoveAt( targetIndex );
		 }

		 private Label[] RandomLabels()
		 {
			  IList<Label> labels = new List<Label>( _labels.Length );
			  foreach ( Label label in _labels )
			  {
					if ( Random.nextBoolean() )
					{
						 labels.Add( label );
					}
			  }
			  return labels.ToArray();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private org.neo4j.consistency.ConsistencyCheckService.Result fullConsistencyCheck() throws org.neo4j.consistency.checking.full.ConsistencyCheckIncompleteException, java.io.IOException
		 private ConsistencyCheckService.Result FullConsistencyCheck()
		 {
			  using ( FileSystemAbstraction fsa = new DefaultFileSystemAbstraction() )
			  {
					ConsistencyCheckService service = new ConsistencyCheckService();
					Config config = Config.defaults();
					return service.runFullConsistencyCheck( Db.databaseLayout(), config, NONE, _log, fsa, true );
			  }
		 }
	}

}