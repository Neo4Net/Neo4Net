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
namespace Neo4Net.Tooling.procedure.compilerutils
{
	using Test = org.junit.Test;

//JAVA TO C# CONVERTER TODO TASK: This Java 'import static' statement cannot be converted to C#:
//	import static org.assertj.core.api.Assertions.assertThat;

	public class CustomNameExtractorTest
	{

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void favours_name_over_value()
		 public virtual void FavoursNameOverValue()
		 {
			  assertThat( CustomNameExtractor.GetName( () => "name", () => "value" ) ).contains("name");
			  assertThat( CustomNameExtractor.GetName( () => "name", () => "" ) ).contains("name");
			  assertThat( CustomNameExtractor.GetName( () => "name", () => "  " ) ).contains("name");
			  assertThat( CustomNameExtractor.GetName( () => "   name  ", () => "  " ) ).contains("name");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void returns_value_if_trimmed_name_is_empty()
		 public virtual void ReturnsValueIfTrimmedNameIsEmpty()
		 {
			  assertThat( CustomNameExtractor.GetName( () => "", () => "value" ) ).contains("value");
			  assertThat( CustomNameExtractor.GetName( () => "   ", () => "value" ) ).contains("value");
			  assertThat( CustomNameExtractor.GetName( () => "   ", () => "   value  " ) ).contains("value");
		 }

//JAVA TO C# CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
//ORIGINAL LINE: @Test public void returns_nothing_if_none_defined()
		 public virtual void ReturnsNothingIfNoneDefined()
		 {
			  assertThat( CustomNameExtractor.GetName( () => "", () => "" ) ).Empty;
			  assertThat( CustomNameExtractor.GetName( () => "   ", () => "" ) ).Empty;
			  assertThat( CustomNameExtractor.GetName( () => "", () => "   " ) ).Empty;
			  assertThat( CustomNameExtractor.GetName( () => "   ", () => "   " ) ).Empty;
		 }
	}

}