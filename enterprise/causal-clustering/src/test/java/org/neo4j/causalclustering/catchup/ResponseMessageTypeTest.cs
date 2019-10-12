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
namespace Org.Neo4j.causalclustering.catchup
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.ResponseMessageType.CORE_SNAPSHOT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.ResponseMessageType.FILE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.ResponseMessageType.INDEX_SNAPSHOT_RESPONSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.ResponseMessageType.PREPARE_STORE_COPY_RESPONSE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.ResponseMessageType.STORE_COPY_FINISHED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.ResponseMessageType.STORE_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.ResponseMessageType.TX;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.ResponseMessageType.TX_STREAM_FINISHED;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.causalclustering.catchup.ResponseMessageType.UNKNOWN;

	public class ResponseMessageTypeTest
	{
		 /*
		 Order should not change. New states should be added as higher values and old states should not be replaced.
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveExpectedValues()
		 public virtual void ShouldHaveExpectedValues()
		 {
			  ResponseMessageType[] givenStates = ResponseMessageType.values();

			  ResponseMessageType[] exepctedStates = new ResponseMessageType[]{ TX, STORE_ID, FILE, STORE_COPY_FINISHED, CORE_SNAPSHOT, TX_STREAM_FINISHED, PREPARE_STORE_COPY_RESPONSE, INDEX_SNAPSHOT_RESPONSE, UNKNOWN };

			  sbyte[] expectedValues = new sbyte[]{ 1, 2, 3, 4, 5, 6, 7, 8, unchecked( ( sbyte ) 200 ) };

			  assertEquals( exepctedStates.Length, givenStates.Length );
			  assertEquals( givenStates.Length, expectedValues.Length );
			  for ( int i = 0; i < givenStates.Length; i++ )
			  {
					assertEquals( givenStates[i].messageType(), exepctedStates[i].messageType() );
					assertEquals( givenStates[i].messageType(), expectedValues[i] );
			  }
		 }
	}

}