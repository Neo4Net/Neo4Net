/*
 * Copyright (c) 2002-2018 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net Enterprise Edition. The included source
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
 * Neo4Net object code can be licensed independently from the source
 * under separate terms from the AGPL. Inquiries can be directed to:
 * licensing@Neo4Net.com
 *
 * More information is also available at:
 * https://Neo4Net.com/licensing/
 */
namespace Neo4Net.causalclustering.catchup
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.catchup.RequestMessageType.CORE_SNAPSHOT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.catchup.RequestMessageType.INDEX_SNAPSHOT;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.catchup.RequestMessageType.PREPARE_STORE_COPY;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.catchup.RequestMessageType.STORE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.catchup.RequestMessageType.STORE_FILE;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.catchup.RequestMessageType.STORE_ID;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.catchup.RequestMessageType.TX_PULL_REQUEST;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static Neo4Net.causalclustering.catchup.RequestMessageType.UNKNOWN;

	public class RequestMessageTypeTest
	{
		 /*
		 Order should not change. New states should be added as higher values and old states should not be replaced.
		  */
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldHaveExpectedValues()
		 public virtual void ShouldHaveExpectedValues()
		 {
			  RequestMessageType[] givenStates = RequestMessageType.values();

			  RequestMessageType[] exepctedStates = new RequestMessageType[]{ TX_PULL_REQUEST, STORE, CORE_SNAPSHOT, STORE_ID, PREPARE_STORE_COPY, STORE_FILE, INDEX_SNAPSHOT, UNKNOWN };
			  sbyte[] expectedValues = new sbyte[]{ 1, 2, 3, 4, 5, 6, 7, unchecked( ( sbyte ) 404 ) };

			  assertEquals( exepctedStates.Length, givenStates.Length );
			  assertEquals( exepctedStates.Length, expectedValues.Length );
			  for ( int i = 0; i < givenStates.Length; i++ )
			  {
					RequestMessageType exepctedState = exepctedStates[i];
					RequestMessageType givenState = givenStates[i];
					assertEquals( format( "Expected %s git %s", givenState, exepctedState ), givenState.messageType(), exepctedState.messageType() );
					assertEquals( givenState.messageType(), expectedValues[i] );
			  }
		 }
	}

}