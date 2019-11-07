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
namespace Neo4Net.Index
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using Label = Neo4Net.GraphDb.Label;
	using IndexManager = Neo4Net.GraphDb.Index.IndexManager;
	using Neo4Net.Collections.Helpers;
	using LuceneBatchInserterIndexProvider = Neo4Net.Index.lucene.@unsafe.batchinsert.LuceneBatchInserterIndexProvider;
	using Inject = Neo4Net.Test.extension.Inject;
	using TestDirectoryExtension = Neo4Net.Test.extension.TestDirectoryExtension;
	using TestDirectory = Neo4Net.Test.rule.TestDirectory;
	using BatchInserter = Neo4Net.@unsafe.Batchinsert.BatchInserter;
	using BatchInserterIndex = Neo4Net.@unsafe.Batchinsert.BatchInserterIndex;
	using BatchInserters = Neo4Net.@unsafe.Batchinsert.BatchInserters;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.RandomStringUtils.randomAlphabetic;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTimeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.helpers.collection.MapUtil.stringMap;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class ExplicitIndexTest
	internal class ExplicitIndexTest
	{
		 private const long TEST_TIMEOUT = 80_000;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private Neo4Net.test.rule.TestDirectory directory;
		 private TestDirectory _directory;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void explicitIndexPopulationWithBunchOfFields()
		 internal virtual void ExplicitIndexPopulationWithBunchOfFields()
		 {
			  assertTimeout(ofMillis(TEST_TIMEOUT), () =>
			  {
				BatchInserter batchNode = BatchInserters.inserter( _directory.databaseDir() );
				LuceneBatchInserterIndexProvider provider = new LuceneBatchInserterIndexProvider( batchNode );
				try
				{
					 BatchInserterIndex batchIndex = provider.nodeIndex( "node_auto_index", stringMap( IndexManager.PROVIDER, "lucene", "type", "fulltext" ) );

					 IDictionary<string, object> properties = IntStream.range( 0, 2000 ).mapToObj( i => Pair.of( Convert.ToString( i ), randomAlphabetic( 200 ) ) ).collect( toMap( Pair.first, Pair.other ) );

					 long node = batchNode.createNode( properties, Label.label( "NODE" ) );
					 batchIndex.add( node, properties );
				}
				finally
				{
					 provider.shutdown();
					 batchNode.shutdown();
				}
			  });
		 }
	}

}