using System;

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
namespace Neo4Net.Graphdb
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using PrimitiveLongCollections = Neo4Net.Collections.PrimitiveLongCollections;
	using GroupingRecoveryCleanupWorkCollector = Neo4Net.Index.Internal.gbptree.GroupingRecoveryCleanupWorkCollector;
	using DatabaseLayout = Neo4Net.Io.layout.DatabaseLayout;
	using IOLimiter = Neo4Net.Io.pagecache.IOLimiter;
	using KernelTransaction = Neo4Net.Kernel.api.KernelTransaction;
	using LabelScanStoreTest = Neo4Net.Kernel.api.impl.labelscan.LabelScanStoreTest;
	using LabelScanStore = Neo4Net.Kernel.api.labelscan.LabelScanStore;
	using LabelScanWriter = Neo4Net.Kernel.api.labelscan.LabelScanWriter;
	using NodeLabelUpdate = Neo4Net.Kernel.api.labelscan.NodeLabelUpdate;
	using ThreadToStatementContextBridge = Neo4Net.Kernel.impl.core.ThreadToStatementContextBridge;
	using LabelScanReader = Neo4Net.Storageengine.Api.schema.LabelScanReader;
	using DatabaseRule = Neo4Net.Test.rule.DatabaseRule;
	using EmbeddedDatabaseRule = Neo4Net.Test.rule.EmbeddedDatabaseRule;
	using RandomRule = Neo4Net.Test.rule.RandomRule;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertArrayEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertTrue;

	public class NativeLabelScanStoreStartupIT
	{
		 private static readonly Label _label = Label.label( "testLabel" );

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.DatabaseRule dbRule = new org.neo4j.test.rule.EmbeddedDatabaseRule();
		 public readonly DatabaseRule DbRule = new EmbeddedDatabaseRule();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.RandomRule random = new org.neo4j.test.rule.RandomRule();
		 public readonly RandomRule Random = new RandomRule();

		 private int _labelId;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void scanStoreStartWithoutExistentIndex() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ScanStoreStartWithoutExistentIndex()
		 {
			  LabelScanStore labelScanStore = LabelScanStore;
			  GroupingRecoveryCleanupWorkCollector workCollector = GroupingRecoveryCleanupWorkCollector;
			  labelScanStore.Shutdown();
			  workCollector.Shutdown();

			  DeleteLabelScanStoreFiles( DbRule.databaseLayout() );

			  workCollector.Init();
			  labelScanStore.Init();
			  workCollector.Start();
			  labelScanStore.Start();

			  CheckLabelScanStoreAccessible( labelScanStore );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void scanStoreRecreateCorruptedIndexOnStartup() throws Throwable
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ScanStoreRecreateCorruptedIndexOnStartup()
		 {
			  LabelScanStore labelScanStore = LabelScanStore;
			  GroupingRecoveryCleanupWorkCollector workCollector = GroupingRecoveryCleanupWorkCollector;

			  CreateTestNode();
			  long[] labels = ReadNodesForLabel( labelScanStore );
			  assertEquals( "Label scan store see 1 label for node", 1, labels.Length );
			  labelScanStore.Force( Neo4Net.Io.pagecache.IOLimiter_Fields.Unlimited );
			  labelScanStore.Shutdown();
			  workCollector.Shutdown();

			  CorruptLabelScanStoreFiles( DbRule.databaseLayout() );

			  workCollector.Init();
			  labelScanStore.Init();
			  workCollector.Start();
			  labelScanStore.Start();

			  long[] rebuildLabels = ReadNodesForLabel( labelScanStore );
			  assertArrayEquals( "Store should rebuild corrupted index", labels, rebuildLabels );
		 }

		 private LabelScanStore LabelScanStore
		 {
			 get
			 {
				  return GetDependency( typeof( LabelScanStore ) );
			 }
		 }

		 private GroupingRecoveryCleanupWorkCollector GroupingRecoveryCleanupWorkCollector
		 {
			 get
			 {
				  return DbRule.DependencyResolver.resolveDependency( typeof( GroupingRecoveryCleanupWorkCollector ) );
			 }
		 }

		 private T GetDependency<T>( Type clazz )
		 {
				 clazz = typeof( T );
			  return DbRule.DependencyResolver.resolveDependency( clazz );
		 }

		 private long[] ReadNodesForLabel( LabelScanStore labelScanStore )
		 {
			  using ( LabelScanReader reader = labelScanStore.NewReader() )
			  {
					return PrimitiveLongCollections.asArray( reader.NodesWithLabel( _labelId ) );
			  }
		 }

		 private Node CreateTestNode()
		 {
			  Node node;
			  using ( Transaction transaction = DbRule.beginTx() )
			  {
					node = DbRule.createNode( _label );
					 KernelTransaction ktx = DbRule.DependencyResolver.resolveDependency( typeof( ThreadToStatementContextBridge ) ).getKernelTransactionBoundToThisThread( true );
						 _labelId = ktx.TokenRead().nodeLabel(_label.name());
					transaction.Success();
			  }
			  return node;
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void scrambleFile(java.io.File file) throws java.io.IOException
		 private void ScrambleFile( File file )
		 {
			  LabelScanStoreTest.scrambleFile( Random.random(), file );
		 }

		 private static File StoreFile( DatabaseLayout databaseLayout )
		 {
			  return databaseLayout.LabelScanStore();
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private void corruptLabelScanStoreFiles(org.neo4j.io.layout.DatabaseLayout databaseLayout) throws java.io.IOException
		 private void CorruptLabelScanStoreFiles( DatabaseLayout databaseLayout )
		 {
			  ScrambleFile( StoreFile( databaseLayout ) );
		 }

		 private static void DeleteLabelScanStoreFiles( DatabaseLayout databaseLayout )
		 {
			  assertTrue( StoreFile( databaseLayout ).delete() );
		 }

//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
//ORIGINAL LINE: private static void checkLabelScanStoreAccessible(org.neo4j.kernel.api.labelscan.LabelScanStore labelScanStore) throws java.io.IOException
		 private static void CheckLabelScanStoreAccessible( LabelScanStore labelScanStore )
		 {
			  int labelId = 1;
			  using ( LabelScanWriter labelScanWriter = labelScanStore.NewWriter() )
			  {
					labelScanWriter.Write( NodeLabelUpdate.labelChanges( 1, new long[]{}, new long[]{labelId} ) );
			  }
			  using ( LabelScanReader labelScanReader = labelScanStore.NewReader() )
			  {
					assertEquals( 1, labelScanReader.NodesWithLabel( labelId ).next() );
			  }
		 }
	}

}