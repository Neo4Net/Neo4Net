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
	using ConsistencyFlags = Neo4Net.Consistency.checking.full.ConsistencyFlags;
	using Label = Neo4Net.GraphDb.Label;
	using Node = Neo4Net.GraphDb.Node;
	using Transaction = Neo4Net.GraphDb.Transaction;
	using Neo4Net.Collections.Helpers;
	using DefaultFileSystemAbstraction = Neo4Net.Io.fs.DefaultFileSystemAbstraction;
	using FileSystemAbstraction = Neo4Net.Io.fs.FileSystemAbstraction;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using Config = Neo4Net.Kernel.configuration.Config;
	using CheckPointer = Neo4Net.Kernel.impl.transaction.log.checkpoint.CheckPointer;
	using SimpleTriggerInfo = Neo4Net.Kernel.impl.transaction.log.checkpoint.SimpleTriggerInfo;
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
//	import static Neo4Net.helpers.progress.ProgressMonitorFactory.NONE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.io.fs.FileUtils.copyFile;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.TestLabels.LABEL_ONE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.TestLabels.LABEL_THREE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.test.TestLabels.LABEL_TWO;

	public class AllNodesInStoreExistInLabelIndexTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.EmbeddedDatabaseRule db = new Neo4Net.test.rule.EmbeddedDatabaseRule();
		 public readonly EmbeddedDatabaseRule Db = new EmbeddedDatabaseRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final Neo4Net.test.rule.RandomRule random = new Neo4Net.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();

		 private readonly AssertableLogProvider _log = new AssertableLogProvider();
		 private static readonly Label[] _labelAlphabet = new Label[]{ LABEL_ONE, LABEL_TWO, LABEL_THREE };
		 private static readonly Label _extraLabel = Label.label( "extra" );
		 private const double DELETE_RATIO = 0.2;
		 private const double UPDATE_RATIO = 0.2;
		 private const int NODE_COUNT_BASELINE = 10;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustReportSuccessfulForConsistentLabelScanStore() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustReportSuccessfulForConsistentLabelScanStore()
		 {
			  // given
			  SomeData();
			  Db.shutdownAndKeepStore();

			  // when
			  ConsistencyCheckService.Result result = FullConsistencyCheck();

			  // then
			  assertTrue( "Expected consistency check to succeed", result.Successful );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void reportNotCleanLabelIndex() throws java.io.IOException, Neo4Net.consistency.checking.full.ConsistencyCheckIncompleteException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReportNotCleanLabelIndex()
		 {
			  DatabaseLayout databaseLayout = Db.databaseLayout();
			  SomeData();
			  Db.resolveDependency( typeof( CheckPointer ) ).forceCheckPoint( new SimpleTriggerInfo( "forcedCheckpoint" ) );
			  File labelIndexFileCopy = databaseLayout.File( "label_index_copy" );
			  copyFile( databaseLayout.LabelScanStore(), labelIndexFileCopy );

			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode( LABEL_ONE );
					tx.Success();
			  }

			  Db.shutdownAndKeepStore();

			  copyFile( labelIndexFileCopy, databaseLayout.LabelScanStore() );

			  ConsistencyCheckService.Result result = FullConsistencyCheck();
			  assertFalse( "Expected consistency check to fail", result.Successful );
			  assertThat( ReadReport( result ), hasItem( containsString( "WARN : Label index was not properly shutdown and rebuild is required." ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void reportNotCleanLabelIndexWithCorrectData() throws java.io.IOException, Neo4Net.consistency.checking.full.ConsistencyCheckIncompleteException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ReportNotCleanLabelIndexWithCorrectData()
		 {
			  DatabaseLayout databaseLayout = Db.databaseLayout();
			  SomeData();
			  Db.resolveDependency( typeof( CheckPointer ) ).forceCheckPoint( new SimpleTriggerInfo( "forcedCheckpoint" ) );
			  File labelIndexFileCopy = databaseLayout.File( "label_index_copy" );
			  copyFile( databaseLayout.LabelScanStore(), labelIndexFileCopy );

			  Db.shutdownAndKeepStore();

			  copyFile( labelIndexFileCopy, databaseLayout.LabelScanStore() );

			  ConsistencyCheckService.Result result = FullConsistencyCheck();
			  assertTrue( "Expected consistency check to fail", result.Successful );
			  assertThat( ReadReport( result ), hasItem( containsString( "WARN : Label index was not properly shutdown and rebuild is required." ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustReportMissingNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustReportMissingNode()
		 {
			  // given
			  SomeData();
			  File labelIndexFileCopy = CopyLabelIndexFile();

			  // when
			  using ( Transaction tx = Db.beginTx() )
			  {
					Db.createNode( LABEL_ONE );
					tx.Success();
			  }

			  // and
			  ReplaceLabelIndexWithCopy( labelIndexFileCopy );
			  Db.shutdownAndKeepStore();

			  // then
			  ConsistencyCheckService.Result result = FullConsistencyCheck();
			  assertFalse( "Expected consistency check to fail", result.Successful );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustReportMissingLabel() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustReportMissingLabel()
		 {
			  // given
			  IList<Pair<long, Label[]>> nodesInStore = SomeData();
			  File labelIndexFileCopy = CopyLabelIndexFile();

			  // when
			  using ( Transaction tx = Db.beginTx() )
			  {
					AddLabelToExistingNode( nodesInStore );
					tx.Success();
			  }

			  // and
			  ReplaceLabelIndexWithCopy( labelIndexFileCopy );
			  Db.shutdownAndKeepStore();

			  // then
			  ConsistencyCheckService.Result result = FullConsistencyCheck();
			  assertFalse( "Expected consistency check to fail", result.Successful );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustReportExtraLabelsOnExistingNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustReportExtraLabelsOnExistingNode()
		 {
			  // given
			  IList<Pair<long, Label[]>> nodesInStore = SomeData();
			  File labelIndexFileCopy = CopyLabelIndexFile();

			  // when
			  using ( Transaction tx = Db.beginTx() )
			  {
					RemoveLabelFromExistingNode( nodesInStore );
					tx.Success();
			  }

			  // and
			  ReplaceLabelIndexWithCopy( labelIndexFileCopy );
			  Db.shutdownAndKeepStore();

			  // then
			  ConsistencyCheckService.Result result = FullConsistencyCheck();
			  assertFalse( "Expected consistency check to fail", result.Successful );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void mustReportExtraNode() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void MustReportExtraNode()
		 {
			  // given
			  IList<Pair<long, Label[]>> nodesInStore = SomeData();
			  File labelIndexFileCopy = CopyLabelIndexFile();

			  // when
			  using ( Transaction tx = Db.beginTx() )
			  {
					RemoveExistingNode( nodesInStore );
					tx.Success();
			  }

			  // and
			  ReplaceLabelIndexWithCopy( labelIndexFileCopy );
			  Db.shutdownAndKeepStore();

			  // then
			  ConsistencyCheckService.Result result = FullConsistencyCheck();
			  assertFalse( "Expected consistency check to fail", result.Successful );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.util.List<String> readReport(Neo4Net.consistency.ConsistencyCheckService.Result result) throws java.io.IOException
		 private IList<string> ReadReport( ConsistencyCheckService.Result result )
		 {
			  return Files.readAllLines( result.ReportFile().toPath() );
		 }

		 private void RemoveExistingNode( IList<Pair<long, Label[]>> nodesInStore )
		 {
			  Node node;
			  Label[] labels;
			  do
			  {
					int targetIndex = Random.Next( nodesInStore.Count );
					Pair<long, Label[]> existingNode = nodesInStore[targetIndex];
					node = Db.getNodeById( existingNode.First() );
					labels = existingNode.Other();
			  } while ( labels.Length == 0 );
			  node.Delete();
		 }

		 private void AddLabelToExistingNode( IList<Pair<long, Label[]>> nodesInStore )
		 {
			  int targetIndex = Random.Next( nodesInStore.Count );
			  Pair<long, Label[]> existingNode = nodesInStore[targetIndex];
			  Node node = Db.getNodeById( existingNode.First() );
			  node.AddLabel( _extraLabel );
		 }

		 private void RemoveLabelFromExistingNode( IList<Pair<long, Label[]>> nodesInStore )
		 {
			  Pair<long, Label[]> existingNode;
			  Node node;
			  do
			  {
					int targetIndex = Random.Next( nodesInStore.Count );
					existingNode = nodesInStore[targetIndex];
					node = Db.getNodeById( existingNode.First() );
			  } while ( existingNode.Other().Length == 0 );
			  node.RemoveLabel( existingNode.Other()[0] );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void replaceLabelIndexWithCopy(java.io.File labelIndexFileCopy) throws java.io.IOException
		 private void ReplaceLabelIndexWithCopy( File labelIndexFileCopy )
		 {
			  Db.restartDatabase((fs, directory) =>
			  {
				DatabaseLayout databaseLayout = Db.databaseLayout();
				fs.deleteFile( databaseLayout.labelScanStore() );
				fs.copyFile( labelIndexFileCopy, databaseLayout.labelScanStore() );
			  });
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private java.io.File copyLabelIndexFile() throws java.io.IOException
		 private File CopyLabelIndexFile()
		 {
			  DatabaseLayout databaseLayout = Db.databaseLayout();
			  File labelIndexFileCopy = databaseLayout.File( "label_index_copy" );
			  Db.restartDatabase( ( fs, directory ) => fs.copyFile( databaseLayout.LabelScanStore(), labelIndexFileCopy ) );
			  return labelIndexFileCopy;
		 }

		 private IList<Pair<long, Label[]>> SomeData()
		 {
			  return SomeData( 50 );
		 }

		 private IList<Pair<long, Label[]>> SomeData( int numberOfModifications )
		 {
			  IList<Pair<long, Label[]>> existingNodes;
			  existingNodes = new List<Pair<long, Label[]>>();
			  using ( Transaction tx = Db.beginTx() )
			  {
					RandomModifications( existingNodes, numberOfModifications );
					tx.Success();
			  }
			  return existingNodes;
		 }

		 private void RandomModifications( IList<Pair<long, Label[]>> existingNodes, int numberOfModifications )
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
		 }

		 private void CreateNewNode( IList<Pair<long, Label[]>> existingNodes )
		 {
			  Label[] labels = RandomLabels();
			  Node node = Db.createNode( labels );
			  existingNodes.Add( Pair.of( node.Id, labels ) );
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
			  IList<Label> labels = new List<Label>( 3 );
			  foreach ( Label label in _labelAlphabet )
			  {
					if ( Random.nextBoolean() )
					{
						 labels.Add( label );
					}
			  }
			  return labels.ToArray();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private Neo4Net.consistency.ConsistencyCheckService.Result fullConsistencyCheck() throws Neo4Net.consistency.checking.full.ConsistencyCheckIncompleteException, java.io.IOException
		 private ConsistencyCheckService.Result FullConsistencyCheck()
		 {
			  using ( FileSystemAbstraction fsa = new DefaultFileSystemAbstraction() )
			  {
					ConsistencyCheckService service = new ConsistencyCheckService();
					Config config = Config.defaults();
					return service.runFullConsistencyCheck( Db.databaseLayout(), config, NONE, _log, fsa, true, new ConsistencyFlags(config) );
			  }
		 }
	}

}