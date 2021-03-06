﻿using System.Collections.Generic;

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
namespace Org.Neo4j.@unsafe.Batchinsert
{
	using Rule = org.junit.Rule;
	using Test = org.junit.Test;


	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using EphemeralFileSystemAbstraction = Org.Neo4j.Graphdb.mockfs.EphemeralFileSystemAbstraction;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using MapUtil = Org.Neo4j.Helpers.Collection.MapUtil;
	using Org.Neo4j.Kernel.extension;
	using GenericNativeIndexProviderFactory = Org.Neo4j.Kernel.Impl.Index.Schema.GenericNativeIndexProviderFactory;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using EphemeralFileSystemRule = Org.Neo4j.Test.rule.fs.EphemeralFileSystemRule;
	using FileSystemClosingBatchInserter = Org.Neo4j.@unsafe.Batchinsert.@internal.FileSystemClosingBatchInserter;

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