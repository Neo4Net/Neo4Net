using System;
using System.Collections.Generic;

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
namespace Org.Neo4j.Index
{
	using Test = org.junit.jupiter.api.Test;
	using ExtendWith = org.junit.jupiter.api.extension.ExtendWith;


	using Label = Org.Neo4j.Graphdb.Label;
	using IndexManager = Org.Neo4j.Graphdb.index.IndexManager;
	using Org.Neo4j.Helpers.Collection;
	using LuceneBatchInserterIndexProvider = Org.Neo4j.Index.lucene.@unsafe.batchinsert.LuceneBatchInserterIndexProvider;
	using Inject = Org.Neo4j.Test.extension.Inject;
	using TestDirectoryExtension = Org.Neo4j.Test.extension.TestDirectoryExtension;
	using TestDirectory = Org.Neo4j.Test.rule.TestDirectory;
	using BatchInserter = Org.Neo4j.@unsafe.Batchinsert.BatchInserter;
	using BatchInserterIndex = Org.Neo4j.@unsafe.Batchinsert.BatchInserterIndex;
	using BatchInserters = Org.Neo4j.@unsafe.Batchinsert.BatchInserters;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.apache.commons.lang3.RandomStringUtils.randomAlphabetic;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertTimeout;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.helpers.collection.MapUtil.stringMap;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @ExtendWith(TestDirectoryExtension.class) class ExplicitIndexTest
	internal class ExplicitIndexTest
	{
		 private const long TEST_TIMEOUT = 80_000;

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Inject private org.neo4j.test.rule.TestDirectory directory;
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