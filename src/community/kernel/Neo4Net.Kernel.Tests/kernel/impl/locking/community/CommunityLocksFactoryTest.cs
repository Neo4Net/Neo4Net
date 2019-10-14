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
namespace Neo4Net.Kernel.impl.locking.community
{
	using Test = org.junit.jupiter.api.Test;

	using Config = Neo4Net.Kernel.configuration.Config;
	using Clocks = Neo4Net.Time.Clocks;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.MatcherAssert.assertThat;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.hamcrest.Matchers.instanceOf;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.jupiter.api.Assertions.assertNotSame;

	internal class CommunityLocksFactoryTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test void createDifferentCommunityLockManagers()
		 internal virtual void CreateDifferentCommunityLockManagers()
		 {
			  CommunityLocksFactory factory = new CommunityLocksFactory();
			  Locks locks1 = factory.NewInstance( Config.defaults(), Clocks.systemClock(), ResourceTypes.values() );
			  Locks locks2 = factory.NewInstance( Config.defaults(), Clocks.systemClock(), ResourceTypes.values() );
			  assertNotSame( locks1, locks2 );
			  assertThat( locks1, instanceOf( typeof( CommunityLockManger ) ) );
			  assertThat( locks2, instanceOf( typeof( CommunityLockManger ) ) );
		 }
	}

}