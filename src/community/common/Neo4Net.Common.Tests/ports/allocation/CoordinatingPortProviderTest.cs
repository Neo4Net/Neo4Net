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
namespace Neo4Net.Ports.Allocation
{
	using Test = org.junit.jupiter.api.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.equalTo;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.@is;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.CoreMatchers.not;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.mock;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.mockito.Mockito.when;

	internal class CoordinatingPortProviderTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
[Fact] //ORIGINAL LINE: @Test void shouldProvideUniquePorts()
		 internal virtual void ShouldProvideUniquePorts()
		 {
			  PortRepository portRepository = mock( typeof( PortRepository ) );
			  PortProvider portProvider = new CoordinatingPortProvider( portRepository, port => false );

			  when( portRepository.ReserveNextPort( "foo" ) ).thenReturn( 40, 41 );
			  int port1 = portProvider.GetNextFreePort( "foo" );
			  int port2 = portProvider.GetNextFreePort( "foo" );

			  assertThat( port1, @is( not( equalTo( port2 ) ) ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
[Fact] //ORIGINAL LINE: @Test void shouldSkipReservedPorts()
		 internal virtual void ShouldSkipReservedPorts()
		 {
			  PortRepository portRepository = mock( typeof( PortRepository ) );
			  PortProvider portProvider = new CoordinatingPortProvider( portRepository, port => false );

			  when( portRepository.ReserveNextPort( "foo" ) ).thenReturn( 40, 41, 43 );
			  assertThat( portProvider.GetNextFreePort( "foo" ), @is( 40 ) );
			  assertThat( portProvider.GetNextFreePort( "foo" ), @is( 41 ) );
			  assertThat( portProvider.GetNextFreePort( "foo" ), @is( 43 ) );
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
[Fact] //ORIGINAL LINE: @Test void shouldSkipOccupiedPorts()
		 internal virtual void ShouldSkipOccupiedPorts()
		 {
			  PortRepository portRepository = mock( typeof( PortRepository ) );
			  PortProbe portProbe = mock( typeof( PortProbe ) );
			  PortProvider portProvider = new CoordinatingPortProvider( portRepository, portProbe );

			  when( portRepository.ReserveNextPort( "foo" ) ).thenReturn( 40, 41, 42, 43 );
			  when( portProbe.IsOccupied( 40 ) ).thenReturn( false );
			  when( portProbe.IsOccupied( 41 ) ).thenReturn( false );
			  when( portProbe.IsOccupied( 42 ) ).thenReturn( true );
			  when( portProbe.IsOccupied( 43 ) ).thenReturn( false );
			  assertThat( portProvider.GetNextFreePort( "foo" ), @is( 40 ) );
			  assertThat( portProvider.GetNextFreePort( "foo" ), @is( 41 ) );
			  assertThat( portProvider.GetNextFreePort( "foo" ), @is( 43 ) );
		 }
	}

}