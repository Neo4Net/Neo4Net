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
namespace Neo4Net.Kernel.api.net
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertEquals;

	internal class NetworkConnectionIdGeneratorTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void shouldGenerateIds()
		 internal virtual void ShouldGenerateIds()
		 {
			  NetworkConnectionIdGenerator idGenerator = new NetworkConnectionIdGenerator();

			  assertEquals( "bolt-0", idGenerator.NewConnectionId( "bolt" ) );
			  assertEquals( "bolt-1", idGenerator.NewConnectionId( "bolt" ) );
			  assertEquals( "bolt-2", idGenerator.NewConnectionId( "bolt" ) );

			  assertEquals( "http-3", idGenerator.NewConnectionId( "http" ) );
			  assertEquals( "http-4", idGenerator.NewConnectionId( "http" ) );

			  assertEquals( "https-5", idGenerator.NewConnectionId( "https" ) );
			  assertEquals( "https-6", idGenerator.NewConnectionId( "https" ) );
			  assertEquals( "https-7", idGenerator.NewConnectionId( "https" ) );
		 }
	}

}