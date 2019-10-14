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
namespace Neo4Net.@unsafe.Batchinsert
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseSettings = Neo4Net.Graphdb.factory.GraphDatabaseSettings;
	using EphemeralFileSystemAbstraction = Neo4Net.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using Iterables = Neo4Net.Helpers.Collections.Iterables;
	using MapUtil = Neo4Net.Helpers.Collections.MapUtil;
	using Neo4Net.Kernel.extension;
	using GenericNativeIndexProviderFactory = Neo4Net.Kernel.Impl.Index.Schema.GenericNativeIndexProviderFactory;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Neo4Net.Test.rule.fs.EphemeralFileSystemRule;
	using FileSystemClosingBatchInserter = Neo4Net.@unsafe.Batchinsert.@internal.FileSystemClosingBatchInserter;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertFalse;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.default_schema_provider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.batchinsert.BatchInserters.inserter;

	public class BatchInsertersTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.TestDirectory testDirectory = org.neo4j.test.rule.TestDirectory.testDirectory();
		 public readonly TestDirectory TestDirectory = TestDirectory.testDirectory();
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Rule public final org.neo4j.test.rule.fs.EphemeralFileSystemRule fileSystemRule = new org.neo4j.test.rule.fs.EphemeralFileSystemRule();
		 public readonly EphemeralFileSystemRule FileSystemRule = new EphemeralFileSystemRule();

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void automaticallyCloseCreatedFileSystemOnShutdown() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void AutomaticallyCloseCreatedFileSystemOnShutdown()
		 {
			  VerifyInserterFileSystemClose( inserter( StoreDir ) );
			  VerifyInserterFileSystemClose( inserter( StoreDir, Config ) );
			  VerifyInserterFileSystemClose( inserter( StoreDir, Config, KernelExtensions ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void providedFileSystemNotClosedAfterShutdown() throws java.io.IOException
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 public virtual void ProvidedFileSystemNotClosedAfterShutdown()
		 {
			  EphemeralFileSystemAbstraction fs = FileSystemRule.get();
			  VerifyProvidedFileSystemOpenAfterShutdown( inserter( StoreDir, fs ), fs );
			  VerifyProvidedFileSystemOpenAfterShutdown( inserter( StoreDir, fs, Config ), fs );
			  VerifyProvidedFileSystemOpenAfterShutdown( inserter( StoreDir, fs, Config, KernelExtensions ), fs );
		 }

//JAVA TO C# CONVERTER WARNING: Java wildcard generics have no direct equivalent in .NET:
//ORIGINAL LINE: private static Iterable<org.neo4j.kernel.extension.KernelExtensionFactory<?>> getKernelExtensions()
		 private static IEnumerable<KernelExtensionFactory<object>> KernelExtensions
		 {
			 get
			 {
				  return Iterables.asIterable( new GenericNativeIndexProviderFactory() );
			 }
		 }

		 private static IDictionary<string, string> Config
		 {
			 get
			 {
				  return MapUtil.stringMap( default_schema_provider.name(), GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10.providerName() );
			 }
		 }

		 private static void VerifyProvidedFileSystemOpenAfterShutdown( BatchInserter inserter, EphemeralFileSystemAbstraction fileSystemAbstraction )
		 {
			  inserter.Shutdown();
			  assertFalse( fileSystemAbstraction.Closed );
		 }

		 private File StoreDir
		 {
			 get
			 {
				  return TestDirectory.storeDir();
			 }
		 }

		 private static void VerifyInserterFileSystemClose( BatchInserter inserter )
		 {
			  assertThat( "Expect specific implementation that will do required cleanups.", inserter, @is( instanceOf( typeof( FileSystemClosingBatchInserter ) ) ) );
			  inserter.Shutdown();
		 }
	}

}