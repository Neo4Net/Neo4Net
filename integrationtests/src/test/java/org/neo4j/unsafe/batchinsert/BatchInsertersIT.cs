﻿using System.Collections.Generic;

/*
 * Copyright (c) 2002-2018 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j Enterprise Edition. The included source
 * code can be redistributed and/or modified under the terms of the
 * GNU AFFERO GENERAL PUBLIC LICENSE Version 3
 * (http://www.fsf.org/licensing/licenses/agpl-3.0.html) with the
 * Commons Clause, as found in the associated LICENSE.txt file.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 *
 * Neo4j object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@neo4j.com
 *
 * More information is also available at:
 * https://neo4j.com/licensing/
 */
namespace Org.Neo4j.@unsafe.Batchinsert
{

	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;

	using GraphDatabaseSettings = Org.Neo4j.Graphdb.factory.GraphDatabaseSettings;
	using Iterables = Org.Neo4j.Helpers.Collection.Iterables;
	using MapUtil = Org.Neo4j.Helpers.Collection.MapUtil;
	using Org.Neo4j.Kernel.extension;
	using GenericNativeIndexProviderFactory = Org.Neo4j.Kernel.Impl.Index.Schema.GenericNativeIndexProviderFactory;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.graphdb.factory.GraphDatabaseSettings.default_schema_provider;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.@unsafe.batchinsert.BatchInserters.inserter;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class BatchInsertersIT
	internal class BatchInsertersIT
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory testDirectory;
		 private TestDirectory _testDirectory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldStartBatchInserterWithRealIndexProvider() throws Exception
//JAVA TO C# CONVERTER WARNING: Method 'throws' clauses are not available in C#:
		 internal virtual void ShouldStartBatchInserterWithRealIndexProvider()
		 {
			  BatchInserter inserter = inserter( _testDirectory.databaseDir(), Config, KernelExtensions );
			  inserter.Shutdown();
		 }

		 private static IDictionary<string, string> Config
		 {
			 get
			 {
				  return MapUtil.stringMap( default_schema_provider.name(), GraphDatabaseSettings.SchemaIndex.NATIVE_BTREE10.providerName() );
			 }
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
	}

}