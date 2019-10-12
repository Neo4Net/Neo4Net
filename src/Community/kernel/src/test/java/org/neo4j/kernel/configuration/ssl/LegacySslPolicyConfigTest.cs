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
namespace Neo4Net.Kernel.configuration.ssl
{
	using Test = org.junit.Test;

	using ConfigValue = Neo4Net.Configuration.ConfigValue;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.junit.Assert.assertEquals;
//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.neo4j.kernel.configuration.ssl.LegacySslPolicyConfig.certificates_directory;

	public class LegacySslPolicyConfigTest
	{
//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void shouldBeFoundInServerDefaults()
		 public virtual void ShouldBeFoundInServerDefaults()
		 {
			  // given
			  Config serverDefaultConfig = Config.builder().withServerDefaults().build();

			  // when
			  Stream<ConfigValue> cvStream = serverDefaultConfig.ConfigValues.Values.stream();

			  // then
			  assertEquals( 1, cvStream.filter( c => c.name().Equals(certificates_directory.name()) ).count() );
		 }
	}

}